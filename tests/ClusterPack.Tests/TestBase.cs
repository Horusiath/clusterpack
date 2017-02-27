using System;
using System.Net;
using ClusterPack.Transport;

namespace ClusterPack.Tests
{
    public class TestBase : IDisposable
    {
        protected readonly InProcTransport Transport;
        protected readonly VirtualTimer Timer;

        public TestBase()
        {
            this.Timer = new VirtualTimer(DateTime.UtcNow);
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