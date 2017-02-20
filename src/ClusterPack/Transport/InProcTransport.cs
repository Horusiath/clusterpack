using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;

namespace ClusterPack.Transport
{
    internal struct InProcConnection : IEquatable<InProcConnection>
    {
        public readonly IPEndPoint Source;
        public readonly IPEndPoint Destination;

        public InProcConnection(IPEndPoint source, IPEndPoint destination)
        {
            Source = source;
            Destination = destination;
        }

        public bool Equals(InProcConnection other)
        {
            return Equals(Source, other.Source) && Equals(Destination, other.Destination);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is InProcConnection && Equals((InProcConnection) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Source != null ? Source.GetHashCode() : 0) * 397) ^ (Destination != null ? Destination.GetHashCode() : 0);
            }
        }
    }

    internal sealed class ConnectionState
    {
        /// <summary>
        /// Lag at which messages comming through that connection are slowed down.
        /// </summary>
        public TimeSpan? Delay { get; internal set; }

        /// <summary>
        /// Flag determining if a connection should behave as disconnected.
        /// </summary>
        public bool IsDisconnected { get; internal set; }
    }

    /// <summary>
    /// An fake transport layer that passed objects through memory. 
    /// Usefull for testing.
    /// </summary>
    public class InProcTransport : ITransport
    {
        private readonly ITimer timer;

        private readonly ConcurrentDictionary<InProcConnection, ConnectionState> connections = 
            new ConcurrentDictionary<InProcConnection, ConnectionState>();

        public InProcTransport(ITimer timer)
        {
            this.timer = timer;
        }

        /// <summary>
        /// Simulates a delay between two communicating endpoints. If 
        /// <paramref name="delay"/> was not provided, it resets a delay, 
        /// changing to instant mode.
        /// </summary>
        public void Delay(IPEndPoint x, IPEndPoint y, TimeSpan? delay = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Simulates reestablishing connection between two endpoints.
        /// </summary>
        public void Connect(IPEndPoint x, IPEndPoint y)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Simulates disconnection between two endpoints.
        /// </summary>
        public void Disconnect(IPEndPoint x, IPEndPoint y)
        {
            throw new NotImplementedException();
        }

        public void Listen(IPEndPoint endpoint)
        {
            throw new NotImplementedException();
        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }

    }
}