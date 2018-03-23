//The MIT License (MIT)
//Copyright (c) 2018 Vladimir Aksenov
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Profiling;

namespace Game.Rendering
{
    public class FlatLightRenderPipeline : RenderPipeline
    {
        private readonly FlatLightRPAsset asset;
        private Camera m_CurrCamera;

        private static readonly ShaderPassName m_LitPassName = new ShaderPassName("FLLit");
        private static readonly ShaderPassName m_NormalsLitPassName = new ShaderPassName("FLLitNormals");
        private static readonly ShaderPassName m_DrawLights = new ShaderPassName("FLLitLight");
        private static readonly ShaderPassName m_DrawUnlt = new ShaderPassName("FLUnlit");
        private static readonly ShaderPassName m_DrawGround = new ShaderPassName("FLGround");

        private int obstracle = Shader.PropertyToID("ObstracleTex");
        private RenderTargetIdentifier obstracleRT;
        private int lights = Shader.PropertyToID("LightsTex");
        private RenderTargetIdentifier lightsRT;
        private int blurLights = Shader.PropertyToID("LightsBlurTex");
        private RenderTargetIdentifier blurLightsRT;
        private int ambient = Shader.PropertyToID("AmbientTex");
        private RenderTargetIdentifier ambientRT;
        private int screenColor = Shader.PropertyToID("ScreenColorTex");
        private RenderTargetIdentifier screenColorRT;
        private int ground = Shader.PropertyToID("Ground");
        private RenderTargetIdentifier groundRT;

        private readonly int Ambient = Shader.PropertyToID("_Ambient");
        private readonly int SampleDistEmission = Shader.PropertyToID("_SampleDistEmission");
        private readonly int EmissionBlend = Shader.PropertyToID("_EmissionBlend");
        private readonly int GridPosMatrix = Shader.PropertyToID("_GridPos");
        private readonly int envObstracleTex = Shader.PropertyToID("ENVObstracleTex");
        private readonly int envLightsTex = Shader.PropertyToID("ENVLightTex");
        private readonly int SampleDistBlur = Shader.PropertyToID("_SampleDistBlur");
        private readonly int TexelSize = Shader.PropertyToID("_TexelSize");
        private readonly int ENVLightOffset = Shader.PropertyToID("ENVLightOffset");

        private Material LightsComposite;
        private readonly Color cWite = new Color(1, 1, 1, 1);

        private const int LIGHT_BLUR_PASS = 1;
        private const int AMBIENT_PASS = 2;
        private const int COMPOSITE_PASS = 3;
        private const int COMPOSITE_AMBIENT_PASS = 4;

        private struct TimeSample
        {
            float time;
            public static TimeSample Sample(string name)
            {
#if UNITY_EDITOR
                Profiler.BeginSample(name);
#endif
                return new TimeSample()
                {
                    time = Time.realtimeSinceStartup * 1000F
                };
            }
            public float Result()
            {
#if UNITY_EDITOR
                Profiler.EndSample();
#endif
                return (1000F * Time.realtimeSinceStartup) - time;
            }
        }

        public FlatLightRenderPipeline(FlatLightRPAsset asset)
        {
            this.asset = asset;
            obstracleRT = new RenderTargetIdentifier(obstracle);
            lightsRT = new RenderTargetIdentifier(lights);
            blurLightsRT = new RenderTargetIdentifier(blurLights);
            ambientRT = new RenderTargetIdentifier(ambient);
            screenColorRT = new RenderTargetIdentifier(screenColor);
            groundRT = new RenderTargetIdentifier(ground);
            Shader composite = asset.GetCopositeShader();
            if (composite == null)
                composite = Shader.Find("FlatLightRP/FLLightComposite");
            if (composite != null)
                LightsComposite = FLUtils.CreateEngineMaterial(composite);
        }

        public override void Dispose()
        {
            base.Dispose();
            Shader.globalRenderPipeline = "";
            if (LightsComposite != null)
                Object.DestroyImmediate(LightsComposite);
        }

        CullResults m_CullResults;
        public override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            base.Render(context, cameras);

            if (LightsComposite == null)
            {
                Debug.LogError("cant render , becouse composite material is null");
                return;
            }

            Shader.globalRenderPipeline = "FlatLightRenderPipeline";

