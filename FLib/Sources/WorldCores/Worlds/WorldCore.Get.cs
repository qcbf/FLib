// ==================== qcbf@qq.com | 2026-01-10 ====================

using System;
using System.Diagnostics;

namespace FLib.WorldCores
{
    public unsafe partial class WorldCore
    {
        /// <summary>
        /// 
        /// </summary>
        public Components<T1, T2> GetSta<T1, T2>(in Entity et) where T1 : unmanaged where T2 : unmanaged
        {
            ref readonly var eti = ref EntityInfos.GetRef(et.Id);
            Debug.Assert(eti.Version == et.Version, "version error");
            var chunk = eti.Chunk;
            var idx = eti.ChunkEntityIdx;
            return new Components<T1, T2>(new Ref<T1>(chunk.Get<T1>(idx)), new Ref<T2>(chunk.Get<T2>(idx)));
        }

        /// <summary>
        /// 
        /// </summary>
        public Components<T1, T2, T3> GetSta<T1, T2, T3>(in Entity et) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged
        {
            ref readonly var eti = ref EntityInfos.GetRef(et.Id);
            Debug.Assert(eti.Version == et.Version, "version error");
            var chunk = eti.Chunk;
            var idx = eti.ChunkEntityIdx;
            return new Components<T1, T2, T3>(new Ref<T1>(chunk.Get<T1>(idx)), new Ref<T2>(chunk.Get<T2>(idx)), new Ref<T3>(chunk.Get<T3>(idx)));
        }

        /// <summary>
        /// 
        /// </summary>
        public Components<T1, T2, T3, T4> GetSta<T1, T2, T3, T4>(in Entity et) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged
        {
            ref readonly var eti = ref EntityInfos.GetRef(et.Id);
            Debug.Assert(eti.Version == et.Version, "version error");
            var chunk = eti.Chunk;
            var idx = eti.ChunkEntityIdx;
            return new Components<T1, T2, T3, T4>(new Ref<T1>(chunk.Get<T1>(idx)), new Ref<T2>(chunk.Get<T2>(idx)), new Ref<T3>(chunk.Get<T3>(idx)), new Ref<T4>(chunk.Get<T4>(idx)));
        }

        /// <summary>
        /// 
        /// </summary>
        public Components<T1, T2, T3, T4, T5> GetSta<T1, T2, T3, T4, T5>(in Entity et) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged
        {
            ref readonly var eti = ref EntityInfos.GetRef(et.Id);
            Debug.Assert(eti.Version == et.Version, "version error");
            var chunk = eti.Chunk;
            var idx = eti.ChunkEntityIdx;
            return new Components<T1, T2, T3, T4, T5>(new Ref<T1>(chunk.Get<T1>(idx)), new Ref<T2>(chunk.Get<T2>(idx)), new Ref<T3>(chunk.Get<T3>(idx)), new Ref<T4>(chunk.Get<T4>(idx)), new Ref<T5>(chunk.Get<T5>(idx)));
        }

        /// <summary>
        /// 
        /// </summary>
        public Components<T1, T2, T3, T4, T5, T6> GetSta<T1, T2, T3, T4, T5, T6>(in Entity et) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged
        {
            ref readonly var eti = ref EntityInfos.GetRef(et.Id);
            Debug.Assert(eti.Version == et.Version, "version error");
            var chunk = eti.Chunk;
            var idx = eti.ChunkEntityIdx;
            return new Components<T1, T2, T3, T4, T5, T6>(new Ref<T1>(chunk.Get<T1>(idx)), new Ref<T2>(chunk.Get<T2>(idx)), new Ref<T3>(chunk.Get<T3>(idx)), new Ref<T4>(chunk.Get<T4>(idx)), new Ref<T5>(chunk.Get<T5>(idx)), new Ref<T6>(chunk.Get<T6>(idx)));
        }

