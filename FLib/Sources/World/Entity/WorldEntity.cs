// =================================================={By Qcbf|qcbf@qq.com|2024-10-22}==================================================

#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FLib.Worlds
{
    public readonly struct WorldEntity : IEquatable<WorldEntity>, IComparable<WorldEntity>, IBytesPackable
    {
        public readonly WorldHandle WorldHandle;
        public readonly uint Id;

        public ushort Index
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (ushort)Id;
        }

        public ushort Version
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (ushort)(Id >> 16);
        }

        public WorldBase World
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => WorldHandle.Val;
        }

        public WorldBase? WorldOrNull
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => WorldHandle.IsEmpty ? null : WorldHandle.Val;
        }

        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Id == 0 || WorldHandle.IsEmpty || World.EntityMgr.AllEntities[Index].Version != Version;
        }

        public EntityWorldEvent Event
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => World.EntityMgr.AllEntityEvents[Index] ??= new EntityWorldEvent(this);
        }

        public EntityWorldEvent? EventDispatcher
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => World.EntityMgr.AllEntityEvents[Index];
        }

        public ref PauseCounter Pauser
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref World.EntityMgr.AllEntityPausers[Index];
        }

        public WorldEntity(WorldHandle world, uint id)
        {
            WorldHandle = world;
            Id = id;
        }

        public WorldEntity(WorldHandle world, ushort index, ushort version)
        {
            WorldHandle = world;
            Id = (uint)version << 16 | index;
        }

        /// <summary>
        ///
        /// </summary>
        public void Destroy()
        {
            World.EntityMgr.RemoveEntity(this);
        }

        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exist<T>() where T : IWorldComponentable, new() => Exist(WorldComponentGroup<T>.TypeId);

        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exist(ushort typeId) => !IsEmpty && World.EntityMgr.GetEntityComponentFirstPos(this, typeId) != 0;

        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WorldComponentHandleEx<T> GetHandle<T>(ELogLevel logLevel = ELogLevel.Fatal) where T : IWorldComponentable, new()
        {
            var handle = GetHandle(WorldComponentGroup<T>.TypeId, logLevel);
            return new WorldComponentHandleEx<T>(World, handle);
        }

        /// <summary>
        /// 
        /// </summary>
        public WorldComponentHandleEx GetHandle(ushort typeId, ELogLevel logLevel = ELogLevel.Fatal)
        {
            if (IsEmpty)
            {
                Log.Get(logLevel)?.Write($"{Id} is empty not found component {WorldComponentManager.GetTypeName(typeId)}");
                return default;
            }

            var firstCompPos = World.EntityMgr.GetEntityComponentFirstPos(this, typeId);
            if (firstCompPos != 0) return World.EntityMgr.AllComponents[firstCompPos - 1].Handle.Cast(World);
            Log.Get(logLevel)?.Write($"{Id} not found component {WorldComponentManager.GetTypeName(typeId)}");
            return default;
        }

        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetOrAdd<T>() where T : IWorldComponentable, new()
        {
            if (!Exist<T>())
                return ref Add(WorldComponentGroup<T>.TypeId).RW<T>(World);
            return ref GetRW<T>();
        }

        /// <summary>
        ///
        /// </summary>
        public ref T GetRW<T>(ELogLevel logLevel = ELogLevel.Fatal) where T : IWorldComponentable, new()
        {
            if (IsEmpty)
            {
                Log.Get(logLevel)?.Write($"{Id} is empty not found component {WorldComponentManager.GetTypeName(typeof(T))}");
                return ref WorldComponentGroup<T>.Default;
            }

            var firstCompPos = World.EntityMgr.GetEntityComponentFirstPos(this, WorldComponentGroup<T>.TypeId);
            if (firstCompPos != 0) return ref World.EntityMgr.AllComponents[firstCompPos - 1].Handle.RW<T>(World);
            Log.Get(logLevel)?.Write($"{Id} not found component {WorldComponentManager.GetTypeName(typeof(T))}");
            return ref WorldComponentGroup<T>.Default;
        }

        /// <summary>
        ///
        /// </summary>
        public ref readonly T GetRO<T>(ELogLevel logLevel = ELogLevel.Fatal) where T : IWorldComponentable, new()
        {
            if (WorldHandle.IsEmpty) // 这里特殊一点，因为移除entity时需要再获取某些组件（ViewComp获取LogicComp数据），而且这里是只读的所以开放一些不用限制太多
            {
                Log.Get(logLevel)?.Write($"{Id} is empty not found component {WorldComponentManager.GetTypeName(typeof(T))}");
                return ref WorldComponentGroup<T>.Default;
            }
            var firstCompPos = World.EntityMgr.GetEntityComponentFirstPos(this, WorldComponentGroup<T>.TypeId);
            if (firstCompPos != 0) return ref World.EntityMgr.AllComponents[firstCompPos - 1].Handle.RO<T>(WorldHandle.Val);
            Log.Get(logLevel)?.Write($"{Id} not found component {WorldComponentManager.GetTypeName(typeof(T))}");
            return ref WorldComponentGroup<T>.Default;
        }

        /// <summary>
        ///
        /// </summary>
        public WorldComponentHandleLinkList.Iterator<T> GetAll<T>() where T : IWorldComponentable, new()
        {
            if (IsEmpty)
                return default;
            var firstCompPos = World.EntityMgr.GetEntityComponentFirstPos(this, WorldComponentGroup<T>.TypeId);
            return firstCompPos == 0 ? WorldComponentHandleLinkList.Iterator<T>.Empty : World.EntityMgr.AllComponents.GetIterator<T>(World, firstCompPos);
        }

        /// <summary>
        ///
        /// </summary>
        public WorldComponentHandleLinkList.Iterator GetAll(ushort compTypeId)
        {
            return IsEmpty ? WorldComponentHandleLinkList.Iterator.Empty : World.EntityMgr.AllComponents.GetIterator(World, World.EntityMgr.GetEntityComponentFirstPos(this, compTypeId));
        }

        /// <summary>
        ///
        /// </summary>
        public WorldComponentHandle Set(in WorldComponentPack pack)
        {
            if (IsEmpty) throw new NullReferenceException(ToString());
            if (World.Syncer?.IsSyncComponent(pack.TypeId) == true)
                World.Syncer.AddCommand(this, new WorldSyncComponentSetCommand() { Comp = pack });
            var entityMgr = World.EntityMgr;
            var group = World.ComponentMgr.GetGroup(pack.TypeId);
            var firstCompPos = entityMgr.EntityComponents[Index * WorldComponentManager.ComponentCount + pack.TypeId];
            if (firstCompPos > 0)
            {
                var oldCompHandle = entityMgr.AllComponents[firstCompPos - 1].Handle;
                group.Remove(new WorldComponentContext(this, oldCompHandle));
                entityMgr.UnlinkEntityComponentImpl(this, oldCompHandle);
                // // DispatchEvent(new WorldRemoveComponentEvent(oldCompHandle.ToExHandle(World)));
            }

            var handle = pack.Add(this, false);
            entityMgr.LinkEntityComponentImpl(this, handle);
            group.AddingFinish(handle.Index);
            pack.UnpackDataBytes(World, handle);
            // DispatchEvent(new WorldAddComponentEvent(handle.ToExHandle(World)));
            return handle;
        }

        /// <summary>
        ///
        /// </summary>
        public WorldComponentHandleEx<T> Set<T>(in T comp) where T : IWorldComponentable, new()
        {
            if (IsEmpty) throw new NullReferenceException(ToString());
            var group = World.ComponentMgr.GetGroup<T>();
            if (Exist<T>())
            {
                var oldCompHandle = GetRO<T>().SelfContext.CompHandle;
                group.Remove(new WorldComponentContext(this, oldCompHandle));
                World.EntityMgr.UnlinkEntityComponentImpl(this, oldCompHandle);
                // DispatchEvent(new WorldRemoveComponentEvent(oldCompHandle.ToExHandle(World)));
            }

            var handle = CreateComponent(group, in comp);
            group.AddingFinish(handle.Index);
            if (World.Syncer?.IsSyncComponent(handle.TypeId) == true)
                World.Syncer.AddCommand(this, new WorldSyncComponentSetCommand() { Comp = WorldComponentPack.Create(World, handle) });
            return handle.Cast<T>(World);
        }

        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WorldEntity AddWith<T>(in T comp) where T : IWorldComponentable, new()
        {
            Add(comp);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WorldEntity AddWith<T>() where T : IWorldComponentable, new()
        {
            Add<T>();
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WorldComponentHandleEx<T> Add<T>() where T : IWorldComponentable, new() => Add(WorldComponentGroup<T>.TypeId).Cast<T>(World);

        /// <summary>
        /// 
        /// </summary>
        public WorldComponentHandle Add(ushort typeId)
        {
            if (IsEmpty) throw new NullReferenceException(ToString());
            var group = World.ComponentMgr.GetGroup(typeId);
            var handle = CreateComponent(group, typeId);
            group.AddingFinish(handle.Index);
            if (World.Syncer?.IsSyncComponent(handle.TypeId) == true)
                World.Syncer.AddCommand(this, new WorldSyncComponentAddCommand() { Comp = WorldComponentPack.Create(World, handle) });
            return handle;
        }

        /// <summary>
        ///
        /// </summary>
        public WorldComponentHandle Add(in IWorldComponentable comp)
        {
            if (IsEmpty) throw new NullReferenceException(ToString());
            var typeId = WorldComponentManager.ComponentTypeIds[comp.GetType()];
            var group = World.ComponentMgr.GetGroup(typeId);
#if DEBUG
            if (!group.Option(EWorldComponentOption.MultipleInstance) && Exist(typeId))
                throw new Exception($"{WorldComponentManager.GetTypeName(typeId)} is solo");
#endif
            var handle = new WorldComponentHandle(typeId, group.Add(this, comp, false));
            World.EntityMgr.LinkEntityComponentImpl(this, handle);
            group.AddingFinish(handle.Index);
            return handle;
        }

        /// <summary>
        ///
        /// </summary>
        public WorldComponentHandleEx<T> Add<T>(in T comp) where T : IWorldComponentable, new()
        {
            if (IsEmpty) throw new NullReferenceException(ToString());
            var group = World.ComponentMgr.GetGroup<T>();
            var handle = CreateComponent(group, in comp);
            group.AddingFinish(handle.Index);
            if (World.Syncer?.IsSyncComponent(handle.TypeId) == true)
                World.Syncer.AddCommand(this, new WorldSyncComponentAddCommand() { Comp = WorldComponentPack.Create(World, handle) });
            return handle.Cast<T>(World);
        }

        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal WorldComponentHandle CreateComponent(WorldComponentGroup group, ushort typeId)
        {
#if DEBUG
            if (!group.Option(EWorldComponentOption.MultipleInstance) && Exist(typeId))
                throw new Exception($"{WorldComponentManager.GetTypeName(typeId)} is solo");
#endif
            var handle = new WorldComponentHandle(typeId, group.Add(this, false));
            World.EntityMgr.LinkEntityComponentImpl(this, handle);
            return handle;
        }

        /// <summary>
        /// 
        /// </summary>
        internal WorldComponentHandle CreateComponent<T>(WorldComponentGroup group, in T comp) where T : IWorldComponentable, new()
        {
#if DEBUG
            if (!group.Option(EWorldComponentOption.MultipleInstance) && Exist<T>())
                throw new Exception($"{WorldComponentManager.GetTypeName(typeof(T))} is solo");
#endif
            var handle = new WorldComponentHandle(WorldComponentGroup<T>.TypeId, group.Add(this, comp, false));
            World.EntityMgr.LinkEntityComponentImpl(this, handle);
            return handle;
        }

        /// <summary>
        /// 
        /// </summary>
        public WorldComponentHandle Add(in WorldComponentPack pack, bool isFirstUnpackData = true)
        {
            if (IsEmpty) throw new NullReferenceException(ToString());
            var group = World.ComponentMgr.GetGroup(pack.TypeId);
#if DEBUG
            if (!group.Option(EWorldComponentOption.MultipleInstance) && Exist(pack.TypeId))
                throw new Exception($"{WorldComponentManager.GetTypeName(pack.TypeId)} is solo");
#endif
            var handle = pack.Add(this, false);
            var entityMgr = World.EntityMgr;
            entityMgr.LinkEntityComponentImpl(this, handle);
            if (isFirstUnpackData)
            {
                pack.UnpackDataBytes(World, handle);
                group.AddingFinish(handle.Index);
            }
            else
            {
                group.AddingFinish(handle.Index);
                pack.UnpackDataBytes(World, handle);
            }
            if (World.Syncer?.IsSyncComponent(handle.TypeId) == true)
                World.Syncer.AddCommand(this, new WorldSyncComponentAddCommand() { Comp = pack });
            // DispatchEvent(new WorldAddComponentEvent(handle.ToExHandle(World)));
            return handle;
        }

        /// <summary>
        ///
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove<T>() where T : IWorldComponentable, new()
        {
            if (IsEmpty)
                return false;
            var comps = GetAll<T>();
            return comps.MoveNext() && Remove(comps.Current);
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Remove(ushort typeId, int linkedId = -1)
        {
            var comps = GetAll(typeId);
            while (comps.MoveNext())
            {
                if (linkedId > 0 && comps.CurrentNode.LinkedId != linkedId) continue;
                if (World.Syncer?.IsSyncComponent(typeId) == true)
                    World.Syncer.AddCommand(this, new WorldSyncComponentRemoveCommand() { TypeId = typeId, LinkedId = linkedId });
                return Remove(comps.Current);
            }
            return false;
        }

        /// <summary>
        ///
        /// </summary>
        public bool Remove(WorldComponentHandle handle)
        {
            if (IsEmpty)
                return false;
            var entityMgr = World.EntityMgr;
            if (World.Syncer?.IsSyncComponent(handle.TypeId) == true)
            {
                var firstPos = entityMgr.GetEntityComponentFirstPos(this, handle.TypeId);
                var linkedId = 0;
                ref var firstNode = ref entityMgr.AllComponents[firstPos - 1];
                if (firstNode.Handle == handle)
                {
                    linkedId = firstNode.LinkedId;
                }
                else
                {
                    var iterator = entityMgr.AllComponents.GetIterator(World, firstNode.NextPosition);
                    while (iterator.MoveNext())
                    {
                        ref var node = ref iterator.CurrentNode;
                        if (node.Handle != handle) continue;
                        linkedId = node.LinkedId;
                        break;
                    }
                }

                World.Syncer.AddCommand(this, new WorldSyncComponentRemoveCommand() { TypeId = handle.TypeId, LinkedId = linkedId });
            }

            entityMgr.UnlinkEntityComponentImpl(this, handle);
            World.ComponentMgr.GetGroup(handle.TypeId).Remove(new WorldComponentContext(this, handle));
            // DispatchEvent(new WorldRemoveComponentEvent(handle.ToExHandle(World)));
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort RemoveAll<T>() where T : IWorldComponentable, new() => RemoveAll(WorldComponentGroup<T>.TypeId);

        /// <summary>
        ///
        /// </summary>
        public ushort RemoveAll(ushort typeId = 0)
        {
            if (IsEmpty)
                return 0;
            ushort count = 0;
            if (typeId == 0)
            {
                for (ushort i = 0; i < WorldComponentManager.ComponentCount; i++)
                    count += RemoveAllImpl(i);
            }
            else
            {
                count = RemoveAllImpl(typeId);
            }

            if (count > 0 && World.Syncer?.IsSyncComponent(typeId) == true)
                World.Syncer?.AddCommand(this, new WorldSyncComponentRemoveCommand() { TypeId = typeId, LinkedId = -1 });
            return count;
        }

        internal ushort RemoveAllImpl(ushort typeId)
        {
            ushort count = 0;
            foreach (var item in GetAll(typeId))
            {
                ++count;
                Remove(item);
            }

            return count;
        }

        #region Key Component
        // public T GetRO<T>(int key, ELogLevel logLevel = ELogLevel.Fatal) where T : IWorldComponentable, new() => GetHandle(key, logLevel).RO<T>();
        // public T GetRW<T>(int key, ELogLevel logLevel = ELogLevel.Fatal) where T : IWorldComponentable, new() => GetHandle(key, logLevel).RW<T>();
        //
        // /// <summary>
        // /// 
        // /// </summary>
        // public WorldComponentHandleEx GetHandle(int key, ELogLevel logLevel = ELogLevel.Fatal)
        // {
        //     if (World == null) // 这里特殊一点，因为移除entity时需要再获取某些组件（ViewComp获取LogicComp数据），而且这里是只读的所以开放一些不用限制太多
        //     {
        //         Log.Get(logLevel)?.Write($"{Id} is empty not found component {key}", null);
        //         return default;
        //     }
        //     if (World.EntityMgr.AllEntityExtraDatas[Index].KeyComponents?.TryGetValue(key, out var handle) == true)
        //         return handle.ToExHandle(World);
        //     Log.Get(logLevel)?.Write($"{Id} is empty not found component {key}", null);
        //     return default;
        // }
        //
        // /// <summary>
        // /// 
        // /// </summary>
        // public WorldComponentHandleEx<T> Set<T>(int key, in T comp) where T : IWorldComponentable, new()
        // {
        //     if (IsEmpty) throw new NullReferenceException(ToString());
        //     var dict = World.EntityMgr.AllEntityExtraDatas[Index].KeyComponents ??= new Dictionary<int, WorldComponentHandle>();
        //     if (dict.TryGetValue(key, out var oldHandle))
        //         Remove(oldHandle);
        //     var result = Add(comp);
        //     dict[key] = result;
        //     return result;
        // }
        //
        // /// <summary>
        // /// 
        // /// </summary>
        // public bool Remove(int key)
        // {
        //     if (World == null)
        //         return false;
        //     if (World.EntityMgr.AllEntityExtraDatas[Index].KeyComponents?.Remove(key, out var handle) == true)
        //     {
        //         Remove(handle);
        //         return true;
        //     }
        //     return false;
        // }
        //
        // /// <summary>
        // /// 
        // /// </summary>
        // public bool Exist(int key)
        // {
        //     return World?.EntityMgr.AllEntityExtraDatas[Index].KeyComponents?.ContainsKey(key) == true;
        // }
        #endregion

        #region misc
        void IBytesPackable.Z_BytesPackWrite(ref BytesPack.KeyHelper key, ref BytesWriter writer)
        {
            key.Push(ref writer, 1);
            for (ushort i = 0; i < WorldComponentManager.ComponentCount; i++)
            {
                var comps = GetAll((ushort)(i + 1));
                writer.PushLength(comps.Count());
                while (comps.MoveNext())
                    BytesPack.Pack(WorldComponentPack.Create(World, comps.Current), ref writer);
            }
        }

        void IBytesPackable.Z_BytesPackRead(int key, ref BytesReader reader)
        {
            if (key != 1)
                return;
            for (ushort i = 0; i < WorldComponentManager.ComponentCount; i++)
            {
                var count = reader.ReadLength();
                for (ushort j = 0; j < count; j++)
                {
                    var compPack = BytesPack.Unpack<WorldComponentPack>(ref reader);
                    Add(compPack, false);
                }
            }
        }

        public override string ToString() => Id.ToString();

        public string ToString(bool isVerbose)
        {
            var strbuf = StringFLibUtility.GetStrBuf();
            strbuf.Append(Id).Append('|');
            for (ushort i = 0; i < WorldComponentManager.ComponentCount; i++)
            {
                var typeId = (ushort)(i + 1);
                var iterator = GetAll(typeId);
                var compsCount = iterator.Count();
                if (compsCount > 0)
                {
                    if (compsCount > 1)
                        strbuf.Append('*').Append(compsCount);
                    if (isVerbose)
                    {
                        strbuf.Append('[');
                        while (iterator.MoveNext())
                        {
                            strbuf.Append('"').Append(iterator.Current.ToString(true)).Append('"');
                            strbuf.Append(',');
                        }
                        if (strbuf[^1] == ',')
                            strbuf.Remove(strbuf.Length - 1, 1);
                        strbuf.Append(']');
                    }
                    else
                    {
                        strbuf.Append(WorldComponentManager.GetTypeName(typeId));
                    }
                    strbuf.Append(',');
                }
            }
            if (strbuf[^1] == ',')
                strbuf.Remove(strbuf.Length - 1, 1);
            return StringFLibUtility.ReleaseStrBufAndResult(strbuf);
        }

        public bool Equals(WorldEntity other) => Id == other.Id;
        public int CompareTo(WorldEntity other) => Id.CompareTo(other.Id);
        public override bool Equals(object? obj) => obj is WorldEntity other && Equals(other);
        public override int GetHashCode() => (int)Id;

        public static bool operator ==(in WorldEntity a, in WorldEntity b) => a.Id == b.Id;
        public static bool operator !=(in WorldEntity a, in WorldEntity b) => a.Id != b.Id;
        public static implicit operator uint(in WorldEntity a) => a.Id;
        public static implicit operator WorldBase(in WorldEntity a) => a.World;
        #endregion
    }
}
