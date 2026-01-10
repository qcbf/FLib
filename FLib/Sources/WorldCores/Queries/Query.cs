// ==================== qcbf@qq.com | 2026-01-09 ====================

namespace FLib.WorldCores
{
    public readonly ref struct Query
    {
        public readonly ushort EntityIndex;
        public readonly Chunk Chunk;

        public bool IsEmpty => Chunk == null;

        public Query(ushort entityIndex, Chunk chunk)
        {
            EntityIndex = entityIndex;
            Chunk = chunk;
        }
    }
}