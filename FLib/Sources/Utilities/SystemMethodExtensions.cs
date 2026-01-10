//==================={By Qcbf|qcbf@qq.com}===================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace FLib
{
    public static class SystemMethodExtensions
    {
        #region extends string class
        public static sbyte ToSByte(this string str) => sbyte.TryParse(str, out var result) ? result : (sbyte)str.ToFloat();
        public static sbyte ToSByte(this in ReadOnlySpan<char> str) => sbyte.TryParse(str, out var result) ? result : (sbyte)str.ToFloat();
        public static byte ToByte(this string str) => byte.TryParse(str, out var result) ? result : (byte)str.ToFloat();
        public static byte ToByte(this in ReadOnlySpan<char> str) => byte.TryParse(str, out var result) ? result : (byte)str.ToFloat();
        public static uint ToUInt(this string str) => uint.TryParse(str, out var result) ? result : (uint)str.ToDouble();
        public static uint ToUInt(this in ReadOnlySpan<char> str) => uint.TryParse(str, out var result) ? result : (uint)str.ToDouble();
        public static int ToInt(this string str) => int.TryParse(str, out var result) ? result : (int)str.ToDouble();
        public static int ToInt(this in ReadOnlySpan<char> str) => int.TryParse(str, out var result) ? result : (int)str.ToDouble();
        public static ushort ToUShort(this string str) => ToUShort(str.AsSpan());
        public static ushort ToUShort(this in ReadOnlySpan<char> str) => ushort.TryParse(str, out var result) ? result : (ushort)str.ToFloat();
        public static short ToShort(this string str) => short.TryParse(str, out var result) ? result : (short)str.ToFloat();
        public static short ToShort(this in ReadOnlySpan<char> str) => short.TryParse(str, out var result) ? result : (short)str.ToFloat();

        public static float ToFloat(this string str)
        {
            float.TryParse(str, out var rt);
            return rt;
        }

        public static float ToFloat(this in ReadOnlySpan<char> str)
        {
            float.TryParse(str, out var rt);
            return rt;
        }

        public static FNum ToFNum(this string str)
        {
            decimal.TryParse(str, out var rt);
            return (FNum)rt;
        }

        public static FNum ToFNum(this in ReadOnlySpan<char> str)
        {
            decimal.TryParse(str, out var rt);
            return (FNum)rt;
        }

        public static double ToDouble(this string str)
        {
            double.TryParse(str, out var rt);
            return rt;
        }

        public static double ToDouble(this in ReadOnlySpan<char> str)
        {
            double.TryParse(str, out var rt);
            return rt;
        }

        public static long ToLong(this string str) => long.TryParse(str, out var rt) ? rt : (long)str.ToDouble();
        public static long ToLong(this in ReadOnlySpan<char> str) => long.TryParse(str, out var rt) ? rt : (long)str.ToDouble();

        public static ulong ToULong(this string str)
        {
            ulong.TryParse(str, out var rt);
            return rt;
        }

        public static ulong ToULong(this in ReadOnlySpan<char> str)
        {
            ulong.TryParse(str, out var rt);
            return rt;
        }

        public static unsafe T ToBuiltinType<T>(this in ReadOnlySpan<char> str)
        {
            void* ptr = null;
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    var boolVal = bool.TryParse(str, out var result) ? result : str.ToInt() == 1;
                    ptr = &boolVal;
                    break;
                case TypeCode.Byte:
                    var byteVal = str.ToByte();
                    ptr = &byteVal;
                    break;
                case TypeCode.SByte:
                    var sByteVal = str.ToSByte();
                    ptr = &sByteVal;
                    break;
                case TypeCode.Int16:
                    var int16Val = str.ToShort();
                    ptr = &int16Val;
                    break;
                case TypeCode.UInt16:
                    var uint16Val = str.ToUShort();
                    ptr = &uint16Val;
                    break;
                case TypeCode.Int32:
                    var int32Val = str.ToInt();
                    ptr = &int32Val;
                    break;
                case TypeCode.UInt32:
                    var uint32Val = str.ToUInt();
                    ptr = &uint32Val;
                    break;
                case TypeCode.Int64:
                    var int64Val = str.ToLong();
                    ptr = &int64Val;
                    break;
                case TypeCode.UInt64:
                    var uint64Val = str.ToULong();
                    ptr = &uint64Val;
                    break;
                case TypeCode.Single:
                    var floatVal = str.ToFloat();
                    ptr = &floatVal;
                    break;
                case TypeCode.Double:
                    var doubleVal = str.ToDouble();
                    ptr = &doubleVal;
                    break;
                default:
                    throw new Exception($"not found type: {typeof(T)}");
            }

            return ptr != null ? Unsafe.AsRef<T>(ptr) : default;
        }
        #endregion

        #region Type
        /// <summary>
        /// 获取当前类型默认值
        /// </summary>
        public static object DefaultValue(this Type t, bool strToEmpty = true)
        {
            if (t.IsValueType) return TypeAssistant.New(t);
            if (strToEmpty && t == typeof(string)) return string.Empty;
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        public static bool IsStatic(this Type t)
        {
            return t.IsAbstract && t.IsSealed;
        }
        #endregion

        #region task
        /// <summary>
        ///
        /// </summary>
        public static void When<T>(this Task<T> task, ROAction<T> action)
        {
            if (!task.IsCompleted)
            {
                task.ContinueWith(task1 =>
                {
                    if (!TaskFailureHandle(task1))
                        action(task.Result);
                }, TaskScheduler.Default);
            }
            else
            {
                if (!TaskFailureHandle(task))
                    action(task.Result);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public static void Forget(this Task task)
        {
            if (task.IsCompleted)
                TaskFailureHandle(task);
            else
                task.ContinueWith(TaskFailureHandle, TaskScheduler.Default);
        }

        /// <summary>
        ///
        /// </summary>
        public static bool TaskFailureHandle(Task task)
        {
            if (task.Exception != null)
            {
                Log.Error?.Write(task.Exception);
                return true;
            }
            if (task.IsCanceled)
            {
                // Log.Info?.Write($"A task[{task.Id}] was canceled.");
                return true;
            }
            return false;
        }
        #endregion
    }
}
