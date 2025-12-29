using System.IO;

namespace UdemyTranscriptExtractor.Models;

public class AppSettings
{
    public string OutputFolder { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "UdemyTranscripts");
    public bool AutoSave { get; set; } = true;
    public bool ShowNotifications { get; set; } = true;
    public string Theme { get; set; } = "Dark";
    public List<string> RecentFiles { get; set; } = new();
    public int MaxRecentFiles { get; set; } = 10;
    public int TotalExtractedCount { get; set; } = 0;
    public List<string> RecentCourses { get; set; } = new();
    public string UdemyBaseUrl { get; set; } = "https://fpl.udemy.com";
}
