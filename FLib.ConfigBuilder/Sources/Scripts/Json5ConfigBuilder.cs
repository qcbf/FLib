// ==================== qcbf@qq.com | 2025-07-01 ====================

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FLib
{
    /// <summary>
    /// <code>
    /// {
    ///     meta: {Name: {Sign: 'c',Default: "not found"}},
    ///     $1001: {Name: "abc"},
    ///     $1002: {Name: "XXX"},
    /// }
    /// </code>
    /// </summary>
    public class Json5ConfigBuilder : ConfigBuilder.IBuildable
    {
        public string Extension => ".json5";


        public class Value : IJson5Deserializable
        {
            public IBytesPackable CfgData;

            public Json5CustomDeserializeResult JsonDeserialize(ref Json5SyntaxNodes nodes, object otherData, in Json5DeserializeOptionData options)
            {
                var ctx = (ConfigBuilder.TableContext)options.UserData;
                CfgData = (IBytesPackable)nodes.To(ctx.ConfigType);
                return true;
            }
        }

        public void Build(in ConfigBuilder.TableContext ctx)
        {
            foreach (var item in Json5.Deserialize<Dictionary<string, Value>>(File.ReadAllText(ctx.SourceFilePath), new Json5DeserializeOptionData() { UserData = ctx }))
            {
                var keyStr = item.Key;
                if (keyStr.StartsWith('$'))
                    keyStr = keyStr[1..];
                var key = ConfigBuilderUtility.ConvertObjectToType(keyStr, ctx.IndexIdField.FieldType);
                var cfg = item.Value.CfgData;
                var rt = ctx.AddConfig(key, cfg);
                if (rt.HasValue)
                    ctx.IndexIdField.SetValue(cfg, ConfigBuilderUtility.ConvertObjectToType(key, ctx.IndexIdField.FieldType));
            }
        }
    }
}