            // no sorting , no stereo
            foreach (Camera camera in cameras)
            {
                bool sceneViewCamera = camera.cameraType == CameraType.SceneView;
                m_CurrCamera = camera;

                ScriptableCullingParameters cullingParameters;
                if (!CullResults.GetCullingParameters(m_CurrCamera, false, out cullingParameters))
                    continue;

#if UNITY_EDITOR
                // Emit scene view UI
                if (sceneViewCamera)
                    ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
#endif

                CullResults.Cull(ref cullingParameters, context, ref m_CullResults);
                // skip light and skip shadow
                // skip intermediate resouces

                // SetupCameraProperties does the following:
                // Setup Camera RenderTarget and Viewport
                // VR Camera Setup and SINGLE_PASS_STEREO props
                // Setup camera view, proj and their inv matrices.
                // Setup properties: _WorldSpaceCameraPos, _ProjectionParams, _ScreenParams, _ZBufferParams, unity_OrthoParams
                // Setup camera world clip planes props
                // setup HDR keyword
                // Setup global time properties (_Time, _SinTime, _CosTime)
                context.SetupCameraProperties(m_CurrCamera, false);

                InternalRender(ref context);

                // no depth pass
                // present
                context.Submit();
            }

        }

        private void InternalRender(ref ScriptableRenderContext context)
        {
            // TODO : temporary for mip map camera renderers
            //if((m_CurrCamera.cullingMask & (1 << 17)) == 1<<17 && m_CurrCamera.cameraType != CameraType.SceneView)
            //{
            //    RenderUnlit(ref context, false);
            //    return;
            //}


            if (asset.Mode == FlatLightRPAsset.DrawMode.Unlit)
            {
                RenderUnlit(ref context, true);
            }
            else
            {
                RenderOpaquesAndTransparent(ref context);
                if (asset.Mode == FlatLightRPAsset.DrawMode.Opaque)
                {
                    CommandBuffer cmd = CmdBufferPool.Get("Display Opaque");
                    cmd.Blit(screenColorRT, BuiltinRenderTextureType.CameraTarget);
                    context.ExecuteCommandBuffer(cmd);
                    CmdBufferPool.Release(cmd);
                }
                else
                {
                    RenderObstracles(ref context);
                    if (asset.Mode == FlatLightRPAsset.DrawMode.Obstracles)
                    {
                        CommandBuffer cmd = CmdBufferPool.Get("Display Obstracles");
                        cmd.Blit(obstracleRT, BuiltinRenderTextureType.CameraTarget);
                        context.ExecuteCommandBuffer(cmd);
                        CmdBufferPool.Release(cmd);
                    }
                    else
                    {
                        RenderLights(ref context);
                        RenderUnlit(ref context, false);
                    }
                }
                ClearResources(ref context);
            }

            if (CmdBufferPool.ReferencesCount > 0)
                Debug.LogError("pool references count is large then 0 !");
        }

        private bool ForceClear()
        {
            // Clear RenderTarget to avoid tile initialization on mobile GPUs
            // https://community.arm.com/graphics/b/blog/posts/mali-performance-2-how-to-correctly-handle-framebuffers
            return (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer);
        }

        private void SetRT(CommandBuffer cmd, RenderTargetIdentifier colorBuffer, Color color, bool clearColor, bool clearDepth)
        {
            cmd.SetRenderTarget(colorBuffer);
            if (clearColor || clearDepth)
            {
                cmd.ClearRenderTarget(clearDepth, clearColor, color);
            }
        }

        private void ClearAndSetRT(CommandBuffer cmd, RenderTargetIdentifier rt, bool clearDepthBuffer = true)
        {
            if (ForceClear())
            {
                SetRT(cmd, rt, m_CurrCamera.backgroundColor.linear, true, clearDepthBuffer);
            }
            else
            {
                bool clearDepth = false, clearColor = false;
                CameraClearFlags cameraClearFlags = m_CurrCamera.clearFlags;
                if (cameraClearFlags != CameraClearFlags.Nothing)
                {
                    clearDepth = true;
                    if (cameraClearFlags == CameraClearFlags.Color || cameraClearFlags == CameraClearFlags.Skybox)
                        clearColor = true;
                }
                SetRT(cmd, rt, m_CurrCamera.backgroundColor.linear, clearColor, clearDepth && clearDepthBuffer);
            }
        }

