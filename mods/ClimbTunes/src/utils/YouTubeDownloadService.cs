using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using BepInEx.Logging;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Collections.Generic;

namespace ClimbTunes.Utils
{
    public class YouTubeDownloadService
    {
        private static readonly ManualLogSource Logger = ClimbTunes.ModLogger;

        private static readonly string baseFolder = Path.Combine("d:\\SteamLibrary\\steamapps\\common\\PEAK\\BepInEx\\plugins\\ClimbTunes\\lib");
        private const string YTDLP_URL = "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe";
        private const string FFMPEG_URL = "https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip";
        
        private static readonly string ytDlpPath = Path.Combine(baseFolder, "yt-dlp.exe");
        private static readonly string ffmpegFolder = Path.Combine(baseFolder, "ffmpeg");
        private static string ffmpegBinPath = Path.Combine(ffmpegFolder, "bin", "ffmpeg.exe");
        
        private static readonly string cacheDirectory = Path.Combine("d:\\SteamLibrary\\steamapps\\common\\PEAK\\BepInEx\\plugins\\ClimbTunes", "audio_cache");
        private static readonly Dictionary<string, AudioClip> memoryCache = new Dictionary<string, AudioClip>();
        private static readonly int maxMemoryCacheSize = 5;
        
        private static bool isInitialized = false;

        public static async Task InitializeAsync()
        {
            if (isInitialized) return;

            try
            {
                if (!Directory.Exists(baseFolder))
                {
                    Directory.CreateDirectory(baseFolder);
                }

                if (!Directory.Exists(cacheDirectory))
                {
                    Directory.CreateDirectory(cacheDirectory);
                }

                if (!File.Exists(ytDlpPath))
                {
                    Logger.LogInfo("yt-dlp not found. Downloading...");
                    await DownloadFileAsync(YTDLP_URL, ytDlpPath);
                }

                bool needsFFmpeg = !File.Exists(ffmpegBinPath);
                if (!needsFFmpeg && !Directory.Exists(Path.GetDirectoryName(ffmpegBinPath)))
                {
                    needsFFmpeg = true;
                }

                if (needsFFmpeg)
                {
                    Logger.LogInfo("ffmpeg not found. Downloading and extracting...");
                    await DownloadAndExtractFFmpegAsync();
                }

                if (!File.Exists(ffmpegBinPath))
                {
                    throw new Exception($"ffmpeg executable was not found at {ffmpegBinPath} after extraction.");
                }

                isInitialized = true;
                Logger.LogInfo("YouTube download service initialization complete.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to initialize YouTube download service: {ex.Message}");
                throw;
            }
        }

        private static async Task DownloadFileAsync(string url, string destinationPath)
        {
            using HttpClient client = new();
            byte[] data = await client.GetByteArrayAsync(url);
            File.WriteAllBytes(destinationPath, data);
        }

        private static async Task DownloadAndExtractFFmpegAsync()
        {
            string zipPath = Path.Combine(baseFolder, "ffmpeg.zip");

            try
            {
                if (Directory.Exists(ffmpegFolder))
                {
                    try
                    {
                        Directory.Delete(ffmpegFolder, true);
                        Directory.CreateDirectory(ffmpegFolder);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning($"Failed to clean ffmpeg folder: {ex.Message}");
                    }
                }

                Logger.LogInfo($"Downloading FFmpeg from {FFMPEG_URL}...");
                await DownloadFileAsync(FFMPEG_URL, zipPath);

                if (!File.Exists(zipPath))
                {
                    throw new Exception("FFmpeg zip file not downloaded properly.");
                }

                Logger.LogInfo($"Downloaded FFmpeg zip file. Extracting...");
                ZipFile.ExtractToDirectory(zipPath, ffmpegFolder);
                File.Delete(zipPath);

                if (!File.Exists(ffmpegBinPath))
                {
                    Logger.LogInfo("FFmpeg not found at expected path. Searching for ffmpeg.exe in extracted files...");

                    string[] ffmpegFiles = Directory.GetFiles(ffmpegFolder, "ffmpeg.exe", SearchOption.AllDirectories);

                    if (ffmpegFiles.Length > 0)
                    {
                        string newPath = ffmpegFiles[0];
                        Logger.LogInfo($"Found ffmpeg.exe at: {newPath}");
                        ffmpegBinPath = newPath;
                    }
                    else
                    {
                        Logger.LogError("ffmpeg.exe not found in extracted files.");
                        throw new Exception("ffmpeg.exe not found in extracted files.");
                    }
                }

                Logger.LogInfo("FFmpeg extracted successfully.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error downloading or extracting FFmpeg: {ex.Message}");
                throw;
            }
        }

        public static bool IsValidYouTubeUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            try
            {
                return url.Contains("youtube.com/watch?v=") || 
                       url.Contains("youtu.be/") || 
                       url.Contains("youtube.com/embed/") ||
                       url.Contains("youtube.com/v/");
            }
            catch
            {
                return false;
            }
        }

