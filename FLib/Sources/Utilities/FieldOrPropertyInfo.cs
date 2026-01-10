//-----------------------------------------------------------------------
//| by:Qcbf                                                             |
//-----------------------------------------------------------------------

using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace FLib
{
    public struct FieldOrPropertyInfo
    {
        /// <summary>
        /// 
        /// </summary>
		public FieldInfo Field;
        /// <summary>
        /// 
        /// </summary>
		public PropertyInfo Property;

        /// <summary>
        /// 
        /// </summary>
		public readonly bool IsEmpty => Field == null && Property == null;

        /// <summary>
        /// 
        /// </summary>
		public readonly bool IsField => Field != null;

        /// <summary>
        /// 
        /// </summary>
		public readonly bool IsProperty => Property != null;

        /// <summary>
        /// 
        /// </summary>
		public readonly Type Type => Field?.FieldType ?? Property.PropertyType;

        /// <summary>
        /// 
        /// </summary>
        public readonly Type DeclaringType => Field?.DeclaringType ?? Property.DeclaringType;

        /// <summary>
        /// 
        /// </summary>
		public readonly string Name => Field?.Name ?? Property.Name;

        /// <summary>
        /// 
        /// </summary>
        public readonly bool IsStatic => Field?.IsStatic ?? Property.GetMethod?.IsStatic ?? Property.SetMethod!.IsStatic;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="field"></param>
        public FieldOrPropertyInfo(FieldInfo field)
        {
            Field = field;
            Property = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prop"></param>
        public FieldOrPropertyInfo(PropertyInfo prop)
        {
            Field = null;
            Property = prop;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="srcType"></param>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <param name="throwException"></param>
        public FieldOrPropertyInfo(Type srcType, string name, BindingFlags flags, bool throwException = true)
        {
            Field = srcType.GetField(name, flags);
            if (Field == null)
            {
                Property = srcType.GetProperty(name, flags);
                if (Property == null && throwException)
                {
                    throw new Exception("not found field or property : " + srcType.Name + "." + name);
                }
            }
            else
            {
                Property = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inst"></param>
        /// <param name="v"></param>
		public readonly void SetValue(object inst, object v)
        {
            if (Field != null)
            {
                Field.SetValue(inst, v);
            }
            else
            {
                Property.SetValue(inst, v, null);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inst"></param>
        /// <returns></returns>
		public T GetValue<T>(object inst) => (T)GetValue(inst);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inst"></param>
        /// <returns></returns>
		public readonly object GetValue(object inst)
        {
            if (Field != null)
            {
                return Field.GetValue(inst);
            }
            else
            {
                return Property.GetValue(inst, null);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="isInherit"></param>
        /// <returns></returns>
		public bool IsDefineAttribute<T>(bool isInherit = true) => IsDefineAttribute(typeof(T), isInherit);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <param name="isInherit"></param>
        /// <returns></returns>
        public readonly bool IsDefineAttribute(Type t, bool isInherit = true)
        {
            if (Field != null)
            {
                return Field.IsDefined(t, isInherit);
            }
            else
            {
                return Property.IsDefined(t, isInherit);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public readonly T GetCustomAttribute<T>(bool inherit) where T : Attribute
        {
            if (Field != null)
            {
                return Field.GetCustomAttribute<T>(inherit);
            }
            else
            {
                return Property.GetCustomAttribute<T>(inherit);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public readonly Attribute GetCustomAttribute(Type t, bool inherit)
        {
            if (Field != null)
            {
                return Field.GetCustomAttribute(t, inherit);
            }
            else
            {
                return Property.GetCustomAttribute(t, inherit);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public readonly override string ToString() => Type + " " + Name;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static FieldOrPropertyInfo[] GetFieldOrPropertInfos(Type t, BindingFlags flags)
        {
            return (from prop in t.GetProperties(flags) select new FieldOrPropertyInfo(prop))
                .Concat(from field in t.GetFields(flags) select new FieldOrPropertyInfo(field))
                .ToArray();
        }

    }
}