        private void RenderOpaquesAndTransparent(ref ScriptableRenderContext context)
        {
            int opaque_width = m_CurrCamera.pixelWidth / asset.GetDownsample(FlatLightRPAsset.FLTechnique.Opaques);
            int opaque_height = m_CurrCamera.pixelHeight / asset.GetDownsample(FlatLightRPAsset.FLTechnique.Opaques);

            if (asset.GroundIsRenderedSeparately)
            {
                var groundTime = TimeSample.Sample("Ground Sample");
                int ground_width = m_CurrCamera.pixelWidth / asset.GetDownsample(FlatLightRPAsset.FLTechnique.Ground);
                int ground_height = m_CurrCamera.pixelHeight / asset.GetDownsample(FlatLightRPAsset.FLTechnique.Ground);
                CommandBuffer cmd = CmdBufferPool.Get("SetCameraRenderTarget(Ground)");
                cmd.GetTemporaryRT(ground, ground_width, ground_height, asset.GetDepthBits());
                ClearAndSetRT(cmd, groundRT);
                context.ExecuteCommandBuffer(cmd);

                var groundDrawSettings = new DrawRendererSettings(m_CurrCamera, m_DrawGround);
                groundDrawSettings.SetShaderPassName(1, m_DrawGround);
                groundDrawSettings.sorting.flags = SortFlags.CommonOpaque;

                var groundFilterSettings = new FilterRenderersSettings(true)
                {
                    renderQueueRange = RenderQueueRange.opaque,
                };

                if (m_CurrCamera.clearFlags == CameraClearFlags.Skybox)
                    context.DrawSkybox(m_CurrCamera);

                context.DrawRenderers(m_CullResults.visibleRenderers, ref groundDrawSettings, groundFilterSettings);

                cmd.Clear();
                cmd.GetTemporaryRT(screenColor, opaque_width, opaque_height, asset.GetDepthBits());
                SetRT(cmd, screenColorRT, m_CurrCamera.backgroundColor.linear, false, true);
                cmd.Blit(groundRT, screenColorRT);
                cmd.ReleaseTemporaryRT(ground);
                context.ExecuteCommandBuffer(cmd);
                CmdBufferPool.Release(cmd);
                asset.GroundRenderTime = groundTime.Result();
            }
            else
            {
                CommandBuffer cmd = CmdBufferPool.Get("SetCameraRenderTarget");
                cmd.GetTemporaryRT(screenColor, opaque_width, opaque_height, asset.GetDepthBits());
                ClearAndSetRT(cmd, screenColorRT);
                context.ExecuteCommandBuffer(cmd);
                CmdBufferPool.Release(cmd);

                if (m_CurrCamera.clearFlags == CameraClearFlags.Skybox)
                    context.DrawSkybox(m_CurrCamera);
            }

            var opTime = TimeSample.Sample("Opaques And Transparent Sample");
            var opaqueDrawSettings = new DrawRendererSettings(m_CurrCamera, m_LitPassName);
            opaqueDrawSettings.SetShaderPassName(1, m_LitPassName);
            opaqueDrawSettings.sorting.flags = SortFlags.CommonOpaque;

            var opaqueFilterSettings = new FilterRenderersSettings(true)
            {
                renderQueueRange = RenderQueueRange.opaque,
            };

            context.DrawRenderers(m_CullResults.visibleRenderers, ref opaqueDrawSettings, opaqueFilterSettings);

            var transparentSettings = new DrawRendererSettings(m_CurrCamera, m_LitPassName);
            transparentSettings.SetShaderPassName(1, m_LitPassName);
            transparentSettings.sorting.flags = SortFlags.CommonTransparent;

            var transparentFilterSettings = new FilterRenderersSettings(true)
            {
                renderQueueRange = RenderQueueRange.transparent
            };

            context.DrawRenderers(m_CullResults.visibleRenderers, ref transparentSettings, transparentFilterSettings);
            asset.OpaquesAndTransparentRenderTime = opTime.Result();
        }