        /// <summary>
        /// 
        /// </summary>
        public Components<T1, T2, T3, T4, T5, T6, T7> GetSta<T1, T2, T3, T4, T5, T6, T7>(in Entity et) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged
        {
            ref readonly var eti = ref EntityInfos.GetRef(et.Id);
            Debug.Assert(eti.Version == et.Version, "version error");
            var chunk = eti.Chunk;
            var idx = eti.ChunkEntityIdx;
            return new Components<T1, T2, T3, T4, T5, T6, T7>(new Ref<T1>(chunk.Get<T1>(idx)), new Ref<T2>(chunk.Get<T2>(idx)), new Ref<T3>(chunk.Get<T3>(idx)), new Ref<T4>(chunk.Get<T4>(idx)), new Ref<T5>(chunk.Get<T5>(idx)), new Ref<T6>(chunk.Get<T6>(idx)), new Ref<T7>(chunk.Get<T7>(idx)));
        }

        /// <summary>
        /// 
        /// </summary>
        public Components<T1, T2, T3, T4, T5, T6, T7, T8> GetSta<T1, T2, T3, T4, T5, T6, T7, T8>(in Entity et) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged
        {
            ref readonly var eti = ref EntityInfos.GetRef(et.Id);
            Debug.Assert(eti.Version == et.Version, "version error");
            var chunk = eti.Chunk;
            var idx = eti.ChunkEntityIdx;
            return new Components<T1, T2, T3, T4, T5, T6, T7, T8>(new Ref<T1>(chunk.Get<T1>(idx)), new Ref<T2>(chunk.Get<T2>(idx)), new Ref<T3>(chunk.Get<T3>(idx)), new Ref<T4>(chunk.Get<T4>(idx)), new Ref<T5>(chunk.Get<T5>(idx)), new Ref<T6>(chunk.Get<T6>(idx)), new Ref<T7>(chunk.Get<T7>(idx)), new Ref<T8>(chunk.Get<T8>(idx)));
        }

        /// <summary>
        /// 
        /// </summary>
        public Components<T1, T2, T3, T4, T5, T6, T7, T8, T9> GetSta<T1, T2, T3, T4, T5, T6, T7, T8, T9>(in Entity et) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged
        {
            ref readonly var eti = ref EntityInfos.GetRef(et.Id);
            Debug.Assert(eti.Version == et.Version, "version error");
            var chunk = eti.Chunk;
            var idx = eti.ChunkEntityIdx;
            return new Components<T1, T2, T3, T4, T5, T6, T7, T8, T9>(new Ref<T1>(chunk.Get<T1>(idx)), new Ref<T2>(chunk.Get<T2>(idx)), new Ref<T3>(chunk.Get<T3>(idx)), new Ref<T4>(chunk.Get<T4>(idx)), new Ref<T5>(chunk.Get<T5>(idx)), new Ref<T6>(chunk.Get<T6>(idx)), new Ref<T7>(chunk.Get<T7>(idx)), new Ref<T8>(chunk.Get<T8>(idx)), new Ref<T9>(chunk.Get<T9>(idx)));
        }

        /// <summary>
        /// 
        /// </summary>
        public Components<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> GetSta<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(in Entity et) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged
        {
            ref readonly var eti = ref EntityInfos.GetRef(et.Id);
            Debug.Assert(eti.Version == et.Version, "version error");
            var chunk = eti.Chunk;
            var idx = eti.ChunkEntityIdx;
            return new Components<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(new Ref<T1>(chunk.Get<T1>(idx)), new Ref<T2>(chunk.Get<T2>(idx)), new Ref<T3>(chunk.Get<T3>(idx)), new Ref<T4>(chunk.Get<T4>(idx)), new Ref<T5>(chunk.Get<T5>(idx)), new Ref<T6>(chunk.Get<T6>(idx)), new Ref<T7>(chunk.Get<T7>(idx)), new Ref<T8>(chunk.Get<T8>(idx)), new Ref<T9>(chunk.Get<T9>(idx)), new Ref<T10>(chunk.Get<T10>(idx)));
        }

