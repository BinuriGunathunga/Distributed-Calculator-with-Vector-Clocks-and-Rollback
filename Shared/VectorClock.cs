using System.Collections.Concurrent;

namespace Shared;

public class VectorClock
{
    private readonly ConcurrentDictionary<string, int> _clock;
    private readonly string _nodeId;

    public VectorClock(string nodeId)
    {
        _nodeId = nodeId;
        _clock = new ConcurrentDictionary<string, int>();
        _clock.TryAdd(nodeId, 0);
    }

    public Dictionary<string, int> GetClock()
    {
        return new Dictionary<string, int>(_clock);
    }

    public void Increment()
    {
        _clock.AddOrUpdate(_nodeId, 1, (key, value) => value + 1);
        Console.WriteLine($"[{_nodeId}] Clock incremented: {this}");
    }

    public void Merge(Dictionary<string, int> otherClock)
    {
        var oldClock = GetClock();
        
        foreach (var kvp in otherClock)
        {
            _clock.AddOrUpdate(kvp.Key, kvp.Value, (key, value) => Math.Max(value, kvp.Value));
        }

        Console.WriteLine($"[{_nodeId}] Clock merged from {string.Join(",", oldClock.Select(x => $"{x.Key}:{x.Value}"))} to {this}");
    }

    private Dictionary<string, int> _rollbackState = new();

    public void SaveState()
    {
        _rollbackState = new Dictionary<string, int>(_clock);
        Console.WriteLine($"[{_nodeId}] Clock state saved: {string.Join(",", _rollbackState.Select(x => $"{x.Key}:{x.Value}"))}");
    }

    public void Rollback()
    {
        if (_rollbackState.Any())
        {
            _clock.Clear();
            foreach (var kvp in _rollbackState)
            {
                _clock.TryAdd(kvp.Key, kvp.Value);
            }
            Console.WriteLine($"[{_nodeId}] ⚠️  ROLLBACK OCCURRED! Clock restored to: {this}");
        }
    }

    public override string ToString()
    {
        return string.Join(",", _clock.OrderBy(x => x.Key).Select(x => $"{x.Key}:{x.Value}"));
    }
}