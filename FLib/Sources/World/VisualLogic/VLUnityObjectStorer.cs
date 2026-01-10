// ==================== qcbf@qq.com | 2025-07-01 ====================

#if UNITY_PROJ
using System;
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;

namespace FLib.Worlds
{
    [Serializable]
    public class VLUnityObjectStorer
    {
        public Object[] UnityObjects = Array.Empty<Object>();
        public List<int> Frees = new();

        /// <summary>
        /// 
        /// </summary>
        public virtual Object this[int index]
        {
            get => UnityObjects[index];
            set => UnityObjects[index] = value;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void AddCapacity(int size)
        {
            var oldSize = UnityObjects.Length;
            var newSize = oldSize + size;
            Array.Resize(ref UnityObjects, newSize);
            for (var i = oldSize; i < newSize; i++)
                Frees.Add(i);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int Alloc(Object obj = null)
        {
            if (Frees.Count == 0)
                AddCapacity(1);
            var index = Frees[^1];
            Frees.RemoveAt(Frees.Count - 1);
            UnityObjects[index] = obj;
            return index;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Free(int index)
        {
            UnityObjects[index] = null;
            Frees.Add(index);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Clear()
        {
            Array.Fill(UnityObjects, null);
            Frees.Clear();
            for (var i = UnityObjects.Length - 1; i >= 0; i--)
                Frees.Add(i);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Trim()
        {
            if (Frees.Count == 0)
                return;
            var freesHash = Frees.ToHashSet();
            var i = UnityObjects.Length - 1;
            for (; i >= 0; i--)
            {
                if (!freesHash.Remove(i))
                {
                    Array.Resize(ref UnityObjects, i + 1);
                    break;
                }
            }
            if (i < 0)
            {
                Frees.Clear();
                UnityObjects = Array.Empty<Object>();
            }
            else
            {
                Frees = freesHash.ToList();
            }
            Frees.TrimExcess();
        }
    }
}

#endif
