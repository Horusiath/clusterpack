namespace ClusterPack.Membership.Swim
{
    internal interface ISwimProtocol { }

    internal enum ProtocolType : byte
    {
        Ack = 1, 
        Ping = 2,
        PingRequest = 3,
        NewMember = 4,
        Join = 5
    }

    internal sealed class Ack : ISwimProtocol
    {
        public ulong Term { get; }
        public GossipState State { get; }

        public Ack(ulong term, GossipState state)
        {
            Term = term;
            State = state;
        }
    }

    internal sealed class Ping : ISwimProtocol
    {
        public ulong Term { get; }
        public GossipState State { get; }

        public Ping(ulong term, GossipState state)
        {
            Term = term;
            State = state;
        }
    }

    internal sealed class RingRequest : ISwimProtocol
    {
        public ulong Term { get; }
        public GossipState State { get; }
        public Member Target { get; }

        public RingRequest(ulong term, GossipState state, Member target)
        {
            Term = term;
            State = state;
            Target = target;
        }
    }

    internal sealed class NewMember : ISwimProtocol
    {
        public GossipState State { get; }
        public Member Member { get; }

        public NewMember(GossipState state, Member member)
        {
            State = state;
            Member = member;
        }
    }

    internal sealed class Join : ISwimProtocol
    {
        public GossipState State { get; }
        public Member Member { get; }

        public Join(GossipState state, Member member)
        {
            State = state;
            Member = member;
        }
    }
}