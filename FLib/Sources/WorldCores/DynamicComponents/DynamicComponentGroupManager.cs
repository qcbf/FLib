// ==================== qcbf@qq.com | 2026-01-10 ====================

using System;

namespace FLib.WorldCores
{
    public class DynamicComponentGroupManager
    {
        public IDynamicComponentGroupable[] Groups = Array.Empty<IDynamicComponentGroupable>();


        public DynamicComponentGroup<T> GetGroup<T>()
        {
            var id = ComponentRegistry.GetId<T>();
            if (id >= Groups.Length)
                Groups = new IDynamicComponentGroupable[id + 1];
            return (DynamicComponentGroup<T>)(Groups[id] ??= new DynamicComponentGroup<T>());
        }

        public IDynamicComponentGroupable GetGroup(Type componentType)
        {
            var id = ComponentRegistry.GetId(componentType);
            if (id >= Groups.Length)
                Groups = new IDynamicComponentGroupable[id + 1];
            return Groups[id] ??= (IDynamicComponentGroupable)TypeAssistant.New(typeof(DynamicComponentGroup<>).MakeGenericType(componentType));
        }
    }
}