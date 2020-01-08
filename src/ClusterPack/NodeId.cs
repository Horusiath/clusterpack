using System;
using System.Runtime.CompilerServices;
using ClusterPack.Membership;

namespace ClusterPack
{
    /// <summary>
    /// Identifier of a particular <see cref="Member"/>.
    /// </summary>
    public readonly struct NodeId : IEquatable<NodeId>, IComparable<NodeId>
    {
        public Guid Value { get; }

        public NodeId(Guid value)
        {
            Value = value;
        }
        
        public override bool Equals(object obj) => obj is NodeId other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(NodeId other) => Value.Equals(other.Value);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(NodeId other) => Value.CompareTo(other.Value);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => Value.GetHashCode();
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => Value.ToString("N");
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NodeId a, NodeId b) => a.Equals(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NodeId a, NodeId b) => !(a == b);

    }
}