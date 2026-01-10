//=================================================={By Qcbf|qcbf@qq.com|11/11/2024 12:04:53 PM}==================================================

using FLib;
using System;
using System.Collections.Generic;


namespace FLib.Worlds
{
    [WorldSyncPermission(false), BytesPackGen]
    public partial struct WorldSyncEffectAddCommand : IWorldSyncCommandable
    {
        [BytesPackGenField]
        public WorldEntity CreateBy;
        [BytesPackGenField]
        public uint Id;
        [BytesPackGenField]
        public int AddCount;

        public readonly void Execute(WorldSyncer syncer, WorldEntity entity)
        {
            entity.GetRO<WorldEffectSystem>().Add(CreateBy, Id, AddCount);
        }

    }
}
