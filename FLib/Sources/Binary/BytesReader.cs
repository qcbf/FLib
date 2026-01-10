//==================={By Qcbf|qcbf@qq.com|10/8/2022 4:20:07 PM}===================

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using FLib;

namespace FLib
{
    public ref struct BytesReader
    {
        public ReadOnlySpan<byte> BytesBuffer;
        public int Position;
        public readonly int Length => BytesBuffer.Length;
        public readonly int Available => BytesBuffer.Length - Position;
        public readonly ReadOnlySpan<byte> Span => BytesBuffer[Position..];

        public readonly byte this[int index] => BytesBuffer[index];

        public BytesReader(ReadOnlySpan<byte> buffer)
        {
            BytesBuffer = buffer;
            Position = 0;
        }

        public readonly override string ToString() =>
            $"{Position}/{BytesBuffer.Length}|{StringFLibUtility.LimitLength(string.Join(',', BytesBuffer.ToArray().Select(v => v.ToString("x2"))), 4096)}";

        public void Read<T>(ref T? value) where T : unmanaged => value = Read<T>();
        public void Read<T>(ref T value) where T : unmanaged => value = Read<T>();

        /// <summary>
        /// 
        /// </summary>
        public unsafe T Read<T>() where T : unmanaged
        {
            var pos = Position;
            Position += sizeof(T);
            fixed (byte* ptr = BytesBuffer)
                return *(T*)(ptr + pos);
        }

        /// <summary>
        /// 
        /// </summary>
        public unsafe ref T ReadRef<T>() where T : unmanaged
        {
            var pos = Position;
            Position += sizeof(T);
            fixed (byte* ptr = BytesBuffer)
                return ref Unsafe.AsRef<T>(ptr + pos);
        }

        public int ReadLength()
        {
            return (int)ReadVInt();
        }

        public int ReadLength3()
        {
            var v = ReadLength3(Span);
            Position += 3;
            return v;
        }

        public static unsafe int ReadLength3(ReadOnlySpan<byte> buffer)
        {
            fixed (byte* ptr = buffer)
                return *(int*)ptr & 0xFFFFFF; // process boundary check?
        }

        /// <summary>
        /// 
        /// </summary>
        public long ReadVInt()
        {
            var v = 0L;
            for (byte i = 0; i < byte.MaxValue; i += 7)
            {
                v |= (long)(BytesBuffer[Position] & 0x7f) << i;
                if ((BytesBuffer[Position++] & 0x80) == 0) break;
            }
            return (v >> 1) ^ -(v & 1);
        }

        #region string
        /// <summary>
        /// 
        /// </summary>
        public void Read(ref string str, Encoding encoding = null)
        {
            str = ReadString(encoding);
        }

        /// <summary>
        /// 
        /// </summary>
        public unsafe string ReadString(Encoding encoding = null)
        {
            var count = ReadLength();
            if (count == 0) return string.Empty;
            fixed (byte* ptr = BytesBuffer)
            {
                var str = (encoding ?? StringFLibUtility.Encoding).GetString(ptr + Position, count);
                Position += count;
                return str;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string[] ReadStrings(string[] result = null, Encoding encoding = null)
        {
            Read(ref result, encoding);
            return result;
        }


        /// <summary>
        ///
        /// </summary>
        public string[][] ReadStrings2(string[][] result = null, Encoding encoding = null)
        {
            Read(ref result, encoding);
            return result;
        }

        public void Read([NotNull] ref string[] result, Encoding encoding = null)
        {
            var length = ReadLength();
            if (length == 0)
            {
                result = Array.Empty<string>();
            }
            else if (result?.Length != length)
            {
                result = new string[length];
                for (var i = 0; i < length; i++)
                {
                    result[i] = ReadString(encoding);
                }
            }
        }

        public void Read([NotNull] ref string[][] result, Encoding encoding = null)
        {
            var length = ReadLength();
            if (length == 0)
            {
                result = Array.Empty<string[]>();
            }
            else if (result?.Length != length)
            {
                result = new string[length][];
                for (var i = 0; i < length; i++)
                {
                    string[] temp = null;
                    Read(ref temp, encoding);
                    result[i] = temp;
                }
            }
        }
        #endregion

        #region array
        /// <summary>
        /// 
        /// </summary>
        public int Read<T>([NotNull] ref T[] array, bool isArrayPool = false) where T : unmanaged
        {
            var len = ReadLength();
            if (len > 0)
            {
                if (array?.Length != len)
                {
                    array = isArrayPool ? ArrayPool<T>.Shared.Rent(len) : new T[len];
                }
                unsafe
                {
                    var size = sizeof(T) * len;
                    fixed (T* ptr = &array[0])
                    {
                        fixed (byte* bPtr = BytesBuffer)
                        {
                            Unsafe.CopyBlock(ptr, bPtr + Position, (uint)size);
                        }
                    }
                    Position += size;
                }
            }
            else
            {
                array = Array.Empty<T>();
            }
            return len;
        }

        /// <summary>
        /// 
        /// </summary>
        public int Read<T>([NotNull] ref T[][] array, bool isArrayPool = false) where T : unmanaged
        {
            var len = ReadLength();
            if (len > 0)
            {
                if (array?.Length != len)
                {
                    array = isArrayPool ? ArrayPool<T[]>.Shared.Rent(len) : new T[len][];
                }
                for (var i = 0; i < len; i++)
                {
                    Read(ref array[i]);
                }
            }
            else
            {
                array = Array.Empty<T[]>();
            }
            return len;
        }

        /// <summary>
        /// 
        /// </summary>
        public T[] ReadArray<T>() where T : unmanaged
        {
            T[] arr = null;
            Read(ref arr);
            return arr;
        }

        /// <summary>
        /// 
        /// </summary>
        public T[][] ReadArray2<T>() where T : unmanaged
        {
            var size = ReadLength();
            var result = size > 0 ? new T[size][] : Array.Empty<T[]>();
            for (var i = 0; i < size; i++)
            {
                result[i] = ReadArray<T>();
            }
            return result;
        }
        #endregion

        #region script
        /// <summary>
        /// 
        /// </summary>
        public IBytesPackable ReadScript()
        {
            var typeName = ReadString();
            if (string.IsNullOrEmpty(typeName))
                return null;
            var script = (IBytesPackable)TypeAssistant.New(typeName);
            BytesPack.Unpack(ref script, ref this);
            return script;
        }

        /// <summary>
        /// 
        /// </summary>
        public T[] ReadScripts<T>() where T : IBytesPackable
        {
            var len = ReadLength();
            if (len == 0)
                return Array.Empty<T>();
            var scripts = new T[len];
            for (var i = 0; i < scripts.Length; i++)
                scripts[i] = (T)ReadScript();
            return scripts;
        }
        #endregion

        public static implicit operator BytesReader(byte[] b) => new() { BytesBuffer = b };
        public static implicit operator BytesReader(Span<byte> b) => new() { BytesBuffer = b };
        public static implicit operator BytesReader(ReadOnlySpan<byte> b) => new() { BytesBuffer = b };
        public static implicit operator BytesReader(Memory<byte> b) => new() { BytesBuffer = b.Span };
        public static implicit operator ReadOnlySpan<byte>(BytesReader b) => b.Span;
        public static implicit operator BytesReader(BytesWriter b) => new() { BytesBuffer = b.Span };
    }
}
