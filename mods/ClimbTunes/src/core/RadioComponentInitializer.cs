using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ClimbTunes.Components;
using ClimbTunes.Utils;

namespace ClimbTunes.Core
{
    /// <summary>
    /// Componente que se agrega automáticamente al GameObject principal de la radio
    /// para inicializar todos los componentes necesarios
    /// </summary>
    public class RadioComponentInitializer : MonoBehaviour
    {
        [Header("Auto-Initialize Settings")]
        public bool autoInitializeOnAwake = true;
        
        private bool isInitialized = false;

        private void Awake()
        {
            if (autoInitializeOnAwake)
            {
                InitializeComponents();
            }
        }

        private void Start()
        {
            // Inicializar en Start si no se hizo en Awake
            if (!isInitialized)
            {
                InitializeComponents();
            }
        }

        public void InitializeComponents()
        {
            if (isInitialized)
            {
                ClimbTunes.ModLogger.LogWarning("Radio components already initialized");
                return;
            }

            try
            {
                ClimbTunes.ModLogger.LogInfo("Initializing radio components...");

                // 1. Inicializar RadioController en el GameObject principal
                InitializeRadioController();

                // 2. Inicializar componentes de UI
                InitializeUIComponents();

                // 3. Inicializar componentes de botones
                InitializeButtonComponents();

                // 4. Configurar AudioSource para proximidad
                ConfigureProximityAudio();

                // 5. Configurar sistema de playlist items
                SetupPlaylistItemSystem();

                isInitialized = true;
                ClimbTunes.ModLogger.LogInfo("Radio components initialized successfully");
                
                // Run debug validation
                RadioDebugUtils.ValidateRadioPrefabStructure(gameObject);
                RadioDebugUtils.ValidateRadioComponents(gameObject);
            }
            catch (System.Exception ex)
            {
                ClimbTunes.ModLogger.LogError($"Error initializing radio components: {ex.Message}");
            }
        }

        private void InitializeRadioController()
        {
            // Agregar RadioController al GameObject principal si no existe
            RadioController radioController = GetComponent<RadioController>();
            if (radioController == null)
            {
                radioController = gameObject.AddComponent<RadioController>();
                ClimbTunes.ModLogger.LogInfo("RadioController added to radio");
            }

            // Configurar referencias del RadioController
            SetupRadioControllerReferences(radioController);
        }

        private void SetupRadioControllerReferences(RadioController radioController)
        {
            // Buscar y asignar AudioSource
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            radioController.audioSource = audioSource;

            // Buscar y asignar elementos de UI
            Transform canvas = transform.Find("Canvas");
            if (canvas != null)
            {
                // Status Text
                Transform statusText = canvas.Find("RadioPanel/ControlsPanel/Status/StatusText");
                if (statusText != null)
                {
                    radioController.statusText = statusText.GetComponent<TextMeshProUGUI>();
                }

                // Volume Slider
                Transform volumeSlider = canvas.Find("RadioPanel/ControlsPanel/VolumeGroup/VolumeSlider");
                if (volumeSlider != null)
                {
                    radioController.volumeSlider = volumeSlider.GetComponent<Slider>();
                }

                // Play/Pause Controller
                Transform playPauseButton = canvas.Find("RadioPanel/ControlsPanel/ButtonsGroup/PlayPauseButton");
                if (playPauseButton != null)
                {
                    PlayPauseController playPauseController = playPauseButton.GetComponent<PlayPauseController>();
                    if (playPauseController == null)
                    {
                        playPauseController = playPauseButton.gameObject.AddComponent<PlayPauseController>();
                        ClimbTunes.ModLogger.LogInfo("PlayPauseController added to PlayPauseButton");
                    }
                    
                    // Setup PlayPauseController references
                    SetupPlayPauseControllerReferences(playPauseController, playPauseButton, audioSource);
                    radioController.playPauseController = playPauseController;
                }
                else
                {
                    ClimbTunes.ModLogger.LogWarning("PlayPauseButton not found at expected path");
                }

                // Add Button Controller - La ruta correcta según la estructura
                Transform addButton = canvas.Find("RadioPanel/InputPanel/AddButton");
                if (addButton != null)
                {
                    AddButtonController addButtonController = addButton.GetComponent<AddButtonController>();
                    if (addButtonController == null)
                    {
                        addButtonController = addButton.gameObject.AddComponent<AddButtonController>();
                        ClimbTunes.ModLogger.LogInfo("AddButtonController added to AddButton");
                    }
                    radioController.addButtonController = addButtonController;
                    ClimbTunes.ModLogger.LogInfo("AddButtonController assigned to RadioController");
                }
                else
                {
                    ClimbTunes.ModLogger.LogWarning("AddButton not found at expected path");
                }
            }

            ClimbTunes.ModLogger.LogInfo("RadioController references configured");
        }

