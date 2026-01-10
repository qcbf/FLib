//==================={By Qcbf|qcbf@qq.com|9/14/2023 6:01:36 PM}===================

using System;
using System.Collections.Generic;

namespace FLib
{
    /// <summary>
    /// aabb
    /// </summary>
    [Serializable]
    public struct FRect : IEquatable<FRect>, IJson5Deserializable
    {
        public FVector2 Min;
        public FVector2 Max;

        public readonly FNum Width => Max.X - Min.X;
        public readonly FNum Height => Max.Y - Min.Y;
        public readonly FVector2 Size => Max - Min;
        public readonly FVector2 Center => Min + Size * FNum.OneHalf;

        public FRect(in FVector2 min, in FVector2 max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Expand(in FVector2 xy)
        {
            Min -= xy;
            Max += xy;
        }

        /// <summary>
        /// 
        /// </summary>
        public void TryUpdateMinMax(FVector2 point)
        {
            if (point < Min)
            {
                Min = point;
            }
            else if (point > Max)
            {
                Max = point;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Add(in FVector2 xy)
        {
            Min += xy;
            Max += xy;
        }

        public static FRect CreateByCenter(in FVector2 center, in FVector2 extends) => new(center - extends, center + extends);
        public readonly override string ToString() => $"{Min},{Max}";
        public readonly bool IsIntersect(in FRect rect) => Max >= rect.Min && Min <= rect.Max;
        public readonly bool IsIntersectCircle(in FVector2 circleCenter, in FNum circleRadius) => IsIntersectSqrCircle(circleCenter, circleRadius * circleRadius);

        public readonly bool IsIntersectSqrCircle(in FVector2 circleCenter, in FNum sqrCircleRadius)
        {
            var sqrDistance = FNum.Zero;
            if (circleCenter.X < Min.X)
            {
                var diff = Min.X - circleCenter.X;
                sqrDistance += diff * diff;
            }
            else if (circleCenter.X > Max.X)
            {
                var diff = circleCenter.X - Max.X;
                sqrDistance += diff * diff;
            }
            if (circleCenter.Y < Min.Y)
            {
                var diff = Min.Y - circleCenter.Y;
                sqrDistance += diff * diff;
            }
            else if (circleCenter.Y > Max.Y)
            {
                var diff = circleCenter.Y - Max.Y;
                sqrDistance += diff * diff;
            }
            return sqrDistance <= sqrCircleRadius;
        }

        public readonly bool Contains(in FVector2 point) => point >= Min && point <= Max;
        public readonly bool Contains(in FVector2 point, in FVector2 expand) => point >= Min - expand && point <= Max + expand;
        public readonly FVector2 GetClosestPoint(FVector2 p) => new(MathEx.Clamp(p.X, Min.X, Max.X), MathEx.Clamp(p.Y, Min.Y, Max.Y));

        public readonly bool TrimPoint(ref FVector2 point)
        {
            var result = false;
            if (point.X < Min.X)
            {
                point.X = Min.X;
                result = true;
            }
            else if (point.X > Max.X)
            {
                point.X = Max.X;
                result = true;
            }
            if (point.Y < Min.Y)
            {
                point.Y = Min.Y;
                result = true;
            }
            else if (point.Y > Max.Y)
            {
                point.Y = Max.Y;
                result = true;
            }
            return result;
        }

        readonly bool IEquatable<FRect>.Equals(FRect other) => Min == other.Min && Max == other.Max;


        public Json5CustomDeserializeResult JsonDeserialize(ref Json5SyntaxNodes nodes, object otherData, in Json5DeserializeOptionData options)
        {
            Span<FNum> vals = stackalloc FNum[4];
            FVector2.JsonParseHelper(ref nodes, ref vals);
            Min.X = vals[0];
            Min.Y = vals[1];
            Max.X = vals[2];
            Max.Y = vals[3];
            return true;
        }
    }
}
