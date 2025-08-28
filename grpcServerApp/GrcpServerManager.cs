using Monster.Core;
using System.Collections.Concurrent;
public class GrpcServerManager
{
    private readonly ILogger<GrpcServerManager> _logger;
    private CancellationToken _stoppingToken;
    private readonly HostDataAccess _dataAccess;
    private IConfiguration _config;
    private ConcurrentDictionary<long,HostStorage> _hostIdDict = new ConcurrentDictionary<long,HostStorage>();
    public GrpcServerManager(ILogger<GrpcServerManager> logger,IConfiguration configuration,HostDataAccess dataAccess)
    {
        _logger=logger;
        _config = configuration;
       
        _dataAccess = dataAccess;
    }
    public void Init()
    {

    }
    public async Task<HostDataResponse> ProcessClientHostDataRequest(HostDataRequest request)
    {
        if(!_hostIdDict.ContainsKey(request.HostId))
        {
            _hostIdDict.TryAdd(request.HostId,new HostStorage(){
                HostID = request.HostId,
                HostAddedTime = DateTime.UtcNow,
                HostName = request.HostName,
                TTLMillisecond=0
            });
            _logger.LogInformation($"Added HostID:{request.HostId} to host Dict");
        }
        try
        {
              //Store in DB
            await _dataAccess.UpsertClientHostDataAsync(request.HostId,
            request.HostName,
            request.HostIp,
            request.ClientOsVersion,
            request.ClientVersion);

        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex,"Error in processing ClientHostData");   
        }
        // ✅ return the response (don’t just call Task.FromResult without returning)
        return new HostDataResponse
        {
            HostId = request.HostId,
            IsEnabled = true,
            KeepAliveSeconds = 200
        };
      
    }
    public async Task StartAsync(CancellationToken stoppingToken)
    {   
        _stoppingToken = stoppingToken;

    }
    public async Task StopAsync(CancellationToken stoppingToken)
    {
        
    }
}