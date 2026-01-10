// ==================== qcbf@qq.com | 2026-01-05 ====================

using System;
using System.Buffers;
using System.Diagnostics;

namespace FLib.WorldCores
{
    public struct ComponentTypeOffsetHelper
    {
        public int[] Offsets;

        public int this[IncrementId componentId]
        {
            get => Get(componentId);
            set => Offsets[componentId] = value;
        }

        public ComponentTypeOffsetHelper(IncrementId maxId, bool isPooledArray) : this(isPooledArray ? ArrayPool<int>.Shared.Rent(maxId.Raw) : new int[maxId.Raw])
        {
        }

        public ComponentTypeOffsetHelper(int[] offsets)
        {
            Offsets = offsets;
            Array.Fill(Offsets, -1);
        }

        public void ResizeOnPool(IncrementId newMaxId)
        {
            if (newMaxId.Raw <= Offsets.Length) return;
            var pool = ArrayPool<int>.Shared;
            var newOffsets = pool.Rent(newMaxId.Raw);
            try
            {
                Array.Copy(Offsets, newOffsets, Offsets.Length);
            }
            catch (Exception ex)
            {
                pool.Return(newOffsets);
                throw;
            }

            pool.Return(Offsets);
            Offsets = newOffsets;
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
            if (Offsets.Length < componentId)
            {
                idx = -1;
                return false;
            }

            return (idx = Offsets[componentId]) >= 0;
        }

        public int Get(Type type) => Get(ComponentRegistry.GetId(type));
        public int Get<T>() => Get(ComponentRegistry.GetId<T>());

        public int Get(IncrementId componentId)
        {
            Debug.Assert(Has(componentId), "not found component");
            return Offsets[componentId];
        }

        public bool Has<T>() => Has(ComponentRegistry.GetId<T>());
        public bool Has(IncrementId componentId) => componentId.Raw <= Offsets.Length && Offsets[componentId] >= 0;

        public int GetAndClear(IncrementId componentId)
        {
            var temp = this[componentId];
            this[componentId] = -1;
            return temp;
        }
    }
}