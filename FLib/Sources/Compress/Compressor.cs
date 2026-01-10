//==================={By Qcbf|qcbf@qq.com|10/9/2022 11:10:05 AM}===================

using System;
using System.Collections.Generic;
using FLib;
using K4os.Compression.LZ4;

namespace FLib
{
    public static class Compressor
    {
        public static Span<byte> Compress(ReadOnlySpan<byte> target, bool isFast = false)
        {
            var writer = new BytesWriter();
            Compress(target, ref writer, isFast);
            return writer.BytesBuffer[..writer.Length];
        }

        public static void Compress(
#if NET8_0_OR_GREATER
            scoped
#endif
            ReadOnlySpan<byte> target, ref BytesWriter writer, bool isFast = false)
        {
            writer.PushLength(target.Length); // 原始大小
            writer.Push(0); // 压缩大小
            writer.TryAddLength(LZ4Codec.MaximumOutputSize(target.Length));
            var compressSize = LZ4Codec.Encode(target, writer.BytesBuffer[writer.Position..], isFast ? LZ4Level.L00_FAST : LZ4Level.L12_MAX);
            if (compressSize <= 0) throw new Exception("compress error");
            writer.Position -= 4;
            writer.Push(compressSize);
            writer.Length = writer.Position + compressSize;
            writer.Position += compressSize;
        }

        public static ArraySegment<byte> Uncompress(
#if NET8_0_OR_GREATER
            scoped
#endif
            in Span<byte> buffer)
        {
            if (buffer.IsEmpty)
                return default;
            BytesWriter writer = default;
            BytesReader r = buffer;
            Uncompress(ref r, ref writer);
            return new ArraySegment<byte>(writer.HeapArray, 0, writer.Length);
        }

        public static int Uncompress(in Span<byte> buffer, BytesWriter writer)
        {
            BytesReader r = buffer;
            Uncompress(ref r, ref writer);
            return writer.Position;
        }

        public static void Uncompress(in Span<byte> buffer, ref BytesWriter writer)
        {
            BytesReader r = buffer;
            Uncompress(ref r, ref writer);
        }

        public static void Uncompress(ref BytesReader reader, ref BytesWriter writer)
        {
            var uncompressedSize = reader.ReadLength();
            var compressedSize = reader.Read<int>();
            writer.TryAddLength(uncompressedSize);
            var decompressSize = LZ4Codec.Decode(reader.BytesBuffer.Slice(reader.Position, compressedSize), writer);
            if (decompressSize <= 0) throw new Exception("decompress error");
            reader.Position += compressedSize;
            writer.Position += decompressSize;
        }

        public static void Uncompress(ref BytesWriter writer)
        {
            var compressed = new BytesWriter { Allocator = BytesWriter.PoolAllocator };
            try
            {
                Uncompress(writer, ref compressed);
                writer.Clear();
                writer.CopyFrom(compressed);
            }
            finally
            {
                compressed.TryReleasePoolAllocator();
            }
        }

        public static int PeekRawDataSize(BytesReader reader)
        {
            return reader.ReadLength();
        }
    }
}
