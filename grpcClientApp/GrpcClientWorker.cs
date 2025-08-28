using System.Net;
using System.Net.Sockets;
using System.Diagnostics; 
namespace grpcClientApp;

public class GrpcClientWorker : BackgroundService
{
    private readonly ILogger<GrpcClientWorker> _logger;
    private readonly IConfiguration _config;
    private long _clientSequenceNumber=0;
    private long _cachedSessionId=0;
    private long _cachedHostId=0;
    private SnowflakeIdGenerator _idGenerator;
    private string _serverAddress;
    private bool _allowInsecureHttp2;
    private SettingsManager _settingsManager;
    private CoreService _coreService;
    public GrpcClientWorker(ILogger<GrpcClientWorker> logger,IConfiguration config,SnowflakeIdGenerator idGenerator ,
    SettingsManager settingsManager,CoreService coreService )
    {
        _logger = logger;
        _config =config;
        _settingsManager = settingsManager;
        _idGenerator = idGenerator;
        _serverAddress = _config["GrpcServer:Address"]??"http://localhost:5001";
      _allowInsecureHttp2 = _serverAddress.StartsWith("http://", StringComparison.OrdinalIgnoreCase);
       _coreService = coreService;
    }
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _cachedSessionId = _idGenerator.NextId();
        _cachedHostId = await this.InitializeHostId();
          
        await base.StartAsync(cancellationToken);
     
        _logger.LogInformation("GrpcClientWorker: Start Async Completed");
    }
    
    private async Task<long> InitializeHostId()
    {
        try{

            Settings settings = await _settingsManager.ReadSettingsAsync();
            if(settings.HostData.HostId==0)
            {
                settings.HostData.HostId = _idGenerator.NextId();
                _logger.LogInformation("Generated new Host ID:{HostId}",settings.HostData.HostId);
                await _settingsManager.WriteSettingsAsync(settings);
            }
            else
            {
                _logger.LogInformation("Reusing existing HostID:{HostId}",settings.HostData.HostId);
            }
            return settings.HostData.HostId;

        }
        catch(System.Exception ex)
        {
            _logger.LogError("Error in reading Settings File");
            return -1;
        }
    }
    private string GetMachineName()
    {
        return Environment.MachineName;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("GrpcClientWorker: Started Execute Async Started");
        await using var telemetry = new TelemetryClient(_serverAddress,_allowInsecureHttp2);
      
        var sentAtMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();


        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
              var startTicks = Stopwatch.GetTimestamp();
            var response = await telemetry.SendClientHostDataAsync(
                _cachedHostId,
                this.GetMachineName(),
                _coreService.GetLocalIpAddress(),
                Interlocked.Increment(ref _clientSequenceNumber),
                _idGenerator.NextId(),
                _coreService.GetOsInfo(),
                 _cachedSessionId,
                 "1.0.0",
                 _coreService.GetTimestampMs());

            var ttlMs = (Stopwatch.GetTimestamp()-startTicks)*1000/Stopwatch.Frequency;
            _logger.LogInformation("SendClientHostData RTT /TTL ={TTLms:F2} ms",ttlMs);

            //Now report the TTL to the back end 
            var reportAck = await telemetry.ReportRoundTripTimeAsync(_cachedHostId,_cachedSessionId,_idGenerator.NextId(),
            Interlocked.Increment(ref _clientSequenceNumber),_coreService.GetTimestampMs(),ttlMs);

            _logger.LogInformation(
                "Received TTL ack from Server for HostID:{HostId}, RequestID:{RequestId}",
                reportAck.HostId,
                reportAck.RequestId);

            
            //measure the response time and then resport it baack
            await Task.Delay(10000, stoppingToken);
        }
        
    }
}
