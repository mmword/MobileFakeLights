using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FLUtilsGUI
{

    public static class Styles
    {
        public static GUIContent[] albedoGlosinessLabels =
        {
            new GUIContent("Base (RGB) Glossiness (A)", "Base Color (RGB) and Glossiness (A)"),
            new GUIContent("Base (RGB)", "Base Color (RGB)")
        };

        public static GUIContent albedoAlphaLabel = new GUIContent("Base (RGB) Alpha (A)",
                "Base Color (RGB) and Transparency (A)");

        public static GUIContent[] specularGlossMapLabels =
        {
            new GUIContent("Specular Map (RGB)", "Specular Color (RGB)"),
            new GUIContent("Specular Map (RGB) Glossiness (A)", "Specular Color (RGB) Glossiness (A)")
        };

        public static GUIContent normalMapText = new GUIContent("Normal Map", "Normal Map");
        public static GUIContent emissionMapLabel = new GUIContent("Emission Map", "Emission Map");

        public static readonly string[] blendNames = Enum.GetNames(typeof(FLUtilsGUI.BlendMode));
        // public static readonly string[] glossinessSourceNames = Enum.GetNames(typeof(GlossinessSource));

        public static string renderingModeLabel = "Rendering Mode";
        public static string specularSourceLabel = "Specular";
        public static string glossinessSourceLabel = "Glossiness Source";
        public static string glossinessSource = "Glossiness Source";
        public static string albedoColorLabel = "Base Color";
        public static string albedoMapAlphaLabel = "Base(RGB) Alpha(A)";
        public static string albedoMapGlossinessLabel = "Base(RGB) Glossiness (A)";
        public static string alphaCutoffLabel = "Alpha Cutoff";
        public static string shininessLabel = "Shininess";
        public static string normalMapLabel = "Normal map";
        public static string emissionColorLabel = "Emission Color";
    }

    public enum BlendMode
    {
        Opaque,
        Cutout,
        Fade,   // Old school alpha-blending mode, fresnel does not affect amount of transparency
        Transparent, // Physically plausible transparency mode, implemented as alpha pre-multiply
        Additive, // Additive
        SoftAdditive, // Soft Additive
        Multiplicative, // Multiplicative
        Multiplicative2x,
        ParticleAdd,
        ParticleAdditiveMultiply,
        Sprite,
        ParticleAdditiveSoft,
        ParticleBlend,
        ParticleMultiply
    }

    private static void TransparencySetup(Material material, UnityEngine.Rendering.BlendMode from, UnityEngine.Rendering.BlendMode to)
    {
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_SrcBlend", (int)from);
        material.SetInt("_DstBlend", (int)to);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        // material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }

   public static void SetupMaterialWithBlendMode(Material material, BlendMode blendMode,int zwrite)
    {
        switch (blendMode)
        {
            case BlendMode.Opaque:
                material.SetOverrideTag("RenderType", "");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", zwrite);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                //  material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = -1;
                break;
            case BlendMode.Cutout:
                material.SetOverrideTag("RenderType", "TransparentCutout");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", zwrite);
                material.EnableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                // material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                break;
            case BlendMode.Fade:
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                // material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                break;
            case BlendMode.Transparent:
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                // material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                break;
            case BlendMode.Additive:
                TransparencySetup(material, UnityEngine.Rendering.BlendMode.One, UnityEngine.Rendering.BlendMode.One);
                break;
            case BlendMode.SoftAdditive:
                TransparencySetup(material, UnityEngine.Rendering.BlendMode.OneMinusDstColor, UnityEngine.Rendering.BlendMode.One);
                break;
            case BlendMode.Multiplicative:
                TransparencySetup(material, UnityEngine.Rendering.BlendMode.DstColor, UnityEngine.Rendering.BlendMode.Zero);
                break;
            case BlendMode.Multiplicative2x:
                TransparencySetup(material, UnityEngine.Rendering.BlendMode.DstColor, UnityEngine.Rendering.BlendMode.SrcColor);
                break;
            case BlendMode.ParticleAdd:
                TransparencySetup(material, UnityEngine.Rendering.BlendMode.SrcAlpha, UnityEngine.Rendering.BlendMode.One);
                break;
            case BlendMode.ParticleAdditiveMultiply:
            case BlendMode.Sprite:
                TransparencySetup(material, UnityEngine.Rendering.BlendMode.One, UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                break;
            case BlendMode.ParticleAdditiveSoft:
                TransparencySetup(material, UnityEngine.Rendering.BlendMode.One, UnityEngine.Rendering.BlendMode.OneMinusSrcColor);
                break;
            case BlendMode.ParticleBlend:
                TransparencySetup(material, UnityEngine.Rendering.BlendMode.DstColor, UnityEngine.Rendering.BlendMode.One);
                break;
            case BlendMode.ParticleMultiply:
                TransparencySetup(material, UnityEngine.Rendering.BlendMode.Zero, UnityEngine.Rendering.BlendMode.SrcColor);
                break;
        }
    }

}
