using System.Collections.Concurrent;
using System.Net;

namespace ClusterPack.Testing
{
    public sealed class TestNetwork
    {
        private readonly ConcurrentDictionary<EndPoint, TestTransport> nodes = new ConcurrentDictionary<EndPoint, TestTransport>();
        
        public TestTransport CreateTransport() => 
            new TestTransport(this);

        public bool TryGetTransport(IPEndPoint destination, out TestTransport transport)
        {
            return nodes.TryGetValue(destination, out transport);
        }

        internal void Register(EndPoint endpoint, TestTransport testTransport)
        {
            nodes.TryAdd(endpoint, testTransport);
        }
    }
}