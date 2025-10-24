using Grpc.Net.Client;
using CalculatorServer;
using Shared;

namespace CalculatorClient.TestScripts
{
    public class LamportComparisonTest
    {
        public static async Task RunComparisonTest()
        {
            Console.WriteLine("🕰️ Vector Clock vs Lamport Clock Comparison");
            Console.WriteLine("=============================================");

            Console.WriteLine("\n--- Phase 1: Vector Clock Test ---");
            await TestWithVectorClock();
            
            Console.WriteLine("\n--- Phase 2: Lamport Clock Test ---");
            await TestWithLamportClock();
            
            Console.WriteLine("\n📊 Comparison Complete - Check the timing differences!");
        }

        private static async Task TestWithVectorClock()
        {
            var client1 = new VectorClock("CompareClient1");
            var client2 = new VectorClock("CompareClient2");
            
            // Simulate concurrent operations
            var task1 = SimulateVectorOperations(client1, "http://localhost:5000", 1);
            var task2 = SimulateVectorOperations(client2, "http://localhost:5001", 2);
            
            await Task.WhenAll(task1, task2);
        }

        private static async Task TestWithLamportClock()
        {
            var client1 = new LamportClock("CompareClient1");
            var client2 = new LamportClock("CompareClient2");
            
            // Simulate concurrent operations
            var task1 = SimulateLamportOperations(client1, "http://localhost:5000", 1);
            var task2 = SimulateLamportOperations(client2, "http://localhost:5001", 2);
            
            await Task.WhenAll(task1, task2);
        }

        private static async Task SimulateVectorOperations(VectorClock clock, string server, int clientNum)
        {
            // Implementation for vector clock operations
            for (int i = 0; i < 3; i++)
            {
                clock.Increment();
                // ... perform calculation and merge response clock
                await Task.Delay(1000);
            }
        }

        private static async Task SimulateLamportOperations(LamportClock clock, string server, int clientNum)
        {
            // Implementation for Lamport clock operations
            for (int i = 0; i < 3; i++)
            {
                clock.Tick();
                // ... perform calculation and update clock
                await Task.Delay(1000);
            }
        }
    }
}