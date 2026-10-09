// ==================== qcbf@qq.com | 2026-03-09 ====================

using System;

namespace FLib.WorldCores.Effects
{
    [Comment("添加方式"), Flags]
    public enum EWorldEffectAddOption : byte
    {
        [Comment("重置时间")] ResetTime,
        [Comment("忽略新的")] IgnoreNew,
        [Comment("多实例", "多个相同id的效果同时运行，相互独立")] MultipleInstance,
        [Comment("替换老的")] Replace,
        [Comment("堆叠层数")] AddStack,
        [Comment("堆叠层数并且每次堆叠重置时间")] AddStackAndResetTime = AddStack | 0x1,
        [Comment("堆叠层数并且时间到移除全部层数")] AddStackAndTimeoutAllStack = AddStack | 0x2,
    }
}