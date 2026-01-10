//=================================================={By Qcbf|qcbf@qq.com|11/11/2024 12:18:31 PM}==================================================

using FLib;
using System;
using System.Collections.Generic;


namespace FLib.Worlds
{
    [WorldSyncPermission(false), BytesPackGen]
    public partial struct WorldSyncComponentRemoveCommand : IWorldSyncCommandable
    {
        [BytesPackGenField]
        public ushort TypeId;
        [BytesPackGenField]
        public int LinkedId;

        public readonly void Execute(WorldSyncer syncer, WorldEntity entity)
        {
            if (LinkedId < 0)
            {
                entity.RemoveAll(TypeId);
            }
            else
            {
                entity.Remove(TypeId, LinkedId);
            }
        }
    }
}
