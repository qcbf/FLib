// ==================== qcbf@qq.com | 2026-01-10 ====================

using System;

namespace FLib.WorldCores
{
    /// <summary>
    /// 动态组件组 管理器
    /// </summary>
    public class SoaComponentGroupManager
    {
        public WorldCore World;
        public ISoaComponentGroupable[] Groups = Array.Empty<ISoaComponentGroupable>();

        public SoaComponentGroupManager(WorldCore world)
        {
            World = world;
        }

        /// <summary>
        /// 获取动态组件组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public SoaComponentGroup<T> GetGroup<T>()
        {
            var id = ComponentRegistry.GetId<T>();
            if (id >= Groups.Length)
                Groups = new ISoaComponentGroupable[id + 1];
            return (SoaComponentGroup<T>)(Groups[id] ??= new SoaComponentGroup<T>() { World = World });
        }

        /// <summary>
        /// 获取动态组件组
        /// </summary>
        /// <param name="componentType"></param>
        /// <returns></returns>
        public ISoaComponentGroupable GetGroup(Type componentType)
        {
            var id = ComponentRegistry.GetId(componentType);
            if (id >= Groups.Length)
                Groups = new ISoaComponentGroupable[id + 1];
            ref var group = ref Groups[id];
            if (group == null)
            {
                group = (ISoaComponentGroupable)TypeAssistant.New(typeof(SoaComponentGroup<>).MakeGenericType(componentType));
                group.World = World;
            }

            return group;
        }
    }
}