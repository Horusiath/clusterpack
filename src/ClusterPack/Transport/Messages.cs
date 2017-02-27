using System.Net;

namespace ClusterPack.Transport
{
    internal enum MessageType
    {
        Ping = 1,
        IndirectPing,
        Ack,
        Nack,
        Alive,
        Suspect,
        Dead,
        PushPull,
        Compound,
        User,
    }

    /// <summary>
    /// Ping messages are send periodically to detect failed nodes. Each 
    /// <see cref="Node"/>  may decide to check if another node is alive. 
    /// To do so it sends a  <see cref="Ping"/> request to it. If it didn't 
    /// received a <see cref="Ack"/> message with the same <see cref="SeqNo"/> 
    /// in time, it starts and <see cref="IndirectPing"/> procedure.
    /// </summary>
    internal struct Ping
    {
        /// <summary>
        /// A sequence number used as correlation id between current 
        /// <see cref="Ping"/> request and expected <see cref="Ack"/> response.
        /// </summary>
        public readonly uint SeqNo;

        /// <summary>
        /// A name of a <see cref="ClusterPack.Node"/> which sent this 
        /// <see cref="Ping"/> request.
        /// </summary>
        public readonly string Node;

        public Ping(uint seqNo, string node)
        {
            SeqNo = seqNo;
            Node = node;
        }
    }

    /// <summary>
    /// When a <see cref="Ping"/> request has not been responded with 
    /// <see cref="Ack"/> within a configured period, a <see cref="Node"/> 
    /// chooses a some amount of other well known nodes and sends an 
    /// <see cref="IndirectPing"/> request to them. In its behalf those nodes
    /// will try to <see cref="Ping"/> target node and if it responded, reply 
    /// with <see cref="Ack"/> to original requestor.
    /// 
    /// <see cref="IndirectPing"/> is used to avoid potential network congestion 
    /// between two nodes, that may cause dropping messages or response slowdowns.
    /// </summary>
    internal struct IndirectPing
    {
        /// <summary>
        /// A sequence number used as correlation id from original 
        /// <see cref="Node"/> to match expected <see cref="Ack"/> reply.
        /// </summary>
        public readonly uint SeqNo;

        /// <summary>
        /// Name of an original <see cref="ClusterPack.Node"/> which is probing 
        /// another node.
        /// </summary>
        public readonly string Node;

        /// <summary>
        /// An endpoint at which a target <see cref="ClusterPack.Node"/> is 
        /// expected to be listening.
        /// </summary>
        public readonly IPEndPoint Endpoint;
        public readonly bool ShouldNack;

        public IndirectPing(uint seqNo, string node, IPEndPoint endpoint, bool shouldNack)
        {
            SeqNo = seqNo;
            Node = node;
            Endpoint = endpoint;
            ShouldNack = shouldNack;
        }
    }

    /// <summary>
    /// A positive response to a <see cref="Ping"/> request.
    /// </summary>
    internal struct Ack
    {
        /// <summary>
        /// Sequence number to correlate this <see cref="Ack"/> response 
        /// with a correct <see cref="Ping"/> request.
        /// </summary>
        public readonly uint SeqNo;
        public readonly byte[] Payload;

        public Ack(uint seqNo, byte[] payload)
        {
            SeqNo = seqNo;
            Payload = payload;
        }
    }

    internal struct Nack
    {
        public readonly uint SeqNo;

        public Nack(uint seqNo)
        {
            SeqNo = seqNo;
        }
    }

    internal struct Suspect
    {
        public readonly uint Incarnation;
        public readonly string Node;
        public readonly string Notifier;

        public Suspect(uint incarnation, string node, string notifier)
        {
            Incarnation = incarnation;
            Node = node;
            Notifier = notifier;
        }
    }

    internal struct Alive
    {
        public readonly uint Incarnation;
        public readonly string Node;
        public readonly IPEndPoint Endpoint;
        public readonly byte[] Metadata;

        public Alive(uint incarnation, string node, IPEndPoint endpoint, byte[] metadata)
        {
            Incarnation = incarnation;
            Node = node;
            Endpoint = endpoint;
            Metadata = metadata;
        }
    }

    internal struct Dead
    {
        public readonly uint Incarnation;
        public readonly string Node;
        public readonly string Notifier;

        public Dead(uint incarnation, string node, string notifier)
        {
            Incarnation = incarnation;
            Node = node;
            Notifier = notifier;
        }
    }

    internal struct PushPullHeader
    {
        public readonly int Nodes;
        public readonly int UserStateSize;
        public readonly bool IsJoin;

        public PushPullHeader(int nodes, int userStateSize, bool isJoin)
        {
            Nodes = nodes;
            UserStateSize = userStateSize;
            IsJoin = isJoin;
        }
    }

    internal struct PushNodeState
    {
        public readonly string Name;
        public readonly IPEndPoint Endpoint;
        public readonly byte[] Metadata;
        public readonly uint Incarnation;
        public readonly MemberState State;

        public PushNodeState(string name, IPEndPoint endpoint, byte[] metadata, uint incarnation, MemberState state)
        {
            Name = name;
            Endpoint = endpoint;
            Metadata = metadata;
            Incarnation = incarnation;
            State = state;
        }
    }

    internal struct Handoff
    {
        public readonly MessageType Type;
        public readonly IPEndPoint From;
        public readonly byte[] Payload;

        public Handoff(MessageType type, IPEndPoint @from, byte[] payload)
        {
            Type = type;
            From = @from;
            Payload = payload;
        }
    }   
}