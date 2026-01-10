// =================================================={By Qcbf|qcbf@qq.com|2024-11-05}==================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FLib.Worlds
{
    public abstract class WorldSyncer : BroadcastWorldEvent
    {
        public static WorldSyncCommandExecutor[] AllExecutors = Array.Empty<WorldSyncCommandExecutor>();
        public static byte[] AllComponentTypeIdPermissions = Array.Empty<byte>();

        public WorldSyncUidGenerator UidGen;
        public int LocalIncrementId;
        public byte Permission;
        public byte MinimumPermision = 1;

        public Dictionary<int, WorldEntity> NetIdEntityMap = new();
        public Dictionary<WorldEntity, int> EntityNetIdMap = new();
        // public BytesWriterBuffer Buffer = new(1024);
        public int BufferSize = 0;

        public PauseCounter CommandPreventer;


        public override string ToString() => $"{UidGen.NetId}|{Permission}|{BufferSize}";

        protected WorldSyncer(WorldBase world, ushort netId) : base(world)
        {
            UidGen = new WorldSyncUidGenerator(netId);
        }

        public virtual void Update()
        {
            // if (BufferSize <= 0) return;
            // var buf = Buffer.GetReader(0, BufferSize);
            // BufferSize = 0;
            // SendCommandBuffer(buf);
        }

        /// <summary>
        ///
        /// </summary>
        public abstract void SendCommandBuffer(BytesReader reader);

        /// <summary>
        ///
        /// </summary>
        public virtual void ReceiveCommandBuffer(BytesReader reader)
        {
            CommandPreventer.Pause(nameof(ReceiveCommandBuffer));
            try
            {
                while (reader.Available > 0)
                {
                    var commandTypeId = (byte)reader.ReadVInt();
                    Log.Assert(commandTypeId > 0 && commandTypeId <= AllExecutors.Length)?.Write($"not found command {commandTypeId}");
                    AllExecutors[commandTypeId - 1].Execute(this, ref reader);
                }
            }
            finally
            {
                CommandPreventer.Unpause(nameof(ReceiveCommandBuffer));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void AddCommand<T>(WorldEntity entity, in T command) where T : IWorldSyncCommandable, new()
        {
            if (CommandPreventer.IsPaused || WorldSyncCommandExecutor<T>.Permission < MinimumPermision || Permission < WorldSyncCommandExecutor<T>.Permission)
                return;

            if (!EntityNetIdMap.TryGetValue(entity, out var netId))
            {
                netId = UidGen.Gen();
                EntityNetIdMap.Add(entity, netId);
                NetIdEntityMap.Add(netId, entity);
            }

            // var writer = Buffer.GetWriter(BufferSize);
            // writer.PushVInt(WorldSyncCommandExecutor<T>.TypeId);
            // WorldSyncUidGenerator.BytesWrite(ref writer, netId);
            // BytesPack.Pack(command, ref writer);
            // BufferSize += writer.Length;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsSyncComponent(ushort typeId) => AllComponentTypeIdPermissions[typeId] >= MinimumPermision && Permission >= AllComponentTypeIdPermissions[typeId];

    }
}
