//==================={By Qcbf|qcbf@qq.com|10/8/2022 11:05:05 AM}===================

using System;
using System.Buffers;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace FLib
{
    public ref struct BytesWriter
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly AllocatorHandle DefaultAllocator = (int size,
#if NET8_0_OR_GREATER
            scoped
#endif
            ref BytesWriter writer) => writer.HeapArray = new byte[size];

        /// <summary>
        /// 使用之后必须手动调用Dispose或者TryReleasePoolAllocator
        /// </summary>
        public static readonly AllocatorHandle PoolAllocator = (int size,
#if NET8_0_OR_GREATER
            scoped
#endif
            ref BytesWriter writer) =>
        {
            var pool = ArrayPool<byte>.Shared;
            if (writer.HeapArray != null)
                pool.Return(writer.HeapArray);
            var arr = pool.Rent(size);
            writer.HeapArray = arr;
            return arr;
        };

        public AllocatorHandle Allocator;
        public byte[] HeapArray;
        public Span<byte> BytesBuffer;
        public int Position;
        public int Length;

        public readonly byte this[int index] => BytesBuffer[index];
        public readonly Span<byte> Span => BytesBuffer[..Length];
        public readonly ArraySegment<byte> HeapArraySegment => new(HeapArray, 0, Length);

        public delegate Span<byte> AllocatorHandle(int newSize,
#if NET8_0_OR_GREATER
            scoped
#endif
            ref BytesWriter writer);

        public readonly override string ToString() =>
            $"{Position}/{Length}|{StringFLibUtility.LimitLength(string.Join(',', BytesBuffer[..Length].ToArray().Select(v => v.ToString("x2"))), 4096)}";


        public static BytesWriter CreateFromPool(int minimumSize = 0)
        {
            var writer = new BytesWriter { Allocator = PoolAllocator };
            if (minimumSize > 0)
                writer.Allocate(minimumSize);
            return writer;
        }

        public BytesWriter(byte[] heapBuffer, int? length = null)
        {
            BytesBuffer = HeapArray = heapBuffer;
            Length = length ?? heapBuffer.Length;
            Position = 0;
            Allocator = null;
        }

        public BytesWriter(
#if NET8_0_OR_GREATER
            scoped
#endif
            in Span<byte> buf, int? length = null)
        {
            Allocator = null;
            BytesBuffer = buf;
            Position = 0;
            Length = length ?? buf.Length;
            HeapArray = null;
        }

        /// <summary>
        ///
        /// </summary>
        public void Allocate(int size)
        {
            if (size <= BytesBuffer.Length) return;
            Span<byte> newBuffer;
            if (Allocator == null)
            {
                Allocator = DefaultAllocator;
                newBuffer = Allocator(Math.Max(16, MathEx.GetNextPowerOfTwo(size)), ref this);
            }
            else
            {
                newBuffer = Allocator(MathEx.GetNextPowerOfTwo(size), ref this);
            }

            if (newBuffer.Length < size) throw new Exception();
            BytesBuffer.CopyTo(newBuffer);
            BytesBuffer = newBuffer;
        }

        /// <summary>
        ///
        /// </summary>
        public void TryAddLength(int v)
        {
            if (Length - Position < v)
            {
                Allocate(Length = Position + v);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void AddPosition(int v)
        {
            TryAddLength(v);
            Position += v;
        }

        /// <summary>
        ///
        /// </summary>
        public void Clear()
        {
            Position = Length = 0;
        }

        /// <summary>
        ///
        /// </summary>
        public void PushLength(int length)
        {
            PushVInt(length);
        }

        /// <summary>
        /// 
        /// </summary>
        public void PushLength3(int length)
        {
            TryAddLength(3);
            WriteLength3(BytesBuffer[Position..], length);
            Position += 3;
        }

        /// <summary>
        ///
        /// </summary>
        public static unsafe void WriteLength3(Span<byte> buffer, int length)
        {
            fixed (byte* ptr = buffer)
            {
                *(ushort*)ptr = (ushort)length;
                *(ptr + 2) = (byte)(length >> 16);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public unsafe void Push<T>(in T data) where T : unmanaged
        {
            fixed (T* ptr = &data)
            {
                var size = sizeof(T);
                TryAddLength(size);
                fixed (byte* bPtr = BytesBuffer)
                    Unsafe.CopyBlock(bPtr + Position, ptr, (uint)size);
                Position += size;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public unsafe void MoveOffset(int atPosition, int offsetLength, bool isCleanMemory)
        {
            var copySize = Length - atPosition;
            TryAddLength(offsetLength);
            // Unsafe.CopyBlock(ref BytesBuffer[toPosition + bytesSize], ref BytesBuffer[toPosition], (uint)copySize);
            fixed (byte* bufPtr = BytesBuffer)
            {
                var srcPtr = bufPtr + atPosition;
                Buffer.MemoryCopy(srcPtr, srcPtr + offsetLength, copySize, copySize);
            }
            if (isCleanMemory)
                Span[atPosition..offsetLength].Clear();
            Position += offsetLength;
        }

        /// <summary>
        ///
        /// </summary>
        public void CopyFrom(int atPosition,
#if NET8_0_OR_GREATER
            scoped
#endif
            ReadOnlySpan<byte> bytes)
        {
            MoveOffset(atPosition, bytes.Length, false);
            bytes.CopyTo(BytesBuffer[atPosition..]);
        }

        /// <summary>
        ///
        /// </summary>
        public void CopyFrom(
#if NET8_0_OR_GREATER
            scoped
#endif
            ReadOnlySpan<byte> bytes)
        {
            TryAddLength(bytes.Length);
            bytes.CopyTo(BytesBuffer[Position..]);
            Position += bytes.Length;
        }

        /// <summary>
        ///
        /// </summary>
        public readonly void TryReleasePoolAllocator()
        {
            if (HeapArray != null)
                ArrayPool<byte>.Shared.Return((byte[])HeapArray);
        }


        #region vint
        public void PushVInt(int atPosition, long v, int placeholderSize = 4)
        {
            var bytes = new BytesWriter(stackalloc byte[placeholderSize], 0);
            bytes.PushVInt(v);
#if NET8_0_OR_GREATER
            CopyFrom(atPosition, bytes);
#else
            MoveOffset(atPosition, bytes.Length, false);
            bytes.Span.CopyTo(BytesBuffer[atPosition..]);
#endif
        }


        /// <summary>
        /// 类似protobuf的变长数字
        /// </summary>
        public unsafe void PushVInt(long v)
        {
            v = (v << 1) ^ (v >> 63);
            var buffer = stackalloc byte[8];
            buffer[0] = (byte)(v & 0x7f);
            v >>= 7;
            var length = 0;
            while (v != 0)
            {
                buffer[length] |= 0x80;
                buffer[++length] |= (byte)(v & 0x7f);
                v >>= 7;
            }

            ++length;
            TryAddLength(length);
            fixed (byte* bPtr = BytesBuffer)
                Unsafe.CopyBlock(bPtr + Position, buffer, (uint)length);
            Position += length;
        }
        #endregion

        #region string
        /// <summary>
        ///
        /// </summary>
        public unsafe void Push(string v, Encoding encoding = null)
        {
            var strCount = (v?.Length).GetValueOrDefault();
            if (strCount == 0)
            {
                PushLength(0);
            }
            else
            {
                encoding ??= StringFLibUtility.Encoding;
                fixed (char* cPtr = v)
                {
                    var byteCount = encoding.GetByteCount(cPtr!, strCount);
                    PushLength(byteCount);
                    TryAddLength(byteCount);
                    fixed (byte* bPtr = BytesBuffer)
                        encoding.GetBytes(cPtr, strCount, bPtr + Position, byteCount);
                    Position += byteCount;
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void Push(string[] v, Encoding encoding = null)
        {
            if (v == null)
            {
                PushLength(0);
            }
            else
            {
                PushLength(v.Length);
                foreach (var t in v)
                    Push(t, encoding);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void Push(string[][] v, Encoding encoding = null)
        {
            if (v == null)
            {
                PushLength(0);
            }
            else
            {
                PushLength(v.Length);
                foreach (var t in v)
                    Push(t, encoding);
            }
        }

        // /// <summary>
        // ///
        // /// </summary>
        // public void PushStringWithoutHead(string v, int byteCount = 0, Encoding encoding = null)
        // {
        //     var strCount = v.Length;
        //     if (byteCount <= 0)
        //         byteCount = (encoding ?? StringFLibUtility.Encoding).GetMaxByteCount(strCount);
        //     TryAddLength(byteCount);
        //     StringFLibUtility.Encoding.GetBytes(v, Span);
        //     Position += byteCount;
        // }
        #endregion

        #region array
        /// <summary>
        ///
        /// </summary>
        public void Push<T>(Span<T> data) where T : unmanaged => Push((ReadOnlySpan<T>)data);

        /// <summary>
        ///
        /// </summary>
        public void Push<T>(ReadOnlySpan<T> data) where T : unmanaged
        {
            var len = data.Length;
            PushLength(len);
            if (len > 0)
            {
                unsafe
                {
                    var size = sizeof(T) * len;
                    TryAddLength(size);
                    fixed (T* ptr = &data[0])
                    {
                        fixed (byte* bPtr = BytesBuffer)
                        {
                            Unsafe.CopyBlock(bPtr + Position, ptr, (uint)size);
                        }
                    }

                    Position += size;
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void Push<T>(T[] data) where T : unmanaged
        {
            Push((ReadOnlySpan<T>)data);
        }

        /// <summary>
        ///
        /// </summary>
        public void Push<T>(T[][] data) where T : unmanaged
        {
            if (data == null)
            {
                PushLength(0);
            }
            else
            {
                PushLength(data.Length);
                foreach (var t in data)
                {
                    Push(t);
                }
            }
        }
        #endregion

        #region script
        /// <summary>
        /// 
        /// </summary>
        public void PushScript<T>(in T script) where T : IBytesPackable
        {
            if (script == null)
            {
                Push(string.Empty);
            }
            else
            {
                Push(TypeAssistant.GetTypeName(script.GetType()));
                BytesPack.Pack(script, ref this);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void PushScript<T>(in T[] scripts) where T : IBytesPackable
        {
            if (scripts != null)
            {
                PushLength(scripts.Length);
                for (var i = 0; i < scripts.Length; i++)
                    PushScript(scripts[i]);
            }
            else
            {
                PushLength(0);
            }
        }
        #endregion


        public static implicit operator Span<byte>(in BytesWriter bytes) => bytes.Span;
        public static implicit operator ReadOnlySpan<byte>(in BytesWriter bytes) => bytes.Span;
        public static implicit operator BytesWriter(in Span<byte> bytes) => new(bytes);
    }
}
