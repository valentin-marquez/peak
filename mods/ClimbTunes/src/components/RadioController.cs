using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Threading.Tasks;
using System.IO;
using TMPro;
using UnityEngine.UI;
using ClimbTunes.Utils;

namespace ClimbTunes.Components
{
    public class RadioController : MonoBehaviour
    {
        [Header("Audio Components")]
        public AudioSource audioSource;

        [Header("UI References")]
        public TextMeshProUGUI statusText;
        public Slider volumeSlider;
        public PlayPauseController playPauseController;
        public AddButtonController addButtonController;

        [Header("Settings")]
        [Range(0f, 1f)]
        public float defaultVolume = 0.5f;

        private bool isLoading = false;
        private string currentUrl = "";
        private string currentTrackTitle = "";
        private YouTubeService youtubeService;
        private YouTubeVideoInfo currentVideoInfo;
        private AudioCacheService cacheService;

        private void Start()
        {
            InitializeRadio();
        }

        private void InitializeRadio()
        {
            ClimbTunes.ModLogger.LogInfo("RadioController: Initializing radio...");
            
            // Initialize services
            youtubeService = new YouTubeService();
            cacheService = new AudioCacheService();
            ClimbTunes.ModLogger.LogInfo("RadioController: Services initialized");

            // Setup audio source
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    ClimbTunes.ModLogger.LogInfo("RadioController: AudioSource component added");
                }
                else
                {
                    ClimbTunes.ModLogger.LogInfo("RadioController: AudioSource found on GameObject");
                }
            }
            else
            {
                ClimbTunes.ModLogger.LogInfo("RadioController: AudioSource already assigned");
            }

            audioSource.volume = defaultVolume;
            audioSource.playOnAwake = false;
            ClimbTunes.ModLogger.LogInfo($"RadioController: AudioSource configured - Volume: {defaultVolume}");

            // Setup volume slider
            if (volumeSlider != null)
            {
                volumeSlider.value = defaultVolume;
                volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
                ClimbTunes.ModLogger.LogInfo("RadioController: Volume slider configured");
            }
            else
            {
                ClimbTunes.ModLogger.LogWarning("RadioController: Volume slider is null");
            }

            // Connect PlayPauseController
            if (playPauseController != null)
            {
                playPauseController.audioSource = audioSource;
                ClimbTunes.ModLogger.LogInfo("RadioController: PlayPauseController connected");
            }
            else
            {
                ClimbTunes.ModLogger.LogWarning("RadioController: PlayPauseController is null");
            }

