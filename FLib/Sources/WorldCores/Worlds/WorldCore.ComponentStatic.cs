// ==================== qcbf@qq.com | 2026-01-14 ====================

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using FLib.WorldCores.SoaComponents;
using FLib.WorldCores.Queries;
using FLib.WorldCores.Entities;
using FLib.WorldCores.Components;

namespace FLib.WorldCores
{
    public partial class WorldCore
    {
        /// <summary>
        /// 获取实体的静态管理组件。
        /// </summary>
        /// <typeparam name="T">管理组件的类型</typeparam>
        /// <param name="et">目标实体</param>
        /// <returns>返回该实体的静态管理组件实例</returns>
        public Mng<T> GetStaMng<T>(WorldEntityId et)
        {
            ref readonly var eti = ref GetEntityInfo(et);
            return eti.Chunk.GetRef<Mng<T>>(eti.IndexInChunk);
        }

        /// <summary>
        /// 设置实体的静态管理组件值。
        /// </summary>
        /// <typeparam name="T">管理组件的类型</typeparam>
        /// <param name="et">目标实体</param>
        /// <param name="val">要设置的值</param>
        public void SetStaMng<T>(WorldEntityId et, in T val)
        {
            ref readonly var eti = ref GetEntityInfo(et);
            eti.Chunk.GetRef<Mng<T>>(eti.IndexInChunk).Set(val);
        }

        /// <summary>
        /// 获取实体的静态组件值（非管理）。
        /// </summary>
        /// <typeparam name="T">组件的类型，必须是非托管类型</typeparam>
        /// <param name="et">目标实体</param>
        /// <returns>返回该实体的静态组件值的Ref包装</returns>
        /// <remarks>结构变更后重取。</remarks>
        public unsafe Ref<T> GetSta<T>(WorldEntityId et) where T : unmanaged
        {
            ref readonly var eti = ref GetEntityInfo(et);
            return new Ref<T>(eti.Chunk.Get<T>(eti.IndexInChunk));
        }

        /// <summary>
        /// 获取实体的静态组件值（非管理）。如果没有则返回默认值。
        /// </summary>
        /// <typeparam name="T">组件的类型，必须是非托管类型</typeparam>
        /// <param name="et">目标实体</param>
        /// <returns>返回该实体的静态组件值的Ref包装, 如果没有则返回默认值</returns>
        /// <remarks>结构变更后重取。</remarks>
        public Ref<T> GetStaOrEmpty<T>(WorldEntityId et) where T : unmanaged
        {
            return HasSta<T>(et) ? GetSta<T>(et) : default;
        }

        /// <summary>
        /// 获取实体的指定类型的静态组件。
        /// </summary>
        /// <param name="et">目标实体</param>
        /// <param name="componentType">组件的类型</param>
        /// <returns>返回该实体的静态组件实例</returns>
        public object GetSta(WorldEntityId et, Type componentType)
        {
            ref readonly var eti = ref GetEntityInfo(et);
            return eti.Chunk.GetObj(eti.IndexInChunk, WorldComponentRegistry.GetMeta(componentType));
        }

        /// <summary>
        /// 获取实体的静态组件的引用。
        /// </summary>
        /// <typeparam name="T">组件的类型，必须是非托管类型</typeparam>
        /// <param name="et">目标实体</param>
        /// <returns>返回对该实体的静态组件的引用</returns>
        /// <remarks>结构变更后重取。</remarks>
        public ref T GetStaRef<T>(WorldEntityId et) where T : unmanaged
        {
            ref readonly var eti = ref GetEntityInfo(et);
            return ref eti.Chunk.GetRef<T>(eti.IndexInChunk);
        }

        /// <summary>
        /// 设置实体的静态组件值。
        /// </summary>
        /// <typeparam name="T">组件的类型，必须是非托管类型</typeparam>
        /// <param name="et">目标实体</param>
        /// <param name="val">要设置的组件值</param>
        public void SetSta<T>(WorldEntityId et, in T val) where T : unmanaged
        {
            ref readonly var eti = ref GetEntityInfo(et);
            eti.Chunk.GetRef<T>(eti.IndexInChunk) = val;
        }

        /// <summary>
        /// 设置实体的共享组件。
        /// </summary>
        /// <typeparam name="T">共享组件的类型</typeparam>
        /// <param name="et">目标实体</param>
        /// <param name="val">要设置的共享组件值</param>
        public void SetShared<T>(WorldEntityId et, in T val) where T : IWorldSharedComponent
        {
            ref readonly var eti = ref GetEntityInfo(et);
            var compId = WorldComponentRegistry.GetId<T>();
            var oldHash = eti.Chunk.SparseComponentMeta[compId];
            var newHash = val.GetHashCode();
            if (oldHash == newHash) return;

            var sharedGroup = (WorldSharedComponentGroup<T>)DynComponentGroups.Get<T>();
            sharedGroup.Alloc(et, val, newHash);
            eti.GetArchetype(this).SetSharedComponent(eti, new WorldQuerySharedComponent(compId, newHash));
            sharedGroup.Free(et, oldHash, false);
        }

        /// <summary>
        /// 检查实体是否拥有指定类型的静态组件。
        /// </summary>
        /// <typeparam name="T">组件的类型，必须是非托管类型</typeparam>
        /// <param name="et">目标实体</param>
        /// <returns>如果实体拥有该组件返回 true，否则返回 false</returns>
        public bool HasSta<T>(WorldEntityId et) where T : unmanaged
        {
            return HasEntity(et) && BitArrayOperator.GetBit(ArchetypeGroup[GetEntityInfo(et).ArchetypeIndex].ComponentMask, WorldComponentRegistry.GetMeta<T>().Id);
        }

        /// <summary>
        /// 检查实体是否拥有指定类型的静态管理组件。
        /// </summary>
        /// <typeparam name="T">管理组件的类型</typeparam>
        /// <param name="et">目标实体</param>
        /// <returns>如果实体拥有该管理组件返回 true，否则返回 false</returns>
        public bool HasStaMng<T>(WorldEntityId et)
        {
            return HasEntity(et) && BitArrayOperator.GetBit(ArchetypeGroup[GetEntityInfo(et).ArchetypeIndex].ComponentMask, WorldComponentRegistry.GetMeta<Mng<T>>().Id);
        }

        /// <summary>
        /// 检查实体是否拥有指定类型的静态组件。
        /// </summary>
        /// <param name="et">目标实体</param>
        /// <param name="componentType">组件的类型</param>
        /// <returns>如果实体拥有该组件返回 true，否则返回 false</returns>
        public bool HasSta(WorldEntityId et, Type componentType)
        {
            return HasEntity(et) && BitArrayOperator.GetBit(ArchetypeGroup[GetEntityInfo(et).ArchetypeIndex].ComponentMask, WorldComponentRegistry.GetMeta(componentType).Id);
        }
    }
}