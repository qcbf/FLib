//==================={By Qcbf|qcbf@qq.com|5/30/2021 4:45:33 PM}===================

using FLib;
using System;
using System.Collections.Generic;

namespace FLib.Worlds.BuiltInScripts
{
    [BytesPackGen, Comment("设置字符串变量")]
    public partial class VLSetVariableStringLogic : VLBaseLogicScript
    {
        [BytesPackGenField, VLFieldComment("值")]
        public VLValue<string> Src;

        [BytesPackGenField, VLFieldComment("目标变量")]
        public VLValue<string> Dst;

        public override void Handle()
        {
            Dst.FixedVLValue.ObjectRawValue = Src.Value;
        }
    }
}