// =================================================={By Qcbf|qcbf@qq.com|2024-09-25}==================================================

namespace FLib.Worlds
{
    public enum EWorldEffectAddOption : byte
    {
        [Comment("忽略新的")] IgnoreNew,
        [Comment("重置时间")] ResetTime,
        [Comment("多实例", "多个相同id的效果同时运行，相互独立")] MultipleInstance,
        [Comment("替换老的")] Replace,
        [Comment("堆叠层数")] AddStack,
        [Comment("堆叠层数并且每次堆叠重置时间")] AddStackWithResetTime,
    }
}
