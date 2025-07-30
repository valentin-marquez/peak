using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ClimbTunes.Components;

namespace ClimbTunes.Utils
{
    public static class RadioDebugUtils
    {
        public static void ValidateRadioPrefabStructure(GameObject radioObject)
        {
            ClimbTunes.ModLogger.LogInfo("=== RADIO PREFAB STRUCTURE VALIDATION ===");
            
            if (radioObject == null)
            {
                ClimbTunes.ModLogger.LogError("RadioDebugUtils: Radio object is null!");
                return;
            }

            ClimbTunes.ModLogger.LogInfo($"Radio object name: {radioObject.name}");
            
            // Validate main Canvas
            Transform canvas = radioObject.transform.Find("Canvas");
            if (canvas == null)
            {
                ClimbTunes.ModLogger.LogError("RadioDebugUtils: Canvas not found!");
                return;
            }
            ClimbTunes.ModLogger.LogInfo("✓ Canvas found");

            // Validate RadioPanel
            Transform radioPanel = canvas.Find("RadioPanel");
            if (radioPanel == null)
            {
                ClimbTunes.ModLogger.LogError("RadioDebugUtils: RadioPanel not found!");
                return;
            }
            ClimbTunes.ModLogger.LogInfo("✓ RadioPanel found");

            // Validate ControlsPanel
            Transform controlsPanel = radioPanel.Find("ControlsPanel");
            if (controlsPanel == null)
            {
                ClimbTunes.ModLogger.LogError("RadioDebugUtils: ControlsPanel not found!");
                return;
            }
            ClimbTunes.ModLogger.LogInfo("✓ ControlsPanel found");

            // Validate ButtonsGroup
            Transform buttonsGroup = controlsPanel.Find("ButtonsGroup");
            if (buttonsGroup == null)
            {
                ClimbTunes.ModLogger.LogError("RadioDebugUtils: ButtonsGroup not found!");
                return;
            }
            ClimbTunes.ModLogger.LogInfo("✓ ButtonsGroup found");

            // Validate Previous Button
            Transform previousButton = buttonsGroup.Find("PreviousButton");
            if (previousButton == null)
            {
                ClimbTunes.ModLogger.LogError("RadioDebugUtils: PreviousButton not found!");
            }
            else
            {
                Button prevBtn = previousButton.GetComponent<Button>();
                ClimbTunes.ModLogger.LogInfo($"✓ PreviousButton found - Button component: {prevBtn != null}");
                if (prevBtn != null)
                {
                    ClimbTunes.ModLogger.LogInfo($"  - Interactable: {prevBtn.interactable}");
                    ClimbTunes.ModLogger.LogInfo($"  - Listeners count: {prevBtn.onClick.GetPersistentEventCount()}");
                }
            }

            // Validate Play/Pause Button
            Transform playPauseButton = buttonsGroup.Find("PlayPauseButton");
            if (playPauseButton == null)
            {
                ClimbTunes.ModLogger.LogError("RadioDebugUtils: PlayPauseButton not found!");
            }
            else
            {
                Button ppBtn = playPauseButton.GetComponent<Button>();
                ClimbTunes.ModLogger.LogInfo($"✓ PlayPauseButton found - Button component: {ppBtn != null}");
                if (ppBtn != null)
                {
                    ClimbTunes.ModLogger.LogInfo($"  - Interactable: {ppBtn.interactable}");
                    ClimbTunes.ModLogger.LogInfo($"  - Listeners count: {ppBtn.onClick.GetPersistentEventCount()}");
                }

                // Check for play/pause icons
                Transform playIcon = playPauseButton.Find("Playicon");
                Transform pauseIcon = playPauseButton.Find("PauseIcon");
                ClimbTunes.ModLogger.LogInfo($"  - Play icon found: {playIcon != null}");
                ClimbTunes.ModLogger.LogInfo($"  - Pause icon found: {pauseIcon != null}");
            }

            // Validate Next Button
            Transform nextButton = buttonsGroup.Find("NextButton");
            if (nextButton == null)
            {
                ClimbTunes.ModLogger.LogError("RadioDebugUtils: NextButton not found!");
            }
            else
            {
                Button nextBtn = nextButton.GetComponent<Button>();
                ClimbTunes.ModLogger.LogInfo($"✓ NextButton found - Button component: {nextBtn != null}");
                if (nextBtn != null)
                {
                    ClimbTunes.ModLogger.LogInfo($"  - Interactable: {nextBtn.interactable}");
                    ClimbTunes.ModLogger.LogInfo($"  - Listeners count: {nextBtn.onClick.GetPersistentEventCount()}");
                }
            }

            // Validate Volume Slider
            Transform volumeGroup = controlsPanel.Find("VolumeGroup");
            if (volumeGroup != null)
            {
                Transform volumeSlider = volumeGroup.Find("VolumeSlider");
                if (volumeSlider != null)
                {
                    Slider slider = volumeSlider.GetComponent<Slider>();
                    ClimbTunes.ModLogger.LogInfo($"✓ VolumeSlider found - Slider component: {slider != null}");
                    if (slider != null)
                    {
                        ClimbTunes.ModLogger.LogInfo($"  - Min value: {slider.minValue}, Max value: {slider.maxValue}");
                        ClimbTunes.ModLogger.LogInfo($"  - Current value: {slider.value}");
                        ClimbTunes.ModLogger.LogInfo($"  - Interactable: {slider.interactable}");
                    }
                }
                else
                {
                    ClimbTunes.ModLogger.LogError("RadioDebugUtils: VolumeSlider not found!");
                }
            }
            else
            {
                ClimbTunes.ModLogger.LogError("RadioDebugUtils: VolumeGroup not found!");
            }

            // Validate Status Text
            Transform status = controlsPanel.Find("Status");
            if (status != null)
            {
                Transform statusText = status.Find("StatusText");
                if (statusText != null)
                {
                    TextMeshProUGUI text = statusText.GetComponent<TextMeshProUGUI>();
                    ClimbTunes.ModLogger.LogInfo($"✓ StatusText found - TextMeshProUGUI component: {text != null}");
                    if (text != null)
                    {
                        ClimbTunes.ModLogger.LogInfo($"  - Current text: '{text.text}'");
                    }
                }
                else
                {
                    ClimbTunes.ModLogger.LogError("RadioDebugUtils: StatusText not found!");
                }
            }
            else
            {
                ClimbTunes.ModLogger.LogError("RadioDebugUtils: Status not found!");
            }

            ClimbTunes.ModLogger.LogInfo("=== END RADIO PREFAB STRUCTURE VALIDATION ===");
        }

