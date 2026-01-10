// //==================={By Qcbf|qcbf@qq.com|5/30/2021 4:57:40 PM}===================
//
// using FLib;
// using System;
// using System.Collections.Generic;
//
// namespace FLib.Worlds.BuiltInScripts
// {
//     [BytesPackGen, Comment("获取字典值")]
//     public partial class VLGetDictElementValueLogic : VLBaseLogicScript
//     {
//         [BytesPackGenField, VLFieldComment("字典", EVLValueType.Dict, IsDisableFixedValue = true)]
//         public VLVariable Dict;
//
//         [BytesPackGenField, VLFieldComment("名称", EVLValueType.String)]
//         public VLVariable Key;
//
//         [BytesPackGenField, VLFieldComment("结果", EVLValueType.None, IsDisableFixedValue = true)]
//         public VLVariable Result;
//
//         public override void Handle()
//         {
//             if (Dict.DictTryGet(Env, Key.Get(Env), out var v))
//             {
//                 Result.Set(Env, v);
//             }
//         }
//     }
// }
