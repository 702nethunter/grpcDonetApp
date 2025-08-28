using Monster.Core;
using System.Collections.Concurrent;
public class GrpcServerManager
{
    private readonly ILogger<GrpcServerManager> _logger;
    private CancellationToken _stoppingToken;
    private readonly HostDataAccess _dataAccess;
    private IConfiguration _config;
    private ConcurrentDictionary<long,HostStorage> _hostIdDict = new ConcurrentDictionary<long,HostStorage>();
    private CoreService _coreService;
    public GrpcServerManager(ILogger<GrpcServerManager> logger,IConfiguration configuration,HostDataAccess dataAccess,
    CoreService coreService)
    {
        _logger=logger;
        _config = configuration;
        _coreService = coreService;      
        _dataAccess = dataAccess;
    }
    public void Init()
    {

    }
    public async Task<int> ProcessTTLData(TTLRequest request)
    {
        try
        {
              //Store in DB
       

        await _dataAccess.UpsertClientHostMetricAsync(
            request.HostId,
            request.TTL,
            request.DateRecieved);

        _logger.LogInformation(
            "Processed TTL information for HostId:{HostId}, TTL:{TTL}, Date:{DateAdded}",
            request.HostId,
            request.TTL,
             request.DateRecieved);


        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex,"Error in processing ClientHostData");   
            return -1;
        }
        return 0;
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