using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace ClusterPack.Membership

{
    public interface IMembershipService
    {
        /// <summary>
        /// Returns a membership info of current node.
        /// </summary>
        Member Local { get; }
        
        /// <summary>
        /// Returns a membership info about all known active cluster members.
        /// </summary>
        ImmutableSortedSet<Member> ActiveMembers { get; }
        
        /// <summary>
        /// Returns an async sequence of user-defined messages incoming from remote members.
        /// </summary>
        IAsyncEnumerable<Message> IncomingMessages { get; }
        
        /// <summary>
        /// Returns an async sequence of membership events. This way you can react to changes in the cluster structure.
        /// </summary>
        IAsyncEnumerable<MembershipEvent> MembershipEvents { get; }
        
        /// <summary>
        /// Asynchronously sends a requested <paramref name="payload"/> to a given <paramref name="recipient"/>.
        /// Returned task completes after message was successfully send over the transport layer.
        /// </summary>
        ValueTask SendAsync(NodeId recipient, ReadOnlySequence<byte> payload);

        /// <summary>
        /// Asynchronously sends a requested <paramref name="payload"/> to all known active members of the cluster. 
        /// Returned task completes after message was successfully send over the transport layer to all members.
        /// </summary>
        ValueTask BroadcastAsync(ReadOnlySequence<byte> payload);
    }
}