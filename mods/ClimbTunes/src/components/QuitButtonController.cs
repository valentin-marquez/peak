using UnityEngine;
using UnityEngine.UI;
using ClimbTunes.Components;

namespace ClimbTunes.Components
{
    public class QuitButtonController : MonoBehaviour
    {
        [Header("UI References")]
        public Button quitButton;

        private RadioUIManager radioUIManager;

        private void Start()
        {
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitClicked);
            }

            // Find the RadioUIManager in parent objects
            radioUIManager = GetComponentInParent<RadioUIManager>();
        }

        private void OnQuitClicked()
        {
            if (radioUIManager != null)
            {
                radioUIManager.CloseUI();
            }
            else
            {
                // Fallback: disable the canvas
                Canvas canvas = GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    canvas.gameObject.SetActive(false);
                }
            }
        }

        private void OnDestroy()
        {
            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(OnQuitClicked);
            }
        }
    }
}