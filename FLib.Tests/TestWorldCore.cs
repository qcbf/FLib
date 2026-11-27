// ==================== qcbf@qq.com | 2026-01-03 ====================

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Linq;
using System.Numerics;
using System.Reflection;
using FLib.WorldCores.Components;
using FLib.WorldCores;
using FLib.WorldCores.Entities;
using FLib.WorldCores.SoaComponents;
using FLib.WorldCores.TimeLogics;

namespace FLib.Tests;

public struct Player
{
    public string Name;
    public override string ToString() => Name;
}

[BytesPackGen]
public partial struct Team
{
    [BytesPackGenField] public byte Value;
    [BytesPackGenField] public byte TestAlign1;
    [BytesPackGenField] public byte TestAlign2;
    public override string ToString() => Value.ToString();

    public void ComponentUpdate(WorldCore world, WorldEntityId eId)
    {
        ++world.Get<Actor>(eId).Id;
        ++Value;
    }
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
    public string Name;
}

[WorldComponentOption(flags: EComponentFlag.DoNotResetMemory | EComponentFlag.AlwaysReceiveDestroy)]
public struct Managed : IWorldAwake, IWorldDestroy, IWorldUpdate, IWorldStart
{
    public List<string> Values;
    public uint AwakeFrame;
    public uint StartFrame;
    public uint UpdateFrame;

    public void OnComponentAwake(WorldCore world, WorldEntityId entityId)
    {
        Values = [nameof(OnComponentAwake)];
        AwakeFrame = world.Frame;
    }

    public void OnComponentStart(WorldCore world, WorldEntityId eId)
    {
        Values.Add(nameof(OnComponentStart));
        StartFrame = world.Frame;
    }

    public void OnComponentDestroy(WorldCore world, WorldEntityId entityId) => Values.Add(nameof(OnComponentDestroy));

    public void OnComponentUpdate(WorldCore world, WorldEntityId entityId)
    {
        Values.Add(nameof(OnComponentUpdate));
        UpdateFrame = world.Frame;
    }
}

public record struct Shared(int Value) : IWorldSharedComponent;

public class TestWorldCore
{
    public TestWorldCore()
    {
        TestUtility.RegisterFLibLog();
    }