        /// <summary>
        /// 
        /// </summary>
        public Components<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> GetSta<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(in Entity et) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged where T11 : unmanaged
        {
            ref readonly var eti = ref EntityInfos.GetRef(et.Id);
            Debug.Assert(eti.Version == et.Version, "version error");
            var chunk = eti.Chunk;
            var idx = eti.ChunkEntityIdx;
            return new Components<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(new Ref<T1>(chunk.Get<T1>(idx)), new Ref<T2>(chunk.Get<T2>(idx)), new Ref<T3>(chunk.Get<T3>(idx)), new Ref<T4>(chunk.Get<T4>(idx)), new Ref<T5>(chunk.Get<T5>(idx)), new Ref<T6>(chunk.Get<T6>(idx)), new Ref<T7>(chunk.Get<T7>(idx)), new Ref<T8>(chunk.Get<T8>(idx)), new Ref<T9>(chunk.Get<T9>(idx)), new Ref<T10>(chunk.Get<T10>(idx)), new Ref<T11>(chunk.Get<T11>(idx)));
        }

        /// <summary>
        /// 
        /// </summary>
        public Components<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> GetSta<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(in Entity et) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged where T11 : unmanaged where T12 : unmanaged
        {
            ref readonly var eti = ref EntityInfos.GetRef(et.Id);
            Debug.Assert(eti.Version == et.Version, "version error");
            var chunk = eti.Chunk;
            var idx = eti.ChunkEntityIdx;
            return new Components<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(new Ref<T1>(chunk.Get<T1>(idx)), new Ref<T2>(chunk.Get<T2>(idx)), new Ref<T3>(chunk.Get<T3>(idx)), new Ref<T4>(chunk.Get<T4>(idx)), new Ref<T5>(chunk.Get<T5>(idx)), new Ref<T6>(chunk.Get<T6>(idx)), new Ref<T7>(chunk.Get<T7>(idx)), new Ref<T8>(chunk.Get<T8>(idx)), new Ref<T9>(chunk.Get<T9>(idx)), new Ref<T10>(chunk.Get<T10>(idx)), new Ref<T11>(chunk.Get<T11>(idx)), new Ref<T12>(chunk.Get<T12>(idx)));
        }

        /// <summary>
        /// 
        /// </summary>
        public Components<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> GetSta<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(in Entity et) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged where T11 : unmanaged where T12 : unmanaged where T13 : unmanaged
        {
            ref readonly var eti = ref EntityInfos.GetRef(et.Id);
            Debug.Assert(eti.Version == et.Version, "version error");
            var chunk = eti.Chunk;
            var idx = eti.ChunkEntityIdx;
            return new Components<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(new Ref<T1>(chunk.Get<T1>(idx)), new Ref<T2>(chunk.Get<T2>(idx)), new Ref<T3>(chunk.Get<T3>(idx)), new Ref<T4>(chunk.Get<T4>(idx)), new Ref<T5>(chunk.Get<T5>(idx)), new Ref<T6>(chunk.Get<T6>(idx)), new Ref<T7>(chunk.Get<T7>(idx)), new Ref<T8>(chunk.Get<T8>(idx)), new Ref<T9>(chunk.Get<T9>(idx)), new Ref<T10>(chunk.Get<T10>(idx)), new Ref<T11>(chunk.Get<T11>(idx)), new Ref<T12>(chunk.Get<T12>(idx)), new Ref<T13>(chunk.Get<T13>(idx)));
        }

