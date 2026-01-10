// ==================== qcbf@qq.com | 2025-09-12 ====================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using FLib;

namespace FLib.Net
{
    public class FNetTcpServerChannel<TId> : FNetSocketChannel
    {
        public int IdleConnectionDieTime = 10 * 1000;
        public Func<FNetTcpServerChannel<TId>, Socket, Client> OnAcceptClient;
        public Action<FNetTcpServerChannel<TId>, Client> OnRemoveClient;
        public Action<FNetTcpServerChannel<TId>, Client> OnRemoveLoginedClient;
        public Action<FNetTcpServerChannel<TId>, Client> OnLoginClient;

        public ConcurrentDictionary<TId, Client> LoginedClients = new();
        public ConcurrentDictionary<Client, long> PendingClients = new();

        public override bool Invalid => Socket == null;
        public Client this[in TId id] => LoginedClients.GetValueOrDefault(id);

        /// <summary>
        /// 
        /// </summary>
        public class Client : FNetSocketChannel
        {
            public TId Id { get; private set; }
            public bool IsLogined { get; private set; }
            public override string ToString() => $"{Id}#{Socket?.RemoteEndPoint}";

            public virtual Client Initialize(FNetTcpServerChannel<TId> server, Socket clientSocket)
            {
                Socket = clientSocket;
                AddressPoint = Socket.RemoteEndPoint;
                ReceiveCallbacks = server.ReceiveCallbacks;
                LoopReceiving();
                return this;
            }

            public virtual void Login(in TId id)
            {
                Id = id;
                IsLogined = true;
            }
        }

        public FNetTcpServerChannel(int port) : this(new IPEndPoint(IPAddress.Any, port))
        {
        }

        public FNetTcpServerChannel(IPEndPoint point) => AddressPoint = point;

        /// <summary>
        /// 
        /// </summary>
        public override void Update()
        {
            base.Update();
            if (!LoginedClients.IsEmpty)
                UpdateClients();
            if (!PendingClients.IsEmpty)
                UpdatePendingClients();
        }

        /// <summary>
        /// 
        /// </summary>
        public FNetTcpServerChannel<TId> Start(int backlog = 1024)
        {
            lock (this)
            {
                Log.Assert(Socket == null);
                Socket = new Socket(AddressPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            }
            Socket.Bind(AddressPoint);
            Socket.Listen(backlog);
            AcceptRequest();
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal override void CloseProcess()
        {
            base.CloseProcess();
            var temp1 = LoginedClients;
            var temp2 = PendingClients;
            PendingClients.Clear();
            LoginedClients.Clear();
            StopClient(temp1.Values, true);
            StopClient(temp2.Keys, false);
            return;

            void StopClient(IEnumerable<Client> clients, bool isLogined)
            {
                foreach (var client in clients)
                {
                    try
                    {
                        client.Close("close server");
                        if (isLogined)
                            OnRemoveLoginedClient?.Invoke(this, client);
                        OnRemoveClient?.Invoke(this, client);
                    }
                    catch (Exception e)
                    {
                        Log.Error?.Write(e.ToString(), client);
                    }
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        protected async void AcceptRequest()
        {
            while (Socket != null)
            {
                Socket socket = null;
                try
                {
                    socket = await Socket.AcceptAsync().ConfigureAwait(false);
                    Log.Debug?.Write("receive connection ", socket.RemoteEndPoint?.ToString());
                    var client = (OnAcceptClient?.Invoke(this, socket) ?? new Client()).Initialize(this, socket);
                    PendingClients.TryAdd(client, Environment.TickCount);
                }
                catch (ObjectDisposedException)
                {
                }
                catch (SocketException e)
                {
                    Log.Info?.Write($"receive connection error {socket?.RemoteEndPoint} {e.Message}");
                    socket?.Dispose();
                }
                catch (Exception e)
                {
                    Log.Info?.Write($"receive connection error {socket?.RemoteEndPoint} \n{e}");
                    socket?.Dispose();
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        private void UpdateClients()
        {
            foreach (var client in LoginedClients)
            {
                if (client.Value.Invalid)
                {
                    if (LoginedClients.TryRemove(client.Key, out var c))
                    {
                        OnRemoveLoginedClient?.Invoke(this, c);
                        OnRemoveClient?.Invoke(this, c);
                    }
                }
                else
                {
                    client.Value.Update();
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        private void UpdatePendingClients()
        {
            var t = Environment.TickCount;
            foreach (var (client, connectedTime) in PendingClients)
            {
                client.Update();
                if (client.Invalid)
                {
                    Log.Debug?.Write("remote close", client);
                    if (PendingClients.TryRemove(client, out _))
                        OnRemoveClient?.Invoke(this, client);
                }
                else if (t - connectedTime >= IdleConnectionDieTime)
                {
                    Log.Debug?.Write("close idle timeout", client);
                    PendingClients.Remove(client, out _);
                    try
                    {
                        client.CloseProcess();
                    }
                    catch (Exception e)
                    {
                        Log.Error?.Write(e, client);
                    }
                    OnRemoveClient?.Invoke(this, client);
                }
                else
                {
                    if (!client.IsLogined) continue;
                    if (PendingClients.TryRemove(client, out _))
                    {
                        if (LoginedClients.TryAdd(client.Id, client))
                        {
                            if (Heartbeat != null)
                                client.Heartbeat = new FNetHeartbeatServer.Client(client);
                            try
                            {
                                OnLoginClient?.Invoke(this, client);
                            }
                            catch (Exception e)
                            {
                                Log.Error?.Write(e, client);
                            }
                        }
                        else
                        {
                            client.Close("add id error");
                        }
                    }
                }
            }
        }
    }
}
