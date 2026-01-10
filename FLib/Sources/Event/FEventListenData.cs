// ==================== qcbf@qq.com | 2025-07-01 ====================

using System;
using System.ComponentModel;

namespace FLib
{
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public struct FEventListenData : IEquatable<FEventListenData>
    {
        public Delegate Handler;
        public short Priority;
        public bool IsListenOnce;
        public readonly bool Equals(FEventListenData other) => Handler == other.Handler;
        public readonly override bool Equals(object obj) => obj is FEventListenData data && data.Handler == Handler;
        public readonly override int GetHashCode() => Handler.GetHashCode();
        public static bool operator ==(FEventListenData a, FEventListenData b) => a.Handler == b.Handler;
        public static bool operator !=(FEventListenData a, FEventListenData b) => a.Handler != b.Handler;
    }
}
