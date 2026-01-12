// ==================== qcbf@qq.com | 2026-01-10 ====================

using System;
using System.Diagnostics;
using System.Reflection;

namespace FLib.WorldCores
{
    public readonly struct ComponentInfo
    {
        public readonly ComponentMeta Meta { get; }
        public readonly Type Type;
        public readonly ComponentInvoker.Delegate ComponentAwake;
        public readonly ComponentInvoker.Delegate ComponentStart;
        public readonly ComponentInvoker.Delegate ComponentDestroy;
        public readonly ComponentInvoker.Delegate ComponentUpdate;

        // public readonly ComponentInvoker.Delegate ComponentEnable; // working
        // public readonly ComponentInvoker.Delegate ComponentDisable;

        public ComponentInfo(ComponentMeta meta, Type type)
        {
            Type = type;
            Meta = meta;
            ComponentAwake = typeof(IComponentAwake).IsAssignableFrom(type) ? ComponentInvoker.Make(type, nameof(IComponentAwake.ComponentAwake)) : null;
            ComponentStart = typeof(IComponentStart).IsAssignableFrom(type) ? ComponentInvoker.Make(type, nameof(IComponentStart.ComponentStart)) : null;
            ComponentDestroy = typeof(IComponentDestroy).IsAssignableFrom(type) ? ComponentInvoker.Make(type, nameof(IComponentDestroy.ComponentDestroy)) : null;
            ComponentUpdate = typeof(IComponentUpdate).IsAssignableFrom(type) ? ComponentInvoker.Make(type, nameof(IComponentUpdate.ComponentUpdate)) : null;
            // ComponentEnable = typeof(IComponentEnable).IsAssignableFrom(type) ? ComponentInvoker.Make(type, nameof(IComponentEnable.ComponentEnable)) : null;
            // ComponentDisable = typeof(IComponentDisable).IsAssignableFrom(type) ? ComponentInvoker.Make(type, nameof(IComponentDisable.ComponentDisable)) : null;
        }
    }
}