        private void InitializeUIComponents()
        {
            Transform canvas = transform.Find("Canvas");
            if (canvas == null)
            {
                ClimbTunes.ModLogger.LogWarning("Canvas not found on radio prefab");
                return;
            }

            // Asegurar que RadioUIManager esté en el Canvas
            RadioUIManager uiManager = canvas.GetComponent<RadioUIManager>();
            if (uiManager == null)
            {
                uiManager = canvas.gameObject.AddComponent<RadioUIManager>();
                ClimbTunes.ModLogger.LogInfo("RadioUIManager added to Canvas");
            }

            // Configurar NavigationController si existe
            Transform radioPanel = canvas.Find("RadioPanel");
            if (radioPanel != null)
            {
                NavigationController navController = radioPanel.GetComponent<NavigationController>();
                if (navController == null)
                {
                    navController = radioPanel.gameObject.AddComponent<NavigationController>();
                    ClimbTunes.ModLogger.LogInfo("NavigationController added to RadioPanel");
                }
                
                // Configure navigation controller button references
                SetupNavigationControllerReferences(navController, canvas);
            }
        }

        private void SetupPlayPauseControllerReferences(PlayPauseController playPauseController, Transform playPauseButton, AudioSource audioSource)
        {
            ClimbTunes.ModLogger.LogInfo("Setting up PlayPauseController references...");
            
            // Check if PlayPauseButton itself has a Button component, if not add one
            Button button = playPauseButton.GetComponent<Button>();
            if (button == null)
            {
                ClimbTunes.ModLogger.LogInfo("PlayPauseController: Adding Button component to PlayPauseButton");
                button = playPauseButton.gameObject.AddComponent<Button>();
            }
            
            if (button != null)
            {
                playPauseController.playPauseButton = button;
                ClimbTunes.ModLogger.LogInfo("PlayPauseController: Button assigned");
            }

            // Find and assign Play icon
            Transform playIcon = playPauseButton.Find("Playicon");
            if (playIcon != null)
            {
                playPauseController.playIcon = playIcon.gameObject;
                ClimbTunes.ModLogger.LogInfo("PlayPauseController: Play icon assigned");
            }
            else
            {
                ClimbTunes.ModLogger.LogWarning("PlayPauseController: Play icon not found (expected 'Playicon')");
            }

            // Find and assign Pause icon
            Transform pauseIcon = playPauseButton.Find("PauseIcon");
            if (pauseIcon != null)
            {
                playPauseController.pauseIcon = pauseIcon.gameObject;
                ClimbTunes.ModLogger.LogInfo("PlayPauseController: Pause icon assigned");
            }
            else
            {
                ClimbTunes.ModLogger.LogWarning("PlayPauseController: Pause icon not found (expected 'PauseIcon')");
            }

            // Assign AudioSource
            if (audioSource != null)
            {
                playPauseController.audioSource = audioSource;
                ClimbTunes.ModLogger.LogInfo("PlayPauseController: AudioSource assigned");
            }
            else
            {
                ClimbTunes.ModLogger.LogWarning("PlayPauseController: AudioSource is null");
            }
        }

