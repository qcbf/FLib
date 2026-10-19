// ==================== qcbf@qq.com |2025-12-28 ====================

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using FLib.WorldCores;

namespace FLib.WorldCores.Components
{
    /// <summary>
    /// 这里会对每个Component类型 首次使用时进行注册, 生成比较的id, size, info等等信息.
    /// 目前是通过反射获取的, 后续会增加warmup接口, 或者直接通过source generator实现在编译时生成,运行时0首次注册开销.
    /// </summary>
    public static class WorldComponentRegistry
    {
        public static readonly Dictionary<Type, WorldComponentMeta> ComponentTypeMap = new(1024);
        public static ushort ComponentCount { get; private set; }

        private static readonly Dictionary<Type, IBytesPackGenericWrapper> SerializationWrapperMap = new();
        private static WorldComponentInfo[] _componentInfos = new WorldComponentInfo[1024];
        private static readonly MethodInfo SizeOfMethod = typeof(Unsafe).GetMethod(nameof(Unsafe.SizeOf));
        private static SpinLock _locker = new(false);

        /// <summary> </summary>
        public static WorldIncrementId GetId<T>()
        {
            return GetMeta<T>().Id;
        }

        /// <summary> </summary>
        public static WorldComponentMeta GetMeta<T>()
        {
            // 考虑直接通过静态构造函数初始化, 避免这次的运行时判断开销是否需要?
            return WorldComponentGenericMap<T>.IsEmpty
                ? WorldComponentGenericMap<T>.Init(Register(typeof(T), SizeOf<T>()))
                : WorldComponentGenericMap<T>.Meta;
        }

        /// <summary> </summary>
        public static WorldIncrementId GetId(Type type)
        {
            return GetMeta(type).Id;
        }

        /// <summary> </summary>
        public static WorldComponentMeta GetMeta(Type type)
        {
            return ComponentTypeMap.TryGetValue(type, out var componentType) ? componentType : Register(type, SizeOf(type));
        }

        /// <summary> </summary>
        public static Type GetType(in WorldIncrementId id)
        {
            return _componentInfos[id].Type;
        }

        /// <summary> </summary>
        public static ref readonly WorldComponentInfo GetInfo<T>()
        {
            return ref _componentInfos[GetMeta<T>().Id];
        }

        /// <summary> </summary>
        public static ref readonly WorldComponentInfo GetInfo(Type type)
        {
            return ref _componentInfos[GetMeta(type).Id];
        }

        /// <summary> </summary>
        public static ref readonly WorldComponentInfo GetInfo(in WorldIncrementId id)
        {
            return ref _componentInfos[id];
        }

        /// <summary> </summary>
        public static WorldComponentMeta Register(Type type, ushort size)
        {
            var locking = false;
            _locker.Enter(ref locking);

            if (!ComponentTypeMap.TryGetValue(type, out var meta))
            {
                var id = new WorldIncrementId(++ComponentCount);
                WorldStaticComponentMask.EnsureCapacity(id);
                meta = new WorldComponentMeta(id, size, type);
                ComponentTypeMap[type] = meta;
                if (_componentInfos.Length <= id)
                    Array.Resize(ref _componentInfos, id + WorldSetting.ComponentTypeCapacityExpandSize);
                _componentInfos[id] = new WorldComponentInfo(meta, type);
            }

            if (locking)
                _locker.Exit(false);
            return meta;
        }

        /// <summary> </summary>
        public static int GetHash(in ReadOnlySpan<ulong> componentTypeMask)
        {
            var hash = new HashCode();
            hash.AddBytes(MemoryMarshal.AsBytes(componentTypeMask));
            return hash.ToHashCode();
        }

        /// <summary> </summary>
        private static ushort SizeOf<T>()
        {
            return (ushort)(typeof(T).IsValueType ? Unsafe.SizeOf<T>() : IntPtr.Size);
        }

        /// <summary> </summary>
        private static ushort SizeOf(Type type)
        {
            return (ushort)(type.IsValueType ? (int)SizeOfMethod.MakeGenericMethod(type).Invoke(null, null)! : IntPtr.Size);
        }

        /// <summary> </summary>
        public static IBytesPackGenericWrapper GetSerializationWrapper(Type type)
        {
            var locking = false;
            _locker.Enter(ref locking);
            try
            {
                if (SerializationWrapperMap.TryGetValue(type, out var wrapper))
                    return wrapper;
                try
                {
                    SerializationWrapperMap.Add(type, wrapper = (IBytesPackGenericWrapper)TypeAssistant.New(typeof(BytesPackGenericWrapper<>).MakeGenericType(type)));
                }
                catch (Exception e)
                {
                    throw new Exception($"nonsupport serialization {TypeAssistant.GetTypeName(type)}", e);
                }

                return wrapper;
            }
            finally
            {
                if (locking)
                    _locker.Exit(false);
            }
        }
    }
}