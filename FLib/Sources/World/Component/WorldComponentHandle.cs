// =================================================={By Qcbf|qcbf@qq.com|2024-10-29}==================================================

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FLib.Worlds
{
    public readonly struct WorldComponentHandle : IEquatable<WorldComponentHandle>
    {
        public readonly ushort TypeId;
        public readonly ushort Index;

        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => TypeId == 0;
        }

        public WorldComponentHandle(ushort typeId, ushort index)
        {
            TypeId = typeId;
            Index = index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WorldEntity Entity(WorldBase world) => world.ComponentMgr.GetGroup(TypeId).GetContext(Index).Entity;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exist(in WorldEntity entity, WorldBase world) => world.ComponentMgr.GetGroup(TypeId).Exist(new WorldComponentContext(entity, this));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T RO<T>(WorldBase world, ELogLevel logLevel = ELogLevel.Fatal) where T : IWorldComponentable, new()
        {
            if (!IsEmpty) return ref world.ComponentMgr.GetGroup<T>().GetRO(Index);
            Log.Get(logLevel)?.Write("component is empty");
            return ref WorldComponentGroup<T>.Default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T RW<T>(WorldBase world, ELogLevel logLevel = ELogLevel.Fatal) where T : IWorldComponentable, new()
        {
            if (!IsEmpty) return ref world.ComponentMgr.GetGroup<T>().GetRW(Index);
            Log.Get(logLevel)?.Write($"component is empty");
            return ref WorldComponentGroup<T>.Default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool OnlyRemoveThisComponent(WorldEntity entity) => !IsEmpty && entity.World.ComponentMgr.GetGroup(TypeId).Remove(new WorldComponentContext(entity, this));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Destroy(WorldEntity entity) => !IsEmpty && entity.Remove(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref PauseCounter GetPause(WorldBase world) => ref world.ComponentMgr.GetGroup(TypeId).GetPause(Index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WorldComponentHandleEx Cast(WorldBase world) => new(world.Handle, this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WorldComponentHandleEx<T> Cast<T>(WorldBase world) where T : IWorldComponentable, new() => new(world, this);

        public override string ToString() => IsEmpty ? "null" : $"{WorldComponentManager.GetTypeName(TypeId)},{Index}";
        public bool Equals(WorldComponentHandle other) => TypeId == other.TypeId && Index == other.Index;
        public override bool Equals(object obj) => obj is WorldComponentHandle other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(TypeId, Index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in WorldComponentHandle a, in WorldComponentHandle b) => a.Index == b.Index && a.TypeId == b.TypeId;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in WorldComponentHandle a, in WorldComponentHandle b) => a.Index != b.Index || a.TypeId != b.TypeId;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator uint(in WorldComponentHandle handle) => (uint)(handle.TypeId << 16 | handle.Index);
    }

    public readonly struct WorldComponentHandleEx : IEquatable<WorldComponentHandleEx>
    {
        public readonly WorldHandle WorldHandle;
        public readonly WorldComponentHandle Handle;
        public WorldBase World => WorldHandle.Val;
        public ushort TypeId => Handle.TypeId;
        public ushort Index => Handle.Index;
        public bool IsEmpty => World == null || Handle.IsEmpty;
        public WorldEntity Entity => Handle.Entity(World);

        public WorldComponentHandleEx(WorldHandle world, in WorldComponentHandle handle)
        {
            WorldHandle = world;
            Handle = handle;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T RO<T>(ELogLevel logLevel = ELogLevel.Fatal) where T : IWorldComponentable, new() => ref Handle.RO<T>(World, logLevel);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T RW<T>(ELogLevel logLevel = ELogLevel.Fatal) where T : IWorldComponentable, new() => ref Handle.RW<T>(World, logLevel);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool OnlyRemoveThisComponent() => !IsEmpty && Handle.OnlyRemoveThisComponent(Entity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Destroy() => !IsEmpty && Entity.Remove(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref PauseCounter GetPause() => ref Handle.GetPause(World);

        // public bool Exist() => Handle.Exist(World);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WorldComponentHandleEx<T> Cast<T>() where T : IWorldComponentable, new() => new(World, this);

        public override string ToString() => ToString(false);
        public string ToString(bool isVerbose) => IsEmpty ? "null" : World.ComponentMgr.GetGroup(Handle.TypeId).ToString(Handle.Index, isVerbose);
        public static implicit operator WorldComponentHandle(in WorldComponentHandleEx i) => i.Handle;
        public static implicit operator uint(in WorldComponentHandleEx v) => (uint)(v.Handle.TypeId << 16 | v.Handle.Index);
        public bool Equals(WorldComponentHandleEx other) => Handle == other;
        public override bool Equals(object obj) => obj is WorldComponentHandleEx other && Equals(other);
        public override int GetHashCode() => Handle.GetHashCode();
        public static bool operator ==(in WorldComponentHandleEx a, in WorldComponentHandleEx b) => a.Handle == b.Handle;
        public static bool operator !=(in WorldComponentHandleEx a, in WorldComponentHandleEx b) => a.Handle != b.Handle;
    }

    public readonly struct WorldComponentHandleEx<T> : IEquatable<WorldComponentHandleEx<T>> where T : IWorldComponentable, new()
    {
        public readonly WorldHandle WorldHandle;
        public readonly WorldComponentHandle Handle;
        public WorldBase World => WorldHandle.Val;
        public bool IsEmpty => Handle.IsEmpty || WorldHandle.IsEmpty;
        public ushort TypeId => Handle.TypeId;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T RO(ELogLevel logLevel = ELogLevel.Fatal) => ref Handle.RO<T>(World, logLevel);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T RW(ELogLevel logLevel = ELogLevel.Fatal) => ref Handle.RW<T>(World, logLevel);

        public WorldEntity Entity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Handle.Entity(World);
        }

        public WorldComponentHandleEx(WorldHandle worldHandle, in WorldComponentHandle handle)
        {
            WorldHandle = worldHandle;
            Handle = handle;
        }

        public WorldComponentHandleEx(WorldHandle worldHandle, in ushort compIndex) : this(worldHandle, new WorldComponentHandle(WorldComponentGroup<T>.TypeId, compIndex)) { }

        // public bool Exist() => Handle.Exist(World);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref PauseCounter GetPause() => ref Handle.GetPause(World);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool OnlyRemoveThisComponent() => !IsEmpty && Handle.OnlyRemoveThisComponent(Entity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Destroy() => !IsEmpty && Entity.Remove(this);

        public override string ToString() => ToString(false);
        public string ToString(bool isVerbose) => World.ComponentMgr.GetGroup(WorldComponentGroup<T>.TypeId).ToString(Handle.Index, isVerbose);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator WorldComponentHandle(in WorldComponentHandleEx<T> i) => new(WorldComponentGroup<T>.TypeId, i.Handle.Index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator WorldComponentHandleEx(in WorldComponentHandleEx<T> i) => new(i.World.Handle, new WorldComponentHandle(WorldComponentGroup<T>.TypeId, i.Handle.Index));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator uint(in WorldComponentHandleEx<T> v) => (uint)(v.Handle.TypeId << 16 | v.Handle.Index);

        public bool Equals(WorldComponentHandleEx<T> other) => Handle == other.Handle;
        public override bool Equals(object obj) => obj is WorldComponentHandleEx<T> other && Equals(other);
        public override int GetHashCode() => Handle.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in WorldComponentHandleEx<T> a, in WorldComponentHandleEx<T> b) => a.Handle == b.Handle;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in WorldComponentHandleEx<T> a, in WorldComponentHandleEx<T> b) => a.Handle != b.Handle;
    }
}
