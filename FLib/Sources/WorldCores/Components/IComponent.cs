// ==================== qcbf@qq.com | 2026-01-10 ====================

namespace FLib.WorldCores
{
    public interface IComponentAwake
    {
        public void ComponentBegin(WorldCore world, Entity entity);
    }

    public interface IComponentDestroy
    {
        public void ComponentEnd(WorldCore world, Entity entity);
    }

    public interface IComponentEnable
    {
        public void ComponentEnable(WorldCore world, Entity entity);
    }

    public interface IComponentDisable
    {
        public void ComponentDisable(WorldCore world, Entity entity);
    }

    public interface IComponentUpdate
    {
        public void ComponentDisable(WorldCore world, Entity entity);
    }
}