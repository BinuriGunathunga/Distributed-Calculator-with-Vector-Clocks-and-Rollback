using System.Collections.Concurrent;

namespace Shared
{
    public class ClockSynchronizationService
    {
        private readonly ConcurrentDictionary<string, VectorClock> _serverClocks = new();
        private readonly ConcurrentDictionary<string, DateTime> _lastSyncTimes = new();

        public void RegisterServer(string serverId, VectorClock clock)
        {
            _serverClocks.TryAdd(serverId, clock);
            _lastSyncTimes.TryAdd(serverId, DateTime.Now);
            Console.WriteLine($"📝 Server {serverId} registered for synchronization");
        }

        public async Task<VectorClock> GetLatestClock(string requestingServerId)
        {
            Console.WriteLine($"🔍 {requestingServerId} requesting latest clock");
            
            // Find the most advanced clock
            VectorClock latestClock = null;
            var maxTotalTime = 0;

            foreach (var kvp in _serverClocks)
            {
                if (kvp.Key == requestingServerId) continue;
                
                var totalTime = kvp.Value.GetClock().Values.Sum();
                if (totalTime > maxTotalTime)
                {
                    maxTotalTime = totalTime;
                    latestClock = kvp.Value;
                }
            }

            if (latestClock != null)
            {
                Console.WriteLine($"📤 Sending latest clock to {requestingServerId}: {latestClock}");
                
                // Update last sync time
                _lastSyncTimes.TryUpdate(requestingServerId, DateTime.Now, _lastSyncTimes[requestingServerId]);
                
                return new VectorClock(requestingServerId);
            }

            return new VectorClock(requestingServerId);
        }

        public void CheckDivergence()
        {
            Console.WriteLine("\n🔍 Checking for clock divergence...");
            
            var now = DateTime.Now;
            var divergedServers = new List<string>();

            foreach (var kvp in _lastSyncTimes)
            {
                var timeSinceSync = now - kvp.Value;
                if (timeSinceSync.TotalSeconds > 5)
                {
                    divergedServers.Add(kvp.Key);
                    Console.WriteLine($"⚠️  DIVERGED: {kvp.Key} hasn't synced for {timeSinceSync.TotalSeconds:F1} seconds");
                }
            }

            if (divergedServers.Count == 0)
            {
                Console.WriteLine("✅ All servers are synchronized");
            }
            else
            {
                Console.WriteLine($"📊 {divergedServers.Count} server(s) have diverged from the cluster");
            }
        }

        public async Task StartPeriodicSynchronization()
        {
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(3000); // Check every 3 seconds
                    CheckDivergence();
                }
            });

            _ = Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(10000); // Sync every 10 seconds
                    await PerformClusterSync();
                }
            });
        }

        private async Task PerformClusterSync()
        {
            Console.WriteLine("\n🔄 Performing cluster-wide synchronization...");
            
            var allClocks = _serverClocks.Values.ToList();
            if (allClocks.Count < 2) return;

            // Create a merged clock representing the latest state
            var mergedClock = new Dictionary<string, int>();
            
            foreach (var clock in allClocks)
            {
                foreach (var kvp in clock.GetClock())
                {
                    if (!mergedClock.ContainsKey(kvp.Key) || mergedClock[kvp.Key] < kvp.Value)
                    {
                        mergedClock[kvp.Key] = kvp.Value;
                    }
                }
            }

            // Update all server clocks with the merged state
            foreach (var kvp in _serverClocks)
            {
                kvp.Value.Merge(mergedClock);
                _lastSyncTimes.TryUpdate(kvp.Key, DateTime.Now, _lastSyncTimes[kvp.Key]);
            }

            Console.WriteLine($"✅ Cluster synchronized. Latest state: {string.Join(",", mergedClock.Select(x => $"{x.Key}:{x.Value}"))}");
        }
    }
}