using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace UdemyTranscriptExtractor.Helpers;

public class ConfettiEffect
{
    private readonly Canvas _container;
    private readonly Random _random = new();
    
    public ConfettiEffect(Canvas container)
    {
        _container = container;
    }
    
    public void Burst(int particleCount = 50)
    {
        for (int i = 0; i < particleCount; i++)
        {
            CreateParticle();
        }
    }
    
    private void CreateParticle()
    {
        var colors = new[] 
        { 
            Colors.Gold, Colors.DeepPink, Colors.Cyan, 
            Colors.LimeGreen, Colors.Orange, Colors.Purple 
        };
        
        var particle = new Ellipse
        {
            Width = _random.Next(6, 12),
            Height = _random.Next(6, 12),
            Fill = new SolidColorBrush(colors[_random.Next(colors.Length)])
        };
        
        var startX = _container.ActualWidth / 2;
        var startY = _container.ActualHeight / 2;
        
        Canvas.SetLeft(particle, startX);
        Canvas.SetTop(particle, startY);
        
        _container.Children.Add(particle);
        
        // Random velocity
        var velocityX = _random.Next(-200, 200);
        var velocityY = _random.Next(-300, -100);
        
        var storyboard = new Storyboard();
        
        // X movement
        var xAnimation = new DoubleAnimation
        {
            From = startX,
            To = startX + velocityX,
            Duration = TimeSpan.FromSeconds(2),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        
        Storyboard.SetTarget(xAnimation, particle);
        Storyboard.SetTargetProperty(xAnimation, new PropertyPath(Canvas.LeftProperty));
        
        // Y movement with gravity
        var yAnimation = new DoubleAnimation
        {
            From = startY,
            To = startY + velocityY + 500, // Gravity effect
            Duration = TimeSpan.FromSeconds(2),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };
        
        Storyboard.SetTarget(yAnimation, particle);
        Storyboard.SetTargetProperty(yAnimation, new PropertyPath(Canvas.TopProperty));
        
        // Fade out
        var fadeAnimation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            BeginTime = TimeSpan.FromSeconds(1),
            Duration = TimeSpan.FromSeconds(1)
        };
        
        Storyboard.SetTarget(fadeAnimation, particle);
        Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(UIElement.OpacityProperty));
        
        // Rotation
        var rotateTransform = new RotateTransform();
        particle.RenderTransform = rotateTransform;
        particle.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
        
        var rotateAnimation = new DoubleAnimation
        {
            From = 0,
            To = _random.Next(-720, 720),
            Duration = TimeSpan.FromSeconds(2)
        };
        
        Storyboard.SetTarget(rotateAnimation, particle);
        Storyboard.SetTargetProperty(rotateAnimation, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));
        
        storyboard.Children.Add(xAnimation);
        storyboard.Children.Add(yAnimation);
        storyboard.Children.Add(fadeAnimation);
        storyboard.Children.Add(rotateAnimation);
        
        storyboard.Completed += (s, e) => _container.Children.Remove(particle);
        
        storyboard.Begin();
    }
}
