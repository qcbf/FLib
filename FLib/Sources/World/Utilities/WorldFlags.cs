// ==================== qcbf@qq.com | 2025-07-01 ====================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FLib.Worlds
{
    /// <summary>
    /// 上限：15个标记组，每个组27个标记
    /// </summary>
    public struct WorldFlags : IBytesSerializable, IJson5Deserializable, IJson5Serializable, IEquatable<WorldFlags>
    {
        public static readonly WorldFlags AllFlags = uint.MaxValue;
        public static readonly WorldFlags EmptyFlags = 0;
        public uint Raw;
        public readonly bool All(uint mask) => (Mask & mask) == mask;
        public readonly bool Any(uint mask) => (Mask & mask) != 0;
        public readonly bool All(in WorldFlags flags) => Group == flags.Group && (Mask & flags.Mask) == flags.Mask;
        public readonly bool Any(in WorldFlags flags) => Group == flags.Group && (Mask & flags.Mask) != 0;
        public byte Group { readonly get => (byte)(Raw & 0xf); set => Raw = (uint)(Raw & ~0xf) | (uint)(value & 0xf); }
        public uint Mask { readonly get => Raw >> 4; set => Raw = (value << 4) | Group; }
        public readonly bool IsEmpty => Mask == 0;
        public readonly override string ToString() => $"{Group}:{Mask}";
        public WorldFlags(byte group, uint mask) => Raw = (group & 0xfu) | (mask << 4);

        public Json5CustomDeserializeResult JsonDeserialize(ref Json5SyntaxNodes nodes, object otherData, in Json5DeserializeOptionData options)
        {
            if (nodes.TryMoveNextValueOrCloseToken(out var node))
            {
                var flagGroup = node.ContentCopyString.ToByte();
                if (flagGroup > 15 || flagGroup >= WorldNamedFlags.FlagNameBits.Length)
                    throw new Exception($"flag group error: {flagGroup}");
                var nameBits = WorldNamedFlags.FlagNameBits[flagGroup];
                while (nodes.TryMoveNextValueOrCloseToken(out node))
                {
                    if (!nameBits.TryGetValue(node.ContentCopyString, out var bit))
                        throw new Exception($"not found flag name: {flagGroup}>{node.ContentCopyString}");
                    Raw |= (uint)1 << bit;
                }
                Raw = Raw << 4 | flagGroup;
                return true;
            }
            return false;
        }

        public readonly string JsonSerialize(object serializeObject, object customData, int indent, Json5SerializeOptionData opData)
        {
            if (Raw == 0)
                return string.Empty;
            var allFlagNames = WorldNamedFlags.FlagGroupNames[Group];
            using var names = new PooledList<string>();
            var mask = Mask;
            for (var i = 0; mask != 0; i++)
            {
                if ((mask & 1) != 0)
                    names.Add(allFlagNames[i]);
                mask >>= 1;
            }
            return names.IsEmpty ? $"[{Group}]" : $"[{Group},{string.Join(',', names)}]";
        }

        public static implicit operator uint(in WorldFlags flags) => flags.Raw;
        public static implicit operator int(in WorldFlags flags) => (int)flags.Raw;
        public static implicit operator long(in WorldFlags flags) => flags.Raw;
        public static implicit operator ushort(in WorldFlags flags) => (ushort)flags.Raw;
        public static implicit operator short(in WorldFlags flags) => (short)flags.Raw;
        public static implicit operator WorldFlags(uint flags) => new() { Raw = flags };
        public static implicit operator WorldFlags(int flags) => new() { Raw = (uint)flags };
        public static implicit operator WorldFlags(long flags) => new() { Raw = (uint)flags };
        public readonly void Z_BytesWrite(ref BytesWriter writer) => writer.PushVInt(Raw);
        public void Z_BytesRead(ref BytesReader reader) => Raw = (uint)reader.ReadVInt();
        public bool Equals(WorldFlags other) => Raw == other.Raw;
        public override bool Equals(object obj) => obj is WorldFlags other && Equals(other);
        public override int GetHashCode() => (int)Raw;
    }

    /// <summary>
    /// 
    /// </summary>
    public static class WorldNamedFlags
    {
        // [groupId:{flagName,flagBit}]
        public static ReadOnlyDictionary<string, byte>[] FlagNameBits;

        // [[group0Flag0,group0Flag1], [group1Flag0,group1Flag1]]
        public static string[][] FlagGroupNames;

        public static bool IsInitialized => FlagNameBits != null;

        /// <summary>
        /// 
        /// </summary>
        public static byte GetFlagBit(byte group, string name)
        {
            if (group >= FlagNameBits.Length || !FlagNameBits[group].TryGetValue(name, out var bit))
                throw new Exception($"not found flag: {group}>{name}");
            return bit;
        }

        /// <summary>
        /// 
        /// </summary>
        public static WorldFlags GetFlag(byte group, string name)
        {
            return new WorldFlags(group, 1u << GetFlagBit(group, name));
        }

        /// <summary>
        /// 
        /// </summary>
        public static void Initialize(string[][] flagGroupNames)
        {
            Log.Assert(flagGroupNames.Length <= 15);
            FlagNameBits = new ReadOnlyDictionary<string, byte>[flagGroupNames.Length];
            FlagGroupNames = flagGroupNames;
            for (var group = 0; group < flagGroupNames.Length; group++)
            {
                var flagNames = flagGroupNames[group];
                if (flagNames == null)
                    continue;
                var dict = new Dictionary<string, byte>();
                for (var i = 0; i < flagNames.Length; i++)
                    dict.Add(flagNames[i], (byte)i);
                FlagNameBits[group] = new ReadOnlyDictionary<string, byte>(dict);
            }
        }
    }
}
