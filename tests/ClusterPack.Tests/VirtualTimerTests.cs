using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace ClusterPack.Tests
{
    public class VirtualTimerTests
    {
        private readonly VirtualTimer timer;
        private readonly ITestOutputHelper output;

        public VirtualTimerTests(ITestOutputHelper output)
        {
            this.output = output;
            this.timer = new VirtualTimer(DateTime.UtcNow);
        }

        [Fact]
        public void VirtualTimer_should_call_scheduled_actions_when_time_advances_to_call_time()
        {
            var called = false;
            timer.After(TimeSpan.FromMilliseconds(500), () => called = true);
            called.Should().Be(false, "scheduled action should not be called immediatelly");

            timer.Current += TimeSpan.FromMilliseconds(500);
            called.Should().Be(true, "scheduled action should be called once scheduled period passed");
        }

        [Fact]
        public void VirtualTimer_should_not_call_scheduled_actions_when_time_advances_before_call_time()
        {
            var called = false;
            timer.After(TimeSpan.FromMilliseconds(500), () => called = true);

            timer.Current += TimeSpan.FromMilliseconds(499);
            called.Should().Be(false, "scheduled action should not be called before scheduled period passeed");
        }

        [Fact]
        public void VirtualTimer_should_call_periodically_scheduler_calls_many_times()
        {
            var calls = new List<int>();
            var i = 0;
            var interval = TimeSpan.FromMilliseconds(500);
            timer.Every(interval, interval, () => calls.Add(i++));

            calls.Should().BeEmpty("periodically scheduled action should not be called immediately");

            timer.Current += TimeSpan.FromMilliseconds(600);
            calls.Should().BeEquivalentTo(1);
            timer.Current += TimeSpan.FromMilliseconds(400);
            calls.Should().BeEquivalentTo(1, 2);
        }

        [Fact]
        public void VirtualTimer_should_isolate_call_failures()
        {
            var called = false;
            var failure = false;
            var delay = TimeSpan.FromMilliseconds(400);
            timer.After(delay, () => { throw new Exception("boom!"); });
            timer.After(delay, () => called = true);
            timer.OnError += (sender, e) =>
            {
                if (e.Cause.Message == "boom!")
                    failure = true;
            };

            timer.Current += delay;
            called.Should().Be(true, "scheduled action failures should not infect other scheduled actions");
            failure.Should().Be(true, "scheduled action failures should trigger OnError event");
        }
    }
}