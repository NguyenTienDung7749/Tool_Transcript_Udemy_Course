using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using UdemyTranscriptExtractor.Models;
using UdemyTranscriptExtractor.Services;

namespace UdemyTranscriptExtractor.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly TranscriptService _transcriptService;
    private readonly SettingsService _settingsService;
    private readonly NotificationService _notificationService;
    
    public Action? OnExtractTriggered { get; set; }
    
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
    
    public MainViewModel(
        TranscriptService transcriptService,
        SettingsService settingsService,
        NotificationService notificationService)
    {
        _transcriptService = transcriptService;
        _settingsService = settingsService;
        _notificationService = notificationService;
        
        LoadRecentFilesAsync();
        LoadTotalExtractedCount();
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
        _notificationService.ShowInfo("Settings dialog coming soon!", "Info");
        await Task.CompletedTask;
    }
    
    [RelayCommand]
    private async Task SelectOutputFolderAsync()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = "Select Output Folder",
            FileName = "Select Folder",
            Filter = "Folder|*.folder"
        };
        
        if (dialog.ShowDialog() == true)
        {
            var settings = await _settingsService.LoadSettingsAsync();
            var folderPath = System.IO.Path.GetDirectoryName(dialog.FileName);
            if (!string.IsNullOrEmpty(folderPath))
            {
                settings.OutputFolder = folderPath;
                await _settingsService.SaveSettingsAsync(settings);
                _notificationService.ShowSuccess($"Output folder set to {folderPath}", "Settings Updated");
            }
        }
    }
    
    private async void LoadRecentFilesAsync()
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
    
    private async void LoadTotalExtractedCount()
    {
        try
        {
            var settings = await _settingsService.LoadSettingsAsync();
            TotalExtracted = settings.RecentFiles.Count;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading total count: {ex.Message}");
        }
    }
}
