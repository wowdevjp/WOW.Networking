using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using WOW.Threading;

namespace WOW.Networking
{
    public class HttpRouterExample : HttpRouter
    {
        public HttpRouterExample(string path) : base(path) { }

        public override async Task WriteContextToResponseAsync(HttpListenerContext context)
        {
            var method = context.Request.HttpMethod.ToString().ToUpper();

            if (context.Request.RawUrl == "/example/mainthread/")
            {
                try
                {
                    await MiniTask.SwitchToMainThread();
                    var gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    var localPosition = new Vector3(
                        Random.Range(-10, 10),
                        Random.Range(-10, 10),
                        Random.Range(-10, 10));
                    gameObject.transform.localPosition = localPosition;

                    var json = JsonUtility.ToJson(localPosition);

                    HttpRouter.WriteContext(context, json, ContentType.Json, StatusCode.Success);
                }
                catch (System.Exception e)
                {
                    HttpRouter.WriteException(context, e);
                }
            }
            else
            {
                try
                {
                    if (method == "GET")
                    {
                        HttpRouter.WriteContext(context, "Hello, world!", ContentType.Plain, StatusCode.Success);
                    }
                    else if (method == "POST")
                    {
                        var body = HttpRouter.ParseStreamToString(context);
                        // echo
                        HttpRouter.WriteContext(context, body, ContentType.Plain, StatusCode.Success);
                    }
                    else
                    {
                        throw new System.Exception("Invalid method.");
                    }
                }
                catch (System.Exception e)
                {
                    HttpRouter.WriteException(context, e);
                }
            }
        }
    }
}