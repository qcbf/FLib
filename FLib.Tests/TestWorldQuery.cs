using FLib.WorldCores;
using FLib.WorldCores.Queries;

namespace FLib.Tests;

public class TestWorldQuery
{
    [Fact]
    public void CachedQueryFindsNewArchetypes()
    {
        using var world = new WorldCore();
        world.CreateEntityBuilder().With<Team>().Build();
        var filter = world.CreateQueryBuilder().WithAll<Actor>().WithAny<Team>().WithNone<Enemy>().Build();
        var copy = filter;
        var query = world.Query(filter);
        var typedQuery = world.Query<Actor, Team>(filter);
        Assert.NotNull(filter);
        Assert.Empty(query);
        Assert.Empty(typedQuery);

        world.CreateEntityBuilder().With<Actor>().Build();
        world.CreateEntityBuilder().With<Actor>().With<Team>().With<Enemy>().Build();
        var first = world.CreateEntityBuilder().With<Actor>().With<Team>().Build();
        Assert.Equal([first], query);
        Assert.Equal([first], typedQuery.Select(item => item.Item1));

        var second = world.CreateEntityBuilder().With<Actor>().With<Team>().WithMng<Player>().Build();
        Assert.Equal([first, second], world.Query(copy));
        Assert.Equal([first, second], query);
        Assert.Equal([first, second], typedQuery.Select(item => item.Item1));

        var sameArchetype = world.CreateEntityBuilder().With<Actor>().With<Team>().Build();
        Assert.Equal([first, sameArchetype, second], query);
        Assert.Equal([first, sameArchetype, second], world.Query(copy));
    }

    [Fact]
    public void CachedQueryOwnsBuilderConditions()
    {
        using var world = new WorldCore();
        var builder = world.CreateQueryBuilder().WithAll<Actor>().WithAny<Team>().WithNone<Enemy>().WithShared(new Shared(7));
        var filter = builder.Build();
        builder.Clear();
        builder.WithAll<Enemy>().WithAny<Actor>().WithNone<Team>().WithShared(new Shared(9));

        var first = world.CreateEntityBuilder().With<Actor>().With<Team>().WithShared<Shared>().Build();
        world.SetShared(first, new Shared(7));
        var wrongShared = world.CreateEntityBuilder().With<Actor>().With<Team>().WithShared<Shared>().Build();
        world.SetShared(wrongShared, new Shared(9));
        var missingAny = world.CreateEntityBuilder().With<Actor>().WithShared<Shared>().Build();
        world.SetShared(missingAny, new Shared(7));
        var missingAll = world.CreateEntityBuilder().With<Team>().WithShared<Shared>().Build();
        world.SetShared(missingAll, new Shared(7));
        var excluded = world.CreateEntityBuilder().With<Actor>().With<Team>().With<Enemy>().WithShared<Shared>().Build();
        world.SetShared(excluded, new Shared(7));
        Assert.Equal([first], world.Query(filter));

        var second = world.CreateEntityBuilder().With<Actor>().With<Team>().WithMng<Player>().WithShared<Shared>().Build();
        world.SetShared(second, new Shared(7));
        Assert.Equal([first, second], world.Query(filter));
    }

    [Fact]
    public void DefaultQueriesCanBeEnumeratedAgainAfterArchetypeCreation()
    {
        using var world = new WorldCore();
        Assert.Null(default(WorldQueryFilter));
        var all = world.Query();
        var actors = world.Query<Actor>();
        Assert.Empty(all);
        Assert.Empty(actors);

        var actor = world.CreateEntityBuilder().With<Actor>().Build();
        var team = world.CreateEntityBuilder().With<Team>().Build();
        Assert.Equal([actor, team], all);
        Assert.Equal([actor], actors.Select(item => item.Item1));
    }

    [Fact]
    public void NestedQueryRefreshDoesNotExtendActiveEnumeration()
    {
        using var world = new WorldCore();
        var first = world.CreateEntityBuilder().With<Actor>().Build();
        var second = world.CreateEntityBuilder().With<Actor>().With<Team>().Build();
        var third = world.CreateEntityBuilder().With<Actor>().With<Enemy>().Build();
        var fourth = world.CreateEntityBuilder().With<Actor>().With<Team>().With<Enemy>().Build();
        var filter = world.CreateQueryBuilder().WithAll<Actor>().Build();
        using var outer = world.Query(filter).GetEnumerator();
        Assert.True(outer.MoveNext());
        Assert.Equal(first, outer.Current);

        var fifth = world.CreateEntityBuilder().With<Actor>().WithMng<Player>().Build();
        Assert.Equal([first, second, third, fourth, fifth], world.Query(filter));
        Assert.True(outer.MoveNext());
        Assert.Equal(second, outer.Current);
        Assert.True(outer.MoveNext());
        Assert.Equal(third, outer.Current);
        Assert.True(outer.MoveNext());
        Assert.Equal(fourth, outer.Current);
        Assert.False(outer.MoveNext());
    }

    [Fact]
    public void CachedQueryEnumerationDoesNotAllocateWithoutNewArchetypes()
    {
        using var world = new WorldCore();
        world.CreateEntityBuilder().With<Actor>().Build();
        var query = world.Query(world.CreateQueryBuilder().WithAll<Actor>().Build());
        foreach (var entity in query) { }

        var count = 0;
        var allocated = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 100; ++i)
            foreach (var entity in query)
                ++count;
        allocated = GC.GetAllocatedBytesForCurrentThread() - allocated;

        Assert.Equal(100, count);
        Assert.Equal(0, allocated);
    }
}
