using grpcClientApp;
using Monster.Logging;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(LogConfigurator.Configure("GrpcClientApp"));

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();