        public static string GetCacheKey(string youtubeUrl)
        {
            try
            {
                Uri uri = new Uri(youtubeUrl);
                if (uri.Host.Contains("youtube.com"))
                {
                    var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                    return query["v"] ?? youtubeUrl.GetHashCode().ToString();
                }
                else if (uri.Host.Contains("youtu.be"))
                {
                    return uri.LocalPath.TrimStart('/');
                }
                
                return youtubeUrl.GetHashCode().ToString();
            }
            catch
            {
                return youtubeUrl.GetHashCode().ToString();
            }
        }

        public static bool IsAudioCached(string youtubeUrl)
        {
            string cacheKey = GetCacheKey(youtubeUrl);
            
            if (memoryCache.ContainsKey(cacheKey))
                return true;
                
            string cacheFilePath = Path.Combine(cacheDirectory, $"{cacheKey}.ogg");
            return File.Exists(cacheFilePath);
        }

        public static AudioClip GetCachedAudio(string youtubeUrl)
        {
            string cacheKey = GetCacheKey(youtubeUrl);
            
            if (memoryCache.TryGetValue(cacheKey, out AudioClip cachedClip))
            {
                Logger.LogInfo($"Retrieved audio from memory cache: {cacheKey}");
                return cachedClip;
            }

            return null;
        }

        public static void CacheAudioInMemory(string youtubeUrl, AudioClip audioClip)
        {
            if (audioClip == null) return;

            string cacheKey = GetCacheKey(youtubeUrl);

            if (memoryCache.ContainsKey(cacheKey))
            {
                memoryCache[cacheKey] = audioClip;
            }
            else
            {
                if (memoryCache.Count >= maxMemoryCacheSize)
                {
                    var oldestKey = memoryCache.Keys.FirstOrDefault();
                    if (!string.IsNullOrEmpty(oldestKey))
                    {
                        var oldClip = memoryCache[oldestKey];
                        memoryCache.Remove(oldestKey);
                        
                        if (oldClip != null)
                        {
                            UnityEngine.Object.Destroy(oldClip);
                        }
                    }
                }

                memoryCache.Add(cacheKey, audioClip);
                Logger.LogInfo($"Cached audio in memory: {cacheKey}");
            }
        }

