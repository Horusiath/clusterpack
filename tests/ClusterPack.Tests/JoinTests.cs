using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace ClusterPack.Tests
{
    public class JoinTests : AbstractSpec
    {
        private readonly ITestOutputHelper output;
        private readonly Node n1;
        private readonly Node n2;

        public JoinTests(ITestOutputHelper output)
        {
            this.output = output;
            this.n1 = new Node(TestSettings("n1", "localhost", 1337));
            this.n2 = new Node(TestSettings("n2", "localhost", 1338));
        }

        [Fact]
        public async Task Node_should_be_able_to_init_cluster()
        {
            var joinedName = default(string);
            n1.OnJoined += async joined => joinedName = joined.Member.Name;
            await n1.Join();
            joinedName.Should().Be("n1");
        }

        [Fact]
        public async Task Node_should_be_able_to_join_the_cluster()
        {
            var nodes = new List<string>();
            n1.OnJoined += async joined => nodes.Add(joined.Member.Name);
            await n1.Join();
            await n2.Join(n1.Endpoint);

            nodes.Should().BeEquivalentTo(n1.Name, n2.Name);
            n1.KnownMembers.Count().Should().Be(2);
            n1.KnownMembers.ShouldAllBeEquivalentTo(n2.KnownMembers);
        }

        [Fact]
        public void Node_should_be_able_to_cancel_join_procedure()
        {
            throw new NotImplementedException();
        }
    }
}