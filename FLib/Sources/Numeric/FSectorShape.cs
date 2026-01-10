//==================={By Qcbf|qcbf@qq.com|10/23/2023 3:20:59 PM}===================

using FLib;
using System;
using System.Collections.Generic;

namespace FLib
{
    public struct FSectorShape : IEquatable<FSectorShape>, IJson5Deserializable
    {
        public FVector2 Center;
        public FNum Radius;
        public FNum OpeningAngle; // 上下边开合角度
        public FNum Angle; // 朝向旋转角度

        public readonly bool Contains(in FVector2 point) => Contains(point, FVector2.Right);

        public readonly bool Contains(in FVector2 point, FVector2 forward)
        {
            if (FVector2.SqrDistance(Center, point) >= Radius * Radius)
                return false;
            if (Angle != FNum.Zero)
                forward = FVector2.Rotate(forward, Angle * MathEx.Deg2Rad);
            return FVector2.Angle(forward, point - Center) <= OpeningAngle;
        }


        public Json5CustomDeserializeResult JsonDeserialize(ref Json5SyntaxNodes nodes, object otherData, in Json5DeserializeOptionData options)
        {
            if (nodes.TryMoveNextValueOrCloseToken(out var node))
                Radius = new FNum(node.ContentSpan);
            if (node.Token != EJson5Token.Close && nodes.TryMoveNextValueOrCloseToken(out node))
                OpeningAngle = new FNum(node.ContentSpan);
            if (node.Token != EJson5Token.Close && nodes.TryMoveNextValueOrCloseTokenThenClose(out node))
                Angle = new FNum(node.ContentSpan);
            return true;
        }

        public readonly bool Equals(FSectorShape other) => Center == other.Center && Radius == other.Radius && Angle == other.Angle;
        public readonly override bool Equals(object obj) => obj is FSectorShape shape && Equals(shape);
        public readonly override int GetHashCode() => HashCode.Combine(Center, Radius, Angle);
        public static bool operator ==(in FSectorShape left, in FSectorShape right) => left.Equals(right);
        public static bool operator !=(in FSectorShape left, in FSectorShape right) => !(left == right);
    }
}
