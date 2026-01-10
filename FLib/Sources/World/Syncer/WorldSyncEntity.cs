//=================================================={By Qcbf|qcbf@qq.com|11/19/2024 6:18:48 PM}==================================================

using FLib;
using System;
using System.Collections.Generic;

namespace FLib.Worlds
{
    public readonly struct WorldSyncEntity : IBytesSerializable
    {
        public readonly WorldEntity Entity;
        public readonly uint NetId;

        public void Z_BytesRead(ref BytesReader reader)
        {

        }

        public void Z_BytesWrite(ref BytesWriter writer)
        {

        }
    }
}
