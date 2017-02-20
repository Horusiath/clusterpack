using System;
using System.Net;

namespace ClusterPack.Transport
{
    /// <summary>
    /// An interface that abstracts underlying transport mechanism.
    /// </summary>
    public interface ITransport : IDisposable
    {
        void Listen(IPEndPoint endpoint);
        void Shutdown();
    }
}