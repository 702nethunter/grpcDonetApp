using grpcClientApp;
using Monster.Logging;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(LogConfigurator.Configure("GrpcClientApp"));
builder.Services.AddSingleton<SettingsManager>();
builder.Services.AddSingleton<CoreService>();
builder.Services.AddSingleton<SnowflakeIdGenerator>(sp=>{
    var config = sp.GetRequiredService<IConfiguration>();
    int workerId = config.GetValue<int>("Snowflake:WorkerId",1);
    int dataCenterId = config.GetValue<int>("Snowflake:DataCenterId",1);
    return new SnowflakeIdGenerator(workerId,dataCenterId);
});

builder.Services.AddHostedService<GrpcClientWorker>();

var host = builder.Build();
host.Run();