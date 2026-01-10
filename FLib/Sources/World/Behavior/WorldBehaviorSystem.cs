// =================================================={By Qcbf|qcbf@qq.com|2024-10-31}==================================================

using System;
using System.Buffers;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using FLib.Worlds;

namespace FLib.Worlds
{
    /// <summary>
    /// 行为系统逻辑组件
    /// </summary>
    [WorldComponentOption(EWorldComponentOption.CallEndOnEntityDestroyed | EWorldComponentOption.Pooling), WorldSyncPermission(false), WorldComponentOrder(Order = short.MinValue >> 1)]
    public class WorldBehaviorSystem : FEvent, IWorldEndComponentable, IBytesPackable, IObjectPoolDeactivatable
    {
        public static WorldBehavior GlobalDefaultBehavior = null!;
        public static WorldBehavior[] AllBehaviors = null!;
        public static ReadOnlyDictionary<Type, WorldBehavior> TypeBehaviors = null!;
        public static ReadOnlyDictionary<ushort, WorldBehavior> ParamTypeBehaviors = null!;

        public WorldBehavior DefaultBehavior = GlobalDefaultBehavior;
        public WorldBehaviorContext Primary;
        public WorldBehaviorContext Secondary;

        public WorldComponentContext SelfContext { get; set; }
        public WorldBase World => SelfContext.World;
        public WorldEntity Entity => SelfContext.Entity;
        public override string ToString() => $"{Primary}|{Secondary}";

        #region implemented interface

        void IWorldEndComponentable.ComponentEnd()
        {
            Release(Secondary);
            Release(Primary);
            Primary = Secondary = null;
            return;

            void Release(WorldBehaviorContext ctx)
            {
                if (ctx == null)
                    return;
                ctx.RemoveInstanceComp();
                ctx.ParamComp.OnlyRemoveThisComponent(SelfContext);
                MultiObjectPool.Global.Release(ctx);
            }
        }

        void IObjectPoolDeactivatable.ObjectPoolDeactivatable()
        {
            DefaultBehavior = GlobalDefaultBehavior;
            ClearListenEvents();
        }

        #region serializer

        void IBytesPackable.Z_BytesPackWrite(ref BytesPack.KeyHelper key, ref BytesWriter writer)
        {
            key.Push(ref writer, 1);
            writer.Push((Primary?.Behavior.TypeId).GetValueOrDefault());
            writer.Push((Secondary?.Behavior.TypeId).GetValueOrDefault());
            if (Primary != null)
            {
                WriteContextInfo(ref writer, Primary);
            }

            if (Secondary != null)
            {
                WriteContextInfo(ref writer, Secondary);
            }

            return;

            static void WriteContextInfo(ref BytesWriter writer, WorldBehaviorContext context)
            {
                writer.Push(!context.ParamComp.IsEmpty);
                if (!context.ParamComp.IsEmpty)
                {
                    BytesPack.Pack(WorldComponentPack.Create(context.World, context.ParamComp), ref writer);
                }

                writer.Push(!context.InstanceComp.IsEmpty);
                if (!context.InstanceComp.IsEmpty)
                {
                    BytesPack.Pack(WorldComponentPack.Create(context.World, context.InstanceComp), ref writer);
                }
            }
        }

        void IBytesPackable.Z_BytesPackRead(int key, ref BytesReader reader)
        {
            if (key != 1) return;
            var primaryTypeId = reader.Read<byte>();
            var secondaryTypeId = reader.Read<byte>();
            Do(ref reader, primaryTypeId, Primary);
            Do(ref reader, secondaryTypeId, Secondary);
            return;

            void Do(ref BytesReader reader, byte typeId, WorldBehaviorContext context)
            {
                if (typeId > 0)
                {
                    WorldComponentPack inst = default, param = default;
                    if (reader.Read<bool>())
                        BytesPack.Unpack(ref param, ref reader);
                    if (reader.Read<bool>())
                        BytesPack.Unpack(ref inst, ref reader);

                    if (context.Behavior.TypeId != typeId)
                    {
                        if (this.Do(EWorldBehaviorDoResult.SuccessPrimary, AllBehaviors[typeId - 1], param) != EWorldBehaviorDoResult.Failure)
                            inst.UnpackDataBytes(context.World, context.InstanceComp);
                    }

                    param.UnpackDataBytes(World, context.ParamComp);
                    param.UnpackDataBytes(World, context.InstanceComp);
                }
                else if (context != null)
                {
                    Stop(context.Behavior);
                }
            }
        }

