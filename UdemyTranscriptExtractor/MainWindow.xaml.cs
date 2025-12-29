using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Web.WebView2.Core;
using UdemyTranscriptExtractor.ViewModels;
using UdemyTranscriptExtractor.Services;
using UdemyTranscriptExtractor.Helpers;
using UdemyTranscriptExtractor.Controls;
using UdemyTranscriptExtractor.Models;

namespace UdemyTranscriptExtractor;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly NotificationService _notificationService;
    private readonly FileService _fileService;
    private readonly SettingsService _settingsService;
    private readonly EasterEggHandler _easterEggHandler;
    private readonly ConfettiEffect _confettiEffect;
    private Microsoft.Web.WebView2.Wpf.WebView2? _webView;

    public MainWindow()
    {
        InitializeComponent();
        
        // Initialize services
        _settingsService = new SettingsService();
        _notificationService = new NotificationService();
        _fileService = new FileService(_settingsService);
        var transcriptService = new TranscriptService(_settingsService);
        
        // Initialize ViewModel
        _viewModel = new MainViewModel(transcriptService, _settingsService, _notificationService);
        DataContext = _viewModel;
        
        // Connect ViewModel to MainWindow
        _viewModel.OnExtractTriggered = async () => await TriggerExtractAsync();
        _viewModel.OnOpenSettingsRequested = () => OpenSettingsWindow();
        
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
                // Setup message handler
                _webView.CoreWebView2.WebMessageReceived += WebView_WebMessageReceived;
                
                // Setup navigation completed handler
                _webView.CoreWebView2.NavigationCompleted += WebView_NavigationCompleted;
                
                // Use configurable URL (default: fpl.udemy.com for Udemy Business)
                string udemyUrl = _viewModel.UdemyBaseUrl ?? "https://fpl.udemy.com";
                _webView.CoreWebView2.Navigate(udemyUrl);
            }
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to initialize browser: {ex.Message}", "WebView2 Error");
        }
    }

    private async void WebView_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        if (_webView?.CoreWebView2 == null)
            return;
            
        try
        {
            await _webView.CoreWebView2.ExecuteScriptAsync(JavaScriptInjectionCode);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error injecting JavaScript: {ex.Message}");
        }
    }
    
    private async void WebView_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        try
        {
            var json = e.TryGetWebMessageAsString();
            if (string.IsNullOrEmpty(json))
                return;
                
            var message = JsonSerializer.Deserialize<WebMessage>(json);
            if (message == null)
                return;
            
            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                switch (message.Type)
                {
                    case "transcriptAvailable":
                        _viewModel.TranscriptDetected = true;
                        ShowTranscriptBanner();
                        break;
                        
                    case "transcriptDetected":
                        await HandleTranscriptExtracted(message);
                        break;
                        
                    case "transcriptNotFound":
                        _viewModel.TranscriptDetected = false;
                        _notificationService.ShowWarning("No transcript found on this page", "Transcript Not Found");
                        break;
                }
            });
        }
        catch (JsonException ex)
        {
            _notificationService.ShowError("Invalid data received from browser", "Error");
            Console.WriteLine($"JSON parse error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Error processing message: {ex.Message}", "Error");
            Console.WriteLine($"WebMessage error: {ex.Message}");
        }
    }
    
    private async Task HandleTranscriptExtracted(WebMessage message)
    {
        try
        {
            string courseName = ExtractCourseName(message.CourseTitle);
            
            var file = await _fileService.SaveTranscriptAsync(
                message.Content,
                courseName,
                message.LectureId
            );
            
            if (file != null)
            {
                _viewModel.LastExtractedFile = file.FileName;
                _viewModel.TotalExtracted++;
                _viewModel.RecentFiles.Insert(0, file);
                
                while (_viewModel.RecentFiles.Count > 10)
                {
                    _viewModel.RecentFiles.RemoveAt(10);
                }
                
                _notificationService.ShowSuccess($"Saved: {file.FileName}", "Success!");
                _confettiEffect.Burst(30);
            }
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Error saving transcript: {ex.Message}", "Save Error");
        }
    }
    
    private string ExtractCourseName(string pageTitle)
    {
        // Remove " | Udemy" suffix
        pageTitle = Regex.Replace(pageTitle, @"\s*\|\s*Udemy.*$", "");
        
        // Remove lecture numbers like " - 05 - "
        pageTitle = Regex.Replace(pageTitle, @"\s*-\s*\d+\s*-\s*", " ");
        
        return pageTitle.Trim();
    }
    
    public async Task TriggerExtractAsync()
    {
        if (_webView?.CoreWebView2 == null)
        {
            _notificationService.ShowError("Browser not ready", "Error");
            return;
        }
        
        try
        {
            await _webView.CoreWebView2.ExecuteScriptAsync("window.extractTranscriptNow();");
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Error triggering extraction: {ex.Message}", "Error");
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
    
    private void OpenSettingsWindow()
    {
        var settingsWindow = new SettingsWindow(_settingsService, _viewModel)
        {
            Owner = this
        };
        settingsWindow.ShowDialog();
        
        // Reload URL after settings window closes
        _viewModel.LoadUdemyBaseUrl();
    }
    
    private const string JavaScriptInjectionCode = @"
(function() {
    // Prevent multiple injections
    if (window.udemyTranscriptExtractorInjected) {
        return;
    }
    window.udemyTranscriptExtractorInjected = true;
    
    // Check if we're on a Udemy domain (supports business subdomains)
    const isUdemyDomain = window.location.hostname.endsWith('udemy.com');
    if (!isUdemyDomain) return;
    
    // Function to extract transcript with updated selectors for fpl.udemy.com
    function extractTranscript() {
        // Primary selector based on actual fpl.udemy.com structure
        const cueTexts = document.querySelectorAll('[data-purpose=""cue-text""]');
        
        if (cueTexts.length === 0) {
            // Fallback selectors for different Udemy layouts
            const fallbackSelectors = [
                '.transcript--cue-container--Vuwj6 span',
                '[class*=""transcript--cue-container""] span',
                '.transcript--highlight-cue--ugVsE',
                '.dashboard-transcript--transcript-panel-- span',
                '[class*=""cue-text""]',
                '.ud-component--transcript--cue span'
            ];
            
            let foundElements = [];
            for (const selector of fallbackSelectors) {
                try {
                    const elements = document.querySelectorAll(selector);
                    if (elements.length > 0) {
                        foundElements = Array.from(elements);
                        console.log('Found transcript with fallback selector:', selector);
                        break;
                    }
                } catch (e) {
                    console.error('Error with selector:', selector, e);
                }
            }
            
            if (foundElements.length === 0) {
                window.chrome.webview.postMessage({ type: 'transcriptNotFound' });
                return;
            }
            
            // Extract text from fallback elements
            let transcript = '';
            foundElements.forEach((element) => {
                const text = element.textContent.trim();
                if (text && text.length > 0) {
                    transcript += text + '\n';
                }
            });
            
            if (transcript.trim().length > 0) {
                const pageTitle = document.title;
                const url = window.location.href;
                const urlParts = window.location.pathname.split('/');
                
                let courseSlug = 'Unknown-Course';
                let lectureId = 'lecture';
                
                for (let i = 0; i < urlParts.length; i++) {
                    if (urlParts[i] === 'course' && urlParts[i + 1]) {
                        courseSlug = urlParts[i + 1];
                    }
                    if (urlParts[i] === 'lecture' && urlParts[i + 1]) {
                        lectureId = urlParts[i + 1];
                    }
                }
                
                window.chrome.webview.postMessage({
                    type: 'transcriptDetected',
                    content: transcript.trim(),
                    courseTitle: pageTitle,
                    courseSlug: courseSlug,
                    lectureId: lectureId,
                    url: url,
                    domain: window.location.hostname
                });
            } else {
                window.chrome.webview.postMessage({ type: 'transcriptNotFound' });
            }
            return;
        }
        
        // Extract from primary selector (data-purpose=""cue-text"")
        let transcript = '';
        cueTexts.forEach((cue, index) => {
            const text = cue.textContent.trim();
            if (text && text.length > 0) {
                transcript += text + '\n';
            }
        });
        
        // Get page info
        const pageTitle = document.title;
        const url = window.location.href;
        const urlParts = window.location.pathname.split('/');
        
        // Extract course and lecture info from URL
        let courseSlug = 'Unknown-Course';
        let lectureId = 'lecture';
        
        for (let i = 0; i < urlParts.length; i++) {
            if (urlParts[i] === 'course' && urlParts[i + 1]) {
                courseSlug = urlParts[i + 1];
            }
            if (urlParts[i] === 'lecture' && urlParts[i + 1]) {
                lectureId = urlParts[i + 1];
            }
        }
        
        window.chrome.webview.postMessage({
            type: 'transcriptDetected',
            content: transcript.trim(),
            courseTitle: pageTitle,
            courseSlug: courseSlug,
            lectureId: lectureId,
            url: url,
            domain: window.location.hostname
        });
    }
    
    // Monitor DOM for transcript appearance
    const observer = new MutationObserver((mutations) => {
        // Check for transcript panel with actual fpl.udemy.com structure
        const transcriptSelectors = [
            '[data-purpose=""transcript-panel""]',
            '.transcript--transcript-panel--JLceZ',
            '[data-purpose=""cue-text""]',
            '.transcript--cue-container--Vuwj6',
            '[class*=""transcript--cue-container""]',
            '.ud-component--transcript--cue-container',
            '[data-purpose*=""transcript""]'
        ];
        
        for (const selector of transcriptSelectors) {
            try {
                if (document.querySelector(selector)) {
                    window.chrome.webview.postMessage({
                        type: 'transcriptAvailable'
                    });
                    observer.disconnect();
                    return;
                }
            } catch (e) {}
        }
    });
    
    observer.observe(document.body, {
        childList: true,
        subtree: true
    });
    
    // Check immediately on load
    setTimeout(() => {
        const transcriptSelectors = [
            '[data-purpose=""transcript-panel""]',
            '.transcript--transcript-panel--JLceZ',
            '[data-purpose=""cue-text""]',
            '.transcript--cue-container--Vuwj6',
            '[class*=""transcript--cue-container""]'
        ];
        
        for (const selector of transcriptSelectors) {
            try {
                if (document.querySelector(selector)) {
                    window.chrome.webview.postMessage({
                        type: 'transcriptAvailable'
                    });
                    return;
                }
            } catch (e) {}
        }
    }, 1000);
    
    // Make extract function available globally
    window.extractTranscriptNow = extractTranscript;
})();
";
}
