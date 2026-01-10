// //==================={By Qcbf|qcbf@qq.com|5/30/2021 4:57:40 PM}===================
//
// using FLib;
// using System;
// using System.Collections.Generic;
//
// namespace FLib.Worlds.BuiltInScripts
// {
//     [BytesPackGen, Comment("添加数组值")]
//     public partial class VLAddArrayElementLogic : VLBaseLogicScript
//     {
//         [BytesPackGenField, VLFieldComment("数组", EVLValueType.Array, IsDisableFixedValue = true)]
//         public VLValueBase Arr;
//
//         [BytesPackGenField, VLFieldComment("索引", EVLValueType.Number, Tooltip = "-1表示结尾添加")]
//         public VLVariable Index;
//
//         [BytesPackGenField, VLFieldComment("来源", EVLValueType.String, AllowTypes = EVLValueType.All, IsDisableFixedValue = true)]
//         public VLVariable Src;
//
//         public override void Handle()
//         {
//             var arr = Arr.Get(Env).AsArray();
//             var index = (int)Index.Get(Env);
//             if (index == -1)
//             {
//                 ArrayFLibUtility.Add(ref arr, Src.Get(Env));
//             }
//             else
//             {
//                 ArrayFLibUtility.Insert(ref arr, Src.Get(Env), index);
//             }
//             var result = VLValue_Obsolete.Create(EVLValueType.Array);
//             result.RefVal = arr;
//             Arr.Set(Env, result);
//         }
//     }
// }
