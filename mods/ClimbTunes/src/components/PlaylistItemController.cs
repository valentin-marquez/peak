using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ClimbTunes.Components
{
    public class PlaylistItemController : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI trackText;
        public Button playButton;
        public Button removeButton;

        [Header("Visual States")]
        public Color normalColor = Color.white;
        public Color playingColor = Color.green;
        public Color selectedColor = Color.cyan;

        private PlaylistTrack trackData;
        private AddButtonController playlistController;
        private Image backgroundImage;
        private bool isCurrentTrack = false;

        private void Awake()
        {
            backgroundImage = GetComponent<Image>();
            AutoConfigureReferences();
        }

        private void Start()
        {
            SetupButtonListeners();
        }

        private void AutoConfigureReferences()
        {
            // Auto-find references based on PlaylistItem prefab structure
            if (trackText == null)
            {
                Transform trackTextTransform = transform.Find("TrackText/Text");
                if (trackTextTransform != null)
                {
                    trackText = trackTextTransform.GetComponent<TextMeshProUGUI>();
                    ClimbTunes.ModLogger.LogInfo("PlaylistItemController: TrackText found and assigned");
                }
                else
                {
                    ClimbTunes.ModLogger.LogWarning("PlaylistItemController: TrackText/Text not found");
                }
            }

            if (playButton == null)
            {
                Transform playButtonTransform = transform.Find("PlayButton");
                if (playButtonTransform != null)
                {
                    playButton = playButtonTransform.GetComponent<Button>();
                }
            }

            if (removeButton == null)
            {
                // Look for PauseButton as remove button (based on prefab structure)
                Transform removeButtonTransform = transform.Find("PauseButton");
                if (removeButtonTransform != null)
                {
                    removeButton = removeButtonTransform.GetComponent<Button>();
                }
            }

            ClimbTunes.ModLogger.LogInfo($"PlaylistItemController auto-configured - TrackText: {trackText != null}, PlayButton: {playButton != null}, RemoveButton: {removeButton != null}");
        }

        private void SetupButtonListeners()
        {
            if (playButton != null)
            {
                playButton.onClick.RemoveAllListeners();
                playButton.onClick.AddListener(OnPlayButtonClicked);
            }

            if (removeButton != null)
            {
                removeButton.onClick.RemoveAllListeners();
                removeButton.onClick.AddListener(OnRemoveButtonClicked);
            }
        }

        public void Initialize(PlaylistTrack track, AddButtonController controller)
        {
            trackData = track;
            playlistController = controller;

            ClimbTunes.ModLogger.LogInfo($"PlaylistItemController: Initializing with track: {track.title}");
            ClimbTunes.ModLogger.LogInfo($"PlaylistItemController: trackText is null: {trackText == null}");

            if (trackText != null)
            {
                trackText.text = track.title;
                ClimbTunes.ModLogger.LogInfo($"PlaylistItemController: Set track text to: {track.title}");
            }
            else
            {
                ClimbTunes.ModLogger.LogWarning("PlaylistItemController: trackText is null, cannot set title");
            }

            UpdateVisualState();
        }

        private void OnPlayButtonClicked()
        {
            if (trackData != null && playlistController != null)
            {
                playlistController.PlayTrack(trackData);
                SetAsCurrentTrack();
            }
        }

        private void OnRemoveButtonClicked()
        {
            if (trackData != null && playlistController != null)
            {
                playlistController.RemoveTrack(trackData);
                DestroyItem();
            }
        }

        public void SetAsCurrentTrack()
        {
            // Find all other playlist items and set them as not current
            PlaylistItemController[] allItems = FindObjectsOfType<PlaylistItemController>();
            foreach (PlaylistItemController item in allItems)
            {
                if (item != this)
                {
                    item.SetCurrentTrack(false);
                }
            }

            SetCurrentTrack(true);
        }

        public void SetCurrentTrack(bool isCurrent)
        {
            isCurrentTrack = isCurrent;
            UpdateVisualState();
        }

        private void UpdateVisualState()
        {
            if (backgroundImage != null)
            {
                if (isCurrentTrack)
                {
                    backgroundImage.color = playingColor;
                }
                else
                {
                    backgroundImage.color = normalColor;
                }
            }

            // Update button states
            if (playButton != null)
            {
                playButton.interactable = !isCurrentTrack;
            }
        }

        private void DestroyItem()
        {
            // Animate out or immediately destroy
            Destroy(gameObject);
        }

        public void OnHover()
        {
            if (!isCurrentTrack && backgroundImage != null)
            {
                backgroundImage.color = selectedColor;
            }
        }

        public void OnHoverExit()
        {
            if (!isCurrentTrack)
            {
                UpdateVisualState();
            }
        }

        private void OnDestroy()
        {
            if (playButton != null)
            {
                playButton.onClick.RemoveListener(OnPlayButtonClicked);
            }

            if (removeButton != null)
            {
                removeButton.onClick.RemoveListener(OnRemoveButtonClicked);
            }
        }
    }
}