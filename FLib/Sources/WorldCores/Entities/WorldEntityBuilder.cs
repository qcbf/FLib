// ==================== qcbf@qq.com | 2026-01-09 ====================

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using FLib.WorldCores.SoaComponents;
using FLib.WorldCores;
using FLib.WorldCores.Archetypes;
using FLib.WorldCores.Components;

namespace FLib.WorldCores.Entities
{
    [StructLayout(LayoutKind.Auto)]
    public ref struct WorldEntityBuilder
    {
        public WorldCore World;
        public WorldEntityId EntityId;
        internal PooledList<WorldComponentMeta> Components;
        internal PooledList<WorldSoaComponentHandle> DynComponents;


        public WorldEntity Entity => EntityId.AsEntity(World);

        /// <summary>
        /// 
        /// </summary>
        public WorldEntityBuilder(WorldCore world) : this()
        {
            World = world;
            WorldStaticComponentMask.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Has<T>()
        {
            return WorldStaticComponentMask.Get(WorldComponentRegistry.GetMeta<T>());
        }

        /// <summary>
        /// 
        /// </summary>
        public WorldEntityBuilder With<T>() where T : unmanaged
        {
            AddComponent(WorldComponentRegistry.GetInfo<T>(), false);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        public WorldEntityBuilder WithMng<T>()
        {
            AddComponent(WorldComponentRegistry.GetInfo<Mng<T>>(), false);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        public WorldEntityBuilder WithShared<T>() where T : IWorldSharedComponent
        {
            AddComponent(WorldComponentRegistry.GetInfo<T>(), true);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        public WorldEntityBuilder SetDyn<T>(in T component)
        {
            Debug.Assert(!EntityId.IsEmpty, "must call PrepareEntity first");
            var id = WorldComponentRegistry.GetId<T>();
            ref var eti = ref World.GetEntityInfo(Entity);
            var group = World.DynComponentGroups.Get<T>();
            World.TryAddRequiredComponents(EntityId, WorldComponentRegistry.GetInfo(typeof(T)));
            DynComponents.Add(new WorldSoaComponentHandle(World.EnsureDynamicComponentIndex(id, ref eti) = group.AllocWithoutAwake(EntityId, component), id));
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        public WorldEntityBuilder SetDynScript(in ScriptPackBytes script)
        {
            Debug.Assert(!EntityId.IsEmpty, "must call PrepareEntity first");
            ref readonly var compInfo = ref WorldComponentRegistry.GetInfo(script.ScriptType);
            var id = compInfo.Meta.Id;
            ref var eti = ref World.GetEntityInfo(Entity);
            var group = World.DynComponentGroups.Get(id);
            World.TryAddRequiredComponents(EntityId, compInfo);
            var slot = World.EnsureDynamicComponentIndex(id, ref eti) = group.AllocWithoutAwake(EntityId);
            if (compInfo.Type.IsClass)
                group.Components.SetValue(TypeAssistant.New(compInfo.Type), slot);
            WorldComponentRegistry.GetSerializationWrapper(compInfo.Type).Deserialize(ref group.GetPointer(slot), script.InstanceBytes.Span);
            DynComponents.Add(new WorldSoaComponentHandle(slot, id));
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initMemory">是否初始化内存, false:性能会更高,但会导致字段不是默认值</param>
        public WorldEntityBuilder PrepareEntity(bool initMemory = true)
        {
            var hash = WorldStaticComponentMask.HashCode();
            if (!World.ArchetypeGroup.ArchetypeMap.TryGetValue(hash, out var archetype))
            {
                using var archetypeBuilder = new WorldArchetypeBuilder(1);
                for (var i = 0; i < Components.Count; i++)
                    archetypeBuilder.With(Components[i]);
                archetype = World.ArchetypeGroup.Create(hash, archetypeBuilder);
            }

            EntityId = archetype.CreateEntity(out var entityInfo);
            var chunk = entityInfo.Chunk;
            var indexInChunk = entityInfo.IndexInChunk;
            if (initMemory)
            {
                foreach (var t in archetype.ComponentTypes)
                    chunk.ClearMemory(indexInChunk, t);
            }

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        public unsafe WorldEntityBuilder PrepareComponents()
        {
            if (EntityId.IsEmpty)
                PrepareEntity();
            try
            {
                var count = Components.Count;
                if (count > 0)
                {
                    ref readonly var eti = ref World.GetEntityInfo(EntityId);
                    for (var i = 0; i < count; i++)
                    {
                        var meta = Components[i];
                        ref readonly var info = ref WorldComponentRegistry.GetInfo(meta);
                        if (!info.IsShared)
                            info.Awake.Invoke(ref *(byte*)eti.Chunk.Get(eti.IndexInChunk, meta), World, EntityId);
                    }
                }

                foreach (var dynComp in DynComponents)
                    World.DynComponentGroups.Get(dynComp.TypeId).InvokeAwake(EntityId, dynComp.Index);
            }
            finally
            {
                DynComponents.Dispose();
                Components.Dispose();
            }

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        public WorldEntityId Build()
        {
            PrepareComponents();
            WorldSetting.OnCreateEntityEvent?.Invoke(EntityId.AsEntity(World));
            return EntityId;
        }

        /// <summary>
        /// 
        /// </summary>
        public WorldEntity BuildAsEntity()
        {
            return Build().AsEntity(World);
        }

        #region privates

        /// <summary>
        /// 
        /// </summary>
        internal void AddComponent(in WorldComponentInfo info, bool isShared)
        {
            AssertNewComponent(World, info, isShared);
            Components.Add(info);
            WorldStaticComponentMask.Set(info.Meta, true);
            if (info.Options?.RequiredComponents != null)
            {
                foreach (var t in info.Options.RequiredComponents)
                {
                    ref readonly var reqInfo = ref WorldComponentRegistry.GetInfo(t);
                    AddComponent(reqInfo, reqInfo.IsShared);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Conditional("DEBUG")]
        internal static void AssertNewComponent(WorldCore world, in WorldComponentInfo info, bool isShared)
        {
            world.Assert(!WorldStaticComponentMask.Get(info.Meta), msg: "already exist");
            world.Assert(!info.Flags(EComponentFlag.RejectChunk));
            if (isShared)
                world.Assert(!info.HasLifecycle, msg: "nonsupport life invoker");
            else
                world.Assert(!info.IsShared);
            if (typeof(IWorldUpdate).IsAssignableFrom(info.Type))
                world.ThrowException($"static component[{TypeAssistant.GetTypeName(info.Type)}] not support update");
            if (typeof(IWorldStart).IsAssignableFrom(info.Type))
                world.ThrowException($"static component[{TypeAssistant.GetTypeName(info.Type)}] not support start");
        }

        #endregion

        public static implicit operator WorldEntityId(in WorldEntityBuilder builder) => builder.EntityId;
        public static implicit operator WorldEntity(in WorldEntityBuilder builder) => builder.EntityId.AsEntity(builder.World);
        public static implicit operator WorldCore(in WorldEntityBuilder builder) => builder.World;
    }
}