        #endregion

        #endregion

        /// <summary>
        /// 
        /// </summary>
        public static WorldBehavior GetBehavior<T>() where T : WorldBehavior => TypeBehaviors[typeof(T)];

        /// <summary>
        /// 
        /// </summary>
        public EWorldBehaviorDoResult DoDefault() => Do(DefaultBehavior);

        /// <summary>
        /// 执行行为, 带参数
        /// </summary>
        public EWorldBehaviorDoResult Do<TParam>(in TParam p) where TParam : IWorldComponentable, new() => Do(ParamTypeBehaviors[WorldComponentGroup<TParam>.TypeId], p);

        /// <summary>
        /// 执行行为, 带参数
        /// </summary>
        public EWorldBehaviorDoResult Do<TParam>(WorldBehavior doBehavior, in TParam p) where TParam : IWorldComponentable, new()
        {
            return Do(doBehavior, new WorldComponentHandleEx(World, new WorldComponentHandle(WorldComponentGroup<TParam>.TypeId, World.ComponentMgr.GetGroup<TParam>().Add(Entity, p))));
        }

        /// <summary>
        /// 执行行为, 带参数
        /// </summary>
        public EWorldBehaviorDoResult Do(WorldBehavior doBehavior, in WorldComponentHandleEx paramCompHandle)
        {
            var rt = EWorldBehaviorDoResult.Failure;
            try
            {
                rt = CheckDo(doBehavior, paramCompHandle);
                if (rt != EWorldBehaviorDoResult.Failure)
                {
                    World.Syncer?.AddCommand(Entity, new WorldSyncBehaviorDoCommand() { TypeId = doBehavior.TypeId, ArgComponent = WorldComponentPack.Create(World, paramCompHandle) });
                    rt = DoImpl(rt, doBehavior, paramCompHandle);
                }
            }
            finally
            {
                if (rt == EWorldBehaviorDoResult.Failure)
                    paramCompHandle.OnlyRemoveThisComponent();
            }

            return rt;
        }

        /// <summary>
        /// 执行行为
        /// </summary>
        public EWorldBehaviorDoResult Do<TBehavior>() where TBehavior : WorldBehavior => Do(TypeBehaviors[typeof(TBehavior)]);

        /// <summary>
        /// 执行行为
        /// </summary>
        public EWorldBehaviorDoResult Do(WorldBehavior doBehavior)
        {
            var rt = CheckDo(doBehavior);
            if (rt == EWorldBehaviorDoResult.Failure)
                return rt;
            World.Syncer?.AddCommand(Entity, new WorldSyncBehaviorDoCommand() { TypeId = doBehavior.TypeId });
            DoImpl(rt, doBehavior);
            return rt;
        }


