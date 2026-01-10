//==================={By Qcbf|qcbf@qq.com|5/28/2021 7:09:13 PM}===================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FLib
{
    public class StringFLibUtility
    {
        public static readonly UTF8Encoding Encoding = new(false);
        public static string WordTenThousand = "w";

        [ThreadStatic]
        private static StringBuilder _strBuf1;

        [ThreadStatic]
        private static StringBuilder _strBuf2;

        /// <summary>
        /// 创建一个临时字符串缓冲对象
        /// </summary>
        public static StringBuilder GetStrBuf(int capacity = 360)
        {
            if (capacity > 360) return new StringBuilder(capacity);
            ref var buf = ref _strBuf1;
            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if (buf == null) buf = ref _strBuf2;
            if (buf == null) return new StringBuilder(capacity);
            if (buf.Capacity < capacity)
                buf.EnsureCapacity(capacity);
            buf.Clear();
            var temp = buf;
            buf = null;
            return temp;
        }

        /// <summary>
        /// 释放strbuf缓存
        /// </summary>
        public static string ReleaseStrBufAndResult(StringBuilder target)
        {
            var temp = target.ToString();
            ReleaseStrBuf(target);
            return temp;
        }

        /// <summary>
        /// 释放strbuf缓存
        /// </summary>
        public static void ReleaseStrBuf(StringBuilder target)
        {
            if (target.Capacity > 360) return;
            if (_strBuf1 == null)
            {
                _strBuf1 = target;
            }
            else if (_strBuf2 == null)
            {
                _strBuf2 = target;
            }
        }


        /// <summary>
        ///
        /// </summary>
        public static bool UTF8Checker(byte[] buffer, int length = 0)
        {
            if (length <= 0)
            {
                length = buffer.Length;
            }

            var position = 0;
            var bytes = 0;
            while (position < length)
            {
                if (!UTF8Checker(buffer, position, length, ref bytes))
                {
                    return false;
                }

                position += bytes;
            }

            return true;
        }

        /// <summary>
        /// 检查是否utf8编码
        /// </summary>
        public static bool UTF8Checker(byte[] buffer, int position, int length, ref int bytes)
        {
            #region utf8 checker

            if (length > buffer.Length)
            {
                throw new ArgumentException("Invalid length");
            }

            if (position > length - 1)
            {
                bytes = 0;
                return true;
            }

            var ch = buffer[position];

            if (ch <= 0x7F)
            {
                bytes = 1;
                return true;
            }

            if (ch >= 0xc2 && ch <= 0xdf)
            {
                if (position >= length - 2)
                {
                    bytes = 0;
                    return false;
                }

                if (buffer[position + 1] < 0x80 || buffer[position + 1] > 0xbf)
                {
                    bytes = 0;
                    return false;
                }

                bytes = 2;
                return true;
            }

            if (ch == 0xe0)
            {
                if (position >= length - 3)
                {
                    bytes = 0;
                    return false;
                }

                if (buffer[position + 1] < 0xa0 || buffer[position + 1] > 0xbf ||
                    buffer[position + 2] < 0x80 || buffer[position + 2] > 0xbf)
                {
                    bytes = 0;
                    return false;
                }

                bytes = 3;
                return true;
            }


            if (ch >= 0xe1 && ch <= 0xef)
            {
                if (buffer[position + 1] < 0x80 || buffer[position + 1] > 0xbf ||
                    buffer[position + 2] < 0x80 || buffer[position + 2] > 0xbf)
                {
                    bytes = 0;
                    return false;
                }

                bytes = 3;
                return true;
            }

            if (ch == 0xf0)
            {
                if (position >= length - 4)
                {
                    bytes = 0;
                    return false;
                }

                if (buffer[position + 1] < 0x90 || buffer[position + 1] > 0xbf ||
                    buffer[position + 2] < 0x80 || buffer[position + 2] > 0xbf ||
                    buffer[position + 3] < 0x80 || buffer[position + 3] > 0xbf)
                {
                    bytes = 0;
                    return false;
                }

                bytes = 4;
                return true;
            }

            if (ch == 0xf4)
            {
                if (position >= length - 4)
                {
                    bytes = 0;
                    return false;
                }

                if (buffer[position + 1] < 0x80 || buffer[position + 1] > 0x8f ||
                    buffer[position + 2] < 0x80 || buffer[position + 2] > 0xbf ||
                    buffer[position + 3] < 0x80 || buffer[position + 3] > 0xbf)
                {
                    bytes = 0;
                    return false;
                }

                bytes = 4;
                return true;
            }

            if (ch >= 0xf1 && ch <= 0xf3)
            {
                if (position >= length - 4)
                {
                    bytes = 0;
                    return false;
                }

                if (buffer[position + 1] < 0x80 || buffer[position + 1] > 0xbf ||
                    buffer[position + 2] < 0x80 || buffer[position + 2] > 0xbf ||
                    buffer[position + 3] < 0x80 || buffer[position + 3] > 0xbf)
                {
                    bytes = 0;
                    return false;
                }

                bytes = 4;
                return true;
            }

            return false;

            #endregion
        }

        /// <summary>
        /// 首字母到大小
        /// </summary>
        public static string FirstCharUpper(string str)
        {
            return char.ToUpper(str[0]) + str[1..];
        }

        /// <summary>
        /// 字符串转换为FNV1AHash，适合较短的字符串，性能更好，比如名称这些
        /// </summary>
        public static int ShortStringToHash(in ReadOnlySpan<char> text)
        {
            const uint seed = 0x811c9dc5;
            const uint fnvPrime = 16777619;
            var hash = seed;
            var len = text.Length;
            for (var i = 0; i < len; i++)
            {
                hash ^= text[i];
                hash *= fnvPrime;
            }

            unchecked
            {
                return (int)hash;
            }
        }

        /// <summary>
        /// 字符串转换为Murmurhash，性能低点，但是冲突概率更小
        /// </summary>
        public static int LongTextToHash(in ReadOnlySpan<char> data)
        {
            const uint c1 = 0xcc9e2d51;
            const uint c2 = 0x1b873593;
            const int r1 = 15;
            const int r2 = 13;
            const uint m = 5;
            const uint n = 0xe6546b64;

            var hash = 0u;
            var len = data.Length * sizeof(char);
            var index = 0;

            while (len >= 4)
            {
                var k = data[index / 2] | ((uint)data[(index / 2) + 1] << 16);
                k *= c1;
                k = (k << r1) | (k >> (32 - r1));
                k *= c2;

                hash ^= k;
                hash = (hash << r2) | (hash >> (32 - r2));
                hash = hash * m + n;

                index += 4;
                len -= 4;
            }

            if (len > 0)
            {
                uint k = 0;
                for (var i = 0; i < len / sizeof(char); i++)
                {
                    k |= (uint)data[index / 2 + i] << (8 * i * sizeof(char));
                }

                k *= c1;
                k = (k << r1) | (k >> (32 - r1));
                k *= c2;
                hash ^= k;
            }

            hash ^= (uint)data.Length * sizeof(char);
            hash ^= hash >> 16;
            hash *= 0x85ebca6b;
            hash ^= hash >> 13;
            hash *= 0xc2b2ae35;
            hash ^= hash >> 16;

            return (int)hash;
        }

        /// <summary>
        /// 截除溢出字符添加省略符号
        /// </summary>
        /// <param name="maxLength"></param>
        /// <param name="ellipsisPosition">省略号位置:-1:开头, 0:中间, 1:结尾</param>
        /// <param name="str"></param>
        public static string LimitLength(string str, int maxLength, sbyte ellipsisPosition = 1)
        {
            const string ellipsisChars = "...";
            str ??= string.Empty;
            if (str.Length <= maxLength) return str;
            switch (ellipsisPosition)
            {
                case -1:
                    str = ellipsisChars + str[^maxLength..];
                    break;
                case 0:
                    maxLength = Math.Max(1, maxLength >> 2);
                    str = str[..maxLength] + ellipsisChars + str[^maxLength..];
                    break;
                case 1:
                    str = str[..maxLength] + ellipsisChars;
                    break;
            }

            return str;
        }

        /// <summary>
        ///
        /// </summary>
        public static ReadOnlySpan<char> SegmentTextRead(ReadOnlySpan<char> text, string breaks = ",/|+")
        {
            return SegmentTextReadWithMoveNext(ref text, breaks);
        }

        /// <summary>
        ///
        /// </summary>
        public static ReadOnlySpan<char> SegmentTextReadWithMoveNext(
#if NET8_0_OR_GREATER
            scoped
#endif
            ref ReadOnlySpan<char> text, string breaks = "+-")
        {
            var count = 0;
            var resultCountOffset = 0;
            while (!text.IsEmpty && count < text.Length)
            {
                var c = text[count++];
                if (!breaks.Contains(c)) continue;
                ++resultCountOffset;
                break;
            }

            if (count == 0)
            {
                return ReadOnlySpan<char>.Empty;
            }

            var result = text[..(count - resultCountOffset)];
            text = text[count..];
            return result;
        }
    }
}
