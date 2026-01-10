//=================================================={By Qcbf|qcbf@qq.com|11/14/2024 3:48:44 PM}==================================================

using FLib;
using System;
using System.Collections.Generic;


namespace FLib.Worlds
{
    public class WorldBehaviorContext : IObjectPoolDeactivatable
    {
        public int Priority;
        public WorldComponentHandle ParamComp;
        public WorldComponentHandle InstanceComp;
        public WorldBehaviorSystem BehaviorSys;
        public WorldBehavior Behavior;
        public uint StartFrame;
        public WorldEntity Entity => BehaviorSys.Entity;
        public WorldBase World => BehaviorSys.World;
        public bool Stop(bool isDoDefault = true) => BehaviorSys.Stop(Behavior, isDoDefault);
        public ref readonly T ParamRO<T>(ELogLevel logLevel = ELogLevel.Fatal) where T : IWorldComponentable, new() => ref ParamComp.RO<T>(Entity, logLevel);
        public ref T ParamRW<T>(ELogLevel logLevel = ELogLevel.Fatal) where T : IWorldComponentable, new() => ref ParamComp.RW<T>(Entity, logLevel);
        public ref readonly T InstanceRO<T>(ELogLevel logLevel = ELogLevel.Fatal) where T : IWorldComponentable, new() => ref InstanceComp.RO<T>(Entity, logLevel);
        public ref T InstanceRW<T>(ELogLevel logLevel = ELogLevel.Fatal) where T : IWorldComponentable, new() => ref InstanceComp.RW<T>(Entity, logLevel);

        public void RemoveInstanceComp()
        {
            var temp = InstanceComp;
            temp.OnlyRemoveThisComponent(Entity);
            if (temp == InstanceComp)
                InstanceComp = default;
        }

        public void ObjectPoolDeactivatable()
        {
            ParamComp = InstanceComp = default;
            BehaviorSys = null;
            Behavior = null;
        }

        public override string ToString() => ToString(false);

        public string ToString(bool isVerbose)
        {
            if (Behavior == null)
                return string.Empty;
            if (isVerbose)
            {
                var strbuf = StringFLibUtility.GetStrBuf();
                strbuf.AppendLine(Behavior.GetType().Name);
                strbuf.AppendLine(ParamComp.Cast(World).ToString(true));
                strbuf.AppendLine(InstanceComp.Cast(World).ToString(true));
                return StringFLibUtility.ReleaseStrBufAndResult(strbuf);
            }
            return $"{Behavior.GetType()}|{ParamComp.Cast(World).ToString(false)}";
        }

        public static implicit operator WorldBehavior(WorldBehaviorContext context) => context.Behavior;
        public static implicit operator WorldBase(WorldBehaviorContext ctx) => ctx.World;
        public static implicit operator WorldEntity(WorldBehaviorContext ctx) => ctx.Entity;
        public static implicit operator WorldBehaviorSystem(WorldBehaviorContext ctx) => ctx.BehaviorSys;
    }
}
