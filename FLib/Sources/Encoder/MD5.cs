//==================={By Qcbf|qcbf@qq.com}===================

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FLib;

namespace FLib
{
    public static class MD5
    {
        public static string Encode(Stream v)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            var result = md5.ComputeHash(v);
            var strbuf = StringFLibUtility.GetStrBuf();
            for (var i = 0; i < result.Length; i++)
            {
                strbuf.Append(result[i].ToString("x2"));
            }
            return strbuf.ToString();
        }

        public static string Encode(in ReadOnlySpan<byte> buffer)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            Span<byte> result = stackalloc byte[16];
            if (md5.TryComputeHash(buffer, result, out var count))
            {
                var strbuf = StringFLibUtility.GetStrBuf();
                for (var i = 0; i < result.Length; i++)
                {
                    strbuf.Append(result[i].ToString("x2"));
                }
                return strbuf.ToString();
            }
            throw new Exception();
        }

        public static string Encode(string str)
        {
            var size = StringFLibUtility.Encoding.GetByteCount(str);
            if (size < 2048)
            {
                Span<byte> buffer = stackalloc byte[size];
                StringFLibUtility.Encoding.GetBytes(str, buffer);
                return Encode(buffer);
            }
            return Encode(StringFLibUtility.Encoding.GetBytes(str));
        }

        public static string EncodeFile(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }
            using var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return Encode(fs);
        }
    }
}
