using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using UdemyTranscriptExtractor.Models;
using UdemyTranscriptExtractor.Services;

namespace UdemyTranscriptExtractor.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly SettingsService _settingsService;
    private readonly NotificationService _notificationService;
    private readonly FolderPickerService _folderPickerService;
    
    public Action? OnExtractTriggered { get; set; }
    public Action? OnOpenSettingsRequested { get; set; }
    
    [ObservableProperty]
    private bool _isSidebarVisible = true;
    
    [ObservableProperty]
    private bool _isExtracting = false;
    
    [ObservableProperty]
    private bool _transcriptDetected = false;
    
    [ObservableProperty]
    private int _totalExtracted = 0;
    
    [ObservableProperty]
    private string _lastExtractedFile = string.Empty;
    
    [ObservableProperty]
    private ObservableCollection<ExtractedFile> _recentFiles = new();
    
    [ObservableProperty]
    private string _currentUrl = "https://www.udemy.com";
    
    [ObservableProperty]
    private string _udemyBaseUrl = "https://fpl.udemy.com";
    
    public MainViewModel(
        SettingsService settingsService,
        NotificationService notificationService,
        FolderPickerService folderPickerService)
    {
        _settingsService = settingsService;
        _notificationService = notificationService;
        _folderPickerService = folderPickerService;
        
        _ = LoadRecentFilesAsync();
        _ = LoadTotalExtractedCountAsync();
        _ = LoadUdemyBaseUrlAsync();
    }
    
    [RelayCommand]
    private void ToggleSidebar()
    {
        IsSidebarVisible = !IsSidebarVisible;
    }
    
    [RelayCommand]
    private async Task ExtractTranscriptAsync()
    {
        if (IsExtracting || !TranscriptDetected)
            return;
        
        try
        {
            IsExtracting = true;
            
            // Trigger extraction through MainWindow
            OnExtractTriggered?.Invoke();
            
            await Task.Delay(100); // Small delay for UI feedback
        }
        catch (Exception ex)
        {
            _notificationService.ShowError(ex.Message, "Error");
        }
        finally
        {
            IsExtracting = false;
        }
    }
    
    [RelayCommand]
    private async Task OpenSettingsAsync()
    {
        OnOpenSettingsRequested?.Invoke();
        await Task.CompletedTask;
    }
    
    [RelayCommand]
    private async Task SelectOutputFolderAsync()
    {
        var folderPath = _folderPickerService.PickFolder();
        if (string.IsNullOrWhiteSpace(folderPath))
            return;

        var settings = await _settingsService.LoadSettingsAsync();
        settings.OutputFolder = folderPath;
        await _settingsService.SaveSettingsAsync(settings);
        _notificationService.ShowSuccess($"Output folder set to {folderPath}", "Settings Updated");
    }
    
    private async Task LoadRecentFilesAsync()
    {
        try
        {
            var settings = await _settingsService.LoadSettingsAsync();
            RecentFiles.Clear();
            
            foreach (var filePath in settings.RecentFiles.Take(10))
            {
                if (System.IO.File.Exists(filePath))
                {
                    var fileInfo = new System.IO.FileInfo(filePath);
                    RecentFiles.Add(new ExtractedFile
                    {
                        FileName = fileInfo.Name,
                        FilePath = filePath,
                        ExtractedAt = fileInfo.LastWriteTime,
                        FileSize = fileInfo.Length,
                        LectureName = System.IO.Path.GetFileNameWithoutExtension(fileInfo.Name)
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading recent files: {ex.Message}");
        }
    }
    
    private async Task LoadTotalExtractedCountAsync()
    {
        try
        {
            var settings = await _settingsService.LoadSettingsAsync();
            TotalExtracted = settings.TotalExtractedCount;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading total count: {ex.Message}");
        }
    }
    
    public void LoadUdemyBaseUrl()
    {
        _ = LoadUdemyBaseUrlAsync();
    }
    
    private async Task LoadUdemyBaseUrlAsync()
    {
        try
        {
            var settings = await _settingsService.LoadSettingsAsync();
            UdemyBaseUrl = settings.UdemyBaseUrl ?? "https://fpl.udemy.com";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading Udemy URL: {ex.Message}");
        }
    }
    
    partial void OnUdemyBaseUrlChanged(string value)
    {
        _ = SaveUdemyBaseUrlAsync(value);
    }
    
    private async Task SaveUdemyBaseUrlAsync(string url)
    {
        try
        {
            var settings = await _settingsService.LoadSettingsAsync();
            settings.UdemyBaseUrl = url;
            await _settingsService.SaveSettingsAsync(settings);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving Udemy URL: {ex.Message}");
        }
    }
}
