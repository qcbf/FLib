// =================================================={By Qcbf|qcbf@qq.com|2024-11-05}==================================================

using System;
using System.Linq;

namespace FLib.Worlds
{
    /// <summary>
    ///
    /// </summary>
    public struct WorldEffectConfigPack : IJson5Deserializable, IBytesSerializable
    {
        public WorldEffect Instance;
        public string Custom;


        public static implicit operator WorldEffect(in WorldEffectConfigPack pack) => pack.Instance;

        public Json5CustomDeserializeResult JsonDeserialize(ref Json5SyntaxNodes nodes, object otherData, in Json5DeserializeOptionData options)
        {
            string typeName;
            (typeName, Custom) = ScriptPack.JsonDeserializeTypeName(ref nodes);
            if (!string.IsNullOrEmpty(WorldInitializer.EffectConfigTypeNamePrefix))
                typeName = WorldInitializer.EffectConfigTypeNamePrefix + typeName;
            Instance = (WorldEffect)nodes.To(TypeAssistant.GetType(typeName));
            return true;
        }

        public void Z_BytesWrite(ref BytesWriter writer)
        {
            writer.PushScript(Instance);
            writer.Push(Custom);
        }

        public void Z_BytesRead(ref BytesReader reader)
        {
            Instance = (WorldEffect)reader.ReadScript();
            Custom = reader.ReadString();
        }
    }
}
