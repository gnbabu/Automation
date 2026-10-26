using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories.TestRunner
{
    public sealed class QueueRunScheduler
    {
        private readonly object _gate = new();
        private readonly Dictionary<Guid, string> _running = new();
        private readonly HashSet<string> _busyLogins = new(StringComparer.OrdinalIgnoreCase);

        public QueueRunScheduler(int maxParallelRuns)
        {
            MaxParallelRuns = Math.Max(1, maxParallelRuns);
        }

        public int MaxParallelRuns { get; }

        public int RunningCount
        {
            get { lock (_gate) return _running.Count; }
        }

        public bool HasFreeSlot
        {
            get { lock (_gate) return _running.Count < MaxParallelRuns; }
        }

        public bool IsRunning(Guid queueId)
        {
            lock (_gate) return _running.ContainsKey(queueId);
        }

        public static string LoginKey(PendingExecutionQueue item)
        {
            var environment = $"env:{item.EnvironmentId?.ToString() ?? "none"}";
            if (!string.IsNullOrWhiteSpace(item.LoginUserName)) return $"{environment}:account:{item.LoginUserName.Trim()}";
            return item.LoginUserId is int id ? $"{environment}:loginuser:{id}" : $"{environment}:default";
        }

        public bool TryReserve(PendingExecutionQueue item)
        {
            var key = LoginKey(item);
            lock (_gate)
            {
                if (_running.Count >= MaxParallelRuns) return false;
                if (_running.ContainsKey(item.QueueId)) return false;
                if (_busyLogins.Contains(key)) return false;
                _running[item.QueueId] = key;
                _busyLogins.Add(key);
                return true;
            }
        }

        public void Release(Guid queueId)
        {
            lock (_gate)
            {
                if (_running.Remove(queueId, out var key)) _busyLogins.Remove(key);
            }
        }
    }
}
