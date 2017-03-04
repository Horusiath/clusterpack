using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ClusterPack.Transport;

namespace ClusterPack
{
    /// <summary>
    /// A node is a core element of ClusterPack. It represents a single 
    /// cluster node and provides a set of methods that can be used to 
    /// manage and react on cluster state changes.
    /// 
    /// Cluster pack uses SWIM protocol to maintain and coordinate cluster
    /// state.
    /// </summary>
    public sealed class Node : IDisposable
    {
        private readonly ClusterSettings settings;
        private readonly ITransport transport;
        private readonly ITimer timer;
        private readonly ConcurrentDictionary<string, MemberInfo> members;
        private readonly object syncLock = new object();

        private bool disposing = false;

        /// <summary>
        /// A list of all known cluster members, no matter in what state 
        /// they are.
        /// </summary>
        public IEnumerable<MemberInfo> KnownMembers => members.Values;

        /// <summary>
        /// Gets endpoint at which node is listening on.
        /// </summary>
        public IPEndPoint Endpoint => settings.Endpoint;

        /// <summary>
        /// Gets name of the current node. It must be unique in cluster scope.
        /// </summary>
        public string Name => settings.Name;
        
        /// <summary>
        /// Event triggered once a current node acknowledges, that a new member 
        /// joined the cluster.
        /// </summary>
        public event AsyncEventHandler<MemberJoined> OnJoined;

        /// <summary>
        /// Event triggered once a current node acknowledges, that an existing
        ///  member has left the cluster.
        /// </summary>
        public event AsyncEventHandler<MemberLeft> OnLeft;

        /// <summary>
        /// Event triggered once a current node receives a message emitted using
        /// either <see cref="Send"/> or <see cref="Broadcast"/>. It's possible 
        /// that node will receive a message send by itself.
        /// </summary>
        public AsyncEventHandler<IncomingMessage> OnMessage;

        public Node(string name, string host = "localhost", ushort port = 0)
            : this(new ClusterSettings(name: name, host: host, port: port)) { }

        public Node(ClusterSettings settings)
        {
            this.settings = settings;
            this.transport = settings.Transport;
            this.timer = settings.Timer;
            this.members = new ConcurrentDictionary<string, MemberInfo>();
            
            this.transport.Listen(this.settings.Endpoint);
        }

        /// <summary>
        /// Asynchronously tries to join this node to the cluster by trying 
        /// to connect to a other nodes listening under given set of 
        /// <paramref name="endpoints"/>. It will try to connect to each 
        /// node one by one, trying a next one if previous failed.
        /// 
        /// If no <paramref name="endpoints"/> were provided, node will 
        /// initialize a new cluster instead.
        /// </summary>
        /// <param name="endpoints">
        /// A collection of endpoints, under which other cluster nodes 
        /// should be listening.
        /// </param>
        public Task Join(params IPEndPoint[] endpoints) => Join(new CancellationToken(), endpoints);

        /// <summary>
        /// Asynchronously tries to join this node to the cluster by trying 
        /// to connect to a other nodes listening under given set of 
        /// <paramref name="endpoints"/>. It will try to connect to each 
        /// node one by one, trying a next one if previous failed.
        /// 
        /// If no <paramref name="endpoints"/> were provided, node will 
        /// initialize a new cluster instead.
        /// </summary>
        /// <param name="cancellationToken">
        /// Cancellation token, that may be used to stop joining process.
        /// </param>
        /// <param name="endpoints">
        /// A collection of endpoints, under which other cluster nodes 
        /// should be listening.
        /// </param>
        public async Task Join(CancellationToken cancellationToken = default(CancellationToken), params IPEndPoint[] endpoints)
        {
            if (endpoints == null || endpoints.Length == 0)
            {
                await InitCluster(cancellationToken);
            }
            else
            {
                var errors = new List<Exception>();
                foreach (var endpoint in endpoints)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        // even when cancelling, we don't want to loose 
                        // previous cancellation reasons
                        errors.Add(new OperationCanceledException(cancellationToken));
                        break;
                    }

                    try
                    {
                        await PushPullNode(endpoint);
                        return;
                    }
                    catch (Exception cause)
                    {
                        errors.Add(cause);
                    }
                }

                if (errors.Count > 0)
                    throw new AggregateException(errors);
            }
        }

        /// <summary>
        /// Asynchronously tries to perform a graceful shutdown procedure, 
        /// trying to leave the cluster with acknowledging other nodes about 
        /// that before disconnecting.
        /// </summary>
        public async Task Leave()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously tries to send a given <paramref name="payload"/> 
        /// directly (without using gossip mechanism) to a member known by 
        /// <paramref name="memberName"/>. This method  will fail fast if no 
        /// member can be found by a given name or if  its last state was 
        /// known to be <see cref="MemberState.Dead"/>.
        /// </summary>
        /// <param name="memberName">
        /// Name of the node, we want to contact with.
        /// </param>
        /// <param name="payload">
        /// A payload to be send to another node.
        /// </param>
        public async Task Send(string memberName, byte[] payload)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously broadcasts provided <paramref name="payload"/> to 
        /// all known nodes in <see cref="MemberState.Alive"/> state.
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        public async Task Broadcast(byte[] payload)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously performs a shutdown procedure.
        /// </summary>
        /// <returns></returns>
        public async Task Shutdown()
        {
            this.transport.Shutdown();
            this.timer.Dispose();
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        private Task InitCluster(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private Task PushPullNode(IPEndPoint endpoint)
        {
            throw new NotImplementedException();
        }

        private void HandleCommand(IPEndPoint from, MessageType type, byte[] paload, DateTime timestamp)
        {
            switch (type)
            {
                case MessageType.Ping: break;
                case MessageType.IndirectPing: break;
                case MessageType.Ack: break;
                case MessageType.Nack: break;
                case MessageType.Suspect: break;
                case MessageType.Alive: break;
                case MessageType.Dead: break;
                case MessageType.User: break;
            }
        }

        private async Task Alive(Alive msg)
        {
            lock (syncLock)
            {
                if (this.members.TryGetValue(msg.Node, out var info))
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}