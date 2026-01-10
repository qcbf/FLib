// ==================== qcbf@qq.com | 2025-08-29 ====================

using System;
using System.Runtime.CompilerServices;
using FLib;

namespace FLib.Worlds
{
    public struct EntityAddComponentBatch : IDisposable
    {
        public WorldEntity Entity;
        public PooledList<WorldComponentHandle> Components;
        public EntityAddComponentBatch(WorldEntity entity) : this() => Entity = entity;

        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>(in T comp) where T : IWorldComponentable, new() => Components.Add(Entity.CreateComponent(Entity.World.ComponentMgr.GetGroup<T>(), comp));

        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>() where T : IWorldComponentable, new() => Add(WorldComponentGroup<T>.TypeId);

        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(ushort typeId) => Components.Add(Entity.CreateComponent(Entity.World.ComponentMgr.GetGroup(typeId), typeId));

        /// <summary>
        /// 
        /// </summary>
        public void Finish()
        {
            var w = Entity.World;
            foreach (var comp in Components)
                w.ComponentMgr.GetGroup(comp.TypeId).AddingFinish(comp.Index);
            Components.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Finish();
            Components.Dispose();
        }
    }
}
