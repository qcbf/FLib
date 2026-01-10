// ==================== qcbf@qq.com |2026-01-02 ====================

using System;
using System.Collections.Generic;

namespace FLib.WorldCores
{
    public class ArchetypeGroup
    {
        public readonly WorldCore World;
        public Archetype[] Archetypes;
        public readonly Dictionary<int, Archetype> ArchetypeMap;

        public ushort Count => (ushort)ArchetypeMap.Count;
        public Archetype this[int key] => ArchetypeMap[key];
        public Archetype this[ushort index] => Archetypes[index];

        public ArchetypeGroup(WorldCore world, int capacity = 16)
        {
            Archetypes = new Archetype[capacity];
            ArchetypeMap = new Dictionary<int, Archetype>(capacity);
            World = world;
        }

        /// <summary>
        /// 
        /// </summary>
        public Archetype Create(int hash, in ArchetypeBuilder builder)
        {
            var index = Count;
            var archetype = new Archetype(World, builder, index);
            if (Archetypes.Length <= index)
                Array.Resize(ref Archetypes, MathEx.GetNextPowerOfTwo(index + 1));
            ArchetypeMap.Add(hash, Archetypes[index] = archetype);
            return archetype;
        }
    }
}