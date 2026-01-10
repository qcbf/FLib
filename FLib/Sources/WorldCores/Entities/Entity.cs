// ==================== qcbf@qq.com |2025-12-11 ====================

using System;
using System.Runtime.CompilerServices;

namespace FLib.WorldCores
{
    [SkipLocalsInit]
    public readonly struct Entity : IEquatable<Entity>
    {
        public readonly ushort Id;
        public readonly ushort Version;
        public bool IsEmpty => Version == 0;

        public Entity(ushort id, ushort version)
        {
            Id = id;
            Version = version;
        }

        public bool Equals(Entity other) => this == other;
        public override bool Equals(object obj) => obj is Entity other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Id, Version);
        public static bool operator ==(in Entity left, in Entity right) => left.Id == right.Id && left.Version == right.Version;
        public static bool operator !=(in Entity left, in Entity right) => left.Id != right.Id || left.Version != right.Version;
    }
}