using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using ClusterPack.Transport;

namespace ClusterPack.Testing
{
    public sealed class TestTransport : ITransport
    {
        private readonly TestNetwork network;
        private readonly Channel<IncomingMessage> channel;
        
        public EndPoint? LocalEndpoint { get; private set; }

        internal TestTransport(TestNetwork network)
        {
            this.network = network;
            this.channel = Channel.CreateUnbounded<IncomingMessage>();
        }

        public async ValueTask SendAsync(IPEndPoint destination, ReadOnlySequence<byte> payload, CancellationToken cancellationToken)
        {
            if (network.TryGetTransport(destination, out var transport))
            {
                await transport.ReceiveAsync(new IncomingMessage(destination, payload), cancellationToken);
            }
            else
            {
                throw new TestTransportException($"Couldn't send message to '{destination}', because there's no known test transport listening on that endpoint");
            }
        }

        private ValueTask ReceiveAsync(IncomingMessage message, CancellationToken cancellationToken)
        {
            return channel.Writer.WriteAsync(message, cancellationToken);
        }

        public async IAsyncEnumerable<IncomingMessage> BindAsync(EndPoint endpoint, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (LocalEndpoint is null)
            {
                LocalEndpoint = endpoint;
                network.Register(LocalEndpoint, this);
                
                var reader = channel.Reader;
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
            else
            {
                throw new ArgumentException($"Failed to bind {nameof(TestTransport)} to '{endpoint}', because it's already bound to '{LocalEndpoint}'");
            }
        }
    }
}