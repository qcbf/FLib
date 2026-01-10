//==================={By Qcbf|qcbf@qq.com|9/14/2023 6:01:36 PM}===================

using System;
using System.Collections.Generic;

namespace FLib
{
    /// <summary>
    /// aabb
    /// </summary>
    [Serializable]
    public struct FRectInt : IEquatable<FRectInt>, IJson5Deserializable
    {
        public FVector2Int Min;
        public FVector2Int Max;

        public readonly int Width => Max.X - Min.X;
        public readonly int Height => Max.Y - Min.Y;
        public readonly FVector2Int Size => Max - Min;
        public readonly FVector2Int Center => Min + Size / 2;

        public FRectInt(in FVector2Int min, in FVector2Int max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Expand(in FVector2Int xy)
        {
            Min -= xy;
            Max += xy;
        }

        /// <summary>
        /// 
        /// </summary>
        public void TryUpdateMinMax(FVector2Int point)
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
        public void Add(in FVector2Int xy)
        {
            Min += xy;
            Max += xy;
        }

        public static FRectInt CreateByCenter(in FVector2Int center, in FVector2Int extends) => new(center - extends, center + extends);
        public readonly override string ToString() => $"{Min},{Max}";
        public readonly bool Contains(in FVector2Int point) => point >= Min && point <= Max;
        public readonly bool Contains(in FVector2Int point, in FVector2Int expand) => point >= Min - expand && point <= Max + expand;

        public readonly bool TrimPoint(ref FVector2Int point)
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

        readonly bool IEquatable<FRectInt>.Equals(FRectInt other) => Min == other.Min && Max == other.Max;


        public Json5CustomDeserializeResult JsonDeserialize(ref Json5SyntaxNodes nodes, object otherData, in Json5DeserializeOptionData options)
        {
            Span<int> vals = stackalloc int[4];
            FVector2Int.JsonParseHelper(ref nodes, ref vals);
            Min.X = vals[0];
            Min.Y = vals[1];
            Max.X = vals[2];
            Max.Y = vals[3];
            return true;
        }
    }
}
