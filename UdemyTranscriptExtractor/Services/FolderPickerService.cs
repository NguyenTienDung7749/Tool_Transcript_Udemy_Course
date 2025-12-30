using System.Windows.Forms;

namespace UdemyTranscriptExtractor.Services;

public class FolderPickerService
{
    public string? PickFolder(string description = "Select Output Folder")
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = description,
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true
        };

        var result = dialog.ShowDialog();
        return result == DialogResult.OK ? dialog.SelectedPath : null;
    }
}
