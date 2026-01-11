// ==================== qcbf@qq.com | 2026-01-09 ====================

using System;

namespace FLib.WorldCores
{
    public struct EntityBuilder : IDisposable
    {
        internal PooledList<ComponentMeta> ComponentTypes;
        internal PooledList<(ComponentMeta, ComponentInvoker.Delegate )> Invokers;

        public EntityBuilder Add<T>() where T : unmanaged
        {
            var meta = ComponentRegistry.GetMeta<T>();
            ComponentTypes.Add(meta);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, meta.Id, true);
            if (ComponentGenericMap<T>.Info.ComponentAwake != null)
                Invokers.Add((meta, ComponentGenericMap<T>.Info.ComponentAwake));
            return this;
        }

        public EntityBuilder AddMng<T>()
        {
            var meta = ComponentRegistry.GetMeta<Mng<T>>();
            ComponentTypes.Add(meta);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, meta.Id, true);
            if (ComponentGenericMap<Mng<T>>.Info.ComponentAwake != null)
                Invokers.Add((meta, ComponentGenericMap<Mng<T>>.Info.ComponentAwake));
            return this;
        }

        public void Dispose()
        {
            ComponentTypes.Dispose();
            Invokers.Dispose();
        }

        public readonly int ComputeHashCode()
        {
            return ComponentRegistry.GetHash(ComponentRegistry.ComponentTypeMaskBuffer);
        }

        public EntityBuilder Finish(WorldCore world, out Entity entity)
        {
            entity = world.CreateEntity(this);
            Dispose();
            return this;
        }
    }
}