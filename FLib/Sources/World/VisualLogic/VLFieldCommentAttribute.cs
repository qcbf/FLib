// //==================={By Qcbf|qcbf@qq.com|5/30/2021 5:30:04 PM}===================
//
// using FLib;
// using System;
// using System.Collections.Generic;
// using System.Diagnostics;
//
// namespace FLib.Worlds
// {
//     [Conditional("DEBUG"), AttributeUsage(AttributeTargets.Field)]
//     public class VLFieldCommentAttribute : Attribute
//     {
//         /// <summary>
//         /// 名称/注释/标签
//         /// </summary>
//         public string Label;
//         /// <summary>
//         /// 提示,备注
//         /// </summary>
//         public string Tooltip;
//         /// <summary>
//         /// 变量类型选择过滤
//         /// </summary>
//         public EVLValueType AllowTypes;
//         /// <summary>
//         /// 
//         /// </summary>
//         public EVLValueType Type;
//         /// <summary>
//         /// 是否禁用固定值
//         /// </summary>
//         public bool IsDisableFixedValue;
//         /// <summary>
//         /// 默认值
//         /// </summary>
//         public object DefaultValue;
//
//         public VLFieldCommentAttribute(string label, EVLValueType type)
//         {
//             Label = label;
//             Type = type;
//         }
//     }
// }
