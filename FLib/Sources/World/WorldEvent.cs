//==================={By Qcbf|qcbf@qq.com|9/15/2023 10:24:44 AM}===================

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FLib;

namespace FLib.Worlds
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class WorldEventBase : FEvent
    {
        protected override void ThrowEventError(Exception ex, in FEventListenData eventListenData) => Log.Error?.Write($"dispatch event error: {eventListenData.Handler}\n{ex}");
    }

    /// <summary>
    /// 
    /// </summary>
    public class EntityWorldEvent : WorldEventBase
    {
        public WorldEntity Entity;
        public WorldBase World => Entity.World;
        public EntityWorldEvent() { }
        public EntityWorldEvent(WorldEntity entity) => Entity = entity;

#if UNITY_2021_1_OR_NEWER
        [UnityEngine.HideInCallstack]
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool DispatchPreEvent<T>(in WorldEntity entity, ref T evtData) => DispatchPreEvent(entity, typeof(T).GetHashCode(), ref evtData);

#if UNITY_2021_1_OR_NEWER
        [UnityEngine.HideInCallstack]
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DispatchEvent<T>(in WorldEntity entity, T evtData) => DispatchEvent(entity, typeof(T).GetHashCode(), evtData);

#if UNITY_2021_1_OR_NEWER
        [UnityEngine.HideInCallstack]
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool DispatchPreEvent<T>(in WorldEntity entity, int evtId, ref T evtData)
        {
            if (entity.EventDispatcher == null)
            {
                var e = GlobalObjectPool<EntityWorldEvent>.Create();
                try
                {
                    e.Entity = entity;
                    return entity.World.DispatchPreEventById(evtId, ref evtData, e);
                }
                finally
                {
                    GlobalObjectPool<EntityWorldEvent>.Release(e);
                }
            }
            return entity.World.DispatchPreEventById(evtId, ref evtData, entity.EventDispatcher) && entity.EventDispatcher.DispatchPreEventById(evtId, ref evtData);
        }
#if UNITY_2021_1_OR_NEWER
        [UnityEngine.HideInCallstack]
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DispatchEvent<T>(in WorldEntity entity, int evtId, in T evtData)
        {
            if (entity.EventDispatcher == null)
            {
                var e = GlobalObjectPool<EntityWorldEvent>.Create();
                try
                {
                    e.Entity = entity;
                    entity.World.DispatchEventById(evtId, evtData, e);
                }
                finally
                {
                    GlobalObjectPool<EntityWorldEvent>.Release(e);
                }
            }
            else
            {
                entity.World.DispatchEventById(evtId, evtData, entity.EventDispatcher);
                entity.EventDispatcher.DispatchEventById(evtId, evtData);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class BroadcastWorldEvent : WorldEventBase
    {
        public WorldBase World;
        public BroadcastWorldEvent(WorldBase world) => World = world;
        protected override void ThrowEventError(Exception ex, in FEventListenData eventListenData) => Log.Error?.Write($"dispatch event error: {eventListenData.Handler}\n{ex}");
#if UNITY_2021_1_OR_NEWER
        [UnityEngine.HideInCallstack]
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool DispatchPreEventById<T>(int evtId, ref T evtData, object dispatcher = null)
        {
            return World.DispatchPreEventById(evtId, ref evtData, dispatcher ?? this) && base.DispatchPreEventById(evtId, ref evtData, dispatcher);
        }
#if UNITY_2021_1_OR_NEWER
        [UnityEngine.HideInCallstack]
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void DispatchEventById<T>(int evtId, in T value, object dispatcher = null)
        {
            World.DispatchEventById(evtId, value, dispatcher ?? this);
            base.DispatchEventById(evtId, value, dispatcher);
        }
    }
}
