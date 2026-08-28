using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AudioSlicerPro.Models;
using Microsoft.Extensions.Logging;
using Xabe.FFmpeg;

namespace AudioSlicerPro.Services;

public class AudioService : IAudioService
{
    private readonly ILogger<AudioService> _logger;
    private bool _isFfmpegInitialized;

    public AudioService(ILogger<AudioService> logger)
    {
        _logger = logger;
    }

    private Task EnsureFFmpegAsync()
    {
        if (_isFfmpegInitialized) return Task.CompletedTask;

        try
        {
            if (File.Exists(@"C:\ffmpeg\bin\ffmpeg.exe"))
            {
                FFmpeg.SetExecutablesPath(@"C:\ffmpeg\bin");
                _logger.LogInformation("Using FFmpeg binaries from C:\\ffmpeg\\bin");
            }
            else
            {
                _logger.LogInformation("Using system PATH FFmpeg binaries");
            }
            _isFfmpegInitialized = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize FFmpeg binaries");
            _isFfmpegInitialized = true;
        }

        return Task.CompletedTask;
    }

    public async Task<AudioFileInfo> GetAudioFileInfoAsync(string inputPath)
    {
        ValidatePath(inputPath);
        await EnsureFFmpegAsync();

        var fileInfo = new FileInfo(inputPath);
        var audioInfo = new AudioFileInfo
        {
            FilePath = inputPath,
            FileName = fileInfo.Name,
            Extension = fileInfo.Extension.ToLowerInvariant(),
            FileSizeBytes = fileInfo.Length
        };

        try
        {
            IMediaInfo mediaInfo = await FFmpeg.GetMediaInfo(inputPath);
            audioInfo.Duration = mediaInfo.Duration;
            _logger.LogInformation("Retrieved media info for {Path}: Duration={Duration}", inputPath, audioInfo.Duration);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not read media info via Xabe.FFmpeg for {Path}", inputPath);
            audioInfo.Duration = TimeSpan.Zero;
        }

        return audioInfo;
    }

    public async Task<IReadOnlyList<string>> SliceAudioAsync(
        string inputPath,
        string tmpDir,
        int segmentSeconds,
        bool trimSilence,
        int silenceThresholdDb,
        IProgress<double> progress,
        CancellationToken ct)
    {
        ValidatePath(inputPath);
        if (string.IsNullOrWhiteSpace(tmpDir)) throw new ArgumentException("Tmp directory cannot be empty", nameof(tmpDir));

        await EnsureFFmpegAsync();
        Directory.CreateDirectory(tmpDir);

        string ext = Path.GetExtension(inputPath).TrimStart('.').ToLowerInvariant();
        if (string.IsNullOrEmpty(ext)) ext = "mp3";

        string outputPattern = Path.Combine(tmpDir, $"segment_%03d.{ext}");

        // Build FFmpeg command args
        // If trim silence is requested, add silenceremove filter
        string filterArg = trimSilence
            ? $"-af \"silenceremove=start_periods=1:start_duration=0.5:start_threshold={silenceThresholdDb}dB:stop_periods=-1:stop_duration=0.5:stop_threshold={silenceThresholdDb}dB\" "
            : "";

        string codecArg = trimSilence ? "-c:a libmp3lame -q:a 2" : "-c copy";

        string arguments = $"-i \"{inputPath}\" {filterArg}-f segment -segment_time {segmentSeconds} {codecArg} \"{outputPattern}\"";

        _logger.LogInformation("Executing FFmpeg Slice: ffmpeg {Args}", arguments);

        await RunFFmpegCommandAsync(arguments, progress, ct);

        // Collect generated segment files sorted by name
        var files = Directory.GetFiles(tmpDir, $"segment_*.{ext}")
                             .OrderBy(f => f)
                             .ToList();

        _logger.LogInformation("Audio sliced into {Count} segments in {TmpDir}", files.Count, tmpDir);
        return files;
    }

