// ==================== qcbf@qq.com | 2025-09-11 ====================

using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace FLib.Net
{
    public class FNetProcessor
    {
        public static readonly ConcurrentStack<FNetProcessor> Pool = new();

        public EFNetProcessType Type;
        public int Cmd;
        public ArraySegment<byte> Buffer;

        /// <summary>
        /// 
        /// </summary>
        public static FNetProcessor Create(EFNetProcessType type, in ArraySegment<byte> bytes = default, int cmd = 0)
        {
            if (Pool.TryPop(out var processor))
            {
                processor.Type = type;
                processor.Buffer = bytes;
                processor.Cmd = cmd;
                return processor;
            }
            return new FNetProcessor() { Type = type, Buffer = bytes, Cmd = cmd };
        }

        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Finish(FNetChannel channel)
        {
            channel.Processors.Enqueue(this);
        }

        /// <summary>
        /// 
        /// </summary>
        internal void Process(FNetChannel channel)
        {
            switch (Type)
            {
                case EFNetProcessType.Close:
                    channel.CloseProcess();
                    break;
                case EFNetProcessType.Receive:
                    var realCmd = Math.Abs(Cmd);
                    if (channel.ReceiveCallbacks.TryGetValue(realCmd, out var callback))
                        callback?.Invoke(channel, this);
                    else
                        Log.Warn?.Write("not found receive callback", channel, FNetChannel.LogCmdHandler(realCmd));
                    break;
                case EFNetProcessType.Send:
                    ((FNetSocketChannel)channel).SendProcess(Buffer);
                    break;
            }
        }
    }
}
