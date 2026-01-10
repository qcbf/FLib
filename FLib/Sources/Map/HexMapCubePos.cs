//==================={By Qcbf|qcbf@qq.com|2/16/2022 8:48:39 PM}===================

using FLib;
using System;
using System.Collections.Generic;

namespace FLib
{
    public struct HexMapCubePos : IEquatable<HexMapCubePos>
    {
        public int X;
        public int Y;
        public int Z;
        public HexMapCubePos(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public HexMapCubePos(FVector2Int pos)
        {
            X = pos.X - pos.Y / 2;
            Y = pos.Y;
            Z = -X - Y;
        }
        public readonly bool Equals(HexMapCubePos other) => X == other.X && Y == other.Y && Z == other.Z;
        public readonly override int GetHashCode() => (X, Y, Z).GetHashCode();
        public readonly override bool Equals(object obj) => (obj is HexMapCubePos cubePos) && cubePos == this;
        public readonly override string ToString() => X + "," + Y + "," + Z;
        public static bool operator ==(in HexMapCubePos a, in HexMapCubePos b) => a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        public static bool operator !=(in HexMapCubePos a, in HexMapCubePos b) => a.X != b.X || a.Y != b.Y || a.Z != b.Z;
        public static implicit operator FVector2Int(in HexMapCubePos pos) => new(pos.X + pos.Y / 2, pos.Y);

    }
}
