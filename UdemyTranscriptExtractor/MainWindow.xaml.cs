using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Microsoft.Web.WebView2.Core;
using UdemyTranscriptExtractor.ViewModels;
using UdemyTranscriptExtractor.Services;
using UdemyTranscriptExtractor.Helpers;
using UdemyTranscriptExtractor.Controls;

namespace UdemyTranscriptExtractor;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly NotificationService _notificationService;
    private readonly EasterEggHandler _easterEggHandler;
    private readonly ConfettiEffect _confettiEffect;
    private Microsoft.Web.WebView2.Wpf.WebView2? _webView;

    public MainWindow()
    {
        InitializeComponent();
        
        // Initialize services
        var settingsService = new SettingsService();
        _notificationService = new NotificationService();
        var transcriptService = new TranscriptService(settingsService);
        
        // Initialize ViewModel
        _viewModel = new MainViewModel(transcriptService, settingsService, _notificationService);
        DataContext = _viewModel;
        
        // Setup notifications
        _notificationService.NotificationRequested += OnNotificationRequested;
        
        // Setup Easter Egg
        _easterEggHandler = new EasterEggHandler(this);
        _easterEggHandler.KonamiCodeActivated += OnKonamiCodeActivated;
        
        // Setup Confetti
        _confettiEffect = new ConfettiEffect(ConfettiCanvas);
        
        // Window drag
        MouseLeftButtonDown += (s, e) =>
        {
            if (e.OriginalSource is not System.Windows.Controls.Button)
                DragMove();
        };
        
        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Startup animation
        await StartupAnimation();
        
        // Initialize WebView2
        await InitializeWebView2Async();
    }

    private async Task StartupAnimation()
    {
        // Fade in animation
        this.Opacity = 0;
        var fadeAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromSeconds(0.5),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        
        this.BeginAnimation(OpacityProperty, fadeAnimation);
        
        await Task.Delay(500);
    }

    private async Task InitializeWebView2Async()
    {
        try
        {
            _webView = new Microsoft.Web.WebView2.Wpf.WebView2();
            WebView2Container.Child = _webView;
            
            await _webView.EnsureCoreWebView2Async();
            
            if (_webView.CoreWebView2 != null)
            {
                _webView.CoreWebView2.Navigate("https://www.udemy.com");
                
                // Listen for navigation
                _webView.CoreWebView2.NavigationCompleted += (s, e) =>
                {
                    CheckForTranscript();
                };
            }
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to initialize browser: {ex.Message}", "WebView2 Error");
        }
    }

    private void CheckForTranscript()
    {
        // Simulate transcript detection
        // In a real scenario, you would check the page content
        var random = new Random();
        if (random.Next(0, 3) == 0) // 33% chance to simulate transcript detection
        {
            ShowTranscriptBanner();
        }
    }

    private void ShowTranscriptBanner()
    {
        TranscriptBanner.Visibility = Visibility.Visible;
        
        var storyboard = new Storyboard();
        
        var slideAnimation = new DoubleAnimation
        {
            From = -100,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        
        var fadeAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(300)
        };
        
        Storyboard.SetTarget(slideAnimation, BannerTransform);
        Storyboard.SetTargetProperty(slideAnimation, new PropertyPath("Y"));
        
        Storyboard.SetTarget(fadeAnimation, TranscriptBanner);
        Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath("Opacity"));
        
        storyboard.Children.Add(slideAnimation);
        storyboard.Children.Add(fadeAnimation);
        
        storyboard.Begin();
    }

    private void OnNotificationRequested(object? sender, NotificationEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            var toast = new ToastNotification();
            ToastContainer.Children.Add(toast);
            
            toast.Show(e.Title, e.Message, e.Type);
            
            // Trigger confetti on success
            if (e.Type == NotificationType.Success)
            {
                _confettiEffect.Burst(30);
            }
        });
    }

    private void OnKonamiCodeActivated(object? sender, EventArgs e)
    {
        _notificationService.ShowSuccess("Konami Code Activated! 🎉", "Easter Egg");
        _confettiEffect.Burst(100);
        
        // RGB party mode animation
        var colorAnimation = new ColorAnimation
        {
            From = System.Windows.Media.Colors.Purple,
            To = System.Windows.Media.Colors.Cyan,
            Duration = TimeSpan.FromSeconds(2),
            AutoReverse = true,
            RepeatBehavior = new RepeatBehavior(3)
        };
        
        // Apply to window border (would need proper implementation)
    }

    private void MinimizeWindow(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MaximizeRestoreWindow(object sender, RoutedEventArgs e)
    {
        if (WindowState == WindowState.Maximized)
        {
            WindowState = WindowState.Normal;
            MaximizeIcon.Data = System.Windows.Media.Geometry.Parse("M4,4H20V20H4V4M6,8V18H18V8H6Z");
        }
        else
        {
            WindowState = WindowState.Maximized;
            MaximizeIcon.Data = System.Windows.Media.Geometry.Parse("M4,8H8V4H20V16H16V20H4V8M16,8V14H18V6H10V8H16M6,12V18H14V12H6Z");
        }
    }

    private void CloseWindow(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
