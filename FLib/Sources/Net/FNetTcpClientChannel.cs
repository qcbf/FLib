// ==================== qcbf@qq.com | 2025-09-11 ====================

using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using FLib;

namespace FLib.Net
{
    public class FNetTcpClientChannel : FNetSocketChannel
    {
        public bool NoDelay = true;

        public virtual async Task Connect()
        {
            if (!Invalid)
                throw new Exception("already connected " + this);

            Log.Debug?.Write("start connect", this);

            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { SendTimeout = Timeout, NoDelay = NoDelay };
#if UNITY_PROJ
            await Task.WhenAny(socket.ConnectAsync(AddressPoint), Task.Delay(Timeout));
#else
            var cts = new CancellationTokenSource(Timeout);
            await socket.ConnectAsync(AddressPoint, cts.Token);
#endif
            if (!socket.Connected)
            {
                Log.Debug?.Write("connect failure", this);
                socket.Dispose();
                return;
            }
            Log.Debug?.Write("connect success", this);
            Socket = socket;
            LoopReceiving();
        }
    }
}
