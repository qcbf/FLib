//==================={By Qcbf|qcbf@qq.com|9/18/2023 3:36:10 PM}===================

using System;
using System.Collections.Generic;

namespace FLib.Worlds
{
    public readonly struct StopBehaviorEvent
    {
        public readonly WorldEntity Entity;
        public readonly WorldBehaviorContext RunningContext;
        public readonly bool IsMainBehavior;


        public StopBehaviorEvent(WorldEntity entity, WorldBehaviorContext behavior, bool isMainBehavior)
        {
            Entity = entity;
            RunningContext = behavior;
            IsMainBehavior = isMainBehavior;
        }
    }
}
