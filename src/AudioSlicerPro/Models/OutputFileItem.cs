using CommunityToolkit.Mvvm.ComponentModel;

namespace AudioSlicerPro.Models;

/// <summary>
/// Represents a completed output file for previewing and playback.
/// </summary>
public partial class OutputFileItem : ObservableObject
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FormattedSize { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsPlaying { get; set; }
}
