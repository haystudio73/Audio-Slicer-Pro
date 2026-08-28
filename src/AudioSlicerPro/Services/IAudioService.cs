using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AudioSlicerPro.Models;

namespace AudioSlicerPro.Services;

public interface IAudioService
{
    Task<AudioFileInfo> GetAudioFileInfoAsync(string inputPath);

    Task<IReadOnlyList<string>> SliceAudioAsync(
        string inputPath,
        string tmpDir,
        int segmentSeconds,
        bool trimSilence,
        int silenceThresholdDb,
        IProgress<double> progress,
        CancellationToken ct);

    Task<string> ConvertToMp4Async(
        string segmentPath,
        string destFolder,
        string audioSubFolder,
        string aspectRatio,
        IProgress<double>? progress,
        CancellationToken ct);

    Task<string> SaveMp3SegmentAsync(
        string segmentPath,
        string mp3DestFolder,
        string audioSubFolder,
        CancellationToken ct);

    Task CleanTmpDirectoryAsync(string tmpDir);
}
