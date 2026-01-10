// ==================== qcbf@qq.com | 2025-07-01 ====================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FLib
{
    public struct PooledHashSet<T> : ISet<T>, IDisposable
    {
        // 后续增加根据size分页储存到不同的池
        [ThreadStatic] public static Stack<HashSet<T>> Frees;
        private HashSet<T> _raw;
        public HashSet<T> Raw => _raw ?? ((Frees ??= new Stack<HashSet<T>>(16)).TryPop(out _raw) ? _raw : _raw = new HashSet<T>());
        public readonly bool IsNull => _raw == null;
        public readonly bool IsNullOrEmpty => !(_raw?.Count > 0);
        public readonly int Count => (_raw?.Count).GetValueOrDefault();
        readonly bool ICollection<T>.IsReadOnly => false;

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (IsNull) return;
            _raw.Clear();
            Frees.Push(_raw);
            _raw = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public HashSet<T>.Enumerator GetEnumerator() => _raw?.GetEnumerator() ?? default;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        [MethodImpl(MethodImplOptions.AggressiveInlining)] IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        [MethodImpl(MethodImplOptions.AggressiveInlining)] void ICollection<T>.Add(T item) => Raw.Add(item);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] bool ISet<T>.Add(T item) => Raw.Add(item);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool Add(in T item) => Raw.Add(item);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void ExceptWith(IEnumerable<T> other) => Raw.ExceptWith(other);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void IntersectWith(IEnumerable<T> other) => Raw.IntersectWith(other);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void SymmetricExceptWith(IEnumerable<T> other) => Raw.SymmetricExceptWith(other);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void UnionWith(IEnumerable<T> other) => Raw.UnionWith(other);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool IsProperSubsetOf(IEnumerable<T> other) => _raw?.IsProperSubsetOf(other) == true;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool IsProperSupersetOf(IEnumerable<T> other) => _raw?.IsProperSupersetOf(other) == true;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool IsSubsetOf(IEnumerable<T> other) => _raw?.IsSubsetOf(other) == true;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool IsSupersetOf(IEnumerable<T> other) => _raw?.IsSupersetOf(other) == true;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool Overlaps(IEnumerable<T> other) => _raw?.Overlaps(other) == true;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool SetEquals(IEnumerable<T> other) => _raw?.SetEquals(other) == true;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Clear() => _raw?.Clear();
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool Contains(T item) => _raw?.Contains(item) == true;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void CopyTo(T[] array, int arrayIndex) => _raw?.CopyTo(array, arrayIndex);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool Remove(T item) => _raw?.Remove(item) == true;
    }
}
