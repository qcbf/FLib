// ==================== qcbf@qq.com | 2026-01-10 ====================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FLib.WorldCores
{
    public class SoaComponentGroup<T> : ISoaComponentGroupable
    {
        public T[] Components = Array.Empty<T>();
        public Stack<int> Frees = new();
        public int Count;

        public WorldCore World { get; set; }
        Array ISoaComponentGroupable.Components => Components;

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
        public virtual void EnsureCapacity(int capacity)
        {
            if (Components.Length >= capacity) return;
            Array.Resize(ref Components, capacity);
            Frees.EnsureCapacity(capacity >> 2);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int Alloc(in Entity et)
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
        public virtual void Free(in Entity et, int key)
        {
            ComponentRegistry.GetInfo<T>().ComponentDestroy?.Invoke(ref Unsafe.As<T, byte>(ref Components[key]), World, et);
            Components[key] = default;
            Frees.Push(key);
            --Count;
        }
    }
}