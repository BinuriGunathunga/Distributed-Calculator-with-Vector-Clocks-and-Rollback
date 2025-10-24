// using Grpc.Net.Client;
// using CalculatorServer;
// using Shared;

// class Program
// {
//     private static VectorClock _clientClock = new VectorClock($"Client-{Environment.MachineName}");
//     private static List<string> _serverAddresses = new List<string>
//     {
//         "https://localhost:5000",
//         "https://localhost:5001",
//         "https://localhost:5002"
//     };

//     static async Task Main(string[] args)
//     {
//         Console.WriteLine("🧮 Distributed Calculator Client");
//         Console.WriteLine("==================================");

//         while (true)
//         {
//             try
//             {
//                 Console.WriteLine("\nAvailable operations:");
//                 Console.WriteLine("1. Square");
//                 Console.WriteLine("2. Cube");
//                 Console.WriteLine("3. SlowMultiply");
//                 Console.WriteLine("4. Exit");
//                 Console.Write("\nSelect operation (1-4): ");

//                 var choice = Console.ReadLine();
//                 if (choice == "4") break;

//                 var serverAddress = SelectServer();
//                 if (serverAddress == null) continue;

//                 using var channel = GrpcChannel.ForAddress(serverAddress);
//                 var client = new Calculator.CalculatorClient(channel);

//                 switch (choice)
//                 {
//                     case "1":
//                         await HandleSquare(client);
//                         break;
//                     case "2":
//                         await HandleCube(client);
//                         break;
//                     case "3":
//                         await HandleSlowMultiply(client);
//                         break;
//                     default:
//                         Console.WriteLine("Invalid choice!");
//                         continue;
//                 }

//                 Console.WriteLine($"\n🕰️  Current client vector clock: {_clientClock}");
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"❌ Error: {ex.Message}");
//             }
//         }
//     }

//     private static string? SelectServer()
//     {
//         Console.WriteLine("\nAvailable servers:");
//         for (int i = 0; i < _serverAddresses.Count; i++)
//         {
//             Console.WriteLine($"{i + 1}. {_serverAddresses[i]}");
//         }
        
//         Console.Write("Select server (1-3): ");
//         if (int.TryParse(Console.ReadLine(), out var serverChoice) && 
//             serverChoice >= 1 && serverChoice <= _serverAddresses.Count)
//         {
//             return _serverAddresses[serverChoice - 1];
//         }
        
//         Console.WriteLine("Invalid server selection!");
//         return null;
//     }

//     private static async Task HandleSquare(Calculator.CalculatorClient client)
//     {
//         Console.Write("Enter number to square: ");
//         if (double.TryParse(Console.ReadLine(), out var number))
//         {
//             await ExecuteWithRetry(async () =>
//             {
//                 _clientClock.Increment();
                
//                 var request = new CalculationRequest
//                 {
//                     Number = number,
//                     VectorClock = { _clientClock.GetClock() }
//                 };

//                 var response = await client.SquareAsync(request);
//                 return (response.IsSuccess, response.Result, response.ErrorMessage, 
//                        response.VectorClock.ToDictionary(x => x.Key, x => x.Value));
//             });
//         }
//     }

//     private static async Task HandleCube(Calculator.CalculatorClient client)
//     {
//         Console.Write("Enter number to cube: ");
//         if (double.TryParse(Console.ReadLine(), out var number))
//         {
//             await ExecuteWithRetry(async () =>
//             {
//                 _clientClock.Increment();
                
//                 var request = new CalculationRequest
//                 {
//                     Number = number,
//                     VectorClock = { _clientClock.GetClock() }
//                 };

//                 var response = await client.CubeAsync(request);
//                 return (response.IsSuccess, response.Result, response.ErrorMessage,
//                        response.VectorClock.ToDictionary(x => x.Key, x => x.Value));
//             });
//         }
//     }

//     private static async Task HandleSlowMultiply(Calculator.CalculatorClient client)
//     {
//         Console.Write("Enter first number: ");
//         if (!double.TryParse(Console.ReadLine(), out var num1)) return;
        
//         Console.Write("Enter second number: ");
//         if (!double.TryParse(Console.ReadLine(), out var num2)) return;

//         await ExecuteWithRetry(async () =>
//         {
//             _clientClock.Increment();
            
//             var request = new MultiplyRequest
//             {
//                 Number1 = num1,
//                 Number2 = num2,
//                 VectorClock = { _clientClock.GetClock() }
//             };

