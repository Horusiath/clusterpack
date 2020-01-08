using System;
using System.Net;

namespace ClusterPack.Membership
{
    public sealed class Member : IEquatable<Member>, IComparable<Member>
    {
        public NodeId NodeId { get; }
        public IPEndPoint Endpoint { get; }

        public Member(NodeId nodeId, IPEndPoint endpoint)
        {
            NodeId = nodeId;
            Endpoint = endpoint;
        }

        public override bool Equals(object? obj) => obj is Member member && Equals(member);

        public override int GetHashCode() => NodeId.GetHashCode();

        public bool Equals(Member? other)
        {
            if (other is null) return false;
            return this.NodeId == other.NodeId;
        }

        public int CompareTo(Member? other)
        {
            if (other is null) return 1;
            return this.NodeId.CompareTo(other.NodeId);
        }

        public override string ToString() => $"Member(nodeId: {NodeId.ToString()}, endpoint: {Endpoint})";
    }
}