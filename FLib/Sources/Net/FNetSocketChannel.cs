// ==================== qcbf@qq.com | 2025-09-11 ====================

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using FLib;

namespace FLib.Net
{
    public abstract class FNetSocketChannel : FNetChannel
    {
        public Socket Socket;
        private int _readedSize;
        public override bool Invalid => Socket == null || !Socket.Connected;

        protected internal override void SendProcess(ReadOnlyMemory<byte> bytes)
        {
            Log.AssertNotNull(Socket);
            if (Invalid)
            {
                Log.Warn?.Write("send failure", this);
                return;
            }
            Socket.Send(bytes.Span);
        }

        protected internal override void CloseProcess()
        {
            try
            {
                Socket?.Close();
            }
            finally
            {
                Socket = null;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        protected internal async void LoopReceiving()
        {
            var sock = Socket;
            _readedSize = -1;
            var receiveCache = ArraySegment<byte>.Empty;
            Memory<byte> receiveBuffer = new byte[512 * 1024];
            try
            {
                while (!Invalid)
                {
                    var receiveSize = await sock.ReceiveAsync(receiveBuffer, SocketFlags.None).ConfigureAwait(false);
                    if (receiveSize <= 0)
                    {
                        Close("remote close " + receiveSize);
                        break;
                    }
                    Receive(receiveBuffer.Slice(0, receiveSize), ref receiveCache);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
                Log.Debug?.Write("socket is disposed", this);
            }
            catch (SocketException ex)
            {
                Close($"receive socket exception {ex.Message}");
            }
            catch (Exception ex)
            {
                Close($"receive exception {ex}");
            }
            finally
            {
                if (receiveCache.Array != null)
                    BufferPool.Return(receiveCache.Array);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void Receive(BytesReader reader, ref ArraySegment<byte> receivedCache)
        {
            while (reader.Available > 0)
            {
                if (_readedSize < 0)
                {
                    var remaining = 3 - receivedCache.Count;
                    if (reader.Available < remaining)
                    {
                        if (receivedCache == null)
                        {
                            receivedCache = new ArraySegment<byte>(BufferPool.Rent(64), 0, 3);
                        }
                        reader.Span.CopyTo(receivedCache);
                        receivedCache = receivedCache[reader.Available..];
                        return;
                    }
                    if (receivedCache.Count > 0)
                    {
                        reader.Span[..remaining].CopyTo(receivedCache);
                        reader.Position += remaining;
                        var totalSize = BytesReader.ReadLength3(receivedCache);
                        if (receivedCache.Array!.Length < totalSize)
                        {
                            BufferPool.Return(receivedCache.Array);
                            receivedCache = new ArraySegment<byte>(BufferPool.Rent(totalSize), 0, totalSize);
                        }
                        else
                        {
                            receivedCache = new ArraySegment<byte>(receivedCache.Array, 0, totalSize);
                        }
                    }
                    else
                    {
                        var totalSize = reader.ReadLength3();
                        receivedCache = new ArraySegment<byte>(BufferPool.Rent(totalSize), 0, totalSize);
                    }
                    _readedSize = 0;
                }

                var readCount = Math.Min(reader.Available, receivedCache.Count - _readedSize);
                reader.Span[..readCount].CopyTo(receivedCache.AsSpan(_readedSize));
                reader.Position += readCount;
                if ((_readedSize += readCount) < receivedCache.Count) return;
                _readedSize = -1;
                var temp = receivedCache;
                receivedCache = ArraySegment<byte>.Empty;
                Receive(temp);
            }
        }
    }
}
