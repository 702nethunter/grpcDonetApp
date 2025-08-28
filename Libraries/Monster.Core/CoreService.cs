using System;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.Extensions.Logging;
using System.Net;             // for Dns, IPHostEntry, IPAddress
using System.Net.Sockets;     // for AddressFamily
public class CoreService
{
    private ILogger<CoreService> _logger;
    public CoreService(ILogger<CoreService> logger)
    {
        _logger = logger;
    }
    public string GetVersion()
    {
        return "Monster.Core v1.0.0";
    }
         public string GetOsInfo()
        {
            string osName;
            string osVersion=string.Empty;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                osName = "Windows";
                osVersion = Environment.OSVersion.Version.ToString();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                osName = "Mac";
                osVersion = Environment.OSVersion.Version.ToString();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                osName = "Linux";
                // Try to identify specific Linux distribution
                if (File.Exists("/etc/os-release"))
                {
                    var lines = File.ReadAllLines("/etc/os-release");
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("NAME="))
                        {
                            osName = line.Substring(5).Trim('"');
                            if (osName.Contains("Ubuntu"))
                                osName = "Ubuntu"; // Normalize for reporting
                        }
                        else if (line.StartsWith("VERSION_ID="))
                        {
                            osVersion = line.Substring(11).Trim('"');
                        }
                    }
                }
                else
                {
                    osVersion = Environment.OSVersion.Version.ToString();
                }
            }
            else
            {
                osName = "Unknown";
                osVersion = Environment.OSVersion.Version.ToString();
            }

            return $"{osName} {osVersion}";
        }
        public long GetTimestampMs()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    public string GetLocalIpAddress()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork) // IPv4
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1"; // Fallback to localhost
        }
        catch (Exception ex)
        {
           _logger.LogError("Error in getting IP Addresss");
            return "127.0.0.1"; // Fallback
        }
    }
}