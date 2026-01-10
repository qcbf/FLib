// =================================================={By Qcbf|qcbf@qq.com|2024-10-22}==================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FLib.Worlds
{
    public abstract class WorldComponentGroup : BroadcastWorldEvent, IEnumerable
    {
        public ReadOnlyCollection<ushort> Followers;
        public abstract ushort Count { get; }
        protected WorldComponentGroup(WorldBase world) : base(world) { }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public abstract bool Option(EWorldComponentOption op);
        public abstract void AllocCapacity(int newSize);
        public abstract ushort Add(WorldEntity entity, bool isFinished = true);
        public abstract ushort Add(WorldEntity entity, in IWorldComponentable defaultComp, bool isFinished = true);
        public abstract void AddingFinish(ushort index);
        public abstract bool Remove(in WorldComponentContext context);
        public abstract void RemoveAll();
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public abstract bool Exist(in WorldComponentContext context);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public abstract ref PauseCounter GetPause(ushort index);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public abstract WorldComponentContext GetContext(ushort index);
        public abstract string ToString(ushort index, bool isVerbose);
        public virtual ushort UnpackBytes(BytesReader reader) => throw new NotSupportedException();
        IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();
    }

    /// <summary>
    ///
    /// </summary>`
    public sealed class WorldComponentGroup<T> : WorldComponentGroup, IEnumerable<WorldComponentHandleEx<T>> where T : IWorldComponentable, new()
    {
        /// <summary>
        /// start with 1
        /// </summary>
        public static ushort TypeId;

        public static EWorldComponentOption Options;

        public static T Default = default;
        public Node[] ComponentMetas = Array.Empty<Node>();
        private ushort[] _freeIndexes;
        private ushort _freeCount;

        // ReSharper disable StaticMemberInGenericType
#pragma warning disable CS0649
        internal static WorldComponentProcessor.LogicProcessDelegate BeginProcess;
        internal static WorldComponentProcessor.LogicProcessDelegate LateBeginProcess;
        internal static WorldComponentProcessor.LogicProcessDelegate EndProcess;
        internal static WorldComponentProcessor.LogicProcessDelegate PreEndProcess;

        private ushort _count;
        public override ushort Count => _count;


        public WorldComponentGroup(WorldBase world) : base(world)
        {
        }

        public struct Node
        {
            public PauseCounter Pause;
            public bool IsValid;
            public T Component;
            public readonly WorldEntity Entity => Component.SelfContext.Entity;
            public override string ToString() => $"{Entity.Id}|{WorldComponentManager.GetTypeName(typeof(T))}";
        }

        /// <summary>
        ///
        /// </summary>
        public override void AllocCapacity(int newSize)
        {
            var oldSize = ComponentMetas.Length;
            if (newSize < oldSize)
                return;
            Array.Resize(ref ComponentMetas, newSize);
            newSize -= oldSize;
            if (_freeIndexes == null || _freeIndexes.Length < newSize)
                Array.Resize(ref _freeIndexes, newSize);
            newSize -= _freeCount;
            for (var i = 0; i < newSize; i++)
                _freeIndexes[_freeCount++] = (ushort)(i + oldSize);
        }

        /// <summary>
        ///
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        ///
        /// </summary>
        public IEnumerator<WorldComponentHandleEx<T>> GetEnumerator() => new WorldComponentEnumerator<T>(World, ComponentMetas);

        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public override bool Option(EWorldComponentOption op) => (Options & op) != 0;

        /// <summary>
        ///
        /// </summary>
        public ref T GetRW(ushort index)
        {
            return ref ComponentMetas[index].Component;
        }

        /// <summary>
        ///
        /// </summary>
        public ref readonly T GetRO(ushort index)
        {
            return ref ComponentMetas[index].Component;
        }

        /// <summary>
        ///
        /// </summary>
        public override void AddingFinish(ushort index)
        {
            ++_count;
            var e1 = new WorldAddComponentEvent<T>(new WorldComponentHandleEx<T>(World, new WorldComponentHandle(TypeId, index)));
            var e2 = new WorldAddComponentEvent(new WorldComponentHandleEx(World, new WorldComponentHandle(TypeId, index)));
            var entityEvent = ComponentMetas[index].Entity.EventDispatcher;
            if (entityEvent != null)
            {
                entityEvent.DispatchPreEvent(ref e1);
                entityEvent.DispatchPreEvent(ref e2);
            }
            DispatchPreEvent(ref e2);
            BeginProcess?.Invoke(this, index);
            if (!ComponentMetas[index].IsValid)
                return;

            Log.Verbose?.Write($"{ComponentMetas[index].ToString()}|{Json5.SerializeToLog(ComponentMetas[index].Component)}", "add component");

            if (entityEvent != null)
            {
                entityEvent.DispatchEvent(e1);
                entityEvent.DispatchEvent(e2);
            }
            DispatchEvent(e2);
            if (ComponentMetas[index].IsValid)
                LateBeginProcess?.Invoke(this, index);
        }

        /// <summary>
        /// 
        /// </summary>
        public override ushort Add(WorldEntity entity, bool isFinished = true)
        {
            var metaIndex = AllocComponent();
            if (AnyOption(EWorldComponentOption.Pooling))
                ComponentMetas[metaIndex].Component ??= new T();
            else
                ComponentMetas[metaIndex].Component = new T();
            return Add(entity, metaIndex, isFinished);
        }

        /// <summary>
        ///
        /// </summary>
        public override ushort Add(WorldEntity entity, in IWorldComponentable defaultComp, bool isFinished = true) => Add(entity, (T)defaultComp, isFinished);

        /// <summary>
        /// 
        /// </summary>
        public ushort Add(WorldEntity entity, in T comp, bool isFinished = true)
        {
            var metaIndex = AllocComponent();
            ComponentMetas[metaIndex].Component = comp;
            return Add(entity, metaIndex, isFinished);
        }

        /// <summary>
        ///
        /// </summary>
        public ushort Add(WorldEntity entity, ushort metaIndex, bool isFinished = true)
        {
            ref var meta = ref ComponentMetas[metaIndex];
#if DEBUG
            if (typeof(T).IsClass && !meta.Component.SelfContext.CompHandle.IsEmpty)
                throw new Exception($"component already existed add:{WorldComponentManager.GetTypeName(typeof(T))}, existed:{WorldComponentManager.GetTypeName(meta.Component.SelfContext.CompHandle.TypeId)}");
#endif
            meta.IsValid = true;
            meta.Component.SelfContext = new WorldComponentContext(entity, new WorldComponentHandle(TypeId, metaIndex));
            if (isFinished)
                AddingFinish(metaIndex);
            return metaIndex;
        }

        /// <summary>
        /// 
        /// </summary>
        private ushort AllocComponent()
        {
            if (_freeCount == 0)
                AllocCapacity(Math.Min(ushort.MaxValue, MathEx.GetNextPowerOfTwo(ComponentMetas.Length + 1)));
            return _freeIndexes[--_freeCount];
        }

        /// <summary>
        ///
        /// </summary>
        private void RemoveImpl(ushort index)
        {
            ref var compMeta = ref ComponentMetas[index];
            var isValidEntity = !compMeta.Entity.IsEmpty;
            --_count;
            compMeta.IsValid = false;
            var evt = new WorldRemoveComponentEvent(new WorldComponentHandleEx(World, new WorldComponentHandle(TypeId, index)));
            try
            {
                Log.Verbose?.Write($"{ComponentMetas[index].ToString()}|{Json5.SerializeToLog(ComponentMetas[index].Component)}", "remove component");
                if (isValidEntity)
                {
                    var evtGeneric = new WorldRemoveComponentEvent<T>(new WorldComponentHandleEx<T>(World, new WorldComponentHandle(TypeId, index)));
                    var entityEvent = ComponentMetas[index].Entity.EventDispatcher;
                    if (entityEvent != null)
                    {
                        entityEvent.DispatchPreEvent(ref evtGeneric);
                        entityEvent.DispatchPreEvent(ref evt);
                    }
                    DispatchPreEvent(ref evt);
                    PreEndProcess?.Invoke(this, index);
                    if (entityEvent != null)
                    {
                        entityEvent.DispatchEvent(evtGeneric);
                        entityEvent.DispatchEvent(evt);
                    }
                }
                else
                {
                    DispatchPreEvent(ref evt);
                    if (AnyOption(EWorldComponentOption.CallEndOnEntityDestroyed))
                        PreEndProcess?.Invoke(this, index);
                }
                DispatchEvent(evt);
            }
            finally
            {
                try
                {
                    if (EndProcess != null && (AnyOption(EWorldComponentOption.CallEndOnEntityDestroyed) || isValidEntity))
                        EndProcess.Invoke(this, index);
                }
                finally
                {
                    compMeta.Pause = default;
                    compMeta.Component.SelfContext = default;
                    if (AnyOption(EWorldComponentOption.Pooling))
                    {
                        if (compMeta.Component is IObjectPoolDeactivatable deactivatable)
                        {
                            try
                            {
                                deactivatable.ObjectPoolDeactivatable();
                            }
                            catch (Exception e)
                            {
                                compMeta.Component = default;
                                Log.Error?.Write(e.ToString());
                            }
                        }
                    }
                    else
                    {
                        compMeta.Component = default;
                    }
                    ++_freeCount;
                    if (_freeIndexes.Length < _freeCount)
                        Array.Resize(ref _freeIndexes, Math.Min(ushort.MaxValue, MathEx.GetNextPowerOfTwo(_freeCount)));
                    _freeIndexes[_freeCount - 1] = index;
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        public override bool Remove(in WorldComponentContext context)
        {
            if (!Exist(context)) return false;
            RemoveImpl(context.CompHandle.Index);
            return true;
        }

        /// <summary>
        ///
        /// </summary>
        public override void RemoveAll()
        {
            for (var i = 0; i < ComponentMetas.Length; i++)
            {
                if (ComponentMetas[i].IsValid)
                    RemoveImpl((ushort)i);
            }
        }

        /// <summary>
        ///
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public override bool Exist(in WorldComponentContext context)
        {
            var index = context.CompHandle.Index;
            return ComponentMetas.Length > index && ComponentMetas[index].IsValid && context == ComponentMetas[index].Component.SelfContext;
        }

        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public override WorldComponentContext GetContext(ushort index)
        {
            return ComponentMetas[index].Component.SelfContext;
        }

        /// <summary>
        ///
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public override ref PauseCounter GetPause(ushort index)
        {
            return ref ComponentMetas[index].Pause;
        }

        /// <summary>
        ///
        /// </summary>
        public override string ToString(ushort index, bool isVerbose)
        {
            var comment = typeof(T).GetCustomAttribute<CommentAttribute>(false);
            var strbuf = StringFLibUtility.GetStrBuf();
            if (ComponentMetas[index].Pause)
                strbuf.Append('p').Append(ComponentMetas[index].Pause.CurrentPauseCount).Append('|');
            strbuf.Append(comment == null ? WorldComponentManager.GetTypeName(typeof(T)) : comment.ToString($"({ComponentMetas[index].Component})"));
            if (isVerbose)
                strbuf.Append(Json5.SerializeToLog(ComponentMetas[index].Component));
            return StringFLibUtility.ReleaseStrBufAndResult(strbuf);
        }

        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AnyOption(EWorldComponentOption options) => (Options & options) != 0;
    }
}
