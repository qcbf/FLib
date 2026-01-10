//==================={By Qcbf|qcbf@qq.com|2/17/2022 6:34:08 PM}===================

using FLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace FLib
{
    [Serializable]
    public struct FVector3 : IEquatable<FVector3>, IJson5Serializable, IJson5Deserializable
    {
        public static readonly FVector3 None = new(FNum.MinValue, FNum.MinValue, FNum.MinValue);
        public static readonly FVector3 Zero = new();
        public static readonly FVector3 One = new(1, 1, 1);
        public static readonly FVector3 Left = new(-1, 0, 0);
        public static readonly FVector3 Right = new(1, 0, 0);
        public static readonly FVector3 Up = new(0, 1, 0);
        public static readonly FVector3 Down = new(0, -1, 0);

        public FNum X, Y, Z;

        public FNum this[int index] => index switch
        {
            0 => X,
            1 => Y,
            _ => Z
        };

        public readonly FNum Magnitude => FNum.Sqrt(MagnitudeSqr);

        public readonly FNum MagnitudeSqr => X * X + Y * Y + Z * Z;

        public readonly FVector3 Normalize
        {
            get
            {
                var m = Magnitude;
                return m == 0 ? Zero : this / Magnitude;
            }
        }


        public FVector3(FNum xyz)
        {
            X = Y = Z = xyz;
        }

        public FVector3(in FNum x, in FNum y, in FNum z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public readonly override string ToString()
        {
            return $"{X:0.###}/{Y:0.###}/{Z:0.###}";
        }

        string IJson5Serializable.JsonSerialize(object obj, object customData, int indent, Json5SerializeOptionData opData) => $"[{X:0.###},{Y:0.###},{Z:0.###}]";

        Json5CustomDeserializeResult IJson5Deserializable.JsonDeserialize(ref Json5SyntaxNodes nodes, object otherData, in Json5DeserializeOptionData options)
        {
            Span<FNum> values = stackalloc FNum[3];
            FVector2.JsonParseHelper(ref nodes, ref values);
            X = values[0];
            Y = values[1];
            Z = values[2];
            return true;
        }

        public readonly bool Equals(FVector3 other)
        {
            return this == other;
        }

        public readonly override bool Equals(object obj)
        {
            return obj is FVector3 v && v == this;
        }

        public readonly override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }

        public static FNum Cross(in FVector3 a, in FVector3 b)
        {
            return a.X * b.Y - a.Y * b.X - a.Z * b.Z;
        }

        public static FNum Dot(in FVector3 a, in FVector3 b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        /// <summary>
        /// 得到角度
        /// </summary>
        public static FNum Angle(in FVector3 from, in FVector3 to)
        {
            var denominator = FNum.Sqrt(from.MagnitudeSqr * to.MagnitudeSqr);
            if (denominator <= FNum.Epsilon) return 0;
            return FNum.Acos(MathEx.Clamp(Dot(from, to) / denominator, -1, 1)) * MathEx.Rad2Deg;
        }

        /// <summary>
        /// 得到带正负的弧度
        /// </summary>
        public static FNum SignedAngle(in FVector3 from, in FVector3 to)
        {
            return Angle(from, to) * FNum.Sign(from.X * to.Y - from.Y * to.X + FNum.Epsilon);
        }

        /// <summary>
        /// 得到带正负的弧度
        /// </summary>
        public static FNum SignedAngle(in FVector3 from, in FVector3 to, in FVector3 axis)
        {
            var num2 = from.Y * to.Z - from.Z * to.Y;
            var num3 = from.Z * to.X - from.X * to.Z;
            var num4 = from.X * to.Y - from.Y * to.X;
            var num5 = FNum.Sign(axis.X * num2 + axis.Y * num3 + axis.Z * num4);
            return Angle(from, to) * num5;
        }

        public static FNum DistanceSqr(in FVector3 a, in FVector3 b)
        {
            var dx = b.X - a.X;
            var dy = b.Y - a.Y;
            var dz = b.Z - a.Z;
            return dx * dx + dy * dy + dz * dz;
        }

        public static FNum Distance(in FVector3 a, in FVector3 b)
        {
            return FNum.Sqrt(DistanceSqr(a, b));
        }

        public static FVector3 Abs(in FVector3 a)
        {
            return new(FNum.Abs(a.X), FNum.Abs(a.Y), FNum.Abs(a.Z));
        }

        public static FVector3 Max(in FVector3 a, in FVector3 b)
        {
            return new(FNum.Max(a.X, b.X), FNum.Max(a.Y, b.Y), FNum.Max(a.Z, b.Z));
        }

        public static FVector3 Min(in FVector3 a, in FVector3 b)
        {
            return new(FNum.Min(a.X, b.X), FNum.Min(a.Y, b.Y), FNum.Min(a.Z, b.Z));
        }

        public static FVector3 operator +(in FVector3 a, in FVector3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static FVector3 operator -(in FVector3 a, in FVector3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static FVector3 operator *(in FVector3 a, in FVector3 b) => new(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        public static FVector3 operator /(in FVector3 a, in FVector3 b) => new(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
        public static FVector3 operator +(in FNum a, in FVector3 b) => new(a + b.X, a + b.Y, a + b.Z);
        public static FVector3 operator -(in FNum a, in FVector3 b) => new(a - b.X, a - b.Y, a - b.Z);
        public static FVector3 operator *(in FNum a, in FVector3 b) => new(a * b.X, a * b.Y, a * b.Z);
        public static FVector3 operator /(in FNum a, in FVector3 b) => new(a / b.X, a / b.Y, a / b.Z);
        public static FVector3 operator +(in FVector3 a, in FNum b) => new(a.X + b, a.Y + b, a.Z + b);
        public static FVector3 operator -(in FVector3 a, in FNum b) => new(a.X + b, a.Y + b, a.Z + b);
        public static FVector3 operator *(in FVector3 a, in FNum b) => new(a.X * b, a.Y * b, a.Z * b);
        public static FVector3 operator /(in FVector3 a, in FNum b) => new(a.X / b, a.Y / b, a.Z / b);
        public static FVector3 operator -(in FVector3 a) => new(-a.X, -a.Y, -a.Z);

        public static bool operator ==(in FVector3 a, in FVector3 b) => FNum.Abs(a.X - b.X) <= FNum.Epsilon && FNum.Abs(a.Y - b.Y) <= FNum.Epsilon && FNum.Abs(a.Z - b.Z) <= FNum.Epsilon;
        public static bool operator !=(in FVector3 a, in FVector3 b) => !(a == b);
        public static bool operator >(in FVector3 a, in FVector3 b) => a.X > b.X && a.Y > b.Y && a.Z > b.Z;
        public static bool operator >=(in FVector3 a, in FVector3 b) => a.X >= b.X && a.Y >= b.Y && a.Z >= b.Z;
        public static bool operator <(in FVector3 a, in FVector3 b) => a.X < b.X && a.Y < b.Y && a.Z < b.Z;
        public static bool operator <=(in FVector3 a, in FVector3 b) => a.X <= b.X && a.Y <= b.Y && a.Z <= b.Z;
    }
}
