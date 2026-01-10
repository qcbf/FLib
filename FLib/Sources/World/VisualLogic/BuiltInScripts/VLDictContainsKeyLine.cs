// //==================={By Qcbf|qcbf@qq.com|5/30/2021 5:10:34 PM}===================
//
// using FLib;
// using System;
// using System.Collections.Generic;
//
// namespace FLib.Worlds.BuiltInScripts
// {
//     [BytesPackGen, CommentAttribute("字典是否存在key")]
//     public partial class VLDictContainsKeyLine : VLBaseLineScript
//     {
//         [BytesPackGenField, VLFieldCommentAttribute("字典", EVLValueType.Dict, IsDisableFixedValue = true)]
//         public VLVariable Dict;
//
//         [BytesPackGenField, VLFieldCommentAttribute("名称", EVLValueType.String)]
//         public VLVariable Key;
//
//         public override bool Handle()
//         {
//             return Dict.Get(Env).DictContainsKey(Key.Get(Env));
//         }
//     }
// }
