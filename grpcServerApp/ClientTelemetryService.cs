using Monster.Telemetry.V1;
using System.Threading.Tasks;

using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Monster.Core;
public class ClientTelemetryService:ClientTelemetry.ClientTelemetryBase
{
    private ILogger<ClientTelemetryService> _logger;
    private GrpcServerManager _serverManager;
    public ClientTelemetryService(ILogger<ClientTelemetryService> logger,GrpcServerManager serverManager)
    {
        _logger = logger;
        _serverManager = serverManager;
    }
    
    public override Task<RttReportAck> ReportRoundTripTime(RttReportRequest request,ServerCallContext context)  
    {
        RttReportAck ack=new();
        return Task.FromResult(ack);
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