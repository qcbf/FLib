// ==================== qcbf@qq.com | 2026-01-17 ====================

using System.Collections;
using System.Collections.Generic;
using FLib.WorldCores;
using FLib.WorldCores.Entities;

namespace FLib.WorldCores.Queries
{
    /// <summary>
    /// 世界查询枚举器，支持对所有匹配查询条件的实体进行01。
    /// </summary>
    public readonly struct WorldQueryEnumerator : IEnumerable<WorldEntityId>
    {
        public readonly WorldQueryFilter Filter;
        public Enumerable GetEnumerator() => new(Filter);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        IEnumerator<WorldEntityId> IEnumerable<WorldEntityId>.GetEnumerator() => GetEnumerator();

        public WorldQueryEnumerator(in WorldCore world, in WorldQueryFilter filter)
        {
            Filter = filter;
            if (Filter == null)
                Filter = new WorldQueryFilterBuilder(world).Build();
        }

        /// <summary>
        /// 世界查询枚举器的内部实现，支持对扩展的递次轮询。
        /// </summary>
        public unsafe struct Enumerable : IEnumerator<WorldEntityId>
        {
            public WorldChunkQueryEnumerator ChunkEnumerator;
            private int _index;
            private int _count;
            private WorldEntityId* _entity;
            public WorldEntityId Current => *(_entity + _index);
            object IEnumerator.Current => Current;

            public Enumerable(WorldQueryFilter filter)
            {
                ChunkEnumerator = new WorldChunkQueryEnumerator(filter);
                _count = _index = 0;
                _entity = null;
            }

            /// <summary>
            /// 自动挪到下一个匹配的实体。
            /// </summary>
            /// <returns>如果还有实体再返回 true，否则返回 false</returns>
            public bool MoveNext()
            {
                if (++_index < _count)
                    return true;
                if (!ChunkEnumerator.MoveNext())
                    return false;

                var chunk = ChunkEnumerator.Current!;
                _index = 0;
                _count = chunk.Count;
                _entity = chunk.GetEntity(0);
                return true;
            }

            public void Reset()
            {
            }

            public void Dispose()
            {
                ChunkEnumerator.Dispose();
            }
        }
    }
}
