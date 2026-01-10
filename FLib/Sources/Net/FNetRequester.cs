// ==================== qcbf@qq.com | 2025-09-11 ====================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace FLib.Net
{
    public class FNetRequester
    {
        public ConcurrentBag<FNetRequestingBase> ReadyRequesting;
        public List<FNetRequestingBase> AllRequesting = new();
        public FNetChannel Channel;

        public FNetRequester(FNetChannel channel, bool concurrent = false)
        {
            Channel = channel;
            if (concurrent)
                ReadyRequesting = new ConcurrentBag<FNetRequestingBase>();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Update()
        {
            while (ReadyRequesting != null && ReadyRequesting.TryTake(out var requesting))
            {
                Channel.ReceiveCallbacks.Add(requesting.Cmd, requesting);
                AllRequesting.Add(requesting);
            }
            Channel.Update();
            if (Channel.Invalid)
            {
                if (AllRequesting.Count > 0)
                    ClearAll();
                return;
            }
            AllRequesting.RemoveAll(static v => v.SendTime <= 0);
            var t = Environment.TickCount;
            foreach (var requesting in AllRequesting)
            {
                if (t - requesting.SendTime < Channel.Timeout) continue;
                OnTimeout(requesting);
                break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void OnTimeout(FNetRequestingBase requesting)
        {
            Log.Warn?.Write("send timeout", Channel, FNetChannel.LogCmdHandler(requesting.Cmd));
            ClearAll();
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void ClearAll()
        {
            foreach (var item in AllRequesting)
                item.Dispose();
            AllRequesting.Clear();
            Channel.Close("request clear all");
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal virtual void OnReceiveError(int cmd, int code, string msg)
        {
            Log.Error?.Write($"receive error {FNetChannel.LogErrorCodeHandler(code)} {msg}", FNetChannel.LogCmdHandler(cmd));
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal virtual void OnRemoveRequesting(FNetRequestingBase requesting)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public RequestingHelper Send(int cmd)
        {
            Channel.Send(cmd);
            return new RequestingHelper(this, cmd);
        }

        /// <summary>
        /// 
        /// </summary>
        public RequestingHelper Send<T>(int cmd, in T data) where T : IBytesPackable, new()
        {
            Channel.Send(cmd, data);
            return new RequestingHelper(this, cmd);
        }

        /// <summary>
        /// 
        /// </summary>
        public RequestingHelper Send<T>(int cmd, in IList<T> datas) where T : IBytesPackable, new()
        {
            Channel.Send(cmd, datas);
            return new RequestingHelper(this, cmd);
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal virtual FNetRequestingBase AddRequesting(int cmd, FNetRequestingBase requesting)
        {
            requesting.Requester = this;
            requesting.SendTime = Environment.TickCount;
            requesting.Cmd = cmd;
            if (ReadyRequesting == null)
            {
                Channel.ReceiveCallbacks.Add(requesting.Cmd, requesting);
                AllRequesting.Add(requesting);
            }
            else
            {
                ReadyRequesting.Add(requesting);
            }
            return requesting;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public readonly ref struct RequestingHelper
    {
        public readonly FNetRequester Requester;
        public readonly int Cmd;

        public RequestingHelper(FNetRequester requester, int cmd)
        {
            Requester = requester;
            Cmd = cmd;
        }

        public Task Response(bool cancelAsException = false)
        {
            var requesting = GlobalObjectPool<FNetRequesting>.Create();
            Requester.AddRequesting(Cmd, requesting).CancelAsException = cancelAsException;
            return requesting.TaskSource.Task;
        }

        public Task<T> Response<T>(bool cancelAsException = false) where T : IBytesPackable, new()
        {
            var requesting = GlobalObjectPool<FNetRequesting<T>>.Create();
            Requester.AddRequesting(Cmd, requesting).CancelAsException = cancelAsException;
            return requesting.TaskSource.Task;
        }

        public Task<T[]> ResponseList<T>(bool cancelAsException = false) where T : IBytesPackable, new()
        {
            var requesting = GlobalObjectPool<FNetRequestingList<T>>.Create();
            Requester.AddRequesting(Cmd, requesting).CancelAsException = cancelAsException;
            return requesting.TaskSource.Task;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class FNetRequestingBase : IFNetCallbackable
    {
        public FNetRequester Requester;
        public bool CancelAsException;
        public long SendTime;
        public int Cmd;

        public virtual void Invoke(FNetChannel channel, FNetProcessor processor)
        {
            try
            {
                if (processor.Cmd < 0)
                {
                    BytesReader reader = processor.Buffer.AsSpan();
                    OnError((int)reader.ReadVInt(), reader.ReadString());
                }
                else
                {
                    OnResult(processor);
                }
            }
            finally
            {
                Dispose();
            }
        }

        protected virtual void OnError(int code, string error)
        {
            Requester.OnReceiveError(Cmd, code, error);
        }

        protected abstract void OnResult(FNetProcessor processor);

        internal virtual void Dispose()
        {
            Requester.Channel.ReceiveCallbacks.Remove(Cmd);
            Requester.OnRemoveRequesting(this);
            CancelAsException = false;
            SendTime = Cmd = 0;
            Requester = null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class FNetRequesting : FNetRequestingBase, IObjectPoolActivatable
    {
#if UNITY_PROJ
        internal TaskCompletionSource<bool> TaskSource = new();
#else
        internal TaskCompletionSource TaskSource = new();
#endif

        public void ObjectPoolActivate()
        {
            TaskSource = new();
        }

        protected override void OnResult(FNetProcessor processor)
        {
            Log.Debug?.Write("receive", Requester.Channel, FNetChannel.LogCmdHandler(Cmd));
#if UNITY_PROJ
            TaskSource?.SetResult(true);
#else
            TaskSource?.SetResult();
#endif
            TaskSource = null;
        }

        protected override void OnError(int code, string error)
        {
            if (CancelAsException)
                TaskSource.SetException(new FNetRequestingException(code, error));
            else
                TaskSource.SetCanceled();
            TaskSource = null;
            base.OnError(code, error);
        }

        internal override void Dispose()
        {
            GlobalObjectPool<FNetRequesting>.Release(this);
            base.Dispose();
            Log.Assert(TaskSource == null);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class FNetRequesting<T> : FNetRequestingBase, IObjectPoolActivatable where T : IBytesPackable, new()
    {
        internal TaskCompletionSource<T> TaskSource;

        public void ObjectPoolActivate()
        {
            TaskSource = new TaskCompletionSource<T>();
        }

        protected override void OnError(int code, string error)
        {
            if (CancelAsException)
                TaskSource.SetException(new FNetRequestingException(code, error));
            else
                TaskSource.SetCanceled();
            TaskSource = null;
            base.OnError(code, error);
        }

        protected override void OnResult(FNetProcessor processor)
        {
            var data = new T();
            BytesPack.Unpack(ref data, processor.Buffer);
            Log.Debug?.Write("receive: " + Json5.SerializeToLog(data), Requester.Channel, FNetChannel.LogCmdHandler(Cmd));
            TaskSource?.SetResult(data);
            TaskSource = null;
        }

        internal override void Dispose()
        {
            GlobalObjectPool<FNetRequesting<T>>.Release(this);
            base.Dispose();
            Log.Assert(TaskSource == null);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class FNetRequestingList<T> : FNetRequestingBase, IObjectPoolActivatable where T : IBytesPackable, new()
    {
        internal TaskCompletionSource<T[]> TaskSource;

        public void ObjectPoolActivate()
        {
            TaskSource = new TaskCompletionSource<T[]>();
        }

        protected override void OnError(int code, string error)
        {
            if (CancelAsException)
                TaskSource.SetException(new FNetRequestingException(code, error));
            else
                TaskSource.SetCanceled();
            TaskSource = null;
            base.OnError(code, error);
        }

        protected override void OnResult(FNetProcessor processor)
        {
            T[] data = null;
            BytesPack.Unpack(ref data, processor.Buffer);
            Log.Debug?.Write("receive: " + Json5.SerializeToLog(data), Requester.Channel, FNetChannel.LogCmdHandler(Cmd));
            TaskSource?.SetResult(data);
            TaskSource = null;
        }

        internal override void Dispose()
        {
            GlobalObjectPool<FNetRequestingList<T>>.Release(this);
            base.Dispose();
            Log.Assert(TaskSource == null);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class FNetRequestingException : Exception
    {
        public int Code;
        public string Error;
        public override string ToString() => $"Code:{Code} Text:{Error}";

        public FNetRequestingException(int code, string error)
        {
            Code = code;
            Error = error;
        }
    }
}
