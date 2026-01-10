//==================={By Qcbf|qcbf@qq.com|5/30/2021 2:56:40 PM}===================

using FLib;
using System;
using System.Collections.Generic;

namespace FLib.Worlds
{
    public abstract class VLBaseEventScript : VLBaseScript
    {
        public VLEventNode Node;

        public override string DebugInfo => $"[{Env.DebugInfo}->{Node.Uid}]: ";
        public abstract void OnActivate();
        public abstract void OnDeactivate();
        public void EventTrigger()
        {
            Node.EventTrigger();
        }
    }
}
