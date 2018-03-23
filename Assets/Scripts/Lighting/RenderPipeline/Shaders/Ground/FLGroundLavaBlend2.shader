Shader "FlatLightRP/Ground/FLGroundLavaBlend2"
{
	Properties
	{
		_Tex1 ("Albedo + Height", 2D) = "white" {}
	    _Tint1 ("Tint", Color) = (1, 1, 1, 1)
		_TexScale1 ("Texture Scale", Float) = 1 
		_Tex2("Albedo + Height", 2D) = "white" {}
		_Tint2 ("Tint", Color) = (1, 1, 1, 1)
		_TexScale2 ("Texture Scale", Float) = 1 
		_Contrast2("Contrast", Range(0,0.99)) = 0.5
		[NoScaleOffset][Normal]_NormalTex("Normal", 2D) = "bump" {}
		_FlowSpeed ("Flow Speed", Float) = 0
		_FlowIntensity ("Flow Intensity", Float) = 1
		_FlowAlpha ("Flow Alpha", Range(0, 1)) = 1
		_FlowRefraction("Flow Refraction", Range(0, 0.3)) = 0.04
		_FlowLightIntensity("Flow Light Intensity", Range(1, 5)) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "RenderPipeline" = "FlatLightRenderPipeline" }
		LOD 100

		Pass
		{
			Tags{ "LightMode" = "FLLit" }
			CGPROGRAM
			#define GROUND_FRAG_2
			#pragma vertex GroundVert
			#pragma fragment GroundFrag2
			#pragma shader_feature __ _NORMALMAP
			#pragma shader_feature __ _FLOW1 _FLOW2
			#pragma shader_feature __ _FLOWDRIFT
			#pragma shader_feature __ _FLOWREFRACTION
			#include "FLGroundInc.cginc"
			ENDCG
		}
		
		Pass
		{
			Cull Off
			ZWrite Off
			Lighting Off
			Blend OneMinusDstColor One
			Tags{ "LightMode" = "FLLitLight" }
			CGPROGRAM
			#define GROUND_FRAG_2
			#pragma vertex GroundVert
			#pragma fragment GroundFrag2Light
			#pragma shader_feature __ _NORMALMAP
			#pragma shader_feature __ _FLOW1 _FLOW2
			#pragma shader_feature __ _FLOWDRIFT
			#pragma shader_feature __ _FLOWREFRACTION
			#include "FLGroundInc.cginc"
			
			half _FlowLightIntensity;
			
			fixed4 GroundFrag2Light (PsInput IN) : SV_Target
			{
				float2 uv2 = IN.uv_Tex1 * _TexScale2;
				INIT_FLOW
				#if _FLOWDRIFT
				fixed4 c2 = FETCH_TEX2(_Tex2, uv2);
				#else
				fixed4 c2 = tex2D(_Tex2, uv2);
				#endif
				
				fixed4 c = c2 * IN.color.r * _FlowLightIntensity;
				c.a = 1;
				return c;
			}
			
			ENDCG
		}
		
	}
	CustomEditor "FLGroundGUI"
}
