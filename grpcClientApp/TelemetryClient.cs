using Grpc.Net.Client;
using Monster.Telemetry.V1;  // comes from option csharp_namespace in prot

public class TelemetryClient:IAsyncDisposable
{
    private readonly GrpcChannel _channel;
    private readonly ClientTelemetry.ClientTelemetryClient _client;

    public TelemetryClient(string serverAddress,bool allowInsecureHttp2=false)
    {
        if (serverAddress.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && allowInsecureHttp2)
        {
           var sockets = new SocketsHttpHandler
            {
                EnableMultipleHttp2Connections = true
            };
            sockets.SslOptions=null;
            sockets.AllowAutoRedirect=false;

            _channel = GrpcChannel.ForAddress(serverAddress,new GrpcChannelOptions{
                HttpHandler = sockets   
            });
        }
        else
        {
            _channel = GrpcChannel.ForAddress(serverAddress);
        }
        _client = new ClientTelemetry.ClientTelemetryClient(_channel);

    }
   public async Task<ClientHostDataResponse> SendClientHostDataAsync(
        long hostId,
        string hostName,
        string hostIp,
        long clientSequenceNum,
        long requestId,
        string clientOsVersion,
        long sessionId,
        string clientVersion,
        long timeStampMs,
        CancellationToken ct = default)
    {
        var req = new ClientHostDataRequest
        {
            HostId = hostId,
            HostName = hostName ?? "",
            HostIp = hostIp ?? "",
            ClientSequenceNum = clientSequenceNum,
            RequestId = requestId,
            ClientOsVersion = clientOsVersion ?? "",
            SessionId = sessionId,
            ClientVersion = clientVersion ?? "",
            TimestampMs = timeStampMs
        };

        return await _client.SendClientHostDataAsync(req, cancellationToken: ct);
    }

    public async Task<RttReportAck> ReportRoundTripTimeAsync(
        long hostId,
        long sessionId,
        long requestId,
        long clientSequenceNum,
        long timestampMs,
        double ttlMilliseconds,
        CancellationToken ct=default
    )
    {
        var req = new RttReportRequest
        {
            HostId = hostId,
            SessionId = sessionId,
            RequestId = requestId,
            ClientSequenceNum = clientSequenceNum,
            TimestampMs =timestampMs,
            TtlMilliseconds = ttlMilliseconds 
        };

        return await _client.ReportRoundTripTimeAsync(req,cancellationToken:ct);
    }
    public async ValueTask DisposeAsync()
    {
        await _channel.ShutdownAsync();
        _channel.Dispose();
    }

}