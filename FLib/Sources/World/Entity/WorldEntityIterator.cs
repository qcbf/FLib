//=================================================={By Qcbf|qcbf@qq.com|11/15/2024 10:13:23 AM}==================================================

using FLib;
using System;
using System.Collections;
using System.Collections.Generic;


namespace FLib.Worlds
{
    public struct WorldEntityIterator : IEnumerator<WorldEntity>, IEnumerable<WorldEntity>
    {
        public readonly WorldEntity[] Entities;
        public readonly ushort EntityCount;
        private ushort _offset;
        private ushort _moveIndex;
        public readonly WorldEntity Current => Entities[_moveIndex + _offset - 1];
        readonly object IEnumerator.Current => Current;

        public WorldEntityIterator(WorldEntity[] entities, ushort entityCount)
        {
            Entities = entities;
            EntityCount = entityCount;
            _moveIndex = _offset = 0;
        }

        public bool MoveNext()
        {
            ++_moveIndex;
            while (_moveIndex + _offset < Entities.Length && Current.IsEmpty)
                ++_offset;
            return _moveIndex <= EntityCount;
        }

        public void Reset() => _moveIndex = _offset = 0;
        public void Dispose() => Reset();
        public readonly IEnumerator<WorldEntity> GetEnumerator() => new WorldEntityIterator(Entities, EntityCount);
        readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
