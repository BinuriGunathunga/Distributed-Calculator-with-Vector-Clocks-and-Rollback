using System.Collections.Concurrent;

namespace Shared
{
    public static class NetworkPartition
    {
        private static readonly ConcurrentHashSet<string> _partitionedServers = new();
        private static readonly object _lock = new object();

        public static void PartitionServer(string serverId)
        {
            lock (_lock)
            {
                _partitionedServers.Add(serverId);
                Console.WriteLine($"🚫 NETWORK PARTITION: {serverId} is now isolated");
            }
        }

        public static void HealPartition(string serverId)
        {
            lock (_lock)
            {
                _partitionedServers.TryRemove(serverId);
                Console.WriteLine($"✅ PARTITION HEALED: {serverId} is back online");
            }
        }

        public static bool IsPartitioned(string serverId)
        {
            return _partitionedServers.Contains(serverId);
        }

        public static void SimulateRandomPartition()
        {
            var servers = new[] { "Server-1", "Server-2", "Server-3" };
            var random = new Random();
            
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(random.Next(10000, 30000)); // 10-30 seconds
                    
                    var serverToPartition = servers[random.Next(servers.Length)];
                    
                    if (!IsPartitioned(serverToPartition))
                    {
                        PartitionServer(serverToPartition);
                        
                        // Heal after 5-15 seconds
                        await Task.Delay(random.Next(5000, 15000));
                        HealPartition(serverToPartition);
                    }
                }
            });
        }
    }

    public class ConcurrentHashSet<T> : IDisposable
    {
        private readonly ConcurrentDictionary<T, bool> _dict = new();

        public bool Add(T item) => _dict.TryAdd(item, true);
        public bool TryRemove(T item) => _dict.TryRemove(item, out _);
        public bool Contains(T item) => _dict.ContainsKey(item);
        public void Dispose() => _dict.Clear();
    }
}