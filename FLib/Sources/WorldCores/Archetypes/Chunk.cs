// ==================== qcbf@qq.com |2026-01-02 ====================

using System;
using System.Diagnostics;
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
        public void* Get(ushort entityIdx, ushort componentSize, IncrementId componentId)
        {
            Debug.Assert(entityIdx < Count);
            return Buffer + ComponentOffset[componentId] + componentSize * entityIdx;
        }

        /// <summary>
        /// 
        /// </summary>
        public void ClearMemory(ushort entityIdx, ushort componentSize, IncrementId componentId)
        {
            Debug.Assert(entityIdx < Count);
            Unsafe.InitBlockUnaligned(Buffer + ComponentOffset[componentId] + componentSize * entityIdx, 0, componentSize);
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
        public object[] GetAll(in ComponentMeta meta)
        {
            var arr = new object[Count];
            var ptr = Buffer + ComponentOffset[meta.Id];
            for (var i = 0; i < Count; i++)
                arr[i] = Marshal.PtrToStructure((IntPtr)ptr + meta.Size * i, meta.Type);
            return arr;
        }
    }
}