//==================={By Qcbf|qcbf@qq.com|12/28/2021 5:35:48 PM}===================

using FLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace FLib
{
    /// <summary>
    /// 32 位版本号，依次存储 8 位 major、11 位 minor 和 13 位 patch。
    /// 版本号上限为 <c>255.2047.8191</c>。
    /// </summary>
    public struct FVersion : IJson5Deserializable, IJson5Serializable
    {
        private const int PatchBits = 13;
        private const int MinorBits = 11;
        private const int MajorShift = PatchBits + MinorBits;
        private const int PatchMask = (1 << PatchBits) - 1;
        private const int MinorMask = (1 << MinorBits) - 1;

        private int _value;

        public byte Major
        {
            readonly get => (byte)(_value >> MajorShift);
            set => _value = (value << MajorShift) | (_value & ((1 << MajorShift) - 1));
        }

        public ushort Minor
        {
            readonly get => (ushort)((_value >> PatchBits) & MinorMask);
            set
            {
                Log.Assert(value <= MinorMask);
                _value = (value << PatchBits) | (_value & ~(MinorMask << PatchBits));
            }
        }

        public ushort Patch
        {
            readonly get => (ushort)(_value & PatchMask);
            set
            {
                Log.Assert(value <= PatchMask);
                _value = value | (_value & ~PatchMask);
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
                    versions[versionIndex++] = new Range(start, i);
                    start = (byte)(i + 1);
                }
            }

            _value = (versionChars[versions[0]].ToByte() << MajorShift) | (versionChars[versions[1]].ToUShort() << PatchBits) | versionChars[start..].ToUShort();
        }

        public FVersion(byte major, ushort minor, ushort patch)
        {
            _value = (major << MajorShift) | (minor << PatchBits) | patch;
        }

        /// <summary>
        /// 按现有位宽增加版本号，低位溢出时自动进位。
        /// </summary>
        /// <exception cref="OverflowException">进位后的主版本号超过 255。</exception>
        public readonly FVersion SafeAdd(uint major, uint minor, uint patch)
        {
            var packedSum = unchecked((uint)_value) + ((ulong)major << MajorShift) + ((ulong)minor << PatchBits) + patch;
            if (packedSum > uint.MaxValue)
                throw new OverflowException("The major version cannot exceed 255.");
            return unchecked((int)packedSum);
        }

        public readonly override string ToString()
            => Major.ToString() + '.' + Minor + '.' + Patch;

        readonly bool IJson5Serializable.JsonSerialize(StringBuilder jsonText, object serializeObject, object customData, int indent, Json5SerializeOptionData opData)
        {
            Json5Serializer.PushValue(ToString(), jsonText, indent, opData);
            return true;
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

        public readonly override bool Equals(object obj) => obj is FVersion ver && ver._value == _value;
        public readonly override int GetHashCode() => _value.GetHashCode();

        public static bool operator ==(FVersion left, FVersion right) => left._value == right._value;
        public static FVersion operator +(FVersion left, FVersion right) => new((byte)(left.Major + right.Major), (ushort)(left.Minor + right.Minor), (ushort)(left.Patch + right.Patch));
        public static FVersion operator -(FVersion left, FVersion right) => new((byte)(left.Major - right.Major), (ushort)(left.Minor - right.Minor), (ushort)(left.Patch - right.Patch));
        public static bool operator !=(FVersion left, FVersion right) => left._value != right._value;

        public static implicit operator int(FVersion v) => v._value;
        public static implicit operator FVersion(int v) => new() { _value = v };
        public static implicit operator Version(FVersion v) => new(v.Major, v.Minor, v.Patch);
    }
}