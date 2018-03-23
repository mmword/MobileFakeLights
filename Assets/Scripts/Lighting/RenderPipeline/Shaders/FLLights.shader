Shader "FlatLightRP/FLLight"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		_intensity ("intensity", Vector) = (0,0,0,0)
		[HideInInspector] _IntensityMode("__intensitymode", Float) = 0.0
		[HideInInspector] _TraceMode("__traceMode", Float) = 0.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "RenderPipeline" = "FlatLightRenderPipeline" }
		LOD 100
		Cull Off
		ZWrite Off
		Lighting Off

		Pass
		{
			Blend OneMinusDstColor One
			Tags{ "LightMode" = "FLLitLight" }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature _ _INTENSITY
			#pragma shader_feature _ _TRACE10 _TRACE20 _TRACE30 _TRACE40 _TRACE50
			#if defined(_TRACE10)
			#define NUM_STEPS 10
			#elif defined(_TRACE20)
			#define NUM_STEPS 20
			#elif defined(_TRACE30)
			#define NUM_STEPS 30
			#elif defined(_TRACE40)
			#define NUM_STEPS 40
			#elif defined(_TRACE50)
			#define NUM_STEPS 50
			#else
			#define NUM_STEPS 10
			#endif
			#include "FLLightTracingBase.cginc"
			ENDCG
		}
	}
	CustomEditor "FLLitLightGUI"
}
