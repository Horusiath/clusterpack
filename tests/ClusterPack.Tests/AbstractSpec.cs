using System;
using System.Net;
using ClusterPack.Transport;

namespace ClusterPack.Tests
{
    public class AbstractSpec : IDisposable
    {
        protected readonly InProcTransport Transport;
        protected readonly VirtualTimer Timer;

        public AbstractSpec()
        {
            this.Timer = new VirtualTimer();
            this.Transport = new InProcTransport(this.Timer);
        }

        public ClusterSettings TestSettings(string name, string host, ushort port)
        {
            return new ClusterSettings(name, host, port, Transport, Timer);
        }

        public void Dispose()
        {
            Transport?.Dispose();
            Timer?.Dispose();
        }
    }
}