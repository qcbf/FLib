// ==================== qcbf@qq.com | 2026-01-18 ====================

using System.Collections;
using System.Collections.Generic;
using FLib.WorldCores.Archetypes;

namespace FLib.WorldCores.Queries
{
    public struct WorldArchetypeQueryEnumerator : IEnumerator<WorldArchetype>
    {
        private readonly WorldArchetype[] _archetypes;
        private readonly int _count;
        private int _index;
        public WorldArchetype Current => _archetypes[_index];
        object IEnumerator.Current => Current;

        public WorldArchetypeQueryEnumerator(WorldArchetype[] archetypes, int count)
        {
            _archetypes = archetypes;
            // Keep the buffer and range fixed even if a nested query grows the cache.
            _count = count;
            _index = -1;
        }

        public bool MoveNext() => ++_index < _count;
        public void Reset() => _index = -1;

        public void Dispose()
        {
        }
    }
}
