using System.Text.Json.Serialization;

namespace UdemyTranscriptExtractor.Models;

public class WebMessage
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
    
    [JsonPropertyName("courseTitle")]
    public string CourseTitle { get; set; } = string.Empty;
    
    [JsonPropertyName("courseSlug")]
    public string CourseSlug { get; set; } = string.Empty;
    
    [JsonPropertyName("lectureId")]
    public string LectureId { get; set; } = string.Empty;
    
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
    
    [JsonPropertyName("domain")]
    public string Domain { get; set; } = string.Empty;
}
