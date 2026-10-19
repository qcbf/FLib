// ==================== qcbf@qq.com | 2025-07-01 ====================

#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FLib.Unity
{
    [Comment("配置表文件")]
    public class ConfigItemEditorHelper : ScriptableObject
    {
        public string ConfigType;
        public byte[] Bytes = Array.Empty<byte>();
        public IBytesPackable Instance;
        public string AssetGuid;

        public IBytesPackable CreateConfig()
        {
            try
            {
                var inst = (IBytesPackable)TypeAssistant.New(ConfigType);
                BytesPack.Unpack(ref inst, Compressor.Uncompress(Bytes));
                return inst;
            }
            catch (Exception e)
            {
                throw new Exception(name, e);
            }
        }

        public void SetConfig(IBytesPackable configInstance)
        {
            Bytes = Compressor.Compress(BytesPack.Pack(configInstance)).ToArray();
        }
    }
}
#endif