            UpdateStatus("Ready");
            ClimbTunes.ModLogger.LogInfo("RadioController: Initialization complete");
        }

        public void LoadAndPlayUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                UpdateStatus("Invalid URL");
                return;
            }

            if (isLoading)
            {
                UpdateStatus("loading...");
                return;
            }

            // Validate YouTube URL first
            if (!youtubeService.IsValidYouTubeUrl(url))
            {
                UpdateStatus("Invalid YouTube URL");
                return;
            }

            currentUrl = url;
            StartCoroutine(LoadYouTubeAudio(url));
        }

        private IEnumerator LoadYouTubeAudio(string url)
        {
            isLoading = true;

            // Stop current audio if playing
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            // Check cache first
            AudioClip cachedClip = cacheService.GetCachedAudio(url);
            if (cachedClip != null)
            {
                // Get video info for display (quick operation)
                UpdateStatus("Loading from cache...");
                Task<YouTubeVideoInfo> videoInfoTask = youtubeService.GetVideoInfoAsync(url);
                yield return new WaitUntil(() => videoInfoTask.IsCompleted);

                if (videoInfoTask.Exception == null)
                {
                    currentVideoInfo = videoInfoTask.Result;
                    currentTrackTitle = currentVideoInfo.Title;
                }

                audioSource.clip = cachedClip;
                audioSource.Play();
                UpdateStatus("Playing (cached)");
                ClimbTunes.ModLogger.LogInfo($"Playing cached audio: {currentTrackTitle}");
                
                isLoading = false;
                yield break;
            }

            UpdateStatus("Loading");

            // Get video info and audio stream URL
            Task<YouTubeVideoInfo> getVideoInfoTask = youtubeService.GetVideoInfoAsync(url);
            yield return new WaitUntil(() => getVideoInfoTask.IsCompleted);

            if (getVideoInfoTask.Exception != null)
            {
                UpdateStatus($"Error: {getVideoInfoTask.Exception.GetBaseException().Message}");
                ClimbTunes.ModLogger.LogError($"Failed to get video info: {getVideoInfoTask.Exception.GetBaseException().Message}");
                isLoading = false;
                yield break;
            }

            currentVideoInfo = getVideoInfoTask.Result;
            currentTrackTitle = currentVideoInfo.Title;

            UpdateStatus("Downloading");
            ClimbTunes.ModLogger.LogInfo($"Downloading YouTube audio: {currentTrackTitle} ({currentVideoInfo.FormattedDuration})");

            // Download audio file to disk using YoutubeExplode direct download
            string cacheKey = cacheService.GetCacheKey(url);
            string baseAudioFilePath = Path.Combine(cacheService.GetCacheDirectory(), cacheKey);
            
            // Check for existing OGG files (our new standard format)
            string oggFilePath = baseAudioFilePath + ".ogg";
            string existingFile = null;
            
            if (File.Exists(oggFilePath))
            {
                existingFile = oggFilePath;
            }
            
            if (existingFile != null)
            {
                ClimbTunes.ModLogger.LogInfo($"Audio file found on disk: {existingFile}");
                yield return LoadAudioFromDisk(existingFile);
                isLoading = false;
                yield break;
            }
            
            // Download and convert using yt-dlp + ffmpeg
            Task<string> downloadTask = youtubeService.DownloadAudioAsync(url, baseAudioFilePath);
            yield return new WaitUntil(() => downloadTask.IsCompleted);

            if (downloadTask.Exception == null && !string.IsNullOrEmpty(downloadTask.Result))
            {
                string downloadedFilePath = downloadTask.Result;
                ClimbTunes.ModLogger.LogInfo($"Audio downloaded and converted successfully: {downloadedFilePath}");
                yield return LoadAudioFromDisk(downloadedFilePath);
            }
            else
            {
                UpdateStatus("Error");
                string errorMessage = downloadTask.Exception?.GetBaseException().Message ?? "Unknown download error";
                ClimbTunes.ModLogger.LogError($"Failed to download and convert audio: {errorMessage}");
            }

            isLoading = false;
        }

        private void OnVolumeChanged(float volume)
        {
            if (audioSource != null)
            {
                audioSource.volume = volume;
            }
        }

        public void NextTrack()
        {
            ClimbTunes.ModLogger.LogInfo("RadioController: NextTrack() called");
            
            if (addButtonController != null && addButtonController.playlist.Count > 0)
            {
                // Find current track index and play next
                int currentIndex = addButtonController.playlist.FindIndex(t => t.url == currentUrl);
                int nextIndex = (currentIndex + 1) % addButtonController.playlist.Count;
                
                ClimbTunes.ModLogger.LogInfo($"RadioController: Moving from track {currentIndex} to {nextIndex} (total: {addButtonController.playlist.Count})");
                
                PlaylistTrack nextTrack = addButtonController.playlist[nextIndex];
                ClimbTunes.ModLogger.LogInfo($"RadioController: Loading next track: {nextTrack.title}");
                LoadAndPlayUrl(nextTrack.url);
            }
            else if (addButtonController == null)
            {
                ClimbTunes.ModLogger.LogWarning("RadioController: Cannot go to next track - addButtonController is null");
            }
            else
            {
                ClimbTunes.ModLogger.LogWarning("RadioController: Cannot go to next track - playlist is empty");
            }
        }

        public void PreviousTrack()
        {
            ClimbTunes.ModLogger.LogInfo("RadioController: PreviousTrack() called");
            
            if (addButtonController != null && addButtonController.playlist.Count > 0)
            {
                // Find current track index and play previous
                int currentIndex = addButtonController.playlist.FindIndex(t => t.url == currentUrl);
                int prevIndex = currentIndex <= 0 ? addButtonController.playlist.Count - 1 : currentIndex - 1;
                
                ClimbTunes.ModLogger.LogInfo($"RadioController: Moving from track {currentIndex} to {prevIndex} (total: {addButtonController.playlist.Count})");
                
                PlaylistTrack prevTrack = addButtonController.playlist[prevIndex];
                ClimbTunes.ModLogger.LogInfo($"RadioController: Loading previous track: {prevTrack.title}");
                LoadAndPlayUrl(prevTrack.url);
            }
            else if (addButtonController == null)
            {
                ClimbTunes.ModLogger.LogWarning("RadioController: Cannot go to previous track - addButtonController is null");
            }
            else
            {
                ClimbTunes.ModLogger.LogWarning("RadioController: Cannot go to previous track - playlist is empty");
            }
        }

        public void UpdateStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }

        private string ExtractTitleFromUrl(string url)
        {
            try
            {
                System.Uri uri = new System.Uri(url);
                string fileName = System.IO.Path.GetFileNameWithoutExtension(uri.LocalPath);
                
                if (string.IsNullOrEmpty(fileName))
                {
                    return "Unknown Track";
                }

                return fileName.Replace("_", " ").Replace("-", " ");
            }
            catch
            {
                return "Unknown Track";
            }
        }

        public string GetCurrentTrackTitle()
        {
            return currentTrackTitle;
        }

        public YouTubeVideoInfo GetCurrentVideoInfo()
        {
            return currentVideoInfo;
        }

        public string GetCurrentTrackInfo()
        {
            if (currentVideoInfo == null)
                return currentTrackTitle;
            
            return $"{currentVideoInfo.Title} by {currentVideoInfo.Author} ({currentVideoInfo.FormattedDuration})";
        }

        public bool IsPlaying()
        {
            return audioSource != null && audioSource.isPlaying;
        }

        public bool IsLoading()
        {
            return isLoading;
        }

        public void ClearAudioCache()
        {
            if (cacheService != null)
            {
                cacheService.ClearMemoryCache();
                UpdateStatus("Audio cache cleared");
            }
        }

        public string GetCacheInfo()
        {
            if (cacheService != null)
            {
                int memoryCount = cacheService.GetMemoryCacheCount();
                return $"Memory cache: {memoryCount} tracks";
            }
            return "Cache not available";
        }

        private IEnumerator LoadAudioFromDisk(string audioFilePath)
        {
            UpdateStatus("Loading from disk");
            
            // Since we're standardizing on OGG format, we expect OGG files
            AudioType audioType = AudioType.OGGVORBIS;
            string extension = Path.GetExtension(audioFilePath).ToLower();
            
            if (extension != ".ogg")
            {
                ClimbTunes.ModLogger.LogWarning($"Expected OGG file but got: {extension}");
                // Try to handle other formats for backward compatibility
                switch (extension)
                {
                    case ".mp3":
                    case ".m4a":
                    case ".mp4":
                        audioType = AudioType.MPEG;
                        break;
                    case ".wav":
                        audioType = AudioType.WAV;
                        break;
                    default:
                        audioType = AudioType.OGGVORBIS; // Default fallback
                        break;
                }
            }
            
            ClimbTunes.ModLogger.LogInfo($"Loading audio file: {audioFilePath} as {audioType}");
            
            string fileUrl = "file://" + audioFilePath.Replace("\\", "/");
            using (UnityWebRequest audioWWW = UnityWebRequestMultimedia.GetAudioClip(fileUrl, audioType))
            {
                yield return audioWWW.SendWebRequest();
                
                if (audioWWW.result == UnityWebRequest.Result.Success)
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(audioWWW);
                    if (clip != null)
                    {
                        ClimbTunes.ModLogger.LogInfo($"AudioClip loaded from disk - Name: {clip.name}, Length: {clip.length}s, Frequency: {clip.frequency}, Channels: {clip.channels}");
                        
                        audioSource.clip = clip;
                        
                        // Cache the audio for future use
                        cacheService.CacheAudio(currentVideoInfo?.Url ?? "unknown", clip);
                        
                        // Auto-play after loading
                        audioSource.Play();
                        UpdateStatus("Playing");
                        
                        ClimbTunes.ModLogger.LogInfo($"Successfully loaded and playing from disk: {currentTrackTitle}");
                    }
                    else
                    {
                        UpdateStatus("Error");
                        ClimbTunes.ModLogger.LogError("Failed to create AudioClip from disk file");
                    }
                }
                else
                {
                    UpdateStatus("Error");
                    ClimbTunes.ModLogger.LogError($"Failed to load audio from disk: {audioWWW.error}");
                }
            }
        }

        private void OnDestroy()
        {
            if (volumeSlider != null)
            {
                volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
            }

            // Clean up cache
            if (cacheService != null)
            {
                cacheService.ClearMemoryCache();
            }
        }
    }
}