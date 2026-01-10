// ==================== qcbf@qq.com | 2025-07-01 ====================

using System;

namespace FLib.Worlds
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class WorldComponentOptionAttribute : Attribute
    {
        public EWorldComponentOption Options;
        public WorldComponentOptionAttribute(EWorldComponentOption options) => Options = options;
    }

    [Flags]
    public enum EWorldComponentOption
    {
        None,

        /// <summary>
        /// 在entity被销毁时依然调用End方法，默认情况下Add和Remove组件会调用对应方法，但是Add之后因为Entity移除导致的组件销毁不会调用End。
        /// </summary>
        CallEndOnEntityDestroyed = 1,

        /// <summary>
        /// 当entity被暂停时忽略依然执行update
        /// </summary>
        IgnoreEntityPause = 1 << 1,

        /// <summary>
        /// 针对class组件，是否使用对象池
        /// </summary>
        Pooling = 1 << 2,

        /// <summary>
        /// 允许存在多个
        /// </summary>
        MultipleInstance = 1 << 4,
    }
}
