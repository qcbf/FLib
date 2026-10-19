//==================={By Qcbf|qcbf@qq.com|12/6/2021 9:51:08 PM}===================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace FLib.Unity.Editor
{
    public static class EditorFLibUnityInitializer
    {
        [InitializeOnLoadMethod]
        public static void InitializeSystem()
        {
            TypeAssistant.AddAssemblies(typeof(EditorFLibUnityInitializer).Assembly);
#if UNITY_2022
            InspectorExUtility.ResetCustomEditors();
            InspectorExUtility.SetCustomEditor(typeof(MonoBehaviour), typeof(MethodButtonEditor), true, true, true);
#endif
            UnityTypeSerializationSupporter.Register();
            ConfigBuilder.CustomBuilder += UnityConfigBuilder;
#if !UNITY_6000_3_OR_NEWER
            UnityToolbarZoneExtender.Initialize();
#endif
            // IEditableConfigId.GuidToConfigIdHandler += UnityConfigGuidHandler;
        }

        // private static uint? UnityConfigGuidHandler(Guid arg)
        // {
        //     if (arg != Guid.Empty)
        //     {
        //         var cfgAssetPath = AssetDatabase.GUIDToAssetPath(Unsafe.As<Guid, GUID>(ref arg));
        //         return ConfigItemEditor.GetAssetNameId(cfgAssetPath).ToUInt();
        //     }
        //     return null;
        // }

        private static void UnityConfigBuilder(ConcurrentDictionary<Type, IConfigBuildTableContext> contexts, Dictionary<string, ConfigBuilder.SourceFileMeta> fileMetas)
        {
            foreach (var assetGuid in AssetDatabase.FindAssets($"t:{nameof(ConfigItemEditorHelper)}"))
            {
                var item = AssetDatabase.LoadAssetAtPath<ConfigItemEditorHelper>(AssetDatabase.GUIDToAssetPath(assetGuid));
                if (item.Bytes == null || item.Bytes.Length == 0 || item.name.EndsWith('~') || item.name.StartsWith('~'))
                    continue;
                var cfg = item.CreateConfig();
                var cfgType = cfg.GetType();
                var cfgAttr = cfgType.GetCustomAttribute<ConfigAttribute>();
                if (!fileMetas.TryGetValue(cfgAttr.ConfigFileName, out var fileMeta))
                    fileMetas.Add(cfgAttr.ConfigFileName,
                        fileMeta = new ConfigBuilder.SourceFileMeta() { Sign = '*', ConfigName = cfgAttr.ConfigFileName, Builder = ConfigBuilder.EmptyBuilder.Default });
                if (!contexts.TryGetValue(cfgType, out var context))
                {
                    if (!contexts.TryAdd(cfgType, context = new ConfigBuilder.TableContext(fileMeta, cfgType, cfgAttr.Options)))
                        throw new Exception();
                }

                try
                {
                    var key = Convert.ChangeType(ConfigItemEditor.GetAssetNameId(item.name).ToString(), context.IndexIdField.FieldType);
                    context.IndexIdField.SetValue(cfg, key);
                    context.AddConfig(key, cfg);
                }
                catch (Exception e)
                {
                    throw new Exception($"{item.name} {cfgAttr.ConfigFileName}", e);
                }
            }
        }
    }
}