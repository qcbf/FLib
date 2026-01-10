//=================================================={By Qcbf|qcbf@qq.com|11/10/2024 3:34:13 PM}==================================================

using FLib;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;


namespace FLib.Worlds
{
    [BytesPackGen]
    public partial struct WorldComponentPack
    {
        [BytesPackGenField] public ushort TypeId;
        [BytesPackGenField] public byte[] DataBytes;
        public readonly bool IsEmpty => DataBytes == null;
        public readonly override string ToString() => $"{WorldComponentManager.GetTypeName(TypeId)}|{DataBytes.Length}";

        public static WorldComponentPack Create(WorldBase world, WorldComponentHandle compHandle)
        {
            var writer = BytesWriter.CreateFromPool();
            try
            {
                world.ComponentMgr.ComponentBytesPack(compHandle, ref writer);
                return new WorldComponentPack() { TypeId = compHandle.TypeId, DataBytes = writer.Span.ToArray() };
            }
            finally
            {
                writer.TryReleasePoolAllocator();
            }
        }

        public readonly WorldComponentHandle AddThenUnpackFinish(in WorldEntity entity)
        {
            var world = entity.World;
            var handle = Add(entity, false);
            if (handle.IsEmpty)
                return handle;
            UnpackDataBytes(world, handle);
            world.ComponentMgr.GetGroup(handle.TypeId).AddingFinish(handle.Index);
            return handle;
        }

        public readonly WorldComponentHandle Add(in WorldEntity entity, bool isFinished)
        {
            return IsEmpty ? default : new WorldComponentHandle(TypeId, entity.World.ComponentMgr.GetGroup(TypeId).Add(entity, isFinished));
        }

        public readonly void UnpackDataBytes(in WorldBase world, in WorldComponentHandle handle)
        {
            if (IsEmpty)
                return;
            BytesReader reader = DataBytes;
            var componentMgr = world.ComponentMgr;
            componentMgr.ComponentBytesUnpack(handle, ref reader);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [BytesPackGen]
    public partial class WorldComponentConfigPack : ScriptPack
    {
        [BytesPackGenField, Comment("是否添加到Entity")]
        public bool IsAddToEntity;

        public override Type BaseType => typeof(IWorldComponentable);
        public WorldComponentPack Value => new() { DataBytes = Bytes, TypeId = WorldComponentManager.ComponentTypeIds[Type] };
        public static implicit operator WorldComponentPack(WorldComponentConfigPack pack) => pack.Value;

        /// <summary>
        /// 
        /// </summary>
        public override Type GetScriptType(string typeName)
        {
            var type = TypeAssistant.GetType(WorldInitializer.ComponentConfigPackTypeNamePrefix + typeName, false, false);
            if (type == null)
                Log.Warn?.Write("not found type:" + typeName);
            return type;
        }

        /// <summary>
        /// 
        /// </summary>
        public override string GetScriptTypeName(Type t)
        {
            var typeName = TypeAssistant.GetTypeName(t);
            return string.IsNullOrEmpty(WorldInitializer.ComponentConfigPackTypeNamePrefix) ? typeName : typeName.Replace(WorldInitializer.ComponentConfigPackTypeNamePrefix, "");
        }

        /// <summary>
        /// 
        /// </summary>
        public static void AddTo(in WorldEntity entity, in WorldComponentConfigPack[] cfgComponents)
        {
            if (cfgComponents?.Length > 0)
            {
                foreach (var comp in cfgComponents)
                    entity.Add(comp.Value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void AddTo(in WorldEntity entity, in WorldComponentConfigPack cfgComp, ref WorldComponentHandle component)
        {
            if (cfgComp.IsAddToEntity)
                entity.Add(cfgComp.Value);
            else
                component = cfgComp.Value.AddThenUnpackFinish(entity);
        }

        /// <summary>
        /// 
        /// </summary>
        public static void AddTo(in WorldEntity entity, in WorldComponentConfigPack[] cfgComponents, ref PooledList<WorldComponentHandle> components)
        {
            if (cfgComponents?.Length > 0)
            {
                components.Allocate(cfgComponents.Length);
                foreach (var comp in cfgComponents)
                {
                    if (comp.IsAddToEntity)
                        entity.Add(comp.Value);
                    else
                        components.Add(comp.Value.AddThenUnpackFinish(entity));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void RemoveFrom(in WorldEntity entity, ref PooledList<WorldComponentHandle> components)
        {
            foreach (var comp in components)
                comp.OnlyRemoveThisComponent(entity);
        }

        /// <summary>
        /// 
        /// </summary>
        public static void RemoveFrom(in WorldEntity entity, ref WorldComponentHandle comp)
        {
            comp.OnlyRemoveThisComponent(entity);
            comp = default;
        }
    }
}
