//==================={By Qcbf|qcbf@qq.com|7/10/2021 4:38:21 PM}===================

using FLib;
using System;
using System.Collections.Generic;

namespace FLib.Worlds
{
    public abstract class VLBaseScript : IBytesPackable
    {
        public VLEnvironment Env;

        public object RuntimeData => Env.RuntimeData;
        public abstract string DebugInfo { get; }
        public abstract void Initialize();
        public virtual void Z_BytesPackWrite(ref BytesPack.KeyHelper key, ref BytesWriter writer) { }
        public virtual void Z_BytesPackRead(int key, ref BytesReader reader) { }
    }
}
