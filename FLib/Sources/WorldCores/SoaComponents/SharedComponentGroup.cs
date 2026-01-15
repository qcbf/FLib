// ==================== qcbf@qq.com | 2026-01-15 ====================

using System;
using System.Collections.Generic;

namespace FLib.WorldCores
{
    public struct SharedComponentGroupRef
    {
        public int Index;
        public int RefCount;
    }

    public class SharedComponentGroup<T> : SoaComponentGroup<T> where T : ISharedComponent
    {
        public SlimDictionary<int, SharedComponentGroupRef> Groups = new();

        public override void EnsureCapacity(int capacity)
        {
            base.EnsureCapacity(capacity);
            Groups.EnsureCapacity(capacity);
        }

        public override int Alloc(in Entity et) => throw new NotSupportedException("need component value");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="et"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public int Alloc(in Entity et, in T value)
        {
            var hash = value.GetHashCode();
            ref var r = ref Groups.GetOrAddValueRef(hash);
            if (r.RefCount == 0)
                Components[r.Index = Alloc(et)] = value;
            ++r.RefCount;
            return hash;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="et"></param>
        /// <param name="key"></param>
        public override void Free(in Entity et, int key)
        {
            var idx = Groups.GetEntryIndex(key);
            if (idx < 0) return;
            ref var r = ref Groups.GetEntryValue(idx);
            if (--r.RefCount > 0) return;
            base.Free(in et, r.Index);
            Groups.Remove(key);
        }
    }
}