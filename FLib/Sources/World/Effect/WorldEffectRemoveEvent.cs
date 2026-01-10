// =================================================={By Qcbf|qcbf@qq.com|2024-11-05}==================================================

namespace FLib.Worlds
{
    public struct WorldEffectRemoveEvent
    {
        public WorldEffectContext Context;
        public int RemoveCount;

        public WorldEffectRemoveEvent(WorldEffectContext context, int removeCount)
        {
            Context = context;
            RemoveCount = removeCount;
        }
    }
}
