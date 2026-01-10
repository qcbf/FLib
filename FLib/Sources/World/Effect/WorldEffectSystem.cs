// =================================================={By Qcbf|qcbf@qq.com|2024-11-03}==================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace FLib.Worlds
{
    /// <summary>
    /// Actor:
    ///   EffectSystem: 
    ///     EffectContext{Effect,Instance}
    ///     EffectContext{Effect,Instance}
    /// </summary>
    [WorldComponentOption(EWorldComponentOption.CallEndOnEntityDestroyed | EWorldComponentOption.Pooling), WorldSyncPermission(false),
     WorldComponentOrder(After = typeof(WorldBehaviorSystem))]
    public class WorldEffectSystem : IBytesPackable, IWorldEndComponentable
    {
        public Dictionary<uint, WorldEffectContext> Effects = new();
        public SlimDictionary<WorldFlags, int> FlagsCount = new();
        private bool _isDisposing;
        public WorldComponentContext SelfContext { get; set; }
        public WorldBase World => SelfContext.World;
        public WorldEntity Entity => SelfContext.Entity;

        public WorldFlags FlagsMask { get; private set; }

        void IWorldEndComponentable.ComponentEnd()
        {
            try
            {
                _isDisposing = true;
                Clear();
            }
            finally
            {
                _isDisposing = false;
                Effects.Clear();
                FlagsCount.Clear();
                FlagsMask = 0;
            }
        }


        /// <summary>
        ///
        /// </summary>
        public bool Contains(uint id) => Effects.ContainsKey(id);

        /// <summary>
        ///
        /// </summary>
        public virtual WorldEffectContext Get(uint id, ELogLevel logLevel = ELogLevel.Fatal) => GetAll(id, logLevel).FirstOrDefault();

        /// <summary>
        ///
        /// </summary>
        public virtual WorldEffectIterator GetAll(uint id, ELogLevel logLevel = ELogLevel.Fatal)
        {
            if (Effects.TryGetValue(id, out var effect))
                return new WorldEffectIterator(effect);
            Log.Get(logLevel)?.Write($"not found effect {id}");
            return default;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual WorldEffectContext Add(WorldEntity createBy, uint id, int addCount = 1)
        {
            if (_isDisposing)
                throw new NullReferenceException();
            if (id == 0)
                throw new Exception("id cannot equal 0");
            if (createBy.IsEmpty)
                createBy = SelfContext.Entity;
            var effectCtx = Effects.GetValueOrDefault(id);
            var effect = WorldInitializer.GetEffectHandle(this, id);
            if (effect == null) throw new Exception($"not found effect: {id}");
            if ((effect.AddOption == EWorldEffectAddOption.IgnoreNew && effectCtx != null) || !effect.Check(this, effectCtx, createBy, id, addCount))
                return null;

            World.Syncer?.AddCommand(Entity, new WorldSyncEffectAddCommand() { AddCount = addCount, CreateBy = createBy, Id = id });

            var evt = new WorldEffectAddEvent(id, effect, createBy, addCount);
            if (Entity.EventDispatcher?.DispatchPreEvent(ref evt) == false)
                return null;

            if (effectCtx == null || effect.AddOption == EWorldEffectAddOption.MultipleInstance)
            {
                Create();
            }
            else
            {
                if (effect.AddOption == EWorldEffectAddOption.ResetTime)
                {
                    effectCtx.UpdaterComponent.RW().StartTime = World.Time;
                    Entity.EventDispatcher?.DispatchEvent(evt);
                    return effectCtx;
                }

                if (effect.AddOption == EWorldEffectAddOption.Replace)
                {
                    Remove(effectCtx);
                    effectCtx = Effects.GetValueOrDefault(id);
                    Create();
                }
                else
                {
                    StackCount();
                }
            }

            Log.Verbose?.Write(Entity, $"id:{id}, by:{createBy.Id}, addStack:{addCount}, {effect.GetType().Name}, {Json5.SerializeToLog(effect)}", "add effect");

            if (effect.AddOption == EWorldEffectAddOption.AddStackWithResetTime)
                effectCtx.UpdaterComponent.RW().StartTime = World.Time;

            if (addCount != 0)
                effect.OnStackCountChange(effectCtx, addCount);

            Entity.EventDispatcher?.DispatchEvent(evt);
            return effectCtx;

            void Create()
            {
                var effectContext = WorldInitializer.CreateEffectContextHandler(id, this);
                effectContext.Id = id;
                effectContext.EffectSys = this;
                effectContext.Effect = effect;
                effectContext.CreateBy = createBy;
                effectContext.UpdaterComponent = new WorldComponentHandleEx<WorldEffectContext.Updater>(SelfContext,
                    SelfContext.World.ComponentMgr.GetGroup<WorldEffectContext.Updater>().Add(SelfContext, new WorldEffectContext.Updater(effectContext)));
                effectContext.Prev = null;
                effectContext.Next = effectCtx;
                if (effectCtx != null)
                {
                    effectCtx.Prev = effectContext;
                    effectContext.LinkedId = effectCtx.LinkedId + 1;
                }
                else
                {
                    effectContext.LinkedId = 1;
                }

                Effects[id] = effectCtx = effectContext;
                FlagsMask |= effect.Flags;
                if (effect.Flags.Mask != 0)
                    ++(FlagsCount ??= new SlimDictionary<WorldFlags, int>(4)).GetOrAddValueRef(effect.Flags);
                StackCount();
                effectContext.Begin();
            }

            void StackCount()
            {
                var maxStackCount = effect.GetMaxStackCount(effectCtx);
                if (maxStackCount > 0)
                {
                    addCount = Math.Min(maxStackCount - effectCtx.StackCount, addCount);
                    if (addCount > 0)
                        effectCtx.StackCount += addCount;
                }
                else
                {
                    effectCtx.StackCount += addCount;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Clear() => Clear(WorldFlags.AllFlags);

        /// <summary>
        ///
        /// </summary>
        public virtual void Clear(WorldFlags flags)
        {
            if (Effects.Count == 0)
                return;
            PooledList<uint> list = new();
            try
            {
                Clear(flags, ref list);
            }
            finally
            {
                list.Dispose();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Clear<T>(WorldFlags flags, ref T removeIds) where T : IList<uint>
        {
            if (Effects.Count == 0)
                return;
            foreach (var effect in Effects)
            {
                if (flags.Any(effect.Value.Effect.GetFlags(effect.Value)))
                    removeIds.Add(effect.Key);
            }

            if (removeIds.Count > 0)
            {
                World.Syncer?.AddCommand(Entity, new WorldSyncEffectClearCommand() { Flags = flags });
                foreach (var id in removeIds)
                {
                    if (Effects.Remove(id, out var ctx))
                    {
                        Log.Verbose?.Write(Entity, $"{ctx.Id}, {ctx.Effect.GetType().Name}", "clear effect");
                        do
                        {
                            var next = ctx.Next;
                            ReleaseEffect(ctx);
                            ctx = next;
                        } while (ctx != null);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool Remove(uint id, int removeCount = 1, int linkedId = 0)
        {
            if (Effects.TryGetValue(id, out var effect))
            {
                if (linkedId == 0 || effect.LinkedId == linkedId)
                {
                    Remove(effect, removeCount);
                    return true;
                }

                foreach (var nextEffect in new WorldEffectIterator(effect.Next))
                {
                    if (nextEffect.LinkedId == linkedId)
                    {
                        Remove(nextEffect, removeCount);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Remove(WorldEffectContext effectContext, int removeCount = 1)
        {
            if (effectContext.IsEmpty)
                return;
            if (removeCount == 0 || removeCount > effectContext.StackCount)
                removeCount = effectContext.StackCount;

            World.Syncer?.AddCommand(Entity, new WorldSyncEffectRemoveCommand() { Id = effectContext.Id, LinkedId = effectContext.LinkedId, RemoveCount = removeCount });

            var evt = new WorldEffectRemoveEvent(effectContext, removeCount);
            if (Entity.EventDispatcher?.DispatchPreEvent(ref evt) == false)
                return;
            if (evt.RemoveCount > effectContext.StackCount)
                evt.RemoveCount = effectContext.StackCount;

            Log.Verbose?.Write(Entity, $"{effectContext.Id}, {effectContext.Effect.GetType().Name}, {evt.RemoveCount}", "remove effect");

            Entity.EventDispatcher?.DispatchEvent(evt);

            effectContext.StackCount -= evt.RemoveCount;
            effectContext.Effect.OnStackCountChange(effectContext, -evt.RemoveCount);
            if (effectContext.StackCount > 0)
                return;
            ReleaseEffectLink(effectContext);
        }

        /// <summary>
        /// 
        /// </summary>
        private void ReleaseEffectLink(WorldEffectContext effectContext)
        {
            try
            {
                if (effectContext.Next == null && effectContext.Prev == null)
                {
                    Effects.Remove(effectContext.Id);
                    var flags = effectContext.Effect.Flags;
                    if (flags != 0 && --FlagsCount[flags] <= 0)
                        FlagsMask &= ~flags;
                }
                else
                {
                    if (effectContext.Next != null)
                        effectContext.Next.Prev = effectContext.Prev;
                    if (effectContext.Prev != null)
                    {
                        effectContext.Prev.Next = effectContext.Next;
                    }
                    else
                    {
                        Effects[effectContext.Id] = effectContext.Next;
                        effectContext.Next!.Prev = effectContext.Prev;
                    }
                }
            }
            finally
            {
                ReleaseEffect(effectContext);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ReleaseEffect(WorldEffectContext ctx)
        {
            try
            {
                if (!_isDisposing || ctx.Effect.CallEndOnDestroyed)
                    ctx.Effect.OnEnd(ctx);
            }
            catch (Exception e)
            {
                Log.Error?.Write($"{ctx.Effect?.GetType()} {e}");
            }
            finally
            {
                ctx.End();
                WorldInitializer.DestroyEffectContextHandler(this, ctx);
            }
        }

        #region implemented interface

        void IBytesPackable.Z_BytesPackWrite(ref BytesPack.KeyHelper key, ref BytesWriter writer)
        {
            key.Push(ref writer, 1);
            if (Effects?.Count > 0)
            {
                foreach (var item in Effects)
                {
                    writer.PushVInt(item.Key);
                    foreach (var e in new WorldEffectIterator(item.Value))
                    {
                        writer.Push(true);
                        writer.PushVInt(e.CreateBy);
                        writer.PushVInt(e.StackCount);
                        writer.Push(e.StartTime);
                        BytesPack.Pack(WorldComponentPack.Create(World, e.Component), ref writer);
                    }

                    writer.Push(false);
                }
            }

            writer.PushVInt(0);
        }

        void IBytesPackable.Z_BytesPackRead(int key, ref BytesReader reader)
        {
            if (key != 1)
                return;
            Clear();
            uint effectKey;
            while ((effectKey = (uint)reader.ReadVInt()) != 0)
            {
                while (reader.Read<bool>())
                {
                    var createBy = (uint)reader.ReadVInt();
                    var stackCount = (int)reader.ReadVInt();
                    var startTime = reader.Read<FNum>();
                    var comp = BytesPack.Unpack<WorldComponentPack>(ref reader);
                    var effect = Add(new WorldEntity(SelfContext.World.Handle, createBy), effectKey, stackCount);
                    if (effect == null) continue;
                    effect.UpdaterComponent.RW().StartTime = startTime;
                    comp.UnpackDataBytes(World, effect.Component);
                }
            }
        }

        #endregion
    }

    public static class WorldEffectSystemHelper
    {
        public static WorldEffectSystem Effect(this in WorldEntity entity, ELogLevel logLevel = ELogLevel.Fatal) => entity.GetRO<WorldEffectSystem>(logLevel);
        public static WorldEffectSystem Effect(this in WorldComponentContext entity, ELogLevel logLevel = ELogLevel.Fatal) => entity.Entity.GetRO<WorldEffectSystem>(logLevel);
    }
}