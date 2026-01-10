//=================================================={By Qcbf|qcbf@qq.com|11/11/2024 12:04:53 PM}==================================================

using FLib;
using System;
using System.Collections.Generic;


namespace FLib.Worlds
{
    [WorldSyncPermission(false), BytesPackGen]
    public partial struct WorldSyncEffectRemoveCommand : IWorldSyncCommandable
    {
        [BytesPackGenField]
        public uint Id;
        [BytesPackGenField]
        public int LinkedId;
        [BytesPackGenField]
        public int RemoveCount;

        public readonly void Execute(WorldSyncer syncer, WorldEntity entity)
        {
            entity.GetRO<WorldEffectSystem>().Remove(Id, RemoveCount, LinkedId);
        }

    }
}
