using System;
using System.Collections;
using System.Collections.Generic;
using FLib.Collections;

namespace FLib
{
    /// <summary>独占集合所有权的值类型；持有集合后不得复制，不得在 Dispose 后使用 Raw/枚举器。</summary>
    public struct PooledHashSet<T> : ISet<T>, IDisposable
    {
        private static readonly HashSet<T> Empty = new();
        private HashSet<T> _raw;
        private readonly HashSetPool<T> _pool;

        public HashSet<T> Raw => _raw ??= (_pool ?? HashSetPool<T>.Shared).Rent();
        public readonly bool IsNull => _raw == null;
        public readonly bool IsNullOrEmpty => Count == 0;
        public readonly int Count => _raw?.Count ?? 0;
        public readonly int Capacity => _raw?.EnsureCapacity(0) ?? 0;
        readonly bool ICollection<T>.IsReadOnly => false;

        public PooledHashSet(int capacity, HashSetPool<T> pool = null)
        {
            _pool = pool;
            _raw = (pool ?? HashSetPool<T>.Shared).Rent(capacity);
        }

        public bool Add(in T item) => Raw.Add(item);
        public bool Remove(T item) => _raw?.Remove(item) == true;
        public readonly bool Contains(T item) => _raw?.Contains(item) == true;
        public void Clear() => _raw?.Clear();
        public int EnsureCapacity(int capacity)
        {
            if (_raw != null) return _raw.EnsureCapacity(capacity);
            if (capacity == 0) return 0;
            _raw = (_pool ?? HashSetPool<T>.Shared).Rent(capacity);
            return Capacity;
        }

        public readonly void CopyTo(T[] array, int arrayIndex) => (_raw ?? Empty).CopyTo(array, arrayIndex);
        public void ExceptWith(IEnumerable<T> other) => Raw.ExceptWith(other);
        public void IntersectWith(IEnumerable<T> other) => Raw.IntersectWith(other);
        public void SymmetricExceptWith(IEnumerable<T> other) => Raw.SymmetricExceptWith(other);
        public void UnionWith(IEnumerable<T> other) => Raw.UnionWith(other);
        public readonly bool IsProperSubsetOf(IEnumerable<T> other) => (_raw ?? Empty).IsProperSubsetOf(other);
        public readonly bool IsProperSupersetOf(IEnumerable<T> other) => (_raw ?? Empty).IsProperSupersetOf(other);
        public readonly bool IsSubsetOf(IEnumerable<T> other) => (_raw ?? Empty).IsSubsetOf(other);
        public readonly bool IsSupersetOf(IEnumerable<T> other) => (_raw ?? Empty).IsSupersetOf(other);
        public readonly bool Overlaps(IEnumerable<T> other) => (_raw ?? Empty).Overlaps(other);
        public readonly bool SetEquals(IEnumerable<T> other) => (_raw ?? Empty).SetEquals(other);

        public void Dispose()
        {
            var raw = _raw;
            if (raw == null) return;
            _raw = null;
            (_pool ?? HashSetPool<T>.Shared).Return(raw);
        }

        public readonly HashSet<T>.Enumerator GetEnumerator() => (_raw ?? Empty).GetEnumerator();
        readonly IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        void ICollection<T>.Add(T item) => Add(item);
        bool ISet<T>.Add(T item) => Add(item);
    }
}
