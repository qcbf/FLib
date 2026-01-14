// ==================== qcbf@qq.com |2025-12-11 ====================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FLib.WorldCores
{
    public partial class WorldCore : FEvent, IDisposable, IEnumerable<Entity>
    {
        /// <summary>
        /// 
        /// </summary>
        public ArchetypeGroup ArchetypeGroup;

        /// <summary>
        /// 
        /// </summary>
        public readonly DynamicComponentGroupManager DynamicComponent;

        /// <summary>
        /// 
        /// </summary>
        public FixedIndexList<ComponentSparseList> DynamicComponentSparse;

        /// <summary>
        /// 
        /// </summary>
        public FixedIndexList<EntityInfo> EntityInfos;

        /// <summary>
        /// 
        /// </summary>
        internal ushort VersionIncrement;

        /// <summary>
        /// 
        /// </summary>
        internal ushort GenVersion() => unchecked(++VersionIncrement == 0 ? ++VersionIncrement : VersionIncrement);

        public bool IsDisposed => ArchetypeGroup == null;

        /// <summary>
        /// 
        /// </summary>
        public WorldCore(int entityCapacity = 1024)
        {
            ArchetypeGroup = new ArchetypeGroup(this);
            DynamicComponent = new DynamicComponentGroupManager(this);
            EntityInfos = new FixedIndexList<EntityInfo>(entityCapacity);
            DynamicComponentSparse = new(entityCapacity >> 1);
        }

        /// <summary>
        /// 
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// 
        /// </summary>
        public IEnumerator<Entity> GetEnumerator()
        {
            var count = EntityInfos.Count;
            for (ushort i = 0; count > 0; i++)
            {
                if (EntityInfos[i].IsEmpty) continue;
                --count;
                yield return new Entity(i, EntityInfos[i].Version);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed) return;

            foreach (var et in this)
            {
                try
                {
                    RemoveEntity(et);
                }
                catch (Exception e)
                {
                    Log.Error?.Write($"{et}  {e}", nameof(WorldCore), nameof(Dispose));
                }
            }

            for (ushort i = 0; i < ArchetypeGroup.Count; i++)
            {
                try
                {
                    ArchetypeGroup[i].Dispose();
                }
                catch (Exception e)
                {
                    Log.Error?.Write($"{i}  {e}", nameof(WorldCore), nameof(Dispose));
                }
            }

            for (var i = 0; i < DynamicComponentSparse.Count; i++)
                DynamicComponentSparse[i].ResizeOnPool(default);

            ArchetypeGroup = null;
            GC.SuppressFinalize(this);
        }
    }
}