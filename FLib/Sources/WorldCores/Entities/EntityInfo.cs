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
        public readonly ushort ArchetypeIndex;
        public ushort ChunkEntityIndex;
        private ushort _dynamicComponentIndex;

        public bool IsEmpty => Version == 0;

        /// <summary>
        /// 
        /// </summary>
        public int DynamicComponentSparseIndex
        {
            readonly get => _dynamicComponentIndex - 1;
            set => _dynamicComponentIndex = checked((ushort)(value + 1));
        }

        /// <summary>
        /// 
        /// </summary>
        public readonly bool HasDynamicComponent => _dynamicComponentIndex != 0;

        public EntityInfo(ushort version, ushort archetypeIndex, ushort chunkEntityIndex, Chunk chunk)
        {
            Version = version;
            ArchetypeIndex = archetypeIndex;
            ChunkEntityIndex = chunkEntityIndex;
            Chunk = chunk;
            _dynamicComponentIndex = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="world"></param>
        /// <returns></returns>
        public readonly Archetype GetArchetype(WorldCore world) => world.ArchetypeGroup[ArchetypeIndex];
    }
}