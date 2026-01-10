//==================={By Qcbf|qcbf@qq.com|8/31/2021 10:38:49 AM}===================

using FLib;
using System;
using System.Collections.Generic;

namespace FLib.Worlds
{
    [BytesPackGen, Comment("监听当前VL局部事件")]
    public partial class VLListenCustomEvent : VLBaseEventScript
    {
        [BytesPackGenField, VLFieldComment("事件名")]
        public VLValue<string> ListenName;

        [BytesPackGenField, VLFieldComment("事件参数1保存到")]
        public VLValue<string> SaveToVar1;

        [BytesPackGenField, VLFieldComment("事件参数2保存到")]
        public VLValue<string> SaveToVar2;

        [BytesPackGenField, VLFieldComment("事件参数3保存到")]
        public VLValue<string> SaveToVar3;

        public override void Initialize()
        {
        }

        public override void OnActivate()
        {
            Env.CustomEventDispatcher += OnTriggerEvent;
        }

        private void OnTriggerEvent(string arg1, string arg2, string arg3, string arg4)
        {
            if (arg1 == ListenName)
            {
                SaveToVar1.FixedVLValue.ObjectRawValue = arg2;
                SaveToVar2.FixedVLValue.ObjectRawValue = arg3;
                SaveToVar3.FixedVLValue.ObjectRawValue = arg4;
                EventTrigger();
            }
        }

        public override void OnDeactivate()
        {
            Env.CustomEventDispatcher -= OnTriggerEvent;
        }
    }
}
