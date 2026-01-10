// //==================={By Qcbf|qcbf@qq.com|4/24/2022 5:34:52 PM}===================
//
// using FLib;
// using System;
// using System.Collections.Generic;
//
// namespace FLib.Worlds
// {
//     public class VLTemplateNode : VLNode
//     {
//         public uint RefSid;
//         public SlimDictionary<string, VLVariable> TemplateVariables;
//
//         public override bool IsLateInitialization => true;
//
//         public VLEnvironment TemplateVLEnv
//         {
//             get;
//             private set;
//         }
//
//
//         public override void Initialize()
//         {
//             try
//             {
//                 var vl = new VLEnvironment();
//                 BytesPack.Pack()
//                 DictSerializer.Deserialize(FBytes.CreateFromArrayPointer(Config<VLTemplateData>.Get(RefSid).Data), ref vl);
//                 TemplateVLEnv = vl;
//                 TemplateVLEnv.RuntimeData = Env.RuntimeData;
//                 TemplateVLEnv.DebugInfo = Env.DebugInfo + "Template" + Uid;
//             }
//             catch (Exception ex)
//             {
//                 throw new Exception($"create visual logic error {Env.DebugInfo + nameof(VLTemplateNode) + Uid}\n{ex}");
//             }
//             if (Variables != null)
//             {
//                 var iterator = TemplateVariables.GetIterator();
//                 var createFallback = false;
//                 while (iterator.Next())
//                 {
//                     if (iterator.Value.IsRefVar)
//                     {
//                         if (!TemplateVLEnv.Variables.Values.Remove(iterator.Key))
//                         {
//                             Log.Error?.Write($"{TemplateVLEnv.DebugInfo} not found key: {iterator.Key}");
//                         }
//                         else
//                         {
//                             createFallback = true;
//                         }
//                     }
//                     else
//                     {
//                         TemplateVLEnv.Variables.Set(iterator.Key, iterator.Value.Value);
//                     }
//                 }
//                 if (createFallback)
//                 {
//                     TemplateVLEnv.Variables.OnFallback = key => ref Env.Variables[TemplateVariables[key].RefName];
//                 }
//             }
//         }
//
//         public override void Handle()
//         {
//             TemplateVLEnv.Startup();
//             VLEnvironment.NodeActionCallback?.Invoke(Env, this, VLEnvironment.ENodeActionType.ExecuteTemplateNode);
//             ExecuteNext();
//         }
//
//
//         public override void SerializeFields(ref DictSerializer serializer)
//         {
//             base.SerializeFields(ref serializer);
//             serializer.PushUInt(nameof(RefSid), RefSid);
//             if (TemplateVariables?.Count > 0)
//             {
//                 var bs = new ByteStream();
//                 bs.PushObject(TemplateVariables);
//                 serializer.PushBytes(nameof(TemplateVariables), bs.HSpan.UnsafeAsArray());
//             }
//
//         }
//
//         public override void DeserializeField(string key, ref FBytes value, ref DictSerializer serializer)
//         {
//             switch (key)
//             {
//                 case nameof(RefSid):
//                     RefSid = value.ReadUInt();
//                     break;
//                 case nameof(TemplateVariables):
//                     var bs = new ByteStream(value.ReadBytes());
//                     bs.ReadObject(ref TemplateVariables);
//                     break;
//                 default: base.DeserializeField(key, ref value, ref serializer); break;
//             }
//         }
//
//
//     }
// }
