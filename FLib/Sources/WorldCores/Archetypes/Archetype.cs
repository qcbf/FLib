// ==================== qcbf@qq.com |2025-12-28 ====================

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FLib.WorldCores
{
    public sealed unsafe class Archetype : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly WorldCore World;

        /// <summary>
        /// 
        /// </summary>
        public readonly ulong[] ComponentMask;

        /// <summary>
        /// 
        /// </summary>
        public readonly ComponentMeta[] ComponentTypes;

        /// <summary>
        /// 
        /// </summary>
        public readonly ComponentSparseList Sparse;

        /// <summary>
        /// 
        /// </summary>
        public readonly int EntitiesPerChunk;

        /// <summary>
        /// 
        /// </summary>
        public readonly ushort Index;

        /// <summary>
        /// 
        /// </summary>
        public readonly IncrementId MaxComponentId;

        /// <summary>
        /// 
        /// </summary>
        public Chunk Chunks { get; private set; }


        public Archetype(WorldCore world, in ArchetypeBuilder builder, ushort index)
        {
            World = world;
            Index = index;
            MaxComponentId = builder.MaxComponentId;
            ComponentTypes = builder.ComponentTypes.ToArray();
            ComponentMask = new ulong[BitArrayOperator.GetBitsLength(MaxComponentId.Raw)];
            EntitiesPerChunk = (int)(GlobalSetting.ChunkAllocator.ChunkSize / (builder.ComponentsSize + sizeof(Entity)));
            Sparse = new ComponentSparseList(MaxComponentId, false);
            var offset = MathEx.AlignUp(EntitiesPerChunk * sizeof(Entity), GlobalSetting.ComponentAlign);
            for (ushort i = 0; i < ComponentTypes.Length; i++)
            {
                ref readonly var meta = ref ComponentTypes[i];
                Sparse[meta.Id] = offset;
                BitArrayOperator.SetBit(ComponentMask, meta.Id, true);
                offset += MathEx.AlignUp(meta.Size * EntitiesPerChunk, GlobalSetting.ComponentAlign);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Entity CreateEntity(out ushort chunkEntityIndex)
        {
            if (Chunks == null || Chunks.Count >= EntitiesPerChunk)
            {
                var newChunk = GlobalObjectPool<Chunk>.Create();
                newChunk.Previous = Chunks;
                newChunk.Sparse = Sparse;
                Chunks = newChunk;
            }

            chunkEntityIndex = Chunks.Count++;
            var eti = new EntityInfo(World.GenVersion(), Index, chunkEntityIndex, Chunks);
            var id = checked((ushort)World.EntityInfos.Add(eti));
            return *Chunks.GetEntity(eti.ChunkEntityIndex) = new Entity(id, eti.Version);
        }

        /// <summary>
        /// 
        /// </summary>
        public void RemoveEntity(in EntityInfo eti)
        {
            var chunk = eti.Chunk;
            // if (--chunk.Count <= 0)
            // {
            //     SharedChunks.Remove(chunk.SharedComponentHash);
            //     GlobalObjectPool<Chunk>.Release(chunk);
            //     return;
            // }
            //
            // if (eti.ChunkEntityIndex == chunk.Count)
            //     return;
            //
            var et = *chunk.GetEntity(eti.ChunkEntityIndex);
            // for (var i = 0; i < ComponentTypes.Length; i++)
            // {
            //     var meta = ComponentTypes[i];
            //     ComponentRegistry.GetInfo(meta).ComponentDestroy?.Invoke(ref *(byte*)chunk.Get(eti.ChunkEntityIndex, meta), World, et);
            // }
            //
            // var srcIndex = (ushort)(chunk.Count - 1);
            // var dstIndex = eti.ChunkEntityIndex;
            // for (var i = 0; i < ComponentTypes.Length; i++)
            // {
            //     var meta = ComponentTypes[i];
            //     Unsafe.CopyBlock(chunk.Get(dstIndex, meta), chunk.Get(srcIndex, meta), meta.Size);
            // }
            //
            // *chunk.GetEntity(dstIndex) = *chunk.GetEntity(srcIndex);

            var backEt = *Chunks.GetEntity(Chunks.Count - 1);
            ref var backEti = ref World.EntityInfos.GetRef(backEt.Id);

            for (var i = 0; i < ComponentTypes.Length; i++)
            {
                var meta = ComponentTypes[i];
                ComponentRegistry.GetInfo(meta).ComponentDestroy?.Invoke(ref *(byte*)chunk.Get(eti.ChunkEntityIndex, meta), World, et);
            }

            for (var i = 0; i < ComponentTypes.Length; i++)
            {
                var meta = ComponentTypes[i];
                Buffer.MemoryCopy(Chunks.Get(backEti.ChunkEntityIndex, meta),
                    chunk.Get(eti.ChunkEntityIndex, meta),
                    ushort.MaxValue, meta.Size);
            }

            backEti.Chunk = chunk;
            backEti.ChunkEntityIndex = eti.ChunkEntityIndex;
            *chunk.GetEntity(eti.ChunkEntityIndex) = backEt;
            if (--Chunks.Count <= 0)
            {
                var temp = Chunks;
                Chunks = Chunks.Previous;
                GlobalObjectPool<Chunk>.Release(temp);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            var chunk = Chunks;
            Chunks = null;
            while (chunk != null)
            {
                var temp = chunk;
                chunk = chunk.Previous;
                GlobalObjectPool<Chunk>.Release(temp);
            }
            // foreach (var kv in SharedChunks)
            // {
            //     var chunk = kv.Value;
            //     while (chunk != null)
            //     {
            //         var temp = chunk;
            //         chunk = chunk.Previous;
            //         GlobalObjectPool<Chunk>.Release(temp);
            //     }
            // }
            //
            // SharedChunks.Clear();
        }
    }
}