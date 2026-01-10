// ==================== qcbf@qq.com | 2026-01-10 ====================

using System;

namespace FLib.WorldCores
{
    public interface IDynamicComponentGroupable
    {
        Array Components { get; }
        void EnsureCapacity(int capacity);

        /// <summary>
        /// 
        /// </summary>
        int Alloc(in Entity et);

        /// <summary>
        /// 
        /// </summary>
        void Free(in Entity et, int idx);

        // /// <summary>
        // /// 
        // /// </summary>
        // bool Has(Entity et, int idx);
    }
}