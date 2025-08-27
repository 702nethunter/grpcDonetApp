namespace grpcServerApp;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private GrpcServerManager _serverManager;

    public Worker(ILogger<Worker> logger,GrpcServerManager serverManager)
    {
        _logger = logger;
        _serverManager = serverManager;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _serverManager.Init();
        await _serverManager.StartAsync(stoppingToken);
        /*
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            await Task.Delay(1000, stoppingToken);
        }
        */
    }
    protected override async Task StopAsync(CancellationToken stoppingToken)
    {
        await _serverManager.StopAsync(stoppingToken);
    }   
}
