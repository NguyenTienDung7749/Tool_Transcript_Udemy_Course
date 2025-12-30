using System.Windows;
using UdemyTranscriptExtractor.ViewModels;
using UdemyTranscriptExtractor.Services;

namespace UdemyTranscriptExtractor;

public partial class SettingsWindow : Window
{
    private readonly SettingsViewModel _viewModel;
    private readonly FolderPickerService _folderPickerService = new();

    public SettingsWindow(SettingsService settingsService)
    {
        InitializeComponent();
        
        _viewModel = new SettingsViewModel(settingsService);
        DataContext = _viewModel;
    }

    private void CloseWindow(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void BrowseOutputFolder(object sender, RoutedEventArgs e)
    {
        var folderPath = _folderPickerService.PickFolder();
        if (!string.IsNullOrWhiteSpace(folderPath))
        {
            await _viewModel.SetOutputFolderAsync(folderPath);
        }
    }
}
