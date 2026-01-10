// ==================== qcbf@qq.com | 2025-07-01 ====================

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FLib
{
    /// <summary>
    /// 
    /// </summary>
    public class FEvent
    {
        public IDictionary<int, List<FEventListenData>> AllListens;
        public List<(bool, int, FEventListenData)> Modifies;
        private byte _isDispatching;

        /// <summary>
        /// 
        /// </summary>
        /// <returns>是否继续执行事件</returns>
        /// <typeparam name="T"></typeparam>
        public delegate bool PreEventHandler<T>(object dispatcher, ref T value);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public delegate void PostEventHandler<T>(object dispatcher, in T value);

        /// <summary>
        /// 事件监听处理程序异常
        /// </summary>
        protected virtual void ThrowEventError(Exception ex, in FEventListenData listenData)
        {
            Log.Error?.Write($"dispatch event error: {listenData.Handler}\n{ex}");
        }

        /// <summary>
        ///
        /// </summary>
#if UNITY_2021_1_OR_NEWER
        [UnityEngine.HideInCallstack]
#endif
        public virtual void DispatchEvent<T>(in T evtData, object dispatcher = null) => DispatchEventById(typeof(T).GetHashCode(), evtData, dispatcher);

        /// <summary>
        /// 
        /// </summary>
#if UNITY_2021_1_OR_NEWER
        [UnityEngine.HideInCallstack]
#endif
        public virtual void DispatchEventById(int evtId, object dispatcher = null) => DispatchEventById<object>(evtId, null, dispatcher);

        /// <summary>
        ///
        /// </summary>
#if UNITY_2021_1_OR_NEWER
        [UnityEngine.HideInCallstack]
#endif
        public virtual void DispatchEventById<T>(int evtId, in T evtData, object dispatcher = null)
        {
            if (AllListens == null || !AllListens.TryGetValue(evtId, out var list)) return;
            ++_isDispatching;
            try
            {
                var finalDispatcher = dispatcher ?? this;
                for (var i = list.Count - 1; i >= 0; i--)
                {
                    var listenData = list[i];
                    try
                    {
                        if (listenData.Handler is PostEventHandler<T> func)
                            func(finalDispatcher, evtData);
#if DEBUG
                        else if (listenData.Handler.GetType().GetGenericTypeDefinition() == typeof(PostEventHandler<>))
                        {
                            Log.Error?.Write($"event handler type error {listenData.Handler.Target?.GetType().Name}.{listenData.Handler.Method.Name} {typeof(T)}");
                        }
#endif
                    }
                    catch (Exception ex)
                    {
                        ThrowEventError(ex, listenData);
                    }
                }
            }
            finally
            {
                ProcessDispatchComplete();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns>is continuing run</returns>
#if UNITY_2021_1_OR_NEWER
        [UnityEngine.HideInCallstack]
#endif
        public virtual bool DispatchPreEvent<T>(ref T evtData, object dispatcher = null) => DispatchPreEventById(typeof(T).GetHashCode(), ref evtData, dispatcher);

        /// <summary>
        ///
        /// </summary>
        /// <returns>is continuing run</returns>
#if UNITY_2021_1_OR_NEWER
        [UnityEngine.HideInCallstack]
#endif
        public virtual bool DispatchPreEventById(int evtId, object dispatcher = null)
        {
            object temp = null;
            return DispatchPreEventById(evtId, ref temp, dispatcher);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns>is continuing run</returns>
#if UNITY_2021_1_OR_NEWER
        [UnityEngine.HideInCallstack]
#endif
        public virtual bool DispatchPreEventById<T>(int evtId, ref T evtData, object dispatcher = null)
        {
            if (AllListens == null || !AllListens.TryGetValue(evtId, out var list)) return true;
            ++_isDispatching;
            try
            {
                var finalDispatcher = dispatcher ?? this;
                for (var i = list.Count - 1; i >= 0; i--)
                {
                    if (list[i].Handler is PreEventHandler<T> func)
                    {
                        try
                        {
                            if (!func(finalDispatcher, ref evtData))
                            {
                                return false;
                            }
                        }
                        catch (Exception ex)
                        {
                            ThrowEventError(ex, list[i]);
                        }
                    }
#if DEBUG
                    else if (list[i].Handler.GetType().GetGenericTypeDefinition() == typeof(PreEventHandler<>))
                    {
                        Log.Error?.Write($"event handler type error {list[i].Handler.Target?.GetType().Name}.{list[i].Handler.Method.Name} {typeof(T)}");
                    }
#endif
                }
            }
            finally
            {
                ProcessDispatchComplete();
            }
            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        protected virtual void ProcessDispatchComplete()
        {
            if (--_isDispatching == 0 && Modifies?.Count > 0)
            {
                try
                {
                    foreach (var (isListen, eventType, listenData) in Modifies)
                    {
                        var handler = listenData.Handler;
                        if (isListen)
                            ListenEventImpl(eventType, handler, listenData.Priority, listenData.IsListenOnce);
                        else
                            UnlistenEventImpl(eventType, handler);
                    }
                }
                finally
                {
                    Modifies?.Clear();
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        protected virtual List<FEventListenData> GetListenEventList(Type t) => GetListenEventList(t.GetHashCode());

        /// <summary>
        ///
        /// </summary>
        protected virtual List<FEventListenData> GetListenEventList(int evtId)
        {
            if (!AllListens.TryGetValue(evtId, out var list))
                AllListens.Add(evtId, list = new List<FEventListenData>());
            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        public FEventListenHelper<object> ListenEvent(int evtId, PostEventHandler<object> handler, short priority = 0, bool isListenOnce = false)
        {
            ListenEventImpl(evtId, handler, priority, isListenOnce);
            return new FEventListenHelper<object>(this, evtId, handler);
        }

        /// <summary>
        /// 
        /// </summary>
        public FEventListenHelper<T> ListenEvent<T>(PostEventHandler<T> handler, short priority = 0, bool isListenOnce = false)
        {
            var id = typeof(T).GetHashCode();
            ListenEventImpl(id, handler, priority, isListenOnce);
            return new FEventListenHelper<T>(this, id, handler);
        }

        /// <summary>
        ///
        /// </summary>
        public FEventListenHelper<T> ListenEvent<T>(int evtId, PostEventHandler<T> handler, short priority = 0, bool isListenOnce = false)
        {
            ListenEventImpl(evtId, handler, priority, isListenOnce);
            return new FEventListenHelper<T>(this, evtId, handler);
        }

        /// <summary>
        /// 
        /// </summary>
        public void ListenPreEvent(int evtId, PreEventHandler<object> handler, short priority = 0, bool isListenOnce = false) => ListenEventImpl(evtId, handler, priority, isListenOnce);

        /// <summary>
        ///
        /// </summary>
        public void ListenPreEvent<T>(PreEventHandler<T> handler, short priority = 0, bool isListenOnce = false) => ListenEventImpl(typeof(T).GetHashCode(), handler, priority, isListenOnce);

        /// <summary>
        ///
        /// </summary>
        public void ListenPreEvent<T>(int evtId, PreEventHandler<T> handler, short priority = 0, bool isListenOnce = false) => ListenEventImpl(evtId, handler, priority, isListenOnce);

        /// <summary>
        ///
        /// </summary>
        protected virtual void ListenEventImpl(int evtId, Delegate handler, short priority, bool isListenOnce)
        {
            var listenData = new FEventListenData() { Handler = handler, IsListenOnce = isListenOnce, Priority = priority };
            if (_isDispatching > 0)
            {
                (Modifies ??= new List<(bool, int, FEventListenData)>()).Add((true, evtId, listenData));
                return;
            }

            AllListens ??= new Dictionary<int, List<FEventListenData>>();
            var list = GetListenEventList(evtId);
            var index = list.Count;
            for (; index > 0; index--)
            {
                if (list[index - 1].Priority >= priority)
                    break;
            }
            list.Insert(index, listenData);
        }

        /// <summary>
        ///
        /// </summary>
        public void UnlistenEvent<T>(PostEventHandler<T> handler) => UnlistenEventImpl(typeof(T).GetHashCode(), handler);

        /// <summary>
        ///
        /// </summary>
        public void UnlistenEvent<T>(PreEventHandler<T> handler) => UnlistenEventImpl(typeof(T).GetHashCode(), handler);

        /// <summary>
        /// 
        /// </summary>
        public void UnlistenEvent(int evtId, PostEventHandler<object> handler) => UnlistenEventImpl(evtId, handler);

        /// <summary>
        ///
        /// </summary>
        public void UnlistenEvent<T>(int evtId, PostEventHandler<T> handler) => UnlistenEventImpl(evtId, handler);

        /// <summary>
        ///
        /// </summary>
        public void UnlistenEvent<T>(int evtId, PreEventHandler<T> handler) => UnlistenEventImpl(evtId, handler);

        /// <summary>
        /// 
        /// </summary>
        public virtual void UnlistenEvents()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///
        /// </summary>
        public virtual void UnlistenEventImpl(int evtId, in Delegate handler)
        {
            if (_isDispatching > 0)
            {
                (Modifies ??= new List<(bool, int, FEventListenData)>()).Add((false, evtId, new FEventListenData() { Handler = handler }));
                return;
            }

            if (AllListens != null && AllListens.TryGetValue(evtId, out var list))
            {
                for (var i = list.Count - 1; i >= 0; i--)
                {
                    if (list[i].Handler == handler)
                    {
                        list.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        public bool IsListenEvent<T>(PostEventHandler<T> handler) => IsListenEventImpl(typeof(T).GetHashCode(), handler);

        /// <summary>
        ///
        /// </summary>
        public bool IsListenEvent<T>(PreEventHandler<T> handler) => IsListenEventImpl(typeof(T).GetHashCode(), handler);

        /// <summary>
        ///
        /// </summary>
        public bool IsListenEvent<T>(int evtId, PostEventHandler<T> handler) => IsListenEventImpl(evtId, handler);

        /// <summary>
        ///
        /// </summary>
        public bool IsListenEvent<T>(int evtId, PreEventHandler<T> handler) => IsListenEventImpl(evtId, handler);

        /// <summary>
        ///
        /// </summary>
        protected internal virtual bool IsListenEventImpl(Type evtType, Delegate handler) => IsListenEventImpl(evtType.GetHashCode(), handler);

        /// <summary>
        ///
        /// </summary>
        protected internal virtual bool IsListenEventImpl(int evtId, Delegate handler) => AllListens != null && AllListens.TryGetValue(evtId, out var list) && list.Contains(new FEventListenData() { Handler = handler });

        /// <summary>
        ///
        /// </summary>
        public void ClearListenEvents() => AllListens?.Clear();
    }
}
