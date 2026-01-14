// ==================== qcbf@qq.com | 2026-01-14 ====================

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace FLib.WorldCores
{
    public partial class WorldCore
    {
        /// <summary>
        /// 
        /// </summary>
        public ref T GetDyn<T>(Entity et)
        {
            var dynIdx = GetEntityInfo(et).DynamicComponentSparseIdx;
            Debug.Assert(dynIdx >= 0);
            var compIdx = DynamicComponentSparse[dynIdx].Get<T>();
            return ref DynamicComponent.GetGroup<T>().Components[compIdx];
        }

        /// <summary>
        /// 
        /// </summary>
        public object GetDyn(Entity et, Type type)
        {
            var dynIdx = GetEntityInfo(et).DynamicComponentSparseIdx;
            Debug.Assert(dynIdx >= 0);
            var compIdx = DynamicComponentSparse[dynIdx].Get(type);
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
            ref readonly var eti = ref GetEntityInfo(et);
            ref var sparse = ref DynamicComponentSparse.GetRef(eti.DynamicComponentSparseIdx);
            var id = ComponentRegistry.GetId<T>();
            var compIdx = sparse.GetAndClear(id);
            DynamicComponent.GetGroup<T>().Free(et, compIdx);
        }

        /// <summary>
        /// 
        /// </summary>
        public void RemoveDyn(Entity et, Type type)
        {
            ref readonly var eti = ref GetEntityInfo(et);
            ref var sparse = ref DynamicComponentSparse.GetRef(eti.DynamicComponentSparseIdx);
            var id = ComponentRegistry.GetId(type);
            var compIdx = sparse.GetAndClear(id);
            DynamicComponent.GetGroup(type).Free(et, compIdx);
        }

        /// <summary>
        /// 
        /// </summary>
        public bool HasDyn<T>(Entity et)
        {
            ref readonly var eti = ref GetEntityInfo(et);
            return eti.HasDynamicComponent && DynamicComponentSparse[eti.DynamicComponentSparseIdx].Has<T>();
        }

        /// <summary>
        /// 
        /// </summary>
        public bool HasDyn(Entity et, Type componentType)
        {
            ref readonly var eti = ref GetEntityInfo(et);
            return eti.HasDynamicComponent && DynamicComponentSparse[eti.DynamicComponentSparseIdx].Has(ComponentRegistry.GetId(componentType));
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
                if (!sparse.TryGet(componentId, out compIdx))
                {
                    sparse.ResizeOnPool(componentId);
                    compIdx = sparse[componentId] = group.Alloc(et);
                }
            }
            else
            {
                compIdx = group.Alloc(et);
                var sparseIndexes = new ComponentSparseList(componentId, true) { [componentId] = compIdx };
                eti.DynamicComponentSparseIdx = DynamicComponentSparse.Add(sparseIndexes);
            }

            return compIdx;
        }
    }
}