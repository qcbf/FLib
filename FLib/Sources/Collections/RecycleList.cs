//=================================================={By Qcbf|qcbf@qq.com|3/29/2025 3:28:41 PM}==================================================

using FLib;
using System;
using System.Collections;
using System.Collections.Generic;

namespace FLib.Sources
{
    public struct RecycleList<T> : IList<T>
    {
        private T[] _values;
        private Stack<int> _frees;

        public readonly ref T this[int index] => ref _values[index];
        T IList<T>.this[int index] { get => this[index]; set => this[index] = value; }
        public readonly int Count => _values == null ? 0 : _values.Length - _frees.Count;
        readonly bool ICollection<T>.IsReadOnly => false;


        public void SetCapacity(int newSize)
        {
            if (newSize <= _values.Length)
                return;
            var oldSize = Count;
            Array.Resize(ref _values, newSize);
            _frees ??= new(newSize);
            for (int i = oldSize; i < newSize; i++)
                _frees.Push(i);
        }

        public ref T AddEmpty(out int index)
        {
            index = Add(default);
            return ref _values[index];
        }

        public int Add(in T val)
        {
            if (!_frees.TryPop(out var index))
            {
                SetCapacity(MathEx.GetNextPowerOfTwo(Count + 1));
                index = _frees.Pop();
            }
            _values[index] = val;
            return index;
        }

        void ICollection<T>.Add(T item) => Add(item);

        public readonly void Clear() => Clear(true);

        public readonly void Clear(bool isClearMemory)
        {
            if (Count == 0)
                return;
            if (isClearMemory)
                Array.Fill(_values, default);
            _frees.Clear();
            for (int i = _values.Length - 1; i >= 0; i--)
                _frees.Push(i);
        }

        public readonly bool Contains(in T item)
        {
            if (Count == 0)
                return false;
            var comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < _values.Length; i++)
            {
                if (comparer.Equals(_values[i], item))
                    return true;
            }
            return false;
        }

        readonly bool ICollection<T>.Contains(T item) => Contains(item);

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }

        int IList<T>.IndexOf(T item)
        {
            throw new NotSupportedException();
        }

        void IList<T>.Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException();
        }

        public readonly void RemoveAt(int index) => RemoveAt(index, true);

        public readonly void RemoveAt(int index, bool isClearMem = true)
        {
            if (isClearMem)
                _values[index] = default;
            _frees.Push(index);
        }
    }
}
