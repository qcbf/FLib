//=================================================={By Qcbf|qcbf@qq.com|11/11/2024 12:05:58 PM}==================================================

using FLib;
using System;
using System.Collections.Generic;


namespace FLib.Worlds
{
    [WorldSyncPermission(false), BytesPackGen]
    public partial struct WorldSyncComponentSetCommand : IWorldSyncCommandable
    {
        [BytesPackGenField]
        public WorldComponentPack Comp;

        public readonly void Execute(WorldSyncer syncer, WorldEntity entity)
        {
            entity.Set(Comp);
        }

    }
}
