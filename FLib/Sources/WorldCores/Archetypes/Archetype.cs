// ==================== qcbf@qq.com |2025-12-28 ====================

using System;
using System.Buffers;

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
        public ulong[] ComponentMask;

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
        public readonly ushort Idx;

        /// <summary>
        /// 
        /// </summary>
        public readonly IncrementId MaxComponentId;

        /// <summary>
        /// 
        /// </summary>
        public Chunk Chunks { get; private set; }


        public Archetype(WorldCore world, in ArchetypeBuilder builder, ushort idx)
        {
            World = world;
            Idx = idx;
            MaxComponentId = builder.MaxComponentId;
            ComponentTypes = builder.ComponentTypes.ToArray();
            ComponentMask = ArrayPool<ulong>.Shared.Rent(BitArrayOperator.GetBitsLength(MaxComponentId.Raw));
            EntitiesPerChunk = (int)(GlobalSetting.ChunkAllocator.ChunkSize / (builder.ComponentsSize + sizeof(Entity)));
            Sparse = new ComponentSparseList(MaxComponentId, false);
            var offset = EntitiesPerChunk * sizeof(Entity);
            for (ushort i = 0; i < ComponentTypes.Length; i++)
            {
                ref readonly var meta = ref ComponentTypes[i];
                Sparse[meta.Id] = offset;
                BitArrayOperator.SetBit(ComponentMask, meta.Id, true);
                offset += meta.Size * EntitiesPerChunk;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Entity CreateEntity(out ushort chunkEntityIdx)
        {
            if (Chunks == null || Chunks.Count >= EntitiesPerChunk)
            {
                var newChunk = GlobalObjectPool<Chunk>.Create();
                newChunk.Previous = Chunks;
                newChunk.Sparse = Sparse;
                Chunks = newChunk;
            }

            chunkEntityIdx = Chunks.Count++;
            var eti = new EntityInfo(World.GenVersion(), Idx, chunkEntityIdx, Chunks);
            var id = checked((ushort)World.EntityInfos.Add(eti));
            return *Chunks.GetEntity(eti.ChunkEntityIdx) = new Entity(id, eti.Version);
        }

        /// <summary>
        /// 
        /// </summary>
        public void RemoveEntity(in EntityInfo eti)
        {
            var etChunk = eti.Chunk;
            var et = *etChunk.GetEntity(eti.ChunkEntityIdx);
            var backEt = *Chunks.GetEntity(Chunks.Count - 1);
            ref var backEti = ref World.EntityInfos.GetRef(backEt.Id);

            for (var i = 0; i < ComponentTypes.Length; i++)
            {
                var meta = ComponentTypes[i];
                ComponentRegistry.GetInfo(meta).ComponentDestroy?.Invoke(ref *(byte*)etChunk.Get(eti.ChunkEntityIdx, meta), World, et);
            }

            for (var i = 0; i < ComponentTypes.Length; i++)
            {
                var meta = ComponentTypes[i];
                Buffer.MemoryCopy(Chunks.Get(backEti.ChunkEntityIdx, meta),
                    etChunk.Get(eti.ChunkEntityIdx, meta),
                    ushort.MaxValue, meta.Size);
            }

            backEti.Chunk = etChunk;
            backEti.ChunkEntityIdx = eti.ChunkEntityIdx;
            *etChunk.GetEntity(eti.ChunkEntityIdx) = backEt;
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

            ArrayPool<ulong>.Shared.Return(ComponentMask);
            ComponentMask = null;
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetMask(IncrementId componentId, bool value)
        {
            if (ComponentMask.Length < BitArrayOperator.GetBitsLength(componentId.Raw))
            {
                var newMask = ArrayPool<ulong>.Shared.Rent(BitArrayOperator.GetBitsLength(componentId.Raw));
                ComponentMask.CopyTo(newMask, 0);
                ArrayPool<ulong>.Shared.Return(ComponentMask);
                ComponentMask = newMask;
            }

            BitArrayOperator.SetBit(ComponentMask, componentId, value);
        }
    }
}