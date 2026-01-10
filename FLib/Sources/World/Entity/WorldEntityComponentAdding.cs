// ==================== qcbf@qq.com | 2025-07-29 ====================

using System;
using System.Runtime.CompilerServices;
using FLib;

namespace FLib.Worlds
{
    public readonly struct WorldEntityComponentAdding : IDisposable
    {
        public readonly WorldComponentHandleEx Handle;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T RO<T>() where T : IWorldComponentable, new() => ref Handle.RO<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T RW<T>() where T : IWorldComponentable, new() => ref Handle.RW<T>();

        public WorldEntityComponentAdding(WorldComponentHandleEx handle) => Handle = handle;

        /// <summary>
        /// 
        /// </summary>
        public static WorldEntityComponentAdding<T> Create<T>(WorldEntity entity) where T : IWorldComponentable, new() => new(Create(entity, WorldComponentGroup<T>.TypeId));

        /// <summary>
        /// 
        /// </summary>
        public static WorldEntityComponentAdding Create(WorldEntity entity, ushort typeId)
        {
            var group = entity.World.ComponentMgr.GetGroup(typeId);
#if DEBUG
            if (!group.Option(EWorldComponentOption.MultipleInstance) && entity.Exist(typeId))
                throw new Exception($"{WorldComponentManager.GetTypeName(typeId)} is solo");
#endif
            var handle = new WorldComponentHandle(typeId, group.Add(entity, false));
            var entityMgr = entity.World.EntityMgr;
            entityMgr.LinkEntityComponentImpl(entity, handle);
            return new WorldEntityComponentAdding(handle.Cast(entity.World));
        }

        private void Finish()
        {
            var world = Handle.World;
            world.ComponentMgr.GetGroup(Handle.TypeId).AddingFinish(Handle.Index);
            if (world.Syncer?.IsSyncComponent(Handle.TypeId) == true)
                world.Syncer.AddCommand(Handle.Entity, new WorldSyncComponentAddCommand() { Comp = WorldComponentPack.Create(world, Handle) });
        }

        public void Dispose() => Finish();
    }

    public readonly struct WorldEntityComponentAdding<T> : IDisposable where T : IWorldComponentable, new()
    {
        public readonly WorldEntityComponentAdding Adding;

        public ref readonly T RO
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Adding.Handle.RO<T>();
        }

        public ref T RW
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Adding.Handle.RW<T>();
        }

        public WorldEntityComponentAdding(WorldEntityComponentAdding adding) => Adding = adding;

        public void Dispose() => Adding.Dispose();
    }
}
