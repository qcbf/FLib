//==================={By Qcbf|qcbf@qq.com|10/8/2022 4:41:37 PM}===================

using System;
using System.Collections.Generic;
using FLib;

namespace FLib
{
    public interface IBytesPackable
    {
        public void Z_BytesPackWrite(ref BytesPack.KeyHelper key, ref BytesWriter writer);
        public void Z_BytesPackRead(int key, ref BytesReader reader);
    }
}
