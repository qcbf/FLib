// ==================== qcbf@qq.com | 2026-01-10 ====================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace FLib.WorldCores
{
    public class DynamicComponentGroup<T> : IDynamicComponentGroupable
    {
        public T[] Components = Array.Empty<T>();
        public Stack<int> Frees = new();
        public int Count;

        Array IDynamicComponentGroupable.Components => Components;

        public ref T this[in DynamicComponentContext ctx] => ref Components[ctx.ComponentIdx];

        // /// <summary>
        // /// 
        // /// </summary>
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public bool Has(Entity et, int idx)
        // {
        //     return idx < Components.Length && Components[idx].DynamicComponentContext.Entity == et;
        // }

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
            if (!Frees.TryPop(out var idx))
            {
                if (Count >= Components.Length)
                    EnsureCapacity(MathEx.GetNextPowerOfTwo(Count + 1));
                idx = Count;
            }

            ++Count;
            return idx;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Free(in Entity et, int idx)
        {
            Components[idx] = default;
            Frees.Push(idx);
            --Count;
        }
    }
}