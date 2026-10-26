using System;
using System.Collections;
using System.Collections.Generic;
using FLib.Collections;

namespace FLib
{
    /// <summary>独占集合所有权的值类型；持有集合后不得复制，不得在 Dispose 后使用 Raw/枚举器。</summary>
    public struct PooledDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IDisposable
    {
        private static readonly Dictionary<TKey, TValue> Empty = new();
        private Dictionary<TKey, TValue> _raw;
        private readonly DictionaryPool<TKey, TValue> _pool;

        public Dictionary<TKey, TValue> Raw => _raw ??= (_pool ?? DictionaryPool<TKey, TValue>.Shared).Rent();
        public readonly bool IsNull => _raw == null;
        public readonly bool IsNullOrEmpty => Count == 0;
        public readonly int Count => _raw?.Count ?? 0;
        public readonly int Capacity => _raw?.EnsureCapacity(0) ?? 0;
        // Keys/Values 必须绑定实际字典，才能反映获取视图之后的增删。
        public Dictionary<TKey, TValue>.KeyCollection Keys => Raw.Keys;
        public Dictionary<TKey, TValue>.ValueCollection Values => Raw.Values;
        ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;
        ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;
        readonly bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        public TValue this[TKey key]
        {
            readonly get => (_raw ?? Empty)[key];
            set => Raw[key] = value;
        }

        public PooledDictionary(int capacity, DictionaryPool<TKey, TValue> pool = null)
        {
            _pool = pool;
            _raw = (pool ?? DictionaryPool<TKey, TValue>.Shared).Rent(capacity);
        }

        public void Add(TKey key, TValue value) => Raw.Add(key, value);
        public bool TryAdd(TKey key, TValue value) => Raw.TryAdd(key, value);
        public readonly bool ContainsKey(TKey key) => (_raw ?? Empty).ContainsKey(key);
        public readonly bool TryGetValue(TKey key, out TValue value) => (_raw ?? Empty).TryGetValue(key, out value);
        public readonly TValue GetValueOrDefault(TKey key, TValue defaultValue = default) =>
            TryGetValue(key, out var value) ? value : defaultValue;
        public bool Remove(TKey key) => (_raw ?? Empty).Remove(key);
        public void Clear() => _raw?.Clear();
        public int EnsureCapacity(int capacity)
        {
            if (_raw != null) return _raw.EnsureCapacity(capacity);
            if (capacity == 0) return 0;
            _raw = (_pool ?? DictionaryPool<TKey, TValue>.Shared).Rent(capacity);
            return Capacity;
        }

        public void Dispose()
        {
            var raw = _raw;
            if (raw == null) return;
            _raw = null;
            (_pool ?? DictionaryPool<TKey, TValue>.Shared).Return(raw);
        }

        public readonly Dictionary<TKey, TValue>.Enumerator GetEnumerator() => (_raw ?? Empty).GetEnumerator();
        readonly IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => GetEnumerator();
        readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => Raw.Add(item.Key, item.Value);
        readonly bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) =>
            ((ICollection<KeyValuePair<TKey, TValue>>)(_raw ?? Empty)).Contains(item);
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) =>
            ((ICollection<KeyValuePair<TKey, TValue>>)(_raw ?? Empty)).Remove(item);
        readonly void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) =>
            ((ICollection<KeyValuePair<TKey, TValue>>)(_raw ?? Empty)).CopyTo(array, arrayIndex);
    }
}
