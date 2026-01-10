// =================================================={By Qcbf|qcbf@qq.com|2024-11-04}==================================================

using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;

namespace FLib.Worlds
{
    public struct WorldComponentHandleLinkList
    {
        public Node[] Nodes;
        public bool IsUseArrayPool;
        private int _freePosition;

        public readonly ref Node this[int index] => ref Nodes[index];

        public struct Node
        {
            public int NextPosition;
            public int PrevPosition;
            public int LinkedId;
            public WorldComponentHandle Handle;
        }

        /// <summary>
        /// 实体身上的组件迭代器
        /// </summary>
        public struct Iterator : IEnumerator<WorldComponentHandleEx>, IEnumerable<WorldComponentHandleEx>
        {
            public static readonly Iterator Empty = new(null, null, -1);
            public readonly WorldBase World;
            public readonly Node[] Nodes;
            public readonly int FirstIndex;
            private int _currentIndex;
            private int _nextIndex;

            public readonly ref Node CurrentNode => ref Nodes[_currentIndex];
            public readonly WorldComponentHandleEx Current => new(World, CurrentNode.Handle);

            readonly object IEnumerator.Current => Current;

            public Iterator(WorldBase world, Node[] nodes, int firstIndex)
            {
                World = world;
                Nodes = nodes;
                FirstIndex = firstIndex;
                _currentIndex = _nextIndex = -1;
            }

            public bool MoveNext()
            {
                _currentIndex = _currentIndex < 0 ? FirstIndex : _nextIndex;
                if (_currentIndex >= 0)
                    _nextIndex = Nodes[_currentIndex].NextPosition - 1;
                return _currentIndex >= 0;
            }

            public void Reset()
            {
                _currentIndex = FirstIndex;
                _nextIndex = -1;
            }

            public void Dispose() => Reset();
            readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            public readonly IEnumerator<WorldComponentHandleEx> GetEnumerator() => new Iterator(World, Nodes, FirstIndex);
        }

        /// <summary>
        /// 实体身上的组件迭代器-泛型
        /// </summary>
        public struct Iterator<T> : IEnumerator<WorldComponentHandleEx<T>>, IEnumerable<WorldComponentHandleEx<T>> where T : IWorldComponentable, new()
        {
            public static readonly Iterator<T> Empty = new(null, null, -1);
            public readonly WorldBase World;
            public readonly Node[] Nodes;
            public readonly int FirstIndex;
            private int _currentIndex;
            private int _nextIndex;

            public readonly ref Node CurrentNode => ref Nodes[_currentIndex];
            public readonly WorldComponentHandleEx<T> Current => new(World, CurrentNode.Handle);

            readonly object IEnumerator.Current => Current;

            public Iterator(WorldBase world, Node[] nodes, int firstIndex)
            {
                World = world;
                Nodes = nodes;
                FirstIndex = firstIndex;
                _currentIndex = _nextIndex = -1;
            }

            public bool MoveNext()
            {
                _currentIndex = _currentIndex < 0 ? FirstIndex : _nextIndex;
                if (_currentIndex >= 0)
                    _nextIndex = Nodes[_currentIndex].NextPosition - 1;
                return _currentIndex >= 0;
            }

            public void Reset()
            {
                _currentIndex = FirstIndex;
                _nextIndex = -1;
            }

            public void Dispose() => Reset();
            public readonly Iterator<T> GetEnumerator() => new(World, Nodes, FirstIndex);
            readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            readonly IEnumerator<WorldComponentHandleEx<T>> IEnumerable<WorldComponentHandleEx<T>>.GetEnumerator() => GetEnumerator();
        }

        /// <summary>
        ///
        /// </summary>
        public void SetCapacity(int size)
        {
            var oldSize = (Nodes?.Length).GetValueOrDefault();
            if (size <= oldSize)
                return;
            if (IsUseArrayPool)
                Nodes = ArrayPool<Node>.Shared.Rent(size);
            else
                Array.Resize(ref Nodes, size);
            if (_freePosition > 0)
                Nodes[_freePosition - 1].NextPosition = oldSize + 1;
            else
                _freePosition = oldSize + 1;
            for (var i = oldSize; i < size - 1; i++)
            {
                Nodes[i].NextPosition = i + 2;
                Nodes[i + 1].PrevPosition = i + 1;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void FreeNode(int position)
        {
            ref var node = ref Nodes[position - 1];
            node = default;
            if (_freePosition > 0)
            {
                Nodes[_freePosition - 1].PrevPosition = position;
                node.NextPosition = _freePosition;
            }
            _freePosition = position;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns>component position</returns>
        public int Create(WorldComponentHandle handle)
        {
            if (_freePosition == 0)
                SetCapacity(MathEx.GetNextPowerOfTwo(Nodes.Length + 1));

            var newCompPos = _freePosition;
            _freePosition = Nodes[_freePosition - 1].NextPosition;
            ref var compNode = ref Nodes[newCompPos - 1];
            compNode = new Node { Handle = handle };
            return newCompPos;
        }

        /// <summary>
        ///
        /// </summary>
        public readonly void ReturnToPool()
        {
            if (Nodes != null)
                ArrayPool<Node>.Shared.Return(Nodes);
        }

        /// <summary>
        ///
        /// </summary>
        public readonly Iterator GetIterator(WorldBase world, int firstPosition) => new(world, Nodes, firstPosition - 1);

        /// <summary>
        ///
        /// </summary>
        public readonly Iterator<T> GetIterator<T>(WorldBase world, int firstPosition) where T : IWorldComponentable, new() => new(world, Nodes, firstPosition - 1);
    }
}
