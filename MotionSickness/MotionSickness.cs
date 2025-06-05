using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace MotionSickness {
    [BepInPlugin("com.ex3124.MotionSickness", "MotionSickness", "1.0")]
    public class Plugin : BaseUnityPlugin {
        internal static ConfigEntry<bool> DisableShake;
        internal static ConfigEntry<bool> DisableBob;
        internal static ConfigEntry<bool> DisableTilt;
        internal static ConfigEntry<float> MaxFOV;
        internal static ConfigEntry<bool> DisableMask;
        internal static ConfigEntry<bool> HideSeamoth;

        public void Awake() {
            DisableShake = Config.Bind("Camera", "Disable Shake", true, "Camera shake effect in certain scenes\nFor example, the shake after executing 'camshake' on the console");
            DisableBob = Config.Bind("Camera", "Disable Bobbing", true, "Bobbing up and down when moving");
            DisableTilt = Config.Bind("Camera", "Disable Tilt", true, "Camera tilt when moving left and right");
            MaxFOV = Config.Bind("Camera", "Max FOV", 120f, new ConfigDescription("Customize maximum FOV value", new AcceptableValueRange<float>(90f, 180f)));
            DisableMask = Config.Bind("Camera", "Disable Mask", true, "Mask on the edge of the screen");
            HideSeamoth = Config.Bind("Renderer", "Hide Seamoth When Entered", false, "Better visibility when driving Seamoth");

            var harmony = new Harmony("com.ex3124.MotionSickness");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(MainCameraControl))]
    [HarmonyPatch("ShakeCamera")]
    public static class CameraShake {
        [HarmonyPrefix]
        public static bool Prefix() => !Plugin.DisableShake.Value;
    }

    [HarmonyPatch(typeof(MainCameraControl))]
    [HarmonyPatch("GetCameraBob")]
    public static class CameraBob {
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result) {
            if (Plugin.DisableBob.Value) {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(MainCameraControl))]
    [HarmonyPatch("UpdateStrafeTilt")]
    public static class CameraTilt {
        [HarmonyPrefix]
        public static bool Prefix() => !Plugin.DisableTilt.Value;
    }

    [HarmonyPatch(typeof(uGUI_TabbedControlsPanel))]
    [HarmonyPatch("AddSliderOption", new Type[] { typeof(int), typeof(string), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float), typeof(UnityAction<float>), typeof(SliderLabelMode), typeof(string), typeof(string) })]
    public static class FOVSlider {
        [HarmonyPrefix]
        public static bool Prefix(string label, ref float maxValue) {
            if (label == "FieldOfView")
                maxValue = Plugin.MaxFOV.Value;
            return true;
        }
    }

    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("SetScubaMaskActive")]
    public static class Mask {
        [HarmonyPrefix]
        public static bool Prefix(ref bool state) {
            if (Plugin.DisableMask.Value)
                state = false;
            return true;
        }
    }

    [HarmonyPatch(typeof(SeaMoth))]
    [HarmonyPatch("Update")]
    public static class RendererUpdate {
        [HarmonyPrefix]
        public static bool Prefix(SeaMoth __instance) {
            Renderer[] allRenderers = __instance.GetComponentsInChildren<Renderer>();
            if (Plugin.HideSeamoth.Value && allRenderers != null) {
                foreach (var renderer in allRenderers) {
                    if (renderer != null && renderer.name.ToLower().Contains("seamoth"))
                        renderer.enabled = !__instance.playerFullyEntered;
                }
            }
            return true;
        }
    }
}
