//==================={By Qcbf|qcbf@qq.com|10/7/2023 5:15:43 PM}===================

using System;
using System.Collections.Generic;

namespace FLib
{
    /// <summary>
    /// obb
    /// </summary>
    [Serializable]
    public struct FOrientedRect : IEquatable<FOrientedRect>, IJson5Deserializable
    {
        public FVector2 Center;
        public FVector2 Extends;
        public FNum Angle;

        public readonly FRect AsAABBRect()
        {
            if (Angle == 0)
                return FRect.CreateByCenter(Center, Extends);
            Span<FVector2> vertices = stackalloc FVector2[4];
            GetVertices(ref vertices);
            var min = vertices[0];
            var max = vertices[2];
            for (var i = 0; i < vertices.Length; i++)
            {
                if (vertices[i].X < min.X)
                    min.X = vertices[i].X;
                if (vertices[i].Y < min.Y)
                    min.Y = vertices[i].Y;
                if (vertices[i].X > max.X)
                    max.X = vertices[i].X;
                if (vertices[i].Y > max.Y)
                    max.Y = vertices[i].Y;
            }

            return new FRect(min, max);
        }

        /// <summary>
        /// 
        /// </summary>
        public readonly void GetVertices(ref Span<FVector2> vertices)
        {
            var radian = Angle * MathEx.Deg2Rad;
            var rotatedExtends = FVector2.Rotate(Extends, radian);
            vertices[0] = new FVector2(Center.X - rotatedExtends.X, Center.Y - rotatedExtends.Y);
            vertices[2] = new FVector2(Center.X + rotatedExtends.X, Center.Y + rotatedExtends.Y);
            rotatedExtends = FVector2.Rotate(Extends, -radian);
            vertices[1] = new FVector2(Center.X + rotatedExtends.X, Center.Y - rotatedExtends.Y);
            vertices[3] = new FVector2(Center.X - rotatedExtends.X, Center.Y + rotatedExtends.Y);
        }

        /// <summary>
        /// 
        /// </summary>
        public readonly bool Contains(in FVector2 point)
        {
            return FRect.CreateByCenter(Center, Extends).Contains(FVector2.Rotate(point, Center, -Angle * MathEx.Deg2Rad));
        }

        /// <summary>
        /// 
        /// </summary>
        public readonly bool IsIntersect(in FOrientedRect rect)
        {
            if (!AsAABBRect().IsIntersect(rect.AsAABBRect()))
                return false;
            if (Angle == 0 && rect.Angle == 0)
                return true;
            Span<FVector2> vertices1 = stackalloc FVector2[4];
            Span<FVector2> vertices2 = stackalloc FVector2[4];
            GetVertices(ref vertices1);
            rect.GetVertices(ref vertices2);
            return IsIntersectInDirections(vertices1, vertices2);
        }

        /// <summary>
        /// 
        /// </summary>
        public readonly bool IsIntersect(in FRect rect)
        {
            if (!AsAABBRect().IsIntersect(rect))
                return false;
            if (Angle == 0)
                return true;
            Span<FVector2> vertices = stackalloc FVector2[4];
            GetVertices(ref vertices);
            Span<FVector2> vertices2 = stackalloc FVector2[] { rect.Min, new(rect.Max.X, rect.Min.X), rect.Max, new(rect.Min.X, rect.Max.Y) };
            return IsIntersectInDirections(vertices, vertices2);
        }

        /// <summary>
        /// 
        /// </summary>
        private static bool IsIntersectInDirections(in Span<FVector2> vertices1, in Span<FVector2> vertices2)
        {
            return IsIntersectInDirection(vertices1[1] - vertices1[0], vertices1, vertices2) && IsIntersectInDirection(vertices1[2] - vertices1[1], vertices1, vertices2)
                                                                                             && IsIntersectInDirection(vertices2[1] - vertices2[0], vertices1, vertices2) &&
                                                                                             IsIntersectInDirection(vertices2[2] - vertices2[1], vertices1, vertices2);
        }

        /// <summary>
        /// 
        /// </summary>
        private static bool IsIntersectInDirection(in FVector2 direction, in Span<FVector2> polygonVertices1, in Span<FVector2> polygonVertices2)
        {
            var ln = new FVector2(-direction.Y, direction.X);
            GetMinMaxProjection(polygonVertices1, ln, out var p1max, out var p1min);
            GetMinMaxProjection(polygonVertices2, ln, out var p2max, out var p2min);
            return p1max >= p2min && p1min <= p2max;

            static void GetMinMaxProjection(in Span<FVector2> vertices, in FVector2 ln, out FNum max, out FNum min)
            {
                min = FNum.MaxValue;
                max = FNum.MinValue;
                for (var i = 0; i < vertices.Length; i++)
                {
                    var dot = FVector2.Dot(vertices[i], ln);
                    if (dot > max)
                        max = dot;
                    if (dot < min)
                        min = dot;
                }
            }
        }

        public readonly override string ToString() => $"{Center},{Extends},{Angle}";
        readonly bool IEquatable<FOrientedRect>.Equals(FOrientedRect other) => Center == other.Center && Extends == other.Extends && Angle == other.Angle;

        public Json5CustomDeserializeResult JsonDeserialize(ref Json5SyntaxNodes nodes, object otherData, in Json5DeserializeOptionData options)
        {
            if (nodes.TryMoveNextValueOrCloseToken(out var node))
                Center.X = node.ContentSpan.ToFNum();
            if (node.Token != EJson5Token.Close && nodes.TryMoveNextValueOrCloseToken(out node))
                Center.Y = node.ContentSpan.ToFNum();
            if (node.Token != EJson5Token.Close && nodes.TryMoveNextValueOrCloseToken(out node))
                Extends.X = node.ContentSpan.ToFNum();
            if (node.Token != EJson5Token.Close && nodes.TryMoveNextValueOrCloseToken(out node))
                Extends.Y = node.ContentSpan.ToFNum();
            if (node.Token != EJson5Token.Close && nodes.TryMoveNextValueOrCloseTokenThenClose(out node))
                Angle = node.ContentSpan.ToFNum();
            return true;
        }
    }
}
