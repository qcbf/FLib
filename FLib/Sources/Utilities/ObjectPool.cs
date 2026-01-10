// =================================================={By Qcbf|qcbf@qq.com|2024-10-19}==================================================

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FLib
{
    /// <summary>
    ///
    /// </summary>
    public interface IObjectPoolActivatable
    {
        void ObjectPoolActivate();
    }

    /// <summary>
    ///
    /// </summary>
    public interface IObjectPoolParamActivatable
    {
        void ObjectPoolActivate(ObjectPool pool);
    }

    /// <summary>
    ///
    /// </summary>
    public interface IObjectPoolDeactivatable
    {
        void ObjectPoolDeactivatable();
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class ObjectPool
    {
        public Type ObjectType;
        public Func<Type, object> NewInstanceHook;
        public int LeastStrongReferenceCount = 4096;
        public readonly Stack<object> Frees = new();


        /// <summary>
        /// 
        /// </summary>
        public ObjectPool(Type objectType)
        {
            ObjectType = objectType;
        }

        /// <summary>
        ///
        /// </summary>
        public object NewInstance()
        {
            return NewInstanceHook?.Invoke(ObjectType) ?? TypeAssistant.New(ObjectType);
        }

        /// <summary>
        ///
        /// </summary>
        public object Create()
        {
            object inst = null;
             while (Frees != null && Frees.TryPop(out var tempInst))
            {
                if (tempInst is WeakReference weakRef)
                {
                    if (!weakRef.IsAlive) continue;
                    inst = weakRef.Target;
                }
                else
                {
                    inst = tempInst;
                }
                break;
            }
            inst ??= NewInstance();
            (inst as IObjectPoolActivatable)?.ObjectPoolActivate();
            (inst as IObjectPoolParamActivatable)?.ObjectPoolActivate(this);
            return inst;
        }

        /// <summary>
        /// 
        /// </summary>
        public object Release(object obj)
        {
            if (obj is IObjectPoolDeactivatable deactivatable)
                deactivatable.ObjectPoolDeactivatable();
            if (Frees.Count > LeastStrongReferenceCount)
                obj = new WeakReference(obj);
            Frees.Push(obj);
            return obj;
        }

        /// <summary>
        ///
        /// </summary>
        public void PreAllocate(int count, bool isDeactivate = true)
        {
            count -= Frees.Count;
#if NET6_0_OR_GREATER
            Frees.EnsureCapacity(count);
#endif
            for (var i = 0; i < count; i++)
            {
                var inst = NewInstance();
                if (isDeactivate && inst is IObjectPoolDeactivatable deactivatable)
                    deactivatable.ObjectPoolDeactivatable();
                Frees.Push(inst);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class MultiObjectPool
    {
        [ThreadStatic]
        private static MultiObjectPool _global;

        public static MultiObjectPool Global => _global ??= new MultiObjectPool();

        public Dictionary<Type, ObjectPool> Pools = new();
        public Dictionary<Type, Func<Type, object>> NewInstanceHooks;
        public int LeastStrongReferenceCount = 4096;

        /// <summary>
        ///
        /// </summary>
        public object NewInstance(Type t)
        {
            return NewInstanceHooks != null && NewInstanceHooks.TryGetValue(t, out var hook) ? hook(t) : TypeAssistant.New(t);
        }

        /// <summary>
        ///
        /// </summary>
        public T Create<T>() where T : class, new() => (T)Create(typeof(T));

        /// <summary>
        ///
        /// </summary>
        public object Create(Type t)
        {
            if (!Pools.TryGetValue(t, out var pool))
                Pools.Add(t, pool = new ObjectPool(t) { NewInstanceHook = NewInstance, LeastStrongReferenceCount = LeastStrongReferenceCount });
            return pool.Create();
        }

        /// <summary>
        ///
        /// </summary>
        public void Release<T>(ref T obj) where T : class
        {
            Release(obj);
            obj = null;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Release(object obj)
        {
            Pools.GetValueOrDefault(obj.GetType())?.Release(obj);
        }

        /// <summary>
        ///
        /// </summary>
        public void FreeAll()
        {
            Pools.Clear();
        }

        /// <summary>
        ///
        /// </summary>
        public void PreAllocate(Type t, int count, bool isDeativate = true)
        {
            if (!Pools.TryGetValue(t, out var pool))
                Pools.Add(t, pool = new ObjectPool(t) { NewInstanceHook = NewInstance, LeastStrongReferenceCount = LeastStrongReferenceCount });
            pool.PreAllocate(count, isDeativate);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class GlobalObjectPool<T> where T : new()
    {
        [ThreadStatic] private static ObjectPool _instance;
        public static ObjectPool Instance => _instance ??= new ObjectPool(typeof(T));
        public static T NewInstance() => (T)Instance.NewInstance();
        public static T Create() => (T)Instance.Create();
        public static void Release(in T obj) => Instance.Release(obj);
        public static void PreAllocate(int count, bool isDeactivate = true) => Instance.PreAllocate(count, isDeactivate);
    }

    /// <summary>
    /// 
    /// </summary>
    public struct GlobalObjectPoolAutoVal<T> : IDisposable where T : new()
    {
        private T _val;
        public T Val => _val ??= GlobalObjectPool<T>.Create();
        public static implicit operator T(in GlobalObjectPoolAutoVal<T> v) => v.Val;

        public void Dispose()
        {
            if (_val != null)
                GlobalObjectPool<T>.Release(_val);
        }
    }
}
