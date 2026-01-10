//=================================================={By Qcbf|qcbf@qq.com|11/19/2024 6:36:17 PM}==================================================

using FLib;
using System;
using System.Collections.Generic;

namespace FLib
{
    public interface IBytesSerializable
    {
        public void Z_BytesWrite(ref BytesWriter writer);
        public void Z_BytesRead(ref BytesReader reader);
    }
}