    [Fact]
    [SuppressMessage("Usage", "CA2263:类型已知时首选泛型重载")]
    public void BasicAll()
    {
        var world = new WorldCore();

        var et = world.CreateEntityBuilder().Build();
        Assert.False(world.Has<Player>(et));
        world.RemoveEntity(et);

        WorldComponentRegistry.GetMeta<Buff>();
        var player1 = world.CreateEntityBuilder().WithMng<Player>().With<Team>().With<Actor>().Build();
        world.SetSta(player1, new Team { Value = 5 });
        world.SetStaMng(player1, new Player { Name = "p1" });

        var player2 = world.CreateEntityBuilder().With<Team>().With<Actor>().WithMng<Player>().Build();
        world.Set(player2, new Team { Value = 10 });

        var enemy1 = world.CreateEntityBuilder().With<Enemy>().With<Team>().With<Actor>().Build();
        world.Set(enemy1, new Team { Value = 100 });

        Assert.Equal(world.Entities[player1.Id].ArchetypeIndex, world.Entities[player2.Id].ArchetypeIndex);
        Assert.False(world.HasSta<Enemy>(player1));
        Assert.False(world.HasStaMng<Player>(enemy1));
        Assert.NotEqual(world.Entities[player1.Id].ArchetypeIndex, world.Entities[enemy1.Id].ArchetypeIndex);
        Assert.ThrowsAny<Exception>(() => world.SetSta(player1, new Enemy()));
        Assert.Equal(5, world.GetSta<Team>(player1).Val.Value);
        Assert.Equal(10, world.GetStaRef<Team>(player2).Value);
        Assert.Equal(100, world.Get<Team>(enemy1).Value);
        Assert.Equal("p1", world.GetStaMng<Player>(player1).Val.Name);
        Assert.Null(world.GetStaMng<Player>(player2).Val.Name);

        Assert.Equal([5, 10, 100], world.Query<Team>().Select(vv => vv.Item2.Val.Value));
        Assert.Equal([5, 10], world.Query<Team>(world.CreateQueryBuilder().WithAll<Team>().WithNone<Enemy>()).Select(vv => vv.Item2.Val.Value));

        // entity
        Assert.Equal(["p1", "5", "FLib.Tests.Actor"], ((List<object>)world.GetAll(player1)).Select(v1 => v1.ToString()));
        world.RemoveEntity(player1);
        Assert.False(world.HasEntity(player1));
        Assert.ThrowsAny<Exception>(() => world.GetSta<Team>(player1));
        Assert.Equal(10, world.GetSta<Team>(player2).Val.Value);
        Assert.Equal(1, world.GetEntityInfo(player2).Chunk.Count);

        // managed
        player1 = world.CreateEntityBuilder().WithMng<Player>().With<Team>().With<Actor>().Build();
        world.SetSta(player1, new Team { Value = 6 });
        Assert.Equal(6, world.GetSta<Team>(player1).Val.Value);
        Assert.Equal(10, world.Get<Team>(player2).Value);
        Assert.Equal(100, world.GetSta<Team>(enemy1).Val.Value);

        // dynamic
        Assert.False(world.HasDyn<Buff>(player1));
        world.SetDyn(player1, new Buff { Name = "abc" });
        foreach (var item in world.CreateQueryBuilder().WithNone<Enemy>().Query())
            Log.Info?.Write(item, nameof(TestWorldCore), nameof(BasicAll));
        Assert.Equal([player2, player1], world.CreateQueryBuilder().WithNone<Enemy>().Query());

        Assert.True(world.Has<Buff>(player1));
        Assert.Equal("abc", world.GetDyn<Buff>(player1).Name);
        Assert.Equal("abc", ((Buff)world.GetDyn(player1, typeof(Buff))).Name);
        world.RemoveDyn<Buff>(player1);
        Assert.False(world.HasDyn<Buff>(player1));
        Assert.ThrowsAny<Exception>(() => world.GetDyn<Buff>(player1));
        world.SetDynObject(player1, new Buff { Name = "aaa" });
        Assert.Equal("aaa", world.GetDyn<Buff>(player1).Name);
        world.Remove<Buff>(player1);
        Assert.False(world.Has<Buff>(player1));
        Assert.Equal(0, world.DynComponentGroups.Get<Buff>().ValidCount);
        world.SetDyn(player1, new Buff { Name = "abc2" });
        Assert.Equal(1, world.DynComponentGroups.Get<Buff>().ValidCount);

        // get all
        Assert.Equal([typeof(Mng<Player>), typeof(Team), typeof(Actor), typeof(Buff)], ((List<object>)world.GetAll(player1)).Select(vv => vv.GetType()));

        // dispose
        world.RemoveEntity(player1);
        Assert.Equal(0, world.DynComponentGroups.Get<Buff>().ValidCount);
        world.Dispose();
        // Assert.True(WorldGlobalSetting.ChunkAllocator.FreePagesCount >= 2);
        // Assert.Empty((IEnumerable)typeof(Mng<Player>).GetField("_objects", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!);
    }


