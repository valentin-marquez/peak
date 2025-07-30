using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using Zorro.Settings;
using System.Collections.Generic;
using UnityEngine.Localization;

namespace HDPeak.Settings
{
    public enum AntialiasingMode
    {
        Off,
        MSAA2x,
        MSAA4x,
        MSAA8x
    }

    public enum AnisotropicFilteringMode
    {
        Disable,
        Enable,
        ForceEnable
    }

    public enum TextureQualityMode
    {
        VeryLow,    // Skip 4 mipmap levels (lowest quality)
        Low,        // Skip 3 mipmap levels  
        Medium,     // Skip 2 mipmap levels
        High,       // Skip 1 mipmap level
        VeryHigh    // No limit (full resolution, highest quality)
    }

    public enum ShadowResolutionMode
    {
        VeryLow,    // 256x256
        Low,        // 512x512
        Medium,     // 1024x1024
        High,       // 2048x2048
        VeryHigh,   // 4096x4096
        Ultra       // 8192x8192
    }

    public enum OpaqueTextureMode
    {
        Disabled,   // Better performance, no opaque texture
        Enabled     // Enables opaque texture for post-processing effects
    }

    public enum MaxLightsMode
    {
        VeryLow,    // 1 light
        Low,        // 2 lights
        Medium,     // 4 lights (default)
        High,       // 6 lights
        VeryHigh    // 8 lights
    }

    public enum DynamicBatchingMode
    {
        Disabled,   // Better for complex scenes
        Enabled     // Better for scenes with many small objects
    }

    public class AntiAliasingSetting : EnumSetting<AntialiasingMode>, IExposedSetting
    {
        public string GetDisplayName()
        {
            return LocalizedText.GetText("HDPEAK_ANTIALIASING");
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
            choices.Add(LocalizedText.GetText("HDPEAK_ANTIALIASING_OFF"));
            choices.Add(LocalizedText.GetText("HDPEAK_ANTIALIASING_MSAA2X"));
            choices.Add(LocalizedText.GetText("HDPEAK_ANTIALIASING_MSAA4X"));
            choices.Add(LocalizedText.GetText("HDPEAK_ANTIALIASING_MSAA8X"));
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

    public class AnisotropicFilteringSetting : EnumSetting<AnisotropicFilteringMode>, IExposedSetting
    {
        public string GetDisplayName()
        {
            return LocalizedText.GetText("HDPEAK_ANISOTROPIC_FILTERING");
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
            choices.Add(LocalizedText.GetText("HDPEAK_ANISOTROPIC_FILTERING_DISABLE"));
            choices.Add(LocalizedText.GetText("HDPEAK_ANISOTROPIC_FILTERING_ENABLE"));
            choices.Add(LocalizedText.GetText("HDPEAK_ANISOTROPIC_FILTERING_FORCE_ENABLE"));
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

    public class TextureQualitySetting : EnumSetting<TextureQualityMode>, IExposedSetting
    {
        public string GetDisplayName()
        {
            return LocalizedText.GetText("HDPEAK_TEXTURE_QUALITY");
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
            choices.Add(LocalizedText.GetText("HDPEAK_TEXTURE_QUALITY_VERY_LOW"));
            choices.Add(LocalizedText.GetText("HDPEAK_TEXTURE_QUALITY_LOW"));
            choices.Add(LocalizedText.GetText("HDPEAK_TEXTURE_QUALITY_MEDIUM"));
            choices.Add(LocalizedText.GetText("HDPEAK_TEXTURE_QUALITY_HIGH"));
            choices.Add(LocalizedText.GetText("HDPEAK_TEXTURE_QUALITY_VERY_HIGH"));
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

    public class ShadowResolutionSetting : EnumSetting<ShadowResolutionMode>, IExposedSetting
    {
        public string GetDisplayName()
        {
            return LocalizedText.GetText("HDPEAK_SHADOW_RESOLUTION");
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
            choices.Add(LocalizedText.GetText("HDPEAK_SHADOW_RESOLUTION_VERY_LOW"));
            choices.Add(LocalizedText.GetText("HDPEAK_SHADOW_RESOLUTION_LOW"));
            choices.Add(LocalizedText.GetText("HDPEAK_SHADOW_RESOLUTION_MEDIUM"));
            choices.Add(LocalizedText.GetText("HDPEAK_SHADOW_RESOLUTION_HIGH"));
            choices.Add(LocalizedText.GetText("HDPEAK_SHADOW_RESOLUTION_VERY_HIGH"));
            choices.Add(LocalizedText.GetText("HDPEAK_SHADOW_RESOLUTION_ULTRA"));
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

    public class LODBiasSetting : FloatSetting, IExposedSetting
    {
        public string GetDisplayName()
        {
            return LocalizedText.GetText("HDPEAK_LOD_BIAS");
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

    public class OpaqueTextureSetting : EnumSetting<OpaqueTextureMode>, IExposedSetting
    {
        public string GetDisplayName()
        {
            return LocalizedText.GetText("HDPEAK_OPAQUE_TEXTURE");
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
            choices.Add(LocalizedText.GetText("HDPEAK_OPAQUE_TEXTURE_DISABLED"));
            choices.Add(LocalizedText.GetText("HDPEAK_OPAQUE_TEXTURE_ENABLED"));
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

    public class MaxLightsSetting : EnumSetting<MaxLightsMode>, IExposedSetting
    {
        public string GetDisplayName()
        {
            return LocalizedText.GetText("HDPEAK_MAX_ADDITIONAL_LIGHTS");
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
            choices.Add(LocalizedText.GetText("HDPEAK_MAX_ADDITIONAL_LIGHTS_VERY_LOW"));
            choices.Add(LocalizedText.GetText("HDPEAK_MAX_ADDITIONAL_LIGHTS_LOW"));
            choices.Add(LocalizedText.GetText("HDPEAK_MAX_ADDITIONAL_LIGHTS_MEDIUM"));
            choices.Add(LocalizedText.GetText("HDPEAK_MAX_ADDITIONAL_LIGHTS_HIGH"));
            choices.Add(LocalizedText.GetText("HDPEAK_MAX_ADDITIONAL_LIGHTS_VERY_HIGH"));
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

    public class DynamicBatchingSetting : EnumSetting<DynamicBatchingMode>, IExposedSetting
    {
        public string GetDisplayName()
        {
            return LocalizedText.GetText("HDPEAK_DYNAMIC_BATCHING");
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
            choices.Add(LocalizedText.GetText("HDPEAK_DYNAMIC_BATCHING_DISABLED"));
            choices.Add(LocalizedText.GetText("HDPEAK_DYNAMIC_BATCHING_ENABLED"));
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