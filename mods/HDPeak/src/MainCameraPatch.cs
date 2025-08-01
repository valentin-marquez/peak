using HarmonyLib;
using UnityEngine;

namespace HDPeak.Patches {
    [HarmonyPatch]
    public static class MainCameraPatches
    {   
        private static bool needsUpdate = false;
        private static int? customFarClipPlane = null;

        private static readonly AccessTools.FieldRef<MainCamera, Camera> CamRef =
            AccessTools.FieldRefAccess<MainCamera, Camera>("cam");

        [HarmonyPatch(typeof(MainCamera), "Awake")]
        [HarmonyPostfix]
        public static void Awake(MainCamera __instance)
        {
            UpdateCameraFarClipPlane(__instance);
        }

        [HarmonyPatch(typeof(MainCamera), "LateUpdate")]
        [HarmonyPostfix]
        public static void LateUpdate(MainCamera __instance)
        {
            if (needsUpdate) {
                UpdateCameraFarClipPlane(__instance);
            }
        }

        private static void UpdateCameraFarClipPlane(MainCamera instance)
        {
            var cam = CamRef(instance);

            if (cam != null && customFarClipPlane.HasValue)
            {
                cam.farClipPlane = customFarClipPlane.Value;
                needsUpdate = false;

                HDPeak.HDPeakPlugin.Logger.LogInfo($"Camera far clip plane was updated to {customFarClipPlane.Value}");
            }
        }

        internal static void SetFarClipDistance(int value)
        {
            customFarClipPlane = value;
            needsUpdate = true;

            HDPeak.HDPeakPlugin.Logger.LogInfo($"Updating culling distance");
        }
    }
}
