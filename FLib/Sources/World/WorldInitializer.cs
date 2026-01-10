// =================================================={By Qcbf|qcbf@qq.com|2024-10-26}==================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

#pragma warning disable CA2211
namespace FLib.Worlds
{
    public static class WorldInitializer
    {
        public static Func<WorldEffectSystem, uint, WorldEffect> GetEffectHandle = static (_, _) => throw new NotSupportedException();
        public static Func<uint, WorldEffectSystem, WorldEffectContext> CreateEffectContextHandler = static (_, _) => MultiObjectPool.Global.Create<WorldEffectContext>();
        public static Action<WorldEffectSystem, WorldEffectContext> DestroyEffectContextHandler = static (_, ctx) => MultiObjectPool.Global.Release(ctx);
        public static string EffectConfigTypeNamePrefix = string.Empty;
        public static string ComponentConfigPackTypeNamePrefix = string.Empty;

        public static void Initialize()
        {
            const string TYPE_ID = "TypeId";

            // record
            var behaviors = new List<WorldBehavior>();
            var syncerCommandTypes = new List<Type>();
            var syncerComponentPermissions = new List<(Type, byte)>();
            var componentFollows = new Dictionary<Type, List<(Type, WorldComponentFollowAttribute)>>();
            var components = new SlimDictionary<Type, (int?, WorldComponentOrderAttribute)>(1024);
            foreach (var asm in TypeAssistant.AllAssemblies)
            {
                foreach (var type in asm.GetExportedTypes())
                {
                    if (type.IsInterface || type.IsAbstract)
                        continue;

                    if (typeof(IWorldComponentable).IsAssignableFrom(type))
                    {
                        components.Add(type, (null, type.GetCustomAttribute<WorldComponentOrderAttribute>(false)));

                        var syncPermissionAttr = type.GetCustomAttribute<WorldSyncPermissionAttribute>(false);
                        if (syncPermissionAttr != null)
                            syncerComponentPermissions.Add((type, syncPermissionAttr.Permission));

                        var followAttr = type.GetCustomAttribute<WorldComponentFollowAttribute>(false);
                        if (followAttr == null) continue;
                        if (!componentFollows.TryGetValue(followAttr.Target, out var list))
                            componentFollows.Add(followAttr.Target, list = new List<(Type, WorldComponentFollowAttribute)>());
                        list.Add((type, followAttr));
                    }
                    else if (typeof(WorldBehavior).IsAssignableFrom(type))
                    {
#if DEBUG
                        if (type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Length > 1)
                        {
                            var fieldNames = string.Join(',', type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Select(v => v.Name));
                            Log.Error?.Write($"{type} must not have instance fields: {fieldNames}");
                            continue;
                        }
#endif
                        behaviors.Add((WorldBehavior)TypeAssistant.New(type));
                    }
                    else if (typeof(IWorldSyncCommandable).IsAssignableFrom(type))
                    {
                        syncerCommandTypes.Add(type);
                    }
                }
            }

            #region component
            var sortNameHash = new HashSet<int>(1024);
            var componentTypes = new List<(Type, int, int)>(1024);

            using var componentOrderIterator = components.GetEnumerator();
            while (componentOrderIterator.MoveNext())
            {
                CalcOrderValue(components, ref componentOrderIterator.Value);
                var nameHash = StringFLibUtility.ShortStringToHash(WorldComponentManager.GetTypeName(componentOrderIterator.Key));
                Log.Assert(sortNameHash.Add(nameHash))?.Write($"{componentOrderIterator.Key} hash collided");
                componentTypes.Add((componentOrderIterator.Key, componentOrderIterator.Value.Item1!.Value, nameHash));
            }

            WorldComponentManager.ComponentCount = (ushort)componentTypes.Count;
            WorldComponentManager.ComponentTypes = new Type[WorldComponentManager.ComponentCount];
            WorldComponentManager.ComponentGroupTypes = new Type[WorldComponentManager.ComponentCount];

            // sort
            for (var i = WorldComponentManager.ComponentCount - 1; i >= 0; i--)
            {
                for (var j = 0; j < i; j++)
                {
                    var a = componentTypes[j];
                    var b = componentTypes[j + 1];
                    var customOrder = (long)a.Item2 - b.Item2;
                    if (customOrder > 0 || (customOrder == 0 && a.Item3 > b.Item3))
                    {
                        (componentTypes[j], componentTypes[j + 1]) = (componentTypes[j + 1], componentTypes[j]);
                    }
                }
            }

            // register
            var typeIdMap = new Dictionary<Type, ushort>();
            var nameIdMap = new Dictionary<string, ushort>();
            var updateProcessors = new List<WorldComponentProcessor.UpdateProcessDelegate>();
            var lateUpdateProcessors = new List<WorldComponentProcessor.UpdateProcessDelegate>();
            var processorMethods = typeof(WorldComponentProcessor).GetMethods(BindingFlags.Static | BindingFlags.NonPublic).ToDictionary(k => k.Name);
            WorldComponentManager.SerializableComponents = new IWorldComponentSerializable[WorldComponentManager.ComponentCount];
            for (ushort i = 0; i < WorldComponentManager.ComponentCount; i++)
            {
                var compType = componentTypes[i].Item1;
                var typeId = (ushort)(i + 1);
                WorldComponentManager.ComponentTypes[i] = compType;
                if (typeof(IBytesPackable).IsAssignableFrom(compType))
                {
                    var serializerType = typeof(WorldComponentSerializer<>).MakeGenericType(compType);
                    WorldComponentManager.SerializableComponents[i] = (IWorldComponentSerializable)TypeAssistant.New(serializerType);
                }
                var groupGenericType = WorldComponentManager.ComponentGroupTypes[i] = typeof(WorldComponentGroup<>).MakeGenericType(compType);
                groupGenericType.GetField(TYPE_ID).SetValue(null, typeId);
                if (typeof(IWorldUpdateComponentable).IsAssignableFrom(compType))
                    updateProcessors.Add((WorldComponentProcessor.UpdateProcessDelegate)processorMethods["UpdateProcess"].MakeGenericMethod(compType).CreateDelegate(typeof(WorldComponentProcessor.UpdateProcessDelegate)));
                if (typeof(IWorldLateUpdateComponentable).IsAssignableFrom(compType))
                    lateUpdateProcessors.Add((WorldComponentProcessor.UpdateProcessDelegate)processorMethods["LateUpdateProcess"].MakeGenericMethod(compType).CreateDelegate(typeof(WorldComponentProcessor.UpdateProcessDelegate)));

                if (typeof(IWorldBeginComponentable).IsAssignableFrom(compType))
                    SetField("BeginProcess", compType, groupGenericType);
                if (typeof(IWorldLateBeginComponentable).IsAssignableFrom(compType))
                    SetField("LateBeginProcess", compType, groupGenericType);
                if (typeof(IWorldEndComponentable).IsAssignableFrom(compType))
                    SetField("EndProcess", compType, groupGenericType);
                if (typeof(IWorldPreEndComponentable).IsAssignableFrom(compType))
                    SetField("PreEndProcess", compType, groupGenericType);
                if (compType.IsDefined(typeof(WorldComponentOptionAttribute)))
                    groupGenericType.GetField("Options", BindingFlags.Static | BindingFlags.Public).SetValue(null, compType.GetCustomAttribute<WorldComponentOptionAttribute>().Options);
                typeIdMap.Add(compType, typeId);
                nameIdMap.Add(WorldComponentManager.GetTypeName(compType), i);
            }

            WorldComponentManager.UpdateComponentGroupIds = updateProcessors.ToArray();
            WorldComponentManager.LateUpdateComponentGroupIds = lateUpdateProcessors.ToArray();
            WorldComponentManager.ComponentNameIds = new ReadOnlyDictionary<string, ushort>(nameIdMap);
            WorldComponentManager.ComponentTypeIds = new ReadOnlyDictionary<Type, ushort>(typeIdMap);

            void SetField(string name, Type compType, Type groupGenericType)
            {
                var method = processorMethods[name].MakeGenericMethod(compType).CreateDelegate(typeof(WorldComponentProcessor.LogicProcessDelegate));
                groupGenericType.GetField(name, BindingFlags.Static | BindingFlags.NonPublic)!.SetValue(null, method);
            }
            #endregion

            #region follow
            WorldComponentFollow.AllComponentFollowerDatas = new ReadOnlyDictionary<ushort, WorldComponentFollow.FollowerData[]>(componentFollows.ToDictionary(
                k => WorldComponentManager.ComponentTypeIds[k.Key],
                v => v.Value.Select(attr => new WorldComponentFollow.FollowerData()
                {
                    CompTypeId = WorldComponentManager.ComponentTypeIds[attr.Item1],
                    FollowerAddHook = attr.Item2.FollowerAddHook != null ? (Func<WorldEntity, WorldComponentHandleEx, Action<WorldComponentHandle>, bool>)attr.Item1.GetMethod(attr.Item2.FollowerAddHook, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)?.CreateDelegate(typeof(Func<WorldEntity, WorldComponentHandleEx, Action<WorldComponentHandle>, bool>)) : null,
                }).ToArray()));
            #endregion

            #region behaviors
            behaviors.Sort((a, b) => string.CompareOrdinal(WorldComponentManager.GetTypeName(a.GetType()), WorldComponentManager.GetTypeName(b.GetType())));
            WorldBehaviorSystem.AllBehaviors = new WorldBehavior[behaviors.Count];
            WorldBehaviorSystem.TypeBehaviors = new ReadOnlyDictionary<Type, WorldBehavior>(behaviors.ToDictionary(k => k.GetType(), v => v));
            WorldBehaviorSystem.ParamTypeBehaviors = new ReadOnlyDictionary<ushort, WorldBehavior>(behaviors.Where(v => v is IWorldBehaviorParamTypeGettable).ToDictionary(k => ((IWorldBehaviorParamTypeGettable)k).ParamTypeId, v => v));
            for (var i = (sbyte)(WorldBehaviorSystem.AllBehaviors.Length - 1); i >= 0; i--)
            {
                behaviors[i].TypeId = (byte)(i + 1);
                behaviors[i].GetType().GetField(TYPE_ID)?.SetValue(behaviors[i], behaviors[i].TypeId);
                WorldBehaviorSystem.AllBehaviors[i] = behaviors[i];
            }
            #endregion

            #region sync command
            syncerCommandTypes.Sort((a, b) => string.CompareOrdinal(WorldComponentManager.GetTypeName(a), WorldComponentManager.GetTypeName(b)));
            WorldSyncer.AllExecutors = new WorldSyncCommandExecutor[syncerCommandTypes.Count];
            for (byte i = 0; i < syncerCommandTypes.Count; i++)
            {
                var genericType = typeof(WorldSyncCommandExecutor<>).MakeGenericType(syncerCommandTypes[i]);
                genericType.GetField(TYPE_ID).SetValue(null, (byte)(i + 1));
                var permissionAttr = syncerCommandTypes[i].GetCustomAttribute<WorldSyncPermissionAttribute>(false);
                if (permissionAttr != null)
                    genericType.GetField("Permission").SetValue(null, permissionAttr.Permission);
                var executorType = syncerCommandTypes[i].GetCustomAttribute<WorldSyncCommandExecutorAttribute>(false)?.ExecutorType ?? genericType;
                WorldSyncer.AllExecutors[i] = (WorldSyncCommandExecutor)TypeAssistant.New(executorType);
            }

            WorldSyncer.AllComponentTypeIdPermissions = new byte[WorldComponentManager.ComponentCount + 1];
            foreach (var item in syncerComponentPermissions)
                WorldSyncer.AllComponentTypeIdPermissions[WorldComponentManager.ComponentTypeIds[item.Item1]] = item.Item2;
            #endregion

            #region verify
#if DEBUG
            Task.Run(() =>
            {
                foreach (var asm in TypeAssistant.AllAssemblies)
                {
                    foreach (var type in asm.GetExportedTypes())
                    {
                        if (!type.IsAbstract && !type.IsInterface && (typeof(WorldBehavior).IsAssignableFrom(type) || typeof(WorldEffect).IsAssignableFrom(type)))
                        {
                            foreach (var f in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly))
                            {
                                if (!f.IsDefined(typeof(BytesPackGenFieldAttribute)))
                                    throw new Exception($"must none field {type.FullName}.{f.Name}");
                            }
                        }
                    }
                }
            }).Forget();
#endif
            #endregion
        }

        /// <summary>
        ///
        /// </summary>
        private static void CalcOrderValue(SlimDictionary<Type, (int?, WorldComponentOrderAttribute)> orders, ref (int?, WorldComponentOrderAttribute) data)
        {
            if (data.Item1 != null)
                return;
            if (data.Item2?.After != null)
            {
                ref var afterOrder = ref orders[data.Item2.After];
                CalcOrderValue(orders, ref afterOrder);
                data.Item1 = afterOrder.Item1!.Value + 1; // + data.Item1.GetValueOrDefault();
            }
            else if (data.Item2?.Before != null)
            {
                ref var beforeOrder = ref orders[data.Item2.Before];
                CalcOrderValue(orders, ref beforeOrder);
                data.Item1 = beforeOrder.Item1!.Value - 1; // + data.Item1.GetValueOrDefault();
            }
            else
            {
                data.Item1 = (data.Item2?.Order).GetValueOrDefault() << 16;
            }
        }
    }
}
