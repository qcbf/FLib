// ==================== qcbf@qq.com |2025-12-28 ====================

using System;

namespace FLib.WorldCores
{
    public unsafe class Archetype
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
        public readonly ComponentTypeOffsetHelper ComponentOffset;

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
            ComponentMask = new ulong[BitArrayOperator.GetBitsLength(MaxComponentId.Raw)];
            EntitiesPerChunk = (int)(GlobalSetting.ChunkAllocator.ChunkSize / (builder.ComponentsSize + sizeof(Entity)));
            ComponentOffset = new ComponentTypeOffsetHelper(MaxComponentId, false);
            var byteOffset = EntitiesPerChunk * sizeof(Entity);
            for (ushort i = 0; i < ComponentTypes.Length; i++)
            {
                ref readonly var meta = ref ComponentTypes[i];
                ComponentOffset[meta.Id] = byteOffset;
                BitArrayOperator.SetBit(ComponentMask, meta.Id, true);
                byteOffset += meta.Size * EntitiesPerChunk;
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
                newChunk.ComponentOffset = ComponentOffset;
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
            var backEt = *Chunks.GetEntity(Chunks.Count - 1);
            ref var backEti = ref World.EntityInfos.GetRef(backEt.Id);
            for (var i = 0; i < ComponentTypes.Length; i++)
            {
                ref readonly var meta = ref ComponentTypes[i];
                Buffer.MemoryCopy(Chunks.Get(backEti.ChunkEntityIdx, meta.Size, meta.Id),
                    etChunk.Get(eti.ChunkEntityIdx, meta.Size, meta.Id), ushort.MaxValue, meta.Size);
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
        public void ClearChunks()
        {
            var chunk = Chunks;
            Chunks = null;
            while (chunk != null)
            {
                var temp = chunk;
                chunk = chunk.Previous;
                GlobalObjectPool<Chunk>.Release(temp);
            }
        }
    }
}