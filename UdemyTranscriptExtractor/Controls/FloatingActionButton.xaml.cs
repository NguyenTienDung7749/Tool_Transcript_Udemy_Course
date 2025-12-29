using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace UdemyTranscriptExtractor.Controls;

public partial class FloatingActionButton : UserControl
{
    public static readonly DependencyProperty IsExtractingProperty =
        DependencyProperty.Register(nameof(IsExtracting), typeof(bool), typeof(FloatingActionButton),
            new PropertyMetadata(false, OnIsExtractingChanged));
    
    public bool IsExtracting
    {
        get => (bool)GetValue(IsExtractingProperty);
        set => SetValue(IsExtractingProperty, value);
    }
    
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(FloatingActionButton));
    
    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }
    
    private Storyboard? _pulseStoryboard;
    
    public FloatingActionButton()
    {
        InitializeComponent();
        
        ButtonBackground.MouseEnter += OnMouseEnter;
        ButtonBackground.MouseLeave += OnMouseLeave;
        ButtonBackground.MouseLeftButtonDown += OnMouseLeftButtonDown;
        
        StartPulse();
    }
    
    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
        var storyboard = new Storyboard();
        
        var scaleAnimation = new DoubleAnimation
        {
            To = 1.1,
            Duration = TimeSpan.FromMilliseconds(150),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        
        var rotateAnimation = new DoubleAnimation
        {
            To = 15,
            Duration = TimeSpan.FromMilliseconds(150)
        };
        
        var shadowAnimation = new DoubleAnimation
        {
            To = 24,
            Duration = TimeSpan.FromMilliseconds(150)
        };
        
        Storyboard.SetTarget(scaleAnimation, ScaleTransform);
        Storyboard.SetTargetProperty(scaleAnimation, new PropertyPath("ScaleX"));
        
        var scaleYAnimation = (DoubleAnimation)scaleAnimation.Clone();
        Storyboard.SetTarget(scaleYAnimation, ScaleTransform);
        Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("ScaleY"));
        
        Storyboard.SetTarget(rotateAnimation, IconRotate);
        Storyboard.SetTargetProperty(rotateAnimation, new PropertyPath("Angle"));
        
        Storyboard.SetTarget(shadowAnimation, Shadow);
        Storyboard.SetTargetProperty(shadowAnimation, new PropertyPath("BlurRadius"));
        
        storyboard.Children.Add(scaleAnimation);
        storyboard.Children.Add(scaleYAnimation);
        storyboard.Children.Add(rotateAnimation);
        storyboard.Children.Add(shadowAnimation);
        
        storyboard.Begin();
    }
    
    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        var storyboard = new Storyboard();
        
        var scaleAnimation = new DoubleAnimation
        {
            To = 1,
            Duration = TimeSpan.FromMilliseconds(150)
        };
        
        var rotateAnimation = new DoubleAnimation
        {
            To = 0,
            Duration = TimeSpan.FromMilliseconds(150)
        };
        
        var shadowAnimation = new DoubleAnimation
        {
            To = 16,
            Duration = TimeSpan.FromMilliseconds(150)
        };
        
        Storyboard.SetTarget(scaleAnimation, ScaleTransform);
        Storyboard.SetTargetProperty(scaleAnimation, new PropertyPath("ScaleX"));
        
        var scaleYAnimation = (DoubleAnimation)scaleAnimation.Clone();
        Storyboard.SetTarget(scaleYAnimation, ScaleTransform);
        Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("ScaleY"));
        
        Storyboard.SetTarget(rotateAnimation, IconRotate);
        Storyboard.SetTargetProperty(rotateAnimation, new PropertyPath("Angle"));
        
        Storyboard.SetTarget(shadowAnimation, Shadow);
        Storyboard.SetTargetProperty(shadowAnimation, new PropertyPath("BlurRadius"));
        
        storyboard.Children.Add(scaleAnimation);
        storyboard.Children.Add(scaleYAnimation);
        storyboard.Children.Add(rotateAnimation);
        storyboard.Children.Add(shadowAnimation);
        
        storyboard.Begin();
    }
    
    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        Command?.Execute(null);
    }
    
    private static void OnIsExtractingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FloatingActionButton fab)
        {
            if ((bool)e.NewValue)
            {
                fab.ShowCheckmark();
            }
            else
            {
                fab.ShowExtractIcon();
            }
        }
    }
    
    private void ShowCheckmark()
    {
        ExtractIcon.Visibility = Visibility.Collapsed;
        CheckmarkIcon.Visibility = Visibility.Visible;
        
        var animation = new DoubleAnimationUsingKeyFrames();
        animation.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        animation.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.4)))
        {
            EasingFunction = new ElasticEase { EasingMode = EasingMode.EaseOut, Oscillations = 1, Springiness = 5 }
        });
        
        CheckmarkScale.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleXProperty, animation);
        CheckmarkScale.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleYProperty, animation);
        
        _pulseStoryboard?.Stop();
    }
    
    private void ShowExtractIcon()
    {
        CheckmarkIcon.Visibility = Visibility.Collapsed;
        ExtractIcon.Visibility = Visibility.Visible;
        
        StartPulse();
    }
    
    private void StartPulse()
    {
        _pulseStoryboard = new Storyboard
        {
            RepeatBehavior = RepeatBehavior.Forever
        };
        
        var scaleXAnimation = new DoubleAnimation
        {
            From = 1,
            To = 1.05,
            Duration = TimeSpan.FromSeconds(2),
            AutoReverse = true,
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };
        
        var scaleYAnimation = (DoubleAnimation)scaleXAnimation.Clone();
        
        Storyboard.SetTarget(scaleXAnimation, ScaleTransform);
        Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath("ScaleX"));
        
        Storyboard.SetTarget(scaleYAnimation, ScaleTransform);
        Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("ScaleY"));
        
        _pulseStoryboard.Children.Add(scaleXAnimation);
        _pulseStoryboard.Children.Add(scaleYAnimation);
        
        _pulseStoryboard.Begin();
    }
}
