//==================={By Qcbf|qcbf@qq.com}===================

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;

namespace FLib
{
    public static class FIO
    {
        /// <summary>
        /// 当前基础目录末尾+/
        /// </summary>
        public static string CurrentBaseDirectory => AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// 当前工作目录末尾没有/
        /// </summary>
        public static string CurrentWorkDirectory => Environment.CurrentDirectory;

        /// <summary>
        /// 创建目录
        /// </summary>
        public static void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path!);
            }
        }

        /// <summary>
        /// 清除目录
        /// </summary>
        public static void ClearDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                CreateDirectory(path);
            }
            else
            {
                foreach (var item in Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly))
                {
                    File.Delete(item);
                }
                foreach (var item in Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly))
                {
                    Directory.Delete(item, true);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static bool TryDeleteFile(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    File.Delete(path);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// 复制目录
        /// </summary>
        public static void CopyDirectory(string src, string dest, string searchPattern = "*", Func<string, bool, bool> filter = null, Action<string, string> copyHandler = null)
        {
            if (!Directory.Exists(dest))
            {
                CreateDirectory(dest);
            }
            foreach (var item in Directory.GetFiles(src, searchPattern, SearchOption.TopDirectoryOnly))
            {
                if (filter?.Invoke(item, false) != false)
                {
                    var fileName = Path.GetFileName(item);
                    if (copyHandler != null)
                    {
                        copyHandler(item, Path.Combine(dest, fileName));
                    }
                    else
                    {
                        File.Copy(item, Path.Combine(dest, fileName), true);
                    }
                }
            }
            foreach (var item in Directory.GetDirectories(src, "*", SearchOption.TopDirectoryOnly))
            {
                if (filter?.Invoke(item, true) != false)
                {
                    CopyDirectory(item, Path.Combine(dest, Path.GetFileName(item)), searchPattern, filter, copyHandler);
                }
            }
        }

        /// <summary>
        /// 裁剪右边路径
        /// </summary>
        /// <param name="path">原始路径</param>
        /// <param name="count">层级次数</param>
        public static string PathTrimRightDirectory(string path, int count)
        {
            var strBuf = StringFLibUtility.GetStrBuf();
            strBuf.Append(path);
            var endChar = strBuf[^1];
            if (endChar == '/' || endChar == '\\')
            {
                strBuf.Remove(strBuf.Length - 1, 1);
            }

            for (var i = strBuf.Length - 1; i >= 0; i--)
            {
                if (strBuf[i] == '/' || strBuf[i] == '\\')
                {
                    count--;
                    if (count <= 0)
                    {
                        strBuf.Remove(i + 1, strBuf.Length - i - 1);
                        break;
                    }
                    else
                    {
                        strBuf.Remove(i, strBuf.Length - i);
                    }
                }
            }
            return StringFLibUtility.ReleaseStrBufAndResult(strBuf);
        }


        /// <summary>
        /// 裁剪左边路径
        /// </summary>
        /// <param name="path">原始路径</param>
        /// <param name="count">层级次数</param>
        public static string PathTrimLeftDirectory(string path, int count)
        {
            if (path.Length <= 2) return path;
            var strBuf = StringFLibUtility.GetStrBuf();
            strBuf.Append(path);
            var firstChar = strBuf[^1];
            if (firstChar == '/' || firstChar == '\\')
            {
                strBuf.Remove(0, 1);
            }

            for (var i = 0; count > 0 && i < strBuf.Length; i++)
            {
                if (strBuf[i] == '/' || strBuf[i] == '\\')
                {
                    strBuf.Remove(0, i + 1);
                    i = 0;
                    count--;
                }
            }
            return StringFLibUtility.ReleaseStrBufAndResult(strBuf);
        }

        /// <summary>
        /// 裁剪到第几个目录
        /// </summary>
        /// <param name="leftCount">从左边第几个目录</param>
        /// <param name="path">目录路径</param>
        public static string PathTrimToDirectionName(string path, int leftCount)
        {
            if (path.Length <= 2) return path;

            if (path[0] == '/' || path[0] == '\\')
            {
                path = path[1..];
            }
            var count = path.Length;
            var strBuf = StringFLibUtility.GetStrBuf();
            for (var i = 0; i < count; i++)
            {
                var c = path[i];
                if (c == '/' || c == '\\')
                {
                    leftCount--;
                    if (leftCount <= 0 || i == count - 1)
                    {
                        break;
                    }
                    else
                    {
                        strBuf.Clear();
                    }
                }
                else
                {
                    strBuf.Append(c);
                }
            }


            return StringFLibUtility.ReleaseStrBufAndResult(strBuf);
        }

        /// <summary>
        /// 
        /// </summary>
        public static ReadOnlySpan<char> PathTrimToDirectionName(ReadOnlySpan<char> path, ReadOnlySpan<char> directionName, bool containsDirectionName = false)
        {
            if (path[^1] == '/' || path[^1] == '\\')
                path = path[..^1];
            var endIndex = path.Length;
            for (var i = path.Length - 1; i >= 0; i--)
            {
                if (path[i] == '/' || path[i] == '\\')
                {
                    if (path[(i + 1)..endIndex].Equals(directionName, StringComparison.Ordinal))
                        return containsDirectionName ? path[..endIndex] : path[..i];
                    endIndex = i;
                }
            }
            return default;
        }

        /// <summary>
        /// 获取一个格式化的文件大小字符串
        /// </summary>
        public static string FormatSize(double size)
        {
            string[] sizeNames = { "Byte", "KB", "MB", "GB", "TB" };
            var order = 0;
            while (size >= 1024 && order < sizeNames.Length - 1)
            {
                order++;
                size /= 1024f;
            }
            return size.ToString("0.##") + sizeNames[order];
        }


        /// <summary>
        /// 获取一个安全的文件名,不会重名
        /// </summary>
        public static string SafePath(bool isFilePath, string path, string splitChar = "-", int minSuffixLength = 0)
        {
            var strbuf = StringFLibUtility.GetStrBuf();
            var oldPath = path;
            if (minSuffixLength > 0)
            {
                var tempDotIndex = path.LastIndexOf('.');
                var suffix = strbuf.Append('0', minSuffixLength).ToString();
                path = tempDotIndex >= 0 ? path.Insert(tempDotIndex, splitChar + suffix) : path + splitChar + suffix;
            }
            if ((isFilePath && !File.Exists(path)) || (!isFilePath && !Directory.Exists(path)))
            {
                return path;
            }
            path = oldPath;

            var path2 = path;
            var extension = string.Empty;
            var dotIndex = path.LastIndexOf('.');
            if (dotIndex >= 0)
            {
                extension = path[dotIndex..];
                path2 = path2[..dotIndex];
            }
            strbuf.Clear();
            strbuf.Append(path2).Append(splitChar);
            var path2Count = path2.Length + 1;
            try
            {
                for (var i = 1; i < int.MaxValue; i++)
                {
                    var str = i.ToString();
                    if (str.Length < minSuffixLength)
                    {
                        strbuf.Append('0', minSuffixLength - str.Length);
                    }
                    strbuf.Append(str);
                    var newPath = strbuf.Append(extension).ToString();
                    strbuf.Remove(path2Count, strbuf.Length - path2Count);
                    if ((isFilePath && !File.Exists(newPath)) || (!isFilePath && !Directory.Exists(newPath)))
                    {
                        return newPath;
                    }
                }
            }
            finally
            {
                StringFLibUtility.ReleaseStrBuf(strbuf);
            }
            throw new Exception("not found new filepath");
        }


        /// <summary>
        /// 修改路径文件的名称
        /// </summary>
        public static string PathRename(string path, string newName, bool isAppendNewName = false, bool isKeepExtension = true)
        {
            if (!isAppendNewName)
            {
                var dirIndex = path.LastIndexOf('/');
                if (dirIndex == -1)
                {
                    dirIndex = path.LastIndexOf('\\');
                }
                if (dirIndex == -1)
                {
                    return newName;
                }
                newName = path[..(dirIndex + 1)] + newName;
                if (!isKeepExtension) return newName;
            }
            var exIndex = path.LastIndexOf('.');
            if (exIndex >= 0)
            {
                if (isAppendNewName)
                {
                    newName = path[..exIndex] + newName;
                }
                if (isKeepExtension) newName += path[exIndex..];
            }
            else if (isAppendNewName)
            {
                newName = path + newName;
            }
            return newName;
        }

        /// <summary>
        /// 移除后缀名
        /// </summary>
        public static string RemoveExtension(string path)
        {
            for (var i = path.Length - 1; i >= 0; i--)
            {
                if (path[i] == '.')
                {
                    return path[..i];
                }
                else if (path[i] == '/' || path[i] == '\\')
                {
                    break;
                }
            }
            return path;
        }

        /// <summary>
        /// 获取文件目录
        /// </summary>
        public static string GetFileDirectory(string filepath)
        {
            return File.Exists(filepath) ? Path.GetDirectoryName(filepath) : filepath;
        }

        /// <summary>
        /// 比较两个流
        /// </summary>
        public static bool Compare(string path1, string path2)
        {
            if (!File.Exists(path1) || !File.Exists(path2)) return false;
            using var a = File.Open(path1, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var b = File.Open(path2, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return Compare(a, b);
        }

        /// <summary>
        /// 比较两个流
        /// </summary>
        public static bool Compare(Stream a, Stream b)
        {
            if (a.Length != b.Length) return false;
            var readCount = sizeof(long);
            var loopBlockCount = a.Length / readCount;
            var loopByteCount = a.Length % readCount;
            //var buffer1 = stackalloc byte[readCount];
            //var buffer2 = stackalloc byte[readCount];
            if (loopBlockCount > 0)
            {
                var buffer1 = new byte[readCount];
                var buffer2 = new byte[readCount];
                for (var i = 0; i < loopBlockCount; i++)
                {
                    _ = a.Read(buffer1, 0, readCount);
                    _ = b.Read(buffer2, 0, readCount);
                    if (BitConverter.ToInt64(buffer1, 0) != BitConverter.ToInt64(buffer2, 0))
                    {
                        return false;
                    }
                }
            }
            if (loopByteCount > 0)
            {
                for (var i = 0; i < loopByteCount; i++)
                {
                    if (a.ReadByte() != b.ReadByte()) return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public static FileStream OpenTempFile(string extension = ".txt")
        {
            var path = Path.Combine(Path.GetTempPath(), Path.GetTempFileName() + extension);
            return File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        }

        /// <summary>
        /// 
        /// </summary>
        public static void CreateZip(string[] paths, string zipFilePath, Regex excludePatterns = null, CompressionLevel level = CompressionLevel.Optimal)
        {
            if (File.Exists(zipFilePath))
                File.Delete(zipFilePath);

            using var zipStream = new FileStream(zipFilePath!, FileMode.CreateNew);
            using var archive = new ZipArchive(zipStream, ZipArchiveMode.Create);

            foreach (var path in paths)
            {
                var entryName = Path.GetFileName(path);
                if (excludePatterns?.Match(entryName).Success == true)
                    continue;
                if (File.Exists(path))
                {
                    if (excludePatterns?.Match(entryName).Success == true)
                        continue;
                    archive.CreateEntryFromFile(path, entryName, level);
                }
                else if (Directory.Exists(path))
                {
                    entryName = Path.GetFullPath(path);
                    var baseDirLen = entryName.Length;
                    if (!entryName.EndsWith(Path.DirectorySeparatorChar))
                        ++baseDirLen;
                    foreach (var dirPath in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                    {
                        entryName = dirPath[baseDirLen..];
                        if (excludePatterns?.Match(entryName).Success == true)
                            continue;
                        archive.CreateEntryFromFile(dirPath, entryName, level);
                    }
                }
                else
                {
                    Log.Warn?.Write($"警告：路径不存在，已跳过 {path}");
                }
            }
        }
    }
}
