//==================={By Qcbf|qcbf@qq.com|9/18/2023 3:35:18 PM}===================

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace FLib.Worlds
{
    [StructLayout(LayoutKind.Auto)]
    public struct DoBehaviorEvent
    {
        public readonly WorldEntity Entity;
        public readonly bool IsMainBehavior;
        public bool IsFirst;
        public readonly WorldBehavior Behavior;
        public WorldBehaviorContext RunningContext;

        public DoBehaviorEvent(WorldEntity entity, WorldBehavior behavior, WorldBehaviorContext ctx, bool isMainBehavior)
        {
            Entity = entity;
            Behavior = behavior;
            IsMainBehavior = isMainBehavior;
            RunningContext = ctx;
            IsFirst = false;
        }
    }
}