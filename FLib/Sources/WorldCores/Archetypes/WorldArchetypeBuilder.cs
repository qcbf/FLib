// ==================== qcbf@qq.com |2026-01-02 ====================

using System;
using System.Runtime.InteropServices;
using FLib.WorldCores.Components;
using FLib.WorldCores;

namespace FLib.WorldCores.Archetypes
{
    [StructLayout(LayoutKind.Auto)]
    public struct WorldArchetypeBuilder : IDisposable
    {
        public PooledList<WorldComponentMeta> ComponentTypes;
        public ushort ComponentsSize;
        public WorldIncrementId MaxComponentId;
#if DEBUG
        private PooledHashSet<ushort> _componentIds;
#endif

        public WorldArchetypeBuilder(int componentCapacity = 8)
        {
            ComponentTypes = new PooledList<WorldComponentMeta>(componentCapacity);
            ComponentsSize = 0;
            MaxComponentId = default;
#if DEBUG
            _componentIds = default;
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            ComponentTypes.Dispose();
#if DEBUG
            _componentIds.Dispose();
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        public void With(in WorldComponentMeta meta)
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
