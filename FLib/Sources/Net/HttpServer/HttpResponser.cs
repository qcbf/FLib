// =================================================={By Qcbf|qcbf@qq.com|2024-09-02}==================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
#if NET5_0_OR_GREATER
using System.Text.Json;
#endif
using System.Threading.Tasks;

namespace FLib.Net
{
    public readonly struct HttpContextHelper
    {
        public readonly HttpListenerContext Ctx;
        public HttpListenerRequest Req => Ctx.Request;
        public HttpListenerResponse Res => Ctx.Response;
        public HttpContextHelper(HttpListenerContext ctx) => Ctx = ctx;
        public string Q(string key) => Req.QueryString[key];
        public T Q<T>(string key) => Json5.Deserialize<T>(Q(key));

        public T Q<T>(string key, in T defaultValue)
        {
            var str = Q(key);
            return string.IsNullOrEmpty(str) ? defaultValue : Json5.Deserialize<T>(Q(key));
        }

        public string Header(string key) => Req.Headers[key];

        public void Error(int code, string msg = "")
        {
            Res.StatusCode = code;
            Res.StatusDescription = msg;
        }

        public void Json(object obj, EJson5SerializeOption options = EJson5SerializeOption.Compatible)
        {
            Res.ContentType = "application/json";
            Write(StringFLibUtility.Encoding.GetBytes(Json5.Serialize(obj, options)));
        }

        public void Html(string obj)
        {
            Res.ContentType = "text/html; charset=utf-8";
            Write(StringFLibUtility.Encoding.GetBytes(obj));
        }

        public void Write(ReadOnlySpan<byte> content)
        {
            Res.ContentLength64 = content.Length;
            Res.OutputStream.Write(content);
        }
    }


    public class HttpResponser
    {
        public bool IsIgnoreMatchCase;
        public readonly Dictionary<string, Delegate> Responses = new(16);
        public Dictionary<string, string> DefaultHeaders = new();

        public async Task Process(HttpContextHelper ctx)
        {
            try
            {
                foreach (var item in DefaultHeaders)
                    ctx.Res.AddHeader(item.Key, item.Value);
                if (ctx.Req.HttpMethod == "OPTIONS")
                {
                    ctx.Res.Headers.Set("access-control-allow-headers", ctx.Header("access-control-request-headers"));
                    ctx.Res.StatusCode = 204;
                    ctx.Res.OutputStream.Close();
                    return;
                }

                Log.Info?.Write(StringFLibUtility.LimitLength(ctx.Req.Url!.ToString(), 256, 0), ctx.Req.RemoteEndPoint!.ToString(), "REQ");
                ctx.Res.ContentEncoding = StringFLibUtility.Encoding;
                var url = ctx.Req.Url?.AbsolutePath;
                if (url == null)
                    throw new ArgumentException(nameof(url));
                if (IsIgnoreMatchCase)
                    url = url.ToLowerInvariant();

                if (Responses.TryGetValue(url, out var handler))
                {
                    if (handler is Func<HttpContextHelper, Task> asyncFunc)
                    {
                        var asyncTask = asyncFunc(ctx);
                        const int millisecondsDelay =
#if DEBUG
                            3 * 60 * 1000;
#else
                            5000;
#endif
                        var task = await Task.WhenAny(asyncTask, Task.Delay(millisecondsDelay)).ConfigureAwait(false);
                        if (!asyncTask.IsCompleted)
                        {
                            asyncTask.Dispose();
                            throw new TimeoutException();
                        }
                        if (asyncTask.Exception != null)
                        {
                            throw asyncTask.Exception;
                        }
                    }
                    else if (handler is Action<HttpContextHelper> syncFunc)
                    {
                        syncFunc(ctx);
                    }
                }
                else if (url != "/")
                {
                    ctx.Error(404);
                }
            }
            catch (Exception ex)
            {
                ctx.Error(500, ex.Message);
                throw;
            }
            finally
            {
                ctx.Res.Close();
            }
        }
    }
}
