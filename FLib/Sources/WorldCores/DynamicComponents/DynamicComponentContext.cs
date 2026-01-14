// ==================== qcbf@qq.com | 2026-01-10 ====================

namespace FLib.WorldCores
{
    public readonly struct DynamicComponentContext
    {
        public readonly Entity Entity;
        public readonly int ComponentIndex;

        public DynamicComponentContext(Entity entity, int componentIndex)
        {
            Entity = entity;
            ComponentIndex = componentIndex;
        }
    }
}