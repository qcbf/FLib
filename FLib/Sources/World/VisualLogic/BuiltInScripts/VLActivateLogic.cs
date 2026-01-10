//==================={By Qcbf|qcbf@qq.com|5/30/2021 5:28:05 PM}===================

using FLib;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FLib.Worlds.BuiltInScripts
{
    [BytesPackGen, Comment("激活事件节点")]
    public partial class VLActivateLogic : VLBaseLogicScript
    {
        [BytesPackGenField, VLFieldComment("事件节点Uid")]
        public VLValue<uint> EventNodeUid;

        [BytesPackGenField, VLFieldComment("是否激活")]
        public VLValue<bool> IsActive;

        public override void Handle()
        {
            var node = (VLEventNode)Env.GetNode(EventNodeUid);
            Log.Verbose?.Write($"activate node: {EventNodeUid.Value}:{IsActive.Value}");
            if (IsActive)
            {
                node.TryInitialize();
                node.Activate();
            }
            else
            {
                node.Deactivate();
            }
        }
    }
}
