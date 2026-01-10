//=================================================={By Qcbf|qcbf@qq.com|12/15/2024 3:51:09 PM}==================================================

using FLib;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable CA2211
namespace FLib
{
    public static class ConfigBuilder
    {
        public static int ConfigTableCompressSize = 1024;
        public static char Sign = '*';
        public static string OutputPath = "cfg.bytes";
        public static Action<ConcurrentDictionary<Type, IConfigBuildTableContext>, Dictionary<string, SourceFileMeta>> CustomBuilder;
        private static readonly Func<IEnumerable<Type>> GetAllTypes = () => TypeAssistant.AllAssemblies.Where(v => v != typeof(ConfigHelper).Assembly && v != typeof(ConfigHelper).Assembly).SelectMany(v => v.ExportedTypes);
        public static readonly Func<IReadOnlyDictionary<string, IBuildable>> GetConfigBuilders = () => TypeAssistant.AllAssemblies.Append(typeof(ConfigBuilder).Assembly).SelectMany(v => v.ExportedTypes).Where(t => !t.IsInterface && typeof(IBuildable).IsAssignableFrom(t)).Select(t => (IBuildable)TypeAssistant.New(t)).ToDictionary(k => k.Extension);

        /// <summary>
        /// 
        /// </summary>
        public interface IBuildable
        {
            void Build(in TableContext ctx);
            string Extension { get; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class EmptyBuilder : IBuildable
        {
            public static readonly EmptyBuilder Default = new();
            public string Extension => "";
            public void Build(in TableContext ctx) { }
        }

        /// <summary>
        /// 
        /// </summary>
        public class TableContext : IConfigBuildTableContext
        {
            private SpinLock _locker;
            private uint _dynamicIdIncrement = 1;
            public SourceFileMeta SourceFile;
            public Type ConfigType { get; set; }
            public FieldInfo IndexIdField { get; }
            public ConfigHelper.EOption Options { get; set; }
            public List<(uint Id, IBytesPackable Cfg)> AllConfigs { get; set; } = new(128);
            public Dictionary<uint, int> AllConfigIdIndexes { get; set; } = new(128);
            public string SourceFilePath => SourceFile.FilePath;
            public string ConfigName => SourceFile.ConfigName;
            public override string ToString() => $"[{ConfigType.Name}]{SourceFile}";

            public TableContext(SourceFileMeta sourceFile, Type type, ConfigHelper.EOption options)
            {
                SourceFile = sourceFile;
                ConfigType = type;
                Options = options;
                IndexIdField = ConfigType.GetFields(BindingFlags.Public | BindingFlags.Instance).OrderBy(v => v.MetadataToken).First();
            }

            /// <summary>
            /// 
            /// </summary>
            public void EnsureCapacity(int capacity)
            {
#if NET6_0_OR_GREATER
                AllConfigs.EnsureCapacity(capacity);
#else
                if (capacity > AllConfigs.Capacity)
                    AllConfigs.Capacity = capacity;
#endif
                AllConfigIdIndexes.EnsureCapacity(capacity);
            }

            public (uint Id, int Index)? AddConfigAndDynamicId(IBytesPackable config)
            {
                var isLocking = false;
                _locker.Enter(ref isLocking);
                try
                {
                    while (AllConfigIdIndexes.ContainsKey(_dynamicIdIncrement))
                        ++_dynamicIdIncrement;
                    var index = AllConfigs.Count;
                    AllConfigs.Add((_dynamicIdIncrement, config));
                    AllConfigIdIndexes.Add(_dynamicIdIncrement, index);
                    return (_dynamicIdIncrement, index);
                }
                finally
                {
                    if (isLocking)
                        _locker.Exit(false);
                }
            }

            /// <summary>
            /// 
            /// </summary>0
            public (uint Id, int Index)? AddConfig(object objId, IBytesPackable config)
            {
                if (objId == null && config == null)
                    return null;
                var isLocking = false;
                _locker.Enter(ref isLocking);
                try
                {
                    var index = AllConfigs.Count;
                    var id = objId is string strId ? ConfigHelper.StringToUniqueId(strId) : Convert.ToUInt32(objId);
                    if (!AllConfigIdIndexes.TryAdd(id, index))
                        Log.Error?.Write($"存在相同Id配置: {ConfigType.Name}.{objId}\n{SourceFile}");
                    AllConfigs.Add((id, config));
                    return (id, index);
                }
                finally
                {
                    if (isLocking)
                        _locker.Exit();
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public class SourceFileMeta
        {
            public IBuildable Builder;
            public string FilePath;
            public string ConfigName;
            public char Sign;
            public override string ToString() => FilePath;
        }

        /// <summary>
        /// 
        /// </summary>
        public static int Build(string sourceDirPaths, bool isMultithread = true)
        {
            try
            {
                ConfigPostBuildProcessData.AdditionConfigPostBuildProcesses = new List<ConfigPostBuildProcessData>();
                var tableContexts = BuildTables(sourceDirPaths, isMultithread);
                PostBuildProcess(tableContexts);
                ConfigPostBuildProcessData.AdditionConfigPostBuildProcesses = null;
                var outPath = Path.GetFullPath(OutputPath);
                FIO.CreateDirectory(Path.GetDirectoryName(outPath));
                File.WriteAllBytes(outPath, Compressor.Compress(GenerateConfigBytes(tableContexts)).ToArray());
                return tableContexts.Count;
            }
            catch (Exception ex)
            {
                Log.Error?.Write(ex.ToString());
            }
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        private static IReadOnlyDictionary<Type, IConfigBuildTableContext> BuildTables(string sourceDirPath, bool isMultithread)
        {
            var sourceFileMetas = new Dictionary<string, SourceFileMeta>();
            var allConfigBuilders = GetConfigBuilders();
            foreach (var filePath in Directory.GetFiles(sourceDirPath, "*", SearchOption.AllDirectories))
            {
                if (filePath[0] == '~')
                    continue;
                var meta = new SourceFileMeta() { FilePath = filePath, Builder = allConfigBuilders.GetValueOrDefault(Path.GetExtension(filePath)) };
                if (filePath[0] == '$')
                {
                    meta.Sign = filePath[1];
                    meta.ConfigName = Path.GetFileNameWithoutExtension(filePath.AsSpan(2)).ToString();
                }
                else
                {
                    meta.Sign = '*';
                    meta.ConfigName = Path.GetFileNameWithoutExtension(filePath);
                }
                sourceFileMetas.Add(meta.ConfigName, meta);
            }

            var contexts = new ConcurrentDictionary<Type, IConfigBuildTableContext>(Environment.ProcessorCount, 1024);
            CustomBuilder?.Invoke(contexts, sourceFileMetas);

            if (isMultithread)
                GetAllTypes().AsParallel().ForAll(t => BuildTablesAddContext(t, contexts, sourceFileMetas));
            else
                foreach (var type in GetAllTypes())
                    BuildTablesAddContext(type, contexts, sourceFileMetas);
            return contexts;
        }

        /// <summary>
        /// 
        /// </summary>
        private static void BuildTablesAddContext(Type type, ConcurrentDictionary<Type, IConfigBuildTableContext> contexts, Dictionary<string, SourceFileMeta> fileMetas)
        {
            var attr = type.GetCustomAttribute<ConfigAttribute>();
            if (attr?.ConfigFileName == null)
                return;
            if (!fileMetas.TryGetValue(attr.ConfigFileName, out var sourceFileMeta))
            {
                Log.Info?.Write($"not found config file {attr.ConfigFileName}");
                return;
            }
            if (!ConfigBuilderUtility.CheckSign(sourceFileMeta.Sign, Sign))
                return;
            var ctx = (TableContext)contexts.GetOrAdd(type, new TableContext(sourceFileMeta, type, attr.Options));
            try
            {
                var configType = ctx.ConfigType;
                if (typeof(IConfigFileCustomDeserializeToTable).IsAssignableFrom(configType))
                {
                    var deserializer = (IConfigFileCustomDeserializeToTable)TypeAssistant.New(configType);
                    deserializer.ConfigFileDeserializeToTable(Sign, ctx, contexts);
                }
                else
                {
                    if (ctx.SourceFile.Builder == null)
                        Log.Error?.Write($"not found config builder {type} {attr.ConfigFileName}");
                    else
                        ctx.SourceFile.Builder.Build(ctx);
                }
            }
            catch (Exception ex)
            {
                Log.Error?.Write($"{ctx}\n{ex}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static void PostBuildProcess(IReadOnlyDictionary<Type, IConfigBuildTableContext> allContexts)
        {
            foreach (var item in ConfigPostBuildProcessData.AdditionConfigPostBuildProcesses)
            {
                if (item.CfgType == null)
                    item.Process.OnConfigPostBuildProcess(Sign, null, allContexts);
                else if (allContexts.TryGetValue(item.CfgType, out var ctx))
                    item.Process.OnConfigPostBuildProcess(Sign, ctx, allContexts);
                else
                    throw new Exception($"not found config {item.CfgType}");
            }

            foreach (var item in allContexts)
            {
                var ctx = item.Value;
                if (typeof(IConfigPostBuildProcessable).IsAssignableFrom(ctx.ConfigType))
                {
                    foreach (var (id, cfg) in ctx.AllConfigs)
                    {
                        try
                        {
                            ((IConfigPostBuildProcessable)cfg).OnConfigPostBuildProcess(Sign, ctx, allContexts);
                        }
                        catch (Exception ex)
                        {
                            Log.Error?.Write($"{id}.{ctx}\n{ex}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static BytesWriter GenerateConfigBytes(IReadOnlyDictionary<Type, IConfigBuildTableContext> contexts)
        {
            var writer = new BytesWriter();
            writer.Allocate(contexts.Count * 1024);
            writer.PushLength(contexts.Count);

            var packBuffer = new BytesWriter();
            foreach (var ctx in contexts)
            {
                var options = ctx.Value.Options;
                writer.Push(TypeAssistant.GetTypeName(ctx.Key), Encoding.ASCII);
                writer.PushLength(ctx.Value.AllConfigs.Count);
                IEnumerable<(uint, IBytesPackable)> configs = ctx.Value.AllConfigs;
                if ((ctx.Value.Options & ConfigHelper.EOption.OrderById) != 0)
                    configs = configs.OrderBy(v => v.Item1);
                foreach (var (id, cfg) in configs)
                {
                    packBuffer.Clear();
                    BytesPack.Pack(cfg, ref packBuffer);
                    var copyOptions = options;
                    if (packBuffer.Length >= ConfigTableCompressSize)
                        copyOptions |= ConfigHelper.EOption.AlwaysCompressRawData;
                    if ((copyOptions & ConfigHelper.EOption.AlwaysCompressRawData) != 0)
                        packBuffer = new BytesWriter(Compressor.Compress(packBuffer));
                    writer.PushVInt(id);
                    writer.Push(copyOptions);
                    writer.Push(packBuffer.Span);
                }
            }
            return writer;
        }
    }
}
