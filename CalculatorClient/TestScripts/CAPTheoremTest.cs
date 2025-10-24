using Grpc.Net.Client;
using CalculatorServer;
using Shared;
using Grpc.Core;

namespace CalculatorClient.TestScripts
{
    public class CAPTheoremTest
    {
        public static async Task RunCAPTest()
        {
            Console.WriteLine("📦 CAP Theorem Test");
            Console.WriteLine("==================");
            
            var servers = new[]
            {
                "http://localhost:5000",
                "http://localhost:5001", 
                "http://localhost:5002"
            };

            Console.WriteLine("Phase 1: Normal Operation (All servers available)");
            await TestAllServersAvailable(servers);
            
            Console.WriteLine("\nPhase 2: Simulate Network Partition");
            NetworkPartition.PartitionServer("Server-1");
            await TestWithPartition(servers);
            
            Console.WriteLine("\nPhase 3: Heal Partition");
            NetworkPartition.HealPartition("Server-1");
            await TestConsistencyRecovery(servers);
            
            Console.WriteLine("\n📋 CAP Analysis Complete");
        }

        private static async Task TestAllServersAvailable(string[] servers)
        {
            foreach (var server in servers)
            {
                try
                {
                    var result = await SendCalculation(server, 5);
                    Console.WriteLine($"✅ {server}: Available, Result={result}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ {server}: Unavailable, Error={ex.Message}");
                }
            }
        }

        private static async Task TestWithPartition(string[] servers)
        {
            foreach (var server in servers)
            {
                try
                {
                    var result = await SendCalculation(server, 7);
                    Console.WriteLine($"✅ {server}: Still available during partition, Result={result}");
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    Console.WriteLine($"🚫 {server}: Partitioned - unavailable but consistent");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ {server}: Error={ex.Message}");
                }
            }
        }

        private static async Task TestConsistencyRecovery(string[] servers)
        {
            // Test if all servers return to consistent state
            foreach (var server in servers)
            {
                try
                {
                    var result = await SendCalculation(server, 10);
                    Console.WriteLine($"🔄 {server}: Recovered, Result={result}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ {server}: Recovery failed, Error={ex.Message}");
                }
            }
        }

        private static async Task<double> SendCalculation(string serverUrl, double number)
        {
            using var channel = GrpcChannel.ForAddress(serverUrl, new GrpcChannelOptions
            {
                HttpHandler = CreateHttpHandler()
            });
            
            var client = new Calculator.CalculatorClient(channel);
            var clientClock = new VectorClock($"CAPTestClient");
            clientClock.Increment();
            
            var request = new CalculationRequest
            {
                Number = number,
                VectorClock = { clientClock.GetClock() }
            };

            var response = await client.SquareAsync(request);
            
            if (response.IsSuccess)
            {
                return response.Result;
            }
            else
            {
                throw new Exception(response.ErrorMessage);
            }
        }
        
        private static HttpClientHandler CreateHttpHandler()
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = 
                (httpRequestMessage, cert, cetChain, policyErrors) => true;
            return handler;
        }
    }
}