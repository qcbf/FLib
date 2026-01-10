// ==================== qcbf@qq.com | 2025-07-01 ====================

namespace FLib.Worlds
{
    internal static class WorldComponentProcessor
    {
        internal delegate void LogicProcessDelegate(WorldComponentGroup group, int componentIndex);

        internal delegate void UpdateProcessDelegate(WorldBase world);


        /// <summary>
        /// 
        /// </summary>
        internal static void UpdateProcess<T>(WorldBase world) where T : IWorldUpdateComponentable, new()
        {
            var isIgnoreEntityPause = (WorldComponentGroup<T>.Options & EWorldComponentOption.IgnoreEntityPause) != 0;
            var metas = world.ComponentMgr.GetGroup<T>().ComponentMetas;
            var entityPausers = world.EntityMgr.AllEntityPausers;
            var metasLength = metas.Length;
            for (var i = 0; i < metasLength; i++)
            {
                ref var meta = ref metas[i];
                if (meta.IsValid && !meta.Pause.IsPaused && (isIgnoreEntityPause || !entityPausers[meta.Entity.Index].IsPaused))
                    meta.Component.ComponentUpdate();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal static void LateUpdateProcess<T>(WorldBase world) where T : IWorldLateUpdateComponentable, new()
        {
            var isIgnoreEntityPause = (WorldComponentGroup<T>.Options & EWorldComponentOption.IgnoreEntityPause) != 0;
            var metas = world.ComponentMgr.GetGroup<T>().ComponentMetas;
            var entityPausers = world.EntityMgr.AllEntityPausers;
            var metasLength = metas.Length;
            for (var i = 0; i < metasLength; i++)
            {
                ref var meta = ref metas[i];
                if (meta.IsValid && !meta.Pause.IsPaused && (isIgnoreEntityPause || !entityPausers[meta.Entity.Index].IsPaused))
                    meta.Component.ComponentLateUpdate();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal static void BeginProcess<T>(WorldComponentGroup group, int componentIndex) where T : IWorldBeginComponentable, new()
            => ((WorldComponentGroup<T>)group).ComponentMetas[componentIndex].Component.ComponentBegin();

        /// <summary>
        /// 
        /// </summary>
        internal static void LateBeginProcess<T>(WorldComponentGroup group, int componentIndex) where T : IWorldLateBeginComponentable, new()
            => ((WorldComponentGroup<T>)group).ComponentMetas[componentIndex].Component.ComponentLateBegin();

        /// <summary>
        /// 
        /// </summary>
        internal static void PreEndProcess<T>(WorldComponentGroup group, int componentIndex) where T : IWorldPreEndComponentable, new()
            => ((WorldComponentGroup<T>)group).ComponentMetas[componentIndex].Component.ComponentPreEnd();

        /// <summary>
        /// 
        /// </summary>
        internal static void EndProcess<T>(WorldComponentGroup group, int componentIndex) where T : IWorldEndComponentable, new()
            => ((WorldComponentGroup<T>)group).ComponentMetas[componentIndex].Component.ComponentEnd();
    }
}
