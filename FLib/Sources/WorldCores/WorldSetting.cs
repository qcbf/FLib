// ==================== qcbf@qq.com |2026-01-01 ====================

using System;
using System.Runtime.InteropServices;
using FLib.WorldCores.Behaviors;
using FLib.WorldCores.Effects;
using FLib.WorldCores.Entities;

namespace FLib.WorldCores
{
    // ReSharper disable ConvertToConstant.Global
#pragma warning disable CA2211
    public static class WorldSetting
    {
        /// <summary>
        /// 帧率
        /// </summary>
        public static byte FrameRate = 30;

        /// <summary>
        /// 每帧时间
        /// </summary>
        public static FNum DeltaTime = FNum.One / FrameRate;

        /// <summary>
        /// 根据id创建效果
        /// </summary>
        public static EffectHandlerDelegate CreateEffectHandler = null;

        /// <summary>
        /// 销毁效果
        /// </summary>
        public static DestroyHandlerDelegate DestroyEffectHandler = null;

        /// <summary>
        /// 默认行为类型
        /// </summary>
        public static RWAction<WorldBehaviorSystem> DoDefaultBehaviorHandler = null;

        /// <summary>
        /// 
        /// </summary>
        public static Action<WorldEntity> OnCreateEntityEvent;

        /// <summary>
        /// 
        /// </summary>
        public static Action<WorldEntity> OnRemoveEntityEvent;

        /// <summary>
        /// 组件每次扩容的大小
        /// </summary>
        public static int ComponentTypeCapacityExpandSize = 32;

        /// <summary> SoA组件下次扩容大小计算 </summary>
        public static Func<int, int> SoaComponentNextCapacity = MathEx.GetNextCapacityLength;

        /// <summary>
        /// 内存分配
        /// (size, alignment) : pointer
        /// </summary>
        public static unsafe Func<uint, uint, IntPtr> MemAlloc = (size, align) =>
#if UNITY_PROJ
            (IntPtr)Unity.Collections.LowLevel.Unsafe.UnsafeUtility.Malloc(size, (int)align, Unity.Collections.Allocator.Persistent);
#else
            (IntPtr)NativeMemory.AlignedAlloc(size, align);
#endif

        /// <summary>
        /// 内存释放
        /// </summary>
        public static unsafe Action<IntPtr> MemFree = ptr =>
#if UNITY_PROJ
            Unity.Collections.LowLevel.Unsafe.UnsafeUtility.Free((void*)ptr, Unity.Collections.Allocator.Persistent);
#else
            NativeMemory.AlignedFree((void*)ptr);
#endif

        /// <summary>
        /// archetype chunk 内存分配器
        /// </summary>
        public static WorldMemoryAllocator ChunkAllocator = new(16 * 1024, 32, 64);

        /// <summary>
        /// 线程并发级别
        /// </summary>
        public static int ThreadConcurrencyLevel =
#if UNITY_PROJ
            1;
#else
            Environment.ProcessorCount;
#endif
    }

    public delegate WorldEffectBase EffectHandlerDelegate(in WorldEffectSystem system, in WorldEntityId source, uint id, int addCount = 1);

    public delegate void DestroyHandlerDelegate(in WorldEffectSystem system, WorldEffectBase effect);
}