//             var response = await client.SlowMultiplyAsync(request);
//             return (response.IsSuccess, response.Result, response.ErrorMessage,
//                    response.VectorClock.ToDictionary(x => x.Key, x => x.Value));
//         });
//     }

//     private static async Task ExecuteWithRetry(Func<Task<(bool success, double result, string error, Dictionary<string, int> vectorClock)>> operation)
//     {
//         const int maxRetries = 3;
//         int attempt = 0;
        
//         // Save client clock state for potential rollback
//         _clientClock.SaveState();

//         while (attempt < maxRetries)
//         {
//             attempt++;
//             try
//             {
//                 var (success, result, error, vectorClock) = await operation();
                
//                 // Merge server's vector clock
//                 _clientClock.Merge(vectorClock);
                
//                 if (success)
//                 {
//                     Console.WriteLine($"✅ Result: {result}");
//                     return;
//                 }
//                 else
//                 {
//                     Console.WriteLine($"❌ Operation failed: {error}");
//                     if (attempt < maxRetries)
//                     {
//                         Console.WriteLine($"🔄 Retrying... (Attempt {attempt + 1}/{maxRetries})");
//                         _clientClock.Rollback(); // Rollback to previous state
//                         await Task.Delay(1000); // Wait before retry
//                     }
//                 }
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"❌ Connection error: {ex.Message}");
//                 if (attempt < maxRetries)
//                 {
//                     Console.WriteLine($"🔄 Retrying... (Attempt {attempt + 1}/{maxRetries})");
//                     await Task.Delay(2000);
//                 }
//             }
//         }
        
//         Console.WriteLine("❌ All retry attempts failed!");
//     }
// }




using Grpc.Net.Client;
using CalculatorServer;
using Shared;

class Program
{
    private static VectorClock _clientClock = new VectorClock($"Client-{Environment.MachineName}");
    
    
    private static List<string> _serverAddresses = new List<string>
    {
        "http://localhost:5000",
        "http://localhost:5001", 
        "http://localhost:5002"
    };

