//=================================================={By Qcbf|qcbf@qq.com|11/27/2024 3:56:40 PM}==================================================

using FLib;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FLib
{
    public class LogFileWriter : LogWriter
    {
        public string FilePath;
        public int MaxSize = 1024 * 1024 * 128;

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
                File.Move(FilePath, FIO.PathRename(FilePath, $".bak{TimeHelper.Timestamp}", true));
                AllocStream();
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
            Stream = new FileStream(FilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite, 16 * 1024);
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
