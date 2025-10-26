namespace Shared
{
    public class LamportClock
    {
        private int _clock;
        private readonly string _nodeId;
        private readonly object _lock = new object();

        public LamportClock(string nodeId)
        {
            _nodeId = nodeId;
            _clock = 0;
        }

        public int GetTime()
        {
            lock (_lock)
            {
                return _clock;
            }
        }

        public void Tick()
        {
            lock (_lock)
            {
                _clock++;
                Console.WriteLine($"[{_nodeId}] Lamport clock ticked: {_clock}");
            }
        }

        public void Update(int receivedTime)
        {
            lock (_lock)
            {
                var oldClock = _clock;
                _clock = Math.Max(_clock, receivedTime) + 1;
                Console.WriteLine($"[{_nodeId}] Lamport clock updated from {oldClock} to {_clock} (received: {receivedTime})");
            }
        }

        public override string ToString()
        {
            return $"{_nodeId}:{_clock}";
        }
    }
}