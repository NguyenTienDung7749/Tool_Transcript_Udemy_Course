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
    private readonly FolderPickerService _folderPickerService;
    private readonly EasterEggHandler _easterEggHandler;
    private readonly ConfettiEffect _confettiEffect;
    private Microsoft.Web.WebView2.Wpf.WebView2? _webView;

    public MainWindow()
    {
        InitializeComponent();
        
        // Initialize services
        _settingsService = new SettingsService();
        _notificationService = new NotificationService();
        _folderPickerService = new FolderPickerService();
        _fileService = new FileService(_settingsService, _folderPickerService);
        
        // Initialize ViewModel
        _viewModel = new MainViewModel(_settingsService, _notificationService, _folderPickerService);
        DataContext = _viewModel;
        _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
        
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
                // Setup message handler FIRST before anything else
                _webView.CoreWebView2.WebMessageReceived += WebView_WebMessageReceived;
                
                // Add script to run on every document creation (more reliable than NavigationCompleted)
                await _webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(JavaScriptInjectionCode);
                
                // Setup navigation completed handler for debugging
                _webView.CoreWebView2.NavigationCompleted += WebView_NavigationCompleted;
                
                // Use configurable URL (default: fpl.udemy.com for Udemy Business)
                string udemyUrl = _viewModel.UdemyBaseUrl ?? "https://fpl.udemy.com";
                _webView.CoreWebView2.Navigate(udemyUrl);
                
                System.Diagnostics.Debug.WriteLine("[WebView2] Initialization complete, WebMessageReceived handler attached");
            }
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to initialize browser: {ex.Message}", "WebView2 Error");
        }
    }

    private void ViewModelOnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.UdemyBaseUrl))
        {
            _ = NavigateToUdemyBaseUrlAsync();
        }
    }

    private Task NavigateToUdemyBaseUrlAsync()
    {
        if (_webView?.CoreWebView2 == null)
            return Task.CompletedTask;

        var url = _viewModel.UdemyBaseUrl ?? "https://fpl.udemy.com";
        _webView.CoreWebView2.Navigate(url);
        return Task.CompletedTask;
    }

    private void WebView_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        // Script is now injected via AddScriptToExecuteOnDocumentCreatedAsync
        // This handler is kept for debugging purposes
        System.Diagnostics.Debug.WriteLine($"[WebView2] Navigation completed: {e.IsSuccess}, URL: {_webView?.Source}");
    }
    
    private async void WebView_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        try
        {
            // Since JavaScript sends JSON string (not object), use TryGetWebMessageAsString
            var json = e.TryGetWebMessageAsString();
            
            // Debug output - THIS SHOULD SHOW IN VISUAL STUDIO OUTPUT WINDOW
            System.Diagnostics.Debug.WriteLine($"[WebView2] *** RECEIVED MESSAGE ***: {json}");
            System.Diagnostics.Trace.WriteLine($"[WebView2] *** RECEIVED MESSAGE ***: {json}");
            
            // Show a visual notification to confirm message was received
            _notificationService.ShowInfo($"Message received!", "Debug");
            
            if (string.IsNullOrEmpty(json))
            {
                // Fallback to WebMessageAsJson
                json = e.WebMessageAsJson;
                System.Diagnostics.Debug.WriteLine($"[WebView2] Fallback to WebMessageAsJson: {json}");
            }
            
            if (string.IsNullOrEmpty(json))
            {
                System.Diagnostics.Debug.WriteLine("[WebView2] Message is empty");
                return;
            }
            
            var message = JsonSerializer.Deserialize<WebMessage>(json);
            if (message == null)
            {
                System.Diagnostics.Debug.WriteLine("[WebView2] Failed to deserialize message");
                return;
            }
            
            System.Diagnostics.Debug.WriteLine($"[WebView2] Message type: {message.Type}");
            
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                switch (message.Type)
                {
                    case "transcriptAvailable":
                        System.Diagnostics.Debug.WriteLine("[WebView2] Setting TranscriptDetected = true");
                        _viewModel.TranscriptDetected = true;
                        _notificationService.ShowSuccess("Transcript found! Click to extract.", "Ready");
                        break;
                        
                    case "transcriptDetected":
                        await HandleTranscriptExtracted(message);
                        break;
                        
                    case "transcriptNotFound":
                        System.Diagnostics.Debug.WriteLine("[WebView2] Received transcriptNotFound message");
                        _viewModel.TranscriptDetected = false;
                        _notificationService.ShowWarning("No transcript available for this lecture", "Not Found");
                        break;
                }
            });
        }
        catch (JsonException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[WebView2] JSON parse error: {ex.Message}");
            _notificationService.ShowError("Invalid data received from browser", "Error");
            Console.WriteLine($"JSON parse error: {ex.Message}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[WebView2] Error: {ex.Message}");
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
                
                while (_viewModel.RecentFiles.Count > 5)
                {
                    _viewModel.RecentFiles.RemoveAt(5);
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
        System.Diagnostics.Debug.WriteLine("[MainWindow] TriggerExtractAsync called");
        
        if (_webView?.CoreWebView2 == null)
        {
            System.Diagnostics.Debug.WriteLine("[MainWindow] Browser not ready");
            _notificationService.ShowError("Browser not ready", "Error");
            return;
        }
        
        try
        {
            System.Diagnostics.Debug.WriteLine("[MainWindow] Executing window.extractTranscriptNow()");
            await _webView.CoreWebView2.ExecuteScriptAsync("console.log('[UdemyExtractor] Manual extraction triggered'); window.extractTranscriptNow();");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MainWindow] Error triggering extraction: {ex.Message}");
            _notificationService.ShowError($"Error triggering extraction: {ex.Message}", "Error");
        }
    }

    private void RecentFile_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        try
        {
            if (sender is FrameworkElement element && element.DataContext is Models.ExtractedFile file)
            {
                if (System.IO.File.Exists(file.FilePath))
                {
                    System.Diagnostics.Process.Start("notepad.exe", file.FilePath);
                }
                else
                {
                    _notificationService.ShowWarning($"File not found: {file.FileName}", "File Missing");
                }
            }
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Cannot open file: {ex.Message}", "Error");
        }
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
        var settingsWindow = new SettingsWindow(_settingsService)
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
    
    console.log('[UdemyExtractor] Initializing transcript extractor...');
    
    // Check if WebView2 API is available
    const hasWebView = !!(window.chrome && window.chrome.webview && window.chrome.webview.postMessage);
    console.log('[UdemyExtractor] WebView2 API available:', hasWebView);
    
    if (!hasWebView) {
        console.error('[UdemyExtractor] ERROR: window.chrome.webview is not available! PostMessage will not work.');
        console.log('[UdemyExtractor] window.chrome:', typeof window.chrome);
        console.log('[UdemyExtractor] window.chrome.webview:', window.chrome ? typeof window.chrome.webview : 'N/A');
    }
    
    // Helper function to send messages to C# host
    function sendMessageToHost(message) {
        try {
            if (window.chrome && window.chrome.webview && window.chrome.webview.postMessage) {
                // Send as JSON STRING for better compatibility
                const jsonString = JSON.stringify(message);
                console.log('[UdemyExtractor] Sending message to host:', jsonString);
                window.chrome.webview.postMessage(jsonString);
                console.log('[UdemyExtractor] Message sent successfully as string');
                return true;
            } else {
                console.error('[UdemyExtractor] Cannot send message - WebView2 API not available');
                return false;
            }
        } catch (error) {
            console.error('[UdemyExtractor] Error sending message:', error);
            return false;
        }
    }
    
    // Check if we're on a Udemy domain (supports business subdomains)
    const isUdemyDomain = window.location.hostname.endsWith('udemy.com');
    if (!isUdemyDomain) {
        console.log('[UdemyExtractor] Not a Udemy domain, exiting.');
        return;
    }
    
    // All possible transcript selectors for Udemy Business and regular Udemy
    const transcriptPanelSelectors = [
        '[data-purpose=\'transcript-panel\']',
        '[data-purpose=\'transcript-cue-container\']',
        '[data-purpose=\'cue-text\']',
        '[class*=\'transcript--panel\']',
        '[class*=\'transcript--transcript-panel\']',
        '[class*=\'transcript--cue-container\']',
        '[class*=\'transcript-panel\']',
        '.ud-component--course-taking--transcript-panel',
        '[data-testid=\'transcript-panel\']',
        '#transcript-panel',
        '[role=\'tabpanel\'][id*=\'transcript\']',
        '.transcript-panel'
    ];
    
    const cueTextSelectors = [
        '[data-purpose=\'cue-text\']',
        '[data-purpose=\'transcript-cue\'] span',
        '[class*=\'transcript--cue-text\']',
        '[class*=\'cue-text\']',
        '[class*=\'transcript--cue-container\'] span',
        '[class*=\'transcript-cue\'] span',
        '.transcript-cue span',
        '[data-purpose=\'transcript-cue-container\'] span'
    ];
    
    // Function to find transcript button (toggle)
    function findTranscriptButton() {
        console.log('[UdemyExtractor] Looking for transcript button...');
        
        // Strategy 1: Find SVG with aria-label containing transcript/Phiên âm
        const svgByLabel = document.querySelector('svg[aria-label*=""Phiên âm""], svg[aria-label*=""transcript""], svg[aria-label*=""Transcript""]');
        if (svgByLabel) {
            const btn = svgByLabel.closest('button');
            if (btn) {
                console.log('[UdemyExtractor] Found button via aria-label');
                return btn;
            }
        }

        // Strategy 2: SVG Icon with xlink:href
        const uses = document.querySelectorAll('use');
        for (let i = 0; i < uses.length; i++) {
            const href = uses[i].getAttribute('xlink:href') || uses[i].getAttribute('href');
            if (href && href.includes('#icon-transcript')) {
                const btn = uses[i].closest('button');
                if (btn) {
                    console.log('[UdemyExtractor] Found button via xlink:href');
                    return btn;
                }
            }
        }
        
        // Strategy 3: Data attribute
        const btnByData = document.querySelector('button[data-purpose=""transcript-toggle""]');
        if (btnByData) {
            console.log('[UdemyExtractor] Found button via data-purpose');
            return btnByData;
        }
        
        console.warn('[UdemyExtractor] Transcript button NOT found with any strategy');
        return null;
    }
    
    // Function to find transcript elements
    function findTranscriptElements() {
        let bestElements = [];
        
        for (const selector of cueTextSelectors) {
            try {
                const elements = document.querySelectorAll(selector);
                if (elements.length > bestElements.length) {
                     bestElements = Array.from(elements);
                     console.log('[UdemyExtractor] Improved match: Found', elements.length, 'elements with selector:', selector);
                }
            } catch (e) {
                console.error('[UdemyExtractor] Error with selector:', selector, e);
            }
        }
        
        if (bestElements.length > 0) {
            return bestElements;
        }
        return [];
    }
    
    // Function to check if transcript panel is visible
    function isTranscriptPanelVisible() {
        // Check for the specific panel element
        const panel = document.querySelector('[data-purpose=""transcript-panel""]');
        if (panel) {
            console.log('[UdemyExtractor] Transcript panel IS visible');
            return true;
        }
        // Fallback: check for sidebar with transcript class
        const sidebar = document.querySelector('[class*=""sidebar--transcript""]');
        if (sidebar) {
            console.log('[UdemyExtractor] Transcript sidebar IS visible');
            return true;
        }
        console.log('[UdemyExtractor] Transcript panel NOT visible');
        return false;
    }
    
    // Function to extract transcript
    async function extractTranscript() {
        console.log('[UdemyExtractor] === STARTING EXTRACTION (Auto-Open Mode) ===');
        
        try {
            // Step 1: Check if panel is open
            let panelVisible = isTranscriptPanelVisible();
            let elements = [];
            
            // Step 2: If panel not visible, try to open it
            if (!panelVisible) {
                console.log('[UdemyExtractor] Panel not visible. Attempting to open...');
                const btn = findTranscriptButton();
                
                if (btn) {
                    console.log('[UdemyExtractor] Clicking transcript button...');
                    btn.click();
                    
                    // Wait for panel to appear (max 8 seconds)
                    console.log('[UdemyExtractor] Waiting for panel to open...');
                    const maxWait = 16; // 8 seconds
                    for (let i = 0; i < maxWait; i++) {
                        await new Promise(r => setTimeout(r, 500));
                        if (isTranscriptPanelVisible()) {
                            console.log('[UdemyExtractor] Panel opened after', (i+1)*500, 'ms');
                            panelVisible = true;
                            break;
                        }
                    }
                    
                    if (!panelVisible) {
                        console.error('[UdemyExtractor] Panel did not open after clicking button');
                        sendMessageToHost({ type: 'transcriptNotFound', error: 'Panel failed to open' });
                        return;
                    }
                    
                    // Extra wait for content to load
                    await new Promise(r => setTimeout(r, 1000));
                } else {
                    console.error('[UdemyExtractor] Cannot find transcript button');
                    sendMessageToHost({ type: 'transcriptNotFound', error: 'Button not found' });
                    return;
                }
            }
            
            // Step 3: Now extract elements
            console.log('[UdemyExtractor] Panel is open. Extracting content...');
            elements = findTranscriptElements();
            
            // If still no elements, wait a bit more and retry
            if (elements.length === 0) {
                console.log('[UdemyExtractor] No elements yet, waiting more...');
                for (let i = 0; i < 6; i++) {
                    await new Promise(r => setTimeout(r, 500));
                    elements = findTranscriptElements();
                    if (elements.length > 0) break;
                }
            }
            
            console.log('[UdemyExtractor] Found', elements.length, 'transcript elements');
            console.log('[UdemyExtractor] Found', elements.length, 'transcript elements');
            
            if (elements.length === 0) {
                console.warn('[UdemyExtractor] No transcript elements found during extraction');
                sendMessageToHost({ type: 'transcriptNotFound' });
                return;
            }
            
            // Extract text from elements
            let transcript = '';
            const seenTexts = new Set();
            
            elements.forEach((element) => {
                const text = element.textContent.trim();
                // Avoid empty lines and potential duplicates
                if (text && text.length > 0 && !seenTexts.has(text)) {
                    seenTexts.add(text);
                    transcript += text + '\n';
                }
            });
            
            console.log('[UdemyExtractor] Extracted text length:', transcript.length);
            
            if (transcript.trim().length === 0) {
                console.warn('[UdemyExtractor] Transcript content is empty');
                sendMessageToHost({ type: 'transcriptNotFound' });
                return;
            }
        
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
            if ((urlParts[i] === 'lecture' || urlParts[i] === 'learn') && urlParts[i + 1]) {
                lectureId = urlParts[i + 1];
            }
        }
        
        console.log('[UdemyExtractor] Transcript extracted successfully, length:', transcript.length);
        
        sendMessageToHost({
            type: 'transcriptDetected',
            content: transcript.trim(),
            courseTitle: pageTitle,
            courseSlug: courseSlug,
            lectureId: lectureId,
            url: url,
            domain: window.location.hostname
        });
        
    } catch (error) {
        console.error('[UdemyExtractor] Critical error in extractTranscript:', error);
        sendMessageToHost({ type: 'transcriptNotFound', error: error.toString() });
    }
    }
    
    // Function to notify that transcript is available
    function notifyTranscriptAvailable() {
        console.log('[UdemyExtractor] Transcript panel is available!');
        sendMessageToHost({
            type: 'transcriptAvailable'
        });
    }
    

    
    // Check for transcript panel periodically
    let checkCount = 0;
    const maxChecks = 60; // Check for 30 seconds max
    
    function checkForTranscript() {
        checkCount++;
        
        // Check for Panel OR Button
        if (isTranscriptPanelVisible() || findTranscriptElements().length > 0 || findTranscriptButton()) {
            notifyTranscriptAvailable();
            return;
        }
        
        if (checkCount < maxChecks) {
            setTimeout(checkForTranscript, 500);
        }
    }
    
    // Monitor DOM for transcript appearance
    const observer = new MutationObserver((mutations) => {
        if (isTranscriptPanelVisible() || findTranscriptElements().length > 0 || findTranscriptButton()) {
            notifyTranscriptAvailable();
            // Don't disconnect - user might navigate to another lecture
        }
    });
    
    // Start observing
    if (document.body) {
        observer.observe(document.body, {
            childList: true,
            subtree: true
        });
    }
    
    // Initial check
    setTimeout(checkForTranscript, 1000);
    
    // Also check when URL changes (for SPA navigation)
    let lastUrl = location.href;
    const urlObserver = new MutationObserver(() => {
        if (location.href !== lastUrl) {
            lastUrl = location.href;
            console.log('[UdemyExtractor] URL changed, rechecking for transcript...');
            checkCount = 0;
            setTimeout(checkForTranscript, 1500);
        }
    });
    
    urlObserver.observe(document, { subtree: true, childList: true });
    
    // Make extract function available globally
    window.extractTranscriptNow = extractTranscript;
    
    console.log('[UdemyExtractor] Initialization complete.');
})();
";
}
