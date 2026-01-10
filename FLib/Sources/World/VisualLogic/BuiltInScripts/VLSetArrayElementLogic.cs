// //==================={By Qcbf|qcbf@qq.com|5/30/2021 4:57:40 PM}===================
//
// using FLib;
// using System;
// using System.Collections.Generic;
//
// namespace FLib.Worlds.BuiltInScripts
// {
//     [BytesPackGen, Comment("设置数组值")]
//     public partial class VLSetArrayElementLogic : VLBaseLogicScript
//     {
//         [BytesPackGenField, VLFieldComment("数组", EVLValueType.Array, IsDisableFixedValue = true)]
//         public VLVariable Arr;
//
//         [BytesPackGenField, VLFieldComment("索引", EVLValueType.Number)]
//         public VLVariable Index;
//
//         [BytesPackGenField, VLFieldComment("来源", EVLValueType.String, AllowTypes = EVLValueType.All, IsDisableFixedValue = true)]
//         public VLVariable Src;
//
//         public override void Handle()
//         {
//             Arr.Get(Env).AsArray()[(int)Index.Get(Env)].Set(Src.Get(Env));
//         }
//     }
// }
