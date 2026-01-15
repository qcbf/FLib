// ==================== qcbf@qq.com | 2026-01-14 ====================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace FLib.WorldCores
{
    public partial class WorldCore
    {
        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref EntityInfo GetEntityInfoOrEmpty(in Entity et)
        {
            ref var eti = ref EntityInfos.GetRef(et.Id);
            if (eti.Version != et.Version)
                return ref EntityInfo.Empty;
            return ref eti;
        }

        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref EntityInfo GetEntityInfo(in Entity et)
        {
            ref var eti = ref EntityInfos.GetRef(et.Id);
            Debug.Assert(eti.Version == et.Version, "version error");
            return ref eti;
        }

        /// <summary>
        /// 
        /// </summary>
        public unsafe Entity CreateEntity(in EntityBuilder builder, bool initMemory = true)
        {
            var hash = builder.ComputeHashCode();
            if (!ArchetypeGroup.ArchetypeMap.TryGetValue(hash, out var archetype))
            {
                using var archetypeBuilder = new ArchetypeBuilder(1);
                for (var i = 0; i < builder.ComponentTypes.Count; i++)
                    archetypeBuilder.Add(builder.ComponentTypes[i]);
                archetype = ArchetypeGroup.Create(hash, archetypeBuilder);
            }

            var et = archetype.CreateEntity(out var entityInfo);
            var chunk = entityInfo.Chunk;
            var indexInChunk = entityInfo.IndexInChunk;
            if (initMemory)
            {
                ref readonly var components = ref builder.ComponentTypes;
                for (var i = 0; i < components.Count; i++)
                    chunk.ClearMemory(indexInChunk, components[i]);
            }

            foreach (var (meta, invoker) in builder.Invokers)
                invoker(ref *(byte*)chunk.Get(indexInChunk, meta), this, et);

            return et;
        }

        /// <summary>
        /// 
        /// </summary>
        public void RemoveEntity(Entity et)
        {
            ref readonly var eti = ref GetEntityInfo(et);
            if (eti.HasDynamicComponent)
            {
                var sparse = DynamicComponentSparse[eti.DynamicComponentSparseIndex];
                var denseIndexes = sparse.List;
                for (var i = 0; i < denseIndexes.Length; i++)
                {
                    var denseIndex = denseIndexes[i];
                    if (denseIndex < 0) continue;
                    var type = ComponentRegistry.GetType(new IncrementId(i + 1));
                    Soa.GetGroup(type).Free(et, denseIndex);
                }
            }

            ArchetypeGroup[eti.ArchetypeIndex].RemoveEntity(eti);
            EntityInfos.RemoveAt(et.Id);
        }


        /// <summary>
        /// 
        /// </summary>
        public bool HasEntity(Entity et)
        {
            return !et.IsEmpty && EntityInfos.Count > et.Id && EntityInfos.GetRef(et.Id).Version == et.Version;
        }


        /// <summary>
        /// 
        /// </summary>
        public IList<object> GetAllEntities(Entity et, IList<object> list = null)
        {
            list ??= new List<object>();
            var eti = GetEntityInfo(et);
            var chunk = eti.Chunk;
            foreach (var meta in ArchetypeGroup[eti.ArchetypeIndex].ComponentTypes)
                list.Add(chunk.GetObj(eti.IndexInChunk, meta));

            if (eti.HasDynamicComponent)
            {
                var sparse = DynamicComponentSparse[eti.DynamicComponentSparseIndex];
                var denseIndexes = sparse.List;
                for (var i = 0; i < denseIndexes.Length; i++)
                {
                    var denseIndex = denseIndexes[i];
                    if (denseIndex < 0) continue;
                    var meta = ComponentRegistry.GetInfo(new IncrementId(i + 1)).Meta;
                    var compIdx = DynamicComponentSparse[denseIndex].Get(meta.Id);
                    var val = Soa.GetGroup(meta.Type).Components.GetValue(compIdx);
                    list.Add(val);
                }
            }

            return list;
        }
    }
}