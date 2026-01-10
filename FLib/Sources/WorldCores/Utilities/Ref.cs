// ==================== qcbf@qq.com | 2026-01-03 ====================

using System;
using System.Runtime.InteropServices;

namespace FLib.WorldCores
{
    public readonly unsafe struct Ref<T> where T : unmanaged
    {
        internal readonly T* ValuePtr;
        public ref T Val => ref *ValuePtr;
        public Ref(T* valuePtr) => ValuePtr = valuePtr;
        public override string ToString() => Val.ToString();
        public override int GetHashCode() => Val.GetHashCode();
        public static implicit operator T(in Ref<T> r) => r.Val;
    }
}