        public static void ValidateRadioComponents(GameObject radioObject)
        {
            ClimbTunes.ModLogger.LogInfo("=== RADIO COMPONENTS VALIDATION ===");
            
            if (radioObject == null)
            {
                ClimbTunes.ModLogger.LogError("RadioDebugUtils: Radio object is null!");
                return;
            }

            // Check RadioController
            RadioController radioController = radioObject.GetComponent<RadioController>();
            ClimbTunes.ModLogger.LogInfo($"RadioController found: {radioController != null}");
            if (radioController != null)
            {
                ClimbTunes.ModLogger.LogInfo($"  - AudioSource assigned: {radioController.audioSource != null}");
                ClimbTunes.ModLogger.LogInfo($"  - StatusText assigned: {radioController.statusText != null}");
                ClimbTunes.ModLogger.LogInfo($"  - VolumeSlider assigned: {radioController.volumeSlider != null}");
                ClimbTunes.ModLogger.LogInfo($"  - PlayPauseController assigned: {radioController.playPauseController != null}");
                ClimbTunes.ModLogger.LogInfo($"  - AddButtonController assigned: {radioController.addButtonController != null}");
            }

            // Check PlayPauseController - buscar en ubicaciones específicas
            Transform canvas = radioObject.transform.Find("Canvas");
            PlayPauseController playPauseController = null;
            if (canvas != null)
            {
                Transform playPauseButton = canvas.Find("RadioPanel/ControlsPanel/ButtonsGroup/PlayPauseButton");
                if (playPauseButton != null)
                {
                    playPauseController = playPauseButton.GetComponent<PlayPauseController>();
                }
            }
            ClimbTunes.ModLogger.LogInfo($"PlayPauseController found: {playPauseController != null}");
            if (playPauseController != null)
            {
                ClimbTunes.ModLogger.LogInfo($"  - Button assigned: {playPauseController.playPauseButton != null}");
                ClimbTunes.ModLogger.LogInfo($"  - Play icon assigned: {playPauseController.playIcon != null}");
                ClimbTunes.ModLogger.LogInfo($"  - Pause icon assigned: {playPauseController.pauseIcon != null}");
                ClimbTunes.ModLogger.LogInfo($"  - AudioSource assigned: {playPauseController.audioSource != null}");
            }

            // Check NavigationController - buscar en ubicaciones específicas
            NavigationController navigationController = null;
            if (canvas != null)
            {
                Transform radioPanel = canvas.Find("RadioPanel");
                if (radioPanel != null)
                {
                    navigationController = radioPanel.GetComponent<NavigationController>();
                }
            }
            ClimbTunes.ModLogger.LogInfo($"NavigationController found: {navigationController != null}");
            if (navigationController != null)
            {
                ClimbTunes.ModLogger.LogInfo($"  - Previous button assigned: {navigationController.previousButton != null}");
                ClimbTunes.ModLogger.LogInfo($"  - Next button assigned: {navigationController.nextButton != null}");
                ClimbTunes.ModLogger.LogInfo($"  - RadioController assigned: {navigationController.radioController != null}");
            }

            // Check AudioSource
            AudioSource audioSource = radioObject.GetComponent<AudioSource>();
            ClimbTunes.ModLogger.LogInfo($"AudioSource found: {audioSource != null}");
            if (audioSource != null)
            {
                ClimbTunes.ModLogger.LogInfo($"  - Volume: {audioSource.volume}");
                ClimbTunes.ModLogger.LogInfo($"  - PlayOnAwake: {audioSource.playOnAwake}");
                ClimbTunes.ModLogger.LogInfo($"  - Clip assigned: {audioSource.clip != null}");
                ClimbTunes.ModLogger.LogInfo($"  - Is playing: {audioSource.isPlaying}");
            }

            // Check AddButtonController
            AddButtonController addButtonController = null;
            if (canvas != null)
            {
                Transform addButton = canvas.Find("RadioPanel/InputPanel/AddButton");
                if (addButton != null)
                {
                    addButtonController = addButton.GetComponent<AddButtonController>();
                }
            }
            ClimbTunes.ModLogger.LogInfo($"AddButtonController found: {addButtonController != null}");
            if (addButtonController != null)
            {
                ClimbTunes.ModLogger.LogInfo($"  - Add button assigned: {addButtonController.addButton != null}");
                ClimbTunes.ModLogger.LogInfo($"  - URL input field assigned: {addButtonController.urlInputField != null}");
                ClimbTunes.ModLogger.LogInfo($"  - Content parent assigned: {addButtonController.contentParent != null}");
                ClimbTunes.ModLogger.LogInfo($"  - Empty state text assigned: {addButtonController.emptyStateText != null}");
                ClimbTunes.ModLogger.LogInfo($"  - Playlist tracks count: {addButtonController.playlist.Count}");
            }

            ClimbTunes.ModLogger.LogInfo("=== END RADIO COMPONENTS VALIDATION ===");
        }

