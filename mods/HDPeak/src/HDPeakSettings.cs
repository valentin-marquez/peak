using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using Zorro.Settings;
using System.Collections.Generic;
using UnityEngine.Localization;
using SettingsExtender;

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
    [ExtenderSetting(page: "HDPeak", displayName: "Anti-Aliasing")]
    public class AntiAliasingSetting : ExtenderEnumSetting<AntialiasingMode>
    {
        protected override AntialiasingMode GetDefaultValue() => AntialiasingMode.MSAA2x;

        public override List<LocalizedString> GetLocalizedChoices() => null;

        public AntiAliasingSetting() : base(Value => {
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
        }) {}
    }

    /// <summary>
    /// Anisotropic filtering setting for improving texture quality at distance
    /// </summary>
    [ExtenderSetting(page: "HDPeak", displayName: "Anisotropic Filtering")]
    public class AnisotropicFilteringSetting : ExtenderEnumSetting<AnisotropicFilteringMode>
    {
        protected override AnisotropicFilteringMode GetDefaultValue() => AnisotropicFilteringMode.Enable;

        public override List<LocalizedString> GetLocalizedChoices() => null;

        public AnisotropicFilteringSetting() : base(Value => {
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
        }) {}
    }

    /// <summary>
    /// Texture quality setting for controlling texture resolution
    /// </summary>
    [ExtenderSetting(page: "HDPeak", displayName: "Texture Quality")]
    public class TextureQualitySetting : ExtenderEnumSetting<TextureQualityMode>
    {
        protected override TextureQualityMode GetDefaultValue() => TextureQualityMode.High;

        public override List<LocalizedString> GetLocalizedChoices() => null;

        public TextureQualitySetting() : base(Value => {
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
        }) {}
    }

    /// <summary>
    /// Shadow resolution setting for controlling shadow map quality
    /// </summary>
    [ExtenderSetting(page: "HDPeak", displayName: "Shadow Resolution")]
    public class ShadowResolutionSetting : ExtenderEnumSetting<ShadowResolutionMode>
    {
        protected override ShadowResolutionMode GetDefaultValue() => ShadowResolutionMode.Medium;

        public override List<LocalizedString> GetLocalizedChoices() => null;

        public ShadowResolutionSetting() : base(Value => {
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
        }) {}
    }

    /// <summary>
    /// LOD bias setting for controlling model detail distance
    /// </summary>
    [ExtenderSetting(page: "HDPeak", displayName: "LOD Bias")]
    public class LODBiasSetting : ExtenderFloatSetting
    {
        protected override float GetDefaultValue() => 0;
        protected override float2 GetMinMaxValue() => new float2(0.5f, 2.0f);

        public LODBiasSetting() : base(Value => {
            QualitySettings.lodBias = Value;
        }) {}
    }

    /// <summary>
    /// Opaque texture setting for controlling URP camera opaque texture
    /// </summary>
    [ExtenderSetting(page: "HDPeak", displayName: "Opaque Texture")]
    public class OpaqueTextureSetting : ExtenderEnumSetting<OpaqueTextureMode>
    {
        protected override OpaqueTextureMode GetDefaultValue() => OpaqueTextureMode.Disabled;

        public override List<LocalizedString> GetLocalizedChoices() => null;

        public OpaqueTextureSetting() : base(Value => {
            var urpAsset = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
            if (urpAsset != null)
            {
                bool enableOpaque = Value == OpaqueTextureMode.Enabled;
                urpAsset.supportsCameraOpaqueTexture = enableOpaque;
            }
        }) {}
    }

    /// <summary>
    /// Maximum additional lights setting for controlling dynamic lighting
    /// </summary>
    [ExtenderSetting(page: "HDPeak", displayName: "Max Additional Lights")]
    public class MaxLightsSetting : ExtenderEnumSetting<MaxLightsMode>
    {
        protected override MaxLightsMode GetDefaultValue() => MaxLightsMode.Medium;

        public override List<LocalizedString> GetLocalizedChoices() => null;

        public MaxLightsSetting() : base(Value => {
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
        }) {}
    }

    /// <summary>
    /// Dynamic batching setting for optimizing rendering of small objects
    /// </summary>
    [ExtenderSetting(page: "HDPeak", displayName: "Dynamic Batching")]
    public class DynamicBatchingSetting : ExtenderEnumSetting<DynamicBatchingMode>
    {
        protected override DynamicBatchingMode GetDefaultValue() => DynamicBatchingMode.Disabled;

        public override List<LocalizedString> GetLocalizedChoices() => null;

        public DynamicBatchingSetting() : base(Value => {
            var urpAsset = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
            if (urpAsset != null)
            {
                bool enableBatching = Value == DynamicBatchingMode.Enabled;
                urpAsset.supportsDynamicBatching = enableBatching;
            }
        }) {}
    }
    
    /// <summary>
    /// Camera culling distance for controlling how far objects are rendered
    /// </summary>
    [ExtenderSetting(page: "HDPeak", displayName: "Culling Distance")]
    public class CullingDistanceSetting : ExtenderFloatSetting
    {
        protected override float GetDefaultValue() => 1500;
        protected override float2 GetMinMaxValue() => new float2(50f, 1500f);

        public override void Load(ISettingsSaveLoad loader)
        {
            base.Load(loader);
            HDPeak.Patches.MainCameraPatches.SetFarClipDistance((int)Value);
        }

        public CullingDistanceSetting() : base(Value => {
            HDPeak.Patches.MainCameraPatches.SetFarClipDistance((int)Value);
        }) {}
    }
}