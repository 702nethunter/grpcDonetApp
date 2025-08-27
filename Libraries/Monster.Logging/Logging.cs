using Serilog;

namespace Monster.Logging;

public static class LogConfigurator
{
    public static ILogger Configure(string appName)
    {
        return new LoggerConfiguration()
            .Enrich.WithProperty("App", appName)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File($"logs/{appName}-.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }
}
