//==================={By Qcbf|qcbf@qq.com|12/30/2019 4:43:39 PM}===================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FLib
{
    public class CircleStack<T> : IReadOnlyCollection<T>, ICollection
    {
        private int mBeginIndex;
        private int mEndIndex = -1;
        private T[] mBuffer;

        /// <summary>
        /// 
        /// </summary>
        public T this[int index]
        {
            get => mBuffer[GetBufferIndex(index)];
            set => mBuffer[GetBufferIndex(index)] = value;
        }

        /// <summary>
        /// 
        /// </summary>
        public int Capacity { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public int Count { get; private set; }

        bool ICollection.IsSynchronized => false;
        object ICollection.SyncRoot => this;


        public struct Enumerator : IEnumerator<T>
        {
            private CircleStack<T> mTarget;
            private int mIndex;


            public T Current { get; private set; }

            readonly object IEnumerator.Current
            {
                get => Current;
            }

            public Enumerator(CircleStack<T> list)
            {
                mTarget = list;
                mIndex = Math.Max(list.mBeginIndex, 0);
                Current = default;
            }

            public void Dispose()
            {
                mTarget = null;
            }

            public bool MoveNext()
            {
                var result = mIndex <= mTarget.mEndIndex;
                if (result)
                {
                    Current = mTarget[mTarget.GetBufferIndex(mIndex++)];
                }

                return result;
            }

            public void Reset()
            {
                mTarget = null;
                mIndex = 0;
                Current = default;
            }
        }


        public CircleStack(int capacity)
        {
            Capacity = capacity;
            mBuffer = new T[Capacity];
        }


        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBufferIndex(int v)
        {
            return v % Capacity;
        }

        public void CopyTo(Array array, int index)
        {
            Array.Copy(mBuffer, 0, array, index, 1);
        }

        /// <summary>
        /// 
        /// </summary>
        public T Pop()
        {
            if (mEndIndex == -1 || mBeginIndex > mEndIndex) return default;
            var result = mBuffer[GetBufferIndex(mEndIndex)];
            mBuffer[GetBufferIndex(mEndIndex--)] = default;
            Count = Math.Max(0, Count - 1);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        public void RemoveAt(int index)
        {
            for (var i = index; i < mEndIndex; i++)
            {
                mBuffer[i] = mBuffer[GetBufferIndex(i + 1)];
            }

            mBuffer[GetBufferIndex(mEndIndex--)] = default;
            Count = Math.Max(0, Count - 1);
        }

        /// <summary>
        /// 
        /// </summary>
        public T Peek()
        {
            if (mEndIndex == -1 || mBeginIndex > mEndIndex) return default;
            return mBuffer[GetBufferIndex(mEndIndex)];
        }

        /// <summary>
        /// 
        /// </summary>
        public void Push(T value)
        {
            Push() = value;
        }

        /// <summary>
        /// 
        /// </summary>
        public ref T Push()
        {
            ++mEndIndex;
            mBeginIndex = mEndIndex - Capacity + 1;
            Count = Math.Min(Capacity, Count + 1);
            return ref mBuffer[GetBufferIndex(mEndIndex)];
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetCapacity(int v)
        {
            Capacity = v;
            if (Capacity >= mBuffer.Length) Array.Resize(ref mBuffer, Capacity);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            Count = 0;
            mEndIndex = -1;
            mBeginIndex = -Capacity;
            for (var i = 0; i < Capacity; i++)
            {
                mBuffer[i] = default;
            }
        }
    }
}
