//=================================================={By Qcbf|qcbf@qq.com|11/10/2024 4:27:50 PM}==================================================

using FLib;
using System;
using System.Collections.Generic;


namespace FLib.Worlds
{
    public interface IWorldComponentSerializable
    {
        bool BytesPack(WorldBase world, ushort index, ref BytesWriter writer, in BytesPackPushHookHandler hook = default);
        void BytesUnpack(WorldBase world, ushort index, ref BytesReader reader, DebugOnlyString errorAdditionText = default);
    }

    public class WorldComponentSerializer<T> : IWorldComponentSerializable where T : IBytesPackable, IWorldComponentable, new()
    {
        public bool BytesPack(WorldBase world, ushort index, ref BytesWriter writer, in BytesPackPushHookHandler hook = default)
        {
            var group = world.ComponentMgr.GetGroup<T>();
            return FLib.BytesPack.Pack(group.ComponentMetas[index].Component, ref writer, hook);
        }

        public void BytesUnpack(WorldBase world, ushort index, ref BytesReader reader, DebugOnlyString errorAdditionText = default)
        {
            var group = world.ComponentMgr.GetGroup<T>();
            FLib.BytesPack.Unpack(ref group.ComponentMetas[index].Component, reader, errorAdditionText);
        }
    }
}
