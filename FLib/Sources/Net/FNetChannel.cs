// ==================== qcbf@qq.com | 2025-09-11 ====================

using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading.Tasks;
using FLib;
using K4os.Compression.LZ4;

namespace FLib.Net
{
    public abstract class FNetChannel
    {
        public static Func<int, string> LogCmdHandler = i => i.ToString();
        public static Func<int, string> LogErrorCodeHandler = i => i.ToString();
        public const int CompressSize = 64 * 1024;

        public static ArrayPool<byte> BufferPool = ArrayPool<byte>.Create(CompressSize * 1024,
#if UNITY_PROJ
            5
#else
            50
#endif
        );

        public Dictionary<int, IFNetCallbackable> ReceiveCallbacks = new();
        public Action<bool> ConnectCallback;
        public IHeartbeat Heartbeat;

        public bool SendInQueue;
        public EndPoint AddressPoint;
        public ConcurrentQueue<FNetProcessor> Processors = new();
        public int Timeout = 5000;

        public abstract bool Invalid { get; }
        public override string ToString() => AddressPoint.ToString();
        protected internal abstract void SendProcess(ReadOnlyMemory<byte> bytes);
        protected internal abstract void CloseProcess();

        public virtual void Update()
        {
            while (Processors.TryDequeue(out var processor))
            {
                try
                {
                    processor.Process(this);
                }
                catch (Exception e)
                {
                    Log.Error?.Write($"{e} {Json5.SerializeToLog(processor)}", this);
                }
                if (processor.Buffer.Array?.Length < CompressSize)
                {
                    BufferPool.Return(processor.Buffer.Array);
                    processor.Buffer = ArraySegment<byte>.Empty;
                }
                FNetProcessor.Pool.Push(processor);
            }
            Heartbeat?.Update();
        }

