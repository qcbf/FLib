// ==================== qcbf@qq.com |2026-01-02 ====================

using System;
using System.Runtime.InteropServices;

namespace FLib.WorldCores
{
    [StructLayout(LayoutKind.Auto)]
    public struct ArchetypeBuilder : IDisposable
    {
        public PooledList<ComponentMeta> ComponentTypes;
        public ushort ComponentsSize;
        public IncrementId MaxComponentId;
#if DEBUG
        private PooledHashSet<ushort> _componentIds;
#endif

        public ArchetypeBuilder(int componentCapacity = 8)
        {
            ComponentTypes = new PooledList<ComponentMeta>(componentCapacity);
            ComponentsSize = 0;
            MaxComponentId = default;
#if DEBUG
            _componentIds = default;
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        public void Add<T>() => Add(ComponentRegistry.GetMeta<T>());

        /// <summary>
        /// 
        /// </summary>
        public void Add(Type type) => Add(ComponentRegistry.GetMeta(type));

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            ComponentTypes.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Add(in ComponentMeta meta)
        {
#if DEBUG
            if (!_componentIds.Add(meta.Id))
                throw new InvalidOperationException($"Component {meta.Type} already exists.");
#endif
            ComponentsSize += meta.Size;
            if (meta.Id > MaxComponentId)
                MaxComponentId = meta.Id;
            ComponentTypes.Add(meta);
        }
    }
}