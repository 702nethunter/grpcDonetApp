namespace Monster.Core;

public class HostDataRequest
{
    public long HostId {get;set;}
    public string HostName {get;set;}
    public string HostIp {get;set;}
    public string ClientOsVersion {get;set;}
    public string ClientVersion {get;set;}

}
public class HostDataResponse
{
    public long HostId {get;set;}
    public bool IsEnabled {get;set;}
    public int KeepAliveSeconds {get;set;}
}
public class TTLRequest 
{
    public long HostId{get;set;}
    public double TTL {get;set;}
    public DateTime DateRecieved{get;set;}
}
