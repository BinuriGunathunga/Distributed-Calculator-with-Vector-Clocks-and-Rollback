using System.Collections.Concurrent;

namespace Shared
{
    public class GossipNode
    {
        public string NodeId { get; set; } = "";
        public string Address { get; set; } = "";
        public VectorClock Clock { get; set; } = null!;
        public DateTime LastSeen { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class GossipProtocol
    {
        private readonly ConcurrentDictionary<string, GossipNode> _nodes = new();
        private readonly Random _random = new();
        private readonly Timer _gossipTimer;
        private readonly string _currentNodeId;

        public GossipProtocol(string nodeId)
        {
            _currentNodeId = nodeId;
            
            // Start gossip timer - gossip every 10 seconds
            _gossipTimer = new Timer(async _ => await PerformGossip(), null, 
                TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
            
            Console.WriteLine($"📣 Gossip protocol started for node: {nodeId}");
        }

        public void RegisterNode(string nodeId, string address, VectorClock clock)
        {
            var node = new GossipNode
            {
                NodeId = nodeId,
                Address = address,
                Clock = clock,
                LastSeen = DateTime.Now,
                IsActive = true
            };

            _nodes.AddOrUpdate(nodeId, node, (key, existing) =>
            {
                existing.Address = address;
                existing.Clock = clock;
                existing.LastSeen = DateTime.Now;
                existing.IsActive = true;
                return existing;
            });

            Console.WriteLine($"📝 Node registered in gossip: {nodeId} at {address}");
        }

        private async Task PerformGossip()
        {
            try
            {
                var activeNodes = _nodes.Values.Where(n => n.IsActive && n.NodeId != _currentNodeId).ToList();
                
                if (activeNodes.Count == 0)
                {
                    Console.WriteLine("📣 No other nodes available for gossip");
                    return;
                }

                // Select random node to gossip with
                var targetNode = activeNodes[_random.Next(activeNodes.Count)];
                
                Console.WriteLine($"📣 Gossiping with {targetNode.NodeId}...");
                
                await SendGossipMessage(targetNode);
                
                // Update convergence metrics
                await CheckConvergence();
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Gossip error: {ex.Message}");
            }
        }

        private async Task SendGossipMessage(GossipNode targetNode)
        {
            // Simulate sending gossip message
            await Task.Delay(100); // Network delay simulation
            
            // In real implementation, this would be a network call
            // For simulation, we'll just merge clocks
            
            var currentNode = _nodes[_currentNodeId];
            if (currentNode != null)
            {
                // Simulate clock exchange and merge
                var beforeClock = currentNode.Clock.GetClock();
                var targetClock = targetNode.Clock.GetClock();
                
                // Both nodes update their clocks
                currentNode.Clock.Merge(targetClock);
                targetNode.Clock.Merge(beforeClock);
                
                Console.WriteLine($"📤 Gossiped clock state with {targetNode.NodeId}");
                Console.WriteLine($"   Current node clock: {currentNode.Clock}");
                Console.WriteLine($"   Target node clock: {targetNode.Clock}");
                
                // Update last seen times
                currentNode.LastSeen = DateTime.Now;
                targetNode.LastSeen = DateTime.Now;
            }
        }

        private async Task CheckConvergence()
        {
            var allClocks = _nodes.Values.Where(n => n.IsActive).Select(n => n.Clock.GetClock()).ToList();
            
            if (allClocks.Count < 2) return;

            // Check if all clocks are similar (within 2 ticks)
            var isConverged = true;
            var firstClock = allClocks.First();
            
            foreach (var clock in allClocks.Skip(1))
            {
                foreach (var nodeId in firstClock.Keys.Union(clock.Keys))
                {
                    var time1 = firstClock.GetValueOrDefault(nodeId, 0);
                    var time2 = clock.GetValueOrDefault(nodeId, 0);
                    
                    if (Math.Abs(time1 - time2) > 2)
                    {
                        isConverged = false;
                        break;
                    }
                }
                if (!isConverged) break;
            }

            if (isConverged)
            {
                Console.WriteLine("🎉 CONVERGENCE ACHIEVED! All nodes have similar clocks");
            }
            else
            {
                Console.WriteLine("🔄 Nodes still converging...");
            }

            // Log current state
            Console.WriteLine("📊 Current gossip network state:");
            foreach (var node in _nodes.Values.Where(n => n.IsActive))
            {
                var timeSinceLastSeen = DateTime.Now - node.LastSeen;
                Console.WriteLine($"   {node.NodeId}: {node.Clock} (last seen {timeSinceLastSeen.TotalSeconds:F0}s ago)");
            }
        }

        public void MarkNodeInactive(string nodeId)
        {
            if (_nodes.TryGetValue(nodeId, out var node))
            {
                node.IsActive = false;
                Console.WriteLine($"💀 Node marked inactive in gossip: {nodeId}");
            }
        }

        public async Task<TimeSpan> MeasureConvergenceTime()
        {
            var startTime = DateTime.Now;
            Console.WriteLine("⏱️  Starting convergence measurement...");
            
            while (true)
            {
                await Task.Delay(1000);
                
                var allClocks = _nodes.Values.Where(n => n.IsActive).Select(n => n.Clock.GetClock()).ToList();
                if (allClocks.Count < 2) continue;

                var isConverged = true;
                var firstClock = allClocks.First();
                
                foreach (var clock in allClocks.Skip(1))
                {
                    foreach (var nodeId in firstClock.Keys.Union(clock.Keys))
                    {
                        var time1 = firstClock.GetValueOrDefault(nodeId, 0);
                        var time2 = clock.GetValueOrDefault(nodeId, 0);
                        
                        if (Math.Abs(time1 - time2) > 1)
                        {
                            isConverged = false;
                            break;
                        }
                    }
                    if (!isConverged) break;
                }

                if (isConverged)
                {
                    var convergenceTime = DateTime.Now - startTime;
                    Console.WriteLine($"🎯 Convergence achieved in {convergenceTime.TotalSeconds:F1} seconds!");
                    return convergenceTime;
                }

                // Timeout after 2 minutes
                if (DateTime.Now - startTime > TimeSpan.FromMinutes(2))
                {
                    Console.WriteLine("⏰ Convergence measurement timed out");
                    return TimeSpan.FromMinutes(2);
                }
            }
        }

        public void Dispose()
        {
            _gossipTimer?.Dispose();
        }
    }
}