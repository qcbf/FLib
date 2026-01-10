// =================================================={By Qcbf|qcbf@qq.com|2024-11-05}==================================================

namespace FLib.Worlds
{
    public readonly struct WorldEntityLifeEvent
    {
        public readonly WorldEntity Entity;
        public readonly bool IsDestroying;

        public WorldEntityLifeEvent(WorldEntity entity, bool isDestroying)
        {
            Entity = entity;
            IsDestroying = isDestroying;
        }
    }
}
