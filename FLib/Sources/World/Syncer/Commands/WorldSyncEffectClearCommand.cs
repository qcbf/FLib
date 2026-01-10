//=================================================={By Qcbf|qcbf@qq.com|11/11/2024 12:04:53 PM}==================================================

using FLib;
using System;
using System.Collections.Generic;


namespace FLib.Worlds
{
    [WorldSyncPermission(false), BytesPackGen]
    public partial struct WorldSyncEffectClearCommand : IWorldSyncCommandable
    {
        [BytesPackGenField]
        public uint Flags;

        public readonly void Execute(WorldSyncer syncer, WorldEntity entity)
        {
            PooledList<uint> list = new();
            try
            {
                entity.GetRO<WorldEffectSystem>().Clear(Flags, ref list);
            }
            finally
            {
                list.ReleasePool();
            }
        }

    }
}