        private void RenderObstracles(ref ScriptableRenderContext context)
        {
            var obstracleTime = TimeSample.Sample("Obstracles Sample");
            int width, height;
            GetLightingResolution(out width, out height);

            var cmdBuf = CmdBufferPool.Get("Obstracles");
            cmdBuf.GetTemporaryRT(obstracle, width, height, asset.GetDepthBits());
            SetRT(cmdBuf, obstracleRT, cWite.linear, true, true);
            context.ExecuteCommandBuffer(cmdBuf);
            CmdBufferPool.Release(cmdBuf);

            var opaqueDrawSettings = new DrawRendererSettings(m_CurrCamera, m_NormalsLitPassName);
            opaqueDrawSettings.SetShaderPassName(1, m_NormalsLitPassName);
            opaqueDrawSettings.sorting.flags = SortFlags.CommonOpaque;

            var opaqueFilterSettings = new FilterRenderersSettings(true)
            {
                renderQueueRange = RenderQueueRange.opaque,
            };

            context.DrawRenderers(m_CullResults.visibleRenderers, ref opaqueDrawSettings, opaqueFilterSettings);
            asset.ObstraclesRenderTime = obstracleTime.Result();
        }

        private void GetLightingResolution(out int width,out int height)
        {
            int downSample = asset.GetDownsample(FlatLightRPAsset.FLTechnique.Ligting);
            width = asset.LightsIsFixedSize ? asset.LightsResolution : m_CurrCamera.pixelWidth / downSample;
            height = asset.LightsIsFixedSize ? asset.LightsResolution : m_CurrCamera.pixelHeight / downSample;
        }

        private void RenderLights(ref ScriptableRenderContext context)
        {
            var lightsTime = TimeSample.Sample("Lights Sample");
            int width, height;
            GetLightingResolution(out width, out height);

            var ColorRT = BuiltinRenderTextureType.CameraTarget;

            Vector2 downSampleTexelSize = new Vector2(1F / width, 1F / height);
            Vector2 texel = new Vector2(1F / m_CurrCamera.pixelWidth, 1F / m_CurrCamera.pixelHeight);
            float LightPixelSize = m_CurrCamera.orthographicSize * 2f / m_CurrCamera.pixelHeight;
            float lightPixelsPerUnityMeter = 1F / LightPixelSize;
            Vector3 mainPos = m_CurrCamera.transform.position;
            Vector3 gridPos = new Vector3(
                Mathf.Round(mainPos.x * lightPixelsPerUnityMeter) / lightPixelsPerUnityMeter,
                Mathf.Round(mainPos.y * lightPixelsPerUnityMeter) / lightPixelsPerUnityMeter,
                Mathf.Round(mainPos.z * lightPixelsPerUnityMeter) / lightPixelsPerUnityMeter);
            Vector3 posDiff = gridPos - mainPos;
            Vector3 pos = posDiff + mainPos;
            Vector2 offset = Vector2.Scale(texel, -posDiff * (lightPixelsPerUnityMeter));

            Matrix4x4 view = Matrix4x4.Inverse(Matrix4x4.TRS(pos, m_CurrCamera.transform.rotation, new Vector3(1, 1, -1)));
            Matrix4x4 proj = GL.GetGPUProjectionMatrix(m_CurrCamera.projectionMatrix, true);
            Matrix4x4 gPos = proj * view;

            var cmdBuf = CmdBufferPool.Get("Lights");
            cmdBuf.SetGlobalMatrix(GridPosMatrix, gPos);
            cmdBuf.GetTemporaryRT(ambient, width, height, 0);
            cmdBuf.GetTemporaryRT(lights, width, height, 0);
            SetRT(cmdBuf, lightsRT, Color.clear, true, false); ;
            cmdBuf.SetGlobalTexture(envObstracleTex, obstracleRT);
            context.ExecuteCommandBuffer(cmdBuf);

            var opaqueDrawSettings = new DrawRendererSettings(m_CurrCamera, m_DrawLights);
            opaqueDrawSettings.SetShaderPassName(1, m_DrawLights);
            opaqueDrawSettings.sorting.flags = SortFlags.CommonOpaque;

            var opaqueFilterSettings = new FilterRenderersSettings(true)
            {
                renderQueueRange = RenderQueueRange.opaque,
            };

            context.DrawRenderers(m_CullResults.visibleRenderers, ref opaqueDrawSettings, opaqueFilterSettings);

            if (asset.Mode == FlatLightRPAsset.DrawMode.Lights)
            {
                cmdBuf.Clear();
                cmdBuf.Blit(lights, ColorRT);
                context.ExecuteCommandBuffer(cmdBuf);
                CmdBufferPool.Release(cmdBuf);
                asset.LightsRenderTime = lightsTime.Result();
                return;
            }

            asset.LightsRenderTime = lightsTime.Result();
            var compositeTime = TimeSample.Sample("Composite Sample");

            // composite
            LightsComposite.SetVector(TexelSize, downSampleTexelSize);
            LightsComposite.SetVector(ENVLightOffset, offset);
            LightsComposite.SetColor(Ambient, asset.GetAmbientColor());
            LightsComposite.SetFloat(EmissionBlend, asset.GetEmissionBlend());
            LightsComposite.SetFloat(SampleDistBlur, asset.GetSampleDistBlur());

            cmdBuf.Clear();

            if (asset.BlurLights && asset.BlurLightsIterations > 0)
            {
                cmdBuf.GetTemporaryRT(blurLights, width, height, 0);
                RenderTargetIdentifier dst = blurLightsRT;
                RenderTargetIdentifier src = lightsRT;
                RenderTargetIdentifier final = src;
                for (int i = 0; i < asset.BlurLightsIterations; ++i)
                {
                    cmdBuf.Blit(src, dst, LightsComposite, LIGHT_BLUR_PASS);
                    RenderTargetIdentifier tmp = dst;
                    final = dst;
                    dst = src;
                    src = tmp;
                }
                if (asset.AmbientPass)
                {
                    cmdBuf.Blit(final, ambientRT, LightsComposite, AMBIENT_PASS);
                    cmdBuf.SetGlobalTexture(envLightsTex, ambientRT);
                }
                else
                    cmdBuf.SetGlobalTexture(envLightsTex, final);
            }
            else if(asset.AmbientPass)
            {
                cmdBuf.Blit(lightsRT, ambientRT, LightsComposite, AMBIENT_PASS);
                cmdBuf.SetGlobalTexture(envLightsTex, ambientRT);
            }
            else
                cmdBuf.SetGlobalTexture(envLightsTex, lightsRT);

            if (asset.AmbientPass)
            {
                if (asset.Mode == FlatLightRPAsset.DrawMode.Ambient)
                {
                    cmdBuf.Blit(ambientRT, ColorRT);
                    context.ExecuteCommandBuffer(cmdBuf);
                    CmdBufferPool.Release(cmdBuf);
                    asset.CompositeRenderTime = compositeTime.Result();
                    return;
                }

                cmdBuf.Blit(screenColorRT, BuiltinRenderTextureType.CameraTarget, LightsComposite, COMPOSITE_PASS);
            }
            else
            {
                cmdBuf.Blit(screenColorRT, BuiltinRenderTextureType.CameraTarget, LightsComposite, COMPOSITE_AMBIENT_PASS);
            }

            context.ExecuteCommandBuffer(cmdBuf);
            CmdBufferPool.Release(cmdBuf);
            asset.CompositeRenderTime = compositeTime.Result();
        }

