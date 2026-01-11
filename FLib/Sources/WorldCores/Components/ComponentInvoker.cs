// ==================== qcbf@qq.com | 2026-01-11 ====================

using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace FLib.WorldCores
{
    public static class ComponentInvoker
    {
        public delegate void Delegate(ref byte ptr, WorldCore world, Entity entity);

        internal static void ComponentAwake<T>(ref byte ptr, WorldCore world, Entity entity) where T : IComponentAwake
        {
            try
            {
                Unsafe.As<byte, T>(ref ptr).ComponentAwake(world, entity);
            }
            catch (Exception e)
            {
                Log.Error?.Write($"{entity} {e}", nameof(ComponentInvoker));
            }
        }

        internal static void ComponentStart<T>(ref byte ptr, WorldCore world, Entity entity) where T : IComponentStart
        {
            try
            {
                Unsafe.As<byte, T>(ref ptr).ComponentStart(world, entity);
            }
            catch (Exception e)
            {
                Log.Error?.Write($"{entity} {e}", nameof(ComponentInvoker));
            }
        }

        internal static void ComponentDestroy<T>(ref byte ptr, WorldCore world, Entity entity) where T : IComponentDestroy
        {
            try
            {
                Unsafe.As<byte, T>(ref ptr).ComponentDestroy(world, entity);
            }
            catch (Exception e)
            {
                Log.Error?.Write($"{entity} {e}", nameof(ComponentInvoker));
            }
        }

        internal static void ComponentEnable<T>(ref byte ptr, WorldCore world, Entity entity) where T : IComponentEnable
        {
            try
            {
                Unsafe.As<byte, T>(ref ptr).ComponentEnable(world, entity);
            }
            catch (Exception e)
            {
                Log.Error?.Write($"{entity} {e}", nameof(ComponentInvoker));
            }
        }

        internal static void ComponentDisable<T>(ref byte ptr, WorldCore world, Entity entity) where T : IComponentDisable
        {
            try
            {
                Unsafe.As<byte, T>(ref ptr).ComponentDisable(world, entity);
            }
            catch (Exception e)
            {
                Log.Error?.Write($"{entity} {e}", nameof(ComponentInvoker));
            }
        }

        internal static void ComponentUpdate<T>(ref byte ptr, WorldCore world, Entity entity) where T : IComponentUpdate
        {
            Unsafe.As<byte, T>(ref ptr).ComponentUpdate(world, entity);
        }

        /// <summary>
        /// 
        /// </summary>
        internal static Delegate Make(Type type, string name)
        {
            var mi = typeof(ComponentInvoker).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(type);
            return mi.CreateDelegate<Delegate>();
        }
    }
}