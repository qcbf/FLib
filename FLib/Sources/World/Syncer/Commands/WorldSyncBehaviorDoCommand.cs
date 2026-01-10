//=================================================={By Qcbf|qcbf@qq.com|11/10/2024 10:42:22 PM}==================================================

using System;
using System.Collections.Generic;


namespace FLib.Worlds
{
    [BytesPackGen, WorldSyncPermission(false)]
    public partial struct WorldSyncBehaviorDoCommand : IWorldSyncCommandable
    {
        [BytesPackGenField]
        public int TypeId;

        [BytesPackGenField]
        public WorldComponentPack ArgComponent;

        public readonly void Execute(WorldSyncer syncer, WorldEntity entity)
        {
            entity.GetRO<WorldBehaviorSystem>().Do(null, WorldBehaviorSystem.AllBehaviors[TypeId - 1], ArgComponent);
        }
    }
}
