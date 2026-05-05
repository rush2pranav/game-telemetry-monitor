
using TelemetryMonitor.Components;
using TelemetryMonitor.Hubs;
using TelemetryMonitor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSignalR();
builder.Services.AddHostedService<TelemetryGeneratorService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapHub<TelemetryHub>("/telemetryhub");

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine(@"
  ========================================
  |  Game Telemetry Monitor - SignalR    |
  | Dashboard: https://localhost:5001    |
  ========================================
");
Console.ResetColor();

app.Run();
