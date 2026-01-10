// =================================================={By Qcbf|qcbf@qq.com|2024-10-23}==================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FLib.Worlds
{
    public class WorldEntityManager : IBytesPackable
    {
        public readonly WorldBase World;
        public Stack<ushort> FreeEntities = new(128);
        public WorldEntity[] AllEntities = new WorldEntity[1024];
        public PauseCounter[] AllEntityPausers = new PauseCounter[1024];
        public EntityWorldEvent[] AllEntityEvents = new EntityWorldEvent[1024];
        public int[] EntityComponents = new int[1024 * WorldComponentManager.ComponentCount];
        public WorldComponentHandleLinkList AllComponents;

        private ushort _entityVersionGen;
        public ushort EntityCount { get; private set; }

        public WorldEntityManager(WorldBase world)
        {
            World = world;
            AllComponents.SetCapacity(4096);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual WorldEntityIterator GetEntities()
        {
            return new WorldEntityIterator(AllEntities, EntityCount);
        }

        /// <summary>
        ///
        /// </summary>
        public virtual WorldEntity CreateEntity()
        {
            var entity = CreateEntityImpl();
            World.Syncer?.AddCommand(entity, new WorldSyncEntityCommand());
            return entity;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void RemoveAllEntity()
        {
            foreach (var entity in GetEntities())
                RemoveEntity(entity);
            Log.Assert(EntityCount == 0);
        }

        /// <summary>
        ///
        /// </summary>
        public virtual bool RemoveEntity(WorldEntity entity)
        {
            if (entity.IsEmpty)
                return false;
            World.Syncer?.AddCommand(entity, new WorldSyncEntityCommand(true));
            RemoveEntityImpl(entity);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual ref int GetEntityComponentFirstPos(in WorldEntity entity, in ushort typeId)
        {
            return ref EntityComponents[entity.Index * WorldComponentManager.ComponentCount + typeId - 1];
        }

        /// <summary>
        ///
        /// </summary>
        protected internal virtual WorldEntity CreateEntityImpl()
        {
            ++EntityCount;
            if (!FreeEntities.TryPop(out var entityIndex))
            {
                entityIndex = (ushort)(EntityCount - 1);
                if (EntityCount >= AllEntities.Length)
                {
                    var count = Math.Min(ushort.MaxValue, MathEx.GetNextPowerOfTwo(EntityCount));
                    Array.Resize(ref AllEntities, count);
                    Array.Resize(ref EntityComponents, count * WorldComponentManager.ComponentCount);
                    Array.Resize(ref AllEntityEvents, count);
                    Array.Resize(ref AllEntityPausers, count);
                }
            }

            ++_entityVersionGen;
            if (_entityVersionGen == 0)
                ++_entityVersionGen;

            var entity = new WorldEntity(World.Handle, entityIndex, _entityVersionGen);
            AllEntities[entityIndex] = entity;
            if (AllEntityEvents[entityIndex] != null)
                AllEntityEvents[entityIndex].Entity = entity;
            World.DispatchEvent(new WorldEntityLifeEvent(entity, false));
            return entity;
        }

        /// <summary>
        ///
        /// </summary>
        protected internal virtual void RemoveEntityImpl(WorldEntity entity)
        {
            --EntityCount;
            var evt = AllEntityEvents[entity.Index];
            if (evt != null)
            {
                evt.ClearListenEvents();
                evt.Entity = default;
            }
            World.DispatchEvent(new WorldEntityLifeEvent(entity, true));
            AllEntityPausers[entity.Index] = default;
            World.EntityMgr.AllEntities[entity.Index] = default;
            World.EntityMgr.FreeEntities.Push(entity.Index);

            var startIndex = entity.Index * WorldComponentManager.ComponentCount;
            for (ushort i = 0; i < WorldComponentManager.ComponentCount; i++)
            {
                var group = World.ComponentMgr.GetGroup((ushort)(i + 1));
                ref var entityCompPos = ref World.EntityMgr.EntityComponents[startIndex + i];
                if (entityCompPos <= 0) continue;
                var pos = entityCompPos;
                entityCompPos = 0;
                do
                {
                    var next = World.EntityMgr.AllComponents[pos - 1].NextPosition;
                    group.Remove(new WorldComponentContext(entity, World.EntityMgr.AllComponents[pos - 1].Handle));
                    World.EntityMgr.AllComponents.FreeNode(pos);
                    pos = next;
                } while (pos > 0);
            }
        }

        /// <summary>
        ///
        /// </summary>
        protected internal virtual void LinkEntityComponentImpl(in WorldEntity entity, in WorldComponentHandle compHandle)
        {
            var pos = AllComponents.Create(compHandle);
            ref var firstCompPos = ref GetEntityComponentFirstPos(entity, compHandle.TypeId);
            if (firstCompPos > 0)
            {
                AllComponents[pos - 1].LinkedId = AllComponents[firstCompPos - 1].LinkedId + 1;
                AllComponents[firstCompPos - 1].PrevPosition = pos;
                AllComponents[pos - 1].NextPosition = firstCompPos;
            }
            else
            {
                AllComponents[pos - 1].LinkedId = 1;
            }
            firstCompPos = pos;
        }

        /// <summary>
        ///
        /// </summary>
        protected internal virtual bool UnlinkEntityComponentImpl(in WorldEntity entity, in WorldComponentHandle compHandle)
        {
            if (entity.IsEmpty)
                return false;

            ref var firstCompPos = ref GetEntityComponentFirstPos(entity, compHandle.TypeId);
            if (firstCompPos == 0)
                return false;

            var compPos = firstCompPos;
            if (AllComponents[compPos - 1].Handle == compHandle)
            {
                firstCompPos = AllComponents[compPos - 1].NextPosition;
            }
            else
            {
                // find match component
                ref var compNode = ref AllComponents[compPos - 1];
                do
                {
                    if (compNode.NextPosition == 0)
                        return false;
                    compPos = compNode.NextPosition;
                    compNode = ref AllComponents[compPos - 1];
                } while (compNode.Handle != compHandle);

                // remove link list
                AllComponents[compNode.PrevPosition - 1].NextPosition = compNode.NextPosition;
                if (compNode.NextPosition > 0)
                    AllComponents[compNode.NextPosition - 1].PrevPosition = compNode.PrevPosition;
            }

            AllComponents.FreeNode(compPos);
            return true;
        }

        public void Z_BytesPackWrite(ref BytesPack.KeyHelper key, ref BytesWriter writer)
        {
            key.Push(ref writer, 1);
            writer.PushLength(EntityCount);
            foreach (var entity in GetEntities())
                BytesPack.Pack(entity, ref writer);
        }

        public void Z_BytesPackRead(int key, ref BytesReader reader)
        {
            if (key != 1)
                return;
            World.Syncer?.CommandPreventer.Pause(nameof(Z_BytesPackRead));
            try
            {
                RemoveAllEntity();
                var count = reader.ReadLength();
                for (var i = 0; i < count; i++)
                {
                    var entity = CreateEntity();
                    BytesPack.Unpack(ref entity, ref reader);
                }
            }
            finally
            {
                World.Syncer?.CommandPreventer.Unpause(nameof(Z_BytesPackRead));
            }
        }
    }
}
