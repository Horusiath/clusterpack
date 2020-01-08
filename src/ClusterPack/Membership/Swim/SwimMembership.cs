using System;
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
    public class SwimMembership : IMembershipService, IAsyncDisposable
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

        /// <inheritdoc cref="IMembershipService"/>
        public Member Local { get; }
        
        /// <inheritdoc cref="IMembershipService"/>
        public ImmutableSortedSet<Member> ActiveMembers { get; }
        
        /// <inheritdoc cref="IMembershipService"/>
        public IAsyncEnumerable<Message> IncomingMessages { get; }
        
        /// <inheritdoc cref="IMembershipService"/>
        public IAsyncEnumerable<MembershipEvent> MembershipEvents { get; }

        public async Task JoinAsync(IDiscovery discovery, CancellationToken cancellationToken)
        {
            
        }
        
        /// <inheritdoc cref="IMembershipService"/>
        public ValueTask SendAsync<T>(NodeId recipient, T payload)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc cref="IMembershipService"/>
        public ValueTask BroadcastAsync<T>(T payload)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc cref="IMembershipService"/>
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