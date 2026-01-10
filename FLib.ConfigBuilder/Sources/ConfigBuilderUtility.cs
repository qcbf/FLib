//=================================================={By Qcbf|qcbf@qq.com|12/15/2024 5:19:37 PM}==================================================

using FLib;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FLib
{
    public static class ConfigBuilderUtility
    {
        /// <summary>
        /// 
        /// </summary>
        public static bool CheckSign(char a, char b) => a == '*' || a == 0 || b == 0 || b == '*' || a == b;

        /// <summary>
        /// 
        /// </summary>
        public static object ConvertObjectToType(object obj, Type toType)
        {
            if (obj != null && obj.GetType() != toType)
            {
                try
                {
                    obj = Convert.ChangeType(obj, toType);
                }
                catch
                {
                    obj = ConvertStringToType(obj.ToString(), toType);
                }
            }

            return obj;
        }

        /// <summary>
        ///
        /// </summary>
        public static object ConvertStringToType(string str, Type toType)
        {
            try
            {
                if (toType == typeof(byte[]))
                {
                    if (string.IsNullOrEmpty(str))
                        return Array.Empty<byte>();
                    var bytes = ArrayPool<byte>.Shared.Rent(str.Length);
                    try
                    {
                        if (Convert.TryFromBase64String(str, bytes, out var size))
                            return bytes.AsSpan(0, size).ToArray();
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(bytes);
                    }
                }

                str = str.Replace("\\n", "\n");
                if (toType == typeof(string))
                    return str;
                try
                {
                    return toType.IsArray && !str.StartsWith('[') ? Json5.Deserialize($"[{str}]", toType) : Json5.Deserialize(str, toType);
                }
                catch (Exception jsonEx)
                {
                    try
                    {
                        return SplitArray(toType.GetElementType(), str);
                    }
                    catch (Exception e)
                    {
                        Log.Error?.Write($"{str} {toType} {jsonEx}\n{e}");
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{str} {toType}", ex);
            }
        }

        /// <summary>
        /// 特殊分割字符串, 分别用 ,#| 作为数组每一层的分隔符如<code> [[[1,2],[3]],[[4]]] 等价 1,2#3|4</code>
        /// </summary>
        private static Array SplitArray(Type elementType, string raw, int arrayDepCountAddition = 0)
        {
            var arrayDepCount = GetArrayDepCount(elementType);
            Span<char> splitChars1 = stackalloc char[] { ',', '#', '|' };
            Span<char> splitChars2 = stackalloc char[] { '/', '#', '|' };
            var splitChar = splitChars1[arrayDepCount + arrayDepCountAddition];
            var splitChar2 = splitChars2[arrayDepCount + arrayDepCountAddition];

            var list = (IList)TypeAssistant.New(typeof(List<>).MakeGenericType(elementType));
            var strbuf = StringFLibUtility.GetStrBuf();
            for (var i = 0; i < raw.Length; i++)
            {
                var c = raw[i];
                if (c == '\\')
                    strbuf.Append(raw[++i]);
                else if (c == splitChar || c == splitChar2)
                    ParseStrbuf();
                else
                    strbuf.Append(c);
            }

            if (strbuf.Length > 0)
            {
                ParseStrbuf();
            }

            StringFLibUtility.ReleaseStrBuf(strbuf);
            var arr = Array.CreateInstance(elementType, list.Count);
            list.CopyTo(arr, 0);
            return arr;

            void ParseStrbuf()
            {
                var str = strbuf.ToString();
                if (string.IsNullOrWhiteSpace(str)) return;
                if (arrayDepCount == 0)
                {
                    list.Add(ConvertStringToType(str, elementType));
                    strbuf.Clear();
                }
                else
                {
                    list.Add(SplitArray(elementType.GetElementType(), str));
                    strbuf.Clear();
                }
            }

            static int GetArrayDepCount(Type t)
            {
                var v = 0;
                while (t!.IsArray)
                {
                    v++;
                    t = t.GetElementType();
                }

                return v;
            }
        }
    }
}
