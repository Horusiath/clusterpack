using System;
using System.Buffers;
using ClusterPack.Membership;
using ClusterPack.Transport;

namespace ClusterPack
{
    /// <summary>
    /// Message to be send to another member. Messages can be send via <see cref="IMembership.SendAsync{T}"/>
    /// or <see cref="IMembership.BroadcastAsync{T}"/>.
    /// </summary>
    public readonly struct Message : IDisposable
    {
        private readonly IMemoryOwner<byte> payload;
        
        /// <summary>
        /// A node identifier of a member, which has send this message.
        /// </summary>
        public NodeId Sender { get; }

        /// <summary>
        /// A binary payload send as a content of the message. 
        /// </summary>
        public ReadOnlySequence<byte> Payload => new ReadOnlySequence<byte>(payload.Memory);

        internal Message(NodeId sender, IMemoryOwner<byte> payload)
        {
            this.Sender = sender;
            this.payload = payload;
        }

        public void Dispose()
        {
            this.payload.Dispose();
        }
    }
}