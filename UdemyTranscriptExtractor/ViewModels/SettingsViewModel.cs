using CommunityToolkit.Mvvm.ComponentModel;
using UdemyTranscriptExtractor.Services;

namespace UdemyTranscriptExtractor.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsService _settingsService;
    private readonly MainViewModel _mainViewModel;

    [ObservableProperty]
    private string _udemyBaseUrl = "https://fpl.udemy.com";

    [ObservableProperty]
    private string _outputFolder = string.Empty;

    public SettingsViewModel(SettingsService settingsService, MainViewModel mainViewModel)
    {
        _settingsService = settingsService;
        _mainViewModel = mainViewModel;
        
        LoadSettingsAsync();
    }

    private async void LoadSettingsAsync()
    {
        try
        {
            var settings = await _settingsService.LoadSettingsAsync();
            UdemyBaseUrl = settings.UdemyBaseUrl ?? "https://fpl.udemy.com";
            OutputFolder = settings.OutputFolder;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading settings: {ex.Message}");
        }
    }

    partial void OnUdemyBaseUrlChanged(string value)
    {
        SaveUdemyBaseUrlAsync(value);
        
        // Also update the MainViewModel
        _mainViewModel.UdemyBaseUrl = value;
    }

    private async void SaveUdemyBaseUrlAsync(string url)
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

    public async Task SetOutputFolderAsync(string folderPath)
    {
        try
        {
            var settings = await _settingsService.LoadSettingsAsync();
            settings.OutputFolder = folderPath;
            await _settingsService.SaveSettingsAsync(settings);
            OutputFolder = folderPath;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving output folder: {ex.Message}");
        }
    }
}
