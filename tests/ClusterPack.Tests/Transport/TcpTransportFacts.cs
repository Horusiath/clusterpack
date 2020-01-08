using System;
using System.Buffers;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClusterPack.Transport;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace ClusterPack.Tests.Transport
{
    [Collection("TcpTransportFacts")]
    public class TcpTransportFacts : IDisposable
    {
        private readonly ITestOutputHelper output;
        private readonly TcpTransport local;
        private readonly IPEndPoint localEndpoint = new IPEndPoint(IPAddress.Loopback, 17001);

        public TcpTransportFacts(ITestOutputHelper output)
        {
            this.output = output;
            this.local = new TcpTransport(output.LoggerFor<TcpTransport>("local"));
        }

        public void Dispose()
        {
            local.DisposeAsync().GetAwaiter().GetResult();
        }

        [Fact]
        public async Task TCP_transport_should_receive_messages_from_remote_endpoint()
        {
            using var cts = new CancellationTokenSource(10_000);
            var token = cts.Token;
            //var token = CancellationToken.None;
            var expected = Enumerable.Range(0, 10)
                .Select(i => "hello " + i.ToString())
                .ToArray();

            // simulate two communicating nodes using two separate Tasks
            var localProcess = Task.Run(async () =>
            {
                var messages = local.BindAsync(localEndpoint, token);
                var i = 0;
                await foreach (var message in messages.WithCancellation(token))
                {
                    using var incoming = message;
                    var payload = Encoding.UTF8.GetString(incoming.Payload.FirstSpan);

                    payload.Should().Be(expected[i]);
                    i++;
                }
            }, token);

            var remoteProcess = Task.Run(async () =>
            {
                var remote = new TcpTransport(output.LoggerFor<TcpTransport>("remote"));
                foreach (var message in expected)
                {
                    var payload = new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(message));
                    await remote.SendAsync(localEndpoint, payload, token);
                }
            }, token);

            await Task.WhenAll(localProcess, remoteProcess);
        }
    }
}