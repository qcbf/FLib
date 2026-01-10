// =================================================={By Qcbf|qcbf@qq.com|2024-11-05}==================================================

using System.Net;

namespace FLib.Worlds
{
    public class WorldEffectContext : IObjectPoolDeactivatable
    {
        public uint Id;
        public WorldEffectSystem EffectSys;
        public WorldEffect Effect;
        public WorldComponentHandle Component;
        public WorldComponentHandleEx<Updater> UpdaterComponent;

        public WorldEntity CreateBy;
        public int StackCount;
        public int LinkedId;

        public WorldEffectContext Next;
        public WorldEffectContext Prev;

        public FNum Duration => UpdaterComponent.RO().Duration;
        public FNum StartTime => UpdaterComponent.RO().StartTime;
        public WorldBase World => EffectSys.World;
        public WorldEntity Entity => EffectSys.Entity;
        public bool IsEmpty => Id == 0;
        public override string ToString() => $"{Effect.GetType().Name}{(StackCount > 1 ? $"*{StackCount}" : string.Empty)}";

        public ref readonly T RO<T>() where T : IWorldComponentable, new() => ref Component.RO<T>(World);
        public ref T RW<T>() where T : IWorldComponentable, new() => ref Component.RW<T>(World);
        public void Destroy() => EffectSys.Remove(this, StackCount);

        /// <summary>
        /// 
        /// </summary>
        [WorldComponentOrder(Order = short.MaxValue)]
        public struct Updater : IWorldUpdateComponentable
        {
            public WorldEffectContext Context;
            public FNum StartTime;
            public FNum Duration;
            public WorldComponentContext SelfContext { get; set; }

            public Updater(WorldEffectContext context) : this()
            {
                Context = context;
                StartTime = SelfContext.World.Time;
                Duration = context.Effect.Duration;
            }

            public void ComponentUpdate()
            {
                if (Duration <= 0)
                    return;
                var time = SelfContext.World.Time;
                if (time - StartTime >= Duration)
                {
                    StartTime = time;
                    if (Context.Effect.AddOption == EWorldEffectAddOption.AddStackWithResetTime)
                        Context.EffectSys.Remove(Context, Context.StackCount);
                    else
                        Context.EffectSys.Remove(Context);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Begin()
        {
            Component = Effect.OnBegin(this);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void End()
        {
            if (!UpdaterComponent.OnlyRemoveThisComponent() || (!Component.IsEmpty && !(Effect.IsEntityComponent ? Component.Destroy(Entity) : Component.OnlyRemoveThisComponent(Entity))))
                Log.Error?.Write($"end effect error {Id}|{GetType().Name}|{Entity}");
        }

        /// <summary>
        ///
        /// </summary>
        public virtual void ObjectPoolDeactivatable()
        {
            Next = Prev = null;
            Id = 0;
            Component = default;
            UpdaterComponent = default;
            EffectSys = null;
            Effect = null;
            StackCount = LinkedId = 0;
        }
    }
}
