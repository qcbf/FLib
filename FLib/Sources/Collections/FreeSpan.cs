//=================================================={By Qcbf|qcbf@qq.com|3/28/2025 8:36:20 PM}==================================================

using FLib;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CS8500
namespace FLib.Sources
{
    public unsafe struct FreeSpan<T> where T : unmanaged
    {
        public T* Ptr;
        public int Length;
        public readonly bool IsEmpty => Length == 0;
        public readonly Span<T> Span => new(Ptr, Length);
        public readonly ref T this[int index] => ref Ptr[index];

        /// <summary>
        /// 
        /// </summary>
        public FreeSpan(T* ptr, int len)
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
        {
            Ptr = ptr;
            Length = len;
        }

        /// <summary>
        /// 
        /// </summary>
        public FreeSpan(Span<T> span)
        {
            ref var first = ref MemoryMarshal.GetReference(span);
#pragma warning disable CS8500
            Ptr = (T*)Unsafe.AsPointer(ref first);
            Length = span.Length;
        }

        /// <summary>
        /// 
        /// </summary>
        public readonly void Fill(in T val) => Span.Fill(val);

        /// <summary>
        /// 
        /// </summary>
        public readonly FreeSpan<T> Slice(int start, int length) => new FreeSpan<T>(Span.Slice(start, length));

        /// <summary>
        /// 
        /// </summary>
        public readonly Span<T>.Enumerator GetEnumerator() => Span.GetEnumerator();

        public static implicit operator FreeSpan<T>(Span<T> v) => new(v);
        public static implicit operator Span<T>(FreeSpan<T> v) => v.Span;
    }
}
