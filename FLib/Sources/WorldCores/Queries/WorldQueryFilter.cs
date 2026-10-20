// ==================== qcbf@qq.com | 2026-01-17 ====================

using System;
using FLib.WorldCores;
using FLib.WorldCores.Archetypes;

namespace FLib.WorldCores.Queries
{
    public sealed class WorldQueryFilter
    {
        private readonly WorldArchetypeGroup _group;
        private readonly WorldQueryFilterBuilder _builder;
        private WorldArchetype[] _archetypes = Array.Empty<WorldArchetype>();
        private int _matchedCount;
        private int _scannedCount;

        internal readonly WorldQuerySharedComponent[] SharedComponents;

        public WorldQueryFilter(WorldCore world, in WorldQueryFilterBuilder builder)
        {
            _group = world.ArchetypeGroup;
            // Builder masks are mutable; a built query owns its conditions.
            _builder = new WorldQueryFilterBuilder(world)
            {
                AllMask = (ulong[])builder.AllMask?.Clone(),
                AnyMask = (ulong[])builder.AnyMask?.Clone(),
                NoneMask = (ulong[])builder.NoneMask?.Clone()
            };
            SharedComponents = builder.SharedComponents?.ToArray() ?? Array.Empty<WorldQuerySharedComponent>();
        }

        // ReSharper disable once NotDisposedResourceIsReturned
        internal WorldArchetypeQueryEnumerator GetArchetypeEnumerator()
        {
            var count = _group.Count;
            // Archetypes are append-only. Filter copies share this scan cursor and cache.
            for (; _scannedCount < count; ++_scannedCount)
            {
                var archetype = _group[(ushort)_scannedCount];
                if (!_builder.Match(archetype))
                    continue;
                if (_matchedCount == _archetypes.Length)
                    Array.Resize(ref _archetypes, Math.Max(4, MathEx.GetNextCapacityLength(_matchedCount)));
                _archetypes[_matchedCount++] = archetype;
            }

            return new WorldArchetypeQueryEnumerator(_archetypes, _matchedCount);
        }
    }
}
