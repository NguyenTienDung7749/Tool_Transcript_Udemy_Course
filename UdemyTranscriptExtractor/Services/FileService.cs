using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UdemyTranscriptExtractor.Models;

namespace UdemyTranscriptExtractor.Services;

public class FileService
{
    private readonly SettingsService _settingsService;
    private readonly Dictionary<string, int> _courseCounters = new();
    private readonly List<ExtractedFile> _extractedFiles = new();
    
    public FileService(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }
    
    public async Task<string> SelectOutputFolderAsync()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = "Select Output Folder",
            FileName = "Select Folder",
            Filter = "Folder|*.folder"
        };
        
        if (dialog.ShowDialog() == true)
        {
            var folderPath = Path.GetDirectoryName(dialog.FileName);
            if (!string.IsNullOrEmpty(folderPath))
            {
                var settings = await _settingsService.LoadSettingsAsync();
                settings.OutputFolder = folderPath;
                await _settingsService.SaveSettingsAsync(settings);
                return folderPath;
            }
        }
        
        return string.Empty;
    }
    
    public async Task<ExtractedFile?> SaveTranscriptAsync(string content, string courseName, string lectureId)
    {
        try
        {
            // 1. Ensure output folder is set
            var settings = await _settingsService.LoadSettingsAsync();
            var outputFolder = settings.OutputFolder;
            
            if (string.IsNullOrEmpty(outputFolder))
            {
                outputFolder = await SelectOutputFolderAsync();
                if (string.IsNullOrEmpty(outputFolder))
                {
                    throw new Exception("Output folder not set");
                }
            }
            
            // Ensure directory exists
            Directory.CreateDirectory(outputFolder);
            
            // 2. Clean course name for filename
            string cleanCourseName = CleanFileName(courseName);
            
            // 3. Get next lecture number for this course
            if (!_courseCounters.ContainsKey(cleanCourseName))
            {
                _courseCounters[cleanCourseName] = GetExistingLectureCount(outputFolder, cleanCourseName);
            }
            _courseCounters[cleanCourseName]++;
            int lectureNumber = _courseCounters[cleanCourseName];
            
            // 4. Generate filename: CourseName_Lecture-01.txt
            string filename = $"{cleanCourseName}_Lecture-{lectureNumber:D2}.txt";
            string fullPath = Path.Combine(outputFolder, filename);
            
            // 5. Handle duplicates
            int suffix = 1;
            while (File.Exists(fullPath))
            {
                filename = $"{cleanCourseName}_Lecture-{lectureNumber:D2}_{suffix}.txt";
                fullPath = Path.Combine(outputFolder, filename);
                suffix++;
            }
            
            // 6. Save file with UTF-8 encoding
            await File.WriteAllTextAsync(fullPath, content, Encoding.UTF8);
            
            // 7. Track file
            var fileInfo = new FileInfo(fullPath);
            var file = new ExtractedFile
            {
                FileName = filename,
                FilePath = fullPath,
                CourseName = courseName,
                LectureName = $"Lecture {lectureNumber}",
                ExtractedAt = DateTime.Now,
                FileSize = fileInfo.Length
            };
            _extractedFiles.Insert(0, file);
            
            // 8. Update settings
            await _settingsService.AddRecentFileAsync(fullPath);
            
            return file;
        }
        catch (IOException ex)
        {
            throw new Exception($"Cannot save file: {ex.Message}", ex);
        }
        catch (UnauthorizedAccessException)
        {
            throw new Exception("Permission denied. Choose another folder.");
        }
    }
    
    private string CleanFileName(string name)
    {
        // Remove invalid file name characters
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c, '-');
        }
        
        // Replace multiple spaces with single dash
        name = Regex.Replace(name, @"\s+", "-");
        
        // Limit length
        if (name.Length > 50)
        {
            name = name.Substring(0, 50);
        }
        
        return name.Trim('-');
    }
    
    private int GetExistingLectureCount(string outputFolder, string courseName)
    {
        if (!Directory.Exists(outputFolder))
            return 0;
        
        var pattern = $"{courseName}_Lecture-*.txt";
        var files = Directory.GetFiles(outputFolder, pattern);
        
        int maxNumber = 0;
        foreach (var file in files)
        {
            var match = Regex.Match(Path.GetFileName(file), @"Lecture-(\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int num))
            {
                maxNumber = Math.Max(maxNumber, num);
            }
        }
        return maxNumber;
    }
    
    public List<ExtractedFile> GetRecentFiles(int count = 10)
    {
        return _extractedFiles.Take(count).ToList();
    }
}
