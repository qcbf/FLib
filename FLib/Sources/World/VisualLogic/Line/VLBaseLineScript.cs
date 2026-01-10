//==================={By Qcbf|qcbf@qq.com|5/30/2021 2:58:07 PM}===================

using FLib;
using System;
using System.Collections.Generic;

namespace FLib.Worlds
{
    public abstract class VLBaseLineScript : VLBaseScript
    {
        public VLLine Line;
        public override string DebugInfo => $"[{Env.DebugInfo}->{Line.LeftNode.Uid}]: ";
        public override void Initialize() { }
        public abstract bool Handle();
    }
}
