using System.Net;
using System.Net.Sockets;

namespace ClusterPack.Transport
{
    /// <summary>
    /// Default transport layer, operating directly on TCP sockets.
    /// </summary>
    public sealed class DefaultTransport : ITransport
    {
        private readonly Socket socket;
        
        public DefaultTransport(
            AddressFamily addressFamily = AddressFamily.InterNetwork, 
            ProtocolType protocolType = ProtocolType.IPv4)
        {
            this.socket = new Socket(addressFamily, SocketType.Stream, protocolType);
        }

        public void Dispose()
        {
            this.socket.Dispose();
        }

        public void Listen(IPEndPoint endpoint)
        {
            this.socket.Bind(endpoint);
            this.socket.Listen(100);
        }

        public void Shutdown()
        {
            throw new System.NotImplementedException();
        }
    }
}