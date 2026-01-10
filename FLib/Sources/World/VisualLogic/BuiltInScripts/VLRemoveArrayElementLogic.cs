// //==================={By Qcbf|qcbf@qq.com|5/30/2021 4:57:40 PM}===================
//
// using FLib;
// using System;
// using System.Collections.Generic;
//
// namespace FLib.Worlds.BuiltInScripts
// {
//     [BytesPackGen, Comment("移除数组值")]
//     public partial class VLRemoveArrayElementLogic : VLBaseLogicScript
//     {
//         [BytesPackGenField, VLFieldComment("数组", EVLValueType.Array, IsDisableFixedValue = true)]
//         public VLVariable Arr;
//
//         [BytesPackGenField, VLFieldComment("索引", EVLValueType.Number, Tooltip = "-1表示移除结尾")]
//         public VLVariable Index;
//
//         public override void Handle()
//         {
//             var arr = Arr.Get(Env).AsArray();
//             var index = (int)Index.Get(Env);
//             if (index == -1)
//             {
//                 index = arr.Length - 1;
//             }
//             ArrayFLibUtility.RemoveAt(ref arr, index);
//
//             var result = VLValue.Create(EVLValueType.Array);
//             result.RefVal = arr;
//             Arr.Set(Env, result);
//         }
//     }
// }
