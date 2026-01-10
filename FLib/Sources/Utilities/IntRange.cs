// ==================== qcbf@qq.com | 2025-07-01 ====================

using System;

namespace FLib
{
    [Serializable]
    public struct IntRange : IEquatable<IntRange>, IJson5Deserializable
    {
        public int Begin;
        public int End;

        public IntRange(int beginEnd) => Begin = End = beginEnd;

        public IntRange(int begin, int end)
        {
            Begin = begin;
            End = end;
        }

        public int AddBegin(int v) => Begin += v;
        public int AddEnd(int v) => End += v;

        public override string ToString() => $"{Begin}-{End}";
        public static implicit operator Range(in IntRange range) => new(range.Begin, range.End);
        public static implicit operator IntRange(in Range range) => new(range.Start.Value, range.End.Value);
        public static implicit operator IntRange(in int beginEnd) => new(beginEnd);
        public bool Equals(IntRange other) => Begin == other.Begin && End == other.End;
        public override bool Equals(object obj) => obj is IntRange range && Equals(range);
        public override int GetHashCode() => HashCode.Combine(Begin, End);
        public static bool operator ==(in IntRange left, in IntRange right) => left.Begin == right.Begin && left.End == right.End;
        public static bool operator !=(in IntRange left, in IntRange right) => !(left == right);

        public Json5CustomDeserializeResult JsonDeserialize(ref Json5SyntaxNodes nodes, object otherData, in Json5DeserializeOptionData options)
        {
            if (!Json5SyntaxNodesReader.TryCreate(ref nodes, out var node, out var reader)) return false;
            var content = node.ContentSpan;
            var temp = StringFLibUtility.SegmentTextReadWithMoveNext(ref content);
            if (!temp.IsEmpty)
            {
                Begin = temp.ToInt();
                temp = StringFLibUtility.SegmentTextReadWithMoveNext(ref content);
                if (!temp.IsEmpty)
                    End = temp.ToInt();
            }
            reader.Close(ref nodes);
            return true;
        }
    }
}
