// ==================== qcbf@qq.com | 2026-01-09 ====================

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace FLib.WorldCores
{
    public struct QueryFilter
    {
        internal ulong[] AllMask;
        internal ulong[] AnyMask;
        internal ulong[] NoneMask;

        public bool IsEmpty => AllMask == null && AnyMask == null && NoneMask == null;

        /// <summary>
        /// 
        /// </summary>
        public QueryFilter All<T>()
        {
            Set(ref AllMask, ComponentRegistry.GetId<T>());
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        public QueryFilter Any<T>()
        {
            Set(ref AnyMask, ComponentRegistry.GetId<T>());
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        public QueryFilter None<T>()
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
        public void Clear()
        {
            AllMask?.AsSpan().Clear();
            AnyMask?.AsSpan().Clear();
            NoneMask?.AsSpan().Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        public readonly bool Match(Archetype archetype)
        {
            if (AllMask != null && !BitArrayOperator.MaskAll(archetype.ComponentMask, AllMask))
                return false;
            if (AnyMask != null && !BitArrayOperator.MaskAny(archetype.ComponentMask, AnyMask))
                return false;
            return NoneMask == null || !BitArrayOperator.MaskAll(archetype.ComponentMask, NoneMask);
        }
    }
}