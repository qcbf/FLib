//==================={By Qcbf|qcbf@qq.com|2/25/2022 12:30:31 PM}===================

using FLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace FLib
{
    [DebuggerDisplay("Count = {Count}")]
    public ref struct StackPriorityList<TValue, TPriority>
    {
        public Span<Element> Buffer;
        private readonly IComparer<TPriority> mComparer;

        public readonly Span<Element> Span => Buffer.Slice(0, Count);

        public int Count
        {
            get;
            private set;
        }

        public readonly ref TValue this[int index]
        {
            get => ref Buffer[index].Value;
        }

        public struct Element
        {
            public TValue Value;
            public TPriority Priority;
            public Element(TValue value, TPriority priority) { Value = value; Priority = priority; }
            public static implicit operator Element((TValue v, TPriority p) v) => new(v.v, v.p);
            public readonly override string ToString() => $"{{{Value}:{Priority}}}";
        }
        public StackPriorityList(in Span<Element> buffer, IComparer<TPriority> comparer = null)
        {
            Buffer = buffer;
            Count = 0;
            mComparer = comparer ?? Comparer<TPriority>.Default;
        }

        ///// <summary>
        ///// 
        ///// </summary>
        //public void GrowCapacity(int capacity)
        //{
        //    if (capacity < 8) capacity = 8;
        //    if (OnResizeCallback == null)
        //    {
        //        if (Buffer.IsEmpty)
        //        {
        //            OnResizeCallback = v => new HSpan<Element>(new Element[v]);
        //            mComparer = Comparer<TPriority>.Default;
        //            Buffer = OnResizeCallback(8);
        //        }
        //        else
        //        {
        //            throw new Exception("cannot resize size to:" + capacity);
        //        }
        //    }
        //    var newBuffer = OnResizeCallback.Invoke(capacity);
        //    Buffer.CopyTo(newBuffer);
        //    Buffer = newBuffer;
        //}

        public void Add(in TValue value, in TPriority priority) => Add(new Element(value, priority));
        public void Add(in Element item)
        {
            //if (Count >= Buffer.Length) GrowCapacity(MathEx.GetNextPowerOfTwo(Buffer.Length + 1));
            if (Count >= Buffer.Length) throw new Exception($"overflow {Count}/{Buffer.Length}");
            if (Count++ == 0)
            {
                Buffer[0] = item;
            }
            else
            {
                var i = Count - 1;
                for (; i > 0;)
                {
                    if (mComparer.Compare(Buffer[i - 1].Priority, item.Priority) > 0)
                    {
                        Buffer[i] = Buffer[i - 1];
                    }
                    else
                    {
                        break;
                    }
                    //if (IsFastMode)
                    //{
                    //    //i -= ;
                    //}
                    //else
                    //{
                    --i;
                    //}
                }
                Buffer[i] = item;
            }
        }

        public void RemoveAt(int index)
        {
            Count--;
            for (var i = index; i < Count; i++)
            {
                Buffer[i] = Buffer[i + 1];
            }
        }

        public void Clear()
        {
            Count = 0;
        }

        public readonly int IndexOf(in TValue item)
        {
            for (var i = 0; i < Count; i++)
            {
                if (EqualityComparer<TValue>.Default.Equals(Buffer[i].Value, item))
                {
                    return i;
                }
            }
            return -1;
        }

        public bool Remove(in TValue item)
        {
            var index = IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        public readonly bool Contains(in TValue item)
        {
            for (var i = 0; i < Count; i++)
            {
                if (EqualityComparer<TValue>.Default.Equals(Buffer[i].Value, item))
                {
                    return true;
                }
            }
            return false;
        }

        public readonly Element[] ToArray()
        {
            return Count > 0 ? Buffer[..Count].ToArray() : Array.Empty<Element>();
        }

        public readonly void CopyTo(Span<Element> array)
        {
            Buffer.CopyTo(array);
        }

    }
}
