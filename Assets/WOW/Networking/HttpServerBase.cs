using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using WOW.Threading;

namespace WOW.Networking
{
    public abstract class HttpServerBase : MonoBehaviour
    {
        [SerializeField]
        protected string ipAddress = "127.0.0.1";
        [SerializeField]
        protected int port = 8080;

        protected CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        protected HttpListener httpListener = null;
        protected List<HttpRouter> routers = new List<HttpRouter>();

        protected virtual void Awake()
        {
            routers.Clear();
            AddRouters();

            StartServer();
        }

        protected virtual void OnDestroy()
        {
            StopServer();
            cancellationTokenSource?.Cancel();
        }

        protected abstract void AddRouters();

        public string GetServerHost()
        {
            return $"{ipAddress}:{port}";
        }

        protected virtual void StartServer()
        {
            StopServer();

            httpListener = new HttpListener();
            httpListener.Prefixes.Add($"http://{GetServerHost()}/");
            httpListener.Start();
            LoopWebServer(cancellationTokenSource.Token).Forget(Debug.LogException);
        }

        protected virtual void StopServer()
        {
            if (httpListener != null)
            {
                httpListener.Stop();
                httpListener = null;
            }
        }

        protected virtual async Task LoopWebServer(CancellationToken token)
        {
            if(httpListener == null)
            {
                return;
            }

            try
            {
#if LOG_VERBOSE_NETWORKING
                Debug.Log($"[HttpServerBase] Start http server: http://{GetServerHost()}/");
#endif

                await MiniTask.SwitchToThreadPool();

                while (httpListener.IsListening)
                {
                    token.ThrowIfCancellationRequested();

                    HttpListenerContext context = null;

                    // Get Request
                    try
                    {
                        context = await httpListener.GetContextAsync().ToCancellableTask(token);
                    }
                    catch (TaskCanceledException exceptionCancelled)
                    {
#if LOG_VERBOSE_NETWORKING
                        Debug.LogException(exceptionCancelled);
#endif
                        return;
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }

                    if (context == null)
                    {
                        continue;
                    }

                    // Router
                    try
                    {
                        Debug.Log(context.Request.UserHostAddress);
                        if (context.Request.UserHostAddress.Equals(GetServerHost()) == false)
                        {
                            throw new SecurityException($"[HttpServerBase] Access Denied: {context.Request.UserHostAddress}");
                        }

#if LOG_VERBOSE_NETWORKING
                        Debug.Log($"[HttpServerBase] {context.Request.HttpMethod}, {context.Request.RawUrl}, {context.Request.Url}");
#endif

                        bool isRouterMatched = false;

                        foreach (var router in routers)
                        {
                            if (router.IsRequestMatched(context))
                            {
                                router.WriteContextToResponseAsync(context).Forget(Debug.LogException);
                                isRouterMatched = true;
                                break;
                            }
                        }

                        if (isRouterMatched == false)
                        {
#if LOG_VERBOSE_NETWORKING
                            HttpRouter.WriteContext(context, "Not Found", ContentType.Plain, StatusCode.NotFound);
#endif
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        HttpRouter.WriteContext(context, e.Message, ContentType.Plain, StatusCode.InternalServerError);
                    }
                }

                // Restart
                StopServer();
                StartServer();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}