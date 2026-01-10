//==================={By Qcbf|qcbf@qq.com|12/10/2020 4:15:36 PM}===================

using FLib;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FLib
{
    public class ArrayFLibUtility
    {
        /// <summary>
        /// 创建一个被元素实例填满的数组
        /// </summary>
        public static T[] CreateFullElements<T>(int length, in T value)
        {
            var arr = new T[length];
            for (var i = 0; i < arr.Length; i++)
            {
                arr[i] = value;
            }

            return arr;
        }

        /// <summary>
        /// 创建一个被元素实例填满的数组
        /// </summary>
        public static T[] CreateFullElements<T>(int length) where T : class
        {
            var arr = new T[length];
            for (var i = 0; i < arr.Length; i++)
            {
                arr[i] = Activator.CreateInstance<T>();
            }

            return arr;
        }

        /// <summary>
        /// 创建一个被元素实例填满的数组
        /// </summary>
        public static Array CreateFullElements(Type t, int length)
        {
            var arr = Array.CreateInstance(t, length);
            for (var i = 0; i < arr.Length; i++)
            {
                arr.SetValue(Activator.CreateInstance(t), i);
            }

            return arr;
        }

        /// <summary>
        /// 移除一个元素,并且把后面的往前挪
        /// </summary>
        public static bool Remove<T>(ref T[] arr, in T value, bool isResizeProcess = true)
        {
            var isFound = false;
            var count = arr.Length;
            for (var i = 0; i < count; i++)
            {
                if (isFound)
                {
                    arr[i - 1] = arr[i];
                }
                else if (EqualityComparer<T>.Default.Equals(arr[i], value))
                {
                    isFound = true;
                    if (arr.Length <= i + 1) continue;
                    arr[i] = arr[i + 1];
                    i++;
                }
            }

            if (isResizeProcess && isFound)
                Array.Resize(ref arr, count - 1);
            return isFound;
        }


        /// <summary>
        /// 移除一个元素,并且把后面的往前挪
        /// </summary>
        public static void RemoveAt<T>(ref T[] arr, int index)
        {
            var count = arr.Length;
            for (var i = index + 1; i < count; i++)
            {
                arr[i - 1] = arr[i];
            }

            Array.Resize(ref arr, count - 1);
        }

        /// <summary>
        /// 添加一个元素到尾部
        /// </summary>
        public static void Add<T>(ref T[] arr, in T value)
        {
            if (arr == null)
            {
                arr = new[] { value };
            }
            else
            {
                var count = arr.Length;
                Array.Resize(ref arr, count + 1);
                arr[count] = value;
            }
        }

        /// <summary>
        /// 添加一个元素到尾部
        /// </summary>
        public static void AddRange<T>(ref T[] arr, in T[] value)
        {
            if (arr == null)
            {
                arr = (T[])value.Clone();
            }
            else
            {
                var count = arr.Length;
                Array.Resize(ref arr, count + value.Length);
                Array.Copy(value, 0, arr, count, value.Length);
            }
        }

        /// <summary>
        /// 添加一个元素到指定索引
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr">数组</param>
        /// <param name="value">值</param>
        /// <param name="index">如果小于数组长度正常插入. 如果等于数组长度自动调用Add方法. 如果大于数组长度,会自动填充到指定索引</param>
        public static void Insert<T>(ref T[] arr, in T value, int index)
        {
            if (arr == null)
            {
                arr = new T[index + 1];
                arr[index] = value;
            }
            else
            {
                var count = arr.Length;
                if (index == count)
                {
                    Add(ref arr, value);
                }
                else if (index > count)
                {
                    Array.Resize(ref arr, count + index - count + 1);
                    arr[index] = value;
                }
                else
                {
                    Array.Resize(ref arr, count + 1);
                    Array.Copy(arr, index, arr, index + 1, count - index);
                    arr[index] = value;
                }
            }
        }

        /// <summary>
        /// 获取指定索引 或者最后一个
        /// </summary>
        public static ref T GetIndexOrBottom<T>(T[] arr, int index)
        {
            var count = arr.Length;
            if (index >= count)
                return ref arr[count - 1];
            return ref arr[index];
        }

        /// <summary>
        /// 获取指定索引 或者最后一个
        /// </summary>
        public static T GetIndexOrDefault<T>(T[] arr, int index, in T defaultValue = default)
        {
            if (arr == null) return default;
            var count = arr.Length;
            return index >= count ? defaultValue : arr.ElementAt(index);
        }

        /// <summary>
        /// 改变索引位置,中间的元素索引依次+1或者-1
        /// </summary>
        public static void ChangeIndex<T>(T[] arr, int oldIndex, int newIndex)
        {
            newIndex = Math.Min(newIndex, arr.Length - 1);
            if (newIndex > oldIndex)
            {
                var temp = arr[oldIndex];
                for (var i = oldIndex + 1; i <= newIndex; i++)
                {
                    arr[i - 1] = arr[i];
                }

                arr[newIndex] = temp;
            }
            else
            {
                var temp = arr[oldIndex];
                for (var i = oldIndex; i > newIndex; i--)
                {
                    arr[i] = arr[i - 1];
                }

                arr[newIndex] = temp;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static TOut[] Convert<TOut, TArrayElement>(TArrayElement[] src, Func<int, TArrayElement, TOut> handler)
        {
            var result = new TOut[src.Length];
            for (var i = 0; i < result.Length; i++) result[i] = handler(i, src[i]);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        public static bool IsNullOrEmpty(in Array v)
        {
            return v == null || v.Length == 0;
        }
    }
}
