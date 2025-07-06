using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using Zorro.Settings;
using System.Collections.Generic;
using UnityEngine.Localization;

namespace HDPeak.Settings
{
    /// <summary>
    /// Anti-aliasing quality modes for reducing jagged edges
    /// </summary>
    public enum AntialiasingMode
    {
        Off,
        MSAA2x,
        MSAA4x,
        MSAA8x
    }

    /// <summary>
    /// Anisotropic filtering modes for improving texture quality at distance
    /// </summary>
    public enum AnisotropicFilteringMode
    {
        Disable,
        Enable,
        ForceEnable
    }

    /// <summary>
    /// Texture quality levels based on Unity's globalTextureMipmapLimit
    /// </summary>
    public enum TextureQualityMode
    {
        VeryLow,    // Skip 4 mipmap levels (lowest quality)
        Low,        // Skip 3 mipmap levels  
        Medium,     // Skip 2 mipmap levels
        High,       // Skip 1 mipmap level
        VeryHigh    // No limit (full resolution, highest quality)
    }

    /// <summary>
    /// Shadow resolution quality levels for URP shadow maps
    /// </summary>
    public enum ShadowResolutionMode
    {
        VeryLow,    // 256x256
        Low,        // 512x512
        Medium,     // 1024x1024
        High,       // 2048x2048
        VeryHigh,   // 4096x4096
        Ultra       // 8192x8192
    }

    /// <summary>
    /// Camera opaque texture modes for URP rendering effects
    /// </summary>
    public enum OpaqueTextureMode
    {
        Disabled,   // Better performance, no opaque texture
        Enabled     // Enables opaque texture for post-processing effects
    }

    /// <summary>
    /// Maximum additional lights count for dynamic lighting
    /// </summary>
    public enum MaxLightsMode
    {
        VeryLow,    // 1 light
        Low,        // 2 lights
        Medium,     // 4 lights (default)
        High,       // 6 lights
        VeryHigh    // 8 lights
    }

    /// <summary>
    /// Dynamic batching modes for optimizing rendering of small objects
    /// </summary>
    public enum DynamicBatchingMode
    {
        Disabled,   // Better for complex scenes
        Enabled     // Better for scenes with many small objects
    }

    /// <summary>
    /// Anti-aliasing setting for reducing jagged edges on 3D objects
    /// </summary>
    public class AntiAliasingSetting : EnumSetting<AntialiasingMode>, IExposedSetting
    {
        public string GetDisplayName()
        {
            return "Anti-Aliasing";
        }

        public string GetCategory()
        {
            return HDPeak.HDPeakSettingsRegistry.GetPageId("HDPeak");
        }

        public override List<LocalizedString> GetLocalizedChoices()
        {
            return null; // Use unlocalized choices (enum names)
        }

        public override List<string> GetUnlocalizedChoices()
        {
            var choices = new List<string>();
            choices.Add("Off");
            choices.Add("MSAA 2x");
            choices.Add("MSAA 4x");
            choices.Add("MSAA 8x");
            return choices;
        }

        protected override AntialiasingMode GetDefaultValue()
        {
            return AntialiasingMode.MSAA2x;
        }

        public override void ApplyValue()
        {
            var currentRP = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
            if (currentRP == null)
            {
                // Fallback to QualitySettings if URP is not available
                switch (Value)
                {
                    case AntialiasingMode.Off:
                        QualitySettings.antiAliasing = 0;
                        break;
                    case AntialiasingMode.MSAA2x:
                        QualitySettings.antiAliasing = 2;
                        break;
                    case AntialiasingMode.MSAA4x:
                        QualitySettings.antiAliasing = 4;
                        break;
                    case AntialiasingMode.MSAA8x:
                        QualitySettings.antiAliasing = 8;
                        break;
                    default:
                        QualitySettings.antiAliasing = 2;
                        break;
                }
                return;
            }

            // Apply MSAA settings to URP asset
            switch (Value)
            {
                case AntialiasingMode.Off:
                    currentRP.msaaSampleCount = 1;
                    break;
                case AntialiasingMode.MSAA2x:
                    currentRP.msaaSampleCount = 2;
                    break;
                case AntialiasingMode.MSAA4x:
                    currentRP.msaaSampleCount = 4;
                    break;
                case AntialiasingMode.MSAA8x:
                    currentRP.msaaSampleCount = 8;
                    break;
                default:
                    currentRP.msaaSampleCount = 2;
                    break;
            }
        }
    }

    /// <summary>
    /// Anisotropic filtering setting for improving texture quality at distance
    /// </summary>
    public class AnisotropicFilteringSetting : EnumSetting<AnisotropicFilteringMode>, IExposedSetting
    {
        public string GetDisplayName()
        {
            return "Anisotropic Filtering";
        }

