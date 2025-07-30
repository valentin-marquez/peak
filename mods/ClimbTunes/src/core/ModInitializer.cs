using BepInEx;
using UnityEngine;
using UnityEngine.SceneManagement;
using ClimbTunes.Manager;
using ClimbTunes.Core;
using HarmonyLib;
using UnityScene = UnityEngine.SceneManagement.Scene;
using System.IO;
using ClimbTunes.Utils;
using System.Threading.Tasks;

namespace ClimbTunes.Core
{
    public class ModInitializer
    {
        private readonly BaseUnityPlugin plugin;
        private AssetManager assetManager;
        private Manager.SceneManager sceneManager;
        private SpawnerManager spawnerManager;
        private Harmony harmony;
        private ToolDownloadService toolDownloadService;

        public ModInitializer(BaseUnityPlugin plugin)
        {
            this.plugin = plugin;
        }

        public void Initialize()
        {
            try
            {
                // Fase 1: Crear directorios necesarios al inicio
                CreateRequiredDirectories();

                // Fase 2: Verificar y descargar herramientas necesarias
                _ = InitializeToolsAsync();

                // Inicializar Harmony para patches
                harmony = new Harmony("com.nozz.climbtunes");
                harmony.PatchAll();
                ClimbTunes.ModLogger.LogInfo("Harmony patches applied");

                // Inicializar managers
                assetManager = new AssetManager(plugin);
                spawnerManager = new SpawnerManager(plugin, assetManager);
                sceneManager = new Manager.SceneManager(plugin, assetManager, spawnerManager);

                // Cargar AssetBundle
                assetManager.LoadAssetBundle();

                // Suscribirse a eventos de cambio de escena
                UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;

                // Crear sistema de fallback para inicialización de radios
                RadioFallbackInitializer.CreateFallbackInitializer();

                ClimbTunes.ModLogger.LogInfo("Initialization complete");
            }
            catch (System.Exception ex)
            {
                ClimbTunes.ModLogger.LogError($"Error during initialization: {ex.Message}");
            }
        }

        private void OnSceneLoaded(UnityScene scene, LoadSceneMode mode)
        {
            sceneManager.HandleSceneLoaded(scene);
        }

        private void CreateRequiredDirectories()
        {
            try
            {
                string modDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                
                // Crear directorio lib para herramientas
                string libDirectory = Path.Combine(modDirectory, "lib");
                if (!Directory.Exists(libDirectory))
                {
                    Directory.CreateDirectory(libDirectory);
                    ClimbTunes.ModLogger.LogInfo($"Created lib directory: {libDirectory}");
                }
                
                // Crear directorio audio_cache para cache de audio
                string audioCacheDirectory = Path.Combine(modDirectory, "audio_cache");
                if (!Directory.Exists(audioCacheDirectory))
                {
                    Directory.CreateDirectory(audioCacheDirectory);
                    ClimbTunes.ModLogger.LogInfo($"Created audio_cache directory: {audioCacheDirectory}");
                }
                
                ClimbTunes.ModLogger.LogInfo("Required directories created successfully");
            }
            catch (System.Exception ex)
            {
                ClimbTunes.ModLogger.LogError($"Failed to create required directories: {ex.Message}");
            }
        }

        private async Task InitializeToolsAsync()
        {
            try
            {
                ClimbTunes.ModLogger.LogInfo("Starting tools initialization...");
                
                // Inicializar servicio de descarga de herramientas
                toolDownloadService = new ToolDownloadService();
                
                // Verificar presencia de yt-dlp y ffmpeg
                bool ytDlpExists = toolDownloadService.IsYtDlpAvailable();
                bool ffmpegExists = toolDownloadService.IsFfmpegAvailable();
                
                ClimbTunes.ModLogger.LogInfo($"yt-dlp available: {ytDlpExists}");
                ClimbTunes.ModLogger.LogInfo($"ffmpeg available: {ffmpegExists}");
                
                // Descargar herramientas si no están presentes
                if (!ytDlpExists || !ffmpegExists)
                {
                    ClimbTunes.ModLogger.LogInfo("Downloading missing tools...");
                    bool toolsReady = await toolDownloadService.EnsureToolsAvailable();
                    
                    if (toolsReady)
                    {
                        ClimbTunes.ModLogger.LogInfo("All tools are now available and ready");
                    }
                    else
                    {
                        ClimbTunes.ModLogger.LogError("Failed to download required tools. Some features may not work.");
                    }
                }
                else
                {
                    ClimbTunes.ModLogger.LogInfo("All tools already available");
                }
                
                // Configurar entorno de trabajo y marcar herramientas como inicializadas
                YouTubeService.MarkToolsAsInitialized();
                ClimbTunes.ModLogger.LogInfo("Work environment configured successfully");
            }
            catch (System.Exception ex)
            {
                ClimbTunes.ModLogger.LogError($"Error during tools initialization: {ex.Message}");
            }
        }

        public void Cleanup()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
            assetManager?.Cleanup();
            toolDownloadService?.Dispose();
            
            // Limpiar patches de Harmony
            harmony?.UnpatchSelf();
            ClimbTunes.ModLogger.LogInfo("Cleanup complete");
        }
    }
}