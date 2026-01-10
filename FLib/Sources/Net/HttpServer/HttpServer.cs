// =================================================={By Qcbf|qcbf@qq.com|2024-08-31}==================================================

using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using static FLib.Net.HttpResponser;

// ReSharper disable AsyncVoidMethod

namespace FLib.Net
{
    public class HttpServer
    {
        public Action<HttpListenerContext, HttpListenerWebSocketContext> OnReceiveWebSocketEvent;
        public Func<HttpListenerContext, Task> OnReceiveHttpEvent;
        public HttpResponser Responser;
        public HttpListener Listener;

        public override string ToString() => string.Join(',', Listener?.Prefixes.Select(v => v) ?? Array.Empty<string>());


        public HttpServer(int port) : this($"http://*:{port}/")
        {
        }

        public HttpServer(string prefix)
        {
            Listener = new HttpListener();
            Listener.Prefixes.Add(prefix);
        }

        public virtual void Start()
        {
            var prefixes = Listener.Prefixes;
            try
            {
                Listener.Start();
            }
            catch (HttpListenerException e)
            {
                if (e.ErrorCode != 5)
                    throw;
                Listener = new HttpListener();
                foreach (var prefix in prefixes)
                {
                    Listener.Prefixes.Add(prefix);
                    Process.Start(new ProcessStartInfo("netsh", $"http add urlacl url={prefix} user=Everyone")
                    {
                        Verb = "runas",
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true,
                        UseShellExecute = true
                    })?.WaitForExit();
                }
                Start();
            }
            AcceptRequest();
        }

        /// <summary>
        ///
        /// </summary>
        public void Stop()
        {
            if (Listener != null)
            {
                Listener.Stop();
                Listener = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetResponse(string path, Func<HttpContextHelper, Task> handler) => (Responser ??= new HttpResponser()).Responses[path] = handler;

        /// <summary>
        /// 
        /// </summary>
        public void SetResponse(string path, Action<HttpContextHelper> handler) => (Responser ??= new HttpResponser()).Responses[path] = handler;

        protected async void AcceptRequest()
        {
            while (Listener != null && Listener.IsListening)
            {
                try
                {
                    var httpContext = await Listener.GetContextAsync().ConfigureAwait(false);
                    OnReceiveRequest(httpContext);
                }
                catch (Exception ex)
                {
                    Log.Error?.Write(ex);
                }
            }
            Log.Info?.Write($"Over {Listener?.IsListening}");
        }

        protected virtual void OnReceiveRequest(HttpListenerContext ctx)
        {
            if (!string.IsNullOrWhiteSpace(ctx.Request.Headers["Sec-WebSocket-Key"]) && ctx.Request.IsWebSocketRequest)
            {
                OnReceiveWebSocket(ctx);
            }
            else
            {
                OnReceiveHttp(ctx);
            }
        }

        protected virtual void OnReceiveHttp(HttpListenerContext context)
        {
            if (OnReceiveHttpEvent != null)
            {
                OnReceiveHttpEvent(context);
            }
            else if (Responser != null)
            {
                Responser.Process(new HttpContextHelper(context)).Forget();
            }
            else
            {
                context.Response.StatusCode = 404;
                context.Response.Close();
            }
        }

        protected virtual async void OnReceiveWebSocket(HttpListenerContext context)
        {
            if (OnReceiveWebSocketEvent != null)
            {
                var ws = await context.AcceptWebSocketAsync(null).ConfigureAwait(false);
                OnReceiveWebSocketEvent?.Invoke(context, ws);
            }
            else
            {
                context.Response.StatusCode = 404;
                context.Response.Close();
            }
        }
    }
}
