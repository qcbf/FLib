//=================================================={By Qcbf|qcbf@qq.com|11/27/2024 3:56:40 PM}==================================================

using FLib;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FLib
{
    public class LogFileWriter : LogWriter
    {
        public string FilePath;
        /// <summary>Maximum size of the active file before it is rotated.</summary>
        public long MaxSize = 64L * 1024 * 1024;

        /// <summary>Maximum number of files kept, including the active file.</summary>
        public int MaxFileCount = 15;

        public FileStream Stream;

        public LogFileWriter(string filePath = "out.log", int capacity = 128) : base(capacity)
        {
            FilePath = filePath;
            AllocStream();
        }

        public override void Dispose()
        {
            base.Dispose();
            Stream.Close();
            Stream.Dispose();
        }


        public override void Write(Log log, string text)
        {
            if (Stream.Length > MaxSize)
            {
                Stream.Dispose();
                File.Move(FilePath, IOUtility.PathRename(FilePath, $".bak{TimeHelper.Timestamp}", true));
                AllocStream();
                CleanupBackups();
            }

            var enc = StringFLibUtility.Encoding;
            var bytes = ArrayPool<byte>.Shared.Rent(enc.GetMaxByteCount(text.Length));
            try
            {
                var size = enc.GetBytes(text, bytes);
                Stream.Write(new ReadOnlySpan<byte>(bytes, 0, size));
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(bytes);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void AllocStream()
        {
            var directory = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
            Stream = new FileStream(FilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite, 16 * 1024);
        }

        private void CleanupBackups()
        {
            var directory = Path.GetDirectoryName(FilePath);
            if (string.IsNullOrEmpty(directory))
                directory = Directory.GetCurrentDirectory();

            var fileName = Path.GetFileName(FilePath);
            var extension = Path.GetExtension(fileName);
            var stem = Path.GetFileNameWithoutExtension(fileName) + ".bak";
            var backups = Directory.EnumerateFiles(directory, stem + "*" + extension, SearchOption.TopDirectoryOnly)
                .OrderBy(File.GetLastWriteTimeUtc)
                .ToList();
            var keepBackups = Math.Max(0, Math.Max(1, MaxFileCount) - 1);
            for (var i = 0; i < backups.Count - keepBackups; ++i)
                File.Delete(backups[i]);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Flush()
        {
            Stream?.Flush();
        }
    }
}