        /// <summary>
        /// 
        /// </summary>
        public Components<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> GetSta<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(in Entity et) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged where T11 : unmanaged where T12 : unmanaged where T13 : unmanaged where T14 : unmanaged
        {
            ref readonly var eti = ref EntityInfos.GetRef(et.Id);
            Debug.Assert(eti.Version == et.Version, "version error");
            var chunk = eti.Chunk;
            var idx = eti.ChunkEntityIdx;
            return new Components<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(new Ref<T1>(chunk.Get<T1>(idx)), new Ref<T2>(chunk.Get<T2>(idx)), new Ref<T3>(chunk.Get<T3>(idx)), new Ref<T4>(chunk.Get<T4>(idx)), new Ref<T5>(chunk.Get<T5>(idx)), new Ref<T6>(chunk.Get<T6>(idx)), new Ref<T7>(chunk.Get<T7>(idx)), new Ref<T8>(chunk.Get<T8>(idx)), new Ref<T9>(chunk.Get<T9>(idx)), new Ref<T10>(chunk.Get<T10>(idx)), new Ref<T11>(chunk.Get<T11>(idx)), new Ref<T12>(chunk.Get<T12>(idx)), new Ref<T13>(chunk.Get<T13>(idx)), new Ref<T14>(chunk.Get<T14>(idx)));
        }

        /// <summary>
        /// 
        /// </summary>
        public Components<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> GetSta<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(in Entity et) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged where T11 : unmanaged where T12 : unmanaged where T13 : unmanaged where T14 : unmanaged where T15 : unmanaged
        {
            ref readonly var eti = ref EntityInfos.GetRef(et.Id);
            Debug.Assert(eti.Version == et.Version, "version error");
            var chunk = eti.Chunk;
            var idx = eti.ChunkEntityIdx;
            return new Components<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(new Ref<T1>(chunk.Get<T1>(idx)), new Ref<T2>(chunk.Get<T2>(idx)), new Ref<T3>(chunk.Get<T3>(idx)), new Ref<T4>(chunk.Get<T4>(idx)), new Ref<T5>(chunk.Get<T5>(idx)), new Ref<T6>(chunk.Get<T6>(idx)), new Ref<T7>(chunk.Get<T7>(idx)), new Ref<T8>(chunk.Get<T8>(idx)), new Ref<T9>(chunk.Get<T9>(idx)), new Ref<T10>(chunk.Get<T10>(idx)), new Ref<T11>(chunk.Get<T11>(idx)), new Ref<T12>(chunk.Get<T12>(idx)), new Ref<T13>(chunk.Get<T13>(idx)), new Ref<T14>(chunk.Get<T14>(idx)), new Ref<T15>(chunk.Get<T15>(idx)));
        }

        /// <summary>
        /// 
        /// </summary>
        public Components<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> GetSta<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(in Entity et) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged where T11 : unmanaged where T12 : unmanaged where T13 : unmanaged where T14 : unmanaged where T15 : unmanaged where T16 : unmanaged
        {
            ref readonly var eti = ref EntityInfos.GetRef(et.Id);
            Debug.Assert(eti.Version == et.Version, "version error");
            var chunk = eti.Chunk;
            var idx = eti.ChunkEntityIdx;
            return new Components<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(new Ref<T1>(chunk.Get<T1>(idx)), new Ref<T2>(chunk.Get<T2>(idx)), new Ref<T3>(chunk.Get<T3>(idx)), new Ref<T4>(chunk.Get<T4>(idx)), new Ref<T5>(chunk.Get<T5>(idx)), new Ref<T6>(chunk.Get<T6>(idx)), new Ref<T7>(chunk.Get<T7>(idx)), new Ref<T8>(chunk.Get<T8>(idx)), new Ref<T9>(chunk.Get<T9>(idx)), new Ref<T10>(chunk.Get<T10>(idx)), new Ref<T11>(chunk.Get<T11>(idx)), new Ref<T12>(chunk.Get<T12>(idx)), new Ref<T13>(chunk.Get<T13>(idx)), new Ref<T14>(chunk.Get<T14>(idx)), new Ref<T15>(chunk.Get<T15>(idx)), new Ref<T16>(chunk.Get<T16>(idx)));
        }

    }
}