using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using System.IO.Compression;
using System.Threading;

namespace ClimbTunes.Utils
{
    public class ToolDownloadService
    {
        private static readonly string YTDLP_URL = "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe";
        private static readonly string FFMPEG_URL = "https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip";
        
        private readonly string toolsDirectory;
        private readonly HttpClient httpClient;

        public ToolDownloadService()
        {
            string modDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            toolsDirectory = Path.Combine(modDirectory, "lib");
            httpClient = new HttpClient();
        }

        public string GetYtDlpPath()
        {
            return Path.Combine(toolsDirectory, "yt-dlp.exe");
        }

        public string GetFfmpegPath()
        {
            return Path.Combine(toolsDirectory, "ffmpeg", "bin", "ffmpeg.exe");
        }

        public bool IsYtDlpAvailable()
        {
            return File.Exists(GetYtDlpPath());
        }

        public bool IsFfmpegAvailable()
        {
            return File.Exists(GetFfmpegPath());
        }

        public async Task<bool> EnsureToolsAvailable()
        {
            bool ytDlpReady = await EnsureYtDlpAvailable();
            bool ffmpegReady = await EnsureFfmpegAvailable();
            
            return ytDlpReady && ffmpegReady;
        }

        public async Task<bool> EnsureYtDlpAvailable()
        {
            if (IsYtDlpAvailable())
            {
                ClimbTunes.ModLogger.LogInfo("yt-dlp.exe already available");
                return true;
            }

            try
            {
                ClimbTunes.ModLogger.LogInfo("Downloading yt-dlp.exe...");
                
                using (var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5)))
                using (var response = await httpClient.GetAsync(YTDLP_URL, cts.Token))
                {
                    response.EnsureSuccessStatusCode();
                    
                    byte[] content = await response.Content.ReadAsByteArrayAsync();
                    string ytDlpPath = GetYtDlpPath();
                    
                    // Ensure directory exists
                    Directory.CreateDirectory(Path.GetDirectoryName(ytDlpPath));
                    
                    File.WriteAllBytes(ytDlpPath, content);
                    
                    // Verify file was written correctly
                    if (File.Exists(ytDlpPath) && new FileInfo(ytDlpPath).Length > 0)
                    {
                        ClimbTunes.ModLogger.LogInfo($"yt-dlp.exe downloaded successfully: {new FileInfo(ytDlpPath).Length} bytes");
                        return true;
                    }
                    else
                    {
                        ClimbTunes.ModLogger.LogError("yt-dlp.exe download verification failed");
                        return false;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                ClimbTunes.ModLogger.LogError("yt-dlp.exe download timeout (5 minutes)");
                return false;
            }
            catch (Exception ex)
            {
                ClimbTunes.ModLogger.LogError($"Failed to download yt-dlp.exe: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> EnsureFfmpegAvailable()
        {
            if (IsFfmpegAvailable())
            {
                ClimbTunes.ModLogger.LogInfo("ffmpeg.exe already available");
                return true;
            }

            try
            {
                ClimbTunes.ModLogger.LogInfo("Downloading ffmpeg...");
                
                string tempZipPath = Path.Combine(toolsDirectory, "ffmpeg-temp.zip");
                
                using (var response = await httpClient.GetAsync(FFMPEG_URL))
                {
                    response.EnsureSuccessStatusCode();
                    
                    byte[] content = await response.Content.ReadAsByteArrayAsync();
                    File.WriteAllBytes(tempZipPath, content);
                }

                ClimbTunes.ModLogger.LogInfo("Extracting ffmpeg...");
                
                string extractPath = Path.Combine(toolsDirectory, "ffmpeg-extract");
                if (Directory.Exists(extractPath))
                {
                    Directory.Delete(extractPath, true);
                }
                
                ZipFile.ExtractToDirectory(tempZipPath, extractPath);
                
                // Find the actual ffmpeg folder in the extracted content
                string[] subDirs = Directory.GetDirectories(extractPath);
                if (subDirs.Length > 0)
                {
                    string sourceDir = subDirs[0];
                    string targetDir = Path.Combine(toolsDirectory, "ffmpeg");
                    
                    if (Directory.Exists(targetDir))
                    {
                        Directory.Delete(targetDir, true);
                    }
                    
                    Directory.Move(sourceDir, targetDir);
                }
                
                // Clean up temporary files
                File.Delete(tempZipPath);
                Directory.Delete(extractPath, true);
                
                if (IsFfmpegAvailable())
                {
                    ClimbTunes.ModLogger.LogInfo($"ffmpeg.exe extracted successfully to: {GetFfmpegPath()}");
                    return true;
                }
                else
                {
                    ClimbTunes.ModLogger.LogError("ffmpeg.exe not found after extraction");
                    return false;
                }
            }
            catch (Exception ex)
            {
                ClimbTunes.ModLogger.LogError($"Failed to download/extract ffmpeg: {ex.Message}");
                return false;
            }
        }

        public void Dispose()
        {
            httpClient?.Dispose();
        }
    }
}