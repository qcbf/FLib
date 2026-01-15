//==================={By Qcbf|qcbf@qq.com|12/30/2019 4:43:39 PM}===================

#if UNITY_2021_1_OR_NEWER
#define UNITY_PROJ
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
#if UNITY_PROJ
using UnityEngine;
#endif

namespace FLib
{
#if UNITY_PROJ
    [Serializable]
#endif
    [DebuggerTypeProxy(typeof(SlimDictionaryDebugView<,>))]
    [DebuggerDisplay("Count = {Count}")]
    public class SlimDictionary<TKey, TValue> : IDictionary, IDictionary<TKey, TValue>
    {
        protected static readonly Entry[] InitialEntries = new Entry[1];
        protected static readonly int[] SizeOneIntArray = new int[1];

        /// <summary>
        /// 0-based index into _entries of head of free chain: -1 means empty
        /// </summary>
#if UNITY_PROJ
        [SerializeField]
#endif
        protected int mFreeList = -1;

        /// <summary>
        /// 1-based index into _entries; 0 means empty
        /// </summary>
#if UNITY_PROJ
        [SerializeField]
#endif
        protected int[] mBuckets;

#if UNITY_PROJ
        [SerializeField]
#endif
        protected Entry[] mEntries;

#if UNITY_PROJ
        [SerializeField]
#endif
        private int _count;

        protected readonly EqualityComparer<TKey> mComparer;

        public int Count
        {
            get => _count;
            private set => _count = value;
        }

        public ref TValue this[TKey key]
        {
            get
            {
                var entries = mEntries;
                var collisionCount = 0;
                var bucketIndex = key.GetHashCode() & (mBuckets.Length - 1);
                for (var i = mBuckets[bucketIndex] - 1; (uint)i < (uint)entries.Length; i = entries[i].Next)
                {
                    if (mComparer.Equals(key, entries[i].Key))
                        return ref entries[i].Value;
                    if (collisionCount == entries.Length)
                        throw new InvalidOperationException("looping forever");
                    collisionCount++;
                }

                throw new KeyNotFoundException(key.ToString());
            }
        }

        [Serializable, DebuggerDisplay("({Key}, {Value})->{Next}")]
        public struct Entry
        {
            public TKey Key;
            public TValue Value;

            /// <summary>
            /// used 2,1,0,-1
            /// free -4,-3,-2
            /// 0-based index of next entry in chain: -1 means end of chain
            /// also encodes whether this entry _itself_ is part of the free list by changing sign and subtracting 3,
            /// so -2 means end of free list, -3 means index 0 but on free list, -4 means index 1 but on free list, etc.
            /// </summary>
            public int Next;
        }

        /// <summary>
        ///
        /// </summary>
        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDictionaryEnumerator
        {
            public SlimDictionary<TKey, TValue> Dict;
            private int _moveIndex, _realIndex;
            private int _initialCount;
            public readonly TKey Key => Dict.mEntries[_realIndex].Key;

            public readonly ref TValue Value
            {
                get
                {
                    if (Log.Error != null && _realIndex < 0 || _realIndex >= Dict.mEntries.Length)
                        throw new Exception($"index error {_moveIndex},{_realIndex}/{Dict.mEntries.Length}");
                    return ref Dict.mEntries[_realIndex].Value;
                }
            }

            #region Interface

            public readonly KeyValuePair<TKey, TValue> Current => new(Key, Value);
            readonly object IEnumerator.Current => new DictionaryEntry(Key, Value);
            readonly DictionaryEntry IDictionaryEnumerator.Entry => new(Key, Value);
            readonly object IDictionaryEnumerator.Key => Key;
            readonly object IDictionaryEnumerator.Value => Value;
            public void Reset() => _moveIndex = _realIndex = -1;

            readonly void IDisposable.Dispose()
            {
            }

            #endregion

            public Enumerator(SlimDictionary<TKey, TValue> dict)
            {
                _realIndex = _moveIndex = -1;
                _initialCount = dict.Count;
                Dict = dict;
            }

            public readonly override string ToString() => $"{{{Key}:{Value}}}";

            public bool MoveNext()
            {
                while (++_realIndex < Dict.mEntries.Length && Dict.mEntries[_realIndex].Next < -1)
                {
                }

                return ++_moveIndex < Math.Min(Dict.Count, _initialCount) && _realIndex < Dict.mEntries.Length;
            }

            public void RemoveSelf()
            {
                ref var entry = ref Dict.mEntries[_realIndex];
                ref var entryIndex = ref Dict.mBuckets[entry.Key.GetHashCode() & (Dict.mBuckets.Length - 1)];
                if (Dict.mComparer.Equals(Dict.mEntries[entryIndex - 1].Key, entry.Key))
                {
                    if (entry.Next == -1)
                    {
                        entryIndex = 0;
                    }
                    else
                    {
                        entryIndex = entry.Next + 1;
                    }
                }
                else
                {
                    var lastEntryIndex = entryIndex - 1;
                    while (Dict.mEntries[lastEntryIndex].Next != _realIndex) lastEntryIndex = Dict.mEntries[lastEntryIndex].Next;
                    Dict.mEntries[lastEntryIndex].Next = entry.Next;
                }

                entry = new Entry { Next = -3 - Dict.mFreeList };
                Dict.mFreeList = _realIndex;
                Dict._count--;
            }
        }

