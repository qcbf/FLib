// ==================== qcbf@qq.com | 2026-01-09 ====================

using System;

namespace FLib.WorldCores
{
    public ref struct QueryFilter
    {
        internal ulong[] AllMask;
        internal ulong[] AnyMask;
        internal ulong[] NoneMask;

        /// <summary>
        /// 
        /// </summary>
        public QueryFilter All<T>() where T : unmanaged
        {
            Set(ref AllMask, ComponentRegistry.GetId<T>());
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        public QueryFilter Any<T>() where T : unmanaged
        {
            Set(ref AnyMask, ComponentRegistry.GetId<T>());
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        public QueryFilter None<T>() where T : unmanaged
        {
            Set(ref NoneMask, ComponentRegistry.GetId<T>());
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        public static void Set(ref ulong[] mask, IncrementId componentId)
        {
            if (mask == null || mask.Length <= BitArrayOperator.GetBitsLength(componentId.Raw))
                Array.Resize(ref mask, BitArrayOperator.GetBitsLength(componentId.Raw));
            BitArrayOperator.SetBit(mask, componentId, true);
        }

        /// <summary>
        /// 
        /// </summary>
        public readonly bool Match(Archetype archetype)
        {
            if (AllMask == null && AnyMask == null && NoneMask == null)
                return true;
            if (AllMask != null && !BitArrayOperator.MaskAll(archetype.ComponentMask, AllMask))
                return false;
            if (AnyMask != null && !BitArrayOperator.MaskAny(archetype.ComponentMask, AnyMask))
                return false;
            return NoneMask == null || !BitArrayOperator.MaskAll(archetype.ComponentMask, NoneMask);
        }
    }
}