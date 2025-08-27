using grpcServerApp;
using Monster.Logging;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Plug Serilog into the new host builder
builder.Logging.ClearProviders(); // optional: removes default Console logger
builder.Logging.AddSerilog(LogConfigurator.Configure("GrpcServerApp"));

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();