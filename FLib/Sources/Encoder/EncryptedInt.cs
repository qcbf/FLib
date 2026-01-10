//==================={By Qcbf|qcbf@qq.com|5/9/2023 3:18:21 PM}===================

using System;
using System.Collections.Generic;
using FLib;

namespace FLib
{
    public struct EncryptedInt
    {
        public static int Key = Environment.TickCount;

        public int Raw;

        public readonly override string ToString() => Get().ToString();

        public static EncryptedInt Create(int v = 0)
        {
            var result = new EncryptedInt();
            result.Set(v);
            return result;
        }

        public void Add(int v)
        {
            Set(Get() + v);
        }

        public void Sub(int v)
        {
            Set(Get() - v);
        }

        public void Set(int v)
        {
            var temp = v & 0xf;
            Raw = (((v & ~0xf) << 1) | temp) ^ Key;
        }

        public readonly int Get()
        {
            var v = Raw ^ Key;
            var temp = v & 0xf;
            return (v >> 1) & ~0xf | temp;
        }


        public static implicit operator int(in EncryptedInt v) => v.Get();


        public void Z_BytesPackRead(int key, ref BytesReader reader)
        {
            Raw = key switch
            {
                2 => (int)reader.ReadVInt(),
                _ => Raw
            };
        }

        public readonly void Z_BytesPackWrite(ref BytesPack.KeyHelper key, ref BytesWriter writer)
        {
            key.Push(ref writer, 2);
            writer.PushVInt(Raw);
        }
    }
}