        public static void TestButtonFunctionality(GameObject radioObject)
        {
            ClimbTunes.ModLogger.LogInfo("=== TESTING BUTTON FUNCTIONALITY ===");
            
            if (radioObject == null)
            {
                ClimbTunes.ModLogger.LogError("RadioDebugUtils: Radio object is null!");
                return;
            }

            Transform canvas = radioObject.transform.Find("Canvas");
            if (canvas == null)
            {
                ClimbTunes.ModLogger.LogError("RadioDebugUtils: Canvas not found!");
                return;
            }

            // Test NavigationController
            Transform radioPanel = canvas.Find("RadioPanel");
            if (radioPanel != null)
            {
                NavigationController navController = radioPanel.GetComponent<NavigationController>();
                if (navController != null)
                {
                    ClimbTunes.ModLogger.LogInfo("Testing navigation buttons...");
                    
                    if (navController.previousButton != null)
                    {
                        ClimbTunes.ModLogger.LogInfo("Simulating Previous button click...");
                        navController.previousButton.onClick.Invoke();
                    }
                    else
                    {
                        ClimbTunes.ModLogger.LogWarning("Previous button is null in NavigationController");
                    }
                    
                    if (navController.nextButton != null)
                    {
                        ClimbTunes.ModLogger.LogInfo("Simulating Next button click...");
                        navController.nextButton.onClick.Invoke();
                    }
                    else
                    {
                        ClimbTunes.ModLogger.LogWarning("Next button is null in NavigationController");
                    }
                }
                else
                {
                    ClimbTunes.ModLogger.LogWarning("NavigationController not found on RadioPanel");
                }
            }

            // Test PlayPauseController
            Transform playPauseButton = canvas.Find("RadioPanel/ControlsPanel/ButtonsGroup/PlayPauseButton");
            if (playPauseButton != null)
            {
                PlayPauseController ppController = playPauseButton.GetComponent<PlayPauseController>();
                if (ppController != null)
                {
                    ClimbTunes.ModLogger.LogInfo("Testing play/pause button...");
                    
                    if (ppController.playPauseButton != null)
                    {
                        ClimbTunes.ModLogger.LogInfo("Simulating Play/Pause button click...");
                        ppController.playPauseButton.onClick.Invoke();
                    }
                    else
                    {
                        ClimbTunes.ModLogger.LogWarning("PlayPause button is null in PlayPauseController");
                    }
                }
                else
                {
                    ClimbTunes.ModLogger.LogWarning("PlayPauseController not found on PlayPauseButton");
                }
            }

            // Test AddButtonController
            Transform addButton = canvas.Find("RadioPanel/InputPanel/AddButton");
            if (addButton != null)
            {
                AddButtonController addController = addButton.GetComponent<AddButtonController>();
                if (addController != null)
                {
                    ClimbTunes.ModLogger.LogInfo("Testing add button...");
                    
                    if (addController.addButton != null)
                    {
                        ClimbTunes.ModLogger.LogInfo("Simulating Add button click (with empty URL)...");
                        addController.addButton.onClick.Invoke();
                    }
                    else
                    {
                        ClimbTunes.ModLogger.LogWarning("Add button is null in AddButtonController");
                    }
                }
                else
                {
                    ClimbTunes.ModLogger.LogWarning("AddButtonController not found on AddButton");
                }
            }

            ClimbTunes.ModLogger.LogInfo("=== END BUTTON FUNCTIONALITY TEST ===");
        }
    }
}