// ==================== qcbf@qq.com | 2025-09-11 ====================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FLib;

namespace FLib.Net
{
    public abstract class FNetResponseBase : IFNetCallbackable
    {
        public static Action<FNetResponseBase, Exception> OnExceptionHandler;
        public FNetChannel Channel;
        public int Cmd;


        public virtual void Invoke(FNetChannel channel, FNetProcessor processor)
        {
            Channel = channel;
            Cmd = processor.Cmd;
        }

        protected virtual void OnException(Exception e)
        {
            Log.Error?.Write(e, Channel, FNetChannel.LogCmdHandler(Cmd));
            if (Channel == null) return;
            if (OnExceptionHandler != null)
                OnExceptionHandler.Invoke(this, e);
            else
                Error(1,
#if DEBUG
                    e.Message
#else
                    null
#endif
                );
        }

        public bool Assert(int code, string text = null)
        {
            if (code == 0) return true;
            Channel.SendState(Cmd, code, text);
            Channel = null;
            return false;
        }

        public bool Assert(bool assert, int code, string text = null)
        {
            if (assert) return true;
            Channel.SendState(Cmd, code, text);
            Channel = null;
            return false;
        }

        public void Error(int code, string text = null)
        {
            Channel.SendState(Cmd, code, text);
            Channel = null;
        }

        public void Ok()
        {
            Channel.Send(Cmd);
            Channel = null;
        }

        public void Ok<T>(in T data) where T : IBytesPackable
        {
            Channel.Send(Cmd, data);
            Channel = null;
        }

        public void Ok<T>(in IList<T> datas) where T : IBytesPackable
        {
            Channel.Send(Cmd, datas);
            Channel = null;
        }
    }

    public class FNetResponse : FNetResponseBase
    {
        public Func<FNetResponse, Task> AsyncHandler;
        public Action<FNetResponse> SyncHandler;

        public FNetResponse(Action<FNetResponse> syncHandler) => SyncHandler = syncHandler;
        public FNetResponse(Func<FNetResponse, Task> asyncHandler) => AsyncHandler = asyncHandler;

        public override void Invoke(FNetChannel channel, FNetProcessor processor)
        {
            base.Invoke(channel, processor);
            Log.Debug?.Write("receive", this, FNetChannel.LogCmdHandler(Cmd));
            try
            {
                if (SyncHandler != null)
                    SyncHandler.Invoke(this);
                else
                    AsyncHandler.Invoke(this).ContinueWith(task =>
                    {
                        if (task.Exception != null)
                            OnException(task.Exception);
                    });
            }
            catch (Exception e)
            {
                OnException(e);
            }
        }
    }

    public class FNetResponseState : FNetResponseBase
    {
        public Action<FNetResponseState> SyncHandler;
        public int Code;
        public string Text;

        public FNetResponseState(Action<FNetResponseState> syncHandler) => SyncHandler = syncHandler;

        public override void Invoke(FNetChannel channel, FNetProcessor processor)
        {
            base.Invoke(channel, processor);
            try
            {
                BytesReader reader = processor.Buffer.AsSpan();
                Code = (int)reader.ReadVInt();
                Text = reader.ReadString();
                SyncHandler.Invoke(this);
            }
            catch (Exception e)
            {
                OnException(e);
            }
        }
    }

    public class FNetResponse<T> : FNetResponseBase where T : IBytesPackable, new()
    {
        public Func<FNetResponse<T>, Task> AsyncHandler;
        public Action<FNetResponse<T>> SyncHandler;
        public T Args;

        public FNetResponse(Action<FNetResponse<T>> syncHandler) => SyncHandler = syncHandler;
        public FNetResponse(Func<FNetResponse<T>, Task> asyncHandler) => AsyncHandler = asyncHandler;

        public override void Invoke(FNetChannel channel, FNetProcessor processor)
        {
            base.Invoke(channel, processor);
            try
            {
                Args = new T();
                BytesPack.Unpack(ref Args, processor.Buffer);
                Log.Debug?.Write("receive: " + Json5.SerializeToLog(Args), Channel, FNetChannel.LogCmdHandler(Cmd));
                if (SyncHandler != null)
                    SyncHandler.Invoke(this);
                else
                    AsyncHandler.Invoke(this).ContinueWith(task =>
                    {
                        if (task.Exception != null)
                            OnException(task.Exception);
                    });
            }
            catch (Exception e)
            {
                OnException(e);
            }
        }
    }

    public class FNetResponseList<T> : FNetResponseBase where T : IBytesPackable
    {
        public Func<FNetResponseList<T>, Task> AsyncHandler;
        public Action<FNetResponseList<T>> SyncHandler;
        public T[] Args = Array.Empty<T>();

        public FNetResponseList(Action<FNetResponseList<T>> syncHandler) => SyncHandler = syncHandler;
        public FNetResponseList(Func<FNetResponseList<T>, Task> asyncHandler) => AsyncHandler = asyncHandler;

        public override void Invoke(FNetChannel channel, FNetProcessor processor)
        {
            base.Invoke(channel, processor);
            try
            {
                BytesPack.Unpack(ref Args, processor.Buffer);
                Log.Debug?.Write("receive: " + Json5.SerializeToLog(Args), Channel, FNetChannel.LogCmdHandler(Cmd));
                if (SyncHandler != null)
                    SyncHandler.Invoke(this);
                else
                    AsyncHandler.Invoke(this).ContinueWith(task =>
                    {
                        if (task.Exception != null)
                            OnException(task.Exception);
                    });
            }
            catch (Exception e)
            {
                OnException(e);
            }
        }
    }
}
