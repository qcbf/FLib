// ==================== qcbf@qq.com | 2025-07-01 ====================

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace FLib
{
    /// <summary>
    /// 主要用于包含事件的字段，如 FEventValue&lt;int&gt; Level; Level.Set(10); Level.Listen()，可以方便的不用为字段手动单独写逻辑代码
    /// </summary>
    [BytesPackGenRelocate("RawValue")]
    public struct FEventValue<T> : IEquatable<FEventValue<T>>
    {
        public T RawValue;
        public FEvent Event;

        public T Value
        {
            get => RawValue;
            set
            {
                var e = new ChangeEvent(RawValue, value);
                if (Event?.DispatchPreEvent(ref e) != false)
                {
                    RawValue = e.NewValue;
                    Event?.DispatchEvent(e);
                }
            }
        }

        public struct ChangeEvent
        {
            public T OldValue;
            public T NewValue;
            public static implicit operator T(ChangeEvent v) => v.NewValue;
            public override string ToString() => $"{OldValue}>{NewValue}";

            public ChangeEvent(T oldValue, T newValue)
            {
                OldValue = oldValue;
                NewValue = newValue;
            }
        }

        public static implicit operator T(FEventValue<T> v) => v.Value;
        public override string ToString() => Value.ToString();

        public FEventValue(T rawValue) : this() => RawValue = rawValue;

        /// <summary>
        /// 
        /// </summary>
        public FEventListenHelper<ChangeEvent> ListenEvent(FEvent.PostEventHandler<ChangeEvent> handler, short level = 0, bool isListenOnce = false)
            => (Event ??= new FEvent()).ListenEvent(handler, level, isListenOnce);

        /// <summary>
        ///
        /// </summary>
        public void ListenPreEvent(FEvent.PreEventHandler<ChangeEvent> handler, short level = 0, bool isListenOnce = false)
            => (Event ??= new FEvent()).ListenPreEvent(handler, level, isListenOnce);

        /// <summary>
        ///
        /// </summary>
        public void UnlistenEvent(FEvent.PostEventHandler<ChangeEvent> handler) => Event?.UnlistenEvent(handler);

        /// <summary>
        /// 
        /// </summary>
        public void UnlistenEvent(FEvent.PreEventHandler<ChangeEvent> handler) => Event?.UnlistenEvent(handler);

        /// <summary>
        /// 
        /// </summary>
        public void ClearEvent() => Event?.ClearListenEvents();

        public bool Equals(FEventValue<T> other) => EqualityComparer<T>.Default.Equals(RawValue, other.RawValue) && Equals(Event, other.Event);
        public override bool Equals(object obj) => obj is FEventValue<T> other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(RawValue, Event);
    }

    [BytesPackGenRelocate("RawValue")]
    public struct FEventArray<T> : IEquatable<FEventArray<T>>
    {
        public T[] RawValue;
        public FEvent Event;

        public T[] Value => RawValue;

        public T this[int index]
        {
            get => RawValue[index];
            set
            {
                var e = new ChangeEvent(RawValue[index], value, index);
                if (Event?.DispatchPreEvent(ref e) != false)
                {
                    RawValue[index] = e.NewValue;
                    Event?.DispatchEvent(e);
                }
            }
        }

        public struct ChangeEvent
        {
            public int Index;
            public T OldValue;
            public T NewValue;
            public static implicit operator T(ChangeEvent v) => v.NewValue;
            public override string ToString() => $"[{Index}]{OldValue}>{NewValue}";

            public ChangeEvent(T oldValue, T newValue, int index)
            {
                OldValue = oldValue;
                NewValue = newValue;
                Index = index;
            }
        }

        public static implicit operator T[](FEventArray<T> v) => v.Value;
        public override string ToString() => Value.ToString();
        public FEventArray(T[] rawValue) : this() => RawValue = rawValue;

        /// <summary>
        /// 
        /// </summary>
        public FEventListenHelper<ChangeEvent> ListenEvent(FEvent.PostEventHandler<ChangeEvent> handler, short level = 0, bool isListenOnce = false)
            => (Event ??= new FEvent()).ListenEvent(handler, level, isListenOnce);

        /// <summary>
        ///
        /// </summary>
        public void ListenPreEvent(FEvent.PreEventHandler<ChangeEvent> handler, short level = 0, bool isListenOnce = false)
            => (Event ??= new FEvent()).ListenPreEvent(handler, level, isListenOnce);

        /// <summary>
        ///
        /// </summary>
        public void UnlistenEvent(FEvent.PostEventHandler<ChangeEvent> handler) => Event?.UnlistenEvent(handler);

        /// <summary>
        /// 
        /// </summary>
        public void UnlistenEvent(FEvent.PreEventHandler<ChangeEvent> handler) => Event?.UnlistenEvent(handler);

        /// <summary>
        /// 
        /// </summary>
        public void ClearEvent() => Event?.ClearListenEvents();

        public bool Equals(FEventArray<T> other) => RawValue == other.RawValue && Equals(Event, other.Event);
        public override bool Equals(object obj) => obj is FEventArray<T> other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(RawValue, Event);
    }

    [BytesPackGenRelocate("RawValue")]
    public struct FEventDict<TKey, TValue> : IEquatable<FEventDict<TKey, TValue>>
    {
        public Dictionary<TKey, TValue> RawValue;
        public FEvent Event;

        public Dictionary<TKey, TValue> Value => RawValue;

        public TValue this[in TKey key]
        {
            get => RawValue[key];
            set
            {
                var e = new ChangeEvent(RawValue[key], value, key);
                if (Event?.DispatchPreEvent(ref e) != false)
                {
                    (RawValue ??= new Dictionary<TKey, TValue>())[key] = e.NewValue;
                    Event?.DispatchEvent(e);
                }
            }
        }

        public struct ChangeEvent
        {
            public TKey Key;
            public TValue OldValue;
            public TValue NewValue;
            public static implicit operator TValue(ChangeEvent v) => v.NewValue;
            public override string ToString() => $"[{Key}]{OldValue}>{NewValue}";

            public ChangeEvent(TValue oldValue, TValue newValue, TKey key)
            {
                OldValue = oldValue;
                NewValue = newValue;
                Key = key;
            }
        }

        public static implicit operator Dictionary<TKey, TValue>(FEventDict<TKey, TValue> v) => v.Value;
        public override string ToString() => Value == null ? "null" : Value.ToString();
        public FEventDict(Dictionary<TKey, TValue> rawValue) : this() => RawValue = rawValue;

        /// <summary>
        /// 
        /// </summary>
        public void Add(in TKey key, in TValue value)
        {
            var e = new ChangeEvent(default, value, key);
            if (Event?.DispatchPreEvent(ref e) != false)
            {
                (RawValue ??= new Dictionary<TKey, TValue>()).Add(key, e.NewValue);
                Event?.DispatchEvent(e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Remove(in TKey key, out TValue value)
        {
            if (!(RawValue ??= new Dictionary<TKey, TValue>()).TryGetValue(key, out value))
                return false;
            var e = new ChangeEvent(value, default, key);
            if (Event?.DispatchPreEvent(ref e) != false)
            {
                RawValue.Remove(key);
                Event?.DispatchEvent(e);
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public FEventListenHelper<ChangeEvent> ListenEvent(FEvent.PostEventHandler<ChangeEvent> handler, short level = 0, bool isListenOnce = false)
            => (Event ??= new FEvent()).ListenEvent(handler, level, isListenOnce);

        /// <summary>
        ///
        /// </summary>
        public void ListenPreEvent(FEvent.PreEventHandler<ChangeEvent> handler, short level = 0, bool isListenOnce = false)
            => (Event ??= new FEvent()).ListenPreEvent(handler, level, isListenOnce);

        /// <summary>
        ///
        /// </summary>
        public void UnlistenEvent(FEvent.PostEventHandler<ChangeEvent> handler) => Event?.UnlistenEvent(handler);

        /// <summary>
        /// 
        /// </summary>
        public void UnlistenEvent(FEvent.PreEventHandler<ChangeEvent> handler) => Event?.UnlistenEvent(handler);

        /// <summary>
        /// 
        /// </summary>
        public void ClearEvent() => Event?.ClearListenEvents();

        public bool Equals(FEventDict<TKey, TValue> other) => RawValue == other.RawValue && Equals(Event, other.Event);
        public override bool Equals(object obj) => obj is FEventDict<TKey, TValue> other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(RawValue, Event);
    }
}
