using System.IO;
using System.Text;
using UdemyTranscriptExtractor.Models;

namespace UdemyTranscriptExtractor.Services;

public class TranscriptService
{
    private readonly SettingsService _settingsService;
    
    public TranscriptService(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }
    
    public async Task<ExtractedFile?> ExtractTranscriptAsync(string htmlContent, string lectureName, string courseName)
    {
        try
        {
            // Simulate transcript extraction logic
            // In a real implementation, this would parse the HTML and extract transcript text
            var transcript = ExtractTranscriptFromHtml(htmlContent);
            
            if (string.IsNullOrWhiteSpace(transcript))
                return null;
            
            var settings = await _settingsService.LoadSettingsAsync();
            
            // Ensure output directory exists
            Directory.CreateDirectory(settings.OutputFolder);
            
            // Create file name
            var sanitizedCourseName = SanitizeFileName(courseName);
            var sanitizedLectureName = SanitizeFileName(lectureName);
            var fileName = $"{sanitizedCourseName}_{sanitizedLectureName}.txt";
            var filePath = Path.Combine(settings.OutputFolder, fileName);
            
            // Save transcript
            await File.WriteAllTextAsync(filePath, transcript, Encoding.UTF8);
            
            // Create extracted file info
            var fileInfo = new FileInfo(filePath);
            var extractedFile = new ExtractedFile
            {
                FileName = fileName,
                FilePath = filePath,
                ExtractedAt = DateTime.Now,
                FileSize = fileInfo.Length,
                CourseName = courseName,
                LectureName = lectureName
            };
            
            // Add to recent files
            await _settingsService.AddRecentFileAsync(filePath);
            
            return extractedFile;
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Error extracting transcript: {ex.Message}");
            return null;
        }
    }
    
    private string ExtractTranscriptFromHtml(string html)
    {
        // Note: The actual transcript extraction is handled by JavaScript in MainWindow.xaml.cs
        // This method receives the already-extracted transcript content from the WebView2 message
        // We simply pass through the content that was sent from the browser
        
        if (string.IsNullOrEmpty(html))
            return string.Empty;
            
        // The html parameter is actually the extracted transcript text from JavaScript
        // No parsing needed - just return it as-is
        return html;
    }
    
    private string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        return sanitized.Trim();
    }
}
