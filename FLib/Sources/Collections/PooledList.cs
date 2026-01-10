//==================={By Qcbf|qcbf@qq.com|2/25/2022 12:30:31 PM}===================

using FLib;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace FLib
{
    public struct PooledList<T> : IList<T>, IDisposable
    {
        public T[] Buffer;

        public int Count { get; set; }

        public readonly Span<T> Span => new(Buffer, 0, Count);
        public readonly ArraySegment<T> Array => new(Buffer, 0, Count);
        public readonly Memory<T> Memory => new(Buffer, 0, Count);
        public readonly bool IsInitialized => Buffer != null;
        public readonly bool IsEmpty => Count == 0;

        readonly bool ICollection<T>.IsReadOnly => false;

        T IList<T>.this[int index]
        {
            readonly get => this[index];
            set => this[index] = value;
        }

        public readonly ref T this[int index]
        {
            get
            {
                if (index < 0 || index >= Count) throw new IndexOutOfRangeException(index.ToString() + "/" + Count);
                return ref Buffer[index];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public struct Enumerator : IEnumerator<T>
        {
            public bool DisposeBuffer;
            public T[] Buffer;
            public int Count;
            private int _index;
            public T Current => Buffer[_index - 1];
            object IEnumerator.Current => Current;
            public bool MoveNext() => _index++ < Count;
            public void Reset() => _index = 0;

            public void Dispose()
            {
                if (DisposeBuffer)
                    ArrayPool<T>.Shared.Return(Buffer);
                else
                    Reset();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public struct EnumeratorWithDispose : IEnumerable<T>
        {
            public T[] Buffer;
            public int Count;
            public IEnumerator<T> GetEnumerator() => new Enumerator() { Buffer = Buffer, Count = Count, DisposeBuffer = true };
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        /// <summary>
        /// 
        /// </summary>
        public PooledList(int capacity)
        {
            Buffer = null;
            Count = 0;
            Allocate(capacity);
        }

        /// <summary>
        /// 
        /// </summary>
        public T ElementOrDefault(int index, in T defaultValue = default)
        {
            return index < Count ? Buffer[index] : defaultValue;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Allocate(int capacity)
        {
            if (Buffer?.Length >= capacity) return;
            var newArr = ArrayPool<T>.Shared.Rent(capacity);
            if (Buffer != null)
            {
                System.Array.Copy(Buffer, newArr, Buffer.Length);
                ArrayPool<T>.Shared.Return(Buffer);
            }

            Buffer = newArr;
        }

        /// <summary>
        /// 
        /// </summary>
        public ref T Add(in T item)
        {
            if (Buffer == null || Count >= Buffer.Length)
                Allocate(Math.Max(8, MathEx.GetNextPowerOfTwo(Count + 1)));
            Buffer![Count] = item;
            return ref Buffer[Count++];
        }

        /// <summary>
        /// 
        /// </summary>
        public void AddRange(ICollection<T> values)
        {
            if (!(values?.Count > 0)) return;
            var newCount = Count + values.Count;
            if (Buffer == null || newCount >= Buffer.Length)
                Allocate(Math.Max(8, MathEx.GetNextPowerOfTwo(newCount)));
            values.CopyTo(Buffer!, Count);
            Count = newCount;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Insert(int index, in T item)
        {
            if (Buffer == null || Count >= Buffer.Length)
                Allocate(Math.Max(8, MathEx.GetNextPowerOfTwo(Count + 1)));
            Count++;
            for (var i = Count - 1; i > index; i--)
                Buffer![i] = Buffer[i - 1];
            Buffer![index] = item;
        }

        /// <summary>
        /// 
        /// </summary>
        public void RemoveAt(int index)
        {
            Log.Assert(index < Count)?.Write($"Index Error: {index}/{Count}");
            Count--;
            for (var i = index; i < Count; i++)
            {
                Buffer[i] = Buffer[i + 1];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Remove(in T item)
        {
            var index = IndexOf(item);
            if (index < 0) return false;
            RemoveAt(index);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear() => Clear(false);

        /// <summary>
        /// 
        /// </summary>
        public void Clear(bool clearArray)
        {
            if (clearArray)
                System.Array.Fill(Buffer, default, 0, Count);
            Count = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        public readonly int IndexOf(in T item)
        {
            for (var i = 0; i < Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(Buffer[i], item))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        public readonly bool Contains(in T item)
        {
            for (var i = 0; i < Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(Buffer[i], item))
                {
                    return true;
                }
            }

            return false;
        }

        public readonly void CopyTo(T[] array, int arrayIndex)
        {
            Buffer?.CopyTo(array, Count - arrayIndex);
        }

        public readonly T[] ToArray()
        {
            return new Span<T>(Buffer, 0, Count).ToArray();
        }

        public T[] ToArrayAndDispose()
        {
            var result = ToArray();
            Dispose();
            return result;
        }

        public void ReleasePool(bool clearArray = false)
        {
            if (Buffer == null) return;
            Count = 0;
            ArrayPool<T>.Shared.Return(Buffer, clearArray);
            Buffer = null;
        }

        public void Dispose()
        {
            ReleasePool();
        }


        readonly int IList<T>.IndexOf(T item) => IndexOf(item);
        void IList<T>.Insert(int index, T item) => Insert(index, item);
        void ICollection<T>.Add(T item) => Add(item);
        readonly bool ICollection<T>.Contains(T item) => Contains(item);
        bool ICollection<T>.Remove(T item) => Remove(item);
        public readonly Enumerator GetEnumerator() => new() { Count = Count, Buffer = Buffer };

        public EnumeratorWithDispose ForEachWithDispose()
        {
            var rs = new EnumeratorWithDispose { Count = Count, Buffer = Buffer };
            Count = 0;
            Buffer = null;
            return rs;
        }

        readonly IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public static implicit operator Span<T>(PooledList<T> list) => new(list.Buffer, 0, list.Count);
        public static implicit operator Memory<T>(PooledList<T> list) => new(list.Buffer, 0, list.Count);
        public static implicit operator ArraySegment<T>(PooledList<T> list) => new(list.Buffer, 0, list.Count);
    }
}
