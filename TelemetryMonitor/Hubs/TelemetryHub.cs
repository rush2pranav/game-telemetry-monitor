using Microsoft.AspNetCore.SignalR;

namespace TelemetryMonitor.Hubs
{
    // SignalR Hub for real-time telemetry streaming.
    // Clients connect to this hub to receive live game events and metrics.
    // SignalR handles the WebSocket connection, automatic reconnection,
    // and message serialization — we just define the communication contract.
    public class TelemetryHub : Hub
    {
        private readonly ILogger<TelemetryHub> _logger;

        public TelemetryHub(ILogger<TelemetryHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}