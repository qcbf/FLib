// ==================== qcbf@qq.com | 2026-01-05 ====================

using System;
using System.Buffers;
using System.Diagnostics;

namespace FLib.WorldCores
{
    public struct ComponentSparseList
    {
        public int[] List;

        public int this[IncrementId componentId]
        {
            get => Get(componentId);
            set => List[componentId] = value;
        }

        public ComponentSparseList(IncrementId maxId, bool isPooledArray) : this(isPooledArray ? ArrayPool<int>.Shared.Rent(maxId.Raw) : new int[maxId.Raw])
        {
        }

        public ComponentSparseList(int[] list)
        {
            List = list;
            Array.Fill(List, -1);
        }

        public void ResizeOnPool(IncrementId newMaxId)
        {
            if (newMaxId.IsEmpty)
            {
                ArrayPool<int>.Shared.Return(List);
                List = null;
                return;
            }

            if (newMaxId.Raw <= List.Length) return;
            var pool = ArrayPool<int>.Shared;
            var list = pool.Rent(newMaxId.Raw);
            try
            {
                Array.Copy(List, list, List.Length);
            }
            catch
            {
                pool.Return(list);
                throw;
            }

            pool.Return(List);
            List = list;
        }

        public bool TryGet(Type type, out int idx)
        {
            return TryGet(ComponentRegistry.GetId(type), out idx);
        }

        public bool TryGet<T>(out int idx)
        {
            return TryGet(ComponentRegistry.GetId<T>(), out idx);
        }

        public bool TryGet(IncrementId componentId, out int idx)
        {
            if (List.Length < componentId)
            {
                idx = -1;
                return false;
            }

            return (idx = List[componentId]) >= 0;
        }

        public int Get(Type type) => Get(ComponentRegistry.GetId(type));
        public int Get<T>() => Get(ComponentRegistry.GetId<T>());

        public int Get(IncrementId componentId)
        {
            Debug.Assert(Has(componentId), "not found component");
            return List[componentId];
        }

        public int GetAndClear(IncrementId componentId)
        {
            var temp = this[componentId];
            this[componentId] = -1;
            return temp;
        }

        public bool Has<T>() => Has(ComponentRegistry.GetId<T>());
        public bool Has(IncrementId componentId) => componentId.Raw <= List.Length && List[componentId] >= 0;
    }
}