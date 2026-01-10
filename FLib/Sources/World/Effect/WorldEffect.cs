// =================================================={By Qcbf|qcbf@qq.com|2024-11-05}==================================================

using System;
using System.Linq;

namespace FLib.Worlds
{
    // todoNext：改为effect就是实例，而不是现在这样静态配置，通过额外添加组件方式当做效果实例。因为实际情况是大部分时候效果是基于事件触发，需要class，中毒这些自身update的效果在begin时自己添加组件来实现逻辑增强性能，所有效果不需要update，只有begin、end
    [BytesPackGenHoldKey(2)]
    public abstract class WorldEffect : IBytesPackable 
    {
        [Comment("持续时间", "0无限时长")]
        public FNum Duration;

        [Comment("最大层数")]
        public int MaxStackCount = 1;

        [Comment("标记")]
        public WorldFlags Flags;

        [Comment("重复添加方式")]
        public EWorldEffectAddOption AddOption = EWorldEffectAddOption.ResetTime;

        public virtual bool CallEndOnDestroyed => false;
        public virtual bool IsEntityComponent => false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctx">如果为null代表当前没有这个id的效果</param>
        /// <param name="effectSystem">效果系统</param>
        /// <param name="createBy">来自哪儿</param>
        /// <param name="id">id</param>
        /// <param name="addCount">添加次数</param>
        public virtual bool Check(WorldEffectSystem effectSystem, WorldEffectContext ctx, in WorldEntity createBy, uint id, int addCount) => true;

        /// <summary>
        /// 
        /// </summary>
        public virtual void OnStackCountChange(WorldEffectContext context, int addCount)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual WorldFlags GetFlags(WorldEffectContext context) => Flags;

        /// <summary>
        /// 
        /// </summary>
        public virtual int GetMaxStackCount(WorldEffectContext context) => MaxStackCount;

        /// <summary>
        /// 
        /// </summary>
        public virtual WorldComponentHandle OnBegin(WorldEffectContext context)
        {
            return default;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void OnEnd(WorldEffectContext context)
        {
        }

        /// <summary>
        /// 添加效果实例组件 
        /// </summary>
        public WorldComponentHandle AddInstanceComponent<T>(WorldEffectContext context) where T : IWorldEffectInstanceComponentable, new()
        {
            var group = context.World.ComponentMgr.GetGroup<T>();
            var index = group.Add(context.Entity, false);
            group.ComponentMetas[index].Component.EffectContext = context;
            group.AddingFinish(index);
            return new WorldComponentHandle(WorldComponentGroup<T>.TypeId, index);
        }

        #region serialization
        public virtual void Z_BytesPackWrite(ref BytesPack.KeyHelper key, ref BytesWriter writer)
        {
            key.Push(ref writer, 1);
            writer.Push(Duration);
            writer.PushVInt(Flags);
            writer.PushVInt(MaxStackCount);
            writer.Push((byte)AddOption);
        }

        public virtual void Z_BytesPackRead(int key, ref BytesReader reader)
        {
            if (key == 1)
            {
                Duration = reader.Read<FNum>();
                Flags = reader.ReadVInt();
                MaxStackCount = (short)reader.ReadVInt();
                AddOption = (EWorldEffectAddOption)reader.Read<byte>();
            }
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class WorldEffect<T> : WorldEffect where T : IWorldEffectInstanceComponentable, new()
    {
        public override WorldComponentHandle OnBegin(WorldEffectContext context) => AddInstanceComponent<T>(context);
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IWorldEffectInstanceComponentable : IWorldComponentable
    {
        WorldEffectContext EffectContext { set; }
    }
}