        /// <summary>
        ///
        /// </summary>
        protected internal virtual EWorldBehaviorDoResult Do(EWorldBehaviorDoResult? result, WorldBehavior doBehavior, in WorldComponentPack compPack)
        {
            result ??= CheckDo(doBehavior);
            if (result == EWorldBehaviorDoResult.Failure)
                return result.Value;
            if (compPack.IsEmpty)
            {
                World.Syncer?.AddCommand(Entity, new WorldSyncBehaviorDoCommand() { TypeId = doBehavior.TypeId });
                return DoImpl(result.Value, doBehavior);
            }

            WorldComponentHandleEx paramCompHandle = default;
            try
            {
                paramCompHandle = compPack.Add(Entity, false).Cast(World);
                World.Syncer?.AddCommand(Entity, new WorldSyncBehaviorDoCommand() { TypeId = doBehavior.TypeId, ArgComponent = WorldComponentPack.Create(World, paramCompHandle) });
                return DoImpl(result.Value, doBehavior, paramCompHandle);
            }
            finally
            {
                if (result == EWorldBehaviorDoResult.Failure)
                    paramCompHandle.OnlyRemoveThisComponent();
            }
        }

        /// <summary>
        ///
        /// </summary>
        protected internal virtual EWorldBehaviorDoResult DoImpl(EWorldBehaviorDoResult result, WorldBehavior doBehavior, in WorldComponentHandleEx paramComp = default)
        {
            if (result == EWorldBehaviorDoResult.Failure)
                return result;

            var evt = new DoBehaviorEvent(Entity, doBehavior, null, result == EWorldBehaviorDoResult.SuccessPrimary);
            Log.Verbose?.Write(Entity, $"{doBehavior.GetType()}|{paramComp.ToString(true)}", "do behavior");
            WorldBehaviorContext context = null;
            if (evt.IsMainBehavior)
            {
                if (Primary != null)
                {
                    if (Primary.Behavior == doBehavior)
                    {
                        evt.RunningContext = Primary;
                        if (!DispatchPreEventById(doBehavior.TypeId, ref evt) || !DispatchPreEvent(ref evt))
                            return EWorldBehaviorDoResult.Failure;
                        DoSameBehavior(Primary, paramComp, result);
                        DispatchEventById(doBehavior.TypeId, evt);
                        DispatchEvent(evt);
                        if (Secondary != null && !doBehavior.IsFriend(SelfContext, Primary, Secondary.Behavior, paramComp))
                            StopImpl(ref Secondary);
                        return result;
                    }

                    if (doBehavior.IsFriend(SelfContext, null, Primary.Behavior, paramComp))
                    {
                        if (!DispatchPreEventById(doBehavior.TypeId, ref evt) || !DispatchPreEvent(ref evt))
                            return EWorldBehaviorDoResult.Failure;
                        if (Secondary != null)
                            StopImpl(ref Secondary);
                        Secondary = Primary;
                        Primary = null;
                    }
                    else
                    {
                        if (!DispatchPreEventById(doBehavior.TypeId, ref evt) || !DispatchPreEvent(ref evt))
                            return EWorldBehaviorDoResult.Failure;
                        if (Primary != null)
                            StopImpl(ref Primary, false);
                        // 停止了主行为还需要继续检查子行为是否友好
                        if (Secondary != null && !doBehavior.IsFriend(SelfContext, null, Secondary.Behavior, paramComp))
                            StopImpl(ref Secondary);
                    }
                }
                else if (!DispatchPreEventById(doBehavior.TypeId, ref evt) || !DispatchPreEvent(ref evt))
                {
                    return EWorldBehaviorDoResult.Failure;
                }

                if (Primary != null)
                {
                    return DoImpl(CheckDo(doBehavior, paramComp), doBehavior, paramComp);
                }

                context = Primary = MultiObjectPool.Global.Create<WorldBehaviorContext>();
            }
            else if (result == EWorldBehaviorDoResult.SuccessSecondary)
            {
                if (Secondary != null)
                {
                    if (Secondary.Behavior == doBehavior)
                    {
                        evt.RunningContext = Secondary;
                        if (DispatchPreEventById(doBehavior.TypeId, ref evt) || DispatchPreEvent(ref evt))
                        {
                            DoSameBehavior(Secondary, paramComp, result);
                            DispatchEventById(doBehavior.TypeId, evt);
                            DispatchEvent(evt);
                            return result;
                        }

                        return EWorldBehaviorDoResult.Failure;
                    }

                    if (!DispatchPreEventById(doBehavior.TypeId, ref evt) || !DispatchPreEvent(ref evt))
                        return EWorldBehaviorDoResult.Failure;
                    StopImpl(ref Secondary);
                }
                else if (!DispatchPreEventById(doBehavior.TypeId, ref evt) || !DispatchPreEvent(ref evt))
                {
                    return EWorldBehaviorDoResult.Failure;
                }

                if (Secondary != null)
                {
                    return DoImpl(CheckDo(doBehavior, paramComp), doBehavior, paramComp);
                }

                context = Secondary = MultiObjectPool.Global.Create<WorldBehaviorContext>();
            }

            Log.AssertNotNull(context);

            context.BehaviorSys = this;
            context.StartFrame = World.Frame;
            context.Behavior = doBehavior;
            context.Priority = doBehavior.GetPriority(SelfContext, paramComp);
            context.ParamComp = paramComp;
            doBehavior.OnBegin(context, true, default, result);
            if (context.Behavior != doBehavior)
                return EWorldBehaviorDoResult.Failure;
            evt.RunningContext = context;
            evt.IsFirst = true;
            DispatchEventById(doBehavior.TypeId, evt);
            DispatchEvent(evt);
            return result;
        }

