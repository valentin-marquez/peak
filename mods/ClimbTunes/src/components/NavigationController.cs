using UnityEngine;
using UnityEngine.UI;

namespace ClimbTunes.Components
{
    public class NavigationController : MonoBehaviour
    {
        [Header("UI References")]
        public Button previousButton;
        public Button nextButton;

        [Header("Radio Reference")]
        public RadioController radioController;

        private void Start()
        {
            ClimbTunes.ModLogger.LogInfo("NavigationController: Initializing...");
            
            if (previousButton != null)
            {
                previousButton.onClick.AddListener(OnPreviousClicked);
                ClimbTunes.ModLogger.LogInfo("NavigationController: Previous button listener added");
            }
            else
            {
                ClimbTunes.ModLogger.LogWarning("NavigationController: previousButton is null!");
            }

            if (nextButton != null)
            {
                nextButton.onClick.AddListener(OnNextClicked);
                ClimbTunes.ModLogger.LogInfo("NavigationController: Next button listener added");
            }
            else
            {
                ClimbTunes.ModLogger.LogWarning("NavigationController: nextButton is null!");
            }

            // Auto-find RadioController if not assigned
            if (radioController == null)
            {
                radioController = FindObjectOfType<RadioController>();
                if (radioController != null)
                {
                    ClimbTunes.ModLogger.LogInfo("NavigationController: RadioController found automatically");
                }
                else
                {
                    ClimbTunes.ModLogger.LogWarning("NavigationController: RadioController not found!");
                }
            }
            else
            {
                ClimbTunes.ModLogger.LogInfo("NavigationController: RadioController already assigned");
            }
            
            ClimbTunes.ModLogger.LogInfo("NavigationController: Initialization complete");
        }

        private void OnPreviousClicked()
        {
            ClimbTunes.ModLogger.LogInfo("NavigationController: Previous button clicked");
            
            if (radioController != null)
            {
                ClimbTunes.ModLogger.LogInfo("NavigationController: Calling PreviousTrack()");
                radioController.PreviousTrack();
            }
            else
            {
                ClimbTunes.ModLogger.LogWarning("NavigationController: Cannot go to previous track - RadioController is null");
            }
        }

        private void OnNextClicked()
        {
            ClimbTunes.ModLogger.LogInfo("NavigationController: Next button clicked");
            
            if (radioController != null)
            {
                ClimbTunes.ModLogger.LogInfo("NavigationController: Calling NextTrack()");
                radioController.NextTrack();
            }
            else
            {
                ClimbTunes.ModLogger.LogWarning("NavigationController: Cannot go to next track - RadioController is null");
            }
        }

        private void OnDestroy()
        {
            if (previousButton != null)
            {
                previousButton.onClick.RemoveListener(OnPreviousClicked);
            }

            if (nextButton != null)
            {
                nextButton.onClick.RemoveListener(OnNextClicked);
            }
        }
    }
}