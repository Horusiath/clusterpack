using System;

namespace ClusterPack.Testing
{
    public class TestTransportException : Exception
    {
        public TestTransportException(string message) : base(message)
        {
        }
    }
}