using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System;
using System.Collections;
using HDPeak.Config;
using HDPeak.Core;

namespace HDPeak
{
    [BepInPlugin("com.peak.hdpeak", "HDPeak", "2.0.0")]
    [BepInProcess("PEAK.exe")]
    public class HDPeakPlugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;
        private Harmony _harmony;
        private HDPeakCore _coreSystem;
        private GameObject _coreGameObject;
        private bool _isInitialized = false;

        private void Awake()
        {
            Logger = base.Logger;
            Logger.LogInfo("HDPeak v2.0.0 - Advanced Graphics Optimization");
            Logger.LogInfo("Loading HDPeak plugin...");

            try
            {
                // Initialize configuration
                HDPeakConfig.Initialize(Config);
                Logger.LogInfo("Configuration initialized");

                // Create core system game object
                _coreGameObject = new GameObject("HDPeakCore");
                DontDestroyOnLoad(_coreGameObject);

                // Add and initialize core component
                _coreSystem = _coreGameObject.AddComponent<HDPeakCore>();

                // Start initialization coroutine to wait for Unity systems
                StartCoroutine(InitializeAfterUnityReady());
            }
            catch (Exception ex)
            {
                Logger.LogError($"Critical error during HDPeak initialization: {ex.Message}");
                Logger.LogDebug($"Stack trace: {ex.StackTrace}");
            }
        }

        private IEnumerator InitializeAfterUnityReady()
        {
            // Wait a few frames for Unity's render pipeline to be ready
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            // Initialize core and apply optimizations with separate error handling
            bool initSuccess = false;
            try
            {
                _coreSystem.Initialize();
                initSuccess = true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to initialize HDPeak core: {ex.Message}");
                Logger.LogDebug($"Stack trace: {ex.StackTrace}");
            }

            if (!initSuccess)
                yield break;

            yield return StartCoroutine(ApplyInitialOptimizations());

            _isInitialized = true;
            Logger.LogInfo("HDPeak: Advanced graphics optimization system ready!");
            Logger.LogInfo("=== HDPeak Features ===");
            Logger.LogInfo("• Automatic hardware detection");
            Logger.LogInfo("• Dynamic resolution scaling");
            Logger.LogInfo("• Real-time performance monitoring");
            Logger.LogInfo("• URP-optimized rendering");
            Logger.LogInfo("• Modular optimization system");
            Logger.LogInfo("========================");
        }

        private IEnumerator ApplyInitialOptimizations()
        {
            if (_coreSystem == null || !_coreSystem.IsInitialized)
                yield break;

            Logger.LogInfo("Applying initial optimizations...");

            // Wait for all modules to be ready
            yield return new WaitForSeconds(1f);

            bool success = false;
            try
            {
                _coreSystem.ApplyOptimizations();
                success = true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to apply initial optimizations: {ex.Message}");
            }

            if (success)
            {
                Logger.LogInfo("Initial optimizations applied successfully");
            }
        }

        /// <summary>
        /// Get the core system instance
        /// </summary>
        public static HDPeakCore GetCore()
        {
            return FindObjectOfType<HDPeakCore>();
        }

        /// <summary>
        /// Check if HDPeak is properly initialized
        /// </summary>
        public static bool IsReady()
        {
            var core = GetCore();
            return core != null && core.IsInitialized;
        }

        private void OnDestroy()
        {
            try
            {
                if (_coreSystem != null)
                {
                    _coreSystem.ResetOptimizations();
                }

                if (_coreGameObject != null)
                {
                    Destroy(_coreGameObject);
                }

                Logger.LogInfo("HDPeak cleanup completed");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error during HDPeak cleanup: {ex.Message}");
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!_isInitialized || _coreSystem == null)
                return;

            try
            {
                if (hasFocus)
                {
                    // Re-apply optimizations when game regains focus
                    Logger.LogDebug("Game regained focus, re-applying optimizations");
                    _coreSystem.ApplyOptimizations();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error handling application focus change: {ex.Message}");
            }
        }
    }
}