        /// <summary>
        ///
        /// </summary>
        protected virtual void DoSameBehavior(WorldBehaviorContext context, in WorldComponentHandle paramComp, EWorldBehaviorDoResult result)
        {
            var oldParamComp = context.ParamComp;
            context.ParamComp = paramComp;
            context.Behavior.OnBegin(context, false, oldParamComp.Cast(World), result);
            if (!oldParamComp.IsEmpty)
                oldParamComp.OnlyRemoveThisComponent(Entity);
        }

        /// <summary>
        /// 检查执行, 并不是真的执行, 得到一个初步能否执行的结果
        /// </summary>
        protected internal virtual EWorldBehaviorDoResult CheckDo(WorldBehavior doBehavior, in WorldComponentHandleEx paramComp = default)
        {
            var result = EWorldBehaviorDoResult.SuccessPrimary;
            if (Primary?.Behavior == doBehavior)
                result = doBehavior!.CheckDo(SelfContext, Primary, paramComp) ? EWorldBehaviorDoResult.SuccessPrimary : EWorldBehaviorDoResult.Failure;
            else if (Secondary?.Behavior == doBehavior)
                result = doBehavior!.CheckDo(SelfContext, Secondary, paramComp) ? EWorldBehaviorDoResult.SuccessSecondary : EWorldBehaviorDoResult.Failure;
            else if (!doBehavior.CheckDo(SelfContext, null, paramComp))
                result = EWorldBehaviorDoResult.Failure;
            else
            {
                var doBehaviorPriority = doBehavior.GetPriority(SelfContext, paramComp);
                if (Primary?.Behavior!.CheckPriority(SelfContext, Primary, doBehaviorPriority, doBehavior) != false) return result;
                if (Primary.Behavior.IsFriend(SelfContext, Primary, doBehavior, paramComp) &&
                    Secondary?.Behavior!.CheckPriority(SelfContext, Secondary, doBehaviorPriority, doBehavior) != false)
                    result = EWorldBehaviorDoResult.SuccessSecondary;
                else
                    result = EWorldBehaviorDoResult.Failure;
            }

            Log.Verbose?.Write($"{doBehavior.GetType().Name} {result} {SelfContext.Entity.ToString(result == EWorldBehaviorDoResult.Failure)}", "check behavior");
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual WorldBehaviorContext Get<T>(ELogLevel logLevel = ELogLevel.None) where T : WorldBehavior, new()
        {
            if (Primary?.Behavior is T)
                return Primary;
            if (Secondary?.Behavior is T)
                return Secondary;
            Log.Get(logLevel)?.Write($"not found behavior: {typeof(T)}");
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual WorldComponentHandleEx<T> GetInstance<T>(ELogLevel logLevel = ELogLevel.Error) where T : IWorldComponentable, new()
        {
            var compTypeId = WorldComponentGroup<T>.TypeId;
            if (Primary?.InstanceComp.TypeId == compTypeId)
                return Primary.InstanceComp.Cast<T>(World);
            if (Secondary?.InstanceComp.TypeId == compTypeId)
                return Secondary.InstanceComp.Cast<T>(World);
            Log.Get(logLevel)?.Write($"not found behavior: {typeof(T)}");
            return default;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual WorldComponentHandleEx<T> GetParam<T>(ELogLevel logLevel = ELogLevel.Error) where T : IWorldComponentable, new()
        {
            var compTypeId = WorldComponentGroup<T>.TypeId;
            if (Primary?.ParamComp.TypeId == compTypeId)
                return Primary.ParamComp.Cast<T>(World);
            if (Secondary?.ParamComp.TypeId == compTypeId)
                return Secondary.ParamComp.Cast<T>(World);
            Log.Get(logLevel)?.Write($"not found behavior: {typeof(T)}");
            return default;
        }

        /// <summary>
        /// 指定行为是否正在运行
        /// </summary>
        public virtual bool IsRunning<T>() where T : WorldBehavior, new()
        {
            return Primary?.Behavior is T || Secondary?.Behavior is T;
        }

        /// <summary>
        /// 指定行为是否正在运行
        /// </summary>
        public virtual bool IsRunning(WorldBehavior behavior)
        {
            return Primary?.Behavior == behavior || Secondary?.Behavior == behavior;
        }

        /// <summary>
        /// 指定行为是否正在运行
        /// </summary>
        public virtual bool IsRunning(uint mask)
        {
            return (((Primary?.Behavior.Mask).GetValueOrDefault() | (Secondary?.Behavior.Mask).GetValueOrDefault()) & mask) != 0;
        }

        /// <summary>
        /// 停止指定行为
        /// </summary>
        public virtual bool Stop<T>(bool isDoDefault = true) => Stop(TypeBehaviors[typeof(T)], isDoDefault);

        /// <summary>
        /// 停止指定行为
        /// </summary>
        public virtual bool Stop(WorldBehavior behavior, bool isDoDefault = true)
        {
            if (Primary?.Behavior == behavior)
            {
                World.Syncer?.AddCommand(Entity, new WorldSyncBehaviorStopCommand() { TypeId = behavior!.TypeId, IsDoDefault = isDoDefault });
                StopImpl(ref Primary);
                if (isDoDefault && Primary == null)
                    DoImpl(CheckDo(DefaultBehavior), DefaultBehavior);
                return true;
            }

            if (Secondary?.Behavior != behavior) return false;
            World.Syncer?.AddCommand(Entity, new WorldSyncBehaviorStopCommand() { TypeId = behavior.TypeId, IsDoDefault = isDoDefault });
            StopImpl(ref Secondary);
            return false;
        }

        /// <summary>
        /// 停止全部行为
        /// </summary>
        public virtual void StopAll(bool isDoDefault = true)
        {
            if (Primary == null && Secondary == null)
                return;
            World.Syncer?.AddCommand(Entity, new WorldSyncBehaviorStopCommand() { IsDoDefault = isDoDefault });
            if (Secondary != null)
            {
                StopImpl(ref Secondary);
            }

            if (Primary != null)
            {
                StopImpl(ref Primary);
            }

            if (isDoDefault && Primary == null)
                DoImpl(CheckDo(DefaultBehavior), DefaultBehavior);
        }

        /// <summary>
        /// 停止指定行为
        /// </summary>
        protected internal virtual void StopImpl(ref WorldBehaviorContext context, bool isSort = true)
        {
            Log.Verbose?.Write(SelfContext, $"{context.ToString(true)}", "stop behavior");

            var isMainBehavior = context == Primary;
            var stopContext = context;
            context = null;
            if (isSort && isMainBehavior && Secondary != null)
            {
                Primary = Secondary;
                Secondary = null;
                Primary.Behavior.OnBegin(Primary, false, Primary.ParamComp.Cast(World), EWorldBehaviorDoResult.SuccessPrimary);
            }

            var inst = stopContext.InstanceComp;
            var param = stopContext.ParamComp;
            try
            {
                var e = new StopBehaviorEvent(SelfContext, stopContext, isMainBehavior);
                DispatchEventById(stopContext.Behavior.TypeId << 16, e);
                DispatchEvent(e);
                stopContext.Behavior.OnEnd(stopContext);
            }
            finally
            {
                inst.OnlyRemoveThisComponent(SelfContext);
                param.OnlyRemoveThisComponent(SelfContext);
                MultiObjectPool.Global.Release(stopContext);
            }
        }

        #region events

        /// <summary>
        /// 
        /// </summary>
        public FEventListenHelper<DoBehaviorEvent> ListenDoBehaviorEvent<T>(PostEventHandler<DoBehaviorEvent> handler, short priority = 0, bool isListenOnce = false)
            where T : WorldBehavior, new() => ListenEvent(TypeBehaviors[typeof(T)].TypeId, handler, priority, isListenOnce);

        /// <summary>
        /// 
        /// </summary>
        public FEventListenHelper<StopBehaviorEvent> ListenStopBehaviorEvent<T>(PostEventHandler<StopBehaviorEvent> handler, short priority = 0, bool isListenOnce = false)
            where T : WorldBehavior, new() => ListenEvent(TypeBehaviors[typeof(T)].TypeId << 16, handler, priority, isListenOnce);

        /// <summary>
        /// 
        /// </summary>
        public void UnlistenDoBehaviorEvent<T>(PostEventHandler<DoBehaviorEvent> handler)
            where T : WorldBehavior, new() => UnlistenEventImpl(TypeBehaviors[typeof(T)].TypeId, handler);

        /// <summary>
        /// 
        /// </summary>
        public void UnlistenStopBehaviorEvent<T>(PostEventHandler<StopBehaviorEvent> handler)
            where T : WorldBehavior, new() => UnlistenEventImpl(TypeBehaviors[typeof(T)].TypeId << 16, handler);

        /// <summary>
        /// 
        /// </summary>
        public FEventListenHelper<DoBehaviorEvent> ListenDoBehaviorEvent(PostEventHandler<DoBehaviorEvent> handler, short priority = 0, bool isListenOnce = false)
            => ListenEvent(handler, priority, isListenOnce);

        /// <summary>
        /// 
        /// </summary>
        public FEventListenHelper<StopBehaviorEvent> ListenStopBehaviorEvent(PostEventHandler<StopBehaviorEvent> handler, short priority = 0, bool isListenOnce = false)
            => ListenEvent(handler, priority, isListenOnce);

        /// <summary>
        /// 
        /// </summary>
        public void UnlistenDoBehaviorEvent(PostEventHandler<DoBehaviorEvent> handler)
            => UnlistenEvent(handler);

        /// <summary>
        /// 
        /// </summary>
        public void UnlistenStopBehaviorEvent(PostEventHandler<StopBehaviorEvent> handler)
            => UnlistenEvent(handler);

        #endregion
    }

    public static class WorldBehaviorSystemHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WorldBehaviorSystem Behavior(this in WorldEntity entity, ELogLevel logLevel = ELogLevel.Fatal) => entity.GetRO<WorldBehaviorSystem>(logLevel);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WorldBehaviorSystem Behavior(this in WorldComponentContext entity, ELogLevel logLevel = ELogLevel.Fatal) => entity.Entity.GetRO<WorldBehaviorSystem>(logLevel);
    }
}