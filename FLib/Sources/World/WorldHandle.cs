// ==================== qcbf@qq.com | 2025-07-01 ====================

using System.Runtime.CompilerServices;

namespace FLib.Worlds
{
    public readonly struct WorldHandle
    {
        public readonly ushort Index;
        public readonly ushort Version;

        public WorldBase Val
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => WorldBase.AllWorlds[Index];
        }

        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Version == 0 || WorldBase.AllWorlds.Count <= Index || WorldBase.AllWorlds[Index]?.Handle.Version != Version;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator WorldBase(in WorldHandle v) => v.Val;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WorldHandle(ushort index, ushort version)
        {
            Index = index;
            Version = version;
        }
    }
}
