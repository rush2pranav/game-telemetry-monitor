using Microsoft.AspNetCore.SignalR;
using TelemetryMonitor.Hubs;
using TelemetryMonitor.Models;

namespace TelemetryMonitor.Services
{
    // this simulates what a real game server's telemetry pipeline would produce such as player actions, purchases, server metrics
    public class TelemetryGeneratorService : BackgroundService
    {
        private readonly IHubContext<TelemetryHub> _hubContext;
        private readonly ILogger<TelemetryGeneratorService> _logger;
        private readonly Random _random = new(42);

        // simulated server state
        private int _onlinePlayers = 850;
        private int _activeMatches = 42;
        private double _totalRevenue = 0;
        private int _totalKills = 0;
        private int _totalLogins = 0;
        private int _peakPlayers = 850;
        private int _eventCount = 0;
        private DateTime _lastEpsCheck = DateTime.UtcNow;

        private readonly List<MetricPoint> _playerHistory = new();
        private readonly List<MetricPoint> _epsHistory = new();
        private readonly List<MetricPoint> _revenueHistory = new();

        // game data for realistic events
        private readonly string[] _playerNames = {
            "ShadowBlade99", "DragonSlayer", "PixelWarrior", "NightHawk42",
            "CrystalMage", "IronFist", "StarKiller", "PhoenixRise",
            "ThunderBolt", "FrostByte", "DarkMatter", "BlazeFury",
            "StormChaser", "VoidWalker", "SilverArrow", "GhostReaper",
            "RuneForge", "WindRunner", "FlameHeart", "IceBreaker",
            "TitanSmash", "LunarEclipse", "SolarFlare", "NeonPulse",
            "CyberWolf", "OmegaStrike", "AlphaHunter", "ZeroGravity",
            "QuantumLeap", "NovaBlast", "EchoStorm", "PrimeShot"
        };

        private readonly string[] _maps = {
            "Crimson Arena", "Frozen Wastes", "Sky Citadel", "Shadow Keep",
            "Dragon Peak", "Sunken Ruins", "Volcanic Pit", "Crystal Caverns",
            "Neon City", "Ancient Temple", "Storm Fortress", "Mystic Gardens"
        };

        private readonly string[] _weapons = {
            "Assault Rifle", "Plasma Pistol", "Void Sniper", "Thunder Shotgun",
            "Shadow Dagger", "Dragon's Breath", "Ice Lance", "Fire Staff"
        };

        private readonly string[] _items = {
            "Legendary Skin Bundle ($24.99)", "Battle Pass ($9.99)",
            "Weapon Skin ($4.99)", "Emote Pack ($2.99)",
            "XP Boost ($1.99)", "Loot Box ($0.99)",
            "Character Bundle ($14.99)", "Map Pack ($7.99)"
        };

        private readonly double[] _itemPrices = {
            24.99, 9.99, 4.99, 2.99, 1.99, 0.99, 14.99, 7.99
        };

        private readonly string[] _achievements = {
            "First Blood", "Unstoppable", "Sharpshooter", "Team Player",
            "Dragon Slayer", "Speed Demon", "Collector", "Veteran"
        };

        public TelemetryGeneratorService(IHubContext<TelemetryHub> hubContext,
            ILogger<TelemetryGeneratorService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Telemetry Generator started");

            while (!stoppingToken.IsCancellationRequested)
            {
                // generate 1-3 events per tick
                int eventsThisTick = _random.Next(1, 4);
                for (int i = 0; i < eventsThisTick; i++)
                {
                    var gameEvent = GenerateRandomEvent();
                    await _hubContext.Clients.All.SendAsync("ReceiveEvent", gameEvent, stoppingToken);
                    _eventCount++;
                }

                // update server metrics every tick
                UpdateServerState();

                // send metrics update every 2 seconds
                if (_eventCount % 5 == 0)
                {
                    var metrics = GetCurrentMetrics();
                    await _hubContext.Clients.All.SendAsync("ReceiveMetrics", metrics, stoppingToken);
                }

                // random delay between events  ie somewhere between 200-800ms for realistic feel
                await Task.Delay(_random.Next(200, 800), stoppingToken);
            }
        }

