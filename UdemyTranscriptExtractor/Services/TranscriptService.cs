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
        // This is a placeholder implementation
        // In a real scenario, you would parse the HTML and extract transcript content
        // For demonstration, we'll return a sample transcript
        
        if (string.IsNullOrEmpty(html))
            return string.Empty;
            
        // Look for transcript markers in HTML
        // This is simplified - actual implementation would be more sophisticated
        var sb = new StringBuilder();
        sb.AppendLine("=== Transcript Extracted ===");
        sb.AppendLine();
        sb.AppendLine("This is a sample transcript extracted from the Udemy lecture.");
        sb.AppendLine("In a production environment, this would contain the actual transcript content.");
        sb.AppendLine();
        sb.AppendLine("Extracted at: " + DateTime.Now.ToString("F"));
        
        return sb.ToString();
    }
    
    private string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        return sanitized.Trim();
    }
}
