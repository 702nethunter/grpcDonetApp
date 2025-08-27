using Monster.Telemetry.V1;
using System.Threading.Tasks;

using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
public class ClientTelemetryService:ClientTelemetry.ClientTelemetryBase
{
    private ILogger<ClientTelemetryService> _logger;
    private GrpcServerManager _serverManager;
    public ClientTelemetryService(ILogger<ClientTelemetryService> logger,GrpcServerManager serverManager)
    {
        _logger = logger;
        _serverManager = serverManager;
    }
    public override Task<ClientHostDataResponse> SendClientHostData(ClientHostDataRequest request,ServerCallContext context)
    {
        ClientHostDataResponse response =new();
        return Task.FromResult(response);
    }
    public override Task<RttReportAck> ReportRoundTripTime(RttReportRequest request,ServerCallContext context)  
    {
        RttReportAck ack=new();
        return Task.FromResult(ack);
    }
}