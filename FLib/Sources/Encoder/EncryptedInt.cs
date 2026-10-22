//==================={By Qcbf|qcbf@qq.com|5/9/2023 3:18:21 PM}===================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using FLib;

namespace FLib
{
    [Serializable]
    public struct EncryptedInt
    {
        private const int RotateBits = 5;

        private int _value;
        private int _key;

        private static uint _seed;

        public EncryptedInt(int v)
        {
            _key = NextKey();
            _value = RotL(v, RotateBits) ^ _key;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int v)
        {
            var key = NextKey();
            _key = key;
            _value = RotL(v, RotateBits) ^ key;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int Get() => RotR(_value ^ _key, RotateBits);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(int v) => Set(unchecked(Get() + v));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sub(int v) => Set(unchecked(Get() - v));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Refresh() => Set(Get());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override string ToString() => Get().ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(in EncryptedInt v) => v.Get();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator EncryptedInt(int v) => new(v);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EncryptedInt operator +(in EncryptedInt a, int b) => new(unchecked(a.Get() + b));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EncryptedInt operator -(in EncryptedInt a, int b) => new(unchecked(a.Get() - b));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EncryptedInt operator ++(EncryptedInt a) => new(unchecked(a.Get() + 1));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EncryptedInt operator --(EncryptedInt a) => new(unchecked(a.Get() - 1));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int NextKey()
        {
            var s = _seed;
            if (s == 0) s = (uint)Stopwatch.GetTimestamp() | 1u;
            s ^= s << 13;
            s ^= s >> 17;
            s ^= s << 5;
            _seed = s;
            return (int)s;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int RotL(int x, int n) => (int)(((uint)x << n) | ((uint)x >> (32 - n)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int RotR(int x, int n) => (int)(((uint)x >> n) | ((uint)x << (32 - n)));
    }
}