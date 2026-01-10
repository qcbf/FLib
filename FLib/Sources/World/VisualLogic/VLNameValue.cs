// //==================={By Qcbf|qcbf@qq.com|8/19/2021 6:32:15 PM}===================
//
// using FLib;
// using System;
// using System.Collections.Generic;
//
// namespace FLib.Worlds
// {
//     public struct VLNameValue : IBytesSerializeable
//     {
//         public string Name;
//         public VLValue Value;
//
//         public readonly void Z_BytesWrite(ref BytesWriter writer)
//         {
//             writer.Push(Name);
//             Value.Z_BytesWrite(ref writer);
//         }
//
//         public void Z_BytesRead(ref BytesReader reader)
//         {
//             Name = reader.ReadString();
//             Value.Z_BytesRead(ref reader);
//         }
//     }
// }
