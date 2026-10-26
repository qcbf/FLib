using System.Collections.Concurrent;
using FLib.Collections;

namespace FLib.Tests;

public class TestCollectionPool
{
    [Fact]
    public void GrowthMovesAnInstanceToItsActualCapacityBucket()
    {
        var pool = new ListPool<int>();
        var grown = pool.Rent(16);
        for (var i = 0; i < 1000; i++) grown.Add(i);
        var capacity = grown.Capacity;
        Assert.True(pool.Return(grown));

        var small = pool.Rent(16);
        Assert.NotSame(grown, small);
        Assert.Equal(16, small.Capacity);
        var reused = pool.Rent(capacity);
        Assert.Same(grown, reused);
        Assert.Empty(reused);
        Assert.Equal(0, pool.RetainedCount);
        Assert.Equal(0, pool.RetainedBytes);
    }

    [Fact]
    public void NonPowerOfTwoCapacitiesCannotUndersizeARental()
    {
        var pool = new ArrayCollectionPool<int>();
        var irregular = new int[31];
        Assert.True(pool.Return(irregular));
        Assert.Equal(32, pool.Rent(17).Length);
        Assert.Same(irregular, pool.Rent(16));
    }

    [Fact]
    public void HashCollectionsRespectRequestedCapacityWithPrimeSizedStorage()
    {
        var sets = new HashSetPool<int>();
        var dictionaries = new DictionaryPool<int, int>();
        var capacities = new[] { 0, 1, 15, 16, 17, 23, 31, 32, 33, 63, 64, 65, 511, 1025 };
        for (var pass = 0; pass < 3; pass++)
        {
            foreach (var capacity in capacities)
            {
                var set = sets.Rent(capacity);
                var dictionary = dictionaries.Rent(capacity);
                Assert.True(set.EnsureCapacity(0) >= capacity);
                Assert.True(dictionary.EnsureCapacity(0) >= capacity);
                Assert.Empty(set);
                Assert.Empty(dictionary);
                set.Add(123);
                dictionary.Add(123, 456);
                Assert.True(sets.Return(set));
                Assert.True(dictionaries.Return(dictionary));
            }
        }
    }

    [Fact]
    public void DictionaryReturnUsesCapacityEvenWhenCountIsZero()
    {
        var pool = new DictionaryPool<int, object>();
        var grown = pool.Rent();
        grown.EnsureCapacity(1000);
        grown.Add(1, new object());
        grown.Clear();
        Assert.True(pool.Return(grown));
        Assert.NotSame(grown, pool.Rent());
        Assert.Same(grown, pool.Rent(1024));
    }

    [Fact]
    public void ByteBudgetRetainsFewerLargeCollections()
    {
        const long budget = 4096;
        var pool = new ListPool<int>(maxBytesPerBucket: budget);
        var smallCost = RetainedCost(pool, 16);
        var largeCost = RetainedCost(pool, 256);

        for (var i = 0; i < 128; i++) pool.Return(new List<int>(16));
        var smallCount = pool.RetainedCount;
        Assert.Equal(budget / smallCost, smallCount);
        Assert.True(pool.RetainedBytes <= budget);
        pool.Clear();

        for (var i = 0; i < 128; i++) pool.Return(new List<int>(256));
        Assert.Equal(budget / largeCost, pool.RetainedCount);
        Assert.True(pool.RetainedCount < smallCount);
        Assert.True(pool.RetainedBytes <= budget);
    }

    [Fact]
    public void BucketBudgetsAreIndependentAndHaveACountLimit()
    {
        var pool = new ListPool<int>(maxBytesPerBucket: 4096, maxCountPerBucket: 2);
        for (var i = 0; i < 10; i++)
        {
            pool.Return(new List<int>(16));
            pool.Return(new List<int>(32));
        }
        Assert.Equal(4, pool.RetainedCount);
        pool.Clear();
        Assert.Equal(0, pool.RetainedCount);
        Assert.Equal(0, pool.RetainedBytes);
        Assert.Equal(16, pool.Rent().Capacity);
    }

    [Fact]
    public void ElementSizeIsIncludedInByteBudget()
    {
        var narrow = new ArrayCollectionPool<byte>(maxBytesPerBucket: 2048);
        var wide = new ArrayCollectionPool<decimal>(maxBytesPerBucket: 2048);
        for (var i = 0; i < 128; i++)
        {
            narrow.Return(new byte[16]);
            wide.Return(new decimal[16]);
        }
        Assert.True(wide.RetainedCount < narrow.RetainedCount);
        Assert.InRange(narrow.RetainedBytes, 1, 2048);
        Assert.InRange(wide.RetainedBytes, 1, 2048);
    }

