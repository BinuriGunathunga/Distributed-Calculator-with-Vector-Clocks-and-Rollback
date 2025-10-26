using System.Text.Json;

namespace Shared
{
    public class LeaderElection
    {
        private const string LEADER_FILE = "leader.json";
        
        public class LeaderInfo
        {
            public string ServerId { get; set; } = "";
            public string Address { get; set; } = "";
            public DateTime ElectedAt { get; set; }
            public bool IsActive { get; set; }
        }

        public static async Task<LeaderInfo?> GetCurrentLeader()
        {
            try
            {
                if (File.Exists(LEADER_FILE))
                {
                    var json = await File.ReadAllTextAsync(LEADER_FILE);
                    return JsonSerializer.Deserialize<LeaderInfo>(json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error reading leader file: {ex.Message}");
            }
            
            return null;
        }

        public static async Task ElectLeader(string serverId, string address)
        {
            var leaderInfo = new LeaderInfo
            {
                ServerId = serverId,
                Address = address,
                ElectedAt = DateTime.Now,
                IsActive = true
            };

            try
            {
                var json = JsonSerializer.Serialize(leaderInfo, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(LEADER_FILE, json);
                Console.WriteLine($"👑 NEW LEADER ELECTED: {serverId} at {address}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error writing leader file: {ex.Message}");
            }
        }

        public static async Task MarkLeaderDown(string serverId)
        {
            var leader = await GetCurrentLeader();
            if (leader != null && leader.ServerId == serverId)
            {
                leader.IsActive = false;
                var json = JsonSerializer.Serialize(leader, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(LEADER_FILE, json);
                Console.WriteLine($"💀 LEADER MARKED DOWN: {serverId}");
            }
        }

        public static async Task<bool> IsLeader(string serverId)
        {
            var leader = await GetCurrentLeader();
            return leader != null && leader.ServerId == serverId && leader.IsActive;
        }
    }
}