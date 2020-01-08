using System;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace ClusterPack.Tests
{
    public class XUnitLogger<T> : ILogger<T>
    {
        private readonly ITestOutputHelper output;
        private readonly string name;

        public XUnitLogger(ITestOutputHelper output, string name)
        {
            this.output = output;
            this.name = name;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var fmt = formatter(state, exception);
            output.WriteLine($"[{name}] [{logLevel}] {fmt}");
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }
    }

    public static class XUnitLoggerExtensions
    {
        public static ILogger<T> LoggerFor<T>(this ITestOutputHelper output, string name) => new XUnitLogger<T>(output, name);
        public static ILogger<T> LoggerFor<T>(this ITestOutputHelper output) => new XUnitLogger<T>(output, typeof(T).Name);
    }
}