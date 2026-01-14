// ==================== qcbf@qq.com | 2026-01-14 ====================

using System;

namespace FLib.WorldCores
{
    public partial class WorldCore
    {
        /// <summary>
        /// 
        /// </summary>
        public unsafe ref T GetSimple<T>(Entity et)
        {
            ref readonly var eti = ref GetEntityInfo(et);
            if (eti.HasDynamicComponent && DynamicComponentSparse[eti.DynamicComponentSparseIdx].TryGet<T>(out var denseIdx))
                return ref DynamicComponent.GetGroup<T>().Components[denseIdx];
            return ref *eti.Chunk.Get<T>(eti.ChunkEntityIdx);
        }
    }
}