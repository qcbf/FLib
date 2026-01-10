// ==================== qcbf@qq.com | 2026-01-10 ====================

using System;

namespace FLib.WorldCores
{
    public partial class WorldCore
    {
        /// <summary>
        /// 创建实体
        /// </summary>
        public unsafe Entity CreateEntity<T1>(in T1 v1 = default) where T1 : unmanaged
        {
            Array.Clear(ComponentRegistry.ComponentTypeMaskBuffer);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T1>(), true);
            var hash = ComponentRegistry.GetHash(ComponentRegistry.ComponentTypeMaskBuffer);
            if (!ArchetypeGroup.ArchetypeMap.TryGetValue(hash, out var archetype))
            {
                using var builder = new ArchetypeBuilder(1);
                builder.Add<T1>();
                archetype = ArchetypeGroup.Create(hash, builder);
            }
            var et = archetype.CreateEntity(out var chunkEntityIdx);
            var chunk = archetype.Chunks;
            *chunk.Get<T1>(chunkEntityIdx) = v1;
            return et;
        }
        /// <summary>
        /// 创建实体
        /// </summary>
        public unsafe Entity CreateEntity<T1, T2>(in T1 v1 = default, in T2 v2 = default) where T1 : unmanaged where T2 : unmanaged
        {
            Array.Clear(ComponentRegistry.ComponentTypeMaskBuffer);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T1>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T2>(), true);
            var hash = ComponentRegistry.GetHash(ComponentRegistry.ComponentTypeMaskBuffer);
            if (!ArchetypeGroup.ArchetypeMap.TryGetValue(hash, out var archetype))
            {
                using var builder = new ArchetypeBuilder(2);
                builder.Add<T1>();
                builder.Add<T2>();
                archetype = ArchetypeGroup.Create(hash, builder);
            }
            var et = archetype.CreateEntity(out var chunkEntityIdx);
            var chunk = archetype.Chunks;
            *chunk.Get<T1>(chunkEntityIdx) = v1;
            *chunk.Get<T2>(chunkEntityIdx) = v2;
            return et;
        }
        /// <summary>
        /// 创建实体
        /// </summary>
        public unsafe Entity CreateEntity<T1, T2, T3>(in T1 v1 = default, in T2 v2 = default, in T3 v3 = default) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged
        {
            Array.Clear(ComponentRegistry.ComponentTypeMaskBuffer);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T1>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T2>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T3>(), true);
            var hash = ComponentRegistry.GetHash(ComponentRegistry.ComponentTypeMaskBuffer);
            if (!ArchetypeGroup.ArchetypeMap.TryGetValue(hash, out var archetype))
            {
                using var builder = new ArchetypeBuilder(3);
                builder.Add<T1>();
                builder.Add<T2>();
                builder.Add<T3>();
                archetype = ArchetypeGroup.Create(hash, builder);
            }
            var et = archetype.CreateEntity(out var chunkEntityIdx);
            var chunk = archetype.Chunks;
            *chunk.Get<T1>(chunkEntityIdx) = v1;
            *chunk.Get<T2>(chunkEntityIdx) = v2;
            *chunk.Get<T3>(chunkEntityIdx) = v3;
            return et;
        }
        /// <summary>
        /// 创建实体
        /// </summary>
        public unsafe Entity CreateEntity<T1, T2, T3, T4>(in T1 v1 = default, in T2 v2 = default, in T3 v3 = default, in T4 v4 = default) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged
        {
            Array.Clear(ComponentRegistry.ComponentTypeMaskBuffer);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T1>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T2>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T3>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T4>(), true);
            var hash = ComponentRegistry.GetHash(ComponentRegistry.ComponentTypeMaskBuffer);
            if (!ArchetypeGroup.ArchetypeMap.TryGetValue(hash, out var archetype))
            {
                using var builder = new ArchetypeBuilder(4);
                builder.Add<T1>();
                builder.Add<T2>();
                builder.Add<T3>();
                builder.Add<T4>();
                archetype = ArchetypeGroup.Create(hash, builder);
            }
            var et = archetype.CreateEntity(out var chunkEntityIdx);
            var chunk = archetype.Chunks;
            *chunk.Get<T1>(chunkEntityIdx) = v1;
            *chunk.Get<T2>(chunkEntityIdx) = v2;
            *chunk.Get<T3>(chunkEntityIdx) = v3;
            *chunk.Get<T4>(chunkEntityIdx) = v4;
            return et;
        }
        /// <summary>
        /// 创建实体
        /// </summary>
        public unsafe Entity CreateEntity<T1, T2, T3, T4, T5>(in T1 v1 = default, in T2 v2 = default, in T3 v3 = default, in T4 v4 = default, in T5 v5 = default) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged
        {
            Array.Clear(ComponentRegistry.ComponentTypeMaskBuffer);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T1>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T2>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T3>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T4>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T5>(), true);
            var hash = ComponentRegistry.GetHash(ComponentRegistry.ComponentTypeMaskBuffer);
            if (!ArchetypeGroup.ArchetypeMap.TryGetValue(hash, out var archetype))
            {
                using var builder = new ArchetypeBuilder(5);
                builder.Add<T1>();
                builder.Add<T2>();
                builder.Add<T3>();
                builder.Add<T4>();
                builder.Add<T5>();
                archetype = ArchetypeGroup.Create(hash, builder);
            }
            var et = archetype.CreateEntity(out var chunkEntityIdx);
            var chunk = archetype.Chunks;
            *chunk.Get<T1>(chunkEntityIdx) = v1;
            *chunk.Get<T2>(chunkEntityIdx) = v2;
            *chunk.Get<T3>(chunkEntityIdx) = v3;
            *chunk.Get<T4>(chunkEntityIdx) = v4;
            *chunk.Get<T5>(chunkEntityIdx) = v5;
            return et;
        }
        /// <summary>
        /// 创建实体
        /// </summary>
        public unsafe Entity CreateEntity<T1, T2, T3, T4, T5, T6>(in T1 v1 = default, in T2 v2 = default, in T3 v3 = default, in T4 v4 = default, in T5 v5 = default, in T6 v6 = default) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged
        {
            Array.Clear(ComponentRegistry.ComponentTypeMaskBuffer);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T1>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T2>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T3>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T4>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T5>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T6>(), true);
            var hash = ComponentRegistry.GetHash(ComponentRegistry.ComponentTypeMaskBuffer);
            if (!ArchetypeGroup.ArchetypeMap.TryGetValue(hash, out var archetype))
            {
                using var builder = new ArchetypeBuilder(6);
                builder.Add<T1>();
                builder.Add<T2>();
                builder.Add<T3>();
                builder.Add<T4>();
                builder.Add<T5>();
                builder.Add<T6>();
                archetype = ArchetypeGroup.Create(hash, builder);
            }
            var et = archetype.CreateEntity(out var chunkEntityIdx);
            var chunk = archetype.Chunks;
            *chunk.Get<T1>(chunkEntityIdx) = v1;
            *chunk.Get<T2>(chunkEntityIdx) = v2;
            *chunk.Get<T3>(chunkEntityIdx) = v3;
            *chunk.Get<T4>(chunkEntityIdx) = v4;
            *chunk.Get<T5>(chunkEntityIdx) = v5;
            *chunk.Get<T6>(chunkEntityIdx) = v6;
            return et;
        }
        /// <summary>
        /// 创建实体
        /// </summary>
        public unsafe Entity CreateEntity<T1, T2, T3, T4, T5, T6, T7>(in T1 v1 = default, in T2 v2 = default, in T3 v3 = default, in T4 v4 = default, in T5 v5 = default, in T6 v6 = default, in T7 v7 = default) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged
        {
            Array.Clear(ComponentRegistry.ComponentTypeMaskBuffer);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T1>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T2>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T3>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T4>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T5>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T6>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T7>(), true);
            var hash = ComponentRegistry.GetHash(ComponentRegistry.ComponentTypeMaskBuffer);
            if (!ArchetypeGroup.ArchetypeMap.TryGetValue(hash, out var archetype))
            {
                using var builder = new ArchetypeBuilder(7);
                builder.Add<T1>();
                builder.Add<T2>();
                builder.Add<T3>();
                builder.Add<T4>();
                builder.Add<T5>();
                builder.Add<T6>();
                builder.Add<T7>();
                archetype = ArchetypeGroup.Create(hash, builder);
            }
            var et = archetype.CreateEntity(out var chunkEntityIdx);
            var chunk = archetype.Chunks;
            *chunk.Get<T1>(chunkEntityIdx) = v1;
            *chunk.Get<T2>(chunkEntityIdx) = v2;
            *chunk.Get<T3>(chunkEntityIdx) = v3;
            *chunk.Get<T4>(chunkEntityIdx) = v4;
            *chunk.Get<T5>(chunkEntityIdx) = v5;
            *chunk.Get<T6>(chunkEntityIdx) = v6;
            *chunk.Get<T7>(chunkEntityIdx) = v7;
            return et;
        }
        /// <summary>
        /// 创建实体
        /// </summary>
        public unsafe Entity CreateEntity<T1, T2, T3, T4, T5, T6, T7, T8>(in T1 v1 = default, in T2 v2 = default, in T3 v3 = default, in T4 v4 = default, in T5 v5 = default, in T6 v6 = default, in T7 v7 = default, in T8 v8 = default) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged
        {
            Array.Clear(ComponentRegistry.ComponentTypeMaskBuffer);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T1>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T2>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T3>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T4>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T5>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T6>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T7>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T8>(), true);
            var hash = ComponentRegistry.GetHash(ComponentRegistry.ComponentTypeMaskBuffer);
            if (!ArchetypeGroup.ArchetypeMap.TryGetValue(hash, out var archetype))
            {
                using var builder = new ArchetypeBuilder(8);
                builder.Add<T1>();
                builder.Add<T2>();
                builder.Add<T3>();
                builder.Add<T4>();
                builder.Add<T5>();
                builder.Add<T6>();
                builder.Add<T7>();
                builder.Add<T8>();
                archetype = ArchetypeGroup.Create(hash, builder);
            }
            var et = archetype.CreateEntity(out var chunkEntityIdx);
            var chunk = archetype.Chunks;
            *chunk.Get<T1>(chunkEntityIdx) = v1;
            *chunk.Get<T2>(chunkEntityIdx) = v2;
            *chunk.Get<T3>(chunkEntityIdx) = v3;
            *chunk.Get<T4>(chunkEntityIdx) = v4;
            *chunk.Get<T5>(chunkEntityIdx) = v5;
            *chunk.Get<T6>(chunkEntityIdx) = v6;
            *chunk.Get<T7>(chunkEntityIdx) = v7;
            *chunk.Get<T8>(chunkEntityIdx) = v8;
            return et;
        }
        /// <summary>
        /// 创建实体
        /// </summary>
        public unsafe Entity CreateEntity<T1, T2, T3, T4, T5, T6, T7, T8, T9>(in T1 v1 = default, in T2 v2 = default, in T3 v3 = default, in T4 v4 = default, in T5 v5 = default, in T6 v6 = default, in T7 v7 = default, in T8 v8 = default, in T9 v9 = default) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged
        {
            Array.Clear(ComponentRegistry.ComponentTypeMaskBuffer);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T1>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T2>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T3>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T4>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T5>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T6>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T7>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T8>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T9>(), true);
            var hash = ComponentRegistry.GetHash(ComponentRegistry.ComponentTypeMaskBuffer);
            if (!ArchetypeGroup.ArchetypeMap.TryGetValue(hash, out var archetype))
            {
                using var builder = new ArchetypeBuilder(9);
                builder.Add<T1>();
                builder.Add<T2>();
                builder.Add<T3>();
                builder.Add<T4>();
                builder.Add<T5>();
                builder.Add<T6>();
                builder.Add<T7>();
                builder.Add<T8>();
                builder.Add<T9>();
                archetype = ArchetypeGroup.Create(hash, builder);
            }
            var et = archetype.CreateEntity(out var chunkEntityIdx);
            var chunk = archetype.Chunks;
            *chunk.Get<T1>(chunkEntityIdx) = v1;
            *chunk.Get<T2>(chunkEntityIdx) = v2;
            *chunk.Get<T3>(chunkEntityIdx) = v3;
            *chunk.Get<T4>(chunkEntityIdx) = v4;
            *chunk.Get<T5>(chunkEntityIdx) = v5;
            *chunk.Get<T6>(chunkEntityIdx) = v6;
            *chunk.Get<T7>(chunkEntityIdx) = v7;
            *chunk.Get<T8>(chunkEntityIdx) = v8;
            *chunk.Get<T9>(chunkEntityIdx) = v9;
            return et;
        }
        /// <summary>
        /// 创建实体
        /// </summary>
        public unsafe Entity CreateEntity<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(in T1 v1 = default, in T2 v2 = default, in T3 v3 = default, in T4 v4 = default, in T5 v5 = default, in T6 v6 = default, in T7 v7 = default, in T8 v8 = default, in T9 v9 = default, in T10 v10 = default) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged
        {
            Array.Clear(ComponentRegistry.ComponentTypeMaskBuffer);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T1>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T2>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T3>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T4>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T5>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T6>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T7>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T8>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T9>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T10>(), true);
            var hash = ComponentRegistry.GetHash(ComponentRegistry.ComponentTypeMaskBuffer);
            if (!ArchetypeGroup.ArchetypeMap.TryGetValue(hash, out var archetype))
            {
                using var builder = new ArchetypeBuilder(10);
                builder.Add<T1>();
                builder.Add<T2>();
                builder.Add<T3>();
                builder.Add<T4>();
                builder.Add<T5>();
                builder.Add<T6>();
                builder.Add<T7>();
                builder.Add<T8>();
                builder.Add<T9>();
                builder.Add<T10>();
                archetype = ArchetypeGroup.Create(hash, builder);
            }
            var et = archetype.CreateEntity(out var chunkEntityIdx);
            var chunk = archetype.Chunks;
            *chunk.Get<T1>(chunkEntityIdx) = v1;
            *chunk.Get<T2>(chunkEntityIdx) = v2;
            *chunk.Get<T3>(chunkEntityIdx) = v3;
            *chunk.Get<T4>(chunkEntityIdx) = v4;
            *chunk.Get<T5>(chunkEntityIdx) = v5;
            *chunk.Get<T6>(chunkEntityIdx) = v6;
            *chunk.Get<T7>(chunkEntityIdx) = v7;
            *chunk.Get<T8>(chunkEntityIdx) = v8;
            *chunk.Get<T9>(chunkEntityIdx) = v9;
            *chunk.Get<T10>(chunkEntityIdx) = v10;
            return et;
        }
        /// <summary>
        /// 创建实体
        /// </summary>
        public unsafe Entity CreateEntity<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(in T1 v1 = default, in T2 v2 = default, in T3 v3 = default, in T4 v4 = default, in T5 v5 = default, in T6 v6 = default, in T7 v7 = default, in T8 v8 = default, in T9 v9 = default, in T10 v10 = default, in T11 v11 = default) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged where T11 : unmanaged
        {
            Array.Clear(ComponentRegistry.ComponentTypeMaskBuffer);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T1>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T2>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T3>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T4>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T5>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T6>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T7>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T8>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T9>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T10>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T11>(), true);
            var hash = ComponentRegistry.GetHash(ComponentRegistry.ComponentTypeMaskBuffer);
            if (!ArchetypeGroup.ArchetypeMap.TryGetValue(hash, out var archetype))
            {
                using var builder = new ArchetypeBuilder(11);
                builder.Add<T1>();
                builder.Add<T2>();
                builder.Add<T3>();
                builder.Add<T4>();
                builder.Add<T5>();
                builder.Add<T6>();
                builder.Add<T7>();
                builder.Add<T8>();
                builder.Add<T9>();
                builder.Add<T10>();
                builder.Add<T11>();
                archetype = ArchetypeGroup.Create(hash, builder);
            }
            var et = archetype.CreateEntity(out var chunkEntityIdx);
            var chunk = archetype.Chunks;
            *chunk.Get<T1>(chunkEntityIdx) = v1;
            *chunk.Get<T2>(chunkEntityIdx) = v2;
            *chunk.Get<T3>(chunkEntityIdx) = v3;
            *chunk.Get<T4>(chunkEntityIdx) = v4;
            *chunk.Get<T5>(chunkEntityIdx) = v5;
            *chunk.Get<T6>(chunkEntityIdx) = v6;
            *chunk.Get<T7>(chunkEntityIdx) = v7;
            *chunk.Get<T8>(chunkEntityIdx) = v8;
            *chunk.Get<T9>(chunkEntityIdx) = v9;
            *chunk.Get<T10>(chunkEntityIdx) = v10;
            *chunk.Get<T11>(chunkEntityIdx) = v11;
            return et;
        }
        /// <summary>
        /// 创建实体
        /// </summary>
        public unsafe Entity CreateEntity<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(in T1 v1 = default, in T2 v2 = default, in T3 v3 = default, in T4 v4 = default, in T5 v5 = default, in T6 v6 = default, in T7 v7 = default, in T8 v8 = default, in T9 v9 = default, in T10 v10 = default, in T11 v11 = default, in T12 v12 = default) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged where T11 : unmanaged where T12 : unmanaged
        {
            Array.Clear(ComponentRegistry.ComponentTypeMaskBuffer);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T1>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T2>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T3>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T4>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T5>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T6>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T7>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T8>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T9>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T10>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T11>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T12>(), true);
            var hash = ComponentRegistry.GetHash(ComponentRegistry.ComponentTypeMaskBuffer);
            if (!ArchetypeGroup.ArchetypeMap.TryGetValue(hash, out var archetype))
            {
                using var builder = new ArchetypeBuilder(12);
                builder.Add<T1>();
                builder.Add<T2>();
                builder.Add<T3>();
                builder.Add<T4>();
                builder.Add<T5>();
                builder.Add<T6>();
                builder.Add<T7>();
                builder.Add<T8>();
                builder.Add<T9>();
                builder.Add<T10>();
                builder.Add<T11>();
                builder.Add<T12>();
                archetype = ArchetypeGroup.Create(hash, builder);
            }
            var et = archetype.CreateEntity(out var chunkEntityIdx);
            var chunk = archetype.Chunks;
            *chunk.Get<T1>(chunkEntityIdx) = v1;
            *chunk.Get<T2>(chunkEntityIdx) = v2;
            *chunk.Get<T3>(chunkEntityIdx) = v3;
            *chunk.Get<T4>(chunkEntityIdx) = v4;
            *chunk.Get<T5>(chunkEntityIdx) = v5;
            *chunk.Get<T6>(chunkEntityIdx) = v6;
            *chunk.Get<T7>(chunkEntityIdx) = v7;
            *chunk.Get<T8>(chunkEntityIdx) = v8;
            *chunk.Get<T9>(chunkEntityIdx) = v9;
            *chunk.Get<T10>(chunkEntityIdx) = v10;
            *chunk.Get<T11>(chunkEntityIdx) = v11;
            *chunk.Get<T12>(chunkEntityIdx) = v12;
            return et;
        }
        /// <summary>
        /// 创建实体
        /// </summary>
        public unsafe Entity CreateEntity<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(in T1 v1 = default, in T2 v2 = default, in T3 v3 = default, in T4 v4 = default, in T5 v5 = default, in T6 v6 = default, in T7 v7 = default, in T8 v8 = default, in T9 v9 = default, in T10 v10 = default, in T11 v11 = default, in T12 v12 = default, in T13 v13 = default) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged where T11 : unmanaged where T12 : unmanaged where T13 : unmanaged
        {
            Array.Clear(ComponentRegistry.ComponentTypeMaskBuffer);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T1>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T2>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T3>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T4>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T5>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T6>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T7>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T8>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T9>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T10>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T11>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T12>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T13>(), true);
            var hash = ComponentRegistry.GetHash(ComponentRegistry.ComponentTypeMaskBuffer);
            if (!ArchetypeGroup.ArchetypeMap.TryGetValue(hash, out var archetype))
            {
                using var builder = new ArchetypeBuilder(13);
                builder.Add<T1>();
                builder.Add<T2>();
                builder.Add<T3>();
                builder.Add<T4>();
                builder.Add<T5>();
                builder.Add<T6>();
                builder.Add<T7>();
                builder.Add<T8>();
                builder.Add<T9>();
                builder.Add<T10>();
                builder.Add<T11>();
                builder.Add<T12>();
                builder.Add<T13>();
                archetype = ArchetypeGroup.Create(hash, builder);
            }
            var et = archetype.CreateEntity(out var chunkEntityIdx);
            var chunk = archetype.Chunks;
            *chunk.Get<T1>(chunkEntityIdx) = v1;
            *chunk.Get<T2>(chunkEntityIdx) = v2;
            *chunk.Get<T3>(chunkEntityIdx) = v3;
            *chunk.Get<T4>(chunkEntityIdx) = v4;
            *chunk.Get<T5>(chunkEntityIdx) = v5;
            *chunk.Get<T6>(chunkEntityIdx) = v6;
            *chunk.Get<T7>(chunkEntityIdx) = v7;
            *chunk.Get<T8>(chunkEntityIdx) = v8;
            *chunk.Get<T9>(chunkEntityIdx) = v9;
            *chunk.Get<T10>(chunkEntityIdx) = v10;
            *chunk.Get<T11>(chunkEntityIdx) = v11;
            *chunk.Get<T12>(chunkEntityIdx) = v12;
            *chunk.Get<T13>(chunkEntityIdx) = v13;
            return et;
        }
        /// <summary>
        /// 创建实体
        /// </summary>
        public unsafe Entity CreateEntity<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(in T1 v1 = default, in T2 v2 = default, in T3 v3 = default, in T4 v4 = default, in T5 v5 = default, in T6 v6 = default, in T7 v7 = default, in T8 v8 = default, in T9 v9 = default, in T10 v10 = default, in T11 v11 = default, in T12 v12 = default, in T13 v13 = default, in T14 v14 = default) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged where T11 : unmanaged where T12 : unmanaged where T13 : unmanaged where T14 : unmanaged
        {
            Array.Clear(ComponentRegistry.ComponentTypeMaskBuffer);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T1>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T2>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T3>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T4>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T5>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T6>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T7>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T8>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T9>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T10>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T11>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T12>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T13>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T14>(), true);
            var hash = ComponentRegistry.GetHash(ComponentRegistry.ComponentTypeMaskBuffer);
            if (!ArchetypeGroup.ArchetypeMap.TryGetValue(hash, out var archetype))
            {
                using var builder = new ArchetypeBuilder(14);
                builder.Add<T1>();
                builder.Add<T2>();
                builder.Add<T3>();
                builder.Add<T4>();
                builder.Add<T5>();
                builder.Add<T6>();
                builder.Add<T7>();
                builder.Add<T8>();
                builder.Add<T9>();
                builder.Add<T10>();
                builder.Add<T11>();
                builder.Add<T12>();
                builder.Add<T13>();
                builder.Add<T14>();
                archetype = ArchetypeGroup.Create(hash, builder);
            }
            var et = archetype.CreateEntity(out var chunkEntityIdx);
            var chunk = archetype.Chunks;
            *chunk.Get<T1>(chunkEntityIdx) = v1;
            *chunk.Get<T2>(chunkEntityIdx) = v2;
            *chunk.Get<T3>(chunkEntityIdx) = v3;
            *chunk.Get<T4>(chunkEntityIdx) = v4;
            *chunk.Get<T5>(chunkEntityIdx) = v5;
            *chunk.Get<T6>(chunkEntityIdx) = v6;
            *chunk.Get<T7>(chunkEntityIdx) = v7;
            *chunk.Get<T8>(chunkEntityIdx) = v8;
            *chunk.Get<T9>(chunkEntityIdx) = v9;
            *chunk.Get<T10>(chunkEntityIdx) = v10;
            *chunk.Get<T11>(chunkEntityIdx) = v11;
            *chunk.Get<T12>(chunkEntityIdx) = v12;
            *chunk.Get<T13>(chunkEntityIdx) = v13;
            *chunk.Get<T14>(chunkEntityIdx) = v14;
            return et;
        }
        /// <summary>
        /// 创建实体
        /// </summary>
        public unsafe Entity CreateEntity<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(in T1 v1 = default, in T2 v2 = default, in T3 v3 = default, in T4 v4 = default, in T5 v5 = default, in T6 v6 = default, in T7 v7 = default, in T8 v8 = default, in T9 v9 = default, in T10 v10 = default, in T11 v11 = default, in T12 v12 = default, in T13 v13 = default, in T14 v14 = default, in T15 v15 = default) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged where T11 : unmanaged where T12 : unmanaged where T13 : unmanaged where T14 : unmanaged where T15 : unmanaged
        {
            Array.Clear(ComponentRegistry.ComponentTypeMaskBuffer);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T1>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T2>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T3>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T4>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T5>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T6>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T7>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T8>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T9>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T10>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T11>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T12>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T13>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T14>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T15>(), true);
            var hash = ComponentRegistry.GetHash(ComponentRegistry.ComponentTypeMaskBuffer);
            if (!ArchetypeGroup.ArchetypeMap.TryGetValue(hash, out var archetype))
            {
                using var builder = new ArchetypeBuilder(15);
                builder.Add<T1>();
                builder.Add<T2>();
                builder.Add<T3>();
                builder.Add<T4>();
                builder.Add<T5>();
                builder.Add<T6>();
                builder.Add<T7>();
                builder.Add<T8>();
                builder.Add<T9>();
                builder.Add<T10>();
                builder.Add<T11>();
                builder.Add<T12>();
                builder.Add<T13>();
                builder.Add<T14>();
                builder.Add<T15>();
                archetype = ArchetypeGroup.Create(hash, builder);
            }
            var et = archetype.CreateEntity(out var chunkEntityIdx);
            var chunk = archetype.Chunks;
            *chunk.Get<T1>(chunkEntityIdx) = v1;
            *chunk.Get<T2>(chunkEntityIdx) = v2;
            *chunk.Get<T3>(chunkEntityIdx) = v3;
            *chunk.Get<T4>(chunkEntityIdx) = v4;
            *chunk.Get<T5>(chunkEntityIdx) = v5;
            *chunk.Get<T6>(chunkEntityIdx) = v6;
            *chunk.Get<T7>(chunkEntityIdx) = v7;
            *chunk.Get<T8>(chunkEntityIdx) = v8;
            *chunk.Get<T9>(chunkEntityIdx) = v9;
            *chunk.Get<T10>(chunkEntityIdx) = v10;
            *chunk.Get<T11>(chunkEntityIdx) = v11;
            *chunk.Get<T12>(chunkEntityIdx) = v12;
            *chunk.Get<T13>(chunkEntityIdx) = v13;
            *chunk.Get<T14>(chunkEntityIdx) = v14;
            *chunk.Get<T15>(chunkEntityIdx) = v15;
            return et;
        }
        /// <summary>
        /// 创建实体
        /// </summary>
        public unsafe Entity CreateEntity<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(in T1 v1 = default, in T2 v2 = default, in T3 v3 = default, in T4 v4 = default, in T5 v5 = default, in T6 v6 = default, in T7 v7 = default, in T8 v8 = default, in T9 v9 = default, in T10 v10 = default, in T11 v11 = default, in T12 v12 = default, in T13 v13 = default, in T14 v14 = default, in T15 v15 = default, in T16 v16 = default) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged where T11 : unmanaged where T12 : unmanaged where T13 : unmanaged where T14 : unmanaged where T15 : unmanaged where T16 : unmanaged
        {
            Array.Clear(ComponentRegistry.ComponentTypeMaskBuffer);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T1>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T2>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T3>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T4>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T5>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T6>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T7>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T8>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T9>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T10>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T11>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T12>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T13>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T14>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T15>(), true);
            BitArrayOperator.SetBit(ComponentRegistry.ComponentTypeMaskBuffer, ComponentRegistry.GetId<T16>(), true);
            var hash = ComponentRegistry.GetHash(ComponentRegistry.ComponentTypeMaskBuffer);
            if (!ArchetypeGroup.ArchetypeMap.TryGetValue(hash, out var archetype))
            {
                using var builder = new ArchetypeBuilder(16);
                builder.Add<T1>();
                builder.Add<T2>();
                builder.Add<T3>();
                builder.Add<T4>();
                builder.Add<T5>();
                builder.Add<T6>();
                builder.Add<T7>();
                builder.Add<T8>();
                builder.Add<T9>();
                builder.Add<T10>();
                builder.Add<T11>();
                builder.Add<T12>();
                builder.Add<T13>();
                builder.Add<T14>();
                builder.Add<T15>();
                builder.Add<T16>();
                archetype = ArchetypeGroup.Create(hash, builder);
            }
            var et = archetype.CreateEntity(out var chunkEntityIdx);
            var chunk = archetype.Chunks;
            *chunk.Get<T1>(chunkEntityIdx) = v1;
            *chunk.Get<T2>(chunkEntityIdx) = v2;
            *chunk.Get<T3>(chunkEntityIdx) = v3;
            *chunk.Get<T4>(chunkEntityIdx) = v4;
            *chunk.Get<T5>(chunkEntityIdx) = v5;
            *chunk.Get<T6>(chunkEntityIdx) = v6;
            *chunk.Get<T7>(chunkEntityIdx) = v7;
            *chunk.Get<T8>(chunkEntityIdx) = v8;
            *chunk.Get<T9>(chunkEntityIdx) = v9;
            *chunk.Get<T10>(chunkEntityIdx) = v10;
            *chunk.Get<T11>(chunkEntityIdx) = v11;
            *chunk.Get<T12>(chunkEntityIdx) = v12;
            *chunk.Get<T13>(chunkEntityIdx) = v13;
            *chunk.Get<T14>(chunkEntityIdx) = v14;
            *chunk.Get<T15>(chunkEntityIdx) = v15;
            *chunk.Get<T16>(chunkEntityIdx) = v16;
            return et;
        }
    }
}