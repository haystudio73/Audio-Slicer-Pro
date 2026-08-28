namespace AudioSlicerPro.Models;

public enum SegmentStatus
{
    Pending,
    Slicing,
    Converting,
    Completed,
    Failed
}

/// <summary>
/// Model representing a single segment during processing.
/// </summary>
public class ProcessingSegment
{
    public int Index { get; set; }
    public string SegmentFileName { get; set; } = string.Empty;
    public string TempAudioPath { get; set; } = string.Empty;
    public string OutputMp4Path { get; set; } = string.Empty;
    public SegmentStatus Status { get; set; } = SegmentStatus.Pending;
    public double ProgressPercentage { get; set; }
    public string StatusMessage { get; set; } = string.Empty;
}
