//==================={By Qcbf|qcbf@qq.com|8/31/2021 10:47:04 AM}===================

using FLib;
using System;
using System.Collections.Generic;

namespace FLib.Worlds
{
    [BytesPackGen, Comment("抛出当前VL局部事件")]
    public partial class VLDispatchCustomEventLogic : VLBaseLogicScript
    {
        [BytesPackGenField, VLFieldComment("事件名")]
        public VLValue<string> Name;

        [BytesPackGenField, VLFieldComment("参数1")]
        public VLValue<string> Arg1;

        [BytesPackGenField, VLFieldComment("参数2")]
        public VLValue<string> Arg2;

        [BytesPackGenField, VLFieldComment("参数3")]
        public VLValue<string> Arg3;

        [BytesPackGenField, VLFieldComment("传递到父节点")]
        public VLValue<bool> ToParent;

        public override void Handle()
        {
            var env = Env;
            if (ToParent)
            {
                while (env != null)
                {
                    env.CustomEventDispatcher?.Invoke(Name, Arg1, Arg2, Arg3);
                    env = env.Parent;
                }
            }
            else
            {
                env.CustomEventDispatcher?.Invoke(Name, Arg1, Arg2, Arg3);
            }
        }
    }
}
