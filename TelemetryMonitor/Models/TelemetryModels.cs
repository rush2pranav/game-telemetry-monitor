namespace TelemetryMonitor.Models
{
    // a single game event from the telemetry stream
    public class GameEvent
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
        public string EventType { get; set; } = string.Empty;   // kill, death, purchase, login, logout, levelup, achievement
        public string PlayerName { get; set; } = string.Empty;
        public string? Details { get; set; }
        public double? Amount { get; set; }                       // for purchase events in usd
        public string? MapName { get; set; }
        public string Severity { get; set; } = "Info";           // info, warning, error

        public string Icon => EventType switch
        {
            "Kill" => "⚔️",
            "Death" => "💀",
            "Purchase" => "💰",
            "Login" => "🟢",
            "Logout" => "🔴",
            "LevelUp" => "⬆️",
            "Achievement" => "🏆",
            "ChatMessage" => "💬",
            "ServerError" => "⚠️",
            _ => "📌"
        };

        public string CssClass => Severity switch
        {
            "Warning" => "event-warning",
            "Error" => "event-error",
            _ => "event-info"
        };
    }

    // real time server metrics snapshots
    public class ServerMetrics
    {
        public int OnlinePlayers { get; set; }
        public int ActiveMatches { get; set; }
        public double EventsPerSecond { get; set; }
        public double ServerCpuPercent { get; set; }
        public double ServerMemoryPercent { get; set; }
        public double TotalRevenueToday { get; set; }
        public int TotalKillsToday { get; set; }
        public int TotalLoginsToday { get; set; }
        public int PeakPlayersToday { get; set; }
        public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;
        public List<MetricPoint> PlayerCountHistory { get; set; } = new();
        public List<MetricPoint> EpsHistory { get; set; } = new();
        public List<MetricPoint> RevenueHistory { get; set; } = new();
    }

    // a single data point
    public class MetricPoint
    {
        public DateTime Time { get; set; }
        public double Value { get; set; }
    }
}