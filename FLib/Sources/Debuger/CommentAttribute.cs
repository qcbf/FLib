//==================={By Qcbf|qcbf@qq.com|5/30/2021 4:06:17 PM}===================

using System;
using System.Diagnostics;
using System.Reflection;

namespace FLib
{
    [Conditional("DEBUG"), AttributeUsage(AttributeTargets.All)]
    public class CommentAttribute : Attribute
    {
        public string Name;
        public string Detail;

        public CommentAttribute(string name, string detail = "")
        {
            Name = name;
            Detail = detail;
        }

        public override string ToString() => ToString(string.Empty);
        public string ToString(string additionText) => string.IsNullOrWhiteSpace(Detail) ? Name + additionText : Name + "\n" + Detail;

        /// <summary>
        /// 
        /// </summary>
        public static string TryGetLabel(Type type, string additionText = "")
        {
            if (type == null)
                return string.Empty;
            var comment = type.GetCustomAttribute<CommentAttribute>(false);
            if (comment == null)
                return type.Name;
            return comment.Name + additionText;
        }

        /// <summary>
        /// 
        /// </summary>
        public static string TryGetLabel(MemberInfo field, string additionText = "")
        {
            if (field == null)
                return string.Empty;
            var comment = field.GetCustomAttribute<CommentAttribute>(false);
            if (comment == null)
                return field.Name;
            return comment.Name + additionText;
        }

        /// <summary>
        /// 
        /// </summary>
        public static string TryGetLabel(Type type, out string detail, string additionText = "", bool appendTypeNameToDetail = true)
        {
            detail = "";
            if (type == null)
                return string.Empty;
            var comment = type.GetCustomAttribute<CommentAttribute>(false);
            if (comment == null)
                return type.Name;
            detail = comment.Detail;
            if (appendTypeNameToDetail)
                detail = $"{type.Name}\n{detail}";
            return comment.Name + additionText;
        }

        /// <summary>
        /// 
        /// </summary>
        public static string TryGetLabel(MemberInfo field, out string detail, string additionText = "")
        {
            detail = "";
            if (field == null)
                return string.Empty;
            var comment = field.GetCustomAttribute<CommentAttribute>(false);
            if (comment == null)
                return field.Name;
            detail = comment.Detail;
            return comment.Name + additionText;
        }
    }
}
