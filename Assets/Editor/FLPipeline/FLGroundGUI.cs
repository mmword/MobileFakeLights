using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class FLGroundGUI : ShaderGUI
{
    enum FlowChannel
    {
        None = 0,
        One,
        Two,
        Three,
        Four,
        Five
    }

    string[] flowChannelNames = new string[]
    {
          "None", "One", "Two", "Three", "Four", "Five"
    };

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
    {
        base.OnGUI(materialEditor, props);

        Material targetMat = materialEditor.target as Material;
        string[] keyWords = targetMat.shaderKeywords;

        FlowChannel fchannel = FlowChannel.None;
        if (keyWords.Contains("_FLOW1"))
            fchannel = FlowChannel.One;
        if (keyWords.Contains("_FLOW2"))
            fchannel = FlowChannel.Two;
        if (keyWords.Contains("_FLOW3"))
            fchannel = FlowChannel.Three;
        if (keyWords.Contains("_FLOW4"))
            fchannel = FlowChannel.Four;
        if (keyWords.Contains("_FLOW5"))
            fchannel = FlowChannel.Five;

        bool flowDrift = keyWords.Contains("_FLOWDRIFT");
        bool flowRefraction = keyWords.Contains("_FLOWREFRACTION");

        EditorGUI.BeginChangeCheck();
        fchannel = (FlowChannel)EditorGUILayout.Popup((int)fchannel, flowChannelNames);
        if (fchannel != FlowChannel.None)
        {
            // var flowSpeed = FindProperty("_FlowSpeed", props);
            // var flowIntensity = FindProperty("_FlowIntensity", props);
            // var flowAlpha = FindProperty("_FlowAlpha", props);
            // var flowRefract = FindProperty("_FlowRefraction", props);

            //materialEditor.ShaderProperty(flowSpeed, "Flow Speed");
            //materialEditor.ShaderProperty(flowIntensity, "Flow Intensity");
            //materialEditor.ShaderProperty(flowAlpha, "Flow Alpha");
            //if (layerCount > 1)
            //{
            //    flowRefraction = EditorGUILayout.Toggle("Flow Refraction", flowRefraction);
            //    if (flowRefraction)
            //    {
            //        materialEditor.ShaderProperty(flowRefract, "Refraction Amount");
            //    }
            //}
            flowDrift = EditorGUILayout.Toggle("Flow Drift", flowDrift);
        }

        if (EditorGUI.EndChangeCheck())
        {
            var newKeywords = new List<string>();
            if (fchannel != FlowChannel.None)
            {
                newKeywords.Add("_FLOW" + (int)fchannel);
            }
            if (flowDrift)
            {
                newKeywords.Add("_FLOWDRIFT");
            }
            targetMat.shaderKeywords = newKeywords.ToArray();
            EditorUtility.SetDirty(targetMat);
        }
    }


}
