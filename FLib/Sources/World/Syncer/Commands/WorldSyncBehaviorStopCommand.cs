//=================================================={By Qcbf|qcbf@qq.com|11/10/2024 10:42:22 PM}==================================================

using System;
using System.Collections.Generic;


namespace FLib.Worlds
{
    [WorldSyncPermission(false), BytesPackGen]
    public partial struct WorldSyncBehaviorStopCommand : IWorldSyncCommandable
    {
        [BytesPackGenField]
        public int TypeId;

        [BytesPackGenField]
        public bool IsDoDefault;

        public readonly void Execute(WorldSyncer syncer, WorldEntity entity)
        {
            var bSys = entity.GetRO<WorldBehaviorSystem>();
            if (TypeId > 0)
            {
                bSys.Stop(WorldBehaviorSystem.AllBehaviors[TypeId - 1], IsDoDefault);
            }
            else
            {
                bSys.StopAll(IsDoDefault);
            }
        }
    }
}
