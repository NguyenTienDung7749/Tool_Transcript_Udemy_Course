using System.Windows;
using System.Windows.Media.Animation;

namespace UdemyTranscriptExtractor.Helpers;

public static class AnimationHelper
{
    public static void FadeIn(UIElement element, double duration = 0.3)
    {
        var animation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromSeconds(duration),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        
        element.BeginAnimation(UIElement.OpacityProperty, animation);
    }
    
    public static void FadeOut(UIElement element, double duration = 0.2, EventHandler? completed = null)
    {
        var animation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromSeconds(duration),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };
        
        if (completed != null)
            animation.Completed += completed;
        
        element.BeginAnimation(UIElement.OpacityProperty, animation);
    }
    
    public static void SlideIn(UIElement element, double from = -300, double to = 0, double duration = 0.25)
    {
        var storyboard = new Storyboard();
        
        var slideAnimation = new DoubleAnimation
        {
            From = from,
            To = to,
            Duration = TimeSpan.FromSeconds(duration),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        
        var fadeAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromSeconds(duration)
        };
        
        Storyboard.SetTarget(slideAnimation, element);
        Storyboard.SetTargetProperty(slideAnimation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
        
        Storyboard.SetTarget(fadeAnimation, element);
        Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(UIElement.OpacityProperty));
        
        storyboard.Children.Add(slideAnimation);
        storyboard.Children.Add(fadeAnimation);
        
        storyboard.Begin();
    }
    
    public static void Pulse(UIElement element, bool start = true)
    {
        if (start)
        {
            var storyboard = new Storyboard
            {
                RepeatBehavior = RepeatBehavior.Forever
            };
            
            var scaleXAnimation = new DoubleAnimation
            {
                From = 1,
                To = 1.05,
                Duration = TimeSpan.FromSeconds(1),
                AutoReverse = true,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };
            
            var scaleYAnimation = new DoubleAnimation
            {
                From = 1,
                To = 1.05,
                Duration = TimeSpan.FromSeconds(1),
                AutoReverse = true,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };
            
            Storyboard.SetTarget(scaleXAnimation, element);
            Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
            
            Storyboard.SetTarget(scaleYAnimation, element);
            Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));
            
            storyboard.Children.Add(scaleXAnimation);
            storyboard.Children.Add(scaleYAnimation);
            
            storyboard.Begin();
        }
        else
        {
            element.BeginAnimation(UIElement.RenderTransformProperty, null);
        }
    }
    
    public static void SuccessCheckmark(UIElement element)
    {
        var storyboard = new Storyboard();
        
        var scaleXAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromSeconds(0.4),
            EasingFunction = new ElasticEase { EasingMode = EasingMode.EaseOut, Oscillations = 1, Springiness = 5 }
        };
        
        var scaleYAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromSeconds(0.4),
            EasingFunction = new ElasticEase { EasingMode = EasingMode.EaseOut, Oscillations = 1, Springiness = 5 }
        };
        
        Storyboard.SetTarget(scaleXAnimation, element);
        Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
        
        Storyboard.SetTarget(scaleYAnimation, element);
        Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));
        
        storyboard.Children.Add(scaleXAnimation);
        storyboard.Children.Add(scaleYAnimation);
        
        storyboard.Begin();
    }
}