        public SlimDictionary() : this(EqualityComparer<TKey>.Default)
        {
        }

        public SlimDictionary(int capacity)
        {
            mComparer = EqualityComparer<TKey>.Default;
            capacity = MathEx.GetNextPowerOfTwo(Math.Max(2, capacity));
            SetBuckets(new int[capacity], new Entry[capacity]);
        }

        public SlimDictionary(EqualityComparer<TKey> comparer = null)
        {
            mComparer = comparer ?? EqualityComparer<TKey>.Default;
            SetBuckets(SizeOneIntArray, InitialEntries);
        }

        public SlimDictionary(SlimDictionary<TKey, TValue> dict, EqualityComparer<TKey> comparer = null)
        {
            mComparer = comparer ?? EqualityComparer<TKey>.Default;
            if (dict == null || dict._count == 0)
            {
                SetBuckets(SizeOneIntArray, InitialEntries);
            }
            else
            {
                _count = dict._count;
                mFreeList = dict.mFreeList;

                SetBuckets(new int[dict.mBuckets.Length], new Entry[dict.mEntries.Length]);
                dict.mBuckets.CopyTo(mBuckets, 0);
                dict.mEntries.CopyTo(mEntries, 0);
            }
        }

        public SlimDictionary(int capacity, EqualityComparer<TKey> comparer = null)
        {
            mComparer = comparer ?? EqualityComparer<TKey>.Default;
            if (capacity < 2)
                capacity = 2; // 1 would indicate the dummy array
            capacity = MathEx.GetNextPowerOfTwo(capacity);
            SetBuckets(new int[capacity], new Entry[capacity]);
        }

        protected virtual void SetBuckets(int[] buckets, Entry[] entries)
        {
            mBuckets = buckets;
            mEntries = entries;
        }

        public void Clear()
        {
            _count = 0;
            mFreeList = -1;
            SetBuckets(SizeOneIntArray, InitialEntries);
        }

        public void ResetToCapacity(int capacity)
        {
            Clear();
            capacity = MathEx.GetNextPowerOfTwo(capacity);
            if (capacity < 2)
                capacity = 2;
            SetBuckets(new int[capacity], new Entry[capacity]);
        }

        /// <summary>
        /// 
        /// </summary>
        public void TryAddCapacity(int addCapacity) => EnsureCapacity(Count + addCapacity);

        /// <summary>
        ///
        /// </summary>
        public void EnsureCapacity(int capacity)
        {
            if (mEntries.Length < capacity)
            {
                // TODO: 待优化
                var temp = mEntries.Where(v => v.Next >= -1).Take(_count).ToArray();
                ResetToCapacity(capacity);
                for (var i = 0; i < temp.Length; i++)
                {
                    GetOrAddValueRef(temp[i].Key) = temp[i].Value;
                }
            }
        }

        public bool ContainsKey(in TKey key)
        {
            var collisionCount = 0;
            var bucketIndex = key.GetHashCode() & (mBuckets.Length - 1);
            for (var i = mBuckets[bucketIndex] - 1; (uint)i < (uint)mEntries.Length; i = mEntries[i].Next)
            {
                if (mComparer.Equals(key, mEntries[i].Key))
                    return true;
                if (collisionCount == mEntries.Length)
                    throw new InvalidOperationException("looping forever");
                collisionCount++;
            }

            return false;
        }

        /// <summary>
        ///
        /// </summary>
        public int GetEntryIndex(TKey key)
        {
            var entries = mEntries;
            var collisionCount = 0;
            var bucketIndex = key.GetHashCode() & (mBuckets.Length - 1);
            for (var i = mBuckets[bucketIndex] - 1; (uint)i < (uint)entries.Length; i = entries[i].Next)
            {
                if (mComparer.Equals(key, entries[i].Key))
                    return i;
                if (collisionCount == entries.Length)
                    throw new InvalidOperationException("looping forever");
                collisionCount++;
            }

            return -1;
        }

