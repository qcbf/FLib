//==================={By Qcbf|qcbf@qq.com|11/24/2021 4:50:10 PM}===================

using FLib;
using System;
using System.Collections.Generic;

namespace FLib.Worlds
{
    [BytesPackGen, Comment("字符串相等")]
    public partial class VLStringEqualsLine : VLBaseLineScript
    {
        [BytesPackGenField, VLFieldComment("值1")]
        public VLValue<string> V1;

        [BytesPackGenField, VLFieldComment("值2")]
        public VLValue<string> V2;

        public override bool Handle()
        {
            return V1.Value == V2.Value;
        }
    }
}
