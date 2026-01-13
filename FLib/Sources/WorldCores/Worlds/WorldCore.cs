// ==================== qcbf@qq.com |2025-12-11 ====================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FLib.WorldCores
{
    public partial class WorldCore : FEvent, IDisposable, IEnumerable<Entity>
    {
        /// <summary>
        /// 
        /// </summary>
        public ArchetypeGroup ArchetypeGroup;

        /// <summary>
        /// 
        /// </summary>
        public readonly DynamicComponentGroupManager DynamicComponent;

        /// <summary>
        /// 
        /// </summary>
        public FixedIndexList<(ComponentTypeOffsetHelper DenseIndexes, int Size)> DynamicComponentSparse;

        /// <summary>
        /// 
        /// </summary>
        public FixedIndexList<EntityInfo> EntityInfos;

        /// <summary>
        /// 
        /// </summary>
        internal ushort VersionIncrement;

        /// <summary>
        /// 
        /// </summary>
        public ushort GenVersion() => unchecked(++VersionIncrement == 0 ? ++VersionIncrement : VersionIncrement);

        public bool IsDisposed => ArchetypeGroup == null;

        /// <summary>
        /// 
        /// </summary>
        public WorldCore(int entityCapacity = 1024)
        {
            ArchetypeGroup = new ArchetypeGroup(this);
            DynamicComponent = new DynamicComponentGroupManager(this);
            EntityInfos = new FixedIndexList<EntityInfo>(entityCapacity);
            DynamicComponentSparse = new(entityCapacity >> 1);
        }

        #region entity

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

            var et = archetype.CreateEntity(out var chunkEntityIdx);
            var chunk = archetype.Chunks;
            if (initMemory)
            {
                ref readonly var components = ref builder.ComponentTypes;
                for (var i = 0; i < components.Count; i++)
                    chunk.ClearMemory(chunkEntityIdx, components[i]);
            }

            foreach (var (meta, invoker) in builder.Invokers)
                invoker(ref *(byte*)chunk.Get(chunkEntityIdx, meta), this, et);

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
                var sparse = DynamicComponentSparse[eti.DynamicComponentSparseIdx];
                var denseIndexes = sparse.DenseIndexes.Offsets;
                for (var i = 0; i < sparse.Size; i++)
                {
                    var denseIdx = denseIndexes[i];
                    if (denseIdx < 0) continue;
                    var type = ComponentRegistry.GetType(new IncrementId(i + 1));
                    DynamicComponent.GetGroup(type).Free(et, denseIdx);
                }
            }

            ArchetypeGroup[eti.ArchetypeIdx].RemoveEntity(eti);
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
        public IList<object> GetAll(Entity et, IList<object> list = null)
        {
            list ??= new List<object>();
            var eti = GetEntityInfo(et);
            var chunk = eti.Chunk;
            foreach (var meta in ArchetypeGroup[eti.ArchetypeIdx].ComponentTypes)
                list.Add(chunk.GetObj(eti.ChunkEntityIdx, meta));

            if (eti.HasDynamicComponent)
            {
                var sparse = DynamicComponentSparse[eti.DynamicComponentSparseIdx];
                var denseIndexes = sparse.DenseIndexes.Offsets;
                for (var i = 0; i < sparse.Size; i++)
                {
                    var denseIdx = denseIndexes[i];
                    if (denseIdx < 0) continue;
                    var meta = ComponentRegistry.GetInfo(new IncrementId(i + 1)).Meta;
                    var compIdx = DynamicComponentSparse[denseIdx].DenseIndexes.Get(meta.Id);
                    var val = DynamicComponent.GetGroup(meta.Type).Components.GetValue(compIdx);
                    list.Add(val);
                }
            }

            return list;
        }

        #endregion

        #region static components

        /// <summary>
        /// 
        /// </summary>
        public unsafe Ref<T1> GetSta<T1>(Entity et) where T1 : unmanaged
        {
            ref readonly var eti = ref GetEntityInfo(et);
            return new Ref<T1>(eti.Chunk.Get<T1>(eti.ChunkEntityIdx));
        }

        /// <summary>
        /// 
        /// </summary>
        public Mng<T1> GetStaMng<T1>(Entity et)
        {
            return GetStaRef<Mng<T1>>(et);
        }

        /// <summary>
        /// 
        /// </summary>
        public ref T1 GetStaRef<T1>(Entity et) where T1 : unmanaged
        {
            ref readonly var eti = ref GetEntityInfo(et);
            return ref eti.Chunk.GetRef<T1>(eti.ChunkEntityIdx);
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetSta<T1>(Entity et, in T1 val) where T1 : unmanaged
        {
            ref readonly var eti = ref GetEntityInfo(et);
            eti.Chunk.GetRef<T1>(eti.ChunkEntityIdx) = val;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool HasSta<T1>(Entity et) where T1 : unmanaged
        {
            return BitArrayOperator.GetBit(ArchetypeGroup[GetEntityInfo(et).ArchetypeIdx].ComponentMask, ComponentRegistry.GetMeta<T1>().Id);
        }

        /// <summary>
        /// 
        /// </summary>
        public bool HasSta(Entity et, Type componentType)
        {
            return BitArrayOperator.GetBit(ArchetypeGroup[GetEntityInfo(et).ArchetypeIdx].ComponentMask, ComponentRegistry.GetMeta(componentType).Id);
        }

        #endregion

        #region dynamic components

        /// <summary>
        /// 
        /// </summary>
        public ref T GetDyn<T>(Entity et)
        {
            var dynIdx = GetEntityInfo(et).DynamicComponentSparseIdx;
            Debug.Assert(dynIdx >= 0);
            var compIdx = DynamicComponentSparse[dynIdx].DenseIndexes.Get<T>();
            return ref DynamicComponent.GetGroup<T>().Components[compIdx];
        }

        /// <summary>
        /// 
        /// </summary>
        public object GetDyn(Entity et, Type type)
        {
            var dynIdx = GetEntityInfo(et).DynamicComponentSparseIdx;
            Debug.Assert(dynIdx >= 0);
            var compIdx = DynamicComponentSparse[dynIdx].DenseIndexes.Get(type);
            return DynamicComponent.GetGroup(type).Components.GetValue(compIdx);
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetDyn<T>(Entity et, in T component)
        {
            var group = DynamicComponent.GetGroup<T>();
            var compIdx = DynamicComponentIndex(et, group, ComponentRegistry.GetId<T>());
            group.Components[compIdx] = component;
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetDyn(Entity et, object component, Type componentType)
        {
            componentType ??= component.GetType();
            var group = DynamicComponent.GetGroup(componentType);
            var compIdx = DynamicComponentIndex(et, group, ComponentRegistry.GetId(componentType));
            group.Components.SetValue(component, compIdx);
        }

        /// <summary>
        /// 
        /// </summary>
        public void RemoveDyn<T>(Entity et)
        {
            ref var sparse = ref DynamicComponentSparse.GetRef(GetEntityInfo(et).DynamicComponentSparseIdx);
            var compIdx = sparse.DenseIndexes.GetAndClear(ComponentRegistry.GetId<T>());
            DynamicComponent.GetGroup<T>().Free(et, compIdx);
        }

        /// <summary>
        /// 
        /// </summary>
        public void RemoveDyn(Entity et, Type type)
        {
            ref var sparse = ref DynamicComponentSparse.GetRef(GetEntityInfo(et).DynamicComponentSparseIdx);
            var compIdx = sparse.DenseIndexes.GetAndClear(ComponentRegistry.GetId(type));
            DynamicComponent.GetGroup(type).Free(et, compIdx);
        }

        /// <summary>
        /// 
        /// </summary>
        public bool HasDyn<T>(Entity et)
        {
            ref readonly var eti = ref GetEntityInfo(et);
            return eti.HasDynamicComponent && DynamicComponentSparse[eti.DynamicComponentSparseIdx].DenseIndexes.Has<T>();
        }

        /// <summary>
        /// 
        /// </summary>
        public bool HasDyn(Entity et, Type componentType)
        {
            ref readonly var eti = ref GetEntityInfo(et);
            return eti.HasDynamicComponent && DynamicComponentSparse[eti.DynamicComponentSparseIdx].DenseIndexes.Has(ComponentRegistry.GetId(componentType));
        }

        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private int DynamicComponentIndex(Entity et, IDynamicComponentGroupable group, IncrementId componentId)
        {
            ref var eti = ref GetEntityInfo(et);
            int compIdx;
            if (eti.HasDynamicComponent)
            {
                ref var sparse = ref DynamicComponentSparse.GetRef(eti.DynamicComponentSparseIdx);
                if (!sparse.DenseIndexes.TryGet(componentId, out compIdx))
                {
                    sparse.DenseIndexes.ResizeOnPool(componentId);
                    compIdx = sparse.DenseIndexes[componentId] = group.Alloc(et);
                }
            }
            else
            {
                compIdx = group.Alloc(et);
                var sparseIndexes = new ComponentTypeOffsetHelper(componentId, true) { [componentId] = compIdx };
                eti.DynamicComponentSparseIdx = DynamicComponentSparse.Add((sparseIndexes, componentId.Raw));
            }

            return compIdx;
        }

        #endregion


        /// <summary>
        /// 
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// 
        /// </summary>
        public IEnumerator<Entity> GetEnumerator()
        {
            var count = EntityInfos.Count;
            for (ushort i = 0; count > 0; i++)
            {
                if (EntityInfos[i].IsEmpty) continue;
                --count;
                yield return new Entity(i, EntityInfos[i].Version);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed) return;

            foreach (var et in this)
            {
                try
                {
                    RemoveEntity(et);
                }
                catch (Exception e)
                {
                    Log.Error?.Write($"{et}  {e}", nameof(WorldCore), nameof(Dispose));
                }
            }

            for (ushort i = 0; i < ArchetypeGroup.Count; i++)
            {
                try
                {
                    ArchetypeGroup[i].ClearChunks();
                }
                catch (Exception e)
                {
                    Log.Error?.Write($"{i}  {e}", nameof(WorldCore), nameof(Dispose));
                }
            }

            for (var i = 0; i < DynamicComponentSparse.Count; i++)
                DynamicComponentSparse[i].DenseIndexes.ResizeOnPool(default);

            ArchetypeGroup = null;
            GC.SuppressFinalize(this);
        }
    }
}