        public string GetCategory()
        {
            return HDPeak.HDPeakSettingsRegistry.GetPageId("HDPeak");
        }

        public override List<LocalizedString> GetLocalizedChoices()
        {
            return null; // Use unlocalized choices
        }

        public override List<string> GetUnlocalizedChoices()
        {
            var choices = new List<string>();
            choices.Add("Disable");
            choices.Add("Enable");
            choices.Add("Force Enable");
            return choices;
        }

        protected override AnisotropicFilteringMode GetDefaultValue()
        {
            return AnisotropicFilteringMode.Enable;
        }

        public override void ApplyValue()
        {
            switch (Value)
            {
                case AnisotropicFilteringMode.Disable:
                    QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
                    break;
                case AnisotropicFilteringMode.Enable:
                    QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
                    break;
                case AnisotropicFilteringMode.ForceEnable:
                    QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
                    break;
                default:
                    QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
                    break;
            }
        }
    }

    /// <summary>
    /// Texture quality setting for controlling texture resolution
    /// </summary>
    public class TextureQualitySetting : EnumSetting<TextureQualityMode>, IExposedSetting
    {
        public string GetDisplayName()
        {
            return "Texture Quality";
        }

        public string GetCategory()
        {
            return HDPeak.HDPeakSettingsRegistry.GetPageId("HDPeak");
        }

        public override List<LocalizedString> GetLocalizedChoices()
        {
            return null; // Use unlocalized choices
        }

        public override List<string> GetUnlocalizedChoices()
        {
            var choices = new List<string>();
            choices.Add("Very Low");
            choices.Add("Low");
            choices.Add("Medium");
            choices.Add("High");
            choices.Add("Very High");
            return choices;
        }

        protected override TextureQualityMode GetDefaultValue()
        {
            return TextureQualityMode.High;
        }

        public override void ApplyValue()
        {
            // Lower mipmap limit = higher quality (0 = full resolution)
            // Higher mipmap limit = lower quality (skip more mipmap levels)
            switch (Value)
            {
                case TextureQualityMode.VeryHigh:
                    QualitySettings.globalTextureMipmapLimit = 0;
                    break;
                case TextureQualityMode.High:
                    QualitySettings.globalTextureMipmapLimit = 1;
                    break;
                case TextureQualityMode.Medium:
                    QualitySettings.globalTextureMipmapLimit = 2;
                    break;
                case TextureQualityMode.Low:
                    QualitySettings.globalTextureMipmapLimit = 3;
                    break;
                case TextureQualityMode.VeryLow:
                    QualitySettings.globalTextureMipmapLimit = 4;
                    break;
                default:
                    QualitySettings.globalTextureMipmapLimit = 1;
                    break;
            }
        }
    }

    /// <summary>
    /// Shadow resolution setting for controlling shadow map quality
    /// </summary>
    public class ShadowResolutionSetting : EnumSetting<ShadowResolutionMode>, IExposedSetting
    {
        public string GetDisplayName()
        {
            return "Shadow Resolution";
        }

        public string GetCategory()
        {
            return HDPeak.HDPeakSettingsRegistry.GetPageId("HDPeak");
        }

        public override List<LocalizedString> GetLocalizedChoices()
        {
            return null; // Use unlocalized choices
        }

        public override List<string> GetUnlocalizedChoices()
        {
            var choices = new List<string>();
            choices.Add("Very Low");
            choices.Add("Low");
            choices.Add("Medium");
            choices.Add("High");
            choices.Add("Very High");
            choices.Add("Ultra");
            return choices;
        }

        protected override ShadowResolutionMode GetDefaultValue()
        {
            return ShadowResolutionMode.Medium;
        }

        public override void ApplyValue()
        {
            var urpAsset = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
            if (urpAsset != null)
            {
                switch (Value)
                {
                    case ShadowResolutionMode.VeryLow:
                        urpAsset.mainLightShadowmapResolution = 0x100; // 256
                        break;
                    case ShadowResolutionMode.Low:
                        urpAsset.mainLightShadowmapResolution = 0x200; // 512
                        break;
                    case ShadowResolutionMode.Medium:
                        urpAsset.mainLightShadowmapResolution = 0x400; // 1024
                        break;
                    case ShadowResolutionMode.High:
                        urpAsset.mainLightShadowmapResolution = 0x800; // 2048
                        break;
                    case ShadowResolutionMode.VeryHigh:
                        urpAsset.mainLightShadowmapResolution = 0x1000; // 4096
                        break;
                    case ShadowResolutionMode.Ultra:
                        urpAsset.mainLightShadowmapResolution = 0x2000; // 8192
                        break;
                    default:
                        urpAsset.mainLightShadowmapResolution = 0x400;
                        break;
                }
            }
        }
    }

