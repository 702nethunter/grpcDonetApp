using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Monster.Logging;
using grpcServerApp;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(LogConfigurator.Configure("GrpcServerApp"));

// Register services
builder.Services.AddSingleton<HostDataAccess>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connStr = config.GetConnectionString("PostgresDb");
    var logger = sp.GetRequiredService<ILogger<HostDataAccess>>();
    return new HostDataAccess(logger, connStr);
});

builder.Services.AddHostedService<Worker>();

// Add Kestrel hosting for gRPC
builder.Services.AddHostedService(sp =>
{
    var webAppBuilder = WebApplication.CreateBuilder();
    webAppBuilder.Configuration.AddConfiguration(builder.Configuration);
    webAppBuilder.WebHost.ConfigureKestrel(options =>
    {
        options.Configure(webAppBuilder.Configuration.GetSection("Kestrel"));
    });
    webAppBuilder.Logging.ClearProviders();
    webAppBuilder.Logging.AddSerilog(LogConfigurator.Configure("GrpcServerApp"));
    webAppBuilder.Services.AddGrpc();
    webAppBuilder.Services.AddSingleton<CoreService>();
    webAppBuilder.Services.AddSingleton<GrpcServerManager>();
    webAppBuilder.Services.AddSingleton<HostDataAccess>(sp =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        var connStr = config.GetConnectionString("PostgresDb");
        var logger = sp.GetRequiredService<ILogger<HostDataAccess>>();
        return new HostDataAccess(logger, connStr);
    });
    var app = webAppBuilder.Build();
    app.MapGrpcService<ClientTelemetryService>();
    app.MapGet("/", () => "This is a gRPC server; use a gRPC client to connect.");
    return new WebApplicationHostedService(app);
});

var host = builder.Build();
host.Run();

class WebApplicationHostedService : IHostedService
{
    private readonly WebApplication _app;
    public WebApplicationHostedService(WebApplication app) => _app = app;
    public Task StartAsync(CancellationToken cancellationToken) => _app.StartAsync(cancellationToken);
    public Task StopAsync(CancellationToken cancellationToken) => _app.StopAsync(cancellationToken);
}