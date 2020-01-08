using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ClusterPack.Transport
{
    public sealed class TcpTransport : ITransport, IAsyncDisposable
    {
        private readonly SemaphoreSlim sync;
        private readonly ILogger logger;
        private readonly TcpTransportOptions options;
        private readonly TaskFactory taskFactory;
        private readonly MemoryPool<byte> memoryPool;
        private readonly Socket socket;
        private readonly Channel<IncomingMessage> incommingMessages;
        private readonly ConcurrentDictionary<IPEndPoint, TcpConnection> connections;
        
        private EndPoint? listenningEndpoint;
        private Task? acceptorLoop;
        private int isDisposed = 0;

        public TcpTransport(ILogger logger, 
            TcpTransportOptions? options = null,
            MemoryPool<byte>? memoryPool = null,
            TaskFactory? taskFactory = null)
        {
            options ??= new TcpTransportOptions();
            this.sync = new SemaphoreSlim(1);
            this.logger = logger;
            this.options = options;
            this.memoryPool = memoryPool ?? MemoryPool<byte>.Shared;
            this.taskFactory = taskFactory ?? Task.Factory;
            this.socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            this.incommingMessages = Channel.CreateBounded<IncomingMessage>(options.IncomingMessageBufferSize);
            this.connections = new ConcurrentDictionary<IPEndPoint, TcpConnection>();
        }

        public bool IsDisposed => isDisposed != 0;

        /// <inheritdoc cref="ITransport"/>
        public async ValueTask SendAsync(IPEndPoint target, ReadOnlySequence<byte> payload, CancellationToken cancellationToken)
        {
            if (!connections.TryGetValue(target, out var connection))
            {
                connection = await AddConnection(target, cancellationToken);
            }

            await connection.SendAsync(payload, cancellationToken);
        }

        private async Task<TcpConnection> AddConnection(IPEndPoint target, CancellationToken cancellationToken)
        {
            TcpConnection connection;
            await sync.WaitAsync(cancellationToken);
            try
            {
                // we need to check again after lock was acquired
                if (!connections.TryGetValue(target, out connection))
                {
                    var sock = new Socket(SocketType.Stream, ProtocolType.Tcp);
                    await sock.ConnectAsync(target);
                    connection = new TcpConnection(sock, memoryPool, incommingMessages.Writer);
                    connections.TryAdd(target, connection);
                }
            }
            finally
            {
                sync.Release();
            }

            connection.Start();
            
            logger.LogInformation("connected to '{0}'", target);
            
            return connection;
        }

        /// <inheritdoc cref="ITransport"/>
        public async IAsyncEnumerable<IncomingMessage> BindAsync(EndPoint endpoint)
        {
            if (!(Interlocked.CompareExchange(ref listenningEndpoint, endpoint, null) is null))
                throw new ArgumentException($"Cannot connect {nameof(TcpTransport)} to endpoint {endpoint}, because it's already bound to {listenningEndpoint}", nameof(endpoint));
            
            this.socket.Bind(endpoint);

            acceptorLoop = this.taskFactory.StartNew(AcceptConnections);
            
            logger.LogInformation("listening on '{0}'", endpoint);

            var reader = incommingMessages.Reader;
            while (await reader.WaitToReadAsync().ConfigureAwait(false))
            {
                while (reader.TryRead(out var message))
                {
                    yield return message;
                }
            }
        }

        private async Task AcceptConnections()
        {
            while (!IsDisposed)
            {
                var incoming = await this.socket.AcceptAsync();
                var connection = new TcpConnection(incoming, this.memoryPool, incommingMessages.Writer);
                while (!this.connections.TryAdd(connection.Endpoint, connection))
                {
                    if (this.connections.TryRemove(connection.Endpoint, out var existing))
                    {
                        await existing.DisposeAsync();
                    }
                }

                connection.Start();
                logger.LogInformation("Received incoming connection from '{0}'", connection.Endpoint);
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref isDisposed, 1, 0) == 0)
            {
                await Task.WhenAll(connections.Values.Select(DisconnectAsync));
                
                acceptorLoop.Dispose();
                socket.Dispose();
                incommingMessages.Writer.Complete();
            }
        }

        private async Task DisconnectAsync(TcpConnection connection)
        {
            try
            {
                await connection.DisposeAsync();
            }
            catch (Exception e)
            {
                logger.LogError(e, "an exception happened while trying to closea TCP connection to '{0}'", connection.Endpoint);
            }
        }
    }
}