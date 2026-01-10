// ==================== qcbf@qq.com |2025-12-16 ====================

using System;
using System.Collections.Generic;
using System.Numerics;

namespace FLib
{
    public static class BitArrayOperator
    {
        public const int BitSize = sizeof(ulong) * 8;

        public static int GetBitsLength(int count)
        {
            return (int)Math.Ceiling(count / (float)BitSize);
        }

        public static void SetAllBits(in Span<ulong> bits, bool value)
        {
            bits.Fill(value ? ulong.MaxValue : ulong.MinValue);
        }

        public static bool GetBit(in Span<ulong> bits, int index)
        {
            return (bits[index / BitSize] & (0x1ul << (index % BitSize))) > 0;
        }

        public static void SetBit(in Span<ulong> bits, int index, bool value)
        {
            if (value)
                bits[index / BitSize] |= 0x1ul << (index % BitSize);
            else
                bits[index / BitSize] &= ~(0x1ul << (index % BitSize));
        }

        public static void Union(in Span<ulong> bits, in Span<ulong> targetBits)
        {
            if (bits.Length != targetBits.Length)
                throw new ArgumentException("Bitmaps must be of equal length to union them.");
            for (var index = 0; index < bits.Length; index++)
                bits[index] |= targetBits[index];
        }

        public static void Intersect(in Span<ulong> bits, in Span<ulong> targetBits)
        {
            if (bits.Length != targetBits.Length)
                throw new ArgumentException("Bitmaps must be of equal length to intersect them.");
            for (var index = 0; index < bits.Length; index++)
                bits[index] &= targetBits[index];
        }

        public static void Invert(in Span<ulong> bits)
        {
            for (var index = 0; index < bits.Length; index++)
                bits[index] = ~bits[index];
        }

        public static void SetRange(in Span<ulong> bits, int start, int end, bool value)
        {
            if (start > end)
                throw new ArgumentException("Range is inverted.", nameof(end));

            if (start == end)
            {
                SetBit(bits, start, value);
                return;
            }

            var startBucket = start / BitSize;
            var startOffset = start % BitSize;
            var endBucket = end / BitSize;
            var endOffset = end % BitSize;

            if (value)
                bits[startBucket] |= ulong.MaxValue << startOffset;
            else
                bits[startBucket] &= ~(ulong.MaxValue << startOffset);

            for (var bucketIndex = startBucket + 1; bucketIndex < endBucket; bucketIndex++)
                bits[bucketIndex] = value ? ulong.MaxValue : ulong.MinValue;

            if (value)
                bits[endBucket] |= ulong.MaxValue >> (BitSize - endOffset - 1);
            else
                bits[endBucket] &= ~(ulong.MaxValue >> (BitSize - endOffset - 1));
        }

        public static HashSet<int> GetTrueBits(in Span<ulong> bits)
        {
            var trueBits = new HashSet<int>();
            var length = bits.Length * BitSize;
            for (var bucketIndex = 0; bucketIndex < bits.Length; bucketIndex++)
            {
                var bucket = bits[bucketIndex];
                var bitIndex = 0;
                while (bucket > 0 && bucketIndex * BitSize + bitIndex < length)
                {
                    if ((bucket & 0x1) > 0)
                    {
                        trueBits.Add((bucketIndex * BitSize) + bitIndex);
                    }

                    bucket >>= 1;
                    bitIndex++;
                }
            }

            return trueBits;
        }

        /// <summary>
        /// 
        /// </summary>
        public static bool MaskAll(in Span<ulong> bits, in Span<ulong> mask)
        {
            for (var i = 0; i < mask.Length; i++)
            {
                if (bits.Length <= i || (bits[i] & mask[i]) != mask[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public static bool MaskAny(in Span<ulong> bits, in Span<ulong> mask)
        {
            for (var i = 0; i < mask.Length; i++)
            {
                if (bits.Length > i && (bits[i] & mask[i]) != 0)
                    return true;
            }

            return false;
        }
    }
}