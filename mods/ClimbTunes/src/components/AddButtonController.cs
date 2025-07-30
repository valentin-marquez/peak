using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClimbTunes.Manager;
using ClimbTunes.Utils;

namespace ClimbTunes.Components
{
    public class AddButtonController : MonoBehaviour
    {
        [Header("UI References")]
        public Button addButton;
        public TMP_InputField urlInputField;
        public Transform contentParent;
        public GameObject playlistItemPrefab;
        public TextMeshProUGUI emptyStateText;

        [Header("Playlist Data")]
        public List<PlaylistTrack> playlist = new List<PlaylistTrack>();
        
        private YouTubeService youtubeService;

        private void Awake()
        {
            AutoConfigureReferences();
        }

        private void Start()
        {
            youtubeService = new YouTubeService();
            SetupButtonListeners();
            UpdateEmptyState();
        }

        private void AutoConfigureReferences()
        {
            ClimbTunes.ModLogger.LogInfo("AddButtonController: Starting auto-configuration...");
            
            // Auto-find references based on radio prefab structure
            if (addButton == null)
            {
                addButton = GetComponent<Button>();
                ClimbTunes.ModLogger.LogInfo($"AddButtonController: Button found: {addButton != null}");
            }

            // Find URL input field in the canvas
            Transform canvas = FindCanvasInParents();
            ClimbTunes.ModLogger.LogInfo($"AddButtonController: Canvas found: {canvas != null}");
            
            if (canvas != null)
            {
                if (urlInputField == null)
                {
                    // Correct path based on the structure: RadioPanel/InputPanel/URLInputField
                    Transform urlInput = canvas.Find("RadioPanel/InputPanel/URLInputField");
                    ClimbTunes.ModLogger.LogInfo($"AddButtonController: URLInputField transform found: {urlInput != null}");
                    
                    if (urlInput != null)
                    {
                        urlInputField = urlInput.GetComponent<TMP_InputField>();
                        ClimbTunes.ModLogger.LogInfo($"AddButtonController: TMP_InputField component found: {urlInputField != null}");
                    }
                    else
                    {
                        ClimbTunes.ModLogger.LogWarning("AddButtonController: URLInputField not found at RadioPanel/InputPanel/URLInputField");
                    }
                }

                if (contentParent == null)
                {
                    Transform content = canvas.Find("RadioPanel/PlaylistPanel/PlaylistScrollView/Viewport/Content");
                    if (content != null)
                    {
                        contentParent = content;
                        ClimbTunes.ModLogger.LogInfo("AddButtonController: Content parent found");
                    }
                    else
                    {
                        ClimbTunes.ModLogger.LogWarning("AddButtonController: Content parent not found");
                    }
                }

                if (emptyStateText == null)
                {
                    Transform emptyText = canvas.Find("RadioPanel/PlaylistPanel/EmptyStateText");
                    if (emptyText != null)
                    {
                        emptyStateText = emptyText.GetComponent<TextMeshProUGUI>();
                        ClimbTunes.ModLogger.LogInfo("AddButtonController: Empty state text found");
                    }
                    else
                    {
                        ClimbTunes.ModLogger.LogWarning("AddButtonController: Empty state text not found");
                    }
                }
            }

            // Try to find or load playlist item prefab
            if (playlistItemPrefab == null)
            {
                TryLoadPlaylistItemPrefab();
            }

            ClimbTunes.ModLogger.LogInfo($"AddButtonController auto-configured - Button: {addButton != null}, InputField: {urlInputField != null}, Content: {contentParent != null}, EmptyText: {emptyStateText != null}, Prefab: {playlistItemPrefab != null}");
        }

        private Transform FindCanvasInParents()
        {
            Transform current = transform;
            while (current != null)
            {
                if (current.name == "Canvas" || current.GetComponent<Canvas>() != null)
                {
                    return current;
                }
                current = current.parent;
            }
            return null;
        }

        private void TryLoadPlaylistItemPrefab()
        {
            // Get the prefab from AssetManager using static method
            playlistItemPrefab = AssetManager.GetStaticPlaylistItemPrefab();
            
            if (playlistItemPrefab != null)
            {
                ClimbTunes.ModLogger.LogInfo("Playlist item prefab loaded successfully from AssetManager");
            }
            else
            {
                ClimbTunes.ModLogger.LogWarning("Failed to load playlist item prefab from AssetManager");
            }
        }

        private void SetupButtonListeners()
        {
            ClimbTunes.ModLogger.LogInfo("AddButtonController: Setting up button listeners...");
            
            if (addButton != null)
            {
                addButton.onClick.RemoveAllListeners();
                addButton.onClick.AddListener(AddTrackToPlaylist);
                ClimbTunes.ModLogger.LogInfo("AddButtonController: Button listener added successfully");
            }
            else
            {
                ClimbTunes.ModLogger.LogWarning("AddButtonController: Add button is null, cannot setup listeners");
            }
        }

