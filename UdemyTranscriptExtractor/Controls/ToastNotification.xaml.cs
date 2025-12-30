using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using UdemyTranscriptExtractor.Services;

namespace UdemyTranscriptExtractor.Controls;

public partial class ToastNotification : System.Windows.Controls.UserControl
{
    public ToastNotification()
    {
        InitializeComponent();
    }
    
    public void Show(string title, string message, NotificationType type)
    {
        TitleText.Text = title;
        MessageText.Text = message;
        
        SetIconAndColor(type);
        
        var storyboard = new Storyboard();
        
        // Slide in
        var slideAnimation = new DoubleAnimation
        {
            From = 400,
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
        
        Storyboard.SetTarget(slideAnimation, SlideTransform);
        Storyboard.SetTargetProperty(slideAnimation, new PropertyPath("X"));
        
        Storyboard.SetTarget(fadeAnimation, ToastBorder);
        Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath("Opacity"));
        
        storyboard.Children.Add(slideAnimation);
        storyboard.Children.Add(fadeAnimation);
        
        storyboard.Completed += (s, e) =>
        {
            // Auto-hide after 3 seconds
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            
            timer.Tick += (sender, args) =>
            {
                timer.Stop();
                Hide();
            };
            
            timer.Start();
        };
        
        storyboard.Begin();
    }
    
    public void Hide()
    {
        var storyboard = new Storyboard();
        
        var slideAnimation = new DoubleAnimation
        {
            From = 0,
            To = 400,
            Duration = TimeSpan.FromMilliseconds(200),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };
        
        var fadeAnimation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(200)
        };
        
        Storyboard.SetTarget(slideAnimation, SlideTransform);
        Storyboard.SetTargetProperty(slideAnimation, new PropertyPath("X"));
        
        Storyboard.SetTarget(fadeAnimation, ToastBorder);
        Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath("Opacity"));
        
        storyboard.Children.Add(slideAnimation);
        storyboard.Children.Add(fadeAnimation);
        
        storyboard.Completed += (s, e) =>
        {
            if (Parent is System.Windows.Controls.Panel panel)
                panel.Children.Remove(this);
        };
        
        storyboard.Begin();
    }
    
    private void SetIconAndColor(NotificationType type)
    {
        SuccessIcon.Visibility = Visibility.Collapsed;
        ErrorIcon.Visibility = Visibility.Collapsed;
        InfoIcon.Visibility = Visibility.Collapsed;
        WarningIcon.Visibility = Visibility.Collapsed;
        
        switch (type)
        {
            case NotificationType.Success:
                SuccessIcon.Visibility = Visibility.Visible;
                IconBackground.Color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#3FB950")!;
                break;
            case NotificationType.Error:
                ErrorIcon.Visibility = Visibility.Visible;
                IconBackground.Color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F85149")!;
                break;
            case NotificationType.Info:
                InfoIcon.Visibility = Visibility.Visible;
                IconBackground.Color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#58A6FF")!;
                break;
            case NotificationType.Warning:
                WarningIcon.Visibility = Visibility.Visible;
                IconBackground.Color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#D29922")!;
                break;
        }
    }
}
