// ==================== qcbf@qq.com | 2026-01-10 ====================

using System;

namespace FLib.WorldCores
{
    /// <summary>
    /// 动态组件组 管理器
    /// </summary>
    public class DynamicComponentGroupManager
    {
        public WorldCore World;
        public IDynamicComponentGroupable[] Groups = Array.Empty<IDynamicComponentGroupable>();

        public DynamicComponentGroupManager(WorldCore world)
        {
            World = world;
        }

        /// <summary>
        /// 获取动态组件组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public DynamicComponentGroup<T> GetGroup<T>()
        {
            var id = ComponentRegistry.GetId<T>();
            if (id >= Groups.Length)
                Groups = new IDynamicComponentGroupable[id + 1];
            return (DynamicComponentGroup<T>)(Groups[id] ??= new DynamicComponentGroup<T>() { World = World });
        }

        /// <summary>
        /// 获取动态组件组
        /// </summary>
        /// <param name="componentType"></param>
        /// <returns></returns>
        public IDynamicComponentGroupable GetGroup(Type componentType)
        {
            var id = ComponentRegistry.GetId(componentType);
            if (id >= Groups.Length)
                Groups = new IDynamicComponentGroupable[id + 1];
            ref var group = ref Groups[id];
            if (group == null)
            {
                group = (IDynamicComponentGroupable)TypeAssistant.New(typeof(DynamicComponentGroup<>).MakeGenericType(componentType));
                group.World = World;
            }

            return group;
        }
    }
}