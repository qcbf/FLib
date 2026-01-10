// ==================== qcbf@qq.com | 2025-07-01 ====================

using System;

namespace FLib
{
    public struct ObjectBytesPackWrap : IBytesPackable
    {
        public object Value;
        public ObjectBytesPackWrap(object value) => Value = value;

        public void Z_BytesPackWrite(ref BytesPack.KeyHelper key, ref BytesWriter writer)
        {
            key.Push(ref writer, 1);
            if (Value != null)
            {
                var json = Json5.Serialize(Value);
                writer.Push(TypeAssistant.GetTypeName(Value.GetType()));
                writer.Push(json);
            }
            else
            {
                writer.Push(string.Empty);
            }
        }

        public void Z_BytesPackRead(int key, ref BytesReader reader)
        {
            if (key == 1)
            {
                var typeName = reader.ReadString();
                if (typeName.Length > 0)
                {
                    var json = reader.ReadString();
                    Value = Json5.Deserialize(json, TypeAssistant.GetType(typeName));
                }
            }
        }
    }
}