    [Fact]
    public void SharedComponent()
    {
        using var world = new WorldCore();
        var et1 = world.CreateEntityBuilder().With<Team>().With<Actor>().WithMng<Player>().WithShared<Shared>().Build();
        Assert.Equal(0, world.GetEntityInfo(et1).Chunk.AllSharedComponentsHash);

        world.SetSta(et1, new Team { Value = 10 });
        world.SetShared(et1, new Shared(1));

        Assert.NotEqual(0, world.GetEntityInfo(et1).Chunk.AllSharedComponentsHash);
        Assert.Equal(10, world.Get<Team>(et1).Value);

        var et2 = world.CreateEntityBuilder().With<Team>().With<Actor>().WithMng<Player>().WithShared<Shared>().Build();
        world.SetSta(et2, new Team { Value = 10 });
        world.SetShared(et2, new Shared(10));

        Assert.Equal([typeof(Team), typeof(Actor), typeof(Mng<Player>), typeof(Shared)], ((List<object>)world.GetAll(et1)).Select(v => v.GetType()));

        Assert.Equal([et1], world.CreateQueryBuilder().WithShared(new Shared(1)).Query());
        Assert.Equal([et2], world.CreateQueryBuilder().WithShared(new Shared(10)).Query());
        Assert.Equal([et1, et2], world.CreateQueryBuilder().WithAll<Team>().Query());
    }


    [Fact]
    public void ComponentSystem()
    {
        using var world = new WorldCore();
        world.Update();
        var et = world.CreateEntityBuilder().With<Team>().With<Actor>().WithMng<Player>().WithShared<Shared>().Build();
        var meta = world.GetEntityInfo(et).Chunk.SparseComponentMeta;
        world.Set(et, new Managed());
        var sparse = world.DynamicComponentSparse.GetRef(world.GetEntityInfo(et).DynamicComponentSparseIndex);
        Assert.False(ReferenceEquals(meta, sparse.Buffer));
        world.Set(et, new Player { Name = "abc" });

        Assert.Equal([nameof(IWorldAwake.OnComponentAwake)], world.Get<Managed>(et).Values);
        Assert.Equal(1u, world.Get<Managed>(et).AwakeFrame);

        world.Update();
        Assert.Equal([nameof(IWorldAwake.OnComponentAwake), nameof(IWorldStart.OnComponentStart), nameof(IWorldUpdate.OnComponentUpdate)], world.Get<Managed>(et).Values);
        Assert.Equal(2u, world.Get<Managed>(et).StartFrame);

        world.Update();
        Assert.Equal([nameof(IWorldAwake.OnComponentAwake), nameof(IWorldStart.OnComponentStart), nameof(IWorldUpdate.OnComponentUpdate), nameof(IWorldUpdate.OnComponentUpdate)], world.Get<Managed>(et).Values);
        Assert.Equal(2u, world.Get<Managed>(et).StartFrame);
        Assert.Equal(3u, world.Get<Managed>(et).UpdateFrame);

        Assert.Equal("abc", world.DynComponentGroups.Get<Player>()[0].Name);
        world.RemoveEntity(et);

        Assert.Equal([nameof(IWorldAwake.OnComponentAwake), nameof(IWorldStart.OnComponentStart), nameof(IWorldUpdate.OnComponentUpdate), nameof(IWorldUpdate.OnComponentUpdate), nameof(IWorldDestroy.OnComponentDestroy)],
            world.DynComponentGroups.Get<Managed>()[0].Values);

        Assert.Null(world.DynComponentGroups.Get<Player>()[0].Name);
    }

    private struct GrowsSparseOnAwake : IWorldAwake
    {
        public bool HasSelfOnAwake;

        public void OnComponentAwake(WorldCore world, WorldEntityId entityId)
        {
            HasSelfOnAwake = world.HasDyn<GrowsSparseOnAwake>(entityId);
            var sparseIndex = world.GetEntityInfo(entityId).DynamicComponentSparseIndex;
            ref var sparse = ref world.DynamicComponentSparse.GetRef(sparseIndex);
            sparse.Allocate(sparse.Buffer.Length + 1);
        }
    }

    [Fact]
    public void SetDynKeepsIndexWhenAwakeGrowsSparse()
    {
        using var world = new WorldCore();
        var et = world.CreateEntityBuilder().Build();

        world.SetDyn(et, new GrowsSparseOnAwake());

        Assert.True(world.HasDyn<GrowsSparseOnAwake>(et));
        Assert.True(world.GetDyn<GrowsSparseOnAwake>(et).HasSelfOnAwake);
    }

