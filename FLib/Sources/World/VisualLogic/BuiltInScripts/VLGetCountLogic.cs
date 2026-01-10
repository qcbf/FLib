// //==================={By Qcbf|qcbf@qq.com|5/30/2021 5:18:11 PM}===================
//
// using FLib;
// using System;
// using System.Collections;
// using System.Collections.Generic;
//
// namespace FLib.Worlds.BuiltInScripts
// {
//     [BytesPackGen, Comment("获取数组/字典长度")]
//     public partial class VLGetCountLogic : VLBaseLogicScript
//     {
//         [BytesPackGenField, VLFieldComment("目标", EVLValueType.String, AllowTypes = EVLValueType.All, IsDisableFixedValue = true)]
//         public VLVariable Target;
//
//         [BytesPackGenField, VLFieldComment("结果", EVLValueType.Number, IsDisableFixedValue = true)]
//         public VLVariable Result;
//
//         public override void Handle()
//         {
//             Result.Set(Env, Target.Get(Env).Count);
//         }
//     }
// }