        // /// <summary>
        // /// 
        // /// </summary>
        // public static unsafe int ReadSize(in ArraySegment<byte> buffer)
        // {
        //     fixed (byte* ptr = buffer.Array)
        //         return *(int*)(ptr + buffer.Offset) & HeadBitMask;
        // }
        //
        // /// <summary>
        // /// 
        // /// </summary>
        // public static unsafe ArraySegment<byte> WriteSize(in ArraySegment<byte> buffer, int size)
        // {
        //     fixed (byte* ptr = buffer.Array)
        //     {
        //         var offsetPtr = ptr + buffer.Offset;
        //         *(ushort*)offsetPtr = (ushort)size;
        //         *(offsetPtr + 2) = (byte)(size >> 16);
        //     }
        //     return buffer.Slice(buffer.Offset, size + HeadByteSize);
        // }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void Receive(ArraySegment<byte> buffer)
        {
            int cmd;
            try
            {
                BytesReader reader = buffer.AsSpan();
                cmd = (int)reader.ReadVInt();
                if (cmd <= 0)
                {
                    buffer = buffer[reader.Position..];
                    goto exit;
                }
                if (reader.Available > 0)
                {
                    var crc = reader.Read<ushort>();
                    var isCompressed = (crc & 1) == 1;
                    if ((CRC16.Encode(reader, 4) ^ crc) >> 1 != 0)
                        throw new VerificationException($"[{LogCmdHandler(cmd)}][{reader.Length}]{string.Join(',', reader.Span.ToArray().Take(128))} {ToString()}");
                    if (isCompressed)
                    {
                        var rawDataSize = reader.ReadLength3();
                        var newBuffer = new byte[rawDataSize];
                        var size = LZ4Codec.Decode(reader.Span, newBuffer);
                        BufferPool.Return(buffer.Array!);
                        Log.Assert(size > 0)?.Write("decompress error");
                        buffer = newBuffer;
                    }
                    else
                        buffer = buffer[reader.Position..];
                }
                else
                {
                    BufferPool.Return(buffer.Array!);
                    buffer = Array.Empty<byte>();
                }
            }
            catch
            {
                BufferPool.Return(buffer.Array!);
                throw;
            }

            exit:
            FNetProcessor.Create(EFNetProcessType.Receive, buffer, cmd).Finish(this);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Close(string log)
        {
            Log.Debug?.Write("close channel: " + log, this);
            FNetProcessor.Create(EFNetProcessType.Close).Finish(this);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Send(int cmd)
        {
            Log.Debug?.Write("send", this, LogCmdHandler(cmd));
            var writer = new BytesWriter(BufferPool.Rent(8), 0) { Position = 3 };
            writer.PushVInt(cmd);
            BytesWriter.WriteLength3(writer.BytesBuffer, writer.Length - 3);
            if (SendInQueue)
                FNetProcessor.Create(EFNetProcessType.Send, writer.HeapArraySegment).Finish(this);
            else
                SendProcess(writer.HeapArraySegment);
        }

        /// <summary>
        /// 
        /// </summary>
        public void SendState(int cmd, int stateCode, string text = null)
        {
            Log.Debug?.Write($"send state: {LogErrorCodeHandler(stateCode)} {text} ", this, LogCmdHandler(cmd));
            var maxSize = 12 + (text == null ? 0 : StringFLibUtility.Encoding.GetMaxByteCount(text.Length));
            var writer = new BytesWriter(BufferPool.Rent(maxSize)) { Position = 3 };
            writer.PushVInt(-cmd);
            writer.PushVInt(stateCode);
            writer.Push(text);
            BytesWriter.WriteLength3(writer.BytesBuffer, writer.Length - 3);
            if (SendInQueue)
                FNetProcessor.Create(EFNetProcessType.Send, writer.HeapArraySegment).Finish(this);
            else
                SendProcess(writer.HeapArraySegment);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Send<T>(int cmd, in T data) where T : IBytesPackable
        {
            Log.Debug?.Write($"send {Json5.SerializeToLog(data)} ", this, LogCmdHandler(cmd));
            var writer = BeginSendData(cmd, out var buffer, out var crcPos, out var cmdPos);
            BytesPack.Pack(data, ref writer);
            EndSendData(writer, buffer, crcPos, cmdPos);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Send<T>(int cmd, in IList<T> datas) where T : IBytesPackable
        {
            Log.Debug?.Write($"send {Json5.SerializeToLog(datas)} ", this, LogCmdHandler(cmd));
            var writer = BeginSendData(cmd, out var buffer, out var crcPos, out var cmdPos);
            BytesPack.Pack(datas, ref writer);
            EndSendData(writer, buffer, crcPos, cmdPos);
        }

        /// <summary>
        /// 
        /// </summary>
        private static BytesWriter BeginSendData(int cmd, out byte[] buffer, out int crcPos, out int cmdPos)
        {
            buffer = BufferPool.Rent(CompressSize);
            var writer = new BytesWriter(buffer, 0) { Position = 3, HeapArray = buffer };
            writer.PushVInt(cmd);
            cmdPos = writer.Position;
            writer.Push(ushort.MinValue); //crc
            crcPos = writer.Position;
            return writer;
        }

        /// <summary>
        /// 
        /// </summary>
        private void EndSendData(BytesWriter writer, byte[] buffer, int crcPos, int cmdPos)
        {
            var compressed = writer.HeapArray != buffer || writer.Position - crcPos - 3 > CompressSize;
            if (compressed)
            {
                BufferPool.Return(buffer);
                var rawDataSize = writer.Length - crcPos;
                var newBuffer = new byte[LZ4Codec.MaximumOutputSize(rawDataSize) + crcPos + 3 + 3]; // head(3), cmd, crc, rawPackSize(3), pack bytes
                writer.Span[..crcPos].CopyTo(newBuffer);
                var compressedPackSize = LZ4Codec.Encode(writer.Span[crcPos..], newBuffer.AsSpan(crcPos + 3), LZ4Level.L12_MAX);
                Log.Assert(compressedPackSize > 0);
                buffer = newBuffer;
                writer = new BytesWriter(buffer, compressedPackSize + crcPos + 3) { Position = crcPos };
                writer.PushLength3(rawDataSize);
            }
            var crc = (ushort)(CRC16.Encode(writer.Span[crcPos..], 4) & ~1 | (compressed ? 1 : 0));
            writer.Position = cmdPos;
            writer.Push(crc);
            writer.Position = 0;
            BytesWriter.WriteLength3(writer.BytesBuffer, writer.Length - 3);
            if (SendInQueue)
                FNetProcessor.Create(EFNetProcessType.Send, writer.HeapArraySegment).Finish(this);
            else
                SendProcess(writer.HeapArraySegment);
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetResponse(int cmd, Action<FNetResponse> syncHandler) => ReceiveCallbacks.Add(cmd, new FNetResponse(syncHandler));

        /// <summary>
        /// 
        /// </summary>
        public void SetResponse(int cmd, Func<FNetResponse, Task> syncHandler) => ReceiveCallbacks.Add(cmd, new FNetResponse(syncHandler));

        /// <summary>
        /// 
        /// </summary>
        public void SetResponse<T>(int cmd, Action<FNetResponse<T>> syncHandler) where T : IBytesPackable, new() => ReceiveCallbacks.Add(cmd, new FNetResponse<T>(syncHandler));

        /// <summary>
        /// 
        /// </summary>
        public void SetResponse<T>(int cmd, Func<FNetResponse<T>, Task> syncHandler) where T : IBytesPackable, new() => ReceiveCallbacks.Add(cmd, new FNetResponse<T>(syncHandler));

        /// <summary>
        /// 
        /// </summary>
        public void SetResponseList<T>(int cmd, Action<FNetResponseList<T>> syncHandler) where T : IBytesPackable => ReceiveCallbacks.Add(cmd, new FNetResponseList<T>(syncHandler));

        /// <summary>
        /// 
        /// </summary>
        public void SetResponseList<T>(int cmd, Func<FNetResponseList<T>, Task> syncHandler) where T : IBytesPackable => ReceiveCallbacks.Add(cmd, new FNetResponseList<T>(syncHandler));
    }
}
