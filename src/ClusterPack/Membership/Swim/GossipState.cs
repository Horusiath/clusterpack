using System;
using System.Collections.Immutable;

namespace ClusterPack.Membership.Swim
{
    internal readonly struct GossipState : IEquatable<GossipState>
    {
        public ImmutableSortedSet<Member> Members { get; }

        public GossipState(ImmutableSortedSet<Member> members)
        {
            Members = members;
        }

        public GossipState Merge(in GossipState other) => 
            new GossipState(Members.Union(other.Members));
        
        public GossipState AddMember(Member member) =>
            new GossipState(Members.Add(member));
        
        public GossipState RemoveMember(Member member) =>
            new GossipState(Members.Remove(member));
        
        public GossipStateDiff Diff(in GossipState other) =>
            new GossipStateDiff(
                localOnly: this.Members.Except(other.Members),
                remoteOnly: other.Members.Except(this.Members));

        public bool Equals(GossipState other) => 
            Members.SetEquals(other.Members);

        public override bool Equals(object? obj) => obj is GossipState other && Equals(other);

        public override int GetHashCode()
        {
            var hasher = new HashCode();
            foreach (var member in Members)
            {
                hasher.Add(member.GetHashCode());
            }

            return hasher.ToHashCode();
        }

        public override string ToString() => $"GossipState(member: {String.Join(", ", Members)})";
    }

    internal readonly struct GossipStateDiff
    {
        /// <summary>
        /// Members present only from local point of view (not seen by remote). 
        /// </summary>
        public ImmutableSortedSet<Member> LocalOnly { get; }
        
        /// <summary>
        /// Members present only from remote point of view (not seen by local).
        /// </summary>
        public ImmutableSortedSet<Member> RemoteOnly { get; }

        public GossipStateDiff(ImmutableSortedSet<Member> localOnly, ImmutableSortedSet<Member> remoteOnly)
        {
            LocalOnly = localOnly;
            RemoteOnly = remoteOnly;
        }
    }
}