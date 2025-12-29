using System.Windows;
using UdemyTranscriptExtractor.ViewModels;
using UdemyTranscriptExtractor.Services;

namespace UdemyTranscriptExtractor;

public partial class SettingsWindow : Window
{
    private readonly SettingsViewModel _viewModel;

    public SettingsWindow(SettingsService settingsService, MainViewModel mainViewModel)
    {
        InitializeComponent();
        
        _viewModel = new SettingsViewModel(settingsService, mainViewModel);
        DataContext = _viewModel;
    }

    private void CloseWindow(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void BrowseOutputFolder(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = "Select Output Folder",
            FileName = "Select Folder",
            Filter = "Folder|*.folder"
        };

        if (dialog.ShowDialog() == true)
        {
            var folderPath = System.IO.Path.GetDirectoryName(dialog.FileName);
            if (!string.IsNullOrEmpty(folderPath))
            {
                await _viewModel.SetOutputFolderAsync(folderPath);
            }
        }
    }
}
