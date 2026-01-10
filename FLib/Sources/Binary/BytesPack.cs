//==================={By Qcbf|qcbf@qq.com|10/8/2022 4:40:49 PM}===================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using FLib;

namespace FLib
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class BytesPackGenHoldKeyAttribute : Attribute
    {
        public int Value;
        public BytesPackGenHoldKeyAttribute(int value) => Value = value;
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class BytesPackGenRelocateAttribute : Attribute
    {
        public string RelocateFieldName;
        public BytesPackGenRelocateAttribute(string relocateFieldName) => RelocateFieldName = relocateFieldName;
    }

    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property)]
    public class BytesPackGenCustomCodeAttribute : Attribute
    {
        public string WriteCode;
        public string ReadCode;
    }

    /// <summary>
    /// Variables: ${FieldName} ${FieldType}
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property)]
    public class BytesPackGenAdditionalCodeAttribute : Attribute
    {
        public string WriteCode;
        public string ReadCode;
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class BytesPackGenAttribute : Attribute
    {
        // public EBytePackGenOption Options;
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class BytesPackGenFieldAttribute : Attribute
    {
        public int Key;
        public string WriteCode;
        public string ReadCode;
        public EBytePackGenFieldOption Options;
        public BytesPackGenFieldAttribute() { }
        public BytesPackGenFieldAttribute(int key) => Key = key;
    }

    // [Flags]
    // public enum EBytePackGenOption
    // {
    //     None,
    //     GenFieldKeys = 0x1,
    // }

    [Flags]
    public enum EBytePackGenFieldOption
    {
        None,
        VInt = 0x1,
    }


    /// <summary>
    /// hook params: (object customData, Type packObjectType, int packFieldKey, Span[byte] content)
    /// </summary>
    public ref struct BytesPackPushHookHandler
    {
        public object CustomData;

        // public delegate*<object, Type, int, Span<byte>, bool> FastHandler;
        public HandlerDelegate Handler;

        public delegate bool HandlerDelegate(object customData, Type type, int key, Span<byte> content);

        public readonly bool IsEmpty => Handler == null;

        public readonly bool Invoke(object customData, Type type, int key, Span<byte> content) => Handler.Invoke(customData, type, key, content);
        // public readonly bool IsEmpty => (FastHandler == (void*)IntPtr.Zero) && Handler == null;
        // public readonly bool Invoke(object customData, Type type, int key, Span<byte> content) => Handler?.Invoke(customData, type, key, content) ?? FastHandler(customData, type, key, content);
    }

    public static class BytesPack
    {
        public ref struct KeyHelper
        {
            public BytesPackPushHookHandler Hook;
            public Type PackObjectType;
            private int ValueStartPosition;

            public void Push(ref BytesWriter writer, int k)
            {
                if (ValueStartPosition > 0)
                    LateProcess(ref writer);
                writer.PushVInt(k);
                ValueStartPosition = writer.Position;
            }

            internal void Finish(ref BytesWriter writer)
            {
                if (ValueStartPosition > 0)
                    LateProcess(ref writer);
            }

            private void LateProcess(ref BytesWriter writer)
            {
                if (!Hook.IsEmpty)
                {
                    throw new NotSupportedException();
                    // var reader = (BytesReader)writer;
                    // reader.Position = ValueStartPosition;
                    // var lastKey = (int)reader.ReadVInt();
                    // var contentBytes = writer.Span[(LastContentPosition + 4)..writer.Position];
                    // if (Hook.Invoke(Hook.CustomData, PackObjectType, lastKey, contentBytes))
                    // {
                    //     return;
                    // }
                }
                writer.PushVInt(ValueStartPosition, writer.Position - ValueStartPosition);
                ValueStartPosition = 0;
            }
        }

        public static Span<byte> Pack<T>(in T v, in BytesPackPushHookHandler hook = default) where T : IBytesPackable
        {
            var w = new BytesWriter();
            Pack(v, ref w, hook);
            w.Position = 0;
            return w.Span;
        }

        public static void Pack<T>(in IList<T[]> v, ref BytesWriter writer, in BytesPackPushHookHandler hook = default) where T : IBytesPackable
        {
            var len = (v?.Count).GetValueOrDefault();
            writer.PushLength(len);
            for (var i = 0; i < len; i++)
            {
                Pack(v![i], ref writer);
            }
        }

        public static Span<byte> Pack<T>(in IList<T> v, in BytesPackPushHookHandler hook = default) where T : IBytesPackable
        {
            var w = new BytesWriter();
            Pack(v, ref w, hook);
            w.Position = 0;
            return w.Span;
        }

        public static void Pack<T>(in IList<T> v, ref BytesWriter writer, in BytesPackPushHookHandler hook = default) where T : IBytesPackable
        {
            var len = (v?.Count).GetValueOrDefault();
            writer.PushLength(len);
            for (var i = 0; i < len; i++)
            {
                Pack(v![i], ref writer, hook);
            }
        }

        public static bool Pack<T>(in T v, ref BytesWriter writer, in BytesPackPushHookHandler hook = default) where T : IBytesPackable
        {
            var key = new KeyHelper { Hook = hook };
            if (!hook.IsEmpty)
                key.PackObjectType = typeof(T);
            var startPosition = writer.Position;
            v.Z_BytesPackWrite(ref key, ref writer);
            key.Finish(ref writer);
            var packSize = writer.Position - startPosition;
            writer.PushVInt(startPosition, packSize);
            return packSize > 0;
        }

        public static T Unpack<T>(ref BytesReader reader, DebugOnlyString errorAdditionText = default) where T : IBytesPackable, new()
        {
            var v = new T();
            Unpack(ref v, ref reader, errorAdditionText);
            return v;
        }

        public static T Unpack<T>(in ReadOnlySpan<byte> bytes, DebugOnlyString errorAdditionText = default) where T : IBytesPackable, new()
        {
            var v = new T();
            Unpack(ref v, bytes, errorAdditionText);
            return v;
        }

        public static void Unpack<T>(ref T data, in ReadOnlySpan<byte> bytes, DebugOnlyString errorAdditionText = default) where T : IBytesPackable
        {
            var reader = (BytesReader)bytes;
            Unpack(ref data, ref reader, errorAdditionText);
        }

        public static void Unpack<T>(ref T data, ref BytesReader reader, DebugOnlyString errorAdditionText = default) where T : IBytesPackable
        {
            if (reader.Available <= 0)
                return;
            var totalSize = reader.ReadVInt();
            var overPos = totalSize + reader.Position;
            while (reader.Position < overPos)
            {
                var k = (int)reader.ReadVInt();
                var size = (int)reader.ReadVInt();
                var oldPos = reader.Position;
                try
                {
                    data!.Z_BytesPackRead(k, ref reader);
                    if (reader.Position == -oldPos)
                    {
                        reader.Position = oldPos + size;
                    }
                    else
                    {
                        var diff = reader.Position - oldPos;
                        if (diff != size)
                        {
                            reader.Position = oldPos + size;
                            Log.Error?.Write($"unpack position[{reader.Position} expect:{size} actual:{diff}] error {data?.GetType() ?? typeof(T)}>{k}>{errorAdditionText}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    reader.Position = oldPos + size;
                    Log.Error?.Write($"unpack exception {data?.GetType() ?? typeof(T)}>{k}>{errorAdditionText}\n{ex}"); // 因为某些字段修改了大小，需要报错并且继续解析后续的字段
                    // throw new Exception($"unpack exception {data.GetType()}>{k}>{errorAdditionText}\n{ex}");
                }
            }
        }

        public static void Unpack<T>(ref T[] data, in Span<byte> bytes, DebugOnlyString errorAdditionText = default) where T : IBytesPackable
        {
            var reader = (BytesReader)bytes;
            Unpack(ref data, ref reader, errorAdditionText);
        }

        public static void Unpack<T>(ref T[] data, ref BytesReader reader, DebugOnlyString errorAdditionText = default) where T : IBytesPackable
        {
            var len = reader.ReadLength();
            if (len != data?.Length)
            {
                data = new T[len];
            }

            for (var i = 0; i < len; i++)
            {
                Unpack(ref data[i], ref reader, errorAdditionText);
            }
        }

        public static void Unpack<T>(ref T[][] data, ref BytesReader reader, DebugOnlyString errorAdditionText = default) where T : IBytesPackable
        {
            var len = reader.ReadLength();
            if (len != data?.Length)
            {
                data = new T[len][];
            }

            for (var i = 0; i < len; i++)
            {
                Unpack(ref data[i], ref reader, errorAdditionText);
            }
        }


        public static void PackNullableElement<T>(in IList<T> v, ref BytesWriter writer) where T : IBytesPackable, new()
        {
            var len = (v?.Count).GetValueOrDefault();
            writer.PushLength(len);
            for (var i = 0; i < len; i++)
            {
                if (v![i] == null)
                {
                    writer.Push(false);
                    continue;
                }

                writer.Push(true);
                Pack(v[i], ref writer);
            }
        }

        public static void PackNullableElement<T>(in IList<T[]> v, ref BytesWriter writer) where T : IBytesPackable, new()
        {
            var len = (v?.Count).GetValueOrDefault();
            writer.PushLength(len);
            for (var i = 0; i < len; i++)
            {
                if (v![i] == null)
                {
                    writer.Push(false);
                    continue;
                }

                writer.Push(true);
                PackNullableElement(v[i], ref writer);
            }
        }

        public static void UnpackNullableElement<T>(ref T[] data, ref BytesReader reader, DebugOnlyString errorAdditionText = default) where T : IBytesPackable, new()
        {
            var len = reader.ReadLength();
            if (len != data?.Length)
            {
                data = new T[len];
            }

            for (var i = 0; i < len; i++)
            {
                if (reader.Read<bool>())
                {
                    data[i] = new T();
                    Unpack(ref data[i], ref reader, errorAdditionText);
                }
            }
        }

        public static void UnpackNullableElement<T>(ref T[][] data, ref BytesReader reader, DebugOnlyString errorAdditionText = default) where T : IBytesPackable, new()
        {
            var len = reader.ReadLength();
            if (len != data?.Length)
            {
                data = new T[len][];
            }

            for (var i = 0; i < len; i++)
            {
                if (reader.Read<bool>())
                {
                    UnpackNullableElement(ref data[i], ref reader, errorAdditionText);
                }
            }
        }
    }
}
