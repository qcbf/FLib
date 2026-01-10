//=================================================={By Qcbf|qcbf@qq.com|11/10/2024 3:25:48 PM}==================================================

using FLib;
using System;
using System.Collections.Generic;


namespace FLib.Worlds
{
    [WorldSyncPermission(false), WorldSyncCommandExecutor(typeof(WorldSyncEntityCommandExecutor)), BytesPackGen]
    public partial struct WorldSyncEntityCommand : IWorldSyncCommandable
    {
        [BytesPackGenField]
        public bool IsDestroy;

        public WorldSyncEntityCommand(bool isDestroy)
        {
            IsDestroy = isDestroy;
        }

        public readonly void Execute(WorldSyncer syncer, WorldEntity entity)
        {
            if (IsDestroy)
                syncer.World.EntityMgr.RemoveEntityImpl(entity);
        }
    }
}
