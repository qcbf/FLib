// ==================== qcbf@qq.com | 2026-01-13 ====================

using System;
using System.Diagnostics;

namespace FLib.WorldCores
{
    public unsafe partial class WorldCore
    {
#pragma warning disable CS0649
        [ThreadStatic] private static QueryFilter _default;


        /// <summary>
        /// 
        /// </summary>
        public void Query(Action<Entity> handler)
        {
            try
            {
                Query(handler, _default);
            }
            finally
            {
                _default.Clear();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Query(Action<Entity> handler, QueryFilter filter)
        {
            using var query = new ChunkQueryEnumerator(this, filter);
            while (query.MoveNext())
            {
                var chunk = query.Current;
                var entity = chunk.GetEntity(0);
                for (var i = 0; i < chunk.Count; i++)
                    handler(*(entity + i));
            }
        }


        public delegate void QueryHandler<T1>(Entity et, ref T1 v1);

        /// <summary>
        /// 
        /// </summary>
        public void Query<T1>(QueryHandler<T1> handler) where T1 : unmanaged
        {
            try
            {
                Query(handler, _default);
            }
            finally
            {
                _default.Clear();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Query<T1>(QueryHandler<T1> handler, QueryFilter filter) where T1 : unmanaged
        {
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T1>());
            using var query = new ChunkQueryEnumerator(this, filter);
            while (query.MoveNext())
            {
                var chunk = query.Current;
                var entity = chunk.GetEntity(0);
                var comp1 = chunk.Get<T1>(0);
                for (var i = 0; i < chunk.Count; i++)
                    handler(*(entity + i), ref *(comp1 + i));
            }
        }


        public delegate void QueryHandler<T1, T2>(Entity et, ref T1 v1, ref T2 v2);

        /// <summary>
        /// 
        /// </summary>
        public void Query<T1, T2>(QueryHandler<T1, T2> handler) where T1 : unmanaged where T2 : unmanaged
        {
            try
            {
                Query(handler, _default);
            }
            finally
            {
                _default.Clear();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Query<T1, T2>(QueryHandler<T1, T2> handler, QueryFilter filter) where T1 : unmanaged where T2 : unmanaged
        {
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T1>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T2>());
            using var query = new ChunkQueryEnumerator(this, filter);
            while (query.MoveNext())
            {
                var chunk = query.Current;
                var entity = chunk.GetEntity(0);
                var comp1 = chunk.Get<T1>(0);
                var comp2 = chunk.Get<T2>(0);
                for (var i = 0; i < chunk.Count; i++)
                    handler(*(entity + i), ref *(comp1 + i), ref *(comp2 + i));
            }
        }


        public delegate void QueryHandler<T1, T2, T3>(Entity et, ref T1 v1, ref T2 v2, ref T3 v3);

        /// <summary>
        /// 
        /// </summary>
        public void Query<T1, T2, T3>(QueryHandler<T1, T2, T3> handler) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged
        {
            try
            {
                Query(handler, _default);
            }
            finally
            {
                _default.Clear();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Query<T1, T2, T3>(QueryHandler<T1, T2, T3> handler, QueryFilter filter) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged
        {
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T1>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T2>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T3>());
            using var query = new ChunkQueryEnumerator(this, filter);
            while (query.MoveNext())
            {
                var chunk = query.Current;
                var entity = chunk.GetEntity(0);
                var comp1 = chunk.Get<T1>(0);
                var comp2 = chunk.Get<T2>(0);
                var comp3 = chunk.Get<T3>(0);
                for (var i = 0; i < chunk.Count; i++)
                    handler(*(entity + i), ref *(comp1 + i), ref *(comp2 + i), ref *(comp3 + i));
            }
        }


        public delegate void QueryHandler<T1, T2, T3, T4>(Entity et, ref T1 v1, ref T2 v2, ref T3 v3, ref T4 v4);

        /// <summary>
        /// 
        /// </summary>
        public void Query<T1, T2, T3, T4>(QueryHandler<T1, T2, T3, T4> handler) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged
        {
            try
            {
                Query(handler, _default);
            }
            finally
            {
                _default.Clear();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Query<T1, T2, T3, T4>(QueryHandler<T1, T2, T3, T4> handler, QueryFilter filter) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged
        {
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T1>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T2>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T3>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T4>());
            using var query = new ChunkQueryEnumerator(this, filter);
            while (query.MoveNext())
            {
                var chunk = query.Current;
                var entity = chunk.GetEntity(0);
                var comp1 = chunk.Get<T1>(0);
                var comp2 = chunk.Get<T2>(0);
                var comp3 = chunk.Get<T3>(0);
                var comp4 = chunk.Get<T4>(0);
                for (var i = 0; i < chunk.Count; i++)
                    handler(*(entity + i), ref *(comp1 + i), ref *(comp2 + i), ref *(comp3 + i), ref *(comp4 + i));
            }
        }


        public delegate void QueryHandler<T1, T2, T3, T4, T5>(Entity et, ref T1 v1, ref T2 v2, ref T3 v3, ref T4 v4, ref T5 v5);

        /// <summary>
        /// 
        /// </summary>
        public void Query<T1, T2, T3, T4, T5>(QueryHandler<T1, T2, T3, T4, T5> handler) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged
        {
            try
            {
                Query(handler, _default);
            }
            finally
            {
                _default.Clear();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Query<T1, T2, T3, T4, T5>(QueryHandler<T1, T2, T3, T4, T5> handler, QueryFilter filter) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged
        {
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T1>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T2>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T3>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T4>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T5>());
            using var query = new ChunkQueryEnumerator(this, filter);
            while (query.MoveNext())
            {
                var chunk = query.Current;
                var entity = chunk.GetEntity(0);
                var comp1 = chunk.Get<T1>(0);
                var comp2 = chunk.Get<T2>(0);
                var comp3 = chunk.Get<T3>(0);
                var comp4 = chunk.Get<T4>(0);
                var comp5 = chunk.Get<T5>(0);
                for (var i = 0; i < chunk.Count; i++)
                    handler(*(entity + i), ref *(comp1 + i), ref *(comp2 + i), ref *(comp3 + i), ref *(comp4 + i), ref *(comp5 + i));
            }
        }


        public delegate void QueryHandler<T1, T2, T3, T4, T5, T6>(Entity et, ref T1 v1, ref T2 v2, ref T3 v3, ref T4 v4, ref T5 v5, ref T6 v6);

        /// <summary>
        /// 
        /// </summary>
        public void Query<T1, T2, T3, T4, T5, T6>(QueryHandler<T1, T2, T3, T4, T5, T6> handler) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged
        {
            try
            {
                Query(handler, _default);
            }
            finally
            {
                _default.Clear();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Query<T1, T2, T3, T4, T5, T6>(QueryHandler<T1, T2, T3, T4, T5, T6> handler, QueryFilter filter) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged
        {
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T1>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T2>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T3>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T4>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T5>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T6>());
            using var query = new ChunkQueryEnumerator(this, filter);
            while (query.MoveNext())
            {
                var chunk = query.Current;
                var entity = chunk.GetEntity(0);
                var comp1 = chunk.Get<T1>(0);
                var comp2 = chunk.Get<T2>(0);
                var comp3 = chunk.Get<T3>(0);
                var comp4 = chunk.Get<T4>(0);
                var comp5 = chunk.Get<T5>(0);
                var comp6 = chunk.Get<T6>(0);
                for (var i = 0; i < chunk.Count; i++)
                    handler(*(entity + i), ref *(comp1 + i), ref *(comp2 + i), ref *(comp3 + i), ref *(comp4 + i), ref *(comp5 + i), ref *(comp6 + i));
            }
        }


        public delegate void QueryHandler<T1, T2, T3, T4, T5, T6, T7>(Entity et, ref T1 v1, ref T2 v2, ref T3 v3, ref T4 v4, ref T5 v5, ref T6 v6, ref T7 v7);

        /// <summary>
        /// 
        /// </summary>
        public void Query<T1, T2, T3, T4, T5, T6, T7>(QueryHandler<T1, T2, T3, T4, T5, T6, T7> handler) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged
        {
            try
            {
                Query(handler, _default);
            }
            finally
            {
                _default.Clear();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Query<T1, T2, T3, T4, T5, T6, T7>(QueryHandler<T1, T2, T3, T4, T5, T6, T7> handler, QueryFilter filter) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged
        {
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T1>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T2>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T3>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T4>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T5>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T6>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T7>());
            using var query = new ChunkQueryEnumerator(this, filter);
            while (query.MoveNext())
            {
                var chunk = query.Current;
                var entity = chunk.GetEntity(0);
                var comp1 = chunk.Get<T1>(0);
                var comp2 = chunk.Get<T2>(0);
                var comp3 = chunk.Get<T3>(0);
                var comp4 = chunk.Get<T4>(0);
                var comp5 = chunk.Get<T5>(0);
                var comp6 = chunk.Get<T6>(0);
                var comp7 = chunk.Get<T7>(0);
                for (var i = 0; i < chunk.Count; i++)
                    handler(*(entity + i), ref *(comp1 + i), ref *(comp2 + i), ref *(comp3 + i), ref *(comp4 + i), ref *(comp5 + i), ref *(comp6 + i), ref *(comp7 + i));
            }
        }


        public delegate void QueryHandler<T1, T2, T3, T4, T5, T6, T7, T8>(Entity et, ref T1 v1, ref T2 v2, ref T3 v3, ref T4 v4, ref T5 v5, ref T6 v6, ref T7 v7, ref T8 v8);

        /// <summary>
        /// 
        /// </summary>
        public void Query<T1, T2, T3, T4, T5, T6, T7, T8>(QueryHandler<T1, T2, T3, T4, T5, T6, T7, T8> handler) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged
        {
            try
            {
                Query(handler, _default);
            }
            finally
            {
                _default.Clear();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Query<T1, T2, T3, T4, T5, T6, T7, T8>(QueryHandler<T1, T2, T3, T4, T5, T6, T7, T8> handler, QueryFilter filter) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged
        {
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T1>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T2>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T3>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T4>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T5>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T6>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T7>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T8>());
            using var query = new ChunkQueryEnumerator(this, filter);
            while (query.MoveNext())
            {
                var chunk = query.Current;
                var entity = chunk.GetEntity(0);
                var comp1 = chunk.Get<T1>(0);
                var comp2 = chunk.Get<T2>(0);
                var comp3 = chunk.Get<T3>(0);
                var comp4 = chunk.Get<T4>(0);
                var comp5 = chunk.Get<T5>(0);
                var comp6 = chunk.Get<T6>(0);
                var comp7 = chunk.Get<T7>(0);
                var comp8 = chunk.Get<T8>(0);
                for (var i = 0; i < chunk.Count; i++)
                    handler(*(entity + i), ref *(comp1 + i), ref *(comp2 + i), ref *(comp3 + i), ref *(comp4 + i), ref *(comp5 + i), ref *(comp6 + i), ref *(comp7 + i), ref *(comp8 + i));
            }
        }


        public delegate void QueryHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Entity et, ref T1 v1, ref T2 v2, ref T3 v3, ref T4 v4, ref T5 v5, ref T6 v6, ref T7 v7, ref T8 v8, ref T9 v9);

        /// <summary>
        /// 
        /// </summary>
        public void Query<T1, T2, T3, T4, T5, T6, T7, T8, T9>(QueryHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9> handler) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged
        {
            try
            {
                Query(handler, _default);
            }
            finally
            {
                _default.Clear();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Query<T1, T2, T3, T4, T5, T6, T7, T8, T9>(QueryHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9> handler, QueryFilter filter) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged
        {
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T1>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T2>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T3>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T4>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T5>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T6>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T7>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T8>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T9>());
            using var query = new ChunkQueryEnumerator(this, filter);
            while (query.MoveNext())
            {
                var chunk = query.Current;
                var entity = chunk.GetEntity(0);
                var comp1 = chunk.Get<T1>(0);
                var comp2 = chunk.Get<T2>(0);
                var comp3 = chunk.Get<T3>(0);
                var comp4 = chunk.Get<T4>(0);
                var comp5 = chunk.Get<T5>(0);
                var comp6 = chunk.Get<T6>(0);
                var comp7 = chunk.Get<T7>(0);
                var comp8 = chunk.Get<T8>(0);
                var comp9 = chunk.Get<T9>(0);
                for (var i = 0; i < chunk.Count; i++)
                    handler(*(entity + i), ref *(comp1 + i), ref *(comp2 + i), ref *(comp3 + i), ref *(comp4 + i), ref *(comp5 + i), ref *(comp6 + i), ref *(comp7 + i), ref *(comp8 + i), ref *(comp9 + i));
            }
        }


        public delegate void QueryHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Entity et, ref T1 v1, ref T2 v2, ref T3 v3, ref T4 v4, ref T5 v5, ref T6 v6, ref T7 v7, ref T8 v8, ref T9 v9, ref T10 v10);

        /// <summary>
        /// 
        /// </summary>
        public void Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(QueryHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> handler) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged
        {
            try
            {
                Query(handler, _default);
            }
            finally
            {
                _default.Clear();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(QueryHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> handler, QueryFilter filter) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged
        {
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T1>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T2>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T3>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T4>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T5>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T6>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T7>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T8>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T9>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T10>());
            using var query = new ChunkQueryEnumerator(this, filter);
            while (query.MoveNext())
            {
                var chunk = query.Current;
                var entity = chunk.GetEntity(0);
                var comp1 = chunk.Get<T1>(0);
                var comp2 = chunk.Get<T2>(0);
                var comp3 = chunk.Get<T3>(0);
                var comp4 = chunk.Get<T4>(0);
                var comp5 = chunk.Get<T5>(0);
                var comp6 = chunk.Get<T6>(0);
                var comp7 = chunk.Get<T7>(0);
                var comp8 = chunk.Get<T8>(0);
                var comp9 = chunk.Get<T9>(0);
                var comp10 = chunk.Get<T10>(0);
                for (var i = 0; i < chunk.Count; i++)
                    handler(*(entity + i), ref *(comp1 + i), ref *(comp2 + i), ref *(comp3 + i), ref *(comp4 + i), ref *(comp5 + i), ref *(comp6 + i), ref *(comp7 + i), ref *(comp8 + i), ref *(comp9 + i), ref *(comp10 + i));
            }
        }


        public delegate void QueryHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Entity et, ref T1 v1, ref T2 v2, ref T3 v3, ref T4 v4, ref T5 v5, ref T6 v6, ref T7 v7, ref T8 v8, ref T9 v9, ref T10 v10, ref T11 v11);

        /// <summary>
        /// 
        /// </summary>
        public void Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(QueryHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> handler) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged where T11 : unmanaged
        {
            try
            {
                Query(handler, _default);
            }
            finally
            {
                _default.Clear();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(QueryHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> handler, QueryFilter filter) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged where T11 : unmanaged
        {
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T1>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T2>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T3>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T4>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T5>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T6>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T7>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T8>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T9>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T10>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T11>());
            using var query = new ChunkQueryEnumerator(this, filter);
            while (query.MoveNext())
            {
                var chunk = query.Current;
                var entity = chunk.GetEntity(0);
                var comp1 = chunk.Get<T1>(0);
                var comp2 = chunk.Get<T2>(0);
                var comp3 = chunk.Get<T3>(0);
                var comp4 = chunk.Get<T4>(0);
                var comp5 = chunk.Get<T5>(0);
                var comp6 = chunk.Get<T6>(0);
                var comp7 = chunk.Get<T7>(0);
                var comp8 = chunk.Get<T8>(0);
                var comp9 = chunk.Get<T9>(0);
                var comp10 = chunk.Get<T10>(0);
                var comp11 = chunk.Get<T11>(0);
                for (var i = 0; i < chunk.Count; i++)
                    handler(*(entity + i), ref *(comp1 + i), ref *(comp2 + i), ref *(comp3 + i), ref *(comp4 + i), ref *(comp5 + i), ref *(comp6 + i), ref *(comp7 + i), ref *(comp8 + i), ref *(comp9 + i), ref *(comp10 + i), ref *(comp11 + i));
            }
        }


        public delegate void QueryHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Entity et, ref T1 v1, ref T2 v2, ref T3 v3, ref T4 v4, ref T5 v5, ref T6 v6, ref T7 v7, ref T8 v8, ref T9 v9, ref T10 v10, ref T11 v11, ref T12 v12);

        /// <summary>
        /// 
        /// </summary>
        public void Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(QueryHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> handler) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged where T11 : unmanaged where T12 : unmanaged
        {
            try
            {
                Query(handler, _default);
            }
            finally
            {
                _default.Clear();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(QueryHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> handler, QueryFilter filter) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged where T11 : unmanaged where T12 : unmanaged
        {
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T1>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T2>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T3>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T4>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T5>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T6>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T7>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T8>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T9>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T10>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T11>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T12>());
            using var query = new ChunkQueryEnumerator(this, filter);
            while (query.MoveNext())
            {
                var chunk = query.Current;
                var entity = chunk.GetEntity(0);
                var comp1 = chunk.Get<T1>(0);
                var comp2 = chunk.Get<T2>(0);
                var comp3 = chunk.Get<T3>(0);
                var comp4 = chunk.Get<T4>(0);
                var comp5 = chunk.Get<T5>(0);
                var comp6 = chunk.Get<T6>(0);
                var comp7 = chunk.Get<T7>(0);
                var comp8 = chunk.Get<T8>(0);
                var comp9 = chunk.Get<T9>(0);
                var comp10 = chunk.Get<T10>(0);
                var comp11 = chunk.Get<T11>(0);
                var comp12 = chunk.Get<T12>(0);
                for (var i = 0; i < chunk.Count; i++)
                    handler(*(entity + i), ref *(comp1 + i), ref *(comp2 + i), ref *(comp3 + i), ref *(comp4 + i), ref *(comp5 + i), ref *(comp6 + i), ref *(comp7 + i), ref *(comp8 + i), ref *(comp9 + i), ref *(comp10 + i), ref *(comp11 + i), ref *(comp12 + i));
            }
        }


        public delegate void QueryHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Entity et, ref T1 v1, ref T2 v2, ref T3 v3, ref T4 v4, ref T5 v5, ref T6 v6, ref T7 v7, ref T8 v8, ref T9 v9, ref T10 v10, ref T11 v11, ref T12 v12, ref T13 v13);

        /// <summary>
        /// 
        /// </summary>
        public void Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(QueryHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> handler) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged where T11 : unmanaged where T12 : unmanaged where T13 : unmanaged
        {
            try
            {
                Query(handler, _default);
            }
            finally
            {
                _default.Clear();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(QueryHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> handler, QueryFilter filter) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged where T11 : unmanaged where T12 : unmanaged where T13 : unmanaged
        {
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T1>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T2>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T3>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T4>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T5>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T6>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T7>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T8>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T9>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T10>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T11>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T12>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T13>());
            using var query = new ChunkQueryEnumerator(this, filter);
            while (query.MoveNext())
            {
                var chunk = query.Current;
                var entity = chunk.GetEntity(0);
                var comp1 = chunk.Get<T1>(0);
                var comp2 = chunk.Get<T2>(0);
                var comp3 = chunk.Get<T3>(0);
                var comp4 = chunk.Get<T4>(0);
                var comp5 = chunk.Get<T5>(0);
                var comp6 = chunk.Get<T6>(0);
                var comp7 = chunk.Get<T7>(0);
                var comp8 = chunk.Get<T8>(0);
                var comp9 = chunk.Get<T9>(0);
                var comp10 = chunk.Get<T10>(0);
                var comp11 = chunk.Get<T11>(0);
                var comp12 = chunk.Get<T12>(0);
                var comp13 = chunk.Get<T13>(0);
                for (var i = 0; i < chunk.Count; i++)
                    handler(*(entity + i), ref *(comp1 + i), ref *(comp2 + i), ref *(comp3 + i), ref *(comp4 + i), ref *(comp5 + i), ref *(comp6 + i), ref *(comp7 + i), ref *(comp8 + i), ref *(comp9 + i), ref *(comp10 + i), ref *(comp11 + i), ref *(comp12 + i), ref *(comp13 + i));
            }
        }


        public delegate void QueryHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Entity et, ref T1 v1, ref T2 v2, ref T3 v3, ref T4 v4, ref T5 v5, ref T6 v6, ref T7 v7, ref T8 v8, ref T9 v9, ref T10 v10, ref T11 v11, ref T12 v12, ref T13 v13, ref T14 v14);

        /// <summary>
        /// 
        /// </summary>
        public void Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(QueryHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> handler) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged where T11 : unmanaged where T12 : unmanaged where T13 : unmanaged where T14 : unmanaged
        {
            try
            {
                Query(handler, _default);
            }
            finally
            {
                _default.Clear();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(QueryHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> handler, QueryFilter filter) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged where T11 : unmanaged where T12 : unmanaged where T13 : unmanaged where T14 : unmanaged
        {
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T1>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T2>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T3>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T4>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T5>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T6>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T7>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T8>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T9>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T10>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T11>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T12>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T13>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T14>());
            using var query = new ChunkQueryEnumerator(this, filter);
            while (query.MoveNext())
            {
                var chunk = query.Current;
                var entity = chunk.GetEntity(0);
                var comp1 = chunk.Get<T1>(0);
                var comp2 = chunk.Get<T2>(0);
                var comp3 = chunk.Get<T3>(0);
                var comp4 = chunk.Get<T4>(0);
                var comp5 = chunk.Get<T5>(0);
                var comp6 = chunk.Get<T6>(0);
                var comp7 = chunk.Get<T7>(0);
                var comp8 = chunk.Get<T8>(0);
                var comp9 = chunk.Get<T9>(0);
                var comp10 = chunk.Get<T10>(0);
                var comp11 = chunk.Get<T11>(0);
                var comp12 = chunk.Get<T12>(0);
                var comp13 = chunk.Get<T13>(0);
                var comp14 = chunk.Get<T14>(0);
                for (var i = 0; i < chunk.Count; i++)
                    handler(*(entity + i), ref *(comp1 + i), ref *(comp2 + i), ref *(comp3 + i), ref *(comp4 + i), ref *(comp5 + i), ref *(comp6 + i), ref *(comp7 + i), ref *(comp8 + i), ref *(comp9 + i), ref *(comp10 + i), ref *(comp11 + i), ref *(comp12 + i), ref *(comp13 + i), ref *(comp14 + i));
            }
        }


        public delegate void QueryHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Entity et, ref T1 v1, ref T2 v2, ref T3 v3, ref T4 v4, ref T5 v5, ref T6 v6, ref T7 v7, ref T8 v8, ref T9 v9, ref T10 v10, ref T11 v11, ref T12 v12, ref T13 v13, ref T14 v14, ref T15 v15);

        /// <summary>
        /// 
        /// </summary>
        public void Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(QueryHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> handler) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged where T11 : unmanaged where T12 : unmanaged where T13 : unmanaged where T14 : unmanaged where T15 : unmanaged
        {
            try
            {
                Query(handler, _default);
            }
            finally
            {
                _default.Clear();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(QueryHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> handler, QueryFilter filter) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged where T11 : unmanaged where T12 : unmanaged where T13 : unmanaged where T14 : unmanaged where T15 : unmanaged
        {
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T1>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T2>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T3>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T4>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T5>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T6>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T7>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T8>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T9>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T10>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T11>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T12>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T13>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T14>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T15>());
            using var query = new ChunkQueryEnumerator(this, filter);
            while (query.MoveNext())
            {
                var chunk = query.Current;
                var entity = chunk.GetEntity(0);
                var comp1 = chunk.Get<T1>(0);
                var comp2 = chunk.Get<T2>(0);
                var comp3 = chunk.Get<T3>(0);
                var comp4 = chunk.Get<T4>(0);
                var comp5 = chunk.Get<T5>(0);
                var comp6 = chunk.Get<T6>(0);
                var comp7 = chunk.Get<T7>(0);
                var comp8 = chunk.Get<T8>(0);
                var comp9 = chunk.Get<T9>(0);
                var comp10 = chunk.Get<T10>(0);
                var comp11 = chunk.Get<T11>(0);
                var comp12 = chunk.Get<T12>(0);
                var comp13 = chunk.Get<T13>(0);
                var comp14 = chunk.Get<T14>(0);
                var comp15 = chunk.Get<T15>(0);
                for (var i = 0; i < chunk.Count; i++)
                    handler(*(entity + i), ref *(comp1 + i), ref *(comp2 + i), ref *(comp3 + i), ref *(comp4 + i), ref *(comp5 + i), ref *(comp6 + i), ref *(comp7 + i), ref *(comp8 + i), ref *(comp9 + i), ref *(comp10 + i), ref *(comp11 + i), ref *(comp12 + i), ref *(comp13 + i), ref *(comp14 + i), ref *(comp15 + i));
            }
        }


        public delegate void QueryHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Entity et, ref T1 v1, ref T2 v2, ref T3 v3, ref T4 v4, ref T5 v5, ref T6 v6, ref T7 v7, ref T8 v8, ref T9 v9, ref T10 v10, ref T11 v11, ref T12 v12, ref T13 v13, ref T14 v14, ref T15 v15, ref T16 v16);

        /// <summary>
        /// 
        /// </summary>
        public void Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(QueryHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> handler) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged where T11 : unmanaged where T12 : unmanaged where T13 : unmanaged where T14 : unmanaged where T15 : unmanaged where T16 : unmanaged
        {
            try
            {
                Query(handler, _default);
            }
            finally
            {
                _default.Clear();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(QueryHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> handler, QueryFilter filter) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged where T11 : unmanaged where T12 : unmanaged where T13 : unmanaged where T14 : unmanaged where T15 : unmanaged where T16 : unmanaged
        {
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T1>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T2>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T3>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T4>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T5>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T6>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T7>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T8>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T9>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T10>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T11>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T12>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T13>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T14>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T15>());
            QueryFilter.Set(ref filter.AllMask, ComponentRegistry.GetId<T16>());
            using var query = new ChunkQueryEnumerator(this, filter);
            while (query.MoveNext())
            {
                var chunk = query.Current;
                var entity = chunk.GetEntity(0);
                var comp1 = chunk.Get<T1>(0);
                var comp2 = chunk.Get<T2>(0);
                var comp3 = chunk.Get<T3>(0);
                var comp4 = chunk.Get<T4>(0);
                var comp5 = chunk.Get<T5>(0);
                var comp6 = chunk.Get<T6>(0);
                var comp7 = chunk.Get<T7>(0);
                var comp8 = chunk.Get<T8>(0);
                var comp9 = chunk.Get<T9>(0);
                var comp10 = chunk.Get<T10>(0);
                var comp11 = chunk.Get<T11>(0);
                var comp12 = chunk.Get<T12>(0);
                var comp13 = chunk.Get<T13>(0);
                var comp14 = chunk.Get<T14>(0);
                var comp15 = chunk.Get<T15>(0);
                var comp16 = chunk.Get<T16>(0);
                for (var i = 0; i < chunk.Count; i++)
                    handler(*(entity + i), ref *(comp1 + i), ref *(comp2 + i), ref *(comp3 + i), ref *(comp4 + i), ref *(comp5 + i), ref *(comp6 + i), ref *(comp7 + i), ref *(comp8 + i), ref *(comp9 + i), ref *(comp10 + i), ref *(comp11 + i), ref *(comp12 + i), ref *(comp13 + i), ref *(comp14 + i), ref *(comp15 + i), ref *(comp16 + i));
            }
        }

    }
}