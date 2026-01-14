// ==================== qcbf@qq.com |2026-01-02 ====================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FLib.WorldCores
{
    public sealed unsafe class Chunk : IObjectPoolActivatable, IObjectPoolDeactivatable
    {
        /// <summary>
        /// head: [entity...]
        /// body: [comp1..., comp2...]
        /// </summary>
        public byte* Buffer;

        /// <summary>
        /// 
        /// </summary>
        public ushort Count;

        /// <summary>
        /// 
        /// </summary>
        public ComponentSparseList Sparse;

        /// <summary>
        /// 
        /// </summary>
        public int SharedComponentIndex;

        /// <summary>
        /// 
        /// </summary>
        public Chunk Previous;

        void IObjectPoolActivatable.ObjectPoolActivate()
        {
            Buffer = GlobalSetting.ChunkAllocator.Alloc();
        }

        void IObjectPoolDeactivatable.ObjectPoolDeactivatable()
        {
            GlobalSetting.ChunkAllocator.Free(ref Buffer);
            Sparse = default;
            Count = 0;
            Previous = null;
        }

        /// <summary>
        /// 
        /// </summary>
        public Entity* GetEntity(int entityIndex)
        {
            Debug.Assert(entityIndex < Count);
            return (Entity*)Buffer + entityIndex;
        }

        /// <summary>
        /// 
        /// </summary>
        public ref T GetRef<T>(ushort entityIndex)
        {
            return ref *Get<T>(entityIndex);
        }

#pragma warning disable CS8500
        /// <summary>
        /// 
        /// </summary>
        public T* Get<T>(ushort entityIndex)
        {
            Debug.Assert(entityIndex < Count);
            Debug.Assert(!RuntimeHelpers.IsReferenceOrContainsReferences<T>());
            return (T*)(Buffer + Sparse.Get<T>()) + entityIndex;
        }

        /// <summary>
        /// 
        /// </summary>
        public void* Get(ushort entityIndex, in ComponentMeta meta)
        {
            Debug.Assert(entityIndex < Count);
            return Buffer + Sparse[meta.Id] + meta.Size * entityIndex;
        }

        /// <summary>
        /// 
        /// </summary>
        public object GetObj(ushort entityIndex, in ComponentMeta meta)
        {
            var ptr = Get(entityIndex, meta);
            object obj;
            if (meta.Type.IsGenericType)
            {
                obj = Activator.CreateInstance(meta.Type);
                var gch = GCHandle.Alloc(obj, GCHandleType.Pinned);
                try
                {
                    System.Buffer.MemoryCopy(ptr, (void*)gch.AddrOfPinnedObject(), meta.Size, meta.Size);
                }
                finally
                {
                    gch.Free();
                }
            }
            else
            {
                obj = Marshal.PtrToStructure((IntPtr)ptr, meta.Type);
            }

            return obj;
        }

        /// <summary>
        /// 
        /// </summary>
        public void ClearMemory(ushort entityIndex, in ComponentMeta meta)
        {
            Debug.Assert(entityIndex < Count);
            Unsafe.InitBlockUnaligned(Buffer + Sparse[meta.Id] + meta.Size * entityIndex, 0, meta.Size);
        }

        /// <summary>
        /// 
        /// </summary>
        public Span<T> GetAll<T>() where T : unmanaged
        {
            return new Span<T>(Buffer + Sparse[ComponentGenericMap<T>.Id], Count);
        }

        /// <summary>
        /// 
        /// </summary>
        public IList GetAll(in ComponentMeta meta, IList list = null)
        {
            list ??= new List<object>(Count);
            for (ushort i = 0; i < Count; i++)
                list.Add(GetObj(i, meta));
            return list;
        }
    }
}