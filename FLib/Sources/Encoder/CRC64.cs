//==================={By Qcbf|qcbf@qq.com|3/14/2022 11:27:53 AM}===================

using FLib;
using System;
using System.Collections.Generic;

namespace FLib
{
    public static class CRC64
    {
        private static readonly ulong[] mLookup;

        static CRC64()
        {
            mLookup = new ulong[256];
            for (var i = 0; i < 256; i++)
            {
                var val = (ulong)i << 56;

                for (var j = 0; j < 8; j++)
                {
                    if ((val & 0x8000_0000_0000_0000) == 0)
                    {
                        val <<= 1;
                    }
                    else
                    {
                        val = (val << 1) ^ 0x42F0E1EBA9EA3693;
                    }
                }
                mLookup[i] = val;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static ulong Encode(in string value)
        {
            var bytesCount = StringFLibUtility.Encoding.GetByteCount(value);
            if (bytesCount < 4096)
            {
                Span<byte> buffer = stackalloc byte[bytesCount];
                StringFLibUtility.Encoding.GetBytes(value, buffer);
                return Encode(buffer);
            }
            return Encode(StringFLibUtility.Encoding.GetBytes(value));
        }

        /// <summary>
        /// 
        /// </summary>
        public static ulong Encode(ReadOnlySpan<byte> source, ulong crc = uint.MaxValue)
        {
            for (var i = 0; i < source.Length; i++)
            {
                var idx = (crc >> 56);
                idx ^= source[i];
                crc = mLookup[idx] ^ (crc << 8);
            }
            return crc;
        }



    }
}
