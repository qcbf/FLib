//==================={By Qcbf|qcbf@qq.com|3/14/2022 11:27:53 AM}===================

using FLib;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FLib
{
    public static class CRC32
    {
        private static readonly uint[] mLookup;

        static CRC32()
        {
            mLookup = new uint[256];
            for (var i = 0; i < 256; i++)
            {
                var val = (uint)i;
                for (var j = 0; j < 8; j++)
                {
                    if ((val & 0b0000_0001) == 0)
                    {
                        val >>= 1;
                    }
                    else
                    {
                        val = (val >> 1) ^ 0xEDB88320u;
                    }
                }

                mLookup[i] = val;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static uint Encode(in ReadOnlySpan<char> source, uint crcMask = byte.MaxValue, uint crcXor = 0, int step = 1)
        {
            for (var i = 0; i < source.Length; i += step)
            {
                crcMask = mLookup[(byte)crcMask ^ (source[i] >> 8)] ^ (crcMask >> 8) ^ crcXor;
                crcMask = mLookup[(byte)crcMask ^ (source[i] & 0xff)] ^ (crcMask >> 8) ^ crcXor;
            }

            return crcMask;
        }

        /// <summary>
        /// 
        /// </summary>
        public static uint Encode(ReadOnlySpan<byte> source, uint crcMask = byte.MaxValue, uint crcXor = 0, int step = 1)
        {
            for (var i = 0; i < source.Length; i += step)
                crcMask = mLookup[(byte)crcMask ^ source[i]] ^ (crcMask >> 8) ^ crcXor;
            return crcMask;
        }
    }
}
