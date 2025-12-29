using System.Windows;

namespace UdemyTranscriptExtractor;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Set up unhandled exception handling
        DispatcherUnhandledException += (sender, args) =>
        {
            MessageBox.Show($"An unexpected error occurred: {args.Exception.Message}", 
                          "Error", 
                          MessageBoxButton.OK, 
                          MessageBoxImage.Error);
            args.Handled = true;
        };
    }
}
