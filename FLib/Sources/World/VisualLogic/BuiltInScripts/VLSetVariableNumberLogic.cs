namespace FLib.Worlds
{
    [BytesPackGen, Comment("设置数值变量")]
    public partial class VLSetVariableNumberLogic : VLBaseLogicScript
    {
        [BytesPackGenField, VLFieldComment("值")]
        public VLValue<double> Src;

        [BytesPackGenField, VLFieldComment("目标变量")]
        public VLValue<double> Dst;

        [BytesPackGenField, VLFieldComment("是否累加")]
        public VLValue<bool> IsAdd;

        public override void Handle()
        {
            if (IsAdd && IsAdd.Value)
            {
                Dst.FixedVLValue.ObjectRawValue = Src.Value + Dst.Value;
            }
            else
            {
                Dst.FixedVLValue.ObjectRawValue = Src.Value;
            }
        }
    }
}