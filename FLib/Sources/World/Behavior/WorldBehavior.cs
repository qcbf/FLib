// =================================================={By Qcbf|qcbf@qq.com|2024-10-31}==================================================

using System;
using System.Runtime.CompilerServices;
using FLib.Worlds;

namespace FLib.Worlds
{
    /// <summary>
    ///
    /// </summary>
    public abstract class WorldBehavior
    {
        public int TypeId;
        public virtual uint Mask => 0;
        public virtual bool IsReAddInstanceComponent => false;
        public virtual int GetPriority(WorldEntity entity, in WorldComponentHandleEx paramComp) => 0;
        public virtual bool CheckDo(WorldEntity entity, WorldBehaviorContext runningContext, in WorldComponentHandleEx paramComp) => runningContext == null || IsReAddInstanceComponent;
        public virtual bool CheckPriority(WorldEntity entity, WorldBehaviorContext runningContext, int targetPriority, WorldBehavior targetBehavior) => targetPriority >= runningContext.Priority;
        public virtual bool IsFriend(WorldEntity entity, WorldBehaviorContext runningContext, WorldBehavior targetBehavior, in WorldComponentHandleEx paramComp) => false;

        public virtual void OnBegin(WorldBehaviorContext context, bool isFirst, in WorldComponentHandleEx oldParamComp, EWorldBehaviorDoResult result)
        {
        }

        public virtual void OnEnd(WorldBehaviorContext context)
        {
        }
    }

    /// <summary>
    ///
    /// </summary>
    public abstract class WorldBehavior<TInstance> : WorldBehavior where TInstance : IWorldBehaviorInstanceComponentable, new()
    {
        public override void OnBegin(WorldBehaviorContext context, bool isFirst, in WorldComponentHandleEx oldParamComp, EWorldBehaviorDoResult result)
        {
#if DEBUG
            if (this is IWorldBehaviorParamTypeGettable paramable && paramable.ParamTypeId != 0 && paramable.ParamTypeId != context.ParamComp.TypeId)
                throw new Exception($"not found param {(((IWorldBehaviorParamTypeGettable)this).ParamTypeId > 0 ? WorldComponentManager.GetTypeName(((IWorldBehaviorParamTypeGettable)this).ParamTypeId) : "")} but receive:{context.Behavior.GetType()}");
#endif
            if (isFirst || context.InstanceComp.IsEmpty)
            {
                AddInstanceComp(context);
            }
            else if (IsReAddInstanceComponent)
            {
                context.RemoveInstanceComp();
                AddInstanceComp(context);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected static void AddInstanceComp(WorldBehaviorContext context, bool withFinish = true)
        {
            var group = context.World.ComponentMgr.GetGroup<TInstance>();
            var compIndex = group.Add(context, new TInstance { BehaviorContext = context }, false);
            context.InstanceComp = new WorldComponentHandle(WorldComponentGroup<TInstance>.TypeId, compIndex);
            if (withFinish)
                group.AddingFinish(compIndex);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IWorldBehaviorParamTypeGettable
    {
        ushort ParamTypeId { get; }
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IWorldBehaviorInstanceComponentable : IWorldComponentable
    {
        public WorldBehaviorContext BehaviorContext { get; set; }
    }
}