        private void SetupNavigationControllerReferences(NavigationController navController, Transform canvas)
        {
            ClimbTunes.ModLogger.LogInfo("Setting up NavigationController button references...");
            
            // Find and assign Previous Button
            Transform prevButton = canvas.Find("RadioPanel/ControlsPanel/ButtonsGroup/PreviousButton");
            if (prevButton != null)
            {
                Button prevBtn = prevButton.GetComponent<Button>();
                if (prevBtn != null)
                {
                    navController.previousButton = prevBtn;
                    ClimbTunes.ModLogger.LogInfo("NavigationController: Previous button assigned");
                }
                else
                {
                    ClimbTunes.ModLogger.LogWarning("NavigationController: Previous button found but no Button component");
                }
            }
            else
            {
                ClimbTunes.ModLogger.LogWarning("NavigationController: Previous button not found at expected path");
            }

            // Find and assign Next Button
            Transform nextButton = canvas.Find("RadioPanel/ControlsPanel/ButtonsGroup/NextButton");
            if (nextButton != null)
            {
                Button nextBtn = nextButton.GetComponent<Button>();
                if (nextBtn != null)
                {
                    navController.nextButton = nextBtn;
                    ClimbTunes.ModLogger.LogInfo("NavigationController: Next button assigned");
                }
                else
                {
                    ClimbTunes.ModLogger.LogWarning("NavigationController: Next button found but no Button component");
                }
            }
            else
            {
                ClimbTunes.ModLogger.LogWarning("NavigationController: Next button not found at expected path");
            }

            // Assign RadioController reference
            RadioController radioController = GetComponent<RadioController>();
            if (radioController != null)
            {
                navController.radioController = radioController;
                ClimbTunes.ModLogger.LogInfo("NavigationController: RadioController reference assigned");
            }
            else
            {
                ClimbTunes.ModLogger.LogWarning("NavigationController: RadioController not found");
            }
        }

        private void InitializeButtonComponents()
        {
            Transform canvas = transform.Find("Canvas");
            if (canvas == null) return;

            // Quit Button Controller
            Transform quitButton = canvas.Find("RadioPanel/HeaderPanel/quit");
            if (quitButton != null)
            {
                QuitButtonController quitController = quitButton.GetComponent<QuitButtonController>();
                if (quitController == null)
                {
                    quitController = quitButton.gameObject.AddComponent<QuitButtonController>();
                    ClimbTunes.ModLogger.LogInfo("QuitButtonController added");
                }
            }

            // Previous Button
            Transform prevButton = canvas.Find("RadioPanel/ControlsPanel/ButtonsGroup/PreviousButton");
            if (prevButton != null)
            {
                Button button = prevButton.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => {
                        RadioController radioController = GetComponent<RadioController>();
                        if (radioController != null)
                        {
                            radioController.PreviousTrack();
                        }
                    });
                }
            }

            // Next Button
            Transform nextButton = canvas.Find("RadioPanel/ControlsPanel/ButtonsGroup/NextButton");
            if (nextButton != null)
            {
                Button button = nextButton.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => {
                        RadioController radioController = GetComponent<RadioController>();
                        if (radioController != null)
                        {
                            radioController.NextTrack();
                        }
                    });
                }
            }
        }

        private void ConfigureProximityAudio()
        {
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
            {
                // Configurar audio 3D para proximidad
                audioSource.spatialBlend = 1.0f; // Full 3D
                audioSource.rolloffMode = AudioRolloffMode.Linear;
                audioSource.minDistance = 5.0f;
                audioSource.maxDistance = 50.0f;
                audioSource.volume = 0.5f;
                audioSource.playOnAwake = false;

                ClimbTunes.ModLogger.LogInfo("Proximity audio configured");
            }
        }

        private void SetupPlaylistItemSystem()
        {
            Transform canvas = transform.Find("Canvas");
            if (canvas == null) return;

            // Buscar el Content area donde se crean los playlist items
            Transform content = canvas.Find("RadioPanel/PlaylistPanel/PlaylistScrollView/Viewport/Content");
            if (content != null)
            {
                // Monitorear cuando se añadan nuevos playlist items
                MonitorPlaylistItems(content);
                ClimbTunes.ModLogger.LogInfo("Playlist item system configured");
            }
        }

        private void MonitorPlaylistItems(Transform content)
        {
            // Agregar un componente que monitoree cambios en los hijos
            PlaylistMonitor monitor = content.GetComponent<PlaylistMonitor>();
            if (monitor == null)
            {
                monitor = content.gameObject.AddComponent<PlaylistMonitor>();
            }
        }

        /// <summary>
        /// Método público para re-inicializar componentes si es necesario
        /// </summary>
        public void RefreshComponents()
        {
            isInitialized = false;
            InitializeComponents();
        }

        private void OnValidate()
        {
            // Validar en editor para debug
            if (Application.isPlaying && isInitialized)
            {
                ClimbTunes.ModLogger.LogInfo("Radio component validation passed");
            }
        }
    }
}