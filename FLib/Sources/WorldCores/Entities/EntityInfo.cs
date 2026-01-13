// ==================== qcbf@qq.com | 2026-01-04 ====================

using System.Runtime.CompilerServices;

namespace FLib.WorldCores
{
    [SkipLocalsInit]
    public struct EntityInfo
    {
        public static EntityInfo Empty = default;
        public Chunk Chunk;
        public readonly ushort Version;
        public readonly ushort ArchetypeIdx;
        public ushort ChunkEntityIdx;
        private ushort _dynamicComponentIdx;

        public bool IsEmpty => Version == 0;

        /// <summary>
        /// 
        /// </summary>
        public int DynamicComponentSparseIdx
        {
            readonly get => _dynamicComponentIdx - 1;
            set => _dynamicComponentIdx = checked((ushort)(value + 1));
        }

        /// <summary>
        /// 
        /// </summary>
        public readonly bool HasDynamicComponent => _dynamicComponentIdx != 0;

        public EntityInfo(ushort version, ushort archetypeIdx, ushort chunkEntityIdx, Chunk chunk)
        {
            Version = version;
            ArchetypeIdx = archetypeIdx;
            ChunkEntityIdx = chunkEntityIdx;
            Chunk = chunk;
            _dynamicComponentIdx = 0;
        }
    }
}