    static async Task Main(string[] args)
    {
        Console.WriteLine("🧮 Distributed Calculator Client");
        Console.WriteLine("==================================");

        while (true)
        {
            try
            {
                Console.WriteLine("\nAvailable operations:");
                Console.WriteLine("1. Square");
                Console.WriteLine("2. Cube");
                Console.WriteLine("3. SlowMultiply");
                Console.WriteLine("4. Exit");
                Console.WriteLine("5. Parallel Test");
                Console.WriteLine("5. Run Parallel Clients Test");
                Console.WriteLine("6. Run CAP Theorem Test"); 
                Console.WriteLine("7. Run Lamport Clock Comparison");
                Console.WriteLine("8. Run Leader Election Test");
                Console.WriteLine("9. Run Two-Phase Commit Test");
                Console.WriteLine("10. Run Eventual Consistency Test");
                Console.WriteLine("11. Run Gossip Protocol Test ()");
                Console.Write("\nSelect operation (1-11): ");

                var choice = Console.ReadLine();
                if (choice == "4") break;

                var serverAddress = SelectServer();
                if (serverAddress == null) continue;

                //  FIXED: Configure channel for HTTP/2 without TLS
                using var channel = GrpcChannel.ForAddress(serverAddress, new GrpcChannelOptions
                {
                    HttpHandler = CreateHttpHandler()
                });
                
                var client = new Calculator.CalculatorClient(channel);

                switch (choice)
                {
                    case "1":
                        await HandleSquare(client);
                        break;
                    case "2":
                        await HandleCube(client);
                        break;
                    case "3":
                        await HandleSlowMultiply(client);
                        break;
                    case "5": 
                         await CalculatorClient.TestScripts.ParallelClientTest.RunParallelTest();
                        break;  
                    case "6":
                        await CalculatorClient.TestScripts.CAPTheoremTest.RunCAPTest();
                          break;
                    case "7":
                        await CalculatorClient.TestScripts.LamportComparisonTest.RunComparisonTest();
                          break;
                    case "8":
                         await CalculatorClient.TestScripts.LeaderElectionTest.RunLeaderElectionTest();
                         break;
                    case "9":
                        await CalculatorClient.TestScripts.TwoPhaseCommitTest.RunTwoPhaseCommitTest();
                         break;
                    case "10":
                        await CalculatorClient.TestScripts.EventualConsistencyTest.RunEventualConsistencyTest();
                        break;
                    case "11":
                        await CalculatorClient.TestScripts.GossipProtocolTest.RunGossipProtocolTest();
                        break;
                    default:
                        Console.WriteLine("Invalid choice!");
                        continue;
                }

                Console.WriteLine($"\n🕰️  Current client vector clock: {_clientClock}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }
    }

    // 🔧 NEW: Create HTTP handler for non-TLS HTTP/2
    private static HttpClientHandler CreateHttpHandler()
    {
        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = 
            (httpRequestMessage, cert, cetChain, policyErrors) => true;
        return handler;
    }

    private static string? SelectServer()
    {
        Console.WriteLine("\nAvailable servers:");
        for (int i = 0; i < _serverAddresses.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {_serverAddresses[i]}");
        }
        
        Console.Write("Select server (1-3): ");
        if (int.TryParse(Console.ReadLine(), out var serverChoice) && 
            serverChoice >= 1 && serverChoice <= _serverAddresses.Count)
        {
            return _serverAddresses[serverChoice - 1];
        }
        
        Console.WriteLine("Invalid server selection!");
        return null;
    }

    private static async Task HandleSquare(Calculator.CalculatorClient client)
    {
        Console.Write("Enter number to square: ");
        if (double.TryParse(Console.ReadLine(), out var number))
        {
            await ExecuteWithRetry(async () =>
            {
                _clientClock.Increment();
                
                var request = new CalculationRequest
                {
                    Number = number,
                    VectorClock = { _clientClock.GetClock() }
                };

                var response = await client.SquareAsync(request);
                return (response.IsSuccess, response.Result, response.ErrorMessage, 
                       response.VectorClock.ToDictionary(x => x.Key, x => x.Value));
            });
        }
    }

    private static async Task HandleCube(Calculator.CalculatorClient client)
    {
        Console.Write("Enter number to cube: ");
        if (double.TryParse(Console.ReadLine(), out var number))
        {
            await ExecuteWithRetry(async () =>
            {
                _clientClock.Increment();
                
                var request = new CalculationRequest
                {
                    Number = number,
                    VectorClock = { _clientClock.GetClock() }
                };

                var response = await client.CubeAsync(request);
                return (response.IsSuccess, response.Result, response.ErrorMessage,
                       response.VectorClock.ToDictionary(x => x.Key, x => x.Value));
            });
        }
    }

    private static async Task HandleSlowMultiply(Calculator.CalculatorClient client)
    {
        Console.Write("Enter first number: ");
        if (!double.TryParse(Console.ReadLine(), out var num1)) return;
        
        Console.Write("Enter second number: ");
        if (!double.TryParse(Console.ReadLine(), out var num2)) return;

        await ExecuteWithRetry(async () =>
        {
            _clientClock.Increment();
            
            var request = new MultiplyRequest
            {
                Number1 = num1,
                Number2 = num2,
                VectorClock = { _clientClock.GetClock() }
            };

            var response = await client.SlowMultiplyAsync(request);
            return (response.IsSuccess, response.Result, response.ErrorMessage,
                   response.VectorClock.ToDictionary(x => x.Key, x => x.Value));
        });
    }

    private static async Task ExecuteWithRetry(Func<Task<(bool success, double result, string error, Dictionary<string, int> vectorClock)>> operation)
    {
        const int maxRetries = 3;
        int attempt = 0;
        
        // Save client clock state for potential rollback
        _clientClock.SaveState();

        while (attempt < maxRetries)
        {
            attempt++;
            try
            {
                var (success, result, error, vectorClock) = await operation();
                
                // Merge server's vector clock
                _clientClock.Merge(vectorClock);
                
                if (success)
                {
                    Console.WriteLine($"✅ Result: {result}");
                    return;
                }
                else
                {
                    Console.WriteLine($"❌ Operation failed: {error}");
                    if (attempt < maxRetries)
                    {
                        Console.WriteLine($"🔄 Retrying... (Attempt {attempt + 1}/{maxRetries})");
                        _clientClock.Rollback(); // Rollback to previous state
                        await Task.Delay(1000); // Wait before retry
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Connection error: {ex.Message}");
                if (attempt < maxRetries)
                {
                    Console.WriteLine($"🔄 Retrying... (Attempt {attempt + 1}/{maxRetries})");
                    await Task.Delay(2000);
                }
            }
        }
        
        Console.WriteLine("❌ All retry attempts failed!");
    }
}