// using Grpc.Net.Client;
// using CalculatorServer;
// using Shared;

// namespace CalculatorClient.TestScripts
// {
//     public class ParallelClientTest
//     {
//         public static async Task RunParallelTest()
//         {
//             Console.WriteLine("🔄 Starting Parallel Client Test");
//             Console.WriteLine("================================");

//             // Create multiple client tasks
//             var tasks = new List<Task>();
            
//             for (int i = 1; i <= 3; i++)
//             {
//                 int clientId = i;
//                 tasks.Add(Task.Run(() => SimulateClient(clientId)));
//             }

//             // Wait for all clients to complete
//             await Task.WhenAll(tasks);
            
//             Console.WriteLine("\n✅ All parallel clients completed");
//         }

//         private static async Task SimulateClient(int clientId)
//         {
//             var clientClock = new VectorClock($"ParallelClient-{clientId}");
//             var servers = new[] { "http://localhost:5000", "http://localhost:5001", "http://localhost:5002" };
//             var random = new Random(clientId * 1000); // Different seed for each client

//             Console.WriteLine($"🚀 Client {clientId} started");

//             for (int operation = 1; operation <= 5; operation++)
//             {
//                 try
//                 {
//                     // Random server selection
//                     var serverUrl = servers[random.Next(servers.Length)];
                    
//                     using var channel = GrpcChannel.ForAddress(serverUrl, new GrpcChannelOptions
//                     {
//                         HttpHandler = CreateHttpHandler()
//                     });
                    
//                     var client = new Calculator.CalculatorClient(channel);
                    
//                     // Increment client clock
//                     clientClock.Increment();
                    
//                     // Random number and operation
//                     var number = random.Next(1, 20);
//                     var isSquare = random.NextDouble() > 0.5;
                    
//                     Console.WriteLine($"[Client-{clientId}] Operation {operation}: {(isSquare ? "Square" : "Cube")}({number}) on {serverUrl}");
                    
//                     if (isSquare)
//                     {
//                         var request = new CalculationRequest
//                         {
//                             Number = number,
//                             VectorClock = { clientClock.GetClock() }
//                         };
                        
//                         var response = await client.SquareAsync(request);
//                         clientClock.Merge(response.VectorClock.ToDictionary(x => x.Key, x => x.Value));
                        
//                         if (response.IsSuccess)
//                         {
//                             Console.WriteLine($"[Client-{clientId}] ✅ Square result: {response.Result}, Clock: {clientClock}");
//                         }
//                         else
//                         {
//                             Console.WriteLine($"[Client-{clientId}] ❌ Error: {response.ErrorMessage}, Clock: {clientClock}");
//                         }
//                     }
//                     else
//                     {
//                         var request = new CalculationRequest
//                         {
//                             Number = number,
//                             VectorClock = { clientClock.GetClock() }
//                         };
                        
//                         var response = await client.CubeAsync(request);
//                         clientClock.Merge(response.VectorClock.ToDictionary(x => x.Key, x => x.Value));
                        
//                         if (response.IsSuccess)
//                         {
//                             Console.WriteLine($"[Client-{clientId}] ✅ Cube result: {response.Result}, Clock: {clientClock}");
//                         }
//                         else
//                         {
//                             Console.WriteLine($"[Client-{clientId}] ❌ Error: {response.ErrorMessage}, Clock: {clientClock}");
//                         }
//                     }
                    
//                     // Random delay between operations
//                     await Task.Delay(random.Next(1000, 3000));
                    
//                 }
//                 catch (Exception ex)
//                 {
//                     Console.WriteLine($"[Client-{clientId}] ❌ Connection error: {ex.Message}");
//                 }
//             }
            
//             Console.WriteLine($"🏁 Client {clientId} completed with final clock: {clientClock}");
//         }
        
//         private static HttpClientHandler CreateHttpHandler()
//         {
//             var handler = new HttpClientHandler();
//             handler.ServerCertificateCustomValidationCallback = 
//                 (httpRequestMessage, cert, cetChain, policyErrors) => true;
//             return handler;
//         }
//     }
// }



using Grpc.Net.Client;
using CalculatorServer;
using Shared;

namespace CalculatorClient.TestScripts
{
    public class ParallelClientTest
    {
        public static async Task RunParallelTest()
        {
            Console.WriteLine("🔄 Interactive Parallel Client Test");
            Console.WriteLine("====================================");
            Console.WriteLine();

            // Get number of clients
            int clientCount = GetClientCount();
            
            // Get operations configuration for each client
            var clientConfigs = new List<ClientConfiguration>();
            for (int i = 1; i <= clientCount; i++)
            {
                Console.WriteLine($"\n--- Configuration for Client {i} ---");
                var config = GetClientConfiguration(i);
                clientConfigs.Add(config);
            }

            Console.WriteLine("\n🚀 Starting parallel execution...");
            Console.WriteLine("================================\n");

            // Create multiple client tasks
            var tasks = new List<Task>();
            
            for (int i = 0; i < clientCount; i++)
            {
                int clientIndex = i;
                var config = clientConfigs[i];
                tasks.Add(Task.Run(() => SimulateClient(config)));
            }

            // Wait for all clients to complete
            await Task.WhenAll(tasks);
            
            Console.WriteLine("\n✅ All parallel clients completed!");
            Console.WriteLine("===================================");
            
            // Show summary
            PrintSummary(clientConfigs);
        }

