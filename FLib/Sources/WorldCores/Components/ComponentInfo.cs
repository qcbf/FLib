// ==================== qcbf@qq.com | 2026-01-10 ====================

using System;

namespace FLib.WorldCores
{
    public readonly struct ComponentInfo
    {
        public readonly Type Type;
        public readonly IComponentAwake ComponentAwake;
        public readonly IComponentDestroy ComponentDestroy;
        public readonly IComponentEnable ComponentEnable;
        public readonly IComponentDisable ComponentDisable;
        public readonly IComponentUpdate ComponentUpdate;

        public ComponentInfo(Type type)
        {
            Type = type;
            
            ComponentAwake = null;
            ComponentDestroy = null;
            ComponentEnable = null;
            ComponentDisable = null;
            ComponentUpdate = null;
        }
    }
}