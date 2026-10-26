using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FLib.Collections
{
    /// <summary>按实际 Capacity 分桶、按估算字节数限制保留量的线程安全集合池。</summary>
    /// <remarks>
    /// Rent/Return 转移所有权；归还后不得继续访问，也不得重复归还。借出的集合本身不保证线程安全。
    /// 预算只约束池内空闲实例，不包含借出实例、元素引用的对象图以及池自身的管理开销。
    /// </remarks>
    public abstract class CollectionPool<T> where T : class
    {
        private sealed class Bucket
        {
            public readonly Stack<T> Items = new();
            public long RetainedBytes;
        }

        private readonly Bucket[] _buckets;
        private int _retainedCount;
        private long _retainedBytes;

        public int MinCapacity { get; }
        public int MaxCapacity { get; }
        public long MaxBytesPerBucket { get; }
        public int MaxCountPerBucket { get; }
        public int RetainedCount { get { lock (_buckets) return _retainedCount; } }
        public long RetainedBytes { get { lock (_buckets) return _retainedBytes; } }

        /// <param name="minCapacity">最小容量，必须是 2 的幂。</param>
        /// <param name="maxCapacity">可保留的实际容量上限，必须是 2 的幂且不小于最小容量。</param>
        /// <param name="maxBytesPerBucket">每桶的估算保留字节数上限；0 表示不保留。</param>
        /// <param name="maxCountPerBucket">每桶的实例数上限，避免小实例数量过多。</param>
        protected CollectionPool(int minCapacity = 16, int maxCapacity = 65536,
            long maxBytesPerBucket = 256 * 1024, int maxCountPerBucket = 128)
        {
            if (!IsPowerOfTwo(minCapacity)) throw new ArgumentOutOfRangeException(nameof(minCapacity));
            if (!IsPowerOfTwo(maxCapacity) || maxCapacity < minCapacity)
                throw new ArgumentOutOfRangeException(nameof(maxCapacity));
            if (maxBytesPerBucket < 0) throw new ArgumentOutOfRangeException(nameof(maxBytesPerBucket));
            if (maxCountPerBucket < 0) throw new ArgumentOutOfRangeException(nameof(maxCountPerBucket));

            MinCapacity = minCapacity;
            MaxCapacity = maxCapacity;
            MaxBytesPerBucket = maxBytesPerBucket;
            MaxCountPerBucket = maxCountPerBucket;
            var bucketCount = 1;
            for (var capacity = maxCapacity; capacity > minCapacity; capacity >>= 1) bucketCount++;
            _buckets = new Bucket[bucketCount];
        }

        /// <summary>取得空集合，Capacity 至少为 minimumCapacity；未命中时允许分配。</summary>
        public T Rent(int minimumCapacity = 0)
        {
            if (minimumCapacity < 0) throw new ArgumentOutOfRangeException(nameof(minimumCapacity));
            if (minimumCapacity > MaxCapacity) return Create(minimumCapacity);

            // 租借向上取整；归还向下分桶，保证桶内任何实例都满足租借容量。
            var capacity = MinCapacity;
            var index = 0;
            while (capacity < minimumCapacity)
            {
                capacity <<= 1;
                index++;
            }

            lock (_buckets)
            {
                var bucket = _buckets[index];
                if (bucket != null && bucket.Items.TryPop(out var item))
                {
                    var bytes = EstimateBytes(GetCapacity(item));
                    bucket.RetainedBytes -= bytes;
                    _retainedBytes -= bytes;
                    _retainedCount--;
                    return item;
                }
            }

            return Create(capacity);
        }

        /// <summary>
        /// 交还所有权。仅返回 true 时保留；超容量、预算不足或不兼容的实例直接丢弃。
        /// 被拒收的实例不保证已清理。
        /// </summary>
        public bool Return(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            var capacity = GetCapacity(item);
            if (capacity < MinCapacity || capacity > MaxCapacity || MaxCountPerBucket == 0 || !CanRetain(item))
                return false;
            var bytes = EstimateBytes(capacity);
            if (bytes > MaxBytesPerBucket) return false;

            var index = 0;
            for (var size = capacity / MinCapacity; size > 1; size >>= 1) index++;
            lock (_buckets)
            {
                var bucket = _buckets[index];
                if (bucket != null && (bucket.Items.Count >= MaxCountPerBucket ||
                                       bytes > MaxBytesPerBucket - bucket.RetainedBytes))
                    return false;
            }

            // Clear 可能随容量/元素数量增长，放在锁外；并发归还后再次检查预算。
            Reset(item);
            lock (_buckets)
            {
                var bucket = _buckets[index] ??= new Bucket();
                if (bucket.Items.Count >= MaxCountPerBucket || bytes > MaxBytesPerBucket - bucket.RetainedBytes)
                    return false;
                bucket.Items.Push(item);
                bucket.RetainedBytes += bytes;
                _retainedBytes += bytes;
                _retainedCount++;
                return true;
            }
        }

        /// <summary>释放当前空闲实例及桶的储存；不影响已借出的集合。</summary>
        public void Clear()
        {
            lock (_buckets)
            {
                Array.Clear(_buckets, 0, _buckets.Length);
                _retainedCount = 0;
                _retainedBytes = 0;
            }
        }

        protected abstract T Create(int capacity);
        protected abstract int GetCapacity(T item);
        protected abstract long EstimateBytes(int capacity);
        protected abstract void Reset(T item);
        protected virtual bool CanRetain(T item) => true;
        private static bool IsPowerOfTwo(int value) => value > 0 && (value & (value - 1)) == 0;
    }

    /// <summary>保守估算托管储存，不依赖运行时私有字段或反射；包含对象、数组头和对齐余量。</summary>
    internal static class CollectionPoolMemory
    {
        public static int AlignedSize<T>() => (Unsafe.SizeOf<T>() + IntPtr.Size - 1) & -IntPtr.Size;
    }

    // public sealed class ListPool<T> : CollectionPool<List<T>>
    // {
    //     public static ListPool<T> Shared { get; } = new();
    //
    //     public ListPool(int minCapacity = 16, int maxCapacity = 65536,
    //         long maxBytesPerBucket = 256 * 1024, int maxCountPerBucket = 128)
    //         : base(minCapacity, maxCapacity, maxBytesPerBucket, maxCountPerBucket) { }
    //
    //     protected override List<T> Create(int capacity) => new(capacity);
    //     protected override int GetCapacity(List<T> item) => item.Capacity;
    //     protected override long EstimateBytes(int capacity) => (long)capacity * Unsafe.SizeOf<T>() + 10 * IntPtr.Size;
    //     protected override void Reset(List<T> item) => item.Clear();
    // }

    /// <summary>每个池固定一个 Comparer；自定义 Comparer 使用独立池，不进入 Shared。</summary>
    public sealed class HashSetPool<T> : CollectionPool<HashSet<T>>
    {
        private readonly IEqualityComparer<T> _comparer;
        public static HashSetPool<T> Shared { get; } = new();

        public HashSetPool(IEqualityComparer<T> comparer = null, int minCapacity = 16, int maxCapacity = 65536,
            long maxBytesPerBucket = 256 * 1024, int maxCountPerBucket = 128)
            : base(minCapacity, maxCapacity, maxBytesPerBucket, maxCountPerBucket)
        {
            _comparer = comparer ?? EqualityComparer<T>.Default;
        }

        protected override HashSet<T> Create(int capacity) => new(capacity, _comparer);
        protected override int GetCapacity(HashSet<T> item) => item.EnsureCapacity(0);
        protected override long EstimateBytes(int capacity) =>
            (long)capacity * (12L + CollectionPoolMemory.AlignedSize<T>()) + 32 * IntPtr.Size;
        protected override void Reset(HashSet<T> item) => item.Clear();
        protected override bool CanRetain(HashSet<T> item) => ReferenceEquals(item.Comparer, _comparer);
    }

    /// <summary>每个池固定一个 Comparer；自定义 Comparer 使用独立池，不进入 Shared。</summary>
    public sealed class DictionaryPool<TKey, TValue> : CollectionPool<Dictionary<TKey, TValue>>
    {
        private readonly IEqualityComparer<TKey> _comparer;
        public static DictionaryPool<TKey, TValue> Shared { get; } = new();

        public DictionaryPool(IEqualityComparer<TKey> comparer = null, int minCapacity = 16, int maxCapacity = 65536,
            long maxBytesPerBucket = 256 * 1024, int maxCountPerBucket = 128)
            : base(minCapacity, maxCapacity, maxBytesPerBucket, maxCountPerBucket)
        {
            _comparer = comparer ?? EqualityComparer<TKey>.Default;
        }

        protected override Dictionary<TKey, TValue> Create(int capacity) => new(capacity, _comparer);
        protected override int GetCapacity(Dictionary<TKey, TValue> item) => item.EnsureCapacity(0);
        protected override long EstimateBytes(int capacity) =>
            (long)capacity * (12L + CollectionPoolMemory.AlignedSize<TKey>() + CollectionPoolMemory.AlignedSize<TValue>()) + 32 * IntPtr.Size;
        protected override void Reset(Dictionary<TKey, TValue> item) => item.Clear();
        protected override bool CanRetain(Dictionary<TKey, TValue> item) => ReferenceEquals(item.Comparer, _comparer);
    }
}
