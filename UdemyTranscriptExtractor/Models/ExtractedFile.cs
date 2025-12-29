namespace UdemyTranscriptExtractor.Models;

public class ExtractedFile
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime ExtractedAt { get; set; }
    public long FileSize { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string LectureName { get; set; } = string.Empty;
    
    public string FormattedSize
    {
        get
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = FileSize;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
    
    public string FormattedDate => ExtractedAt.ToString("MMM dd, HH:mm");
}
