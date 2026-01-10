//==================={By Qcbf|qcbf@qq.com|5/30/2021 4:41:58 PM}===================

using FLib;
using System;
using System.Collections.Generic;

namespace FLib.Worlds.BuiltInScripts
{
    [BytesPackGen, Comment("打印日志")]
    public partial class VLPrintLogic : VLBaseLogicScript
    {
        [BytesPackGenField, VLFieldComment("标签")]
        public VLValue<string> Tag;

        [BytesPackGenField, VLFieldComment("内容")]
        public VLValue<string> Content;

        public override void Handle()
        {
            Log.Info?.Write("[" + Tag + "] " + Content);
        }
    }
}