        public void AddTrackToPlaylist()
        {
            // Start async operation
            _ = AddTrackToPlaylistAsync();
        }

        private async Task AddTrackToPlaylistAsync()
        {
            ClimbTunes.ModLogger.LogInfo("AddButtonController: AddTrackToPlaylist called");
            
            if (urlInputField == null)
            {
                ClimbTunes.ModLogger.LogWarning("AddButtonController: URL input field is null");
                UpdateStatusText("Error");
                return;
            }
            
            string inputText = urlInputField.text;
            
            if (string.IsNullOrEmpty(inputText))
            {
                ClimbTunes.ModLogger.LogWarning("AddButtonController: URL input is empty");
                UpdateStatusText("Enter URL");
                return;
            }

            string url = inputText.Trim();
            ClimbTunes.ModLogger.LogInfo($"AddButtonController: Processing URL: '{url}'");
            
            // Validate YouTube URL
            if (!youtubeService.IsValidYouTubeUrl(url))
            {
                ClimbTunes.ModLogger.LogWarning($"AddButtonController: Invalid YouTube URL: {url}");
                UpdateStatusText("Invalid URL");
                return;
            }

            // Check for duplicates
            if (IsUrlAlreadyInPlaylist(url))
            {
                ClimbTunes.ModLogger.LogInfo($"AddButtonController: URL already in playlist: {url}");
                UpdateStatusText("Duplicate");
                return;
            }

            // Update status while fetching
            UpdateStatusText("Loading");

            try
            {
                // Fetch video information
                var videoInfo = await youtubeService.GetVideoInfoAsync(url);
                
                // Create track data with real title
                PlaylistTrack newTrack = new PlaylistTrack
                {
                    url = url,
                    title = videoInfo.Title,
                    isLoaded = false
                };

                ClimbTunes.ModLogger.LogInfo($"AddButtonController: Created track - Title: {newTrack.title}, URL: {newTrack.url}");

                // Add to playlist
                playlist.Add(newTrack);
                ClimbTunes.ModLogger.LogInfo($"AddButtonController: Track added to playlist. Total tracks: {playlist.Count}");

                // Create UI item
                CreatePlaylistItem(newTrack);

                // Clear input field
                urlInputField.text = "";

                // Update empty state
                UpdateEmptyState();

                // Update status
                UpdateStatusText("Added");
                
                ClimbTunes.ModLogger.LogInfo($"AddButtonController: Successfully added track to playlist: {newTrack.title}");
            }
            catch (System.Exception ex)
            {
                ClimbTunes.ModLogger.LogError($"AddButtonController: Failed to fetch video info: {ex.Message}");
                UpdateStatusText("Error");
            }
        }

        private void CreatePlaylistItem(PlaylistTrack track)
        {
            if (playlistItemPrefab == null || contentParent == null)
            {
                Debug.LogError("PlaylistItem prefab or content parent not assigned");
                return;
            }

            GameObject newItem = Instantiate(playlistItemPrefab, contentParent);
            
            // Apply game font to fix magenta text issue
            FontManager.ApplyGameFontToPrefab(newItem);
            
            PlaylistItemController itemController = newItem.GetComponent<PlaylistItemController>();
            
            if (itemController != null)
            {
                itemController.Initialize(track, this);
            }
        }

        public void PlayTrack(PlaylistTrack track)
        {
            // Find RadioController and load the track
            RadioController radioController = FindObjectOfType<RadioController>();
            if (radioController != null)
            {
                radioController.LoadAndPlayUrl(track.url);
            }
        }

        public void RemoveTrack(PlaylistTrack track)
        {
            playlist.Remove(track);
            UpdateEmptyState();
        }

        private void UpdateEmptyState()
        {
            if (emptyStateText != null)
            {
                emptyStateText.gameObject.SetActive(playlist.Count == 0);
            }
        }

        private bool IsValidUrl(string url)
        {
            return System.Uri.TryCreate(url, System.UriKind.Absolute, out System.Uri result) 
                   && (result.Scheme == System.Uri.UriSchemeHttp || result.Scheme == System.Uri.UriSchemeHttps);
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

        private bool IsUrlAlreadyInPlaylist(string url)
        {
            foreach (var track in playlist)
            {
                if (track.url.Equals(url, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private void UpdateStatusText(string message)
        {
            // Find RadioController to update status
            RadioController radioController = FindObjectOfType<RadioController>();
            if (radioController != null)
            {
                radioController.UpdateStatus(message);
            }
        }

        private void OnDestroy()
        {
            if (addButton != null)
            {
                addButton.onClick.RemoveListener(AddTrackToPlaylist);
            }
        }
    }

    [System.Serializable]
    public class PlaylistTrack
    {
        public string url;
        public string title;
        public bool isLoaded;
        public float duration;
    }
}