using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
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
        
        private EndPoint? localEndpoint;
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

        /// <summary>
        /// Returns true if current TCP transport has been disposed.
        /// </summary>
        public bool IsDisposed => isDisposed != 0;
        
        /// <summary>
        /// Returns an endpoint, at which current TCP transport is listening, ready to accept new connections.
        /// This require to call <see cref="BindAsync"/> first, otherwise listener will not be started and
        /// a null value will be returned. 
        /// </summary>
        public EndPoint? LocalEndpoint => localEndpoint;

        /// <inheritdoc cref="ITransport"/>
        public async ValueTask SendAsync(IPEndPoint target, ReadOnlySequence<byte> payload, CancellationToken cancellationToken)
        {
            if (!connections.TryGetValue(target, out var connection))
            {
                connection = await AddConnection(target, cancellationToken);
            }

            await connection.SendAsync(payload, cancellationToken);
        }

        /// <inheritdoc cref="ITransport"/>
        public IAsyncEnumerable<IncomingMessage> BindAsync(EndPoint endpoint, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var previous = Interlocked.CompareExchange(ref localEndpoint, endpoint, null);
            if (previous is null)
            {
                this.socket.Bind(endpoint);

                acceptorLoop = this.taskFactory.StartNew(AcceptConnections, cancellationToken);
            
                logger.LogInformation("listening on '{0}'", endpoint);

                return MessageStream(cancellationToken);
            }
            else
            {
                throw new ArgumentException($"Cannot bind {nameof(TcpTransport)} to endpoint '{endpoint}', because it's already listening at {localEndpoint}", nameof(endpoint));
            }
        }

        private async IAsyncEnumerable<IncomingMessage> MessageStream(CancellationToken cancellationToken)
        {
            var reader = incommingMessages.Reader;
            while (await reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
            {
                while (reader.TryRead(out var message))
                {
                    if (cancellationToken.IsCancellationRequested)
                        yield break;

                    yield return message;
                }
            }
        }

        /// <summary>
        /// Asynchronously disconnects maintained TCP connection with a given <paramref name="endpoint"/>.
        /// Returns false if no active connection to a corresponding <paramref name="endpoint"/> was found. 
        /// </summary>
        public async ValueTask<bool> DisconnectAsync(IPEndPoint endpoint)
        {
            if (connections.TryRemove(endpoint, out var connection))
            {
                await connection.DisposeAsync();
                return true;
            }

            return false;
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

                this.taskFactory.StartNew(connection.Start).ConfigureAwait(false);
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