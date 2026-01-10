// =================================================={By Qcbf|qcbf@qq.com|2024-10-23}==================================================

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FLib.Worlds
{
    public readonly struct WorldComponentContext : IEquatable<WorldComponentContext>
    {
        public readonly WorldEntity Entity;
        public readonly WorldComponentHandle CompHandle;
        public WorldBase World => Entity.World;
        public bool IsEmpty => Entity.IsEmpty;

        public WorldComponentContext(WorldEntity entity, WorldComponentHandle compHandle)
        {
            Entity = entity;
            CompHandle = compHandle;
        }

        public override string ToString() => $"{CompHandle}|{Entity}";
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public WorldComponentContext WithIndex(WorldComponentHandle handle) => new(Entity, handle);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public ref readonly T RO<T>() where T : IWorldComponentable, new() => ref CompHandle.RO<T>(Entity.World);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public ref T RW<T>() where T : IWorldComponentable, new() => ref CompHandle.RW<T>(Entity.World);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool Destroy() => Entity.Remove(this);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public WorldComponentHandleEx<T> ToHandle<T>() where T : IWorldComponentable, new() => new(Entity.World, CompHandle);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public WorldComponentHandleEx ToHandle() => new(Entity.World, CompHandle);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator ==(WorldComponentContext a, WorldComponentContext b) => a.CompHandle == b.CompHandle && a.Entity == b.Entity;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator !=(WorldComponentContext a, WorldComponentContext b) => a.CompHandle != b.CompHandle || a.Entity != b.Entity;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool Equals(WorldComponentContext other) => Entity.Equals(other.Entity) && CompHandle == other.CompHandle;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public override bool Equals(object obj) => obj is WorldComponentContext other && Equals(other);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public override int GetHashCode() => HashCode.Combine(Entity, CompHandle);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static implicit operator WorldComponentHandle(in WorldComponentContext a) => a.CompHandle;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static implicit operator WorldComponentHandleEx(in WorldComponentContext a) => new(a.World, a.CompHandle);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static implicit operator WorldEntity(in WorldComponentContext a) => a.Entity;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static implicit operator WorldBase(in WorldComponentContext a) => a.Entity.World;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static implicit operator WorldHandle(in WorldComponentContext a) => a.Entity.WorldHandle;
    }
}