        private static int GetClientCount()
        {
            while (true)
            {
                Console.Write("Enter number of parallel clients (1-10): ");
                var input = Console.ReadLine();
                
                if (int.TryParse(input, out int count) && count >= 1 && count <= 10)
                {
                    return count;
                }
                
                Console.WriteLine("❌ Invalid input! Please enter a number between 1 and 10.");
            }
        }

        private static ClientConfiguration GetClientConfiguration(int clientId)
        {
            var config = new ClientConfiguration
            {
                ClientId = clientId,
                Operations = new List<OperationConfig>()
            };

            // Get number of operations for this client
            while (true)
            {
                Console.Write($"Number of operations for Client {clientId} (1-20): ");
                var input = Console.ReadLine();
                
                if (int.TryParse(input, out int opCount) && opCount >= 1 && opCount <= 20)
                {
                    config.OperationCount = opCount;
                    break;
                }
                
                Console.WriteLine("❌ Invalid input! Please enter a number between 1 and 20.");
            }

            // Configure each operation
            for (int op = 1; op <= config.OperationCount; op++)
            {
                Console.WriteLine($"\n  Operation {op} for Client {clientId}:");
                var opConfig = GetOperationConfiguration(clientId, op);
                config.Operations.Add(opConfig);
            }

            return config;
        }

        private static OperationConfig GetOperationConfiguration(int clientId, int opNumber)
        {
            var opConfig = new OperationConfig { OperationNumber = opNumber };

            // Select operation type
            Console.WriteLine("  Select operation:");
            Console.WriteLine("    1. Square");
            Console.WriteLine("    2. Cube");
            Console.WriteLine("    3. SlowMultiply");
            Console.Write("  Choice (1-3): ");
            
            var choice = Console.ReadLine();
            opConfig.OperationType = choice switch
            {
                "1" => "Square",
                "2" => "Cube",
                "3" => "SlowMultiply",
                _ => "Square" // Default
            };

            // Get number(s)
            Console.Write($"  Enter number for {opConfig.OperationType}: ");
            if (double.TryParse(Console.ReadLine(), out double num1))
            {
                opConfig.Number1 = num1;
            }
            else
            {
                opConfig.Number1 = 5; // Default
            }

            if (opConfig.OperationType == "SlowMultiply")
            {
                Console.Write("  Enter second number for multiplication: ");
                if (double.TryParse(Console.ReadLine(), out double num2))
                {
                    opConfig.Number2 = num2;
                }
                else
                {
                    opConfig.Number2 = 3; // Default
                }
            }

            // Select server
            Console.WriteLine("  Select server:");
            Console.WriteLine("    1. Server 1 (5000)");
            Console.WriteLine("    2. Server 2 (5001)");
            Console.WriteLine("    3. Server 3 (5002)");
            Console.WriteLine("    4. Random server");
            Console.Write("  Choice (1-4): ");
            
            var serverChoice = Console.ReadLine();
            opConfig.ServerChoice = serverChoice switch
            {
                "1" => "http://localhost:5000",
                "2" => "http://localhost:5001",
                "3" => "http://localhost:5002",
                _ => "random"
            };

            return opConfig;
        }

        private static async Task SimulateClient(ClientConfiguration config)
        {
            var clientClock = new VectorClock($"ParallelClient-{config.ClientId}");
            var servers = new[] { "http://localhost:5000", "http://localhost:5001", "http://localhost:5002" };
            var random = new Random(config.ClientId * 1000);

            Console.WriteLine($"\n🚀 Client {config.ClientId} started with {config.OperationCount} operations");

            foreach (var opConfig in config.Operations)
            {
                try
                {
                    // Determine server
                    var serverUrl = opConfig.ServerChoice == "random" 
                        ? servers[random.Next(servers.Length)] 
                        : opConfig.ServerChoice;
                    
                    using var channel = GrpcChannel.ForAddress(serverUrl, new GrpcChannelOptions
                    {
                        HttpHandler = CreateHttpHandler()
                    });
                    
                    var client = new Calculator.CalculatorClient(channel);
                    
                    // Increment client clock
                    clientClock.Increment();
                    
                    Console.WriteLine($"[Client-{config.ClientId}] Operation {opConfig.OperationNumber}: {opConfig.OperationType}({opConfig.Number1}{(opConfig.OperationType == "SlowMultiply" ? $", {opConfig.Number2}" : "")}) on {serverUrl}");
                    
                    // Execute operation based on type
                    switch (opConfig.OperationType)
                    {
                        case "Square":
                            await ExecuteSquare(client, clientClock, opConfig, config.ClientId);
                            break;
                        case "Cube":
                            await ExecuteCube(client, clientClock, opConfig, config.ClientId);
                            break;
                        case "SlowMultiply":
                            await ExecuteSlowMultiply(client, clientClock, opConfig, config.ClientId);
                            break;
                    }
                    
                    // Small delay between operations
                    await Task.Delay(random.Next(500, 1500));
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Client-{config.ClientId}] ❌ Connection error: {ex.Message}");
                }
            }
            
