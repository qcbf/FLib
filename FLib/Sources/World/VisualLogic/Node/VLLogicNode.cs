//==================={By Qcbf|qcbf@qq.com|5/30/2021 2:59:41 PM}===================

using FLib;
using System;
using System.Collections.Generic;

namespace FLib.Worlds
{
    public class VLLogicNode : VLNode
    {
        public VLBaseLogicScript[] Scripts = Array.Empty<VLBaseLogicScript>();

        public override void Initialize()
        {
            for (var i = 0; i < Scripts.Length; i++)
            {
                if (Scripts[i] != null)
                    Scripts[i].Initialize();
            }
        }

        public override void Handle()
        {
            Log.Verbose?.Write($"execute {Uid}|{Env.Uid}", "VLLogic");
            VLEnvironment.NodeActionCallback?.Invoke(Env, this, VLEnvironment.ENodeActionType.ExecuteLogicNode);
            foreach (var item in Scripts)
            {
                try
                {
                    item.Handle();
                }
                catch (Exception ex)
                {
                    Log.Error?.Write($"LogicNode Handle Exception: {Env.DebugInfo}-> {Uid}-> {item.GetType()}\n{ex}");
                }
            }
            ExecuteNext();
        }

        public override void Z_BytesPackRead(int key, ref BytesReader reader)
        {
            if (key == 5)
            {
                var count = reader.ReadLength();
                Scripts = new VLBaseLogicScript[count];
                for (var i = 0; i < count; i++)
                {
                    var typeName = reader.ReadString();
                    Scripts[i] = (VLBaseLogicScript)TypeAssistant.New(typeName);
                    Scripts[i].Env = Env;
                    Scripts[i].Node = this;
                    BytesPack.Unpack(ref Scripts[i], ref reader);
                }
            }
            else
            {
                base.Z_BytesPackRead(key, ref reader);
            }
        }

        public override void Z_BytesPackWrite(ref BytesPack.KeyHelper key, ref BytesWriter writer)
        {
            base.Z_BytesPackWrite(ref key, ref writer);
            key.Push(ref writer, 5);
            var count = (Scripts?.Length).GetValueOrDefault();
            writer.PushLength(count);
            for (var i = 0; i < count; i++)
            {
                writer.Push(TypeAssistant.GetTypeName(Scripts![i].GetType()));
                BytesPack.Pack(Scripts[i], ref writer);
            }
        }
    }
}