        public static async Task<(string filePath, string title)> DownloadAudioWithTitleAsync(string videoUrl)
        {
            await InitializeAsync();

            string cacheKey = GetCacheKey(videoUrl);
            string cachedFilePath = Path.Combine(cacheDirectory, $"{cacheKey}.ogg");
            
            // Check if already cached on disk
            if (File.Exists(cachedFilePath))
            {
                string cachedTitle = await GetVideoTitleAsync(videoUrl);
                Logger.LogInfo($"Using cached audio: {cachedTitle}");
                return (cachedFilePath, cachedTitle);
            }

            string tempFolder = Path.Combine(cacheDirectory, "temp_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempFolder);

            Logger.LogInfo($"Getting title and downloading audio for {videoUrl}...");

            return await Task.Run(async () =>
            {
                try
                {
                    string title = await GetVideoTitleAsync(videoUrl);
                    if (string.IsNullOrEmpty(title))
                    {
                        title = "Unknown Title";
                    }

                    title = title.Replace("\n", "").Replace("\r", "").Trim();

                    string tempFileName = $"audio_{DateTime.Now.Ticks}.%(ext)s";
                    string command = $"-x --audio-format mp3 --audio-quality 192K --ffmpeg-location \"{ffmpegBinPath}\" --output \"{Path.Combine(tempFolder, tempFileName)}\" {videoUrl}";

                    ProcessStartInfo processInfo = new()
                    {
                        FileName = ytDlpPath,
                        Arguments = command,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.UTF8
                    };

                    using (Process process = Process.Start(processInfo))
                    {
                        if (process == null)
                        {
                            throw new Exception("Failed to start yt-dlp process.");
                        }

                        string output = await process.StandardOutput.ReadToEndAsync();
                        string error = await process.StandardError.ReadToEndAsync();

                        process.WaitForExit();

                        if (!string.IsNullOrEmpty(error))
                        {
                            Logger.LogWarning($"yt-dlp reported warnings: {error}");
                        }

                        if (process.ExitCode != 0)
                        {
                            throw new Exception($"yt-dlp download failed. Exit Code: {process.ExitCode}. Error: {error}");
                        }
                    }

                    await Task.Delay(1000);

                    string downloadedFile = Directory.GetFiles(tempFolder, "*.mp3").FirstOrDefault();
                    if (downloadedFile == null)
                    {
                        string[] allFiles = Directory.GetFiles(tempFolder);
                        Logger.LogError($"No MP3 files found. Total files: {allFiles.Length}");
                        foreach (string file in allFiles)
                        {
                            Logger.LogError($"Found file: {file}");
                        }
                        throw new Exception("Audio download failed. No MP3 file created.");
                    }

                    // Convert MP3 to OGG for Unity compatibility
                    string oggOutputPath = Path.Combine(cacheDirectory, $"{cacheKey}.ogg");
                    await ConvertToOggAsync(downloadedFile, oggOutputPath);

                    // Clean up temp folder
                    try
                    {
                        Directory.Delete(tempFolder, true);
                    }
                    catch (Exception cleanupEx)
                    {
                        Logger.LogWarning($"Failed to clean up temp folder: {cleanupEx}");
                    }

                    Logger.LogInfo($"Successfully downloaded and converted audio: {title}");
                    return (oggOutputPath, title);
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Download Error: {ex}");

                    if (Directory.Exists(tempFolder))
                    {
                        try
                        {
                            Directory.Delete(tempFolder, true);
                        }
                        catch (Exception cleanupEx)
                        {
                            Logger.LogError($"Failed to clean up temp folder: {cleanupEx}");
                        }
                    }
                    throw;
                }
            });
        }

        private static async Task ConvertToOggAsync(string inputPath, string outputPath)
        {
            ProcessStartInfo convertPsi = new ProcessStartInfo
            {
                FileName = ffmpegBinPath,
                Arguments = $"-i \"{inputPath}\" -c:a libvorbis -q:a 4 \"{outputPath}\" -y -loglevel error",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (Process convertProcess = Process.Start(convertPsi))
            {
                string error = await convertProcess.StandardError.ReadToEndAsync();
                convertProcess.WaitForExit();

                if (convertProcess.ExitCode != 0 && !string.IsNullOrEmpty(error))
                {
                    Logger.LogWarning($"ffmpeg conversion warning: {error}");
                }

                Logger.LogInfo("Audio conversion to OGG completed");
            }
        }

        private static async Task<string> GetVideoTitleAsync(string url)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = ytDlpPath,
                    Arguments = $"--get-title {url}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = new Process { StartInfo = psi })
                {
                    process.Start();

                    string title = await process.StandardOutput.ReadToEndAsync();
                    await process.StandardError.ReadToEndAsync();

                    var timeoutTask = Task.Delay(10000);
                    var processExitTask = Task.Run(() => process.WaitForExit());

                    if (await Task.WhenAny(processExitTask, timeoutTask) == timeoutTask)
                    {
                        try { process.Kill(); } catch { }
                        Logger.LogWarning("yt-dlp title fetch timed out");
                        return "Unknown Title (Timeout)";
                    }

                    if (process.ExitCode != 0)
                    {
                        Logger.LogError($"yt-dlp error code: {process.ExitCode}");
                        return "Unknown Title";
                    }

                    if (!string.IsNullOrEmpty(title))
                    {
                        try
                        {
                            title = new string(title.Where(c => !char.IsControl(c) || c == '\n' || c == '\r' || c == '\t').ToArray());
                        }
                        catch (Exception ex)
                        {
                            Logger.LogWarning($"Error sanitizing title: {ex.Message}");
                        }
                    }

                    return string.IsNullOrEmpty(title) ? "Unknown Title" : title.Trim();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error getting video title: {ex.Message}");
                return "Unknown Title";
            }
        }

        public static void ClearMemoryCache()
        {
            foreach (var clip in memoryCache.Values)
            {
                if (clip != null)
                {
                    UnityEngine.Object.Destroy(clip);
                }
            }
            memoryCache.Clear();
            Logger.LogInfo("Cleared audio memory cache");
        }

        public static void ClearDiskCache()
        {
            try
            {
                if (Directory.Exists(cacheDirectory))
                {
                    Directory.Delete(cacheDirectory, true);
                    Directory.CreateDirectory(cacheDirectory);
                    Logger.LogInfo("Cleared audio disk cache");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to clear disk cache: {ex.Message}");
            }
        }

        public static long GetCacheSize()
        {
            long totalSize = 0;
            try
            {
                if (Directory.Exists(cacheDirectory))
                {
                    var files = Directory.GetFiles(cacheDirectory);
                    foreach (var file in files)
                    {
                        totalSize += new FileInfo(file).Length;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to calculate cache size: {ex.Message}");
            }
            return totalSize;
        }

        // Compatibility method for existing code
        public static async Task<YouTubeVideoInfo> GetVideoInfoAsync(string url)
        {
            try
            {
                string title = await GetVideoTitleAsync(url);
                
                return new YouTubeVideoInfo
                {
                    Title = title,
                    Author = "Unknown",
                    Duration = TimeSpan.Zero,
                    Url = url,
                    AudioStreamUrl = url,
                    AudioBitrate = 0,
                    AudioSize = 0
                };
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to get video info: {ex.Message}");
                throw;
            }
        }
    }

    public class YouTubeVideoInfo
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public TimeSpan Duration { get; set; }
        public string Url { get; set; }
        public string AudioStreamUrl { get; set; }
        public long AudioBitrate { get; set; }
        public long AudioSize { get; set; }

        public string FormattedDuration 
        { 
            get 
            {
                if (Duration.TotalHours >= 1)
                    return Duration.ToString(@"h\:mm\:ss");
                else
                    return Duration.ToString(@"m\:ss");
            } 
        }

        public string FormattedSize
        {
            get
            {
                const long KB = 1024;
                const long MB = KB * 1024;
                
                if (AudioSize >= MB)
                    return $"{AudioSize / (double)MB:F1} MB";
                else if (AudioSize >= KB)
                    return $"{AudioSize / (double)KB:F1} KB";
                else
                    return $"{AudioSize} bytes";
            }
        }
    }
}