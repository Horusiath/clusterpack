using System;
using System.Net;

namespace ClusterPack
{
    public enum MemberState
    {
        Alive = 1,
        Suspect = 2,
        Dead = 3
    }

    public sealed class MemberInfo : IEquatable<MemberInfo>
    {
        /// <summary>
        /// Name of the member info. Must be unique in scope of the cluster.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Endpoint where this member info process can be located.
        /// </summary>
        public IPEndPoint Endpoint { get; }

        /// <summary>
        /// Current state of the member node.
        /// </summary>
        public MemberState State { get; }

        /// <summary>
        /// Incarnation of the member info. It's monotonically increasing value.
        /// </summary>
        public int Incarnation { get; }

        /// <summary>
        /// Timestamp determining, how old is current member info snapshot status.
        /// </summary>
        public DateTime LastChanged { get; }

        /// <summary>
        /// Metadata gossiped with this member info.
        /// </summary>
        public byte[] Metadata { get; }

        public MemberInfo(string name, IPEndPoint endpoint, MemberState state, int incarnation, DateTime lastChanged, byte[] metadata)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name), "Member info name must be provided");
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint), "Member info endpoint must be provided");

            Name = name;
            Endpoint = endpoint;
            State = state;
            Incarnation = incarnation;
            LastChanged = lastChanged;
            Metadata = metadata;
        }

        public bool Equals(MemberInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name) 
                && Equals(Endpoint, other.Endpoint) 
                && State == other.State 
                && Incarnation == other.Incarnation 
                && LastChanged.Equals(other.LastChanged) 
                && Equals(Metadata, other.Metadata);
        }

        public override bool Equals(object obj) => obj is MemberInfo && Equals((MemberInfo) obj);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Name.GetHashCode();
                hashCode = (hashCode * 397) ^ Endpoint.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) State;
                hashCode = (hashCode * 397) ^ Incarnation;
                hashCode = (hashCode * 397) ^ LastChanged.GetHashCode();
                hashCode = (hashCode * 397) ^ (Metadata != null ? Metadata.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
