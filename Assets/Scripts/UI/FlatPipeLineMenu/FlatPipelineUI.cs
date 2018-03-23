using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Game.Rendering;

public class FlatPipelineUI : MonoBehaviour {

    public FlatLightRPAsset asset;
    public Toggle toggleBlur,fixedSizeToggle,toggleAmbient;
    public Slider blurIterations;
    public Slider LightsDownsample,LightsResolution;
    public Slider OpaquesDownsample;
    public Slider GroundDownsample;
    public Dropdown dropDown;
    public Text DrawStat;
    
    float updateTime = 0F;

    private void Start()
    {
        toggleBlur.isOn = asset.BlurLights;
        fixedSizeToggle.isOn = asset.LightsIsFixedSize;
        toggleAmbient.isOn = asset.AmbientPass;
        blurIterations.value = asset.BlurLightsIterations;
        LightsDownsample.value = asset.GetDownsample(FlatLightRPAsset.FLTechnique.Ligting);
        OpaquesDownsample.value = asset.GetDownsample(FlatLightRPAsset.FLTechnique.Opaques);
        GroundDownsample.value = asset.GetDownsample(FlatLightRPAsset.FLTechnique.Ground);
        LightsResolution.value = Mathf.Log(asset.LightsResolution, 2);

        dropDown.ClearOptions();
        dropDown.AddOptions(new List<string>(Enum.GetNames(typeof(FlatLightRPAsset.DrawMode))));
        dropDown.value = (int)asset.Mode;
    }

    private void Update()
    {
        if (DrawStat != null && asset != null && (Time.time - updateTime) > 0.5F)
        {
            updateTime = Time.time;
            UpdateDrawStat();
        }
    }

    static float rnd(float val)
    {
        return (float)Math.Round(val, 3);
    }

    void UpdateDrawStat()
    {
        string stat = string.Format(
        "GroundRenderTime = {0}\nOpaquesAndTransparentRenderTime={1}\nObstraclesRenderTime={2}\nLightsRenderTime={3}\nCompositeRenderTime={4}\nUnlitRenderTime={5}",
        rnd(asset.GroundRenderTime), rnd(asset.OpaquesAndTransparentRenderTime),
        rnd(asset.ObstraclesRenderTime), rnd(asset.LightsRenderTime), rnd(asset.CompositeRenderTime), rnd(asset.UnlitRenderTime));
        DrawStat.text = stat;
    }

    public void DrawModeChanged(int mode)
    {
        asset.SetDrawMode((FlatLightRPAsset.DrawMode)mode);
    }

    public void OnSetBlurIterations()
    {
        asset.BlurLightsIterations = (int)blurIterations.value;
    }

    public void OnToggleBlur()
    {
        asset.BlurLights = toggleBlur.isOn;
    }

    public void OnToggleAmbient()
    {
        asset.AmbientPass = toggleAmbient.isOn;
    }

    public void OnToggleFixedSize()
    {
        OnSetLightResolution();
    }

    public void OnSetLightResolution()
    {
        asset.SetLightFixedSize(fixedSizeToggle.isOn,1 << Mathf.RoundToInt(LightsResolution.value));
    }

    public void OnSetLightsDownsample()
    {
        asset.SetDownsample(FlatLightRPAsset.FLTechnique.Ligting,(int)LightsDownsample.value);
    }

    public void OnSetOpaquessDownsample()
    {
        asset.SetDownsample(FlatLightRPAsset.FLTechnique.Opaques, (int)OpaquesDownsample.value);
    }

    public void OnSetGroundDownsample()
    {
        asset.SetDownsample(FlatLightRPAsset.FLTechnique.Ground, (int)GroundDownsample.value);
    }

}
