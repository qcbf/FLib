//==================={By Qcbf|qcbf@qq.com|5/30/2021 2:50:29 PM}===================

using FLib;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FLib.Worlds
{
    public abstract class VLNode : IBytesPackable
    {
        public VLEnvironment Env;
        public uint Uid;

        public VLLine[] Lines = Array.Empty<VLLine>();

        // public VLDynamicFields Variables = new();
        private bool mIsInitialized;
        public virtual bool IsLateInitialization => false;
        public abstract void Initialize();
        public abstract void Handle();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TryInitialize()
        {
            if (!mIsInitialized)
            {
                mIsInitialized = true;
                Initialize();
                foreach (var line in Lines) line.Initialize();
            }
        }

        public virtual void Execute()
        {
            TryInitialize();
            Handle();
        }


        public unsafe int ExecuteNext()
        {
            var successCount = 0;
            var results = stackalloc bool[Lines.Length];
            //for (var i = 0; i < Lines.Length; i++)
            //{
            //    results[i] = Lines[i].CheckCondition();
            //}
            for (var i = 0; i < Lines.Length; i++)
            {
                if (Lines[i].CheckCondition())
                {
                    Lines[i].RightNode.Execute();
                    ++successCount;
                }
            }
            return successCount;
        }

        public virtual void Z_BytesPackWrite(ref BytesPack.KeyHelper key, ref BytesWriter writer)
        {
            key.Push(ref writer, 1);
            writer.PushVInt(Uid);

            key.Push(ref writer, 2);
            var len = (Lines?.Length).GetValueOrDefault();
            writer.PushLength(len);
            for (var i = 0; i < len; i++)
            {
                if (Lines![i] == null)
                {
                    writer.Push(false);
                    continue;
                }

                writer.Push(true);
                BytesPack.Pack(Lines[i], ref writer);
            }

            // key.Push(ref writer, 3);
            // BytesPack.Pack(Variables, ref writer);
        }

        public virtual void Z_BytesPackRead(int key, ref BytesReader reader)
        {
            switch (key)
            {
                case 1: Uid = (uint)reader.ReadVInt(); break;
                case 2:
                    Lines = new VLLine[reader.ReadLength()];
                    for (var i = 0; i < Lines.Length; i++)
                    {
                        if (!reader.Read<bool>()) continue;
                        Lines[i] = new VLLine { Env = Env, LeftNodeUid = Uid };
                        BytesPack.Unpack(ref Lines[i], ref reader);
                    }
                    break;
                // case 3: BytesPack.Unpack(ref Variables, ref reader); break;
            }
        }
    }
}
