using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ClusterPack.Discovery
{
    public interface IDiscovery
    {
        ValueTask<IEnumerable<EndPoint>> DiscoverNodesAsync(CancellationToken cancellationToken);
    }
    
    public sealed class StaticDiscovery : IDiscovery
    {
        private readonly IEnumerable<EndPoint> endpoints;

        public StaticDiscovery(IEnumerable<EndPoint> endpoints)
        {
            this.endpoints = endpoints;
        }

        public ValueTask<IEnumerable<EndPoint>> DiscoverNodesAsync(CancellationToken cancellationToken) => 
            new ValueTask<IEnumerable<EndPoint>>(endpoints);
    }
}