    [Fact]
    public void OversizeAndDisabledPoolsDoNotRetainInstances()
    {
        var pool = new ListPool<int>(maxCapacity: 64);
        var large = pool.Rent(1000);
        Assert.True(large.Capacity >= 1000);
        large.Add(1);
        Assert.False(pool.Return(large));
        Assert.Single(large); // 拒收时不做无用的 Clear。
        Assert.False(pool.Return(new List<int>(1)));
        Assert.Equal(0, pool.RetainedCount);
        Assert.False(new ListPool<int>(maxBytesPerBucket: 0).Return(new List<int>(16)));
        Assert.False(new ListPool<int>(maxCountPerBucket: 0).Return(new List<int>(16)));
    }

    [Fact]
    public void ComparersAreIsolatedByPool()
    {
        var sets = new HashSetPool<string>(StringComparer.OrdinalIgnoreCase);
        var dictionaries = new DictionaryPool<string, int>(StringComparer.OrdinalIgnoreCase);
        var set = sets.Rent();
        var dictionary = dictionaries.Rent();
        set.Add("A");
        dictionary.Add("A", 1);
        Assert.False(set.Add("a"));
        Assert.Equal(1, dictionary["a"]);
        Assert.False(HashSetPool<string>.Shared.Return(set));
        Assert.False(DictionaryPool<string, int>.Shared.Return(dictionary));
        Assert.True(sets.Return(set));
        Assert.True(dictionaries.Return(dictionary));
        Assert.Same(set, sets.Rent());
        Assert.Same(dictionary, dictionaries.Rent());
    }

    [Fact]
    public void PooledListGrowthPreservesUnusedBufferSlots()
    {
        var list = new PooledList<int>(16);
        try
        {
            list.Add(42);
            // 动态组件稀疏表在 Count 之外也保存 -1 哨兵，扩容必须保留。
            list.Buffer.AsSpan(list.Count).Fill(-1);
            var oldCapacity = list.Buffer.Length;
            list.Allocate(oldCapacity + 1);

            Assert.Single(list);
            Assert.Equal(42, list[0]);
            for (var i = list.Count; i < oldCapacity; i++)
                Assert.Equal(-1, list.Buffer[i]);
        }
        finally
        {
            list.Dispose();
        }
    }

    [Fact]
    public void PooledListTransfersBufferToEnumerator()
    {
        var list = new PooledList<int>(16);
        list.Add(1);
        list.Add(2);
        list[1] = 3;

        var items = list.ForEachWithDispose();
        Assert.False(list.IsInitialized);
        Assert.Empty(list);
        var sum = 0;
        foreach (var item in items)
            sum += item;
        Assert.Equal(4, sum);
        list.Dispose();
    }

    [Fact]
    public void DefaultWrappersHaveCorrectEmptySemantics()
    {
        using var set = new PooledHashSet<int>();
        using var dictionary = new PooledDictionary<int, int>();
        Assert.Empty(set);
        Assert.True(set.IsSubsetOf(new[] { 1 }));
        Assert.True(set.SetEquals(Array.Empty<int>()));
        foreach (var item in set) Assert.Fail("Set should be empty.");
        foreach (var item in dictionary) Assert.Fail("Dictionary should be empty.");
        Assert.False(dictionary.TryGetValue(1, out _));
        Assert.False(dictionary.ContainsKey(1));
        Assert.False(dictionary.Remove(1));
        Assert.Equal(7, dictionary.GetValueOrDefault(1, 7));
        Assert.True(set.IsProperSubsetOf(new[] { 1 }));
        Assert.False(set.IsProperSupersetOf(Array.Empty<int>()));
        Assert.True(set.IsSupersetOf(Array.Empty<int>()));
        Assert.False(set.Overlaps(new[] { 1 }));
        set.CopyTo(Array.Empty<int>(), 0);
        Assert.True(set.IsNull);
        Assert.True(dictionary.IsNull);
        set.Add(2);
        dictionary.Add(2, 3);
        Assert.Contains(2, set);
        Assert.Equal(3, dictionary[2]);
    }

