// ==================== qcbf@qq.com | 2026-01-14 ====================

#nullable enable
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using FLib.WorldCores.Entities;
using FLib.WorldCores.Components;
using FLib.WorldCores.SoaComponents;

namespace FLib.WorldCores
{
    public partial class WorldCore
    {
        /// <summary>
        /// 获取实体的动态组件。
        /// </summary>
        /// <typeparam name="T">组件的类型</typeparam>
        /// <param name="et">目标实体</param>
        /// <returns>返回对该实体的动态组件的引用</returns>
        public ref T GetDyn<T>(WorldEntityId et)
        {
            var dynIdx = GetEntityInfo(et).DynamicComponentSparseIndex;
            Assert(dynIdx >= 0);
            var compIdx = DynamicComponentSparse.GetRef(dynIdx)[WorldComponentRegistry.GetId<T>()];
            return ref DynComponentGroups.Get<T>()[compIdx];
        }

        /// <summary>
        /// 获取实体的指定类型的动态组件。
        /// </summary>
        /// <param name="et">目标实体</param>
        /// <param name="type">组件的类型</param>
        /// <returns>返回该实体的动态组件实例</returns>
        public object GetDyn(WorldEntityId et, Type type)
        {
            var dynIdx = GetEntityInfo(et).DynamicComponentSparseIndex;
            Assert(dynIdx >= 0);
            var compIdx = DynamicComponentSparse.GetRef(dynIdx)[WorldComponentRegistry.GetId(type)];
            return DynComponentGroups.Get(type).Components.GetValue(compIdx)!;
        }

        /// <summary>
        /// 设置实体的动态组件。
        /// </summary>
        /// <typeparam name="T">组件的类型</typeparam>
        /// <param name="et">目标实体</param>
        /// <param name="component">要设置的组件值</param>
        /// <returns>返回组件在动态组件组中的索引</returns>
        public int SetDyn<T>(WorldEntityId et, in T component)
        {
            return SetDyn(et, component, ref GetEntityInfo(et));
        }

        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// 设置实体的动态组件（使用实体信息引用）。
        /// </summary>
        /// <typeparam name="T">组件的类型</typeparam>
        /// <param name="et">目标实体</param>
        /// <param name="component">要设置的组件值</param>
        /// <param name="eti">实体信息的引用</param>
        /// <returns>返回组件在动态组件组中的索引</returns>
        internal int SetDyn<T>(WorldEntityId et, in T component, ref WorldEntityInfo eti)
        {
            var id = WorldComponentRegistry.GetId<T>();
            ref readonly var info = ref WorldComponentGenericMap<T>.Info;
            Assert(!info.IsShared, et);
            var group = (WorldSoaComponentGroup<T>)DynComponentGroups.Get(id);
            var sparseIndex = eti.DynamicComponentSparseIndex;
            var componentId = id.Id;
            int[]? sparseBuffer = null;
            var index = -1;
            if (sparseIndex >= 0)
            {
                ref var sparse = ref DynamicComponentSparse.GetRef(sparseIndex);
                if (componentId < sparse.Count)
                {
                    sparseBuffer = sparse.Buffer;
                    index = sparseBuffer[componentId];
                }
            }

            if (index < 0)
            {
                Assert(!eti.IsDestroying, et, "entity is destroying");
                if (info.Options?.RequiredComponents is { Length: > 0 })
                {
                    sparseBuffer = null;
                    TryAddRequiredComponents(et, info);
                }
            }
            else if (info.IsAwakeComponent || (info.IsDestroyComponent && info.Flags(EComponentFlag.AlwaysReceiveDestroy)))
            {
                sparseBuffer = null;
                group.Free(et, index, false);
            }
            else
            {
                group[index] = component;
                return index;
            }

            var componentCapacity = group.Components.Length;
            index = group.AllocWithoutAwake(et, component);
            // 分配期间进入虚方法 EnsureCapacity 时，sparse 可能已经扩容。
            if (sparseBuffer != null && index < componentCapacity)
                sparseBuffer[componentId] = index;
            else
                EnsureDynamicComponentIndex(id, ref GetEntityInfo(et)) = index;
            group.InvokeAwake(et, index);
            return index;
        }

        /// <summary>
        /// 设置实体的指定类型的动态组件。
        /// </summary>
        /// <param name="et">目标实体</param>
        /// <param name="component">要设置的组件值</param>
        /// <param name="componentType">组件的类型（若为 null 则使用 component 的实际类型）</param>
        /// <returns>返回组件在动态组件组中的索引</returns>
        public int SetDynObject(WorldEntityId et, object? component, Type? componentType = null)
        {
            componentType ??= component!.GetType();
            Assert(!WorldComponentRegistry.GetInfo(componentType).IsShared, et);
            var id = WorldComponentRegistry.GetId(componentType);
            ref var eti = ref GetEntityInfo(et);
            ref var slot = ref EnsureDynamicComponentIndex(id, ref eti);
            var group = DynComponentGroups.Get(id);
            if (slot < 0)
            {
                TryAddRequiredComponents(et, WorldComponentRegistry.GetInfo(id));
                slot = group.Alloc(et, component);
            }
            else
            {
                group.Components.SetValue(component, slot);
            }

            return slot;
        }

