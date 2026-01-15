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
        public unsafe Ref<T1> GetSta<T1>(Entity et) where T1 : unmanaged
        {
            ref readonly var eti = ref GetEntityInfo(et);
            return new Ref<T1>(eti.Chunk.Get<T1>(eti.IndexInChunk));
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
            return ref eti.Chunk.GetRef<T1>(eti.IndexInChunk);
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetSta<T1>(Entity et, in T1 val) where T1 : unmanaged
        {
            ref readonly var eti = ref GetEntityInfo(et);
            eti.Chunk.GetRef<T1>(eti.IndexInChunk) = val;
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetShared<T>(Entity et, in T val) where T : unmanaged
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public bool HasSta<T1>(Entity et) where T1 : unmanaged
        {
            return BitArrayOperator.GetBit(ArchetypeGroup[GetEntityInfo(et).ArchetypeIndex].ComponentMask, ComponentRegistry.GetMeta<T1>().Id);
        }

        /// <summary>
        /// 
        /// </summary>
        public bool HasSta(Entity et, Type componentType)
        {
            return BitArrayOperator.GetBit(ArchetypeGroup[GetEntityInfo(et).ArchetypeIndex].ComponentMask, ComponentRegistry.GetMeta(componentType).Id);
        }
    }
}