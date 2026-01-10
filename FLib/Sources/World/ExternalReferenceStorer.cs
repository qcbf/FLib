// ==================== qcbf@qq.com | 2025-07-01 ====================

using System;
using System.Collections.Generic;
using System.Linq;

namespace FLib.Worlds
{
    [Serializable]
    public abstract class ExternalReferenceStorer
    {
        public List<int> FreeIndexes = new();

        public abstract object this[int index] { get; set; }
        public abstract void SetArraySize(int newSize);
        public abstract int GetArraySize();

        /// <summary>
        /// 
        /// </summary>
        protected virtual void SetCapacity(int newSize)
        {
            var oldSize = GetArraySize();
            SetArraySize(newSize);
            for (var i = oldSize; i < newSize; i++)
                FreeIndexes.Add(i);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int Set(object obj, int relativeIndex)
        {
            if (obj == null)
            {
                if (relativeIndex > 0)
                    Free(relativeIndex - 1);
                return 0;
            }
            if (relativeIndex > 0)
                this[relativeIndex - 1] = obj;
            else
                relativeIndex = Alloc(obj);
            return relativeIndex;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual T Get<T>(int relativeIndex) => (T)Get(relativeIndex);

        /// <summary>
        /// 
        /// </summary>
        public virtual object Get(int relativeIndex)
        {
            return relativeIndex > 0 ? this[relativeIndex - 1] : null;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int Alloc(object obj = null)
        {
            if (FreeIndexes.Count == 0)
                SetCapacity(GetArraySize() + 1);
            var index = FreeIndexes[^1];
            FreeIndexes.RemoveAt(FreeIndexes.Count - 1);
            this[index] = obj;
            return index;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Free(int index)
        {
            this[index] = null;
            FreeIndexes.Add(index);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Clear()
        {
            FreeIndexes.Clear();
            for (var i = GetArraySize() - 1; i >= 0; i--)
                FreeIndexes.Add(i);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Trim()
        {
            if (FreeIndexes.Count == 0)
                return;
            var freesHash = FreeIndexes.ToHashSet();
            var i = GetArraySize() - 1;
            for (; i >= 0; i--)
            {
                if (!freesHash.Remove(i))
                {
                    SetArraySize(i + 1);
                    break;
                }
            }
            if (i < 0)
            {
                FreeIndexes.Clear();
                SetArraySize(0);
            }
            else
            {
                FreeIndexes = freesHash.ToList();
            }
            FreeIndexes.TrimExcess();
        }
    }

    public interface IExternalReferenceField
    {
        public int Index { get; set; }
    }

    [Serializable]
    public struct ExternalReferenceField<T> : IExternalReferenceField, IBytesSerializable where T : class
    {
        private int _index;
        public int Index { readonly get => _index - 1; set => _index = value + 1; }
        int IExternalReferenceField.Index { get => Index; set => Index = value; }
        public static implicit operator int(ExternalReferenceField<T> v) => v.Index;
        public void Z_BytesWrite(ref BytesWriter writer) => writer.PushVInt(_index);
        public void Z_BytesRead(ref BytesReader reader) => _index = (int)reader.ReadVInt();
    }
}