        /// <summary>
        /// 效率最高的获取值
        /// </summary>
        public ref TValue GetEntryValue(int index)
        {
            return ref mEntries[index].Value;
        }

        /// <summary>
        /// 效率最高的设置值
        /// </summary>
        public void SetEntryValue(int index, in TValue v)
        {
            mEntries[index].Value = v;
        }

        /// <summary>
        /// Gets the value if present for the specified key.
        /// </summary>
        /// <param name="key">Key to look for</param>
        /// <param name="value">Value found, otherwise default(TValue)</param>
        /// <returns>true if the key is present, otherwise false</returns>
        public bool TryGetValue(in TKey key, out TValue value)
        {
            var entries = mEntries;
            var collisionCount = 0;
            for (var i = mBuckets[key.GetHashCode() & (mBuckets.Length - 1)] - 1; (uint)i < (uint)entries.Length; i = entries[i].Next)
            {
                if (mComparer.Equals(key, entries[i].Key))
                {
                    value = entries[i].Value;
                    return true;
                }

                if (collisionCount == entries.Length)
                    throw new InvalidOperationException("looping forever");
                collisionCount++;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Removes the entry if present with the specified key.
        /// </summary>
        /// <param name="key">Key to look for</param>
        /// <returns>true if the key is present, false if it is not</returns>
        public bool Remove(in TKey key)
        {
            var entries = mEntries;
            var bucketIndex = key.GetHashCode() & (mBuckets.Length - 1);
            var entryIndex = mBuckets[bucketIndex] - 1;

            var lastIndex = -1;
            var collisionCount = 0;
            while (entryIndex != -1)
            {
                var candidate = entries[entryIndex];
                if (mComparer.Equals(candidate.Key, key))
                {
                    if (lastIndex != -1)
                    {
                        // Fixup preceding element in chain to point to next (if any)
                        entries[lastIndex].Next = candidate.Next;
                    }
                    else
                    {
                        // Fixup bucket to new head (if any)
                        mBuckets[bucketIndex] = candidate.Next + 1;
                    }

                    entries[entryIndex] = default;

                    entries[entryIndex].Next = -3 - mFreeList; // New head of free list
                    mFreeList = entryIndex;

                    _count--;
                    return true;
                }

                lastIndex = entryIndex;
                entryIndex = candidate.Next;

                if (collisionCount == entries.Length)
                    throw new InvalidOperationException("looping forever");
                collisionCount++;
            }

            return false;
        }

        public void Add(TKey k, in TValue v)
        {
            GetOrAddValueRef(k) = v;
        }

        /// <summary>
        ///
        /// </summary>
        public TValue GetValueOrDefault(TKey key)
        {
            var index = GetEntryIndex(key);
            return index >= 0 ? GetEntryValue(index) : default;
        }

        // Not safe for concurrent _reads_ (at least, if either of them add)
        // For concurrent reads, prefer TryGetValue(key, out value)
        /// <summary>
        /// Gets the value for the specified key, or, if the key is not present,
        /// adds an entry and returns the value by ref. This makes it possible to
        /// add or update a value in a single look up operation.
        /// </summary>
        /// <param name="key">Key to look for</param>
        /// <returns>Reference to the new or existing value</returns>
        public ref TValue GetOrAddValueRef(TKey key)
        {
            var entries = mEntries;
            var collisionCount = 0;
            var bucketIndex = key.GetHashCode() & (mBuckets.Length - 1);
            for (var i = mBuckets[bucketIndex] - 1; (uint)i < (uint)entries.Length; i = entries[i].Next)
            {
                if (mComparer.Equals(key, entries[i].Key))
                    return ref entries[i].Value;
                if (collisionCount == entries.Length)
                    throw new InvalidOperationException("looping forever");
                collisionCount++;
            }

            return ref AddKey(key, bucketIndex);
        }

        /// <summary>
        /// 在没有remove的情况下获取首个,否则获取的可能是任意一个
        /// </summary>
        public ref TValue GetFirstValue()
        {
            for (var i = 0; i < mEntries.Length; i++)
            {
                if (mEntries[i].Next > -2)
                {
                    return ref mEntries[i].Value;
                }
            }

            throw new IndexOutOfRangeException();
        }

        /// <summary>
        /// 在没有remove的情况下获取首个,否则获取的可能是任意一个
        /// </summary>
        public ref TKey GetFirstKey()
        {
            for (var i = 0; i < mEntries.Length; i++)
            {
                if (mEntries[i].Next > -2)
                {
                    return ref mEntries[i].Key;
                }
            }

            throw new IndexOutOfRangeException();
        }

        private ref TValue AddKey(TKey key, int bucketIndex)
        {
            var entries = mEntries;
            int entryIndex;
            if (mFreeList != -1)
            {
                entryIndex = mFreeList;
                mFreeList = -3 - entries[mFreeList].Next;
            }
            else
            {
                if (_count == entries.Length || entries.Length == 1)
                {
                    entries = Resize(0);
                    bucketIndex = key.GetHashCode() & (mBuckets.Length - 1);
                    // entry indexes were not changed by Resize
                }

                entryIndex = _count;
            }

            entries[entryIndex].Key = key;
            entries[entryIndex].Next = mBuckets[bucketIndex] - 1;
            mBuckets[bucketIndex] = entryIndex + 1;
            _count++;
            return ref entries[entryIndex].Value;
        }

        protected virtual Entry[] Resize(int newSize)
        {
            Log.Assert(mEntries.Length == _count || mEntries.Length == 1); // We only copy _count, so if it's longer we will miss some
            var count = _count;
            if (newSize <= 0)
            {
                newSize = mEntries.Length * 2;
            }

            if ((uint)newSize > int.MaxValue) // uint cast handles overflow
                throw new OverflowException();

            var entries = new Entry[newSize];
            Array.Copy(mEntries, 0, entries, 0, count);

            var newBuckets = new int[entries.Length];
            while (count-- > 0)
            {
                var bucketIndex = entries[count].Key.GetHashCode() & (newBuckets.Length - 1);
                entries[count].Next = newBuckets[bucketIndex] - 1;
                newBuckets[bucketIndex] = count + 1;
            }

            SetBuckets(newBuckets, entries);
            return entries;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public bool ChangeKey(in TKey oldKey, in TKey newKey, bool isOverride = false)
        {
            var index = GetEntryIndex(oldKey);
            if (index >= 0)
            {
                ref var v = ref GetEntryValue(index);
                index = GetEntryIndex(newKey);
                if (index < 0)
                {
                    AddKey(newKey, newKey.GetHashCode() & (mBuckets.Length - 1)) = v;
                    Remove(oldKey);
                }
                else if (isOverride)
                {
                    SetEntryValue(index, v);
                }
                else
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        #region misc

        public ICollection<TKey> Keys => mEntries.Where(v => v.Next >= -1).Take(Count).Select(v => v.Key).ToArray();
        public ICollection<TValue> Values => mEntries.Where(v => v.Next >= -1).Take(Count).Select(v => v.Value).ToArray();
        ICollection IDictionary.Keys => (ICollection)Keys;
        ICollection IDictionary.Values => (ICollection)Values;
        bool IDictionary.IsFixedSize => false;
        bool IDictionary.IsReadOnly => false;
        bool ICollection.IsSynchronized => false;
        object ICollection.SyncRoot => null!;
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        TValue IDictionary<TKey, TValue>.this[TKey key]
        {
            get => this[key];
            set => GetOrAddValueRef(key) = value;
        }

        object IDictionary.this[object key]
        {
            get => this[(TKey)key];
            set => GetOrAddValueRef((TKey)key) = (TValue)value;
        }

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value) => GetOrAddValueRef(key) = value;
        bool IDictionary<TKey, TValue>.ContainsKey(TKey key) => ContainsKey(key);
        bool IDictionary<TKey, TValue>.Remove(TKey key) => Remove(key);
        bool IDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value) => TryGetValue(key, out value);
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => GetOrAddValueRef(item.Key) = item.Value;
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) => ContainsKey(item.Key);

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            foreach (var item in this)
            {
                array.SetValue(new KeyValuePair<TKey, TValue>(item.Key, item.Value), arrayIndex++);
                if (arrayIndex >= Count) break;
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);
        void IDictionary.Add(object key, object value) => GetOrAddValueRef((TKey)key) = (TValue)value;
        bool IDictionary.Contains(object key) => ContainsKey((TKey)key);
        IDictionaryEnumerator IDictionary.GetEnumerator() => new Enumerator(this);
        void IDictionary.Remove(object key) => Remove((TKey)key);

        void ICollection.CopyTo(Array array, int index)
        {
            foreach (var item in this)
            {
                array.SetValue(new KeyValuePair<TKey, TValue>(item.Key, item.Value), index++);
                if (index >= Count) break;
            }
        }

        #endregion
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal sealed class SlimDictionaryDebugView<K, V> where K : IEquatable<K>
    {
        private readonly SlimDictionary<K, V> mDictionary;

        public SlimDictionaryDebugView(SlimDictionary<K, V> dictionary)
        {
            mDictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KeyValuePair<K, V>[] Items => mDictionary.ToArray();
    }
}