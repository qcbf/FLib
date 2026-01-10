//=================================================={By Qcbf|qcbf@qq.com|11/10/2024 6:54:28 PM}==================================================

using FLib;
using System;
using System.Collections.Generic;


namespace FLib.Worlds
{
    public interface IWorldSyncCommandable : IBytesPackable
    {
        void Execute(WorldSyncer syncer, WorldEntity entity);
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class WorldSyncCommandExecutorAttribute : Attribute
    {
        public Type ExecutorType;
        public WorldSyncCommandExecutorAttribute(Type executorType)
        {
            ExecutorType = executorType;
        }
    }

    public abstract class WorldSyncCommandExecutor
    {
        public abstract void Execute(WorldSyncer syncer, ref BytesReader reader);
    }


    public class WorldSyncCommandExecutor<T> : WorldSyncCommandExecutor where T : IWorldSyncCommandable, new()
    {
        /// <summary>
        /// start with 1
        /// </summary>
        public static byte TypeId;
        /// <summary>
        /// 
        /// </summary>
        public static byte Permission;

        public override void Execute(WorldSyncer syncer, ref BytesReader reader)
        {
            var netId = WorldSyncUidGenerator.BytesRead(ref reader);
            var command = BytesPack.Unpack<T>(ref reader);
            if (!syncer.NetIdEntityMap.TryGetValue(netId, out var entity))
                Log.Warn?.Write($"not found entity from {netId}");
            else if (entity.IsEmpty)
                Log.Error?.Write($"not found entity", entity.ToString());
            else
                command.Execute(syncer, entity);
        }
    }

    public class WorldSyncEntityCommandExecutor : WorldSyncCommandExecutor
    {
        public override void Execute(WorldSyncer syncer, ref BytesReader reader)
        {
            var netId = WorldSyncUidGenerator.BytesRead(ref reader);
            var command = BytesPack.Unpack<WorldSyncEntityCommand>(ref reader);
            if (command.IsDestroy)
            {
                if (!syncer.NetIdEntityMap.Remove(netId, out var entity))
                {
                    Log.Warn?.Write($"not found entity from {netId}");
                    return;
                }
                syncer.EntityNetIdMap.Remove(entity);
                command.Execute(syncer, entity);
            }
            else
            {
                if (!syncer.NetIdEntityMap.TryGetValue(netId, out var entity))
                {
                    entity = syncer.World.EntityMgr.CreateEntityImpl();
                    syncer.NetIdEntityMap.Add(netId, entity);
                    syncer.EntityNetIdMap.TryAdd(entity, netId);
                }
                else
                {
                    syncer.EntityNetIdMap[entity] = netId;
                }
                command.Execute(syncer, entity);
            }
        }
    }
}
