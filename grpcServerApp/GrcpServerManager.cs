public class GrpcServerManager
{
    private readonly ILogger<GrpcServerManager> _logger;
    private CancellationToken _stoppingToken;
    private readonly HostDataAccess _dataAccess;
    private IConfiguration _config;
    private ConcurrentDictionary<long,HostStorage> _hostIdDict = new ConcurrentDictionary<long,HostStorage>();
    public GrpcServerManager(ILogger<GrpcServerManager> logger,IConfiguration configuration)
    {
        _logger=logger;
        _config = configuration;
         var connStr = _config.GetConnectionString("PostgresDb");
        _dataAccess = new HostDataAccess(connStr);
    }
    public void Init()
    {

    }
    public Task<ClientHostDataResponse> ProcessClientHostDataRequest(ClientHostDataRequest request)
    {
        if(!_hostIdDict.ContainsKey(request.HostID))
        {
            _hostIdDict.TryAdd(request.HostID,new HostStorage(){
                HostID = request.HostID,
                HostAddedTime = DateTime.UtcNow,
                HostName = request.HostName,
                TTLMillisecond=0
            });
            _logger.LogInformation($"Added HostID:{request.HostID} to host Dict");
        }
    }
    public async Task StartAsync(CancellationToken stoppingToken)
    {   
        _stoppingToken = stoppingToken;

    }
    public async Task StopAsync(CancellationToken stoppingToken)
    {
        
    }
}