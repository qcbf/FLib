// ==================== qcbf@qq.com | 2026-01-09 ====================

namespace FLib.WorldCores
{
    public ref struct ChunkQueryEnumerator
    {
        public PooledList<Archetype> Archetypes;
        private int _archetypeIndex;
        public Chunk Current { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ChunkQueryEnumerator(WorldCore world, in QueryFilter filter)
        {
            Archetypes = new PooledList<Archetype>();
            for (ushort i = 0; i < world.ArchetypeGroup.Count; i++)
            {
                var archetype = world.ArchetypeGroup[i];
                if (filter.Match(archetype))
                    Archetypes.Add(archetype);
            }

            Current = null;
            _archetypeIndex = -1;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool MoveNext()
        {
            if (Current == null)
                return NextArchetype();
            Current = Current.Previous;
            return Current != null || NextArchetype();
        }

        /// <summary>
        /// 
        /// </summary>
        private bool NextArchetype()
        {
            while (Archetypes.Count > ++_archetypeIndex && Archetypes[_archetypeIndex].Chunks?.Count > 0)
            {
                Current = Archetypes[_archetypeIndex].Chunks;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Archetypes.Dispose();
        }
    }
}