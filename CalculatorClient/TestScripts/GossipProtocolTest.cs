using Shared;

namespace CalculatorClient.TestScripts
{
    public class GossipProtocolTest
    {
        public static async Task RunGossipProtocolTest()
        {
            Console.WriteLine("📣 Gossip Protocol Test");
            Console.WriteLine("=======================");

            // Create multiple gossip nodes
            var nodes = new List<GossipProtocol>();
            var clocks = new Dictionary<string, VectorClock>();

            for (int i = 1; i <= 4; i++)
            {
                var nodeId = $"GossipNode-{i}";
                var clock = new VectorClock(nodeId);
                var gossipNode = new GossipProtocol(nodeId);
                
                nodes.Add(gossipNode);
                clocks[nodeId] = clock;
                
                // Register this node with all other nodes
                foreach (var otherNode in nodes.Take(nodes.Count - 1))
                {
                    otherNode.RegisterNode(nodeId, $"http://localhost:{5000 + i}", clock);
                }
                
                // Register other nodes with this node
                for (int j = 1; j < i; j++)
                {
                    var otherNodeId = $"GossipNode-{j}";
                    gossipNode.RegisterNode(otherNodeId, $"http://localhost:{5000 + j}", clocks[otherNodeId]);
                }
            }

            Console.WriteLine("\n🔄 Simulating different operations on nodes to create clock divergence...");
            
            // Create clock divergence
            await SimulateClockDivergence(clocks);

            Console.WriteLine("\n📊 Initial clock states (before gossip):");
            foreach (var kvp in clocks)
            {
                Console.WriteLine($"   {kvp.Key}: {kvp.Value}");
            }

            Console.WriteLine("\n⏱️  Starting gossip protocol and measuring convergence time...");
            
            // Measure convergence time
            var convergenceTask = nodes.First().MeasureConvergenceTime();
            
            // Let gossip run for a while
            await Task.Delay(TimeSpan.FromMinutes(1));
            
            try
            {
                var convergenceTime = await convergenceTask;
                Console.WriteLine($"🎉 Final convergence time: {convergenceTime.TotalSeconds:F1} seconds");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Convergence measurement failed: {ex.Message}");
            }

            Console.WriteLine("\n📊 Final clock states (after gossip):");
            foreach (var kvp in clocks)
            {
                Console.WriteLine($"   {kvp.Key}: {kvp.Value}");
            }

            // Cleanup
            foreach (var node in nodes)
            {
                node.Dispose();
            }

            Console.WriteLine("\n✅ Gossip protocol test completed");
        }

        private static async Task SimulateClockDivergence(Dictionary<string, VectorClock> clocks)
        {
            var random = new Random();
            var tasks = new List<Task>();

            foreach (var kvp in clocks)
            {
                var nodeId = kvp.Key;
                var clock = kvp.Value;
                
                tasks.Add(Task.Run(async () =>
                {
                    var operationCount = random.Next(3, 8);
                    
                    for (int i = 0; i < operationCount; i++)
                    {
                        clock.Increment();
                        await Task.Delay(random.Next(100, 500));
                    }
                    
                    Console.WriteLine($"📈 {nodeId} completed {operationCount} operations");
                }));
            }

            await Task.WhenAll(tasks);
        }
    }
}