    public async Task<string> ConvertToMp4Async(
        string segmentPath,
        string destFolder,
        string audioSubFolder,
        string aspectRatio,
        IProgress<double>? progress,
        CancellationToken ct)
    {
        ValidatePath(segmentPath);
        if (string.IsNullOrWhiteSpace(destFolder)) throw new ArgumentException("Destination folder cannot be empty", nameof(destFolder));

        await EnsureFFmpegAsync();

        string targetDir = Path.Combine(destFolder, audioSubFolder);
        Directory.CreateDirectory(targetDir);

        string segmentFileName = Path.GetFileNameWithoutExtension(segmentPath);
        string outputMp4Path = Path.Combine(targetDir, $"{segmentFileName}.mp4");

        string resolution = string.Equals(aspectRatio, "9:16", StringComparison.OrdinalIgnoreCase) ? "720x1280" : "1280x720";

        // Convert audio to standard MP4 video format compatible with Google Flow & web uploaders (H.264 video + AAC audio)
        string arguments = $"-f lavfi -i color=c=black:s={resolution}:r=30 -i \"{segmentPath}\" -c:v libx264 -preset ultrafast -tune stillimage -pix_fmt yuv420p -c:a aac -b:a 192k -shortest -movflags +faststart \"{outputMp4Path}\"";

        _logger.LogInformation("Executing FFmpeg Convert to MP4 ({AspectRatio} - {Resolution}): ffmpeg {Args}", aspectRatio, resolution, arguments);

        await RunFFmpegCommandAsync(arguments, progress, ct);

        _logger.LogInformation("Converted {Input} to {Output}", segmentPath, outputMp4Path);
        return outputMp4Path;
    }

    public async Task<string> SaveMp3SegmentAsync(
        string segmentPath,
        string mp3DestFolder,
        string audioSubFolder,
        CancellationToken ct)
    {
        ValidatePath(segmentPath);
        if (string.IsNullOrWhiteSpace(mp3DestFolder)) throw new ArgumentException("MP3 destination folder cannot be empty", nameof(mp3DestFolder));

        string targetDir = Path.Combine(mp3DestFolder, audioSubFolder);
        Directory.CreateDirectory(targetDir);

        string fileName = Path.GetFileName(segmentPath);
        string outputMp3Path = Path.Combine(targetDir, fileName);

        await Task.Run(() => File.Copy(segmentPath, outputMp3Path, overwrite: true), ct);

        _logger.LogInformation("Saved MP3 segment {Input} to {Output}", segmentPath, outputMp3Path);
        return outputMp3Path;
    }

    public Task CleanTmpDirectoryAsync(string tmpDir)
    {
        return Task.Run(() =>
        {
            try
            {
                if (Directory.Exists(tmpDir))
                {
                    Directory.Delete(tmpDir, recursive: true);
                    _logger.LogInformation("Cleaned temporary directory {TmpDir}", tmpDir);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error cleaning tmp directory {TmpDir}", tmpDir);
            }
        });
    }

    private async Task RunFFmpegCommandAsync(string arguments, IProgress<double>? progress, CancellationToken ct)
    {
        string ffmpegPath = "ffmpeg.exe";
        if (File.Exists(@"C:\ffmpeg\bin\ffmpeg.exe"))
        {
            ffmpegPath = @"C:\ffmpeg\bin\ffmpeg.exe";
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                _logger.LogDebug("[FFmpeg Stderr] {Log}", e.Data);
                ParseProgress(e.Data, progress);
            }
        };

        process.Start();
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();

        using var registration = ct.Register(() =>
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                    _logger.LogWarning("FFmpeg process killed due to cancellation request.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error killing FFmpeg process");
            }
        });

        await process.WaitForExitAsync(ct);

        if (process.ExitCode != 0 && !ct.IsCancellationRequested)
        {
            throw new InvalidOperationException($"FFmpeg process exited with code {process.ExitCode}");
        }
    }

    private static void ParseProgress(string logLine, IProgress<double>? progress)
    {
        if (progress == null) return;

        // Parse time=HH:MM:SS.ms from stderr output
        var match = Regex.Match(logLine, @"time=(\d{2}):(\d{2}):(\d{2}\.\d+)");
        if (match.Success)
        {
            if (double.TryParse(match.Groups[1].Value, out double hours) &&
                double.TryParse(match.Groups[2].Value, out double minutes) &&
                double.TryParse(match.Groups[3].Value, out double seconds))
            {
                double totalSecondsProcessed = (hours * 3600) + (minutes * 60) + seconds;
                progress.Report(totalSecondsProcessed);
            }
        }
    }

    private static void ValidatePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("File path cannot be null or empty", nameof(path));
        if (path.IndexOfAny(Path.GetInvalidPathChars()) >= 0) throw new ArgumentException("Path contains invalid characters", nameof(path));
        if (!File.Exists(path)) throw new FileNotFoundException($"Audio file not found at path: {path}", path);
    }
}
