// =================================================={By Qcbf|qcbf@qq.com|2024-10-22}==================================================

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace FLib.Worlds
{
    public class WorldComponentManager
    {
        public static ushort ComponentCount;
        public static Type[] ComponentTypes = Array.Empty<Type>();
        public static Type[] ComponentGroupTypes = Array.Empty<Type>();
        public static ReadOnlyDictionary<string, ushort> ComponentNameIds;
        public static ReadOnlyDictionary<Type, ushort> ComponentTypeIds;
        public static IWorldComponentSerializable[] SerializableComponents;
        internal static WorldComponentProcessor.UpdateProcessDelegate[] UpdateComponentGroupIds;
        internal static WorldComponentProcessor.UpdateProcessDelegate[] LateUpdateComponentGroupIds;

        public readonly WorldBase World;
        public readonly WorldComponentGroup[] Groups = new WorldComponentGroup[ComponentCount];

        public WorldComponentManager(WorldBase world)
        {
            World = world;
        }

        /// <summary>
        ///
        /// </summary>
        public void Update()
        {
            foreach (var item in UpdateComponentGroupIds)
                item(World);
            foreach (var item in LateUpdateComponentGroupIds)
                item(World);
        }

        /// <summary>
        /// 
        /// </summary>
        public void ClearAll()
        {
            foreach (var item in Groups)
                item.RemoveAll();
        }

        /// <summary>
        ///
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public WorldComponentGroup<T> GetGroup<T>() where T : IWorldComponentable, new()
            => (WorldComponentGroup<T>)GetGroup(WorldComponentGroup<T>.TypeId);

        /// <summary>
        ///
        /// </summary>
        public WorldComponentGroup GetGroup(ushort typeId)
        {
            Log.Assert(typeId > 0)?.Write("typeid error");
            --typeId;
            return Groups[typeId] ??= (WorldComponentGroup)TypeAssistant.New(ComponentGroupTypes[typeId], World);
        }

        /// <summary>
        /// 
        /// </summary>
        public bool ComponentBytesPack(in WorldComponentHandle handle, ref BytesWriter writer, in BytesPackPushHookHandler hook = default)
        {
            Log.Assert(handle.TypeId > 0)?.Write("typeid error");
            return (SerializableComponents[handle.TypeId - 1] ?? throw new NotSupportedException($"{ComponentTypes[handle.TypeId - 1]} serialization")).BytesPack(World, handle.Index, ref writer, hook);
        }

        /// <summary>
        /// 
        /// </summary>
        public void ComponentBytesUnpack(in WorldComponentHandle handle, ref BytesReader reader, in DebugOnlyString errorAdditionText = default)
        {
            Log.Assert(handle.TypeId > 0)?.Write("typeid error");
            (SerializableComponents[handle.TypeId - 1] ?? throw new NotSupportedException($"{ComponentTypes[handle.TypeId - 1]} serialization")).BytesUnpack(World, handle.Index, ref reader, errorAdditionText);
        }

        /// <summary>
        ///
        /// </summary>
        public static string GetTypeName(ushort typeId)
        {
            Log.Assert(typeId > 0)?.Write("typeid error");
            return GetTypeName(ComponentTypes[typeId - 1]);
        }

        /// <summary>
        ///
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetTypeName(Type t)
        {
            var tName = t.Name;
            var strbuf = StringFLibUtility.GetStrBuf(tName.Length);
            strbuf.Append(tName);
            var parentType = t.DeclaringType;
            while (parentType != null)
            {
                tName = parentType.Name;
                strbuf.EnsureCapacity(strbuf.Length + tName.Length);
                strbuf.Insert(0, '.').Insert(0, tName);
                parentType = parentType.DeclaringType;
            }
            return StringFLibUtility.ReleaseStrBufAndResult(strbuf);
        }
    }
}
