// ==================== qcbf@qq.com | 2025-07-01 ====================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FLib.Collections
{
    public class QuadTree<T> : IEnumerable<QuadTree<T>.NodeEnumerator.Item>
    {
        public int MaxDepthLimit = 15;
        public int SplitNodeObjectNumber = 10;
        public int MaxObjectGroup = 1;
        public FVector2 NodeRectOutOffset = new(1);
        public ValueLinkedList<Node> Nodes;
        public ValueLinkedList<ObjectData> Objects;
        private FNum _size;
        private FNum _sizeHalf;

        public ref Node Root => ref Nodes[0];

        public QuadTree(FRect rect, int maxNodeDepthLimit = 0) => Set(rect, maxNodeDepthLimit);

        public QuadTree<T> Set(FRect rect, int maxNodeDepthLimit = 0)
        {
            _size = FNum.Max(rect.Width, rect.Height);
            _sizeHalf = FNum.Max(rect.Width, rect.Height) * FNum.OneHalf;
            if (maxNodeDepthLimit == 0)
            {
                var temp = (int)FNum.Ceiling(FNum.Log2(_size / 4));
                if (temp < MaxDepthLimit)
                    MaxDepthLimit = temp;
            }
            else
            {
                MaxDepthLimit = maxNodeDepthLimit;
            }
            if (Nodes.Count == 0)
                Nodes.Add(new Node() { Tree = this, ParentIdx = -1 });
            Root.Rect = rect;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref Node GetNode(int index) => ref Nodes[index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref ObjectData GetObj(int index) => ref Objects[index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref Node GetObjNode(int index) => ref Nodes[Objects[index].NodeId];

        /// <summary>
        /// 
        /// </summary>
        public int Add(in T obj, in FVector2 position, byte group = 0)
        {
            if (!Root.Rect.Contains(position))
                return -1;
            var objIdx = Objects.Add(new ObjectData() { Position = position, Obj = obj });
            Root.AddOrChildren(objIdx, group);
            return objIdx;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Remove(int objIndex)
        {
            var nodeIdx = Objects[objIndex].NodeId;
            var group = Objects[objIndex].Group;
            var v = Objects.RemoveAt(objIndex);
            v &= GetNode(nodeIdx).RemoveOrParents(objIndex, group);
            return v;
        }

        /// <summary>
        /// 
        /// </summary>
        public void RefreshPosition(int objIndex, in FVector2 position, byte? newGroup = null)
        {
            ref var obj = ref Objects[objIndex];
            var oldPos = obj.Position;
            obj.Position = position;
            ref var node = ref GetNode(obj.NodeId);
            if (node.Rect.Contains(position, NodeRectOutOffset) && (newGroup == null || obj.Group == newGroup))
                return;
            node.RemoveOrParents(objIndex, obj.Group);
            if (FVector2.SqrDistance(oldPos, position) >= _sizeHalf * _sizeHalf)
            {
                if (!Root.AddOrChildren(objIndex, newGroup ?? obj.Group))
                    throw new Exception($"add new node failure {obj}");
            }
            else
            {
                var parentIdx = node.ParentIdx;
                while (parentIdx >= 0)
                {
                    ref var tempNode = ref GetNode(parentIdx);
                    if (tempNode.AddOrParents(objIndex, newGroup ?? obj.Group))
                        break;
                    parentIdx = tempNode.ParentIdx;
                }
                if (parentIdx < 0)
                    throw new Exception($"add new node failure, {node.Depth},{node.Rect}");
            }
        }

        public NodeReverseEnumerator GetReverseEnumerator(bool isSkipRepeatNode = true) => Root.GetReverseEnumerator(isSkipRepeatNode);
        public NodeEnumerator GetEnumerator() => Root.GetEnumerator();
        IEnumerator<NodeEnumerator.Item> IEnumerable<NodeEnumerator.Item>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// 
        /// </summary>
        public struct ObjectData
        {
            public FVector2 Position;
            public T Obj;
            public int NodeId;
            public byte Group;
            public override string ToString() => $"{Position}|{Obj}|{Group}";
        }

        /// <summary>
        /// 
        /// </summary>
        public struct Node : IEnumerable<NodeEnumerator.Item>
        {
            public QuadTree<T> Tree;
            public HashSet<int>[] ObjIndexes;
            public int[] TotalObjCounts;
            public int[] Children;
            public FRect Rect;
            public int ParentIdx;
            public int SelfIndex;
            public int Depth;
            public ref Node Parent => ref Tree.GetNode(ParentIdx);
            public readonly bool HasChild => Children != null && Children[0] >= 0;
            public override string ToString() => $"{ParentIdx}/{SelfIndex}|{Depth}|{Rect}|{string.Join(',', Children)}";

            /// <summary>
            /// 添加值
            /// </summary>
            public bool AddOrParents(int objIdx, byte group, in FVector2 rectOutOffset = default)
            {
                if (AddOrChildren(objIdx, group, rectOutOffset))
                {
                    var parent = ParentIdx;
                    while (parent >= 0)
                    {
                        ref var node = ref Tree.GetNode(parent);
                        ++(node.TotalObjCounts ??= new int[Tree.MaxObjectGroup])[group];
                        parent = node.ParentIdx;
                    }
                    return true;
                }
                return false;
            }

            /// <summary>
            /// 
            /// </summary>
            public bool AddOrChildren(int objIdx, byte group, in FVector2 rectOutOffset = default)
            {
                var pos = Tree.GetObj(objIdx).Position;
                if (!Rect.Contains(pos, rectOutOffset))
                    return false;

                if (HasChild)
                {
                    foreach (var childIdx in Children)
                    {
                        ref var child = ref Tree.GetNode(childIdx);
                        if (child.Rect.Contains(pos, rectOutOffset))
                        {
                            ++(TotalObjCounts ??= new int[Tree.MaxObjectGroup])[group];
                            return child.AddOrChildren(objIdx, group, rectOutOffset);
                        }
                    }
                    return false;
                }

                ref var obj = ref Tree.Objects[objIdx];
                obj.Group = group;
                obj.NodeId = SelfIndex;

                if (ObjIndexes == null)
                {
                    ObjIndexes = new HashSet<int>[Tree.MaxObjectGroup];
                    for (var i = ObjIndexes.Length - 1; i >= 0; i--)
                        ObjIndexes[i] = new HashSet<int>();
                }
                if (!ObjIndexes[group].Add(objIdx))
                    throw new Exception($"already exist index {group} {Depth},{Rect}");
                ++(TotalObjCounts ??= new int[Tree.MaxObjectGroup])[group];
                if (Depth < Tree.MaxDepthLimit)
                {
                    if (TotalObjCounts.Sum() >= Tree.SplitNodeObjectNumber)
                        SplitToChildren();
                }
                return true;
            }

            /// <summary>
            /// 
            /// </summary>
            public bool RemoveOrParents(int index, byte group)
            {
                if (RemoveOrChildren(index, group))
                {
                    var parent = ParentIdx;
                    while (parent >= 0)
                    {
                        ref var node = ref Tree.GetNode(parent);
                        --node.TotalObjCounts[group];
                        parent = node.ParentIdx;
                    }
                    return true;
                }
                return false;
            }

            /// <summary>
            /// 移除值
            /// </summary>
            public bool RemoveOrChildren(int index, byte group)
            {
                if (HasChild)
                {
                    foreach (var childId in Children)
                    {
                        ref var node = ref Tree.GetNode(childId);
                        if (node.TotalObjCounts?[group] > 0 && node.RemoveOrChildren(index, group))
                        {
                            --TotalObjCounts[group];
                            return true;
                        }
                    }
                }
                else
                {
                    if (ObjIndexes[group].Remove(index))
                    {
                        --TotalObjCounts[group];
                        if (Depth > 1 && ParentIdx >= 0 && TotalObjCounts.Sum() == 0)
                        {
                            var mergeToParentId = ParentIdx;
                            while (mergeToParentId >= 0)
                            {
                                ref readonly var node = ref Tree.GetNode(mergeToParentId);
                                if (node.ParentIdx < 0 || node.TotalObjCounts.Sum() > Tree.SplitNodeObjectNumber)
                                    break;
                                mergeToParentId = node.ParentIdx;
                            }
                            try
                            {
                                Tree.GetNode(mergeToParentId).MergeChildren();
                            }
                            catch (Exception e)
                            {
                                throw new Exception($" {mergeToParentId} | {Tree.Nodes.Count} | {Tree.Nodes.NodeBuffer?.Length} | {e}");
                            }
                        }
                        return true;
                    }
                }
                return false;
            }

            /// <summary>
            /// 合并子节点
            /// </summary>
            public void MergeChildren()
            {
                foreach (var childId in Children)
                    MargeTo(Tree, SelfIndex, childId);
                Array.Fill(Children, -1);
                return;

                static void MargeTo(QuadTree<T> tree, int toNodeId, int fromNodeId)
                {
                    ref var fromNode = ref tree.GetNode(fromNodeId);
                    if (fromNode.HasChild)
                    {
                        foreach (var childId in fromNode.Children)
                            MargeTo(tree, toNodeId, childId);
                        Array.Fill(fromNode.Children, -1);
                        Array.Fill(fromNode.TotalObjCounts, 0);
                    }
                    else
                    {
                        if (fromNode.TotalObjCounts?.Sum() > 0)
                        {
                            Array.Fill(fromNode.TotalObjCounts, 0);
                            ref var toNode = ref tree.GetNode(toNodeId);
                            for (var group = 0; group < fromNode.ObjIndexes.Length; group++)
                            {
                                foreach (var objIdx in fromNode.ObjIndexes[group])
                                {
                                    if (!toNode.ObjIndexes[group].Add(objIdx))
                                        throw new Exception($"already exist index {toNodeId} {toNode.Depth},{toNode.Rect}");
                                    ++toNode.TotalObjCounts[group];
                                    tree.Objects[objIdx].NodeId = toNodeId;
                                }
                                fromNode.ObjIndexes[group].Clear();
                            }
                        }
                    }
                    tree.Nodes.RemoveAt(fromNodeId, false);
                }
            }

            /// <summary>
            /// 分裂
            /// </summary>
            public void SplitToChildren()
            {
                var size = Rect.Size;
                var sizeHalf = size * 0.5f;
                Children ??= new int[4];
                Children[0] = CreateChildNode(new FRect(Rect.Min, Rect.Max - sizeHalf), Tree, SelfIndex, Depth);
                Children[1] = CreateChildNode(new FRect(new FVector2(Rect.Min.X + sizeHalf.X, Rect.Min.Y), new FVector2(Rect.Max.X, Rect.Max.Y - sizeHalf.Y)), Tree, SelfIndex, Depth);
                Children[2] = CreateChildNode(new FRect(Rect.Min + sizeHalf, Rect.Max), Tree, SelfIndex, Depth);
                Children[3] = CreateChildNode(new FRect(new FVector2(Rect.Min.X, Rect.Min.Y + sizeHalf.Y), new FVector2(Rect.Max.X - sizeHalf.X, Rect.Max.Y)), Tree, SelfIndex, Depth);
                if (ObjIndexes != null)
                {
                    for (byte group = 0; group < ObjIndexes.Length; group++)
                    {
                        foreach (var objIdx in ObjIndexes[group])
                        {
                            ref var obj = ref Tree.GetObj(objIdx);
                            if (!AddOrChildren(objIdx, group) && !AddOrChildren(objIdx, group, Tree.NodeRectOutOffset))
                                throw new Exception($"add failure rect:{Rect} pos:{obj}");
                        }
                        ObjIndexes[group].Clear();
                    }
                }
                return;

                static int CreateChildNode(in FRect rect, QuadTree<T> tree, int selfId, int depth)
                {
                    tree.Nodes.AddEmpty(out var index) = new Node() { Tree = tree, SelfIndex = index, ParentIdx = selfId, Depth = depth + 1, Rect = rect };
                    return index;
                }
            }

            public NodeReverseEnumerator GetReverseEnumerator(bool isSkipRepeatNode = true) => new(Tree, SelfIndex, isSkipRepeatNode);
            public NodeEnumerator GetEnumerator() => new(Tree, SelfIndex);
            IEnumerator<NodeEnumerator.Item> IEnumerable<NodeEnumerator.Item>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }


        /// <summary>
        /// 
        /// </summary>
        public struct NodeEnumerator : IEnumerator<NodeEnumerator.Item>, IEnumerable<NodeEnumerator.Item>
        {
            public QuadTree<T> Tree;
            public Stack<int> Children;
            private int _nodeIndex;
            public Item Current => new() { Tree = Tree, NodeIndex = _nodeIndex, Children = Children };
            object IEnumerator.Current => Current;

            public struct Item
            {
                public QuadTree<T> Tree;
                public Stack<int> Children;
                public FRect Rect => Value.Rect;
                public int NodeIndex;
                public ref Node Value => ref Tree.GetNode(NodeIndex);

                /// <summary>
                /// 
                /// </summary>
                public bool ContinueChildren()
                {
                    ref readonly var node = ref Tree.GetNode(NodeIndex);
                    if (node.HasChild)
                    {
                        foreach (var childIndex in node.Children)
                            Children.Push(childIndex);
                        return false;
                    }
                    return true;
                }

                public readonly ObjectEnumerator GroupObjects(byte group)
                {
                    ref readonly var node = ref Tree.GetNode(NodeIndex);
                    return node.HasChild || node.ObjIndexes == null ? default : new ObjectEnumerator { Tree = Tree, Enumerator = node.ObjIndexes[group].GetEnumerator() };
                }
            }

            public NodeEnumerator(QuadTree<T> tree, int nodeIndex)
            {
                Tree = tree;
                Children = GlobalObjectPool<Stack<int>>.Create();
                Children.Push(nodeIndex);
                _nodeIndex = -1;
            }

            public bool MoveNext() => Children.TryPop(out _nodeIndex);
            public void Reset() { }
            public void Dispose() => GlobalObjectPool<Stack<int>>.Release(Children);
            public NodeEnumerator GetEnumerator() => this;
            IEnumerator IEnumerable.GetEnumerator() => this;
            IEnumerator<Item> IEnumerable<Item>.GetEnumerator() => this;
        }

        /// <summary>
        /// 
        /// </summary>
        public struct NodeReverseEnumerator : IEnumerator<NodeEnumerator>, IEnumerable<NodeEnumerator>
        {
            public QuadTree<T> Tree;
            public int ParentNodeIndex;
            private int _currentNodeIndex;
            private byte _childIndex;
            private sbyte _childCount;
            public bool IsSkipRepeatNode;
            public ref Node CurrentNode => ref Tree.GetNode(_currentNodeIndex);
            public ref Node ParentNode => ref Tree.GetNode(ParentNodeIndex);
            public NodeEnumerator Current => new(Tree, _currentNodeIndex);
            object IEnumerator.Current => Current;

            public NodeReverseEnumerator(QuadTree<T> tree, int currentNodeIndex, bool isSkipRepeatNode = true)
            {
                Tree = tree;
                IsSkipRepeatNode = isSkipRepeatNode;
                _childCount = -1;
                _childIndex = 0;
                _currentNodeIndex = currentNodeIndex;
                ParentNodeIndex = Tree.GetNode(_currentNodeIndex).ParentIdx;
                if (ParentNodeIndex >= 0)
                    _childIndex = (byte)Array.IndexOf(ParentNode.Children, currentNodeIndex);
            }

            public bool MoveNext()
            {
                if (_childCount == -1)
                {
                    ++_childIndex;
                    _childCount = 1;
                }
                else if (++_childCount == 5)
                {
                    _childCount = 1;
                    _currentNodeIndex = ParentNodeIndex;
                    ParentNodeIndex = CurrentNode.ParentIdx;
                    if (ParentNodeIndex >= 0)
                        _childIndex = (byte)(Array.IndexOf(ParentNode.Children, _currentNodeIndex) + 1);
                    if (IsSkipRepeatNode)
                        MoveNext();
                }
                else
                {
                    if (ParentNodeIndex < 0)
                        return false;
                    if (_currentNodeIndex >= 0)
                        _currentNodeIndex = ParentNode.Children[_childIndex++ % 4];
                }
                return _currentNodeIndex >= 0;
            }

            public void Reset() { }
            public void Dispose() { }
            public NodeReverseEnumerator GetEnumerator() => this;
            IEnumerator IEnumerable.GetEnumerator() => this;
            IEnumerator<NodeEnumerator> IEnumerable<NodeEnumerator>.GetEnumerator() => this;
        }

        /// <summary>
        /// 
        /// </summary>
        public struct ObjectEnumerator : IEnumerator<ObjectData>, IEnumerable<ObjectData>
        {
            public QuadTree<T> Tree;
            public HashSet<int>.Enumerator Enumerator;
            public ObjectData Current => Tree.GetObj(Enumerator.Current);
            object IEnumerator.Current => Current;
            public bool MoveNext() => Tree != null && Enumerator.MoveNext();
            public void Reset() { }
            public void Dispose() => Enumerator.Dispose();
            public ObjectEnumerator GetEnumerator() => this;
            IEnumerator IEnumerable.GetEnumerator() => this;
            IEnumerator<ObjectData> IEnumerable<ObjectData>.GetEnumerator() => this;
        }
    }
}
