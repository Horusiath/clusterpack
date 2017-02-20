using System.Threading.Tasks;

namespace ClusterPack
{
    public delegate Task ClusterEventHandler<in T>(T e);

    public class MemberStateChanged
    {
        public MemberStateChanged(MemberInfo member)
        {
            Member = member;
        }

        public MemberInfo Member { get; }
    }

    public sealed class MemberJoined : MemberStateChanged
    {
        public MemberJoined(MemberInfo member) : base(member)
        {
        }
    }

    public sealed class MemberLeft : MemberStateChanged
    {
        public MemberLeft(MemberInfo member) : base(member)
        {
        }
    }

    public sealed class IncomingMessage
    {
        public MemberInfo Sender { get; }
        public byte[] Payload { get; }

        public IncomingMessage(MemberInfo sender, byte[] payload)
        {
            Sender = sender;
            Payload = payload;
        }
    }
}