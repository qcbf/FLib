// ==================== qcbf@qq.com | 2026-03-02 ====================

#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using FLib.WorldCores.Components;

namespace FLib.WorldCores.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public readonly struct WorldEntity : IEquatable<WorldEntity>, IJson5Serializable
    {
        public readonly WorldHandle WorldHandle;
        public readonly WorldEntityId Id;
        public WorldCore World => WorldHandle.World;
        public bool IsEmpty => Id.IsEmpty || WorldHandle.IsEmpty;
        public override string ToString() => Id.ToString();

        public WorldEntity(WorldHandle world, WorldEntityId eId)
        {
            WorldHandle = world;
            Id = eId;
        }

        #region Simple

        /// <summary>
        /// 获取实体的组件（静态或动态）。
        /// </summary>
        public ref T Get<T>() => ref World.Get<T>(Id);

        /// <summary>
        /// 获取实体的组件（静态或动态），通过类型参数。
        /// </summary>
        public object Get(Type componentType) => World.Get(Id, componentType);

        /// <summary>
        /// 设置实体的组件（如果存在静态则写入静态，否则写入动态）。
        /// </summary>
        public void Set<T>(in T component) => World.Set(Id, component);

        /// <summary>
        /// 移除实体上的组件（只能用于动态组件）。
        /// </summary>
        public void Remove<T>() => World.Remove<T>(Id);

        /// <summary>
        /// 判断实体是否包含某组件（静态或动态）。
        /// </summary>
        public bool Has<T>() => World.Has<T>(Id);

        /// <summary>
        /// 获得实体上所有组件的列表。
        /// </summary>
        public IList GetAll(IList? result = null) => World.GetAll(Id, result);

        /// <summary>
        /// 获得实体上所有组件类型的列表。
        /// </summary>
        public List<Type> GetAllTypes(List<Type>? result = null) => World.GetAllTypes(Id, result);

        #endregion

        #region Static

        /// <summary>
        /// 获取实体的静态组件并返回一个 <see cref="Ref{T}"/> 包装。
        /// </summary>
        public Ref<T> GetSta<T>() where T : unmanaged => World.GetSta<T>(Id);

        /// <summary>
        /// 获取实体的静态组件值（非管理）。如果没有则返回默认值。
        /// </summary>
        public Ref<T> GetStaOrEmpty<T>() where T : unmanaged => World.GetStaOrEmpty<T>(Id);

        /// <summary>
        /// 获取实体的静态组件并返回对组件的引用。
        /// </summary>
        public ref T GetStaRef<T>() where T : unmanaged => ref World.GetStaRef<T>(Id);

        /// <summary>
        /// 获取实体的静态管理组件（<see cref="Mng{T}"/>）。
        /// </summary>
        public Mng<T> GetStaMng<T>() => World.GetStaMng<T>(Id);

        /// <summary>
        /// 设置实体的静态组件值。
        /// </summary>
        public void SetSta<T>(in T val) where T : unmanaged => World.SetSta(Id, val);

        /// <summary>
        /// 设置实体的静态管理组件值。
        /// </summary>
        public void SetStaMng<T>(in T val) => World.SetStaMng(Id, val);

        /// <summary>
        /// 设置实体的共享组件值。
        /// </summary>
        public void SetShared<T>(in T val) where T : IWorldSharedComponent => World.SetShared(Id, val);

        /// <summary>
        /// 检查实体是否包含指定的静态组件。
        /// </summary>
        public bool HasSta<T>() where T : unmanaged => World.HasSta<T>(Id);

        /// <summary>
        /// 检查实体是否包含指定的静态管理组件。
        /// </summary>
        public bool HasStaMng<T>() => World.HasStaMng<T>(Id);

        /// <summary>
        /// 检查实体是否包含指定类型的静态组件。
        /// </summary>
        public bool HasSta(Type componentType) => World.HasSta(Id, componentType);

        /// <summary>
        /// 获取实体的指定类型静态组件（非泛型）。
        /// </summary>
        public object GetSta(Type componentType) => World.GetSta(Id, componentType);

        #endregion

        #region Dynamic

        /// <summary>
        /// 获取实体的动态组件并返回引用。
        /// </summary>
        public bool TryGetDyn<T>([MaybeNullWhen(false)] out T component)
        {
            if (World.HasDyn<T>(Id))
            {
                component = World.GetDyn<T>(Id);
                return true;
            }

            component = default;
            return false;
        }

        /// <summary>
        /// 获取实体的动态组件并返回引用。
        /// </summary>
        public ref T GetDyn<T>() => ref World.GetDyn<T>(Id);

        /// <summary>
        /// 获取实体的动态组件（非泛型）。
        /// </summary>
        public object GetDyn(Type type) => World.GetDyn(Id, type);

        /// <summary>
        /// 设置实体的动态组件。
        /// </summary>
        public int SetDyn<T>(in T component) => World.SetDyn(Id, component);

        /// <summary> 设置实体的动态组件（通过对象） </summary>
        public int SetDynObject(object? component, Type? componentType = null) => World.SetDynObject(Id, component, componentType);

        /// <summary> 设置实体的动态组件（通过脚本） </summary>
        public int SetDynScript(in ScriptPackBytes script) => World.SetDynScript(Id, script);

        /// <summary>
        /// 移除实体的动态组件。
        /// </summary>
        public void RemoveDyn<T>() => World.RemoveDyn<T>(Id);

        /// <summary>
        /// 移除实体的动态组件（通过类型）。
        /// </summary>
        public void RemoveDyn(Type type) => World.RemoveDyn(Id, type);

        /// <summary>
        /// 检查实体是否包含指定的动态组件。
        /// </summary>
        public bool HasDyn<T>() => World.HasDyn<T>(Id);

        /// <summary>
        /// 检查实体是否包含指定类型的动态组件。
        /// </summary>
        public bool HasDyn(Type type) => World.HasDyn(Id, type);

        #endregion

        #region Events

        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// 分发事件（后处理）。
        /// </summary>
#if UNITY_2021_1_OR_NEWER
        [UnityEngine.HideInCallstack]
#endif
        public void DispatchEvent<T>(in T evtData)
        {
            var etEvt = World.Entities.GetDispatchEvent(Id.Id);
            if (etEvt == null)
            {
                using var e = new GlobalObjectPoolAutoVal<EntityEvent>();
                e.Val.Entity = this;
                World.DispatchEvent(evtData, e.Val);
            }
            else
            {
                etEvt.DispatchEvent(evtData);
                World.DispatchEvent(evtData, etEvt);
            }
        }

        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// 按 ID 分发事件（后处理）。
        /// </summary>
#if UNITY_2021_1_OR_NEWER
        [UnityEngine.HideInCallstack]
#endif
        public void DispatchEventById<T>(int evtId, in T evtData)
        {
            var etEvt = World.Entities.GetDispatchEvent(Id.Id);
            if (etEvt == null)
            {
                using var e = new GlobalObjectPoolAutoVal<EntityEvent>();
                e.Val.Entity = this;
                World.DispatchEventById(evtId, evtData, e.Val);
            }
            else
            {
                etEvt.DispatchEventById(evtId, evtData);
                World.DispatchEventById(evtId, evtData, etEvt);
            }
        }

        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// 按 ID 分发空事件（后处理）。
        /// </summary>
#if UNITY_2021_1_OR_NEWER
        [UnityEngine.HideInCallstack]
#endif
        public void DispatchEventById(int evtId)
        {
            var etEvt = World.Entities.GetDispatchEvent(Id.Id);
            if (etEvt == null)
            {
                using var e = new GlobalObjectPoolAutoVal<EntityEvent>();
                e.Val.Entity = this;
                World.DispatchEventById(evtId, e.Val);
            }
            else
            {
                etEvt.DispatchEventById(evtId);
                World.DispatchEventById(evtId, etEvt);
            }
        }

        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// 分发前置事件，返回是否继续执行。
        /// </summary>
        /// <returns>is continuing run</returns>
#if UNITY_2021_1_OR_NEWER
        [UnityEngine.HideInCallstack]
#endif
        public bool DispatchPreEvent<T>(ref T evtData)
        {
            var etEvt = World.Entities.GetDispatchEvent(Id.Id);
            if (etEvt == null)
            {
                using var e = new GlobalObjectPoolAutoVal<EntityEvent>();
                e.Val.Entity = this;
                return World.DispatchPreEvent(ref evtData, e.Val);
            }

            return World.DispatchPreEvent(ref evtData, etEvt) && etEvt.DispatchPreEvent(ref evtData);
        }

        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// 按 ID 分发前置事件，返回是否继续执行。
        /// </summary>
        /// <returns>is continuing run</returns>
#if UNITY_2021_1_OR_NEWER
        [UnityEngine.HideInCallstack]
#endif
        public bool DispatchPreEventById<T>(int evtId, ref T evtData)
        {
            var etEvt = World.Entities.GetDispatchEvent(Id.Id);
            if (etEvt == null)
            {
                using var e = new GlobalObjectPoolAutoVal<EntityEvent>();
                e.Val.Entity = this;
                return World.DispatchPreEventById(evtId, ref evtData, e.Val);
            }

            return World.DispatchPreEventById(evtId, ref evtData, etEvt) && etEvt.DispatchPreEventById(evtId, ref evtData);
        }

        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// 按 ID 分发前置空事件，返回是否继续执行。
        /// </summary>
        /// <returns>is continuing run</returns>
#if UNITY_2021_1_OR_NEWER
        [UnityEngine.HideInCallstack]
#endif
        public bool DispatchPreEventById(int evtId)
        {
            var etEvt = World.Entities.GetDispatchEvent(Id.Id);
            if (etEvt == null)
            {
                using var e = new GlobalObjectPoolAutoVal<EntityEvent>();
                e.Val.Entity = this;
                return World.DispatchPreEventById(evtId, e.Val);
            }

            return World.DispatchPreEventById(evtId, etEvt) && etEvt.DispatchPreEventById(evtId);
        }

        /// <summary>
        /// 监听事件（后处理）。
        /// </summary>
        public FEventListenHelper ListenEvent<T>(FEvent.PostEventHandler<T> handler, short priority = 0, bool isListenOnce = false)
            => World.Entities.GetEvent(Id.Id).ListenEvent(handler, priority, isListenOnce);

        /// <summary>
        /// 监听事件（后处理），指定事件 ID。
        /// </summary>
        public FEventListenHelper ListenEvent<T>(int evtId, FEvent.PostEventHandler<T> handler, short priority = 0, bool isListenOnce = false)
            => World.Entities.GetEvent(Id.Id).ListenEvent(evtId, handler, priority, isListenOnce);

        /// <summary>
        /// 监听空事件（后处理）。
        /// </summary>
        public FEventListenHelper ListenEvent(int evtId, FEvent.PostEventHandler<object> handler, short priority = 0, bool isListenOnce = false)
            => World.Entities.GetEvent(Id.Id).ListenEvent(evtId, handler, priority, isListenOnce);

        /// <summary>
        /// 监听前置事件。
        /// <remarks>回调方法返回值: 是否继续执行事件</remarks>
        /// </summary>
        public FEventListenHelper ListenPreEvent<T>(FEvent.PreEventHandler<T> handler, short priority = 0, bool isListenOnce = false)
            => World.Entities.GetEvent(Id.Id).ListenPreEvent(handler, priority, isListenOnce);

        /// <summary>
        /// 监听前置事件，指定事件 ID。
        /// <remarks>回调方法返回值: 是否继续执行事件</remarks>
        /// </summary>
        public FEventListenHelper ListenPreEvent<T>(int evtId, FEvent.PreEventHandler<T> handler, short priority = 0, bool isListenOnce = false)
            => World.Entities.GetEvent(Id.Id).ListenPreEvent(evtId, handler, priority, isListenOnce);

        /// <summary>
        /// 监听空前置事件。
        /// <remarks>回调方法返回值: 是否继续执行事件</remarks>
        /// </summary>
        public FEventListenHelper ListenPreEvent(int evtId, FEvent.PreEventHandler<object> handler, short priority = 0, bool isListenOnce = false)
            => World.Entities.GetEvent(Id.Id).ListenPreEvent(evtId, handler, priority, isListenOnce);

        /// <summary>
        /// 取消监听事件。
        /// </summary>
        public void UnlistenEvent<T>(FEvent.PostEventHandler<T> handler)
            => World.Entities.GetEvent(Id.Id).UnlistenEvent(handler);

        /// <summary>
        /// 取消监听前置事件。
        /// </summary>
        public void UnlistenEvent<T>(FEvent.PreEventHandler<T> handler)
            => World.Entities.GetEvent(Id.Id).UnlistenEvent(handler);

        /// <summary>
        /// 取消监听指定 ID 的事件。
        /// </summary>
        public void UnlistenEvent<T>(int evtId, FEvent.PostEventHandler<T> handler)
            => World.Entities.GetEvent(Id.Id).UnlistenEvent(evtId, handler);

        /// <summary>
        /// 取消监听指定 ID 的前置事件。
        /// </summary>
        public void UnlistenEvent<T>(int evtId, FEvent.PreEventHandler<T> handler)
            => World.Entities.GetEvent(Id.Id).UnlistenEvent(evtId, handler);

        /// <summary>
        /// 检查是否已监听事件。
        /// </summary>
        public bool IsListenEvent<T>(FEvent.PostEventHandler<T> handler)
            => World.Entities.GetDispatchEvent(Id.Id)?.IsListenEvent(handler) == true;

        /// <summary>
        /// 检查是否已监听前置事件。
        /// </summary>
        public bool IsListenEvent<T>(FEvent.PreEventHandler<T> handler)
            => World.Entities.GetDispatchEvent(Id.Id)?.IsListenEvent(handler) == true;

        /// <summary>
        /// 检查是否已监听指定 ID 的事件。
        /// </summary>
        public bool IsListenEvent<T>(int evtId, FEvent.PostEventHandler<T> handler)
            => World.Entities.GetDispatchEvent(Id.Id)?.IsListenEvent(evtId, handler) == true;

        /// <summary>
        /// 检查是否已监听指定 ID 的前置事件。
        /// </summary>
        public bool IsListenEvent<T>(int evtId, FEvent.PreEventHandler<T> handler)
            => World.Entities.GetDispatchEvent(Id.Id)?.IsListenEvent(evtId, handler) == true;

        /// <summary>
        /// 清空所有事件监听。
        /// </summary>
        public void ClearListenEvents()
            => World.Entities.GetEvent(Id.Id).ClearListenEvents();

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveSelf() => World.RemoveEntity(Id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has() => World.HasEntity(Id);

        /// <summary>
        /// 
        /// </summary>
        public string Dump()
        {
            if (IsEmpty)
                return ToString();
            var strbuf = new StringBuilder(512);
            var allComponents = World.GetAll(this);
            strbuf.Append(Id).Append('[').Append(allComponents.Count).Append(']').AppendLine(": ");
            foreach (var component in allComponents)
                strbuf.Append(TypeAssistant.GetTypeName(component.GetType())).Append(" : ").AppendLine(Json5.Serialize(component));
            return strbuf.ToString();
        }

        bool IJson5Serializable.JsonSerialize(StringBuilder jsonText, object serializeObject, object? customData, int indent, Json5SerializeOptionData opData)
        {
            jsonText.Append(ToString());
            return true;
        }

        public override int GetHashCode() => HashCode.Combine(WorldHandle, Id);
        public static implicit operator WorldEntityId(in WorldEntity helper) => helper.Id;
        public static implicit operator WorldCore(in WorldEntity helper) => helper.World;
        public static bool operator ==(in WorldEntity left, in WorldEntity right) => left.Id == right.Id && left.World == right.World;
        public static bool operator !=(in WorldEntity left, in WorldEntity right) => left.Id != right.Id || left.World != right.World;
        public bool Equals(WorldEntity other) => WorldHandle.Equals(other.WorldHandle) && Id.Equals(other.Id);
        public override bool Equals(object? obj) => obj is WorldEntity other && Equals(other);
    }
}