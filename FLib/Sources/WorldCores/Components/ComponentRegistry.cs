// ==================== qcbf@qq.com |2025-12-28 ====================

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FLib.WorldCores
{
    public static class ComponentRegistry
    {
        [ThreadStatic] private static ulong[] _componentTypeMaskBuffer;
        public static ulong[] ComponentTypeMaskBuffer => _componentTypeMaskBuffer ??= new ulong[4];

        public static readonly Dictionary<Type, ComponentMeta> ComponentTypeMap = new(1024);
        public static ushort ComponentCount { get; private set; }
        private static ComponentInfo[] _componentInfos = new ComponentInfo[1024];
        private static readonly MethodInfo SizeOfMethod = typeof(Unsafe).GetMethod(nameof(Unsafe.SizeOf));

        /// <summary>
        /// 
        /// </summary>
        public static IncrementId GetId<T>()
        {
            return GetMeta<T>().Id;
        }

        /// <summary>
        /// 
        /// </summary>
        public static ComponentMeta GetMeta<T>()
        {
            return ComponentGenericMap<T>.IsEmpty ? ComponentGenericMap<T>.Meta = Register(typeof(T), SizeOf<T>()) : ComponentGenericMap<T>.Meta;
        }

        /// <summary>
        /// 
        /// </summary>
        public static IncrementId GetId(Type type)
        {
            return GetMeta(type).Id;
        }

        /// <summary>
        /// 
        /// </summary>
        public static ComponentMeta GetMeta(Type type)
        {
            return ComponentTypeMap.TryGetValue(type, out var componentType) ? componentType : Register(type, SizeOf(type));
        }

        /// <summary>
        /// 
        /// </summary>
        public static Type GetType(in ComponentMeta meta)
        {
            return _componentInfos[meta.Id].Type;
        }

        /// <summary>
        /// 
        /// </summary>
        public static ref readonly ComponentInfo GetInfo(in ComponentMeta meta)
        {
            return ref _componentInfos[meta.Id];
        }

        /// <summary>
        /// 
        /// </summary>
        public static ComponentMeta Register(Type type, ushort size)
        {
            var id = new IncrementId(++ComponentCount);
            var cType = new ComponentMeta(id, size);
            ComponentTypeMap[type] = cType;
            
            if (_componentInfos.Length <= id)
                Array.Resize(ref _componentInfos, id + GlobalSetting.CapacityExpandSize);
            _componentInfos[id] = new ComponentInfo(type);
            
            var maxBit = (int)Math.Ceiling(id.Raw / (float)BitArrayOperator.BitSize);
            if (ComponentTypeMaskBuffer.Length < maxBit)
                _componentTypeMaskBuffer = new ulong[MathEx.GetNextPowerOfTwo(maxBit)];
            return cType;
        }

        /// <summary>
        /// 
        /// </summary>
        public static int GetHash(in ReadOnlySpan<ulong> componentTypeMask)
        {
            var hash = new HashCode();
            hash.AddBytes(MemoryMarshal.AsBytes(componentTypeMask));
            return hash.ToHashCode();
        }

        /// <summary>
        /// 
        /// </summary>
        private static ushort SizeOf<T>()
        {
            return (ushort)(typeof(T).IsValueType ? Unsafe.SizeOf<T>() : IntPtr.Size);
        }

        /// <summary>
        /// 
        /// </summary>
        private static ushort SizeOf(Type type)
        {
            return (ushort)(type.IsValueType ? (int)SizeOfMethod.MakeGenericMethod(type).Invoke(null, null)! : IntPtr.Size);
        }
    }
}