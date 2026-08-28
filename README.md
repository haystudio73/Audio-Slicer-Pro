# 🎵 Audio Slicer Pro — Windows 11 Native Audio Tool

> **Ứng dụng Windows 11 native** để cắt file audio thành từng đoạn 15 giây và chuyển đổi sang định dạng MP4 (audio-only video).

---

## 📋 Mô tả ứng dụng

**AudioSlicer Pro** là ứng dụng Windows 11 thuần (WinUI 3 / Win32) cho phép người dùng:

1. **Cắt file audio** (MP3, WAV, FLAC, AAC, OGG, …) thành các đoạn nhỏ có thời lượng **15 giây** mỗi đoạn.
2. **Chuyển đổi** từng đoạn audio sang **file MP4 video** (audio-only, không có track video — chỉ có audio stream).
3. **Quản lý thư mục**: chọn thư mục `tmp` để lưu file trung gian và thư mục đích sẽ tự động tạo theo tên file audio gốc.
4. **Đa ngôn ngữ**: hỗ trợ Tiếng Anh và Tiếng Việt; dễ dàng thêm ngôn ngữ khác.

---

## ✨ Tính năng chính

| Tính năng | Mô tả |
|---|---|
| 🎵 Hỗ trợ đa định dạng | MP3, WAV, FLAC, AAC, OGG, M4A, OPUS, … |
| ✂️ Cắt audio tự động | Tự động cắt thành các đoạn 15 giây |
| 🎬 Xuất MP4 audio-only | Mỗi đoạn được đóng gói thành file `.mp4` với stream audio |
| 📁 Quản lý thư mục | Chọn thư mục `tmp` và thư mục `dest` tự động đặt tên |
| 🌐 Đa ngôn ngữ | Anh / Việt, có thể mở rộng thêm ngôn ngữ |
| 📊 Thanh tiến trình | Hiển thị tiến trình xử lý từng file |
| 📝 Log hoạt động | Ghi log chi tiết từng bước xử lý |
| ⚙️ Cài đặt bền vững | Lưu cài đặt vào file JSON (không mất sau khi tắt app) |

---

## 🏗️ Kiến trúc ứng dụng

```
AudioSlicer Pro
├── UI Layer          → WinUI 3 (XAML + C#)
├── Business Logic    → C# .NET 8
├── Audio Processing  → FFmpeg (via Xabe.FFmpeg wrapper)
├── Settings          → JSON (System.Text.Json)
└── Localization      → RESW resource files
```

---

## 🛠️ Yêu cầu hệ thống

| Yêu cầu | Chi tiết |
|---|---|
| **Hệ điều hành** | Windows 10 (21H1+) / Windows 11 |
| **.NET Runtime** | .NET 8.0 (Desktop Runtime) |
| **Windows App SDK** | 1.5+ |
| **RAM** | Tối thiểu 512 MB |
| **Disk** | Tối thiểu 200 MB (không kể file tmp) |
| **FFmpeg** | Tự động tải qua NuGet (Xabe.FFmpeg) |

---

## 📦 Cài đặt (Development)

### Bước 1 — Cài đặt công cụ

```powershell
# 1. Cài đặt .NET 8 SDK
winget install Microsoft.DotNet.SDK.8

# 2. Cài đặt Visual Studio 2022 (Community hoặc cao hơn)
winget install Microsoft.VisualStudio.2022.Community
# Workloads cần chọn trong VS Installer:
#   - ".NET desktop development"
#   - "Windows application development" (WinUI 3)
```

### Bước 2 — Clone & Mở dự án

```powershell
git clone <repo-url>
cd "MP3 tools"
# Mở file AudioSlicerPro.sln trong Visual Studio 2022
```

### Bước 3 — Restore NuGet Packages

```powershell
dotnet restore
```

Các package chính sẽ tự động tải:
- `Microsoft.WindowsAppSDK`
- `Xabe.FFmpeg`
- `CommunityToolkit.WinUI`
- `Microsoft.Extensions.Logging`

### Bước 4 — Build & Run

```powershell
dotnet build -c Debug
# hoặc nhấn F5 trong Visual Studio
```

---

## 🚀 Hướng dẫn sử dụng

### 1. Cài đặt ban đầu (Settings)

1. Mở ứng dụng → nhấn **⚙️ Settings** (góc phải trên)
2. **Tmp Folder**: Chọn thư mục lưu file trung gian (vd: `C:\Temp\AudioSlicer`)
3. **Destination Folder**: Chọn thư mục gốc cho file đầu ra
4. **Language**: Chọn `English` hoặc `Tiếng Việt`
5. Nhấn **Save**

### 2. Chọn file Audio

1. Nhấn **📂 Open File** hoặc kéo-thả file vào cửa sổ
2. Hỗ trợ: `.mp3`, `.wav`, `.flac`, `.aac`, `.ogg`, `.m4a`, `.opus`
3. Thông tin file hiển thị ngay (tên, thời lượng, định dạng)

### 3. Xử lý (Slice & Convert)

1. Nhấn **▶️ Start Processing**
2. Ứng dụng thực hiện 2 bước:
   - **Bước 1**: Cắt audio → lưu từng đoạn 15s vào `tmp/`
   - **Bước 2**: Chuyển đổi mỗi đoạn → MP4 → lưu vào `dest/<TênFile>/`
3. Theo dõi tiến trình qua Progress Bar và Log Panel

### 4. Kết quả

```
<DestFolder>/<TênFileAudioGốc>/
  ├── segment_001.mp4   (0:00 - 0:15)
  ├── segment_002.mp4   (0:15 - 0:30)
  ├── segment_003.mp4   (0:30 - 0:45)
  └── ...
```

---

## 🌐 Thêm ngôn ngữ mới

1. Copy file `Strings/en-US/Resources.resw`
2. Đặt tên theo mã ngôn ngữ: `Strings/ja-JP/Resources.resw`
3. Dịch tất cả string values
4. Thêm entry vào `Languages.json`

---

## 📝 License

MIT License

## 🤝 Contributing

Vui lòng đọc [.agent/PLANS.md](.agent/PLANS.md) và [.agent/WORKFLOW.md](.agent/WORKFLOW.md) trước khi đóng góp.
