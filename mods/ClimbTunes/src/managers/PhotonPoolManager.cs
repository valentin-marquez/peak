using UnityEngine;
using Photon.Pun;
using BepInEx;
using System.Collections.Generic;

namespace ClimbTunes.Manager
{
    public class PhotonPoolManager
    {
        private static CustomPrefabPool customPool;
        private static bool isInitialized = false;

        public static void Initialize(BaseUnityPlugin plugin = null)
        {
            if (isInitialized) return;

            try
            {
                // Guardar el pool original y crear nuestro pool personalizado
                IPunPrefabPool originalPool = PhotonNetwork.PrefabPool;
                customPool = new CustomPrefabPool(originalPool, plugin);
                
                // Reemplazar el pool de Photon con nuestro pool personalizado
                PhotonNetwork.PrefabPool = customPool;
                
                isInitialized = true;
                ClimbTunes.ModLogger.LogInfo("Custom Photon PrefabPool initialized successfully");
            }
            catch (System.Exception ex)
            {
                ClimbTunes.ModLogger.LogError($"Error initializing custom prefab pool: {ex.Message}");
            }
        }

        public static void RegisterCustomPrefab(string prefabName, GameObject prefab)
        {
            if (!isInitialized)
            {
                ClimbTunes.ModLogger.LogError("PhotonPoolManager not initialized");
                return;
            }

            if (customPool == null)
            {
                ClimbTunes.ModLogger.LogError("Custom pool is null");
                return;
            }

            customPool.RegisterCustomPrefab(prefabName, prefab);
        }

        public static bool HasCustomPrefab(string prefabName)
        {
            return customPool?.HasCustomPrefab(prefabName) ?? false;
        }
    }
}