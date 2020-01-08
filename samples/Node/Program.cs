using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClusterPack.Transport;
using CommandLine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace Node
{
    public class Options
    {
        [Option("endpoint", Required = true, HelpText = "Endpoint in format [host:port] on which current node will start listening")]
        public string LocalEndpoint { get; set; }
        
        [Option("seed-nodes", Required = false, HelpText = "List of remote endpoints in format [host:port] to which you can connect to")]
        public IEnumerable<string> SeedNodes { get; set; }
    }
    
    class Program
    {
        static async Task Main(string[] args)
        {
            var options = ((Parsed<Options>)Parser.Default.ParseArguments<Options>(args)).Value;
            var token = CancellationToken.None;
            
            using var loggerFactory = new LoggerFactory();
            await using var transport = new TcpTransport(loggerFactory.CreateLogger("TCP local"));

            var messages = transport.BindAsync(IPEndPoint.Parse(options.LocalEndpoint), token);

            var reader = Task.Run(async () =>
            {
                await foreach (var incoming in messages.WithCancellation(token))
                {
                    using var msg = incoming;
                    var text = Encoding.UTF8.GetString(msg.Payload.FirstSpan);
                    Console.WriteLine($"Received message from '{msg.Endpoint}': {text}");
                }
            });
            
            Console.WriteLine($"Local host started at {transport.LocalEndpoint} ...");

            var endpoints = new List<IPEndPoint>();
            foreach (var hostWithPort in options.SeedNodes)
            {
                var endpoint = IPEndPoint.Parse(hostWithPort);
                endpoints.Add(endpoint);
            }
            
            Console.WriteLine("Ready to receive messages...");
            var userText = "";
            do
            {
                Console.Write("> ");
                userText = Console.ReadLine();
                var bytes = new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(userText));

                foreach (var endpoint in endpoints)
                {
                    await transport.SendAsync(endpoint, bytes, token);
                    Console.WriteLine($"Sent '{userText}' to {endpoint}...");
                }

            } while (userText != "quit");
        }
    }
}