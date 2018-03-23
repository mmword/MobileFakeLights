using System;
using UnityEditor;
using UnityEngine;

public class FLLitLightGUI : ShaderGUI
{
    private MaterialProperty intensityModeProp = null;
    private MaterialProperty albedoMapProp = null;
    private MaterialProperty albedoColorProp = null;
    private MaterialProperty traceProp = null;
    private MaterialEditor m_MaterialEditor = null;

    private int InTensityMode = 0, TraceMode = 0;

    private void FindMaterialProperties(MaterialProperty[] properties)
    {
        intensityModeProp = FindProperty("_IntensityMode", properties);
        albedoMapProp = FindProperty("_MainTex", properties);
        albedoColorProp = FindProperty("_Color", properties);
        traceProp = FindProperty("_TraceMode", properties);
    }

    private void SetupIntensityMode(Material m)
    {
        if (InTensityMode > 0)
            m.EnableKeyword("_INTENSITY");
        else
            m.DisableKeyword("_INTENSITY");
    }

    private void SetupTraceMode(Material m)
    {
        for(int i = 1;i < 6;++i)
        {
            string value = string.Format("_TRACE{0}0", i);
            m.DisableKeyword(value);
        }
        m.EnableKeyword(string.Format("_TRACE{0}0", TraceMode));
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        //base.OnGUI(materialEditor, properties);
        Material material = materialEditor.target as Material;
        m_MaterialEditor = materialEditor;

        FindMaterialProperties(properties);

        EditorGUI.BeginChangeCheck();
        {
            m_MaterialEditor.TexturePropertySingleLine(FLUtilsGUI.Styles.albedoGlosinessLabels[1], albedoMapProp,albedoColorProp);
            m_MaterialEditor.TextureScaleOffsetProperty(albedoMapProp);

            InTensityMode = (int)intensityModeProp.floatValue;
            InTensityMode = Convert.ToInt32(EditorGUILayout.Toggle("intensity mode", Convert.ToBoolean(InTensityMode)));
            intensityModeProp.floatValue = InTensityMode;

            TraceMode = (int)traceProp.floatValue;
            TraceMode = EditorGUILayout.IntSlider("trace", TraceMode, 1, 5);
            traceProp.floatValue = TraceMode;
        }
        if (EditorGUI.EndChangeCheck())
        {
            material.shaderKeywords = null;
            SetupIntensityMode(material);
            SetupTraceMode(material);
        }

        materialEditor.RenderQueueField();

        EditorGUILayout.LabelField(string.Join(",",material.shaderKeywords));
    }

}
