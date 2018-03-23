using System;
using UnityEditor;
using UnityEngine;

public class FLLightingGUI : ShaderGUI  {

    private MaterialProperty blendModeProp = null;
    private MaterialProperty albedoMapProp = null;
    private MaterialProperty albedoColorProp = null;
    private MaterialProperty alphaCutoffProp = null;
    private MaterialProperty billboard = null;
    private MaterialProperty intensityProperty = null;
    private MaterialProperty zwriteProperty = null;

    private MaterialEditor m_MaterialEditor = null;

    private void FindMaterialProperties(MaterialProperty[] properties)
    {
        blendModeProp = FindProperty("_Mode", properties);
        albedoMapProp = FindProperty("_MainTex", properties);
        albedoColorProp = FindProperty("_Color", properties);
        intensityProperty = FindProperty("_Intensity", properties);

        alphaCutoffProp = FindProperty("_Cutoff", properties);
        billboard = FindProperty("_Billboard", properties);
        zwriteProperty = FindProperty("_ZWrite",properties);
    }

    private void SetupZWrite(Material m)
    {
        int v = (int)zwriteProperty.floatValue;
        EditorGUI.BeginChangeCheck();
        v = Convert.ToInt32(EditorGUILayout.Toggle("zwrite", Convert.ToBoolean(v)));
        if (EditorGUI.EndChangeCheck())
            zwriteProperty.floatValue = v;
    }

    private void SetupBillboard(Material m)
    {
        int v = (int)billboard.floatValue;
        EditorGUI.BeginChangeCheck();
        v = Convert.ToInt32(EditorGUILayout.Toggle("billboard", Convert.ToBoolean(v)));
        if (EditorGUI.EndChangeCheck())
            billboard.floatValue = v;
        if (v > 0)
            m.EnableKeyword("_BILLBOARD_ON");
        else
            m.DisableKeyword("_BILLBOARD_ON");
    }

    private void MaterialChanged(Material material)
    {
        material.shaderKeywords = null;
        FLUtilsGUI.SetupMaterialWithBlendMode(material, (FLUtilsGUI.BlendMode)material.GetFloat("_Mode"), (int)material.GetFloat("_ZWrite"));
        SetupBillboard(material);
        SetupZWrite(material);
    }

    private void DoBlendMode()
    {
        int modeValue = (int)blendModeProp.floatValue;
        EditorGUI.BeginChangeCheck();
        modeValue = EditorGUILayout.Popup(FLUtilsGUI.Styles.renderingModeLabel, modeValue, FLUtilsGUI.Styles.blendNames);
        if (EditorGUI.EndChangeCheck())
            blendModeProp.floatValue = modeValue;

        FLUtilsGUI.BlendMode mode = (FLUtilsGUI.BlendMode)blendModeProp.floatValue;

        EditorGUILayout.Space();

        if (mode == FLUtilsGUI.BlendMode.Opaque)
        {
            //int glossSource = (int)glossinessSourceProp.floatValue;
            m_MaterialEditor.TexturePropertySingleLine(FLUtilsGUI.Styles.albedoGlosinessLabels[1], albedoMapProp,
                albedoColorProp);
            m_MaterialEditor.TextureScaleOffsetProperty(albedoMapProp);
        }
        else
        {
            m_MaterialEditor.TexturePropertySingleLine(FLUtilsGUI.Styles.albedoAlphaLabel, albedoMapProp, albedoColorProp);
            if (mode == FLUtilsGUI.BlendMode.Cutout)
                m_MaterialEditor.RangeProperty(alphaCutoffProp, "Cutoff");
        }
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        //base.OnGUI(materialEditor, properties);

        Material material = materialEditor.target as Material;
        m_MaterialEditor = materialEditor;

        FindMaterialProperties(properties);

        EditorGUI.BeginChangeCheck();
        {
            DoBlendMode();
            SetupBillboard(material);
            SetupZWrite(material);
            m_MaterialEditor.TextureScaleOffsetProperty(albedoMapProp);
            m_MaterialEditor.FloatProperty(intensityProperty, "intensity");
        }
        if (EditorGUI.EndChangeCheck())
        {
            foreach (var obj in blendModeProp.targets)
                MaterialChanged((Material)obj);
        }

        materialEditor.RenderQueueField();

    }

}
