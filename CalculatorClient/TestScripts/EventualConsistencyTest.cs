using Shared;

namespace CalculatorClient.TestScripts
{
    public class EventualConsistencyTest
    {
        public static async Task RunEventualConsistencyTest()
        {
            Console.WriteLine("🔃 Eventual Consistency Test");
            Console.WriteLine("============================");

            var syncService = new ClockSynchronizationService();
            
            // Register multiple servers
            var server1Clock = new VectorClock("Server-1");
            var server2Clock = new VectorClock("Server-2");
            var server3Clock = new VectorClock("Server-3");

            syncService.RegisterServer("Server-1", server1Clock);
            syncService.RegisterServer("Server-2", server2Clock);
            syncService.RegisterServer("Server-3", server3Clock);

            // Start periodic synchronization
            await syncService.StartPeriodicSynchronization();

            Console.WriteLine("\n📊 Simulating operations that cause divergence...");

            // Simulate operations on different servers
            await SimulateDivergentOperations(server1Clock, server2Clock, server3Clock);

            Console.WriteLine("\n⏱️  Observing convergence over time...");
            
            // Let the system run for a while to observe convergence
            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(2000);
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Clock states:");
                Console.WriteLine($"  Server-1: {server1Clock}");
                Console.WriteLine($"  Server-2: {server2Clock}");
                Console.WriteLine($"  Server-3: {server3Clock}");
            }

            Console.WriteLine("\n✅ Eventual consistency test completed");
        }

        private static async Task SimulateDivergentOperations(VectorClock clock1, VectorClock clock2, VectorClock clock3)
        {
            // Simulate rapid operations on different servers
            var tasks = new List<Task>
            {
                Task.Run(async () =>
                {
                    for (int i = 0; i < 5; i++)
                    {
                        clock1.Increment();
                        await Task.Delay(500);
                    }
                }),
                
                Task.Run(async () =>
                {
                    await Task.Delay(1000); // Start later
                    for (int i = 0; i < 7; i++)
                    {
                        clock2.Increment();
                        await Task.Delay(300);
                    }
                }),
                
                Task.Run(async () =>
                {
                    await Task.Delay(2000); // Start even later
                    for (int i = 0; i < 3; i++)
                    {
                        clock3.Increment();
                        await Task.Delay(800);
                    }
                })
            };

            await Task.WhenAll(tasks);
            
            Console.WriteLine("📈 Operations completed, clocks have diverged:");
            Console.WriteLine($"  Server-1: {clock1}");
            Console.WriteLine($"  Server-2: {clock2}");
            Console.WriteLine($"  Server-3: {clock3}");
        }
    }
}