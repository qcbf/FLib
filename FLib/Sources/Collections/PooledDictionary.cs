using System;
using System.Collections;
using System.Collections.Generic;
using FLib.Collections;

namespace FLib
{
    /// <summary>独占集合所有权的值类型；持有集合后不得复制，不得在 Dispose 后使用 Raw/枚举器。</summary>
    public struct PooledDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IDisposable
    {
        private Dictionary<TKey, TValue> _raw;
        private readonly DictionaryPool<TKey, TValue> _pool;

        public Dictionary<TKey, TValue> Raw => _raw ??= (_pool ?? DictionaryPool<TKey, TValue>.Shared).Rent();
        public readonly bool IsNull => _raw == null;
        public readonly bool IsNullOrEmpty => Count == 0;
        public readonly int Count => _raw?.Count ?? 0;
        public readonly int Capacity => _raw?.EnsureCapacity(0) ?? 0;
        public Dictionary<TKey, TValue>.KeyCollection Keys => Raw.Keys;
        public Dictionary<TKey, TValue>.ValueCollection Values => Raw.Values;
        ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;
        ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;
        readonly bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        public TValue this[TKey key]
        {
            get => Raw[key];
            set => Raw[key] = value;
        }

        public PooledDictionary(int capacity, DictionaryPool<TKey, TValue> pool = null)
        {
            _pool = pool;
            _raw = (pool ?? DictionaryPool<TKey, TValue>.Shared).Rent(capacity);
        }

        public void Add(TKey key, TValue value) => Raw.Add(key, value);
        public bool TryAdd(TKey key, TValue value) => Raw.TryAdd(key, value);
        public bool ContainsKey(TKey key) => Raw.ContainsKey(key);
        public bool TryGetValue(TKey key, out TValue value) => Raw.TryGetValue(key, out value);
        public bool Remove(TKey key) => Raw.Remove(key);
        public void Clear() => _raw?.Clear();
        public int EnsureCapacity(int capacity) => Raw.EnsureCapacity(capacity);

        public void Dispose()
        {
            var raw = _raw;
            if (raw == null) return;
            _raw = null;
            (_pool ?? DictionaryPool<TKey, TValue>.Shared).Return(raw);
        }

        public Dictionary<TKey, TValue>.Enumerator GetEnumerator() => Raw.GetEnumerator();
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => Raw.Add(item.Key, item.Value);
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) =>
            ((ICollection<KeyValuePair<TKey, TValue>>)Raw).Contains(item);
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) =>
            ((ICollection<KeyValuePair<TKey, TValue>>)Raw).Remove(item);
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) =>
            ((ICollection<KeyValuePair<TKey, TValue>>)Raw).CopyTo(array, arrayIndex);
    }
}
