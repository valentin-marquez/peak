using UnityEngine;
using UnityEngine.UI;

namespace ClimbTunes.Components
{
    public class PlayPauseController : MonoBehaviour
    {
        [Header("UI References")]
        public Button playPauseButton;
        public GameObject playIcon;
        public GameObject pauseIcon;

        [Header("Audio")]
        public AudioSource audioSource;

        private bool isPlaying = false;

        private void Start()
        {
            ClimbTunes.ModLogger.LogInfo("PlayPauseController: Initializing...");
            
            if (playPauseButton != null)
            {
                playPauseButton.onClick.AddListener(TogglePlayPause);
                ClimbTunes.ModLogger.LogInfo("PlayPauseController: Button listener added successfully");
            }
            else
            {
                ClimbTunes.ModLogger.LogWarning("PlayPauseController: playPauseButton is null!");
            }

            UpdateButtonVisual();
            ClimbTunes.ModLogger.LogInfo("PlayPauseController: Initialization complete");
        }

        private void Update()
        {
            // Sync visual state with audio source
            if (audioSource != null)
            {
                bool audioIsPlaying = audioSource.isPlaying;
                if (audioIsPlaying != isPlaying)
                {
                    isPlaying = audioIsPlaying;
                    UpdateButtonVisual();
                }
            }
        }

        public void TogglePlayPause()
        {
            ClimbTunes.ModLogger.LogInfo($"PlayPauseController: TogglePlayPause called - Current state: {(isPlaying ? "Playing" : "Stopped")}");
            
            if (audioSource != null)
            {
                if (isPlaying)
                {
                    ClimbTunes.ModLogger.LogInfo("PlayPauseController: Calling Pause()");
                    Pause();
                }
                else
                {
                    ClimbTunes.ModLogger.LogInfo("PlayPauseController: Calling Play()");
                    Play();
                }
            }
            else
            {
                ClimbTunes.ModLogger.LogWarning("PlayPauseController: audioSource is null, cannot toggle playback");
            }
        }

        public void Play()
        {
            ClimbTunes.ModLogger.LogInfo("PlayPauseController: Play() called");
            
            if (audioSource != null && !audioSource.isPlaying)
            {
                audioSource.Play();
                isPlaying = true;
                UpdateButtonVisual();
                ClimbTunes.ModLogger.LogInfo("PlayPauseController: Audio playback started successfully");
            }
            else if (audioSource == null)
            {
                ClimbTunes.ModLogger.LogWarning("PlayPauseController: Cannot play - audioSource is null");
            }
            else if (audioSource.isPlaying)
            {
                ClimbTunes.ModLogger.LogInfo("PlayPauseController: Audio is already playing");
            }
        }

        public void Pause()
        {
            ClimbTunes.ModLogger.LogInfo("PlayPauseController: Pause() called");
            
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Pause();
                isPlaying = false;
                UpdateButtonVisual();
                ClimbTunes.ModLogger.LogInfo("PlayPauseController: Audio playback paused successfully");
            }
            else if (audioSource == null)
            {
                ClimbTunes.ModLogger.LogWarning("PlayPauseController: Cannot pause - audioSource is null");
            }
            else if (!audioSource.isPlaying)
            {
                ClimbTunes.ModLogger.LogInfo("PlayPauseController: Audio is not playing, nothing to pause");
            }
        }

        public void Stop()
        {
            if (audioSource != null)
            {
                audioSource.Stop();
                isPlaying = false;
                UpdateButtonVisual();
            }
        }

        private void UpdateButtonVisual()
        {
            if (playIcon != null && pauseIcon != null)
            {
                playIcon.SetActive(!isPlaying);
                pauseIcon.SetActive(isPlaying);
            }
        }

        private void OnDestroy()
        {
            if (playPauseButton != null)
            {
                playPauseButton.onClick.RemoveListener(TogglePlayPause);
            }
        }
    }
}