        /// <summary>
        /// 设置实体的动态组件（使用脚本包）。
        /// </summary>
        public int SetDynScript(WorldEntityId et, in ScriptPackBytes script)
        {
            ref readonly var compInfo = ref WorldComponentRegistry.GetInfo(script.ScriptType);
            Assert(!compInfo.IsShared, et);
            ref var eti = ref GetEntityInfo(et);
            ref var slot = ref EnsureDynamicComponentIndex(compInfo.Meta.Id, ref eti);
            var group = DynComponentGroups.Get(compInfo.Meta.Id);
            if (slot < 0)
            {
                TryAddRequiredComponents(et, compInfo);
                slot = group.AllocWithoutAwake(et);
                if (compInfo.Type.IsClass)
                    group.Components.SetValue(TypeAssistant.New(compInfo.Type), slot);
                WorldComponentRegistry.GetSerializationWrapper(compInfo.Type).Deserialize(ref group.GetPointer(slot), script.InstanceBytes.Span);
                group.InvokeAwake(et, slot);
            }
            else
            {
                WorldComponentRegistry.GetSerializationWrapper(compInfo.Type).Deserialize(ref group.GetPointer(slot), script.InstanceBytes.Span);
            }

            return slot;
        }


        /// <summary>
        /// 设置实体的动态组件（使用脚本包）。
        /// </summary>
        public int SetDynScript<T>(WorldEntityId et, in ScriptPackBytes<T> script) where T : IBytesPackable
        {
            return SetDynScript(et, (ScriptPackBytes)script);
        }

        /// <summary>
        /// 移除实体的指定类型的动态组件。
        /// </summary>
        /// <typeparam name="T">组件的类型</typeparam>
        /// <param name="et">目标实体</param>
        public void RemoveDyn<T>(WorldEntityId et)
        {
            RemoveDyn(et, typeof(T));
        }

        /// <summary>
        /// 移除实体的指定类型的动态组件。
        /// </summary>
        /// <param name="et">目标实体</param>
        /// <param name="type">要移除的组件类型</param>
        public void RemoveDyn(WorldEntityId et, Type type)
        {
            Assert(!GetEntityInfo(et).Chunk.Has(WorldComponentRegistry.GetId(type)), et, "cannot remove static component");
            // Assert(!GetEntityInfo(et).IsDestroying, et, "entity is destroying");
            ref readonly var eti = ref GetEntityInfo(et);
            ref var sparse = ref DynamicComponentSparse.GetRef(eti.DynamicComponentSparseIndex);
            var id = WorldComponentRegistry.GetId(type).Id;
            var compIdx = sparse[id];
            sparse[id] = -1;
            DynComponentGroups.Get(type).Free(et, compIdx, false);
        }

        /// <summary>
        /// 检查实体是否拥有指定类型的动态组件。
        /// </summary>
        /// <typeparam name="T">组件的类型</typeparam>
        /// <param name="et">目标实体</param>
        /// <returns>如果实体拥有该动态组件返回 true，否则返回 false</returns>
        public bool HasDyn<T>(WorldEntityId et)
        {
            return HasDyn(et, typeof(T));
        }

        /// <summary>
        /// 检查实体是否拥有指定类型的动态组件。
        /// </summary>
        /// <param name="et">目标实体</param>
        /// <param name="componentType">要检查的组件类型</param>
        /// <returns>如果实体拥有该动态组件返回 true，否则返回 false</returns>
        public bool HasDyn(WorldEntityId et, Type componentType)
        {
            if (et.IsEmpty) return false;
            ref readonly var eti = ref GetEntityInfo(et);
            if (!eti.HasDynamicComponent) return false;
            var compId = WorldComponentRegistry.GetId(componentType).Id;
            ref readonly var sparse = ref DynamicComponentSparse.GetRef(eti.DynamicComponentSparseIndex);
            return compId < sparse.Count && sparse[compId] >= 0;
        }

        /// <summary>
        /// 确保实体有动态组件索引，不存在则创建。
        /// </summary>
        /// <param name="componentId">组件的 ID</param>
        /// <param name="eti">实体信息的引用</param>
        /// <returns>返回对组件索引的引用</returns>
#if NET6_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
#endif
        internal ref int EnsureDynamicComponentIndex(WorldIncrementId componentId, ref WorldEntityInfo eti)
        {
            var compId = componentId.Id;
            if (eti.HasDynamicComponent)
            {
                ref var sparse = ref DynamicComponentSparse.GetRef(eti.DynamicComponentSparseIndex);
                if (sparse.Count <= compId)
                {
                    var oldSize = sparse.Buffer.Length;
                    if (sparse.Allocate(componentId.Raw))
                        sparse.Buffer.AsSpan(oldSize).Fill(-1);
                    sparse.Count = componentId.Raw;
                }

                return ref sparse[compId];
            }
            else
            {
                var sparse = new PooledList<int>(componentId.Raw);
                sparse.Count = componentId.Raw;
                Array.Fill(sparse.Buffer, -1);
                eti.DynamicComponentSparseIndex = DynamicComponentSparse.Add(sparse);
                return ref DynamicComponentSparse.GetRef(eti.DynamicComponentSparseIndex)[compId];
            }
        }

        /// <summary>
        /// 尝试添加该组件的所有必需组件。
        /// </summary>
        /// <param name="et">目标实体</param>
        /// <param name="info">组件信息</param>
        internal void TryAddRequiredComponents(WorldEntityId et, in WorldComponentInfo info)
        {
            if (info.Options?.RequiredComponents == null)
                return;
            foreach (var reqComp in info.Options.RequiredComponents)
                SetDynObject(et, TypeAssistant.New(reqComp), reqComp);
        }
    }
}