        private void RenderUnlit(ref ScriptableRenderContext context, bool clear)
        {
            var time = TimeSample.Sample("Unlit Sample");
            if (clear)
            {
                CommandBuffer cmd = CmdBufferPool.Get("Unlit");
                SetRT(cmd, BuiltinRenderTextureType.CameraTarget, m_CurrCamera.backgroundColor.linear, true, true);
                context.ExecuteCommandBuffer(cmd);
                CmdBufferPool.Release(cmd);
            }

            var opaqueDrawSettings = new DrawRendererSettings(m_CurrCamera, m_DrawUnlt);
            opaqueDrawSettings.sorting.flags = SortFlags.CommonOpaque;

            var opaqueFilterSettings = new FilterRenderersSettings(true)
            {
                renderQueueRange = RenderQueueRange.opaque,
            };

            context.DrawRenderers(m_CullResults.visibleRenderers, ref opaqueDrawSettings, opaqueFilterSettings);
            asset.UnlitRenderTime = time.Result();
        }

        private void ClearResources(ref ScriptableRenderContext context)
        {
            var cmdBuf = CmdBufferPool.Get("release resources");
            cmdBuf.ReleaseTemporaryRT(obstracle);
            cmdBuf.ReleaseTemporaryRT(lights);
            cmdBuf.ReleaseTemporaryRT(ambient);
            cmdBuf.ReleaseTemporaryRT(blurLights);
            cmdBuf.ReleaseTemporaryRT(screenColor);
            context.ExecuteCommandBuffer(cmdBuf);
            CmdBufferPool.Release(cmdBuf);
        }

    }
}