// ==================== qcbf@qq.com | 2026-01-10 ====================

using System;

namespace FLib.WorldCores
{
    public class DynamicComponentGroupManager
    {
        public WorldCore World;
        public IDynamicComponentGroupable[] Groups = Array.Empty<IDynamicComponentGroupable>();

        public DynamicComponentGroupManager(WorldCore world)
        {
            World = world;
        }


        public DynamicComponentGroup<T> GetGroup<T>()
        {
            var id = ComponentRegistry.GetId<T>();
            if (id >= Groups.Length)
                Groups = new IDynamicComponentGroupable[id + 1];
            return (DynamicComponentGroup<T>)(Groups[id] ??= new DynamicComponentGroup<T>() { World = World });
        }

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