        private GameEvent GenerateRandomEvent()
        {
            var player = _playerNames[_random.Next(_playerNames.Length)];
            var map = _maps[_random.Next(_maps.Length)];
            int roll = _random.Next(100);

            if (roll < 35)
            {
                var victim = _playerNames[_random.Next(_playerNames.Length)];
                var weapon = _weapons[_random.Next(_weapons.Length)];
                _totalKills++;
                return new GameEvent
                {
                    EventType = "Kill",
                    PlayerName = player,
                    Details = $"eliminated {victim} with {weapon}",
                    MapName = map
                };
            }
            else if (roll < 50)
            {
                return new GameEvent
                {
                    EventType = "Death",
                    PlayerName = player,
                    Details = $"was eliminated on {map}",
                    MapName = map
                };
            }
            else if (roll < 62)
            {
                int itemIdx = _random.Next(_items.Length);
                var amount = _itemPrices[itemIdx];
                _totalRevenue += amount;
                return new GameEvent
                {
                    EventType = "Purchase",
                    PlayerName = player,
                    Details = _items[itemIdx],
                    Amount = amount,
                    Severity = amount >= 10 ? "Warning" : "Info"
                };
            }
            else if (roll < 75)
            {
                _onlinePlayers += _random.Next(1, 4);
                _totalLogins++;
                return new GameEvent
                {
                    EventType = "Login",
                    PlayerName = player,
                    Details = $"joined from {map} server"
                };
            }
            else if (roll < 85)
            {
                _onlinePlayers = Math.Max(100, _onlinePlayers - _random.Next(1, 3));
                return new GameEvent
                {
                    EventType = "Logout",
                    PlayerName = player,
                    Details = $"played for {_random.Next(5, 180)} minutes"
                };
            }
            else if (roll < 92)
            {
                return new GameEvent
                {
                    EventType = "LevelUp",
                    PlayerName = player,
                    Details = $"reached level {_random.Next(2, 100)}"
                };
            }
            else if (roll < 97)
            {
                var achievement = _achievements[_random.Next(_achievements.Length)];
                return new GameEvent
                {
                    EventType = "Achievement",
                    PlayerName = player,
                    Details = $"unlocked \"{achievement}\""
                };
            }
            else
            {
                return new GameEvent
                {
                    EventType = "ServerError",
                    PlayerName = "SYSTEM",
                    Details = $"Latency spike on {map} ({_random.Next(100, 500)}ms)",
                    Severity = "Error",
                    MapName = map
                };
            }
        }

        private void UpdateServerState()
        {
            // simulate natural player count fluctuation
            _onlinePlayers += _random.Next(-2, 4);
            _onlinePlayers = Math.Clamp(_onlinePlayers, 200, 2000);
            _peakPlayers = Math.Max(_peakPlayers, _onlinePlayers);

            _activeMatches = _onlinePlayers / _random.Next(8, 15);

            // track history - keeping the last 60 points
            var now = DateTime.UtcNow;
            _playerHistory.Add(new MetricPoint { Time = now, Value = _onlinePlayers });
            if (_playerHistory.Count > 60) _playerHistory.RemoveAt(0);

            // calcualting EPS
            var elapsed = (now - _lastEpsCheck).TotalSeconds;
            if (elapsed >= 2)
            {
                double eps = _eventCount / elapsed;
                _epsHistory.Add(new MetricPoint { Time = now, Value = Math.Round(eps, 1) });
                if (_epsHistory.Count > 60) _epsHistory.RemoveAt(0);
                _eventCount = 0;
                _lastEpsCheck = now;
            }

            _revenueHistory.Add(new MetricPoint { Time = now, Value = Math.Round(_totalRevenue, 2) });
            if (_revenueHistory.Count > 60) _revenueHistory.RemoveAt(0);
        }

        private ServerMetrics GetCurrentMetrics() => new()
        {
            OnlinePlayers = _onlinePlayers,
            ActiveMatches = _activeMatches,
            EventsPerSecond = _epsHistory.LastOrDefault()?.Value ?? 0,
            ServerCpuPercent = Math.Round(30 + _random.NextDouble() * 40, 1),
            ServerMemoryPercent = Math.Round(45 + _random.NextDouble() * 25, 1),
            TotalRevenueToday = Math.Round(_totalRevenue, 2),
            TotalKillsToday = _totalKills,
            TotalLoginsToday = _totalLogins,
            PeakPlayersToday = _peakPlayers,
            LastUpdatedUtc = DateTime.UtcNow,
            PlayerCountHistory = new List<MetricPoint>(_playerHistory),
            EpsHistory = new List<MetricPoint>(_epsHistory),
            RevenueHistory = new List<MetricPoint>(_revenueHistory)
        };
    }
}