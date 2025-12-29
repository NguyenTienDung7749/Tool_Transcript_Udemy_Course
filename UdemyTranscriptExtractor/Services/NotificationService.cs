using System.Windows;
using System.Windows.Media;

namespace UdemyTranscriptExtractor.Services;

public class NotificationService
{
    public event EventHandler<NotificationEventArgs>? NotificationRequested;
    
    public void ShowSuccess(string message, string title = "Success")
    {
        NotificationRequested?.Invoke(this, new NotificationEventArgs
        {
            Message = message,
            Title = title,
            Type = NotificationType.Success
        });
    }
    
    public void ShowError(string message, string title = "Error")
    {
        NotificationRequested?.Invoke(this, new NotificationEventArgs
        {
            Message = message,
            Title = title,
            Type = NotificationType.Error
        });
    }
    
    public void ShowInfo(string message, string title = "Info")
    {
        NotificationRequested?.Invoke(this, new NotificationEventArgs
        {
            Message = message,
            Title = title,
            Type = NotificationType.Info
        });
    }
    
    public void ShowWarning(string message, string title = "Warning")
    {
        NotificationRequested?.Invoke(this, new NotificationEventArgs
        {
            Message = message,
            Title = title,
            Type = NotificationType.Warning
        });
    }
}

public enum NotificationType
{
    Success,
    Error,
    Info,
    Warning
}

public class NotificationEventArgs : EventArgs
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
}
