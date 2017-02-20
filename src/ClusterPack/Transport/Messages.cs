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


    internal struct Ping
    {
        public readonly uint SeqNo;
        public readonly string Node;

        public Ping(uint seqNo, string node)
        {
            SeqNo = seqNo;
            Node = node;
        }
    }

    internal struct IndirectPing
    {
        public readonly uint SeqNo;
        public readonly string Node;
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

    internal struct Ack
    {
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