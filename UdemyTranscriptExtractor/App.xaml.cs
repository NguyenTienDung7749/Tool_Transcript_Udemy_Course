using System.Windows;

namespace UdemyTranscriptExtractor;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Set up unhandled exception handling
        DispatcherUnhandledException += (sender, args) =>
        {
            System.Windows.MessageBox.Show($"An unexpected error occurred: {args.Exception.Message}", 
                          "Error", 
                          MessageBoxButton.OK, 
                          MessageBoxImage.Error);
            args.Handled = true;
        };
    }
}
