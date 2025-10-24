using Shared;

namespace CalculatorClient.TestScripts
{
    public class TwoPhaseCommitTest
    {
        public static async Task RunTwoPhaseCommitTest()
        {
            Console.WriteLine("🔁 Two-Phase Commit Test");
            Console.WriteLine("========================");

            var participants = new List<string>
            {
                "http://localhost:5000",
                "http://localhost:5001",
                "http://localhost:5002"
            };

            var coordinator = new TwoPhaseCommitCoordinator(participants);

            while (true)
            {
                Console.WriteLine("\nTwo-Phase Commit Operations:");
                Console.WriteLine("1. Distributed Square (Server A prepares, Server B commits)");
                Console.WriteLine("2. Distributed Multiply (Multiple servers)");
                Console.WriteLine("3. Simulate Transaction Failure");
                Console.WriteLine("4. Exit");
                Console.Write("Choice: ");

                var choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            await TestDistributedSquare(coordinator);
                            break;
                        case "2":
                            await TestDistributedMultiply(coordinator);
                            break;
                        case "3":
                            await TestTransactionFailure(coordinator);
                            break;
                        case "4":
                            return;
                        default:
                            Console.WriteLine("Invalid choice!");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Transaction failed: {ex.Message}");
                }
            }
        }

        private static async Task TestDistributedSquare(TwoPhaseCommitCoordinator coordinator)
        {
            Console.Write("Enter number to square: ");
            if (double.TryParse(Console.ReadLine(), out var number))
            {
                Console.WriteLine($"\n🔄 Starting distributed square transaction for {number}");
                var result = await coordinator.ExecuteDistributedTransaction("square", number);
                Console.WriteLine($"🎉 Distributed transaction completed! Result: {result}");
            }
        }

        private static async Task TestDistributedMultiply(TwoPhaseCommitCoordinator coordinator)
        {
            Console.Write("Enter first number: ");
            if (!double.TryParse(Console.ReadLine(), out var num1)) return;
            
            Console.Write("Enter second number: ");
            if (!double.TryParse(Console.ReadLine(), out var num2)) return;

            Console.WriteLine($"\n🔄 Starting distributed multiply transaction for {num1} × {num2}");
            var result = await coordinator.ExecuteDistributedTransaction("multiply", num1, num2);
            Console.WriteLine($"🎉 Distributed transaction completed! Result: {result}");
        }

        private static async Task TestTransactionFailure(TwoPhaseCommitCoordinator coordinator)
        {
            Console.WriteLine("\n💥 Simulating transaction failure scenario...");
            
            // This will likely fail in the prepare phase due to our 20% failure simulation
            try
            {
                var result = await coordinator.ExecuteDistributedTransaction("square", -5);
                Console.WriteLine($"Unexpected success: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✅ Expected failure occurred: {ex.Message}");
                Console.WriteLine("📋 Observe how all participants rolled back the transaction");
            }
        }
    }
}