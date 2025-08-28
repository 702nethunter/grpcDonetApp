using Monster.Telemetry.V1;
using System.Threading.Tasks;

using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Monster.Core;
public class ClientTelemetryService:ClientTelemetry.ClientTelemetryBase
{
    private ILogger<ClientTelemetryService> _logger;
    private GrpcServerManager _serverManager;
    private CoreService _coreService;
    public ClientTelemetryService(ILogger<ClientTelemetryService> logger,GrpcServerManager serverManager,CoreService coreService)
    {
        _logger = logger;
        _serverManager = serverManager;
        _coreService = coreService;
    }
    
    public override async Task<RttReportAck> ReportRoundTripTime(RttReportRequest request,ServerCallContext context)  
    {
        _logger.LogInformation($"Processing Round Trip time for HostId:{request.HostId},TTL:{request.TtlMilliseconds}");
        TTLRequest ttlRequest = new();
        ttlRequest.HostId = request.HostId;
        ttlRequest.TTL = request.TtlMilliseconds;
        ttlRequest.DateRecieved = DateTimeOffset
            .FromUnixTimeMilliseconds(request.TimestampMs)
            .ToLocalTime()
            .DateTime;
        var result = await _serverManager.ProcessTTLData(ttlRequest);
        RttReportAck ack=new();
        ack.SessionId = request.SessionId;
        ack.RequestId = request.RequestId;
        ack.HostId = request.HostId;
        ack.TimestampMs = _coreService.GetTimestampMs();
        _logger.LogInformation($"Sending TTL Report Ack to HostId:{request.HostId},SessionId:{request.SessionId},RequestId:{request.RequestId}");
        return ack;
    }
    public override async Task<ClientHostDataResponse> SendClientHostData(
             ClientHostDataRequest request,
                ServerCallContext context)
    {
        HostDataRequest hostDataRequest = new()
        {
            HostId = request.HostId,
            HostName = request.HostName,
            HostIp = request.HostIp,
            ClientOsVersion = request.ClientOsVersion,
            ClientVersion = request.ClientVersion
        };

        HostDataResponse response = await _serverManager.ProcessClientHostDataRequest(hostDataRequest);

        ClientHostDataResponse clientResponse = new()
        {
            HostId = response.HostId,
            IsEnabled = response.IsEnabled,
            KeepAliveSeconds = response.KeepAliveSeconds
        };

        _logger.LogInformation("Sending response to HostID {HostId} , response {Response}", 
            request.HostId, response);

        
        return clientResponse;
    }
   

}