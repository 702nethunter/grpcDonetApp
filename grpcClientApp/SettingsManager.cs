using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
public class Settings{
    public HostDetails HostData{get;set;}=new();
}
public class HostDetails
{
    public long HostId{get;set;}
}
public class SettingsManager
{
    private  string _settingsPath;
    private static readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1,1);
    private ILogger<SettingsManager> _logger;
    public SettingsManager(ILogger<SettingsManager> logger)
    {
        _settingsPath = Path.Combine(AppContext.BaseDirectory,"settings.json");
        _logger =logger;
    }
    public async Task<Settings> ReadSettingsAsync()
    {
        await _fileLock.WaitAsync();
        try
        {
            if(!File.Exists(_settingsPath))
            {   
               _logger.LogWarning("settings.json not found at {Path}. Creating a new one at application base directory.", _settingsPath);

                _settingsPath = Path.Combine(AppContext.BaseDirectory,"settings.json");

               var defaultSettings = @"{
                        ""HostDetails"": {
                            ""HostID"": 0
                        }
                        }";
                File.WriteAllText(_settingsPath,defaultSettings);

                _logger.LogInformation("Created new settings.json at {Path}",_settingsPath);

            }
            string json = await File.ReadAllTextAsync(_settingsPath);
            return JsonSerializer.Deserialize<Settings>(json)??new Settings();
        }
        finally
        {
            _fileLock.Release();
        }
    }
   public async Task WriteSettingsAsync(Settings settings)
   {
     await _fileLock.WaitAsync();
     try
     {
        string json = JsonSerializer.Serialize(settings,new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_settingsPath,json);
     }
     finally
     {
        _fileLock.Release();
     }
   }
}