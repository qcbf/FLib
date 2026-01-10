// =================================================={By Qcbf|qcbf@qq.com|2024-08-31}==================================================

using System;

namespace FLib.Net
{
    public interface IHeartbeat
    {
        void Update();
    }

    public class FNetHeartbeatClient : IHeartbeat, IFNetCallbackable
    {
        public readonly FNetChannel Channel;
        public int Interval = 2000;

        /// <summary>
        /// greater than zero receive heart time
        /// less than zero send heart time
        /// </summary>
        private long _actionTime;

        private readonly byte[] _sendBuffer;


        public int NetDelay { get; private set; }

        public FNetHeartbeatClient(FNetChannel channel, int cmd)
        {
            var writer = new BytesWriter(stackalloc byte[8]);
            writer.PushLength3(0);
            writer.PushVInt(cmd);
            BytesWriter.WriteLength3(writer.BytesBuffer, writer.Length - 3);
            _sendBuffer = writer.BytesBuffer[..writer.Length].ToArray();
            Channel = channel;
            _actionTime = Environment.TickCount;
            channel.ReceiveCallbacks.Add(cmd, this);
        }

        /// <summary>
        ///
        /// </summary>
        public void Update()
        {
            var t = Environment.TickCount;
            if (_actionTime >= 0 && t - _actionTime >= Interval)
            {
                if (Channel.Invalid)
                {
                    _actionTime = t;
                }
                else
                {
                    _actionTime = -t;
                    Channel.SendProcess(_sendBuffer);
                }
            }
            else if (_actionTime < 0 && t - -_actionTime >= Interval)
            {
                _actionTime = 0;
                Channel.Close("heartbeat client timeout");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Invoke(FNetChannel channel, FNetProcessor processor)
        {
            var t = Environment.TickCount;
            NetDelay = (int)(t - -_actionTime);
            _actionTime = t;
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class FNetHeartbeatServer : IFNetCallbackable, IHeartbeat
    {
        private readonly byte[] _sendBuffer;

        public FNetHeartbeatServer(int cmd, FNetChannel channel)
        {
            var writer = new BytesWriter(stackalloc byte[8]);
            writer.PushLength3(0);
            writer.PushVInt(cmd);
            BytesWriter.WriteLength3(writer.BytesBuffer, writer.Length - 3);
            _sendBuffer = writer.BytesBuffer[..writer.Length].ToArray();
            channel.ReceiveCallbacks.Add(cmd, this);
        }

        public void Invoke(FNetChannel channel, FNetProcessor processor)
        {
            if (channel == null || channel.Invalid)
                return;

            if (channel.Heartbeat != null)
                ((Client)channel.Heartbeat)._lastActiveTime = Environment.TickCount;

            try
            {
                channel.SendProcess(_sendBuffer);
            }
            catch (Exception e)
            {
                channel.Close(e.Message);
            }
        }

        public void Update()
        {
        }

        public class Client : IHeartbeat
        {
            public int ReceiveTimeout = 5000;
            public FNetChannel Channel;
            internal long _lastActiveTime = Environment.TickCount;

            public Client(FNetChannel channel)
            {
                Channel = channel;
            }

            public void Update()
            {
                var t = Environment.TickCount;
                if (Channel.Invalid)
                {
                    _lastActiveTime = t;
                    return;
                }
                if (t - _lastActiveTime >= ReceiveTimeout)
                    Channel.Close("heartbeat server timeout");
            }
        }
    }
}
