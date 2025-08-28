using grpcServerApp;
using Monster.Logging;
using Serilog;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging (shared logging lib)
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(LogConfigurator.Configure("GrpcServerApp"));

// Add gRPC server support (uses Kestrel internally)
builder.Services.AddGrpc();
builder.Services.AddSingleton<HostDataAccess>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connStr = config.GetConnectionString("PostgresDb");
    var logger = sp.GetRequiredService<ILogger<HostDataAccess>>();
    return new HostDataAccess(logger, connStr);
});

builder.Services.AddSingleton<GrpcServerManager>();
// Add your background worker
builder.Services.AddHostedService<Worker>();

// Add Kestrel hosting for gRPC
builder.Services.AddHostedService(sp =>
{
    var webAppBuilder = WebApplication.CreateBuilder();

    // Important: copy configuration from parent builder (so it sees appsettings.json)
    webAppBuilder.Configuration.AddConfiguration(builder.Configuration);

    // Configure Kestrel from configuration
    webAppBuilder.WebHost.ConfigureKestrel(options =>
    {
        options.Configure(webAppBuilder.Configuration.GetSection("Kestrel"));
    });

    webAppBuilder.Logging.ClearProviders();
    webAppBuilder.Logging.AddSerilog(LogConfigurator.Configure("GrpcServerApp"));

    webAppBuilder.Services.AddGrpc();

    var app = webAppBuilder.Build();
    app.MapGrpcService<ClientTelemetryService>();
    app.MapGet("/", () => "This is a gRPC server; use a gRPC client to connect.");

    return new WebApplicationHostedService(app);
});

var host = builder.Build();
host.Run();


/// Helper so we can run WebApplication inside Worker host
class WebApplicationHostedService : IHostedService
{
    private readonly WebApplication _app;
    public WebApplicationHostedService(WebApplication app) => _app = app;

    public Task StartAsync(CancellationToken cancellationToken) => _app.StartAsync(cancellationToken);
    public Task StopAsync(CancellationToken cancellationToken) => _app.StopAsync(cancellationToken);
}