            Console.WriteLine($"\n🏁 Client {config.ClientId} completed with final clock: {clientClock}");
        }

        private static async Task ExecuteSquare(Calculator.CalculatorClient client, VectorClock clientClock, OperationConfig opConfig, int clientId)
        {
            var request = new CalculationRequest
            {
                Number = opConfig.Number1,
                VectorClock = { clientClock.GetClock() }
            };
            
            var response = await client.SquareAsync(request);
            clientClock.Merge(response.VectorClock.ToDictionary(x => x.Key, x => x.Value));
            
            if (response.IsSuccess)
            {
                Console.WriteLine($"[Client-{clientId}] ✅ Square result: {response.Result}, Clock: {clientClock}");
            }
            else
            {
                Console.WriteLine($"[Client-{clientId}] ❌ Error: {response.ErrorMessage}, Clock: {clientClock}");
            }
        }

        private static async Task ExecuteCube(Calculator.CalculatorClient client, VectorClock clientClock, OperationConfig opConfig, int clientId)
        {
            var request = new CalculationRequest
            {
                Number = opConfig.Number1,
                VectorClock = { clientClock.GetClock() }
            };
            
            var response = await client.CubeAsync(request);
            clientClock.Merge(response.VectorClock.ToDictionary(x => x.Key, x => x.Value));
            
            if (response.IsSuccess)
            {
                Console.WriteLine($"[Client-{clientId}] ✅ Cube result: {response.Result}, Clock: {clientClock}");
            }
            else
            {
                Console.WriteLine($"[Client-{clientId}] ❌ Error: {response.ErrorMessage}, Clock: {clientClock}");
            }
        }

        private static async Task ExecuteSlowMultiply(Calculator.CalculatorClient client, VectorClock clientClock, OperationConfig opConfig, int clientId)
        {
            var request = new MultiplyRequest
            {
                Number1 = opConfig.Number1,
                Number2 = opConfig.Number2,
                VectorClock = { clientClock.GetClock() }
            };
            
            var response = await client.SlowMultiplyAsync(request);
            clientClock.Merge(response.VectorClock.ToDictionary(x => x.Key, x => x.Value));
            
            if (response.IsSuccess)
            {
                Console.WriteLine($"[Client-{clientId}] ✅ SlowMultiply result: {response.Result}, Clock: {clientClock}");
            }
            else
            {
                Console.WriteLine($"[Client-{clientId}] ❌ Error: {response.ErrorMessage}, Clock: {clientClock}");
            }
        }

        private static void PrintSummary(List<ClientConfiguration> configs)
        {
            Console.WriteLine("\n📊 Execution Summary:");
            Console.WriteLine("====================");
            
            int totalOperations = configs.Sum(c => c.OperationCount);
            
            Console.WriteLine($"Total Clients: {configs.Count}");
            Console.WriteLine($"Total Operations: {totalOperations}");
            Console.WriteLine();
            
            foreach (var config in configs)
            {
                Console.WriteLine($"Client {config.ClientId}: {config.OperationCount} operations");
                foreach (var op in config.Operations)
                {
                    var opDesc = op.OperationType == "SlowMultiply" 
                        ? $"{op.OperationType}({op.Number1}, {op.Number2})"
                        : $"{op.OperationType}({op.Number1})";
                    
                    var server = op.ServerChoice == "random" ? "Random" : op.ServerChoice.Replace("http://localhost:", "Port ");
                    Console.WriteLine($"  Op {op.OperationNumber}: {opDesc} on {server}");
                }
                Console.WriteLine();
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

    // Configuration classes
    public class ClientConfiguration
    {
        public int ClientId { get; set; }
        public int OperationCount { get; set; }
        public List<OperationConfig> Operations { get; set; } = new();
    }

    public class OperationConfig
    {
        public int OperationNumber { get; set; }
        public string OperationType { get; set; } = "Square";
        public double Number1 { get; set; }
        public double Number2 { get; set; }
        public string ServerChoice { get; set; } = "random";
    }
}