// ==================== qcbf@qq.com | 2025-07-01 ====================

using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_PROJ
using UnityEngine;
using Object = UnityEngine.Object;
#endif

namespace FLib.Worlds
{
    [BytesPackGenCustomCode]
    public abstract class VLValueBase
    {
        [NonSerialized, BytesPackGenCustomCode(ReadCode = "${FieldName}.Env=Env;")]
        public VLEnvironment Env;

        [BytesPackGenCustomCode(WriteCode = "${Gen}", ReadCode = "${Gen}")]
        public string RefVarName;

        public abstract Type ValueType { get; }
        public abstract object ObjectValue { get; }
        public abstract object ObjectRawValue { get; set; }
        public VLValueBase FixedVLValue => IsFixedValue ? this : Env.Variables[RefVarName].FixedVLValue;
        public bool IsFixedValue => string.IsNullOrEmpty(RefVarName);
        public abstract VLValueBase Clone();
        public VLValueBase GetRef() => !string.IsNullOrEmpty(RefVarName) ? Env.Variables[RefVarName] : null;
    }

    public class VLVariable<T> : VLValueBase
    {
        public T RawValue;

        public T Value
        {
            get
            {
                var refVal = GetRef();
                return refVal == null ? RawValue : ((VLVariable<T>)refVal).Value;
            }
        }

        public override Type ValueType => typeof(T);
        public override object ObjectValue => Value;

        public override object ObjectRawValue
        {
            get => RawValue;
            set => RawValue = value == null ? default : (T)value;
        }

        public override VLValueBase Clone() => new VLVariable<T>() { Env = Env, RefVarName = RefVarName };
    }

    [BytesPackGenCustomCode]
    public class VLValue<T> : VLValueBase
    {
        [BytesPackGenCustomCode(WriteCode = "${Gen}", ReadCode = "${Gen}")]
        public T RawValue;

        public T Value
        {
            get
            {
                var refVal = GetRef();
                return refVal == null ? RawValue : ((VLValue<T>)refVal).Value;
            }
        }

        public override Type ValueType => typeof(T);
        public override object ObjectValue => Value;

        public override object ObjectRawValue
        {
            get => RawValue;
            set => RawValue = value == null ? default : (T)value;
        }

        public override VLValueBase Clone() => new VLValue<T>() { Env = Env, RefVarName = RefVarName, RawValue = RawValue };
        public static implicit operator T(VLValue<T> v) => v.Value;
    }

#if UNITY_PROJ
    /// <summary>
    /// 
    /// </summary>
    [BytesPackGenCustomCode]
    public class VLValueUnity<T> : VLValueBase where T : class
    {
        [BytesPackGenCustomCode(WriteCode = "${Gen}", ReadCode = "${Gen}")]
        public int RefIndex;

        public T RawValue
        {
            get => RefIndex > 0 ? (T)(Env.UnityObjectStorer[RefIndex - 1] as object) : null;
            set
            {
                if (RefIndex > 0)
                {
                    if (value == null)
                    {
                        Env.UnityObjectStorer.Free(RefIndex - 1);
                        RefIndex = 0;
                    }
                    else
                    {
                        Env.UnityObjectStorer[RefIndex - 1] = (value as UnityEngine.Object);
                    }
                }
                else if (value != null)
                {
                    RefIndex = Env.UnityObjectStorer.Alloc((value as UnityEngine.Object)) + 1;
                }
            }
        }
        public T Value
        {
            get
            {
                var refVal = GetRef();
                return refVal != null ? ((VLValueUnity<T>)refVal).Value : RawValue;
            }
        }

        public override object ObjectValue => Value;

        public override object ObjectRawValue
        {
            get => RawValue;
            set => RawValue = value as T;
        }

        public override Type ValueType => typeof(T);
        public override VLValueBase Clone() => new VLValueUnity<T>() { RefIndex = RefIndex, Env = Env, RefVarName = RefVarName };
        public static implicit operator T(VLValueUnity<T> v) => v.RawValue;
    }
#endif
}