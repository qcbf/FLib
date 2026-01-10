//==================={By Qcbf|qcbf@qq.com|5/30/2021 3:27:04 PM}===================

using FLib;
using System;
using System.Collections.Generic;

namespace FLib.Worlds
{
    public class VLLine : IBytesPackable
    {
        public VLEnvironment Env;

        public uint LeftNodeUid;
        public uint RightNodeUid;
        public Condition[] Conditions;
        public bool IsInvertResult;

        private VLNode mLeftNodeCached;
        private VLNode mRightNodeCached;
        public VLNode LeftNode => mLeftNodeCached ??= Env.GetNode(LeftNodeUid);
        public VLNode RightNode => mRightNodeCached ??= Env.GetNode(RightNodeUid);

        [BytesPackGenAdditionalCode(ReadCode = "if (${FieldName}.Script!=null)${FieldName}.Script.Env=Env;")]
        public struct Condition
        {
            /// <summary>
            /// 0:None, 1:Invert Result, 2:Or Condition
            /// </summary>
            public byte Type;

            public VLBaseLineScript Script;


            public void Z_BytesRead(ref BytesReader reader)
            {
                Type = reader.Read<byte>();
                if (Type != 2)
                {
                    var typeName = reader.ReadString();
                    Script = (VLBaseLineScript)TypeAssistant.New(typeName);
                    BytesPack.Unpack(ref Script, ref reader);
                }
            }
        }


        public void Initialize()
        {
            for (var i = 0; i < Conditions.Length; i++)
            {
                try
                {
                    if (Conditions[i].Type != 2)
                    {
                        Conditions[i].Script.Env = Env;
                        Conditions[i].Script.Line = this;
                        Conditions[i].Script.Initialize();
                    }
                }
                catch (Exception err)
                {
                    Log.Error?.Write($"{i} {Conditions[i].Script} condition error\n{err}");
                }
            }
        }

        public bool CheckCondition()
        {
            var isAllResult = true;
            for (var i = 0; i < Conditions.Length; i++)
            {
                if (Conditions[i].Type == 2)
                {
                    if (isAllResult) break;
                    isAllResult = true;
                }
                else if (isAllResult)
                {
                    try
                    {
                        var result = Conditions[i].Script.Handle();
                        if (Conditions[i].Type == 1) result = !result;
                        if (!result)
                        {
                            isAllResult = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error?.Write($"Line Exception: {Env.DebugInfo}-> {LeftNodeUid}-> {Conditions[i].Script?.GetType()}\n{ex}");
                    }
                }
            }
            if (IsInvertResult) isAllResult = !isAllResult;
            return isAllResult;
        }


        public void Z_BytesPackWrite(ref BytesPack.KeyHelper key, ref BytesWriter writer)
        {
            if (RightNodeUid != 0)
            {
                key.Push(ref writer, 1);
                writer.PushVInt(RightNodeUid);
            }
            if (Conditions != null)
            {
                key.Push(ref writer, 2);
                writer.PushLength(Conditions.Length);
                for (var i = 0; i < Conditions.Length; i++)
                {
                    ref readonly var c = ref Conditions[i];
                    writer.Push(c.Type);
                    if (c.Type != 2 && c.Script != null)
                    {
                        writer.Push(TypeAssistant.GetTypeName(c.Script.GetType()));
                        BytesPack.Pack(c.Script, ref writer);
                    }
                }
            }
            if (IsInvertResult)
            {
                key.Push(ref writer, 3);
                writer.Push(IsInvertResult);
            }
        }

        public void Z_BytesPackRead(int key, ref BytesReader reader)
        {
            switch (key)
            {
                case 1: RightNodeUid = (uint)reader.ReadVInt(); break;
                case 2:
                    var count = reader.ReadLength();
                    Conditions = new Condition[count];
                    for (var i = 0; i < count; i++)
                    {
                        ref var c = ref Conditions[i];
                        c.Type = reader.Read<byte>();
                        if (c.Type != 2)
                        {
                            var typeName = reader.ReadString();
                            c.Script = (VLBaseLineScript)TypeAssistant.New(typeName);
                            Conditions[i].Script.Env = Env;
                            BytesPack.Unpack(ref c.Script, ref reader);
                        }
                    }
                    break;
                case 3: IsInvertResult = reader.Read<bool>(); break;
            }
        }
    }
}
