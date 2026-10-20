// ==================== qcbf@qq.com | 2026-07-12 ====================


using System.Collections;
using System.Collections.Generic;
using FLib.WorldCores.Queries;
using FLib.WorldCores.Entities;

namespace FLib.WorldCores
{
    public unsafe partial class WorldCore
    {

        /// <summary>
        /// 
        /// </summary>
        public QueryEnumerator<T1> Query<T1>(in WorldQueryFilter filter = null) where T1 : unmanaged => new(this, filter);

        /// <summary>
        /// 
        /// </summary>
        public readonly struct QueryEnumerator<T1> : IEnumerable<(WorldEntityId, Ref<T1>)> where T1 : unmanaged 
        {
            private readonly WorldQueryFilter _filter;
            public Enumerable GetEnumerator() => new(_filter);
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            IEnumerator<(WorldEntityId, Ref<T1>)> IEnumerable<(WorldEntityId, Ref<T1>)>.GetEnumerator() => GetEnumerator();
            public QueryEnumerator(in WorldCore world, in WorldQueryFilter filter)
            {
                _filter = filter;
                if (_filter == null)
                    _filter = new WorldQueryFilterBuilder(world).WithAll<T1>().Build();
            }
            public struct Enumerable : IEnumerator<(WorldEntityId, Ref<T1>)>
            {
                public WorldChunkQueryEnumerator ChunkEnumerator;
                private int _index;
                private WorldEntityId* _entity;
                private T1* _component1;
                private WorldEntityId _currentEntity;
                public (WorldEntityId, Ref<T1>) Current => (_currentEntity, new Ref<T1>(_component1 + _index));
                object IEnumerator.Current => Current;
                public Enumerable(WorldQueryFilter filter)
                {
                    ChunkEnumerator = new WorldChunkQueryEnumerator(filter);
                    _index = 0;
                    _entity = null;
                    _component1 = null;
                    _currentEntity = default;
                }
                public bool MoveNext()
                {
                    var chunk = ChunkEnumerator.Current;
                    if (chunk != null)
                    {
                        var count = chunk.Count;
                        if (count > _index && *(_entity + _index) == _currentEntity)
                            ++_index;

                        if (_index < count)
                        {
                            _currentEntity = *(_entity + _index);
                            return true;
                        }
                    }

                    if (!ChunkEnumerator.MoveNext())
                        return false;

                    chunk = ChunkEnumerator.Current!;
                    _index = 0;
                    _entity = chunk.GetEntity(0);
                    _component1 = (T1*)chunk.Get<T1>(0);
                    _currentEntity = *_entity;
                    return true;
                }
                public void Reset() { }
                public void Dispose() => ChunkEnumerator.Dispose();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public QueryEnumerator<T1, T2> Query<T1, T2>(in WorldQueryFilter filter = null) where T1 : unmanaged where T2 : unmanaged => new(this, filter);

        /// <summary>
        /// 
        /// </summary>
        public readonly struct QueryEnumerator<T1, T2> : IEnumerable<(WorldEntityId, Ref<T1>, Ref<T2>)> where T1 : unmanaged where T2 : unmanaged 
        {
            private readonly WorldQueryFilter _filter;
            public Enumerable GetEnumerator() => new(_filter);
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            IEnumerator<(WorldEntityId, Ref<T1>, Ref<T2>)> IEnumerable<(WorldEntityId, Ref<T1>, Ref<T2>)>.GetEnumerator() => GetEnumerator();
            public QueryEnumerator(in WorldCore world, in WorldQueryFilter filter)
            {
                _filter = filter;
                if (_filter == null)
                    _filter = new WorldQueryFilterBuilder(world).WithAll<T1>().WithAll<T2>().Build();
            }
            public struct Enumerable : IEnumerator<(WorldEntityId, Ref<T1>, Ref<T2>)>
            {
                public WorldChunkQueryEnumerator ChunkEnumerator;
                private int _index;
                private WorldEntityId* _entity;
                private T1* _component1;
				private T2* _component2;
                private WorldEntityId _currentEntity;
                public (WorldEntityId, Ref<T1>, Ref<T2>) Current => (_currentEntity, new Ref<T1>(_component1 + _index), new Ref<T2>(_component2 + _index));
                object IEnumerator.Current => Current;
                public Enumerable(WorldQueryFilter filter)
                {
                    ChunkEnumerator = new WorldChunkQueryEnumerator(filter);
                    _index = 0;
                    _entity = null;
                    _component1 = null;
					_component2 = null;
                    _currentEntity = default;
                }
                public bool MoveNext()
                {
                    var chunk = ChunkEnumerator.Current;
                    if (chunk != null)
                    {
                        var count = chunk.Count;
                        if (count > _index && *(_entity + _index) == _currentEntity)
                            ++_index;

                        if (_index < count)
                        {
                            _currentEntity = *(_entity + _index);
                            return true;
                        }
                    }

                    if (!ChunkEnumerator.MoveNext())
                        return false;

                    chunk = ChunkEnumerator.Current!;
                    _index = 0;
                    _entity = chunk.GetEntity(0);
                    _component1 = (T1*)chunk.Get<T1>(0);
					_component2 = (T2*)chunk.Get<T2>(0);
                    _currentEntity = *_entity;
                    return true;
                }
                public void Reset() { }
                public void Dispose() => ChunkEnumerator.Dispose();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public QueryEnumerator<T1, T2, T3> Query<T1, T2, T3>(in WorldQueryFilter filter = null) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged => new(this, filter);

        /// <summary>
        /// 
        /// </summary>
        public readonly struct QueryEnumerator<T1, T2, T3> : IEnumerable<(WorldEntityId, Ref<T1>, Ref<T2>, Ref<T3>)> where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged 
        {
            private readonly WorldQueryFilter _filter;
            public Enumerable GetEnumerator() => new(_filter);
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            IEnumerator<(WorldEntityId, Ref<T1>, Ref<T2>, Ref<T3>)> IEnumerable<(WorldEntityId, Ref<T1>, Ref<T2>, Ref<T3>)>.GetEnumerator() => GetEnumerator();
            public QueryEnumerator(in WorldCore world, in WorldQueryFilter filter)
            {
                _filter = filter;
                if (_filter == null)
                    _filter = new WorldQueryFilterBuilder(world).WithAll<T1>().WithAll<T2>().WithAll<T3>().Build();
            }
            public struct Enumerable : IEnumerator<(WorldEntityId, Ref<T1>, Ref<T2>, Ref<T3>)>
            {
                public WorldChunkQueryEnumerator ChunkEnumerator;
                private int _index;
                private WorldEntityId* _entity;
                private T1* _component1;
				private T2* _component2;
				private T3* _component3;
                private WorldEntityId _currentEntity;
                public (WorldEntityId, Ref<T1>, Ref<T2>, Ref<T3>) Current => (_currentEntity, new Ref<T1>(_component1 + _index), new Ref<T2>(_component2 + _index), new Ref<T3>(_component3 + _index));
                object IEnumerator.Current => Current;
                public Enumerable(WorldQueryFilter filter)
                {
                    ChunkEnumerator = new WorldChunkQueryEnumerator(filter);
                    _index = 0;
                    _entity = null;
                    _component1 = null;
					_component2 = null;
					_component3 = null;
                    _currentEntity = default;
                }
                public bool MoveNext()
                {
                    var chunk = ChunkEnumerator.Current;
                    if (chunk != null)
                    {
                        var count = chunk.Count;
                        if (count > _index && *(_entity + _index) == _currentEntity)
                            ++_index;

                        if (_index < count)
                        {
                            _currentEntity = *(_entity + _index);
                            return true;
                        }
                    }

                    if (!ChunkEnumerator.MoveNext())
                        return false;

                    chunk = ChunkEnumerator.Current!;
                    _index = 0;
                    _entity = chunk.GetEntity(0);
                    _component1 = (T1*)chunk.Get<T1>(0);
					_component2 = (T2*)chunk.Get<T2>(0);
					_component3 = (T3*)chunk.Get<T3>(0);
                    _currentEntity = *_entity;
                    return true;
                }
                public void Reset() { }
                public void Dispose() => ChunkEnumerator.Dispose();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public QueryEnumerator<T1, T2, T3, T4> Query<T1, T2, T3, T4>(in WorldQueryFilter filter = null) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged => new(this, filter);

        /// <summary>
        /// 
        /// </summary>
        public readonly struct QueryEnumerator<T1, T2, T3, T4> : IEnumerable<(WorldEntityId, Ref<T1>, Ref<T2>, Ref<T3>, Ref<T4>)> where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged 
        {
            private readonly WorldQueryFilter _filter;
            public Enumerable GetEnumerator() => new(_filter);
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            IEnumerator<(WorldEntityId, Ref<T1>, Ref<T2>, Ref<T3>, Ref<T4>)> IEnumerable<(WorldEntityId, Ref<T1>, Ref<T2>, Ref<T3>, Ref<T4>)>.GetEnumerator() => GetEnumerator();
            public QueryEnumerator(in WorldCore world, in WorldQueryFilter filter)
            {
                _filter = filter;
                if (_filter == null)
                    _filter = new WorldQueryFilterBuilder(world).WithAll<T1>().WithAll<T2>().WithAll<T3>().WithAll<T4>().Build();
            }
            public struct Enumerable : IEnumerator<(WorldEntityId, Ref<T1>, Ref<T2>, Ref<T3>, Ref<T4>)>
            {
                public WorldChunkQueryEnumerator ChunkEnumerator;
                private int _index;
                private WorldEntityId* _entity;
                private T1* _component1;
				private T2* _component2;
				private T3* _component3;
				private T4* _component4;
                private WorldEntityId _currentEntity;
                public (WorldEntityId, Ref<T1>, Ref<T2>, Ref<T3>, Ref<T4>) Current => (_currentEntity, new Ref<T1>(_component1 + _index), new Ref<T2>(_component2 + _index), new Ref<T3>(_component3 + _index), new Ref<T4>(_component4 + _index));
                object IEnumerator.Current => Current;
                public Enumerable(WorldQueryFilter filter)
                {
                    ChunkEnumerator = new WorldChunkQueryEnumerator(filter);
                    _index = 0;
                    _entity = null;
                    _component1 = null;
					_component2 = null;
					_component3 = null;
					_component4 = null;
                    _currentEntity = default;
                }
                public bool MoveNext()
                {
                    var chunk = ChunkEnumerator.Current;
                    if (chunk != null)
                    {
                        var count = chunk.Count;
                        if (count > _index && *(_entity + _index) == _currentEntity)
                            ++_index;

                        if (_index < count)
                        {
                            _currentEntity = *(_entity + _index);
                            return true;
                        }
                    }

                    if (!ChunkEnumerator.MoveNext())
                        return false;

                    chunk = ChunkEnumerator.Current!;
                    _index = 0;
                    _entity = chunk.GetEntity(0);
                    _component1 = (T1*)chunk.Get<T1>(0);
					_component2 = (T2*)chunk.Get<T2>(0);
					_component3 = (T3*)chunk.Get<T3>(0);
					_component4 = (T4*)chunk.Get<T4>(0);
                    _currentEntity = *_entity;
                    return true;
                }
                public void Reset() { }
                public void Dispose() => ChunkEnumerator.Dispose();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public QueryEnumerator<T1, T2, T3, T4, T5> Query<T1, T2, T3, T4, T5>(in WorldQueryFilter filter = null) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged => new(this, filter);

        /// <summary>
        /// 
        /// </summary>
        public readonly struct QueryEnumerator<T1, T2, T3, T4, T5> : IEnumerable<(WorldEntityId, Ref<T1>, Ref<T2>, Ref<T3>, Ref<T4>, Ref<T5>)> where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged 
        {
            private readonly WorldQueryFilter _filter;
            public Enumerable GetEnumerator() => new(_filter);
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            IEnumerator<(WorldEntityId, Ref<T1>, Ref<T2>, Ref<T3>, Ref<T4>, Ref<T5>)> IEnumerable<(WorldEntityId, Ref<T1>, Ref<T2>, Ref<T3>, Ref<T4>, Ref<T5>)>.GetEnumerator() => GetEnumerator();
            public QueryEnumerator(in WorldCore world, in WorldQueryFilter filter)
            {
                _filter = filter;
                if (_filter == null)
                    _filter = new WorldQueryFilterBuilder(world).WithAll<T1>().WithAll<T2>().WithAll<T3>().WithAll<T4>().WithAll<T5>().Build();
            }
            public struct Enumerable : IEnumerator<(WorldEntityId, Ref<T1>, Ref<T2>, Ref<T3>, Ref<T4>, Ref<T5>)>
            {
                public WorldChunkQueryEnumerator ChunkEnumerator;
                private int _index;
                private WorldEntityId* _entity;
                private T1* _component1;
				private T2* _component2;
				private T3* _component3;
				private T4* _component4;
				private T5* _component5;
                private WorldEntityId _currentEntity;
                public (WorldEntityId, Ref<T1>, Ref<T2>, Ref<T3>, Ref<T4>, Ref<T5>) Current => (_currentEntity, new Ref<T1>(_component1 + _index), new Ref<T2>(_component2 + _index), new Ref<T3>(_component3 + _index), new Ref<T4>(_component4 + _index), new Ref<T5>(_component5 + _index));
                object IEnumerator.Current => Current;
                public Enumerable(WorldQueryFilter filter)
                {
                    ChunkEnumerator = new WorldChunkQueryEnumerator(filter);
                    _index = 0;
                    _entity = null;
                    _component1 = null;
					_component2 = null;
					_component3 = null;
					_component4 = null;
					_component5 = null;
                    _currentEntity = default;
                }
                public bool MoveNext()
                {
                    var chunk = ChunkEnumerator.Current;
                    if (chunk != null)
                    {
                        var count = chunk.Count;
                        if (count > _index && *(_entity + _index) == _currentEntity)
                            ++_index;

                        if (_index < count)
                        {
                            _currentEntity = *(_entity + _index);
                            return true;
                        }
                    }

                    if (!ChunkEnumerator.MoveNext())
                        return false;

                    chunk = ChunkEnumerator.Current!;
                    _index = 0;
                    _entity = chunk.GetEntity(0);
                    _component1 = (T1*)chunk.Get<T1>(0);
					_component2 = (T2*)chunk.Get<T2>(0);
					_component3 = (T3*)chunk.Get<T3>(0);
					_component4 = (T4*)chunk.Get<T4>(0);
					_component5 = (T5*)chunk.Get<T5>(0);
                    _currentEntity = *_entity;
                    return true;
                }
                public void Reset() { }
                public void Dispose() => ChunkEnumerator.Dispose();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public QueryEnumerator<T1, T2, T3, T4, T5, T6> Query<T1, T2, T3, T4, T5, T6>(in WorldQueryFilter filter = null) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged => new(this, filter);

        /// <summary>
        /// 
        /// </summary>
        public readonly struct QueryEnumerator<T1, T2, T3, T4, T5, T6> : IEnumerable<(WorldEntityId, Ref<T1>, Ref<T2>, Ref<T3>, Ref<T4>, Ref<T5>, Ref<T6>)> where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged 
        {
            private readonly WorldQueryFilter _filter;
            public Enumerable GetEnumerator() => new(_filter);
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            IEnumerator<(WorldEntityId, Ref<T1>, Ref<T2>, Ref<T3>, Ref<T4>, Ref<T5>, Ref<T6>)> IEnumerable<(WorldEntityId, Ref<T1>, Ref<T2>, Ref<T3>, Ref<T4>, Ref<T5>, Ref<T6>)>.GetEnumerator() => GetEnumerator();
            public QueryEnumerator(in WorldCore world, in WorldQueryFilter filter)
            {
                _filter = filter;
                if (_filter == null)
                    _filter = new WorldQueryFilterBuilder(world).WithAll<T1>().WithAll<T2>().WithAll<T3>().WithAll<T4>().WithAll<T5>().WithAll<T6>().Build();
            }
            public struct Enumerable : IEnumerator<(WorldEntityId, Ref<T1>, Ref<T2>, Ref<T3>, Ref<T4>, Ref<T5>, Ref<T6>)>
            {
                public WorldChunkQueryEnumerator ChunkEnumerator;
                private int _index;
                private WorldEntityId* _entity;
                private T1* _component1;
				private T2* _component2;
				private T3* _component3;
				private T4* _component4;
				private T5* _component5;
				private T6* _component6;
                private WorldEntityId _currentEntity;
                public (WorldEntityId, Ref<T1>, Ref<T2>, Ref<T3>, Ref<T4>, Ref<T5>, Ref<T6>) Current => (_currentEntity, new Ref<T1>(_component1 + _index), new Ref<T2>(_component2 + _index), new Ref<T3>(_component3 + _index), new Ref<T4>(_component4 + _index), new Ref<T5>(_component5 + _index), new Ref<T6>(_component6 + _index));
                object IEnumerator.Current => Current;
                public Enumerable(WorldQueryFilter filter)
                {
                    ChunkEnumerator = new WorldChunkQueryEnumerator(filter);
                    _index = 0;
                    _entity = null;
                    _component1 = null;
					_component2 = null;
					_component3 = null;
					_component4 = null;
					_component5 = null;
					_component6 = null;
                    _currentEntity = default;
                }
                public bool MoveNext()
                {
                    var chunk = ChunkEnumerator.Current;
                    if (chunk != null)
                    {
                        var count = chunk.Count;
                        if (count > _index && *(_entity + _index) == _currentEntity)
                            ++_index;

                        if (_index < count)
                        {
                            _currentEntity = *(_entity + _index);
                            return true;
                        }
                    }

                    if (!ChunkEnumerator.MoveNext())
                        return false;

                    chunk = ChunkEnumerator.Current!;
                    _index = 0;
                    _entity = chunk.GetEntity(0);
                    _component1 = (T1*)chunk.Get<T1>(0);
					_component2 = (T2*)chunk.Get<T2>(0);
					_component3 = (T3*)chunk.Get<T3>(0);
					_component4 = (T4*)chunk.Get<T4>(0);
					_component5 = (T5*)chunk.Get<T5>(0);
					_component6 = (T6*)chunk.Get<T6>(0);
                    _currentEntity = *_entity;
                    return true;
                }
                public void Reset() { }
                public void Dispose() => ChunkEnumerator.Dispose();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public QueryEnumerator<T1, T2, T3, T4, T5, T6, T7> Query<T1, T2, T3, T4, T5, T6, T7>(in WorldQueryFilter filter = null) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged => new(this, filter);

        /// <summary>
        /// 
        /// </summary>
        public readonly struct QueryEnumerator<T1, T2, T3, T4, T5, T6, T7> : IEnumerable<(WorldEntityId, Ref<T1>, Ref<T2>, Ref<T3>, Ref<T4>, Ref<T5>, Ref<T6>, Ref<T7>)> where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged 
        {
            private readonly WorldQueryFilter _filter;
            public Enumerable GetEnumerator() => new(_filter);
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            IEnumerator<(WorldEntityId, Ref<T1>, Ref<T2>, Ref<T3>, Ref<T4>, Ref<T5>, Ref<T6>, Ref<T7>)> IEnumerable<(WorldEntityId, Ref<T1>, Ref<T2>, Ref<T3>, Ref<T4>, Ref<T5>, Ref<T6>, Ref<T7>)>.GetEnumerator() => GetEnumerator();
            public QueryEnumerator(in WorldCore world, in WorldQueryFilter filter)
            {
                _filter = filter;
                if (_filter == null)
                    _filter = new WorldQueryFilterBuilder(world).WithAll<T1>().WithAll<T2>().WithAll<T3>().WithAll<T4>().WithAll<T5>().WithAll<T6>().WithAll<T7>().Build();
            }
            public struct Enumerable : IEnumerator<(WorldEntityId, Ref<T1>, Ref<T2>, Ref<T3>, Ref<T4>, Ref<T5>, Ref<T6>, Ref<T7>)>
            {
                public WorldChunkQueryEnumerator ChunkEnumerator;
                private int _index;
                private WorldEntityId* _entity;
                private T1* _component1;
				private T2* _component2;
				private T3* _component3;
				private T4* _component4;
				private T5* _component5;
				private T6* _component6;
				private T7* _component7;
                private WorldEntityId _currentEntity;
                public (WorldEntityId, Ref<T1>, Ref<T2>, Ref<T3>, Ref<T4>, Ref<T5>, Ref<T6>, Ref<T7>) Current => (_currentEntity, new Ref<T1>(_component1 + _index), new Ref<T2>(_component2 + _index), new Ref<T3>(_component3 + _index), new Ref<T4>(_component4 + _index), new Ref<T5>(_component5 + _index), new Ref<T6>(_component6 + _index), new Ref<T7>(_component7 + _index));
                object IEnumerator.Current => Current;
                public Enumerable(WorldQueryFilter filter)
                {
                    ChunkEnumerator = new WorldChunkQueryEnumerator(filter);
                    _index = 0;
                    _entity = null;
                    _component1 = null;
					_component2 = null;
					_component3 = null;
					_component4 = null;
					_component5 = null;
					_component6 = null;
					_component7 = null;
                    _currentEntity = default;
                }
                public bool MoveNext()
                {
                    var chunk = ChunkEnumerator.Current;
                    if (chunk != null)
                    {
                        var count = chunk.Count;
                        if (count > _index && *(_entity + _index) == _currentEntity)
                            ++_index;

                        if (_index < count)
                        {
                            _currentEntity = *(_entity + _index);
                            return true;
                        }
                    }

                    if (!ChunkEnumerator.MoveNext())
                        return false;

                    chunk = ChunkEnumerator.Current!;
                    _index = 0;
                    _entity = chunk.GetEntity(0);
                    _component1 = (T1*)chunk.Get<T1>(0);
					_component2 = (T2*)chunk.Get<T2>(0);
					_component3 = (T3*)chunk.Get<T3>(0);
					_component4 = (T4*)chunk.Get<T4>(0);
					_component5 = (T5*)chunk.Get<T5>(0);
					_component6 = (T6*)chunk.Get<T6>(0);
					_component7 = (T7*)chunk.Get<T7>(0);
                    _currentEntity = *_entity;
                    return true;
                }
                public void Reset() { }
                public void Dispose() => ChunkEnumerator.Dispose();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public QueryEnumerator<T1, T2, T3, T4, T5, T6, T7, T8> Query<T1, T2, T3, T4, T5, T6, T7, T8>(in WorldQueryFilter filter = null) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged => new(this, filter);

        /// <summary>
        /// 
        /// </summary>
        public readonly struct QueryEnumerator<T1, T2, T3, T4, T5, T6, T7, T8> : IEnumerable<(WorldEntityId, Ref<T1>, Ref<T2>, Ref<T3>, Ref<T4>, Ref<T5>, Ref<T6>, Ref<T7>, Ref<T8>)> where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged 
        {
            private readonly WorldQueryFilter _filter;
            public Enumerable GetEnumerator() => new(_filter);
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            IEnumerator<(WorldEntityId, Ref<T1>, Ref<T2>, Ref<T3>, Ref<T4>, Ref<T5>, Ref<T6>, Ref<T7>, Ref<T8>)> IEnumerable<(WorldEntityId, Ref<T1>, Ref<T2>, Ref<T3>, Ref<T4>, Ref<T5>, Ref<T6>, Ref<T7>, Ref<T8>)>.GetEnumerator() => GetEnumerator();
            public QueryEnumerator(in WorldCore world, in WorldQueryFilter filter)
            {
                _filter = filter;
                if (_filter == null)
                    _filter = new WorldQueryFilterBuilder(world).WithAll<T1>().WithAll<T2>().WithAll<T3>().WithAll<T4>().WithAll<T5>().WithAll<T6>().WithAll<T7>().WithAll<T8>().Build();
            }
            public struct Enumerable : IEnumerator<(WorldEntityId, Ref<T1>, Ref<T2>, Ref<T3>, Ref<T4>, Ref<T5>, Ref<T6>, Ref<T7>, Ref<T8>)>
            {
                public WorldChunkQueryEnumerator ChunkEnumerator;
                private int _index;
                private WorldEntityId* _entity;
                private T1* _component1;
				private T2* _component2;
				private T3* _component3;
				private T4* _component4;
				private T5* _component5;
				private T6* _component6;
				private T7* _component7;
				private T8* _component8;
                private WorldEntityId _currentEntity;
                public (WorldEntityId, Ref<T1>, Ref<T2>, Ref<T3>, Ref<T4>, Ref<T5>, Ref<T6>, Ref<T7>, Ref<T8>) Current => (_currentEntity, new Ref<T1>(_component1 + _index), new Ref<T2>(_component2 + _index), new Ref<T3>(_component3 + _index), new Ref<T4>(_component4 + _index), new Ref<T5>(_component5 + _index), new Ref<T6>(_component6 + _index), new Ref<T7>(_component7 + _index), new Ref<T8>(_component8 + _index));
                object IEnumerator.Current => Current;
                public Enumerable(WorldQueryFilter filter)
                {
                    ChunkEnumerator = new WorldChunkQueryEnumerator(filter);
                    _index = 0;
                    _entity = null;
                    _component1 = null;
					_component2 = null;
					_component3 = null;
					_component4 = null;
					_component5 = null;
					_component6 = null;
					_component7 = null;
					_component8 = null;
                    _currentEntity = default;
                }
                public bool MoveNext()
                {
                    var chunk = ChunkEnumerator.Current;
                    if (chunk != null)
                    {
                        var count = chunk.Count;
                        if (count > _index && *(_entity + _index) == _currentEntity)
                            ++_index;

                        if (_index < count)
                        {
                            _currentEntity = *(_entity + _index);
                            return true;
                        }
                    }

                    if (!ChunkEnumerator.MoveNext())
                        return false;

                    chunk = ChunkEnumerator.Current!;
                    _index = 0;
                    _entity = chunk.GetEntity(0);
                    _component1 = (T1*)chunk.Get<T1>(0);
					_component2 = (T2*)chunk.Get<T2>(0);
					_component3 = (T3*)chunk.Get<T3>(0);
					_component4 = (T4*)chunk.Get<T4>(0);
					_component5 = (T5*)chunk.Get<T5>(0);
					_component6 = (T6*)chunk.Get<T6>(0);
					_component7 = (T7*)chunk.Get<T7>(0);
					_component8 = (T8*)chunk.Get<T8>(0);
                    _currentEntity = *_entity;
                    return true;
                }
                public void Reset() { }
                public void Dispose() => ChunkEnumerator.Dispose();
            }
        }
    }
}
