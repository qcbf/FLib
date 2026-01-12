// ==================== qcbf@qq.com | 2026-01-10 ====================

using System;

namespace FLib.WorldCores
{
    public readonly struct IncrementId : IEquatable<IncrementId>
    {
        public readonly ushort Raw;
        public ushort Id => (ushort)(Raw - 1);
        public bool IsEmpty => Raw == 0;
        public IncrementId(ushort raw) => Raw = raw;
        public IncrementId(int raw) => Raw = checked((ushort)raw);
        public override string ToString() => Id.ToString();
        public static implicit operator ushort(in IncrementId id) => id.Id;
        public static implicit operator int(in IncrementId id) => id.Id;
        public bool Equals(IncrementId other) => Raw == other.Raw;
        public override bool Equals(object obj) => obj is IncrementId other && Equals(other);
        public override int GetHashCode() => Raw.GetHashCode();
        public static bool operator ==(IncrementId left, IncrementId right) => left.Raw == right.Raw;
        public static bool operator !=(IncrementId left, IncrementId right) => !(left == right);
        public static bool operator >(IncrementId left, IncrementId right) => left.Raw > right.Raw;
        public static bool operator <(IncrementId left, IncrementId right) => left.Raw < right.Raw;
    }
}