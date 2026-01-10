//==================={By Qcbf|qcbf@qq.com|5/30/2021 4:03:08 PM}===================

using FLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace FLib.Worlds.BuiltInScripts
{
    [BytesPackGen, Comment("计算器")]
    public partial class VLCalcLogic : VLBaseLogicScript
    {
        [BytesPackGenField, VLFieldComment("值1")]
        public VLValue<FNum> V1;

        [BytesPackGenField, VLFieldComment("值2")]
        public VLValue<FNum> V2;

        [BytesPackGenField, VLFieldComment("操作+-*/")]
        public VLValue<char> OperatorType;

        [BytesPackGenField, VLFieldComment("结果", IsVariableOnly = true)]
        public VLValue<FNum> Result;

        public override void Handle()
        {
            var c = OperatorType.RawValue;
            var r = (VLValue<FNum>)Result.FixedVLValue;
            if (c == '+') r.RawValue = V1.RawValue + V2.RawValue;
            else if (c == '-') r.RawValue = V1.RawValue - V2.RawValue;
            else if (c == '*') r.RawValue = V1.RawValue * V2.RawValue;
            else if (c == '/') r.RawValue = V1.RawValue / V2.RawValue;
        }
    }
}
