//==================={By Qcbf|qcbf@qq.com|7/17/2023 8:29:11 PM}===================

using System;
using FLib;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FLib
{
    /// <summary>
    /// <code>
    /// [meta]
    /// Name = { Sign = "c", Default = "Not Found" }
    /// [1001]
    /// Name = "ABC"
    /// [1002]
    /// Name = "XXX"
    /// </code>
    /// </summary>
    public class TomlConfigBuilder : ConfigBuilder.IBuildable
    {
        public string Extension => ".toml";

        public class MetaData : ITomlParseable
        {
            public string Sign;
            public TomlNode Default;

            public object TomlParse(TomlNode node)
            {
                var type = GetType();
                foreach (var kv in ((TomlTable)node).RawTable)
                {
                    if (kv.Key == nameof(Default))
                    {
                        Default = kv.Value;
                    }
                    else
                    {
                        var field = type.GetField(kv.Key) ?? throw new Exception($"not found field {kv.Key}");
                        field.SetValue(this, kv.Value.To(field.FieldType));
                    }
                }

                return null;
            }
        }

        public void Build(in ConfigBuilder.TableContext ctx)
        {
            var toml = Toml.Parse(File.OpenText(ctx.SourceFile.FilePath));
            if (toml.ChildrenCount == 0)
                return;

            FieldInfo configKeyField = null;
            Dictionary<string, MetaData> fieldMetas = null;
            if (toml.TryGetNode("meta", out var tomlMeta))
            {
                fieldMetas = tomlMeta.To<Dictionary<string, MetaData>>();
                if (fieldMetas.Remove("_KeyName", out var KeyNameNode))
                    configKeyField = ctx.ConfigType.GetField(KeyNameNode.ToString()!, BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic);
                toml.Delete("meta");
            }
            ctx.EnsureCapacity(toml.RawTable.Count);
            configKeyField ??= ctx.ConfigType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).First();
            foreach (var line in toml.RawTable)
            {
                try
                {
                    var valuesTable = line.Value.AsTable;
                    object key = line.Key;

                    if (fieldMetas != null)
                    {
                        foreach (var meta in fieldMetas)
                        {
                            if (meta.Value.Sign != null && !ConfigBuilderUtility.CheckSign(ConfigBuilder.Sign, meta.Value.Sign.FirstOrDefault()))
                                valuesTable.Delete(meta.Key);
                            if (meta.Value.Default != null)
                            {
                                if (valuesTable.RawTable.TryGetValue(meta.Key, out tomlMeta))
                                {
                                    if (meta.Value.Default is TomlTable defaultTable && tomlMeta is TomlTable valueTable)
                                    {
                                        foreach (var item in defaultTable.RawTable)
                                        {
                                            if (!valueTable.HasKey(item.Key))
                                                valueTable.Add(item.Key, item.Value);
                                        }
                                    }
                                }
                                else
                                {
                                    valuesTable.Add(meta.Key, meta.Value.Default);
                                }
                            }
                        }
                    }

                    var value = (IBytesPackable)valuesTable.To(ctx.ConfigType, (tomlNode, toType) => ConfigBuilderUtility.ConvertStringToType(tomlNode.ToString(), toType));
                    key = ConfigBuilderUtility.ConvertObjectToType(key, configKeyField.FieldType);
                    configKeyField.SetValue(value, key);
                    ctx.AddConfig(key, value);
                }
                catch (Exception ex)
                {
                    Log.Error?.Write($"{ctx}\n{line}\n{ex}");
                }
            }
        }
    }
}
