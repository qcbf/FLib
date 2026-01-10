//==================={By Qcbf|qcbf@qq.com|10/13/2022 5:36:33 PM}===================

using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FLib;

namespace FLib
{
    public struct ValueLinkedList<T> : IEnumerable<T>
    {
        public Node[] NodeBuffer;
        public bool IsUsePool;
        public int AllocateAddSize;
        private int mFreeIndex;
        public readonly ref T this[int index] => ref NodeBuffer[index].Value;
        public int ActiveIndex { get; private set; }
        public int Count { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public ref struct BytesPackHelper
        {
            public Node[] Nodes;
            public int Count;
            public int Index;
            public readonly ref T Value => ref Nodes[Index].Value;
            public readonly bool IsFreed => Nodes[Index].IsFreed;

            private BytesPackHelper(in ValueLinkedList<T> list, int count)
            {
                Nodes = list.NodeBuffer;
                Index = -1;
                Count = count;
            }

            public static BytesPackHelper Create(in ValueLinkedList<T> list, ref BytesWriter writer)
            {
                var v = new BytesPackHelper(list, (list.NodeBuffer?.Length).GetValueOrDefault());
                writer.PushLength(v.Count);
                writer.PushVInt(list.Count);
                writer.PushVInt(list.mFreeIndex);
                writer.PushVInt(list.ActiveIndex);
                return v;
            }

            public static BytesPackHelper Create(ref ValueLinkedList<T> list, ref BytesReader reader)
            {
                var v = new BytesPackHelper(list, reader.ReadLength());
                list.Clear();
                list.Count = (int)reader.ReadVInt();
                list.mFreeIndex = (int)reader.ReadVInt();
                list.ActiveIndex = (int)reader.ReadVInt();
                if (v.Count > 0)
                {
                    list.Allocate(v.Count);
                    v.Nodes = list.NodeBuffer;
                }
                return v;
            }

            public bool Push(ref BytesWriter writer)
            {
                if (++Index < Count)
                {
                    writer.PushVInt(Nodes[Index].Down);
                    writer.PushVInt(Nodes[Index].Up);
                    return true;
                }
                return false;
            }

            public bool Read(ref BytesReader reader)
            {
                if (++Index < Count)
                {
                    Nodes[Index].Down = (int)reader.ReadVInt();
                    Nodes[Index].Up = (int)reader.ReadVInt();
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public struct Node
        {
            public int Up;
            public int Down;
            public T Value;
            public readonly bool IsFreed => Down <= 0;
            public readonly override string ToString() => Value.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        public struct Enumerator : IEnumerator<T>
        {
            public Node[] Nodes;
            public int Index;
            public int Count;
            public readonly ref T Current => ref Nodes[Index].Value;
            readonly T IEnumerator<T>.Current => Current;
            readonly object IEnumerator.Current => Current;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                Reset();
                Nodes = null;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                Index = -1;
            }

            public bool MoveNext()
            {
                if (--Count < 0) return false;
                while (++Index < Nodes.Length && Nodes[Index].IsFreed)
                {
                }
                return Index < Nodes.Length;
            }
        }

        public ValueLinkedList(int capacity, bool isUsePool = false)
        {
            IsUsePool = isUsePool;
            NodeBuffer = IsUsePool ? ArrayPool<Node>.Shared.Rent(capacity) : new Node[capacity];
            ActiveIndex = Count = mFreeIndex = 0;
            AllocateAddSize = 0;
        }


        public void Clear(bool isFreeMemory = false)
        {
            if (NodeBuffer == null) return;
            if (isFreeMemory)
                TryReleasePool();
            else
                Array.Clear(NodeBuffer, 0, NodeBuffer.Length);
            mFreeIndex = Count = 0;
            ActiveIndex = -1;
        }

        public int IndexOf(in T v)
        {
            using var iterator = GetEnumerator();
            while (iterator.MoveNext())
            {
                if (EqualityComparer<T>.Default.Equals(iterator.Current, v))
                    return iterator.Index;
            }
            return -1;
        }

        public bool RemoveAt(int index, bool isClearValue = true)
        {
            ref var node = ref NodeBuffer[index];
            if (node.IsFreed) return false;
            ++index;
            var isTheLast = node.Down == index;
            if (node.Up > 0)
            {
                NodeBuffer[node.Up - 1].Down = node.Down;
            }
            else
            {
                ActiveIndex = isTheLast ? -1 : node.Down - 1;
            }
            if (!isTheLast)
            {
                NodeBuffer[node.Down - 1].Up = node.Up;
            }
            if (isClearValue)
                node.Value = default;
            node.Up = 0;
            node.Down = -mFreeIndex;
            if (mFreeIndex > 0)
            {
                NodeBuffer[mFreeIndex - 1].Up = index;
            }
            mFreeIndex = index;
            --Count;
            return true;
        }

        public int Add(in T v)
        {
            var index = Count++;
            if (mFreeIndex == 0)
            {
                if (!(Count < NodeBuffer?.Length))
                {
                    var newSize = AllocateAddSize == 0 ? MathEx.GetNextPowerOfTwo(Count) : index + AllocateAddSize;
                    Allocate(newSize);
                }
                NodeBuffer![index] = new Node { Value = v };
            }
            else
            {
                index = mFreeIndex - 1;
                ref var freeNode = ref NodeBuffer[index];
                mFreeIndex = -freeNode.Down;
                freeNode.Value = v;
            }
            ActiveNode(index);
            return index;
        }

        public ref T AddEmpty(out int idx)
        {
            idx = Add(default);
            return ref NodeBuffer[idx].Value;
        }

        private void ActiveNode(int index)
        {
            ref var node = ref NodeBuffer[index];
            ++index;
            node.Up = 0;
            if (ActiveIndex >= 0)
            {
                node.Down = ActiveIndex + 1;
                NodeBuffer[ActiveIndex].Up = index;
            }
            else
            {
                node.Down = index; //self
            }
            ActiveIndex = index - 1;
        }

        public readonly Enumerator GetEnumerator()
        {
            return new Enumerator { Nodes = NodeBuffer, Index = -1, Count = Count };
        }

        public bool Allocate(int v)
        {
            if (NodeBuffer?.Length >= v) return false;
            Node[] newBuffer;
            if (IsUsePool)
            {
                newBuffer = ArrayPool<Node>.Shared.Rent(v);
                NodeBuffer?.CopyTo(newBuffer, 0);
                TryReleasePool();
            }
            else
            {
                newBuffer = new Node[v];
                NodeBuffer?.CopyTo(newBuffer, 0);
            }
            NodeBuffer = newBuffer;
            return true;
        }

        public void TryReleasePool()
        {
            if (NodeBuffer != null)
            {
                if (IsUsePool)
                    ArrayPool<Node>.Shared.Return(NodeBuffer, true);
                NodeBuffer = null;
            }
        }

        public readonly bool Contains(int index)
        {
            return index >= 0 && index < NodeBuffer.Length && !NodeBuffer[index].IsFreed;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
