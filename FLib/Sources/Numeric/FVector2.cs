//==================={By Qcbf|qcbf@qq.com|2/17/2022 6:34:08 PM}===================

using FLib;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace FLib
{
    [Serializable]
    public struct FVector2 : IEquatable<FVector2>, IJson5Serializable, IJson5Deserializable
    {
        public static readonly FVector2 None = new(FNum.MinValue, FNum.MinValue);
        public static readonly FVector2 Zero = new();
        public static readonly FVector2 One = new(1, 1);
        public static readonly FVector2 Left = new(-1, 0);
        public static readonly FVector2 Right = new(1, 0);
        public static readonly FVector2 Up = new(0, 1);
        public static readonly FVector2 Down = new(0, -1);

        public FNum X, Y;

        public readonly FNum Magnitude => FNum.Sqrt(SqrMagnitude);

        public readonly FNum SqrMagnitude => X * X + Y * Y;

        public readonly FNum this[int index] => index == 0 ? X : Y;

        public FVector2 Normalized
        {
            get
            {
                var m = Magnitude;
                return m == 0 ? Zero : this / Magnitude;
            }
        }

        public FVector2(FNum xy)
        {
            X = Y = xy;
        }

        public FVector2(in FNum x, in FNum y)
        {
            X = x;
            Y = y;
        }

        public readonly override string ToString()
        {
            return $"{X:0.###},{Y:0.###}";
        }

        string IJson5Serializable.JsonSerialize(object obj, object customData, int indent, Json5SerializeOptionData opData) => $"[{X:0.###},{Y:0.###}]";

        Json5CustomDeserializeResult IJson5Deserializable.JsonDeserialize(ref Json5SyntaxNodes nodes, object otherData, in Json5DeserializeOptionData options)
        {
            Span<FNum> vals = stackalloc FNum[2];
            JsonParseHelper(ref nodes, ref vals);
            X = vals[0];
            Y = vals[1];
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public static void JsonParseHelper(ref Json5SyntaxNodes nodes, ref Span<FNum> values)
        {
            Json5SyntaxNode node = default;
            for (var i = 0; i < values.Length; i++)
            {
                if (nodes.TryMoveNextValueOrCloseToken(out node))
                    values[i] = new FNum(node.ContentSpan);
                else
                    break;
            }
            if (node.Token != EJson5Token.Close)
                nodes.MoveNext(EJson5Token.Close);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)] public readonly bool Equals(FVector2 other) => this == other;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public readonly override bool Equals(object obj) => obj is FVector2 v && v == this;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public readonly override int GetHashCode() => HashCode.Combine(X, Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static FNum Cross(in FVector2 a, in FVector2 b) => a.X * b.Y - a.Y * b.X;

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static FNum Dot(in FVector2 a, in FVector2 b) => a.X * b.X + a.Y * b.Y;

        /// <summary>
        /// 旋转向量
        /// </summary>
        public static FVector2 Rotate(in FVector2 direction, FNum radian)
        {
            var s = FNum.Sin(radian);
            var c = FNum.Cos(radian);
            return new FVector2(direction.X * c - direction.Y * s, direction.X * s + direction.Y * c);
        }

        /// <summary>
        /// 旋转坐标
        /// </summary>
        public static FVector2 Rotate(in FVector2 point, in FVector2 originPoint, FNum radian)
        {
            var dx = point.X - originPoint.X;
            var dy = point.Y - originPoint.Y;
            if (dx - dy == 0)
                return point;

            var x2 = dx * FNum.Cos(radian) - dy * FNum.Sin(radian);
            var y2 = dx * FNum.Sin(radian) + dy * FNum.Cos(radian);
            return new FVector2(x2 + originPoint.X, y2 + originPoint.Y);
        }

        /// <summary>
        /// 得到角度
        /// </summary>
        public static FNum Angle(in FVector2 from, in FVector2 to)
        {
            var denominator = FNum.Sqrt(from.SqrMagnitude * to.SqrMagnitude);
            if (denominator <= FNum.Epsilon) return 0;
            return FNum.Acos(MathEx.Clamp(Dot(from, to) / denominator, -1, 1)) * MathEx.Rad2Deg;
        }

        /// <summary>
        /// 得到带正负的弧度
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FNum SignedAngle(in FVector2 from, in FVector2 to)
        {
            var cross = Cross(from, to);
            var dot = Dot(from, to);
            return FNum.Atan2(cross, dot) * MathEx.Rad2Deg;
            // return Angle(from, to) * FNum.SignWithoutZero(from.X * to.Y - from.Y * to.X);
        }

        /// <summary>
        /// 得到角度
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FNum Angle360(in FVector2 from, in FVector2 to)
        {
            var a = SignedAngle(from, to);
            if (a < 0) a += 360;
            return a;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FNum SqrDistance(in FVector2 a, in FVector2 b)
        {
            var dx = b.X - a.X;
            var dy = b.Y - a.Y;
            return dx * dx + dy * dy;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static FNum Distance(in FVector2 a, in FVector2 b) => FNum.Sqrt(SqrDistance(a, b));
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static FVector2 Abs(in FVector2 a) => new(FNum.Abs(a.X), FNum.Abs(a.Y));
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static FVector2 Max(in FVector2 a, in FVector2 b) => new(FNum.Max(a.X, b.X), FNum.Max(a.Y, b.Y));
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static FVector2 Min(in FVector2 a, in FVector2 b) => new(FNum.Min(a.X, b.X), FNum.Min(a.Y, b.Y));
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static FVector2 Lerp(in FVector2 from, in FVector2 to, FNum t) => new(from.X + (to.X - from.X) * t, from.Y + (to.Y - from.Y) * t);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static FVector2 Forward(FNum radian) => new(FNum.FastCos(radian), FNum.FastSin(radian));

        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool CheckMoveToPoint(in FVector2 current, in FVector2 next, in FVector2 to, in FNum distance)
            => SqrDistance(next, to) <= distance * distance || CheckMoveToPoint(current, next, to);

        /// <summary>
        /// 
        /// </summary>
        /// <returns>is move finished</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool CheckMoveToPoint(in FVector2 current, in FVector2 next, in FVector2 to)
            => (to.X - current.X) * (next.Y - current.Y) == (next.X - current.X) * (to.Y - current.Y) &&
               FNum.Min(current.X, next.X) <= to.X && to.X <= FNum.Max(current.X, next.X) &&
               FNum.Min(current.Y, next.Y) <= to.Y && to.Y <= FNum.Max(current.Y, next.Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static FVector2 operator +(in FVector2 a, in FVector2 b) => new(a.X + b.X, a.Y + b.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static FVector2 operator -(in FVector2 a, in FVector2 b) => new(a.X - b.X, a.Y - b.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static FVector2 operator *(in FVector2 a, in FVector2 b) => new(a.X * b.X, a.Y * b.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static FVector2 operator /(in FVector2 a, in FVector2 b) => new(a.X / b.X, a.Y / b.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static FVector2 operator +(in FNum a, in FVector2 b) => new(a + b.X, a + b.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static FVector2 operator -(in FNum a, in FVector2 b) => new(a - b.X, a - b.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static FVector2 operator *(in FNum a, in FVector2 b) => new(a * b.X, a * b.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static FVector2 operator /(in FNum a, in FVector2 b) => new(a / b.X, a / b.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static FVector2 operator +(in FVector2 a, in FNum b) => new(a.X + b, a.Y + b);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static FVector2 operator -(in FVector2 a, in FNum b) => new(a.X - b, a.Y - b);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static FVector2 operator *(in FVector2 a, in FNum b) => new(a.X * b, a.Y * b);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static FVector2 operator /(in FVector2 a, in FNum b) => new(a.X / b, a.Y / b);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static FVector2 operator +(in FVector2 a, in double b) => new(a.X + (FNum)b, a.Y + (FNum)b);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static FVector2 operator -(in FVector2 a, in double b) => new(a.X - (FNum)b, a.Y - (FNum)b);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static FVector2 operator *(in FVector2 a, in double b) => new(a.X * (FNum)b, a.Y * (FNum)b);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static FVector2 operator /(in FVector2 a, in double b) => new(a.X / (FNum)b, a.Y / (FNum)b);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static FVector2 operator -(in FVector2 a) => new(-a.X, -a.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator ==(in FVector2 a, in FVector2 b) => FNum.Abs(a.X - b.X) <= FNum.Epsilon && FNum.Abs(a.Y - b.Y) <= FNum.Epsilon;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator !=(in FVector2 a, in FVector2 b) => !(a == b);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator >(in FVector2 a, in FVector2 b) => a.X > b.X && a.Y > b.Y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator >=(in FVector2 a, in FVector2 b) => a.X >= b.X && a.Y >= b.Y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator <(in FVector2 a, in FVector2 b) => a.X < b.X && a.Y < b.Y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator <=(in FVector2 a, in FVector2 b) => a.X <= b.X && a.Y <= b.Y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static implicit operator FVector2(in (FNum, FNum) v) => new(v.Item1, v.Item2);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static implicit operator (FNum, FNum)(in FVector2 v) => new(v.X, v.Y);
    }
}
