// //==================={By Qcbf|qcbf@qq.com|5/30/2021 4:57:40 PM}===================
//
// using FLib;
// using System;
// using System.Collections.Generic;
//
// namespace FLib.Worlds.BuiltInScripts
// {
//     [BytesPackGen, Comment("获取数组值")]
//     public partial class VLGetArrayElementLogic : VLBaseLogicScript
//     {
//         [BytesPackGenField, VLFieldComment("数组", EVLValueType.Array, IsDisableFixedValue = true)]
//         public VLVariable Arr;
//
//         [BytesPackGenField, VLFieldComment("索引", EVLValueType.Number)]
//         public VLVariable Index;
//
//         [BytesPackGenField, VLFieldComment("结果", EVLValueType.String, AllowTypes = EVLValueType.All, IsDisableFixedValue = true)]
//         public VLVariable Result;
//
//         public override void Handle()
//         {
//             Result.Set(Env, Arr.Get(Env, (int)Index.Get(Env)));
//         }
//     }
// }
