namespace AudioSlicerPro.Models;

/// <summary>
/// Represents user settings stored in %LOCALAPPDATA%\AudioSlicerPro\settings.json
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Path to temporary working folder for sliced audio files
    /// </summary>
    public string TmpFolderPath { get; set; } = string.Empty;

    /// <summary>
    /// Path to destination output folder for MP4 files
    /// </summary>
    public string DestFolderPath { get; set; } = string.Empty;

    /// <summary>
    /// Path to destination output folder for MP3 files
    /// </summary>
    public string Mp3DestFolderPath { get; set; } = string.Empty;

    /// <summary>
    /// Current selected UI language ("vi-VN" or "en-US")
    /// </summary>
    public string Language { get; set; } = "vi-VN";

    /// <summary>
    /// Segment duration in seconds (default: 15 seconds)
    /// </summary>
    public int SegmentDurationSeconds { get; set; } = 15;

    /// <summary>
    /// Whether to trim silence from beginning and end of audio before slicing
    /// </summary>
    public bool TrimSilence { get; set; } = true;

    /// <summary>
    /// Silence detection threshold in dB (default: -50)
    /// </summary>
    public int SilenceThresholdDb { get; set; } = -50;

    /// <summary>
    /// MP4 video output aspect ratio ("16:9" or "9:16"). Default: "16:9"
    /// </summary>
    public string AspectRatio { get; set; } = "16:9";
}
