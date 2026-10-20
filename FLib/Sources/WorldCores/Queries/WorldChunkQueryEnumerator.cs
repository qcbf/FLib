// ==================== qcbf@qq.com | 2026-01-09 ====================

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FLib.WorldCores.Archetypes;

namespace FLib.WorldCores.Queries
{
    public struct WorldChunkQueryEnumerator : IEnumerator<WorldChunk>
    {
        private readonly WorldQuerySharedComponent[] _sharedComponents;
        private WorldArchetypeQueryEnumerator _archetypeEnumerator;
        private Dictionary<int, List<WorldChunk>>.Enumerator _sharedChunkEnumerator;
        private List<WorldChunk> _chunks;
        private int _chunkIndex;
        private bool _initialized;
        public WorldChunk Current { get; private set; }
        object IEnumerator.Current => Current;

        /// <summary>
        /// 
        /// </summary>
        public WorldChunkQueryEnumerator(in WorldQueryFilter filter)
        {
            _sharedComponents = filter.SharedComponents;
            _archetypeEnumerator = filter.GetArchetypeEnumerator();
            _sharedChunkEnumerator = default;
            _chunks = null;
            _chunkIndex = -1;
            Current = null;
            _initialized = false;
        }

        /// <summary>
        /// 
        /// </summary>
#if NET6_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
#endif
        public bool MoveNext()
        {
            if (!_initialized)
            {
                _initialized = true;
                if (!MoveNextArchetype())
                    return false;
            }

            while (true)
            {
                if (MoveNextChunk())
                    return true;
                if (MoveNextSharedChunk())
                    continue;
                if (!MoveNextArchetype())
                    return false;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _sharedChunkEnumerator.Dispose();
            _archetypeEnumerator.Dispose();
        }

        public void Reset()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining
#if NET6_0_OR_GREATER
                    | MethodImplOptions.AggressiveOptimization
#endif
        )]
        private bool MoveNextArchetype()
        {
            while (_archetypeEnumerator.MoveNext())
            {
                _sharedChunkEnumerator = _archetypeEnumerator.Current!.SharedChunks.GetEnumerator();
                if (MoveNextSharedChunk())
                    return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining
#if NET6_0_OR_GREATER
                    | MethodImplOptions.AggressiveOptimization
#endif
        )]
        private bool MoveNextSharedChunk()
        {
            while (_sharedChunkEnumerator.MoveNext())
            {
                _chunks = _sharedChunkEnumerator.Current.Value;
                _chunkIndex = _chunks.Count;
                if (_chunks.Count > 0)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining
#if NET6_0_OR_GREATER
                    | MethodImplOptions.AggressiveOptimization
#endif
        )]
        private bool MoveNextChunk()
        {
            while (--_chunkIndex >= 0)
            {
                var chunk = _chunks[_chunkIndex];
                for (var i = 0; i < _sharedComponents.Length; i++)
                {
                    var sharedComponent = _sharedComponents[i];
                    if (!chunk.Has(sharedComponent.ComponentId, sharedComponent.Hash))
                        goto ContinueWhile;
                }

                Current = chunk;
                return true;
                ContinueWhile: ;
            }

            return false;
        }
    }
}
