using System;

namespace AudioSlicerPro.Models;

/// <summary>
/// Holds metadata for an imported audio file.
/// </summary>
public class AudioFileInfo
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public long FileSizeBytes { get; set; }

    public string FormattedSize => $"{FileSizeBytes / (1024.0 * 1024.0):F2} MB";
    public string FormattedDuration => Duration.ToString(@"hh\:mm\:ss");
    public string DurationDisplay => $"Duration: {FormattedDuration}";
    public string SizeDisplay => $"Size: {FormattedSize}";
}
