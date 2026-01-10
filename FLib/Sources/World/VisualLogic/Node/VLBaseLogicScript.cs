//==================={By Qcbf|qcbf@qq.com|5/30/2021 2:55:12 PM}===================

using FLib;
using System;
using System.Collections.Generic;

namespace FLib.Worlds
{
    public abstract class VLBaseLogicScript : VLBaseScript
    {
        public VLLogicNode Node;
        public override string DebugInfo => $"[{Env.DebugInfo}->{Node.Uid}]: ";
        public override void Initialize() { }
        public abstract void Handle();
    }
}
