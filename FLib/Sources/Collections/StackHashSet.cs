//==================={By Qcbf|qcbf@qq.com|4/29/2022 3:14:28 PM}===================

using FLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace FLib
{
    /// <summary>
    /// 极其轻量栈上分配的HashSet
    /// </summary>
    public ref struct StackHashSet<T>
    {
        public ushort Length;

        /// <summary>
        /// 0: empty | other: index(-1)
        /// </summary>
        public ushort FreeIndex;

        /// <summary>
        /// 
        /// </summary>
        public Span<Entry> Entries;

        /// <summary>
        /// element 0: empty | element other: index(-1)
        /// </summary>
        public Span<ushort> Buckets;

        private readonly EqualityComparer<T> Comparer;

        public struct Entry
        {
            /// <summary>
            /// 0: empty or the last entry | other: next index(-1)
            /// </summary>
            public ushort NextIndex;

            public bool IsUsed;

            public T Value;

            public readonly override string ToString()
            {
                return $"{Value}|{NextIndex}";
            }
        }


        public ref struct Enumerator
        {
            internal Span<Entry> Entries;
            internal int Length;
            private int mIndex;
            public readonly ref T Current => ref Entries[mIndex - 1].Value;

            public bool MoveNext()
            {
                if (--Length < 0)
                    return false;
                while (!Entries[mIndex++].IsUsed)
                {
                    if (mIndex + 1 > Entries.Length)
                        return false;
                }

                return true;
            }
        }


        public StackHashSet(Span<ushort> buckets, in Span<Entry> buffer, EqualityComparer<T> comparer = null)
        {
            Comparer = comparer ?? EqualityComparer<T>.Default;
            Entries = buffer;
            Buckets = buckets;
            Length = 0;
            FreeIndex = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly int HashToIndex(int hash)
        {
            return hash % Entries.Length;
        }

        /// <summary>
        /// 
        /// </summary>
        public void AddRange(in Span<T> values)
        {
            for (var i = 0; i < values.Length; i++)
                Add(values[i]);
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Add(in T v)
        {
            var hash = HashToIndex(v.GetHashCode());
            ref var bucketToEntryIndex = ref Buckets[hash];
            var entryIndex = bucketToEntryIndex;
            while (entryIndex > 0)
            {
                ref var entry = ref Entries[entryIndex - 1];
                if (Comparer.Equals(entry.Value, v))
                    return false;
                entryIndex = entry.NextIndex;
            }

            if (FreeIndex > 0)
            {
                entryIndex = FreeIndex;
                FreeIndex = Entries[FreeIndex - 1].NextIndex;
            }
            else
            {
                entryIndex = (ushort)(Length + 1);
            }

            Entries[entryIndex - 1] = new Entry { Value = v, NextIndex = bucketToEntryIndex, IsUsed = true };
            bucketToEntryIndex = entryIndex;
            ++Length;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Contains(in T v)
        {
            if (Entries.IsEmpty) return false;
            var hash = HashToIndex(v.GetHashCode());
            ref var bucket = ref Buckets[hash];
            var entryIndex = bucket;
            while (entryIndex > 0)
            {
                ref var entry = ref Entries[entryIndex - 1];
                if (Comparer.Equals(entry.Value, v))
                    return true;
                entryIndex = entry.NextIndex;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Remove(in T v)
        {
            if (Entries.IsEmpty) return false;
            var hash = HashToIndex(v.GetHashCode());
            ref var bucket = ref Buckets[hash];
            var entryIndex = bucket;
            var lastEntryIndex = (ushort)0;
            while (entryIndex > 0)
            {
                ref var entry = ref Entries[entryIndex - 1];
                if (Comparer.Equals(entry.Value, v))
                {
                    if (lastEntryIndex == 0)
                    {
                        bucket = 0;
                    }
                    else
                    {
                        Entries[lastEntryIndex - 1].NextIndex = entry.NextIndex;
                    }

                    entry = new() { NextIndex = FreeIndex };
                    FreeIndex = entryIndex;
                    --Length;
                    return true;
                }

                lastEntryIndex = entryIndex;
                entryIndex = entry.NextIndex;
            }

            return false;
        }

        public Enumerator GetEnumerator() => new() { Entries = Entries, Length = Length };
    }
}
