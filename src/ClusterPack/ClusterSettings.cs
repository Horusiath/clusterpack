using System;
using System.Net;
using ClusterPack.Transport;

namespace ClusterPack
{
    public sealed class ClusterSettings
    {
        public string Name { get; }
        public IPEndPoint Endpoint { get; }
        public ITransport Transport { get; }
        public ITimer Timer { get; }
        public TimeSpan TimerFrequency { get; }

        public ClusterSettings(string name, IPEndPoint endpoint, ITransport transport = null, ITimer timer = null, TimeSpan? timerFrequency = null)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name), "Node name must be provided");
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint), "Node listening endpoint must be provided");
                
            var frequency = timerFrequency ?? TimeSpan.FromMilliseconds(20);
            Name = name;
            Endpoint = endpoint;
            Transport = transport ?? new DefaultTransport();
            Timer = timer ?? new DefaultTimer(frequency);
            TimerFrequency = frequency;
        }

        public ClusterSettings(string name, string host, ushort port, ITransport transport = null, ITimer timer = null, TimeSpan? timerFrequency = null)
            : this(name, CreateEndPoint(host, port), transport, timer, timerFrequency)
        {
        }

        private static IPEndPoint CreateEndPoint(string host, ushort port)
        {
            if (host == "localhost") return new IPEndPoint(IPAddress.Loopback, port);
            if (IPAddress.TryParse(host, out var addr)) return new IPEndPoint(addr, port);
            
            throw new ArgumentException($"Couldn't resolve host [{host}]. Only localhost and IP addresses are supported.", nameof(host));
        }
    }
}