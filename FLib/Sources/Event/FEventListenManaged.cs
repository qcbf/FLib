// ==================== qcbf@qq.com | 2025-10-24 ====================

using System;
using FLib;

namespace FLib
{
    /// <summary>
    /// 对事件监听的托管
    /// </summary>
    public struct FEventListenManaged : IDisposable
    {
        public ListenData One;
        public PooledList<ListenData> More;

        public readonly bool IsEmpty => One.IsEmpty && More.IsEmpty;

        /// <summary>
        /// 
        /// </summary>
        public readonly struct ListenData
        {
            public readonly int EvtId;
            public readonly FEvent Evt;
            public readonly Delegate Handler;

            public ListenData(FEvent evt, int evtId, Delegate handler)
            {
                Evt = evt;
                EvtId = evtId;
                Handler = handler;
            }

            public bool IsEmpty => Handler == null;
            public void Unlisten() => Evt?.UnlistenEventImpl(EvtId, Handler);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Push(FEvent evt, int evtId, Delegate handler)
        {
            var data = new ListenData(evt, evtId, handler);
            if (One.IsEmpty)
                One = data;
            else
                More.Add(data);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            One.Unlisten();
            One = default;
            if (!More.IsInitialized) return;
            try
            {
                for (var i = 0; i < More.Count; i++)
                    More[i].Unlisten();
            }
            finally
            {
                More.Dispose();
                More = default;
            }
        }
    }
}
