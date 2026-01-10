// =================================================={By Qcbf|qcbf@qq.com|2024-10-22}==================================================

namespace FLib.Worlds
{
    public interface IWorldComponentable
    {
        // todoNext: 去掉这个，换其他方式实现，让业务层struct更加纯粹，上下文通过方法参数传递
        public WorldComponentContext SelfContext { get; set; }
    }

    /// <summary>
    /// 每帧调用
    /// </summary>
    public interface IWorldUpdateComponentable : IWorldComponentable
    {
        void ComponentUpdate();
    }

    /// <summary>
    /// 每帧结尾调用
    /// </summary>
    public interface IWorldLateUpdateComponentable : IWorldComponentable
    {
        void ComponentLateUpdate();
    }

    /// <summary>
    /// 添加组件时调用一次
    /// </summary>
    public interface IWorldBeginComponentable : IWorldComponentable
    {
        void ComponentBegin();
    }

    /// <summary>
    /// 添加组件之后调用一次
    /// </summary>
    public interface IWorldLateBeginComponentable : IWorldComponentable
    {
        void ComponentLateBegin();
    }

    /// <summary>
    /// 移除组件之后调用
    /// </summary>
    public interface IWorldEndComponentable : IWorldComponentable
    {
        void ComponentEnd();
    }

    /// <summary>
    /// 移除组件之前调用
    /// </summary>
    public interface IWorldPreEndComponentable : IWorldComponentable
    {
        void ComponentPreEnd();
    }
}
