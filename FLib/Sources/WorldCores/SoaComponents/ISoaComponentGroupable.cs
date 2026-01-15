// ==================== qcbf@qq.com | 2026-01-10 ====================

using System;

namespace FLib.WorldCores
{
    public interface ISoaComponentGroupable
    {
        WorldCore World { get; set; }
        Array Components { get; }

        /// <summary>
        /// 分配一个动态组件
        /// </summary>
        int Alloc(in Entity et);

        /// <summary>
        /// 释放动态组件
        /// </summary>
        void Free(in Entity et, int key);

        // /// <summary>
        // /// 
        // /// </summary>
        // bool Has(Entity et, int index);

        /// <summary>
        /// 预分配动态组件
        /// </summary>
        void EnsureCapacity(int count);
    }
}