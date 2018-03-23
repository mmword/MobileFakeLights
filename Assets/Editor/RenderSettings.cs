using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Game.Rendering;

public class RenderSettings : EditorWindow {

    static FlatLightRPAsset asset;
    static Shader extVolumeEffect;
    static Shader extTangentVolumeEffect;
    static Shader defaultLit;

    [MenuItem("GameTools/RenderSettings")]
    static void Wnd()
    {
        var wnd = GetWindow<RenderSettings>(true);
        wnd.Show();
    }

    static void Setup()
    {
        if (asset == null)
            asset = AssetDatabase.LoadAssetAtPath<FlatLightRPAsset>("Assets/Content/Lighting/FlatLightRPAsset.asset");
        if (defaultLit == null)
            defaultLit = Shader.Find("FlatLightRP/FLLitOnly");
    }

    private void OnGUI()
    {
        Setup();
        if(asset != null)
        {
            EditorGUI.BeginChangeCheck();
            var mode = (FlatLightRPAsset.DrawMode)EditorGUILayout.EnumPopup(asset.Mode);
            var blurLights = EditorGUILayout.Toggle("blur lights", asset.BlurLights);
            var blurIterations = EditorGUILayout.IntSlider("blur iterations",asset.BlurLightsIterations, 0, 10);
            var SampleDistBlur = EditorGUILayout.FloatField("sample dist blur", asset.GetSampleDistBlur());
            var LightingDownSample = EditorGUILayout.IntSlider("lighting downsample",asset.GetDownsample(FlatLightRPAsset.FLTechnique.Ligting), 1, 10);
            var OpaqueDownsample = EditorGUILayout.IntSlider("opaque downsample", asset.GetDownsample(FlatLightRPAsset.FLTechnique.Opaques), 1, 10);
            var GroundDownsample = EditorGUILayout.IntSlider("ground downsample", asset.GetDownsample(FlatLightRPAsset.FLTechnique.Ground), 1, 10);
            var Depth = EditorGUILayout.IntField("depth", asset.GetDepthBits());
            var Ambient = EditorGUILayout.ColorField("Ambient", asset.GetAmbientColor());
            var EmissionBlend = EditorGUILayout.Slider(asset.GetEmissionBlend(), 0F, 1F);
            if(EditorGUI.EndChangeCheck())
            {
                asset.SetDrawMode(mode);
                asset.BlurLights = blurLights;
                asset.BlurLightsIterations = blurIterations;
                asset.SetSampleDistBlur(SampleDistBlur);
                asset.SetDownsample(FlatLightRPAsset.FLTechnique.Ligting, LightingDownSample);
                asset.SetDownsample(FlatLightRPAsset.FLTechnique.Opaques, OpaqueDownsample);
                asset.SetDownsample(FlatLightRPAsset.FLTechnique.Ground, GroundDownsample);
                asset.SetDepthBits(Depth);
                asset.SetAmbientColor(Ambient);
                asset.SetEmissionBlend(EmissionBlend);
                SceneView.RepaintAll();
            }
        }
    }


}
