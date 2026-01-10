//==================={By Qcbf|qcbf@qq.com|10/14/2022 11:27:43 AM}===================

using System;
using System.Collections.Generic;
using System.Text;

namespace FLib
{
    [Serializable]
    public struct FVector2Int : IJson5Deserializable, IJson5Serializable, IEquatable<FVector2Int>
    {
        public static readonly FVector2Int None = new(int.MinValue, int.MinValue);
        public static readonly FVector2Int Zero = new();
        public static readonly FVector2Int One = new(1, 1);
        public int X, Y;

        public double Magnitude => Math.Sqrt(SqrMagnitude);

        public readonly int SqrMagnitude => X * X + Y * Y;
        public int this[int index] => index == 0 ? X : Y;

        public FVector2Int(int x, int y)
        {
            X = x;
            Y = y;
        }

        public readonly override string ToString()
        {
            return $"{X}/{Y}";
        }


        public static int DistanceSqr(in FVector2Int a, in FVector2Int b)
        {
            var dx = b.X - a.X;
            var dy = b.Y - a.Y;
            return dx * dx + dy * dy;
        }

        public static double Distance(in FVector2Int a, in FVector2Int b)
        {
            return Math.Sqrt(DistanceSqr(a, b));
        }


        string IJson5Serializable.JsonSerialize(object obj, object customData, int indent, Json5SerializeOptionData opData) => $"[{X:0.###},{Y:0.###}]";

        Json5CustomDeserializeResult IJson5Deserializable.JsonDeserialize(ref Json5SyntaxNodes nodes, object otherData, in Json5DeserializeOptionData options)
        {
            Span<int> vals = stackalloc int[2];
            JsonParseHelper(ref nodes, ref vals);
            X = vals[0];
            Y = vals[1];
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public static void JsonParseHelper(ref Json5SyntaxNodes nodes, ref Span<int> values)
        {
            Json5SyntaxNode node = default;
            for (var i = 0; i < values.Length; i++)
            {
                if (nodes.TryMoveNextValueOrCloseToken(out node))
                    values[i] = node.ContentSpan.ToInt();
                else
                    break;
            }
            if (node.Token != EJson5Token.Close)
                nodes.MoveNext(EJson5Token.Close);
        }

        public readonly bool Equals(FVector2Int other) => X == other.X && Y == other.Y;
        public readonly override int GetHashCode() => (X, Y).GetHashCode();
        public readonly override bool Equals(object obj) => (obj is FVector2Int cubePos) && cubePos == this;
        public static bool operator ==(in FVector2Int a, in FVector2Int b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(in FVector2Int a, in FVector2Int b) => a.X != b.X || a.Y != b.Y;
        public static bool operator >(in FVector2Int a, in FVector2Int b) => a.X > b.X && a.Y > b.Y;
        public static bool operator >=(in FVector2Int a, in FVector2Int b) => a.X >= b.X && a.Y >= b.Y;
        public static bool operator <(in FVector2Int a, in FVector2Int b) => a.X < b.X && a.Y < b.Y;
        public static bool operator <=(in FVector2Int a, in FVector2Int b) => a.X <= b.X && a.Y <= b.Y;

        public static FVector2Int operator +(in FVector2Int a, in FVector2Int b) => new(a.X + b.X, a.Y + b.Y);
        public static FVector2Int operator -(in FVector2Int a, in FVector2Int b) => new(a.X - b.X, a.Y - b.Y);
        public static FVector2Int operator *(in FVector2Int a, in FVector2Int b) => new(a.X * b.X, a.Y * b.Y);
        public static FVector2Int operator /(in FVector2Int a, in FVector2Int b) => new(a.X / b.X, a.Y / b.Y);
        public static FVector2Int operator +(in int a, in FVector2Int b) => new(a + b.X, a + b.Y);
        public static FVector2Int operator -(in int a, in FVector2Int b) => new(a - b.X, a - b.Y);
        public static FVector2Int operator *(in int a, in FVector2Int b) => new(a * b.X, a * b.Y);
        public static FVector2Int operator /(in int a, in FVector2Int b) => new(a / b.X, a / b.Y);
        public static FVector2Int operator +(in FVector2Int a, in int b) => new(a.X + b, a.Y + b);
        public static FVector2Int operator -(in FVector2Int a, in int b) => new(a.X - b, a.Y - b);
        public static FVector2Int operator *(in FVector2Int a, in int b) => new(a.X * b, a.Y * b);
        public static FVector2Int operator /(in FVector2Int a, in int b) => new(a.X / b, a.Y / b);
        public static FVector2Int operator -(in FVector2Int a) => new(-a.X, -a.Y);

        public static implicit operator FVector2Int(in FVector2 v) => new((int)v.X, (int)v.Y);
        public static implicit operator FVector2(in FVector2Int v) => new(v.X, v.Y);
        public static implicit operator FVector2Int(in (int, int) v) => new(v.Item1, v.Item2);
        public static implicit operator (int, int)(in FVector2Int v) => new(v.X, v.Y);
    }
}