    private sealed class BuffGroupThatGrowsSparse(WorldCore world, WorldEntityId entityId) : WorldSoaComponentGroup<Buff>(world)
    {
        public override bool EnsureCapacity(int capacity)
        {
            var sparseIndex = World.GetEntityInfo(entityId).DynamicComponentSparseIndex;
            ref var sparse = ref World.DynamicComponentSparse.GetRef(sparseIndex);
            sparse.Allocate(sparse.Buffer.Length + 1);
            return base.EnsureCapacity(capacity);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SetDynKeepsIndexForExistingSparseSlot(bool resizeDuringAllocation)
    {
        using var world = new WorldCore();
        var et = world.CreateEntityBuilder().Build();
        world.SetDyn(et, new Buff());
        world.RemoveDyn<Buff>(et);
        if (resizeDuringAllocation)
            world.DynComponentGroups.Groups[WorldComponentRegistry.GetId<Buff>()] = new BuffGroupThatGrowsSparse(world, et);

        world.SetDyn(et, new Buff { Name = "updated" });

        Assert.True(world.HasDyn<Buff>(et));
        Assert.Equal("updated", world.GetDyn<Buff>(et).Name);
    }

    [Fact]
    public void ManyEntities()
    {
        var w = new WorldCore();
        var count = 3000;
        for (var i = 0; i < count; i++)
            w.CreateEntityBuilder().With<Team>().BuildAsEntity();
        Assert.Equal(count, w.Entities.ValidCount);
        foreach (var etId in w.CreateQueryBuilder().WithNone<Actor>().Query())
        {
            if (count <= 661)
                Log.Info?.Write("", nameof(TestWorldCore), nameof(ManyEntities));
            w.RemoveEntity(etId);
            Assert.Equal(--count, w.Entities.ValidCount);
        }

        Assert.Equal(0, count);
        Assert.Equal(0, w.Entities.ValidCount);
    }

    [Fact]
    public void StableEntityIndicesRemainQueryableAfterLowerSlotRemoval()
    {
        using var world = new WorldCore();
        var first = world.CreateEntityBuilder().Build();
        var second = world.CreateEntityBuilder().Build();
        var third = world.CreateEntityBuilder().Build();

        world.RemoveEntity(first);

        Assert.True(world.HasEntityAndNotDestroying(third));
        Assert.Equal([second, third], world.GetAllEntities());
        Assert.Equal([second, third], world.ToArray());
    }

    [Fact]
    public void WorldHandleRemainsValidAfterLowerWorldRemoval()
    {
        using var first = new WorldCore();
        using var second = new WorldCore();
        // ReSharper disable once DisposeOnUsingVariable
        first.Dispose();
        Assert.False(second.Handle.IsEmpty);
        Assert.Same(second, second.Handle.World);
    }

    [Fact]
    public void SoaEnumeratorSkipsRemovedLowerSlot()
    {
        using var world = new WorldCore();
        var first = world.CreateEntityBuilder().Build();
        var second = world.CreateEntityBuilder().Build();
        var third = world.CreateEntityBuilder().Build();
        WorldComponentRegistry.GetMeta<Buff>();

        var group = world.DynComponentGroups.Get<Buff>();
        var firstIndex = group.AllocWithoutAwake(first, new Buff { Name = "first" });
        group.AllocWithoutAwake(second, new Buff { Name = "second" });
        group.AllocWithoutAwake(third, new Buff { Name = "third" });
        group.Free(first, firstIndex, false);

        using var enumerator = group.GetEnumerator();
        var entities = new List<WorldEntityId>();
        while (enumerator.MoveNext())
            entities.Add(enumerator.Entity);

        Assert.Equal([second, third], entities);
    }
}
