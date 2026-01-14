// ==================== qcbf@qq.com | 2026-01-10 ====================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FLib.WorldCores
{
    public class DynamicComponentGroup<T> : IDynamicComponentGroupable
    {
        public T[] Components = Array.Empty<T>();
        public Stack<int> Frees = new();
        public int Count;

        public WorldCore World { get; set; }
        Array IDynamicComponentGroupable.Components => Components;


        public ref T this[in DynamicComponentContext ctx] => ref Components[ctx.ComponentIndex];

        // /// <summary>
        // /// 
        // /// </summary>
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public bool Has(Entity et, int idx)
        // {
        //     return idx < Components.Length && Components[idx].DynamicComponentContext.Entity == et;
        // }

        /// <summary>
        /// 
        /// </summary>
        public void EnsureCapacity(int capacity)
        {
            if (Components.Length >= capacity) return;
            Array.Resize(ref Components, capacity);
            Frees.EnsureCapacity(capacity >> 2);
        }

        /// <summary>
        /// 
        /// </summary>
        public int Alloc(in Entity et)
        {
            if (!Frees.TryPop(out var index))
            {
                if (Count >= Components.Length)
                    EnsureCapacity(MathEx.GetNextPowerOfTwo(Count + 1));
                index = Count;
            }

            ++Count;
            ref var first = ref MemoryMarshal.GetArrayDataReference(Components);
            first = ref Unsafe.Add(ref first, index);
            ComponentRegistry.GetInfo<T>().ComponentAwake?.Invoke(ref Unsafe.As<T, byte>(ref first), World, et);
            return index;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Free(in Entity et, int index)
        {
            ComponentRegistry.GetInfo<T>().ComponentDestroy?.Invoke(ref Unsafe.As<T, byte>(ref Components[index]), World, et);
            Components[index] = default;
            Frees.Push(index);
            --Count;
        }
    }
}