    /// <summary>
    /// LOD bias setting for controlling model detail distance
    /// </summary>
    public class LODBiasSetting : FloatSetting, IExposedSetting
    {
        public string GetDisplayName()
        {
            return "LOD Bias";
        }

        public string GetCategory()
        {
            return HDPeak.HDPeakSettingsRegistry.GetPageId("HDPeak");
        }

        protected override float GetDefaultValue()
        {
            return 1.0f;
        }

        protected override float2 GetMinMaxValue()
        {
            return new float2(0.5f, 2.0f);
        }

        public override void ApplyValue()
        {
            QualitySettings.lodBias = Value;
        }
    }

    /// <summary>
    /// Opaque texture setting for controlling URP camera opaque texture
    /// </summary>
    public class OpaqueTextureSetting : EnumSetting<OpaqueTextureMode>, IExposedSetting
    {
        public string GetDisplayName()
        {
            return "Opaque Texture";
        }

        public string GetCategory()
        {
            return HDPeak.HDPeakSettingsRegistry.GetPageId("HDPeak");
        }

        public override List<LocalizedString> GetLocalizedChoices()
        {
            return null; // Use unlocalized choices
        }

        public override List<string> GetUnlocalizedChoices()
        {
            var choices = new List<string>();
            choices.Add("Disabled");
            choices.Add("Enabled");
            return choices;
        }

        protected override OpaqueTextureMode GetDefaultValue()
        {
            return OpaqueTextureMode.Disabled;
        }

        public override void ApplyValue()
        {
            var urpAsset = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
            if (urpAsset != null)
            {
                bool enableOpaque = Value == OpaqueTextureMode.Enabled;
                urpAsset.supportsCameraOpaqueTexture = enableOpaque;
            }
        }
    }

    /// <summary>
    /// Maximum additional lights setting for controlling dynamic lighting
    /// </summary>
    public class MaxLightsSetting : EnumSetting<MaxLightsMode>, IExposedSetting
    {
        public string GetDisplayName()
        {
            return "Max Additional Lights";
        }

        public string GetCategory()
        {
            return HDPeak.HDPeakSettingsRegistry.GetPageId("HDPeak");
        }

        public override List<LocalizedString> GetLocalizedChoices()
        {
            return null; // Use unlocalized choices
        }

        public override List<string> GetUnlocalizedChoices()
        {
            var choices = new List<string>();
            choices.Add("Very Low");
            choices.Add("Low");
            choices.Add("Medium");
            choices.Add("High");
            choices.Add("Very High");
            return choices;
        }

        protected override MaxLightsMode GetDefaultValue()
        {
            return MaxLightsMode.Medium;
        }

        public override void ApplyValue()
        {
            var urpAsset = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
            if (urpAsset != null)
            {
                switch (Value)
                {
                    case MaxLightsMode.VeryLow:
                        urpAsset.maxAdditionalLightsCount = 1;
                        break;
                    case MaxLightsMode.Low:
                        urpAsset.maxAdditionalLightsCount = 2;
                        break;
                    case MaxLightsMode.Medium:
                        urpAsset.maxAdditionalLightsCount = 4;
                        break;
                    case MaxLightsMode.High:
                        urpAsset.maxAdditionalLightsCount = 6;
                        break;
                    case MaxLightsMode.VeryHigh:
                        urpAsset.maxAdditionalLightsCount = 8;
                        break;
                    default:
                        urpAsset.maxAdditionalLightsCount = 4;
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Dynamic batching setting for optimizing rendering of small objects
    /// </summary>
    public class DynamicBatchingSetting : EnumSetting<DynamicBatchingMode>, IExposedSetting
    {
        public string GetDisplayName()
        {
            return "Dynamic Batching";
        }

        public string GetCategory()
        {
            return HDPeak.HDPeakSettingsRegistry.GetPageId("HDPeak");
        }

        public override List<LocalizedString> GetLocalizedChoices()
        {
            return null; // Use unlocalized choices
        }

        public override List<string> GetUnlocalizedChoices()
        {
            var choices = new List<string>();
            choices.Add("Disabled");
            choices.Add("Enabled");
            return choices;
        }

        protected override DynamicBatchingMode GetDefaultValue()
        {
            return DynamicBatchingMode.Disabled;
        }

        public override void ApplyValue()
        {
            var urpAsset = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
            if (urpAsset != null)
            {
                bool enableBatching = Value == DynamicBatchingMode.Enabled;
                urpAsset.supportsDynamicBatching = enableBatching;
            }
        }
    }
}