// ==================== qcbf@qq.com | 2025-07-25 ====================

using FLib;

namespace FLib
{
    /// <summary>
    /// 事件监听后续辅助处理
    /// </summary>
    public readonly ref struct FEventListenHelper<T>
    {
        public readonly int EvtId;
        public readonly FEvent Evt;
        public readonly FEvent.PostEventHandler<T> Handler;

        public FEventListenHelper(FEvent evt, int evtId, FEvent.PostEventHandler<T> handler)
        {
            Evt = evt;
            EvtId = evtId;
            Handler = handler;
        }
    }

    public static class FEventExtension
    {
        public static FEventListenHelper<T> Immediate<T>(this in FEventListenHelper<T> helper, in T evtData = default, object dispatcher = null)
        {
            helper.Handler(dispatcher ?? helper.Evt, evtData);
            return helper;
        }
    }
}
