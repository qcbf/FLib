// =================================================={By Qcbf|qcbf@qq.com|2024-10-26}==================================================

using System;

namespace FLib.Worlds
{
    /// <summary>
    /// 指定组件执行顺序 （After、Before）和（Order）只能有一类排序方式
    /// </summary>
    public class WorldComponentOrderAttribute : Attribute
    {
        /// <summary>
        /// 在指定组件之后执行
        /// </summary>
        public Type After;

        /// <summary>
        /// 在指定组件之前执行
        /// </summary>
        public Type Before;

        /// <summary>
        /// 以数值排序，越小越先执行。
        /// </summary>
        public short Order;
    }
}
