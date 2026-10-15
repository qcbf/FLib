// // ==================== qcbf@qq.com | 2026-09-15 ====================

using System;
using System.Buffers;

namespace FLib
{
    /// <summary>在二进制数据与 Base91 文本之间进行编码和解码。</summary>
    public static class Base91
    {
        private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!#$%&()*+,.-:;<=>'@[]^_`{|}~\"";
        private static readonly sbyte[] DecodeTable = CreateDecodeTable();

        private static sbyte[] CreateDecodeTable()
        {
            var table = new sbyte[256];
            Array.Fill(table, (sbyte)-1);
            for (var i = 0; i < Alphabet.Length; i++) table[Alphabet[i]] = (sbyte)i;
            return table;
        }

        /// <summary>计算编码指定长度数据所需的最大输出字符数。</summary>
        public static int GetMaxEncodedLength(int dataLength) => (dataLength * 16 + 12) / 13;

        /// <summary>计算解码指定长度文本所需的最大输出字节数（上界）。</summary>
        public static int GetMaxDecodedLength(int textLength) => textLength * 14 / 16 + 2;

        /// <summary>使用 Base91 编码指定的二进制数据。</summary>
        /// <param name="data">要编码的二进制数据。</param>
        /// <returns>Base91 编码后的字符串。</returns>
        public static string Encode(ReadOnlySpan<byte> data)
        {
            var maxLen = GetMaxEncodedLength(data.Length);
            var buffer = ArrayPool<char>.Shared.Rent(maxLen);
            try
            {
                var written = Encode(data, buffer);
                return new string(buffer, 0, written);
            }
            finally
            {
                ArrayPool<char>.Shared.Return(buffer);
            }
        }

        /// <summary>使用 Base91 编码指定的二进制数据，写入调用方提供的缓冲区。</summary>
        /// <param name="data">要编码的二进制数据。</param>
        /// <param name="destination">输出缓冲区，长度至少为 <see cref="GetMaxEncodedLength"/>。</param>
        /// <returns>实际写入的字符数。</returns>
        /// <exception cref="ArgumentException">destination 容量不足。</exception>
        public static int Encode(ReadOnlySpan<byte> data, Span<char> destination)
        {
            if (destination.Length < GetMaxEncodedLength(data.Length))
                throw new ArgumentException("Destination buffer is too small.", nameof(destination));

            var outputCount = 0;
            ulong bitQueue = 0;
            var bitCount = 0;

            foreach (var b in data)
            {
                bitQueue |= (ulong)b << bitCount;
                bitCount += 8;

                while (bitCount > 13)
                {
                    var value = bitQueue & 8191;
                    var bits = 13;
                    if (value <= 88)
                    {
                        value = bitQueue & 16383;
                        bits = 14;
                    }

                    bitQueue >>= bits;
                    bitCount -= bits;
                    destination[outputCount++] = Alphabet[(int)(value % 91)];
                    destination[outputCount++] = Alphabet[(int)(value / 91)];
                }
            }

            if (bitCount <= 0) return outputCount;
            destination[outputCount++] = Alphabet[(int)(bitQueue % 91)];
            if (bitCount > 7 || bitQueue > 90)
                destination[outputCount++] = Alphabet[(int)(bitQueue / 91)];

            return outputCount;
        }

        /// <summary>将 Base91 文本解码为原始二进制数据。遇到字母表之外的字符会抛出异常。</summary>
        /// <param name="text">要解码的 Base91 编码文本。</param>
        /// <returns>解码后的二进制数据。</returns>
        /// <exception cref="FormatException">text 包含非 Base91 字母表字符。</exception>
        public static byte[] Decode(ReadOnlySpan<char> text)
        {
            var maxLen = GetMaxDecodedLength(text.Length);
            var buffer = ArrayPool<byte>.Shared.Rent(maxLen);
            try
            {
                var written = Decode(text, buffer);
                return buffer.AsSpan(0, written).ToArray();
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        /// <summary>将 Base91 文本解码为原始二进制数据，写入调用方提供的缓冲区。遇到字母表之外的字符会抛出异常。</summary>
        /// <param name="text">要解码的 Base91 编码文本。</param>
        /// <param name="destination">输出缓冲区，长度至少为 <see cref="GetMaxDecodedLength"/>。</param>
        /// <returns>实际写入的字节数。</returns>
        /// <exception cref="ArgumentException">destination 容量不足。</exception>
        /// <exception cref="FormatException">text 包含非 Base91 字母表字符。</exception>
        public static int Decode(ReadOnlySpan<char> text, Span<byte> destination)
        {
            if (destination.Length < GetMaxDecodedLength(text.Length))
                throw new ArgumentException("Destination buffer is too small.", nameof(destination));

            var resultCount = 0;
            ulong bitQueue = 0;
            var bitCount = 0;
            var value = -1;

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                var idx = c <= byte.MaxValue ? DecodeTable[c] : (sbyte)-1;
                if (idx < 0)
                    throw new FormatException($"Invalid Base91 character '{c}' at index {i}.");

                if (value < 0)
                {
                    value = idx;
                    continue;
                }

                value += idx * 91;
                bitQueue |= (ulong)value << bitCount;
                bitCount += (value & 8191) > 88 ? 13 : 14;

                do
                {
                    destination[resultCount++] = (byte)bitQueue;
                    bitQueue >>= 8;
                    bitCount -= 8;
                } while (bitCount > 7);

                value = -1;
            }

            if (value >= 0)
                destination[resultCount++] = (byte)(bitQueue | (ulong)value << bitCount);

            return resultCount;
        }
    }
}
