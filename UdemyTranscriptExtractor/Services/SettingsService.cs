using System.IO;
using System.Text.Json;
using UdemyTranscriptExtractor.Models;

namespace UdemyTranscriptExtractor.Services;

public class SettingsService
{
    private readonly string _settingsPath;
    private AppSettings? _cachedSettings;
    
    public SettingsService()
    {
        var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UdemyTranscriptExtractor");
        Directory.CreateDirectory(appDataPath);
        _settingsPath = Path.Combine(appDataPath, "settings.json");
    }
    
    public async Task<AppSettings> LoadSettingsAsync()
    {
        if (_cachedSettings != null)
            return _cachedSettings;
            
        if (!File.Exists(_settingsPath))
        {
            _cachedSettings = new AppSettings();
            await SaveSettingsAsync(_cachedSettings);
            return _cachedSettings;
        }
        
        try
        {
            var json = await File.ReadAllTextAsync(_settingsPath);
            _cachedSettings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            return _cachedSettings;
        }
        catch
        {
            _cachedSettings = new AppSettings();
            return _cachedSettings;
        }
    }
    
    public async Task SaveSettingsAsync(AppSettings settings)
    {
        _cachedSettings = settings;
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        await File.WriteAllTextAsync(_settingsPath, json);
    }
    
    public async Task AddRecentFileAsync(string filePath)
    {
        var settings = await LoadSettingsAsync();
        
        if (settings.RecentFiles.Contains(filePath))
            settings.RecentFiles.Remove(filePath);
            
        settings.RecentFiles.Insert(0, filePath);
        
        if (settings.RecentFiles.Count > settings.MaxRecentFiles)
            settings.RecentFiles = settings.RecentFiles.Take(settings.MaxRecentFiles).ToList();
            
        await SaveSettingsAsync(settings);
    }
}
