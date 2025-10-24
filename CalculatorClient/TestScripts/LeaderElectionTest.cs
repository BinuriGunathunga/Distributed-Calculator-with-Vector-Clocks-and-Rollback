using Grpc.Net.Client;
using CalculatorServer;
using Shared;

namespace CalculatorClient.TestScripts
{
    public class LeaderElectionTest
    {
        private static readonly string[] BackupServers = {
            "http://localhost:5001",
            "http://localhost:5002"
        };

        public static async Task RunLeaderElectionTest()
        {
            Console.WriteLine("🗳️ Leader Election Test");
            Console.WriteLine("=======================");

            while (true)
            {
                try
                {
                    Console.WriteLine("\n1. Test with leader");
                    Console.WriteLine("2. Simulate leader failure");
                    Console.WriteLine("3. Exit");
                    Console.Write("Choice: ");
                    
                    var choice = Console.ReadLine();
                    
                    switch (choice)
                    {
                        case "1":
                            await TestWithLeader();
                            break;
                        case "2":
                            await SimulateLeaderFailure();
                            break;
                        case "3":
                            return;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error: {ex.Message}");
                }
            }
        }

        private static async Task TestWithLeader()
        {
            var leader = await LeaderElection.GetCurrentLeader();
            
            if (leader == null)
            {
                Console.WriteLine("❌ No leader found!");
                return;
            }

            Console.WriteLine($"👑 Current leader: {leader.ServerId} at {leader.Address}");
            
            try
            {
                var result = await SendToServer(leader.Address, 25);
                Console.WriteLine($"✅ Leader responded: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Leader failed: {ex.Message}");
                await TryBackupServers();
            }
        }

        private static async Task SimulateLeaderFailure()
        {
            var leader = await LeaderElection.GetCurrentLeader();
            
            if (leader != null)
            {
                await LeaderElection.MarkLeaderDown(leader.ServerId);
                Console.WriteLine($"💀 Simulated failure of leader: {leader.ServerId}");
                
                // Try to elect new leader from backups
                foreach (var backup in BackupServers)
                {
                    try
                    {
                        var result = await SendToServer(backup, 10);
                        await LeaderElection.ElectLeader($"Server-{backup}", backup);
                        Console.WriteLine($"👑 New leader elected: {backup}");
                        break;
                    }
                    catch
                    {
                        Console.WriteLine($"❌ Backup {backup} also failed");
                    }
                }
            }
        }

        private static async Task TryBackupServers()
        {
            Console.WriteLine("🔄 Trying backup servers...");
            
            foreach (var backup in BackupServers)
            {
                try
                {
                    var result = await SendToServer(backup, 15);
                    Console.WriteLine($"✅ Backup server {backup} responded: {result}");
                    
                    // Elect this server as new leader
                    await LeaderElection.ElectLeader($"Server-{backup}", backup);
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Backup {backup} failed: {ex.Message}");
                }
            }
            
            Console.WriteLine("💀 All servers are down!");
        }

        private static async Task<double> SendToServer(string serverUrl, double number)
        {
            using var channel = GrpcChannel.ForAddress(serverUrl, new GrpcChannelOptions
            {
                HttpHandler = CreateHttpHandler()
            });
            
            var client = new Calculator.CalculatorClient(channel);
            var clientClock = new VectorClock("LeaderTestClient");
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