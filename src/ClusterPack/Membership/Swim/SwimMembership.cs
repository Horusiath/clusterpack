using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ClusterPack.Discovery;
using ClusterPack.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClusterPack.Membership.Swim
{
    public sealed class SwimMembership : IMembership, IAsyncDisposable
    {
        private readonly ILogger logger;
        private readonly SwimMembershipOptions options;
        private readonly ITransport transport;

        public SwimMembership(ILogger logger,
            ITransport transport, 
            SwimMembershipOptions options = null)
        {
            this.logger = logger;
            this.options = options ?? new SwimMembershipOptions();
            this.transport = transport;
        }

        /// <inheritdoc cref="IMembership"/>
        public Member Local { get; }
        
        /// <inheritdoc cref="IMembership"/>
        public ImmutableSortedSet<Member> ActiveMembers { get; }
        
        /// <inheritdoc cref="IMembership"/>
        public IAsyncEnumerable<Message> IncomingMessages { get; }
        
        /// <inheritdoc cref="IMembership"/>
        public IAsyncEnumerable<MembershipEvent> MembershipEvents { get; }

        public async Task JoinAsync(IDiscovery discovery, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        
        /// <inheritdoc cref="IMembership"/>
        public ValueTask SendAsync(NodeId recipient, ReadOnlySequence<byte> payload)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc cref="IMembership"/>
        public ValueTask BroadcastAsync(ReadOnlySequence<byte> payload)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc cref="IMembership"/>
        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }

        public static async Task<SwimMembership> JoinAsync(EndPoint localEndpoint, CancellationToken cancellationToken = default, params EndPoint[] remoteEndpoints)
        {
            var transport = new TcpTransport(NullLogger.Instance);
            var discovery = new StaticDiscovery(remoteEndpoints);
            var membership = new SwimMembership(NullLogger.Instance, transport);

            await membership.JoinAsync(discovery, cancellationToken);
            return membership;
        }
    }
}