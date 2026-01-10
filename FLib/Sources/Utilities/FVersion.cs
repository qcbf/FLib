//==================={By Qcbf|qcbf@qq.com|12/28/2021 5:35:48 PM}===================

using FLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace FLib
{
    /// <summary>
    /// 轻巧的version结构
    /// note:   max value 
    /// <c>[major,255,8bit] | [minor,1023,10bit] | [patch,16383,14bit]</c>
    /// </summary>
    public struct FVersion : IJson5Deserializable
    {
        private int mValue;

        public byte Major
        {
            readonly get => (byte)(mValue >> 24);
            set => mValue = (value << 24) | (mValue & 0x00ffffff);
        }

        public ushort Minor
        {
            readonly get => (ushort)((mValue & 0xffc000) >> 14);
            set
            {
                Log.Assert(value <= 1023);
                mValue = value << 14 | (int)(mValue & 0xff003fff);
            }
        }

        public ushort Patch
        {
            readonly get => (ushort)(mValue & 0x3fff);
            set
            {
                Log.Assert(value <= 16383);
                mValue = value | (int)(mValue & 0xffffc000);
            }
        }

        public FVersion(ReadOnlySpan<char> versionChars)
        {
            Span<Range> versions = stackalloc Range[2];
            byte versionIndex = 0;
            byte start = 0;
            for (byte i = 0; i < versionChars.Length; i++)
            {
                if (versionChars[i] == '.')
                {
                    versions[versionIndex++] = new Range(start, i - 1);
                    start = (byte)(i + 1);
                }
            }
            mValue = (versionChars[versions[0]].ToByte() << 24) | (versionChars[versions[1]].ToUShort() << 14) | versionChars[start..].ToUShort();
        }

        public FVersion(byte major, ushort minor, ushort patch)
        {
            mValue = (major << 24) | (minor << 14) | patch;
        }

        public readonly Version ConvertSystemVersion()
        {
            return new Version(Major, Minor, Patch);
        }

        public readonly override string ToString()
        {
            return Major.ToString() + '.' + Minor + '.' + Patch;
        }

        public Json5CustomDeserializeResult JsonDeserialize(ref Json5SyntaxNodes nodes, object otherData, in Json5DeserializeOptionData options)
        {
            var node = nodes.MoveNext(EJson5Token.Value | EJson5Token.ArrayOpen);
            if (node.Token == EJson5Token.Value)
            {
                this = new FVersion(node.ContentSpan);
                return true;
            }
            Major = node.ContentSpan.ToByte();
            if (node.Token != EJson5Token.Close && nodes.TryMoveNextValueOrCloseToken(out node))
                Minor = node.ContentSpan.ToUShort();
            if (node.Token != EJson5Token.Close && nodes.TryMoveNextValueOrCloseTokenThenClose(out node))
                Patch = node.ContentSpan.ToUShort();
            return true;
        }

        public readonly override bool Equals(object obj) => obj is FVersion ver && ver.mValue == mValue;
        public readonly override int GetHashCode() => mValue.GetHashCode();

        public static bool operator ==(FVersion left, FVersion right) => left.mValue == right.mValue;
        public static FVersion operator +(FVersion left, FVersion right) => new((byte)(left.Major + right.Major), (ushort)(left.Minor + right.Minor), (ushort)(left.Patch + right.Patch));
        public static FVersion operator -(FVersion left, FVersion right) => new((byte)(left.Major - right.Major), (ushort)(left.Minor - right.Minor), (ushort)(left.Patch - right.Patch));
        public static bool operator !=(FVersion left, FVersion right) => left.mValue != right.mValue;

        public static implicit operator int(FVersion v) => v.mValue;
        public static implicit operator FVersion(int v) => new() { mValue = v };
    }
}
