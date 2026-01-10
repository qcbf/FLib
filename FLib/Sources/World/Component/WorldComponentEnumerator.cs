// =================================================={By Qcbf|qcbf@qq.com|2024-11-01}==================================================

using System.Collections;
using System.Collections.Generic;

namespace FLib.Worlds
{
    /// <summary>
    /// 组件迭代器-泛型
    /// </summary>
    public struct WorldComponentEnumerator<T> : IEnumerator<WorldComponentHandleEx<T>>, IEnumerable<WorldComponentHandleEx<T>> where T : IWorldComponentable, new()
    {
        public readonly WorldBase World;
        public readonly WorldComponentGroup<T>.Node[] Nodes;
        private int _index;
        public readonly WorldComponentHandleEx<T> Current => new(World, new WorldComponentHandle(WorldComponentGroup<T>.TypeId, (ushort)_index));
        readonly object IEnumerator.Current => Current;

        public WorldComponentEnumerator(WorldBase world, WorldComponentGroup<T>.Node[] nodes)
        {
            World = world;
            Nodes = nodes;
            _index = -1;
        }

        public bool MoveNext()
        {
            if (Nodes == null)
                return false;
            while (++_index < Nodes.Length && !Nodes[_index].IsValid)
            {
            }
            return _index < Nodes.Length;
        }

        public void Reset() => _index = -1;
        public void Dispose() => Reset();
        readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public readonly IEnumerator<WorldComponentHandleEx<T>> GetEnumerator() => new WorldComponentEnumerator<T>(World, Nodes);
    }
}
