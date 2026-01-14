// ==================== qcbf@qq.com | 2026-01-03 ====================

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FLib.WorldCores;

namespace FLib.Tests;

public struct Player
{
    public string Name;
}

public struct Team
{
    public byte Value;
    public byte TestAlign1;
    public byte TestAlign2;
    public override string ToString() => Value.ToString();
}

public struct Actor
{
    public uint Id;
}

public struct Enemy
{
    public uint Ai;
}

public struct Buff
{
    public DynamicComponentContext DynamicComponentContext { get; set; }
    public string Name;
}

public class TestWorldCore
{
    [Fact]
    public void Basic()
    {
        var world = new WorldCore();
        ComponentRegistry.GetMeta<Buff>();
        var player1 = world.CreateEntity<Mng<Player>, Team, Actor>(v2: new Team() { Value = 5 }, v1: Mng.T(new Player() { Name = "p1" }));
        using var _ = new EntityBuilder().Add<Team>().Add<Actor>().AddMng<Player>().Finish(world, out var player2);
        world.SetSta(player2, new Team() { Value = 10 });
        var enemy1 = world.CreateEntity<Enemy, Team, Actor>(v2: new Team() { Value = 100 });

        Assert.Equal(world.EntityInfos[player1.Id].ArchetypeIndex, world.EntityInfos[player2.Id].ArchetypeIndex);
        Assert.False(world.HasSta<Enemy>(player1));
        Assert.False(world.HasSta<Mng<Player>>(enemy1));
        Assert.NotEqual(world.EntityInfos[player1.Id].ArchetypeIndex, world.EntityInfos[enemy1.Id].ArchetypeIndex);
        Assert.ThrowsAny<Exception>(() => world.SetSta(player1, new Enemy()));
        Assert.Equal(5, world.GetSta<Team>(player1).Val.Value);
        Assert.Equal(10, world.GetSta<Team>(player2).Val.Value);
        Assert.Equal(100, world.GetSta<Team>(enemy1).Val.Value);
        Assert.Equal("p1", world.GetStaMng<Player>(player1).Val.Name);
        Assert.Null(world.GetStaMng<Player>(player2).Val.Name);

        // query
        var results = new Queue<int>([5, 10, 100]);
        world.Query<Team>((_, ref v1) => Assert.Equal(v1.Value, results.Dequeue()));
        Assert.Empty(results);

        results = new Queue<int>([5, 10]);
        world.Query<Team>((_, ref v1) => Assert.Equal(v1.Value, results.Dequeue()), new QueryFilter().None<Enemy>());
        Assert.Empty(results);

        //entity
        Assert.Equal(["FLib.Tests.Player", "5", "FLib.Tests.Actor"], world.GetAllEntities(player1).Select(v => v.ToString()));
        world.RemoveEntity(player1);
        Assert.False(world.HasEntity(player1));
        Assert.ThrowsAny<Exception>(() => world.GetSta<Team>(player1));
        Assert.Equal(10, world.GetSta<Team>(player2).Val.Value);
        Assert.Equal(1, world.GetEntityInfo(player2).Chunk.Count);

        // managed
        player1 = world.CreateEntity<Mng<Player>, Team, Actor>(v2: new Team() { Value = 6 });
        Assert.Equal(6, world.GetSta<Team>(player1).Val.Value);
        Assert.Equal(10, world.GetSimple<Team>(player2).Value);
        Assert.Equal(100, world.GetSta<Team>(enemy1).Val.Value);

        // dynamic
        Assert.False(world.HasDyn<Buff>(player1));
        world.SetDyn(player1, new Buff() { Name = "abc" });
        // results = new Queue<int>([player2.Version, enemy1.Version]);
        // world.Query(et => Assert.Equal(results.Dequeue(), et.Version), new QueryFilter().None<Buff>());
        // Assert.Empty(results);

        Assert.True(world.HasDyn<Buff>(player1));
        Assert.Equal("abc", world.GetDyn<Buff>(player1).Name);
        Assert.Equal("abc", ((Buff)world.GetDyn(player1, typeof(Buff))).Name);
        world.RemoveDyn<Buff>(player1);
        Assert.False(world.HasDyn<Buff>(player1));
        Assert.ThrowsAny<Exception>(() => world.GetDyn<Buff>(player1));
        world.SetDyn(player1, new Buff() { Name = "aaa" }, null);
        Assert.Equal("aaa", world.GetDyn<Buff>(player1).Name);
        world.RemoveDyn<Buff>(player1);
        Assert.False(world.HasDyn<Buff>(player1));
        Assert.Equal(0, world.DynamicComponent.GetGroup<Buff>().Count);
        world.SetDyn(player1, new Buff() { Name = "abc2" });
        Assert.Equal(1, world.DynamicComponent.GetGroup<Buff>().Count);
        world.RemoveEntity(player1);
        Assert.Equal(0, world.DynamicComponent.GetGroup<Buff>().Count);

        // dispose
        world.Dispose();
        Assert.Equal(2, GlobalSetting.ChunkAllocator.FreePagesCount);
    }
}