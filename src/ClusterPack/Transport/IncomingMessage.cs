using System;
using System.Buffers;
using System.Net;

namespace ClusterPack.Transport
{
    public readonly struct IncomingMessage : IDisposable
    {
        private readonly IMemoryOwner<byte> payload;

        public IPEndPoint Endpoint { get; }
        public ReadOnlySequence<byte> Payload => new ReadOnlySequence<byte>(payload.Memory);

        internal IncomingMessage(IPEndPoint endpoint, IMemoryOwner<byte> payload)
        {
            this.Endpoint = endpoint;
            this.payload = payload;
        }

        public void Dispose()
        {
            payload.Dispose();
        }
    }
}