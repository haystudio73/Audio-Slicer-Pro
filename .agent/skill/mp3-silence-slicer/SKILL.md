---
name: mp3-silence-slicer
description: Skill cắt nhỏ file MP3 kết hợp tự động loại bỏ khoảng lặng (silence) ở đầu (first) và cuối (last) file audio.
---

# MP3 Trim Silence & Audio Slicer Skill

Skill này hướng dẫn quy trình cắt nhỏ file âm thanh (MP3, WAV, FLAC, ...) thành các phân đoạn (segments) nhỏ, đồng thời tự động phát hiện và xoá bỏ phần im lặng (silence) ở đầu (beginning/first) và cuối (ending/last) của file audio.

---

## 1. Phương pháp 1: Sử dụng lệnh FFmpeg (Khuyên dùng & Tốc độ nhanh nhất)

### Bước 1: Trim khoảng lặng ở đầu và cuối file (Trim First & Last Silence)

Cú pháp FFmpeg filter `silenceremove`:
```bash
ffmpeg -i "input.mp3" -af "silenceremove=start_periods=1:start_duration=0.5:start_threshold=-50dB:stop_periods=-1:stop_duration=0.5:stop_threshold=-50dB" -c:a libmp3lame -q:a 2 "trimmed.mp3"
```

**Giải thích tham số:**
- `start_periods=1`: Xoá khoảng lặng ở đầu file.
- `start_duration=0.5`: Độ dài khoảng lặng tối thiểu ở đầu (ví dụ: 0.5 giây).
- `start_threshold=-50dB`: Ngưỡng âm thanh dưới -50dB được tính là khoảng lặng.
- `stop_periods=-1`: Xoá khoảng lặng ở cuối file.
- `stop_duration=0.5`: Độ dài khoảng lặng tối thiểu ở cuối.
- `stop_threshold=-50dB`: Ngưỡng âm thanh ở cuối file.

### Bước 2: Cắt nhỏ file đã trim thành các phân đoạn (ví dụ: 15 giây/file)

```bash
ffmpeg -i "trimmed.mp3" -f segment -segment_time 15 -c copy "segment_%03d.mp3"
```

### Kết hợp 1 bước duy nhất (One-Liner FFmpeg)

```bash
ffmpeg -i "input.mp3" -af "silenceremove=start_periods=1:start_duration=0.5:start_threshold=-50dB:stop_periods=-1:stop_duration=0.5:stop_threshold=-50dB" -f segment -segment_time 15 -c:a libmp3lame -q:a 2 "output/segment_%03d.mp3"
```

---

## 2. Phương pháp 2: Script Python (`pydub`)

Nếu bạn muốn xử lý bằng code Python linh hoạt:

```python
import sys
from pathlib import Path
from pydub import AudioSegment
from pydub.silence import detect_leading_silence

def trim_silence(audio: AudioSegment, silence_threshold: int = -50, chunk_size: int = 10) -> AudioSegment:
    # Trim silence at start (first)
    start_trim = detect_leading_silence(audio, silence_threshold=silence_threshold, chunk_size=chunk_size)
    # Trim silence at end (last)
    end_trim = detect_leading_silence(audio.reverse(), silence_threshold=silence_threshold, chunk_size=chunk_size)
    
    duration = len(audio)
    return audio[start_trim : duration - end_trim]

def slice_audio(audio: AudioSegment, segment_length_ms: int = 15000, output_dir: Path = Path("output")):
    output_dir.mkdir(parents=True, exist_ok=True)
    total_ms = len(audio)
    
    index = 1
    for start_ms in range(0, total_ms, segment_length_ms):
        end_ms = min(start_ms + segment_length_ms, total_ms)
        chunk = audio[start_ms:end_ms]
        output_file = output_dir / f"segment_{index:03d}.mp3"
        chunk.export(output_file, format="mp3", bitrate="192k")
        print(f"Exported: {output_file} ({len(chunk)/1000:.2f}s)")
        index += 1

def process_mp3(file_path: str, segment_seconds: int = 15):
    path = Path(file_path)
    print(f"Loading {path}...")
    audio = AudioSegment.from_file(path)
    
    print("Trimming leading & trailing silence...")
    trimmed_audio = trim_silence(audio, silence_threshold=-50)
    
    print(f"Slicing into {segment_seconds}s chunks...")
    slice_audio(trimmed_audio, segment_length_ms=segment_seconds * 1000, output_dir=path.parent / "output")

if __name__ == "__main__":
    if len(sys.argv) > 1:
        process_mp3(sys.argv[1])
```

---

## 3. Phương pháp 3: Tích hợp C# / Xabe.FFmpeg (Cho AudioSlicer Pro)

Dành cho dự án C# WinUI 3:

```csharp
using Xabe.FFmpeg;

public async Task TrimAndSliceAudioAsync(string inputPath, string outputFolder, int segmentSeconds = 15)
{
    Directory.CreateDirectory(outputFolder);
    string outputPattern = Path.Combine(outputFolder, "segment_%03d.mp3");

    // FFmpeg filter: remove silence from beginning & end, then segment
    string arguments = $"-i \"{inputPath}\" " +
                       $"-af \"silenceremove=start_periods=1:start_duration=0.5:start_threshold=-50dB:stop_periods=-1:stop_duration=0.5:stop_threshold=-50dB\" " +
                       $"-f segment -segment_time {segmentSeconds} -c:a libmp3lame -q:a 2 \"{outputPattern}\"";

    IConversion conversion = FFmpeg.Conversions.New();
    conversion.AddParameter(arguments);
    await conversion.Start();
}
```

---

## 📌 Lưu ý quan trọng
1. **Ngưỡng Silence (`-50dB`)**: Có thể điều chỉnh từ `-40dB` đến `-60dB` tùy độ nhiễu nền của file MP3.
2. **Thời gian im lặng (`start_duration`/`stop_duration`)**: Đặt `0.5` giây để tránh cắt nhầm các khoảng dừng nghỉ giọng tự nhiên trong nói/hát.
3. **Re-encoding (`libmp3lame`)**: Khi dùng audio filter (`-af`), FFmpeg bắt buộc phải re-encode lại audio (không dùng `-c copy` trực tiếp trên filter).
