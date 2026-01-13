// ==================== qcbf@qq.com |2025-12-11 ====================

using System;
using System.Collections;
using System.Collections.Generic;

namespace FLib
{
    public struct FixedIndexList<T> : IList<T>
    {
        public T[] Values;
        public Stack<int> Frees;
#if DEBUG
        public HashSet<int> Uses;
#endif

        public int Count { get; private set; }
        bool ICollection<T>.IsReadOnly => false;

        public T this[int index]
        {
            get => Values[index];
            set => Values[index] = value;
        }

        public FixedIndexList(int capacity) : this()
        {
            capacity = Math.Max(4, capacity);
            Values = new T[capacity];
            Frees = new Stack<int>(capacity >> 1);
        }

        public IEnumerator<T> GetEnumerator()
        {
            // 先这样实现功能, 后续再优化.
            var frees = new PooledHashSet<int>();
            frees.Raw.EnsureCapacity(Frees.Count);
            frees.Raw.UnionWith(Frees);
            for (var i = 0; i < Values.Length; i++)
            {
                if (frees.Contains(i))
                    continue;
                yield return Values[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public readonly ref T GetRef(int index) => ref Values[index];
        void ICollection<T>.Add(T item) => Add(item);

        public int Add(in T item)
        {
            if (Frees?.TryPop(out var index) != true)
            {
                if (Values == null || Values.Length <= Count)
                    Array.Resize(ref Values, MathEx.GetNextPowerOfTwo(Count + 1));
                index = Count;
            }

            ++Count;
            Values[index] = item;
#if DEBUG
            (Uses ??= new HashSet<int>()).Add(index);
#endif
            return index;
        }

        void IList<T>.Insert(int index, T item) => throw new NotSupportedException();

        public void RemoveAt(int index) => RemoveAt(index, true);

        public void RemoveAt(int index, bool clean)
        {
#if DEBUG
            if (!(Uses ??= new HashSet<int>()).Remove(index))
                throw new Exception($"not found {index}");
#endif
            --Count;
            if (index < Count)
                (Frees ??= new Stack<int>()).Push(index);
            if (clean)
                Values[index] = default;
        }

        public void Clear() => Clear(true);

        public void Clear(bool clean)
        {
            Frees?.Clear();
            Count = 0;
            if (clean)
                Array.Fill(Values, default);
        }

        bool ICollection<T>.Contains(T item) => Contains(item);

        bool ICollection<T>.Remove(T item) => Remove(item);

        public bool Remove(in T item)
        {
            var index = IndexOf(item);
            if (index < 0) return false;
            RemoveAt(index);
            return true;
        }

        int IList<T>.IndexOf(T item) => IndexOf(item);
        public int IndexOf(in T item) => Array.IndexOf(Values, item);
        public bool Contains(in T item) => IndexOf(item) >= 0;
        public void CopyTo(T[] array, int arrayIndex) => throw new NotSupportedException();
    }
}