    [Fact]
    public void EmptyReadsPreserveValidationWithoutRenting()
    {
        var dictionary = new PooledDictionary<string, int>();
        Assert.Throws<ArgumentNullException>(() => dictionary.ContainsKey(null!));
        Assert.Throws<ArgumentNullException>(() => dictionary.TryGetValue(null!, out _));
        Assert.Throws<ArgumentNullException>(() => dictionary.GetValueOrDefault(null!));
        Assert.Throws<ArgumentNullException>(() => dictionary.Remove(null!));
        Assert.Throws<ArgumentNullException>(() => dictionary[null!]);
        Assert.Throws<KeyNotFoundException>(() => dictionary["missing"]);
        Assert.True(dictionary.IsNull);

        IReadOnlyDictionary<string, int> readOnly = dictionary;
        Assert.Equal(7, readOnly.GetValueOrDefault("missing", 7));
        Assert.True(((PooledDictionary<string, int>)readOnly).IsNull);
        ICollection<KeyValuePair<string, int>> items = dictionary;
        Assert.False(items.Contains(new("missing", 0)));
        Assert.False(items.Remove(new("missing", 0)));
        items.CopyTo(Array.Empty<KeyValuePair<string, int>>(), 0);
        Assert.Throws<ArgumentNullException>(() => items.CopyTo(null!, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => items.CopyTo(Array.Empty<KeyValuePair<string, int>>(), -1));
        Assert.True(((PooledDictionary<string, int>)items).IsNull);

        var set = new PooledHashSet<int>();
        Assert.Throws<ArgumentNullException>(() => set.IsSubsetOf(null!));
        Assert.Throws<ArgumentNullException>(() => set.SetEquals(null!));
        Assert.Throws<ArgumentNullException>(() => set.CopyTo(null!, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => set.CopyTo(Array.Empty<int>(), -1));
        Assert.True(set.IsNull);
    }

    [Fact]
    public void EnsureCapacityRentsTheRequestedBucketAndPreservesComparer()
    {
        var sets = new HashSetPool<string>(StringComparer.OrdinalIgnoreCase);
        var dictionaries = new DictionaryPool<string, int>(StringComparer.OrdinalIgnoreCase);
        var set = new PooledHashSet<string>(16, sets);
        var dictionary = new PooledDictionary<string, int>(16, dictionaries);
        set.Dispose();
        dictionary.Dispose();
        var largeSet = sets.Rent(1024);
        var largeDictionary = dictionaries.Rent(1024);
        sets.Return(largeSet);
        dictionaries.Return(largeDictionary);

        try
        {
            Assert.True(set.EnsureCapacity(1024) >= 1024);
            Assert.True(dictionary.EnsureCapacity(1024) >= 1024);
            Assert.Same(largeSet, set.Raw);
            Assert.Same(largeDictionary, dictionary.Raw);
            set.Add("A");
            dictionary.Add("A", 123);
            Assert.False(set.Add("a"));
            Assert.Equal(123, dictionary.GetValueOrDefault("a"));
            Assert.True(set.EnsureCapacity(2048) >= 2048);
            Assert.True(dictionary.EnsureCapacity(2048) >= 2048);
            Assert.False(set.Add("a"));
            Assert.Equal(123, dictionary.GetValueOrDefault("a"));
        }
        finally
        {
            set.Dispose();
            dictionary.Dispose();
        }
    }

    [Fact]
    public void EmptyEnsureCapacityValidatesWithoutRenting()
    {
        var set = new PooledHashSet<int>();
        var dictionary = new PooledDictionary<int, int>();
        Assert.Equal(0, set.EnsureCapacity(0));
        Assert.Equal(0, dictionary.EnsureCapacity(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => set.EnsureCapacity(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => dictionary.EnsureCapacity(-1));
        Assert.True(set.IsNull);
        Assert.True(dictionary.IsNull);
    }

    [Fact]
    public void DictionaryViewsObserveSubsequentChanges()
    {
        using var dictionary = new PooledDictionary<int, int>();
        var keys = dictionary.Keys;
        var values = dictionary.Values;
        dictionary.Add(1, 2);
        Assert.Contains(1, keys);
        Assert.Contains(2, values);
        dictionary.Remove(1);
        Assert.Empty(keys);
        Assert.Empty(values);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DictionaryGetValueOrDefaultDoesNotAllocate(bool populated)
    {
        using var dictionary = new PooledDictionary<uint, long>();
        if (populated) dictionary.Add(1, 123);
        Read(dictionary);
        var before = GC.GetAllocatedBytesForCurrentThread();
        var total = Read(dictionary);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.Equal(populated ? 123000L : 7000L, total);
        Assert.Equal(0, allocated);
        Assert.Equal(!populated, dictionary.IsNull);

        static long Read(in PooledDictionary<uint, long> dictionary)
        {
            long total = 0;
            for (var i = 0; i < 1000; i++)
                total += dictionary.GetValueOrDefault(1u, 7L);
            return total;
        }
    }

    [Fact]
    public void WrappersReturnToTheirConfiguredPoolAndDisposeIsIdempotent()
    {
        var sets = new HashSetPool<string>(StringComparer.OrdinalIgnoreCase);
        var dictionaries = new DictionaryPool<string, object>(StringComparer.OrdinalIgnoreCase);
        var set = new PooledHashSet<string>(16, sets);
        var dictionary = new PooledDictionary<string, object>(16, dictionaries);
        set.Add("A");
        dictionary.Add("A", new object());
        Assert.False(set.Add("a"));
        Assert.NotNull(dictionary["a"]);
        set.Dispose();
        dictionary.Dispose();
        set.Dispose();
        dictionary.Dispose();
        Assert.Equal(1, sets.RetainedCount);
        Assert.Equal(1, dictionaries.RetainedCount);
        Assert.Empty(sets.Rent());
        Assert.Empty(dictionaries.Rent());
    }

    [Fact]
    public void ConcurrentRentReturnNeverSharesALiveInstanceOrExceedsBudgets()
    {
        const int workerCount = 8;
        var pool = new ListPool<int>(maxBytesPerBucket: 4096, maxCountPerBucket: 4);
        var active = new ConcurrentDictionary<List<int>, byte>(ReferenceEqualityComparer.Instance);
        Parallel.For(0, workerCount, worker =>
        {
            for (var i = 0; i < 2000; i++)
            {
                var list = pool.Rent(16);
                Assert.True(active.TryAdd(list, 0));
                Assert.Empty(list);
                list.Add(i);
                Assert.True(active.TryRemove(list, out _));
                pool.Return(list);
            }
        });
        Assert.Empty(active);
        Assert.InRange(pool.RetainedCount, 1, 4);
        Assert.InRange(pool.RetainedBytes, 1, 4096);
    }

    [Fact]
    public void ConcurrentClearAndReturnKeepAccountingConsistent()
    {
        var pool = new ListPool<int>(maxCountPerBucket: 8);
        Parallel.For(0, 8, worker =>
        {
            for (var i = 0; i < 1000; i++)
            {
                var list = pool.Rent(16 << (worker % 3));
                list.Add(i);
                pool.Return(list);
                if (i % 37 == 0) pool.Clear();
            }
        });
        var rentals = new List<List<int>>();
        for (var capacity = 16; capacity <= 64; capacity <<= 1)
            for (var i = 0; i < 8; i++) rentals.Add(pool.Rent(capacity));
        Assert.Equal(0, pool.RetainedCount);
        Assert.Equal(0, pool.RetainedBytes);
        Assert.Equal(rentals.Count, rentals.Distinct(ReferenceEqualityComparer.Instance).Count());
    }

    [Fact]
    public void WarmedPoolOperationsDoNotAllocate()
    {
        var pool = new DictionaryPool<int, int>();
        for (var i = 0; i < 100; i++) Exercise(pool);
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 1000; i++) Exercise(pool);
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);

        static void Exercise(DictionaryPool<int, int> pool)
        {
            using var dictionary = new PooledDictionary<int, int>(16, pool);
            dictionary.Add(1, 2);
            foreach (var item in dictionary) _ = item.Value;
        }
    }

    [Fact]
    public void InvalidPoolParametersAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ListPool<int>(minCapacity: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ListPool<int>(minCapacity: 3));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ListPool<int>(maxCapacity: 8));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ListPool<int>(maxBytesPerBucket: -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ListPool<int>(maxCountPerBucket: -1));
        var pool = new ListPool<int>();
        Assert.Throws<ArgumentOutOfRangeException>(() => pool.Rent(-1));
        Assert.Throws<ArgumentNullException>(() => pool.Return(null!));
    }

    private static long RetainedCost(ListPool<int> pool, int capacity)
    {
        Assert.True(pool.Return(pool.Rent(capacity)));
        var bytes = pool.RetainedBytes;
        pool.Clear();
        return bytes;
    }
}
