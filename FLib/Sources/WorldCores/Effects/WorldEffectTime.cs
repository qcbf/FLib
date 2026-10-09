// ==================== qcbf@qq.com | 2026-03-13 ====================

using System.Runtime.CompilerServices;
using FLib.WorldCores.Entities;

namespace FLib.WorldCores.Effects
{
    /// <summary>
    /// 
    /// </summary>
    [WorldComponentOption(1)]
    public struct WorldEffectTime : IWorldUpdate
    {
        public readonly WorldEffectBase Effect;
        public FNum EndTime;

        public readonly FNum Remaining => EndTime - Effect.World.Time;

        public WorldEffectTime(WorldEffectBase effect)
        {
            Effect = effect;
            EndTime = Effect.World.Time + Effect.Duration;
        }

        public void OnComponentUpdate(WorldCore world, WorldEntityId entityId)
        {
            if (world.Time >= EndTime)
            {
                ResetTime(world.Time);
                Effect.RemoveSelf(Effect.AddOption == EWorldEffectAddOption.AddStackAndTimeoutAllStack ? ushort.MaxValue : (ushort)1);
            }
        }

        public void ResetTime(in FNum time)
        {
            EndTime = time + Effect.Duration;
        }
    }
}