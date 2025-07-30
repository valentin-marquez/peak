using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using ClimbTunes.Components;
using ClimbTunes.Manager;

namespace ClimbTunes.Patches
{
    [HarmonyPatch(typeof(Item))]
    public class RadioItemPatch
    {
        // Permitir que la interacción normal funcione (pickup con E)
        // No parchear Interact() - debe funcionar normalmente para agarrar el objeto

        // Parche para el uso primario (click izquierdo cuando se tiene en las manos)
        [HarmonyPatch("StartUsePrimary")]
        [HarmonyPrefix]
        static bool StartUsePrimaryPrefix(Item __instance)
        {
            if (IsRadioItem(__instance))
            {
                ClimbTunes.ModLogger.LogInfo("Radio primary use started - showing UI");
                ShowRadioUI(__instance);
                return false; // Prevenir ejecución original
            }
            return true;
        }

        // También parchear FinishCastPrimary para manejar cuando se suelta el botón
        [HarmonyPatch("FinishCastPrimary")]
        [HarmonyPrefix]
        static bool FinishCastPrimaryPrefix(Item __instance)
        {
            if (IsRadioItem(__instance))
            {
                ClimbTunes.ModLogger.LogInfo("Radio primary use finished");
                // Aquí podrías manejar lógica adicional si es necesario
                return false; // Prevenir ejecución original
            }
            return true;
        }

        // Parchear el GUIManager para registrar nuestro Canvas como una ventana que bloquea input
        [HarmonyPatch(typeof(GUIManager), "UpdateWindowStatus")]
        [HarmonyPostfix]
        static void UpdateWindowStatusPostfix(GUIManager __instance)
        {
            // Buscar radios activas con UI abierta
            if (HasActiveRadioUI())
            {
                // Forzar que el cursor se muestre y el input se bloquee
                typeof(GUIManager).GetProperty("windowShowingCursor").SetValue(__instance, true);
                typeof(GUIManager).GetProperty("windowBlockingInput").SetValue(__instance, true);
            }
        }

        private static bool IsRadioItem(Item item)
        {
            // Verificar si es nuestro objeto Radio
            // Solo verificar cuando el objeto está siendo usado (Held state)
            return item.gameObject.name.Contains("Radio") && 
                   item.itemID == ItemDatabaseManager.GetRadioItemID() &&
                   item.itemState == ItemState.Held;
        }

        private static void ShowRadioUI(Item radioItem)
        {
            try
            {
                // Buscar el Canvas en el objeto
                Transform canvasTransform = radioItem.transform.Find("Canvas");
                if (canvasTransform == null)
                {
                    ClimbTunes.ModLogger.LogError("Canvas not found in Radio item");
                    return;
                }

                GameObject canvas = canvasTransform.gameObject;
                bool wasActive = canvas.activeInHierarchy;
                
                // Asegurar que el Canvas tiene el RadioUIManager
                RadioUIManager uiManager = canvas.GetComponent<RadioUIManager>();
                if (uiManager == null)
                {
                    uiManager = canvas.AddComponent<RadioUIManager>();
                    ClimbTunes.ModLogger.LogInfo("RadioUIManager added to Canvas");
                }
                
                // Activar/desactivar el canvas (toggle)
                canvas.SetActive(!wasActive);
                
                if (!wasActive)
                {
                    ClimbTunes.ModLogger.LogInfo("Radio UI activated");
                    
                    // Activar el RadioPanel específico
                    Transform radioPanelTransform = canvasTransform.Find("RadioPanel");
                    if (radioPanelTransform != null)
                    {
                        radioPanelTransform.gameObject.SetActive(true);
                    }
                }
                else
                {
                    ClimbTunes.ModLogger.LogInfo("Radio UI deactivated");
                }
            }
            catch (System.Exception ex)
            {
                ClimbTunes.ModLogger.LogError($"Error showing radio UI: {ex.Message}");
            }
        }

        private static bool HasActiveRadioUI()
        {
            // Buscar todas las radios activas con UI abierta usando el RadioUIManager
            RadioUIManager[] allRadioUIs = Object.FindObjectsOfType<RadioUIManager>();
            foreach (RadioUIManager radioUI in allRadioUIs)
            {
                if (radioUI.IsUIActive())
                {
                    return true;
                }
            }
            return false;
        }
    }
}