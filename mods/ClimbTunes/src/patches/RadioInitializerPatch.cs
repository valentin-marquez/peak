using HarmonyLib;
using UnityEngine;
using ClimbTunes.Core;

namespace ClimbTunes.Patches
{
    [HarmonyPatch(typeof(GameObject), "SetActive")]
    public class RadioInitializerPatch
    {
        private static System.Collections.Generic.HashSet<int> processedObjects = new System.Collections.Generic.HashSet<int>();

        static void Postfix(GameObject __instance, bool value)
        {
            // Solo procesar cuando se activa un GameObject
            if (!value) return;

            // Evitar procesar el mismo objeto múltiples veces
            int instanceId = __instance.GetInstanceID();
            if (processedObjects.Contains(instanceId)) return;

            // Verificar si es un objeto de radio
            if (!IsRadioObject(__instance)) return;

            try
            {
                // Agregar el inicializador si no existe
                RadioComponentInitializer initializer = __instance.GetComponent<RadioComponentInitializer>();
                if (initializer == null)
                {
                    initializer = __instance.AddComponent<RadioComponentInitializer>();
                    ClimbTunes.ModLogger.LogInfo($"RadioComponentInitializer added to {__instance.name}");
                }

                // Marcar como procesado
                processedObjects.Add(instanceId);
            }
            catch (System.Exception ex)
            {
                ClimbTunes.ModLogger.LogError($"Error adding RadioComponentInitializer to {__instance.name}: {ex.Message}");
            }
        }

        private static bool IsRadioObject(GameObject obj)
        {
            try
            {
                // Verificar si es nuestro objeto de radio
                if (obj.name.Contains("Radio") || obj.name.Contains("climbtunes"))
                {
                    // Verificar que tenga la estructura esperada (Canvas hijo)
                    Transform canvas = obj.transform.Find("Canvas");
                    if (canvas != null)
                    {
                        // Verificar que tenga el RadioPanel
                        Transform radioPanel = canvas.Find("RadioPanel");
                        return radioPanel != null;
                    }
                }
            }
            catch (System.Exception ex)
            {
                ClimbTunes.ModLogger.LogWarning($"Error checking if {obj?.name} is radio object: {ex.Message}");
            }
            return false;
        }
    }

}