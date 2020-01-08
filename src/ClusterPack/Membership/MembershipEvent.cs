namespace ClusterPack.Membership
{
    public enum EventType
    {
        Joined,
        Left,
        Suspected,
        Unreachable
    }
    
    public readonly struct MembershipEvent
    {
        public EventType EventType { get; }
        public Member Member { get; }

        public MembershipEvent(EventType eventType, Member member)
        {
            EventType = eventType;
            Member = member;
        }
    }
}