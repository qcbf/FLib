//=================================================={By Qcbf|qcbf@qq.com|11/15/2024 6:22:12 PM}==================================================

using FLib;
using System;
using System.Collections.Generic;


namespace FLib.Worlds
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class WorldSyncPermissionAttribute : Attribute
    {
        public readonly byte Permission;
        public WorldSyncPermissionAttribute(byte permission)
        {
            Permission = permission;
        }

        public WorldSyncPermissionAttribute(bool isHigh) : this((byte)(isHigh ? 100 : 50))
        {
        }
    }
}
