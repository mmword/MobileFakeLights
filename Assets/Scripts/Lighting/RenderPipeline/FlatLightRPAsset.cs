//The MIT License (MIT)
//Copyright (c) 2018 Vladimir Aksenov
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using UnityEngine;
using UnityEngine.Experimental.Rendering;
using System;

namespace Game.Rendering
{
    public class FlatLightRPAsset : RenderPipelineAsset
    {
        public enum DrawMode
        {
            Lit,
            Unlit,
            Opaque,
            Obstracles,
            Lights,
            Ambient
        }

        public enum FLTechnique
        {
            Ground,
            Opaques,
            Ligting
        }

        [SerializeField] DrawMode mode = DrawMode.Lit;
        [SerializeField] bool blurLights = false;
        [SerializeField] bool ambientPass = false;
        [SerializeField] int blurLightsIterations = 2;
        [SerializeField] float SampleDistBlur = 1;
        [SerializeField] int LightingDownSample = 4;
        [SerializeField] int OpaqueDownsample = 4;
        [SerializeField] int GroundDownsample = 4;
        [SerializeField] int Depth = 16;
        [SerializeField] Color Ambient = Color.clear;
        [SerializeField] float EmissionBlend = 0.5F;
        [SerializeField] bool RenderGroundSeparately = false;
        [SerializeField] bool LightsFixedSize;
        [SerializeField] int LightsRes = 128;
        [SerializeField] Shader compositeShader;
        [SerializeField] Shader defaultShader;
        [SerializeField] Material defaultMaterial;
        [SerializeField] Material defaultUIMaterial;

        [HideInInspector]
        [NonSerialized]
        public float UnlitRenderTime;
        [HideInInspector]
        [NonSerialized]
        public float GroundRenderTime;
        [HideInInspector]
        [NonSerialized]
        public float OpaquesAndTransparentRenderTime;
        [HideInInspector]
        [NonSerialized]
        public float ObstraclesRenderTime;
        [HideInInspector]
        [NonSerialized]
        public float LightsRenderTime;
        [HideInInspector]
        [NonSerialized]
        public float CompositeRenderTime;

#if UNITY_EDITOR
        [UnityEditor.MenuItem("RenderPipeline/Create FlatLightRPAsset", false, 20)]
        static void CreateFlatLightRPAsset()
        {
            var instance = CreateInstance<FlatLightRPAsset>();

            string path = UnityEditor.EditorUtility.SaveFilePanelInProject("Save Lightweight Asset", "FlatLightRPAsset", "asset", "Please enter a file name to save the asset to");
            if (path.Length > 0)
                UnityEditor.AssetDatabase.CreateAsset(instance, path);
        }
#endif

        void OnValidate()
        {
            DestroyCreatedInstances();
        }

        public Color GetAmbientColor()
        {
            return Ambient;
        }

        public void SetAmbientColor(Color col)
        {
            Ambient = col;
        }

        public Shader GetCopositeShader()
        {
            return compositeShader;
        }

        public float GetEmissionBlend()
        {
            return Mathf.Clamp01(EmissionBlend);
        }

        public float SetEmissionBlend(float val)
        {
            return EmissionBlend = Mathf.Clamp01(val);
        }

        public int BlurLightsIterations
        {
            get
            {
                return Mathf.Max(0, blurLightsIterations);
            }
            set
            {
                blurLightsIterations = Mathf.Max(0, value);
            }
        }

        public float GetSampleDistBlur()
        {
            return SampleDistBlur;
        }

        public void SetSampleDistBlur(float val)
        {
            SampleDistBlur = val;
        }

        public void SetDrawMode(DrawMode mode)
        {
            this.mode = mode;
        }

        public DrawMode Mode
        {
            get
            {
                return mode;
            }
        }

        public bool LightsIsFixedSize
        {
            get
            {
                return LightsFixedSize;
            }
        }
        public int LightsResolution
        {
            get
            {
                return LightsRes;
            }
        }

        public bool GroundIsRenderedSeparately
        {
            get
            {
                return RenderGroundSeparately;
            }
        }

        public bool BlurLights
        {
            get
            {
                return blurLights;
            }
            set
            {
                blurLights = value;
            }
        }

        public bool AmbientPass
        {
            get
            {
                return ambientPass;
            }
            set
            {
                ambientPass = value;
            }
        }

        public void SetDepthBits(int depth)
        {
            this.Depth = depth;
        }

        public void SetLightFixedSize(bool enabled,int size)
        {
            LightsFixedSize = enabled;
            LightsRes = size;
        }

        public int GetDepthBits()
        {
            return Depth;
        }

        public int GetDownsample(FLTechnique tech)
        {
            switch (tech)
            {
                case FLTechnique.Ligting: return Mathf.Max(1, LightingDownSample);
                case FLTechnique.Opaques: return Mathf.Max(1, OpaqueDownsample);
                case FLTechnique.Ground: return Mathf.Max(1, GroundDownsample);
            }
            return 1;
        }

        public void SetDownsample(FLTechnique tech, int downsample)
        {
            int value = Mathf.Max(1, downsample);
            switch (tech)
            {
                case FLTechnique.Ligting: LightingDownSample = value; break;
                case FLTechnique.Opaques: OpaqueDownsample = value; break;
                case FLTechnique.Ground: GroundDownsample = value; break;
            }
        }

        protected override IRenderPipeline InternalCreatePipeline()
        {
            return new FlatLightRenderPipeline(this);
        }

        public override Shader GetDefaultShader()
        {
            return defaultShader;
        }

        public override Material GetDefaultMaterial()
        {
            return defaultMaterial;
        }

        public override Material GetDefaultUIMaterial()
        {
            if(defaultUIMaterial != null)
                return defaultUIMaterial;
            return base.GetDefaultUIMaterial();
        }

    }
}