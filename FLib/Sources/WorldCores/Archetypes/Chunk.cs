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
    public unsafe class Chunk : IObjectPoolActivatable, IObjectPoolDeactivatable
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
        public ComponentTypeOffsetHelper ComponentOffset;

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
            ComponentOffset = default;
            Count = 0;
            Previous = null;
        }

        /// <summary>
        /// 
        /// </summary>
        public Entity* GetEntity(int entityIdx)
        {
            Debug.Assert(entityIdx < Count);
            return (Entity*)Buffer + entityIdx;
        }

        /// <summary>
        /// 
        /// </summary>
        public ref T GetRef<T>(ushort entityIdx) where T : unmanaged
        {
            return ref *Get<T>(entityIdx);
        }

        /// <summary>
        /// 
        /// </summary>
        public T* Get<T>(ushort entityIdx) where T : unmanaged
        {
            Debug.Assert(entityIdx < Count);
            return (T*)(Buffer + ComponentOffset.Get<T>()) + entityIdx;
        }

        /// <summary>
        /// 
        /// </summary>
        public void* Get(ushort entityIdx, in ComponentMeta meta)
        {
            Debug.Assert(entityIdx < Count);
            return Buffer + ComponentOffset[meta.Id] + meta.Size * entityIdx;
        }

        /// <summary>
        /// 
        /// </summary>
        public object GetObj(ushort entityIdx, in ComponentMeta meta)
        {
            var ptr = Get(entityIdx, meta);
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
        public void ClearMemory(ushort entityIdx, in ComponentMeta meta)
        {
            Debug.Assert(entityIdx < Count);
            Unsafe.InitBlockUnaligned(Buffer + ComponentOffset[meta.Id] + meta.Size * entityIdx, 0, meta.Size);
        }

        /// <summary>
        /// 
        /// </summary>
        public Span<T> GetAll<T>() where T : unmanaged
        {
            return new Span<T>(Buffer + ComponentOffset[ComponentGenericMap<T>.Id], Count);
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