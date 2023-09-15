using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor.PackageManager;
using UnityEngine.Analytics;

namespace WOW.Networking
{
    /// <summary>
    /// Router for http server.
    /// </summary>
    public abstract class HttpRouter
    {
        /// <summary>
        /// Router path
        /// </summary>
        public string Path { get => path; }

        private string path = null;

        public HttpRouter(string path)
        {
            this.path = path;
        }

        /// <summary>
        /// Is the url matched this router?
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool IsRequestMatched(HttpListenerContext context)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(context.Request.RawUrl, path);
        }

        /// <summary>
        /// Process request
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public abstract Task WriteContextToResponseAsync(HttpListenerContext context);

        /// <summary>
        /// Write binary to the context for the response. (Helper)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="content"></param>
        /// <param name="contentType"></param>
        public static void WriteContext(HttpListenerContext context, byte[] content, string contentType, StatusCode statusCode = StatusCode.Success)
        {
            // 基本的にキャッシュは残さずに、誰にでも返してあげる
            context.Response.StatusCode = (int)statusCode;
            context.Response.KeepAlive = false;
            context.Response.AddHeader(Header.CacheControl, Header.NoCache);
            context.Response.AddHeader(Header.CORS, Header.Any);
            context.Response.ContentType = contentType;

            context.Response.ContentLength64 = content.Length;
            context.Response.ContentType = contentType;

            var stream = context.Response.OutputStream;
            stream.Write(content, 0, content.Length);
            context.Response.Close();
        }

        /// <summary>
        /// Write string to the context for the response. (Helper)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="content"></param>
        /// <param name="contentType"></param>
        public static void WriteContext(HttpListenerContext context, string content, string contentType, StatusCode statusCode = StatusCode.Success)
        {
            var responseBytes = Encoding.UTF8.GetBytes(content);
            WriteContext(context, responseBytes, contentType, statusCode);
        }

        /// <summary>
        /// Write exception message to the context for the response. (Helper)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="exception"></param>
        /// <param name="contentType"></param>
        /// <param name="statusCode"></param>
        public static void WriteException(HttpListenerContext context, Exception exception, string contentType = ContentType.Plain, StatusCode statusCode = StatusCode.InternalServerError)
        {
            WriteContext(context, exception.Message, contentType, statusCode);
        }

        /// <summary>
        /// Parse request stream to byte array.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static byte[] ParseStreamToBytes(HttpListenerContext context, byte[] defaultValue = default(byte[]))
        {
            var streamBytes = defaultValue;

            using(var inputStream = context.Request.InputStream)
            using(var memoryStream = new System.IO.MemoryStream())
            {
                inputStream.CopyTo(memoryStream);
                streamBytes = memoryStream.ToArray();
            }

            return streamBytes;
        }

        /// <summary>
        /// Parse request stream to string.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string ParseStreamToString(HttpListenerContext context, string defaultValue = default(string))
        {
            string str = defaultValue;

            using(var inputStream = context.Request.InputStream)
            using(var memoryStream = new System.IO.MemoryStream())
            {
                inputStream.CopyTo(memoryStream);

                var encoding = context.Request.ContentEncoding;
                str = encoding.GetString(memoryStream.ToArray());
            }

            return str;
        }
    }
}