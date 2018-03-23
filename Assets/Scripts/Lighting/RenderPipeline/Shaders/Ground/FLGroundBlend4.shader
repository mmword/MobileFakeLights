Shader "FlatLightRP/Ground/FLGroundBlend4"
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
		_Tex3("Albedo + Height", 2D) = "white" {}
		_Tint3 ("Tint", Color) = (1, 1, 1, 1)
		_TexScale3 ("Texture Scale", Float) = 1
		_Contrast3("Contrast", Range(0,0.99)) = 0.5
		_Tex4("Albedo + Height", 2D) = "white" {}
		_Tint4 ("Tint", Color) = (1, 1, 1, 1)
		_TexScale4 ("Texture Scale", Float) = 1
		_Contrast4("Contrast", Range(0,0.99)) = 0.5
		[NoScaleOffset][Normal]_NormalTex("Normal", 2D) = "bump" {}
		_FlowSpeed ("Flow Speed", Float) = 0
		_FlowIntensity ("Flow Intensity", Float) = 1
		_FlowAlpha ("Flow Alpha", Range(0, 1)) = 1
		_FlowRefraction("Flow Refraction", Range(0, 0.3)) = 0.04
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "RenderPipeline" = "FlatLightRenderPipeline" }
		LOD 100

		Pass
		{
		    //Tags{ "LightMode" = "FLGround" }
			Tags{ "LightMode" = "FLLit" }
			CGPROGRAM
			#define GROUND_FRAG_4
			#pragma vertex GroundVert
			#pragma fragment GroundFrag4
			#pragma shader_feature __ _NORMALMAP
			#pragma shader_feature __ _FLOW1 _FLOW2 _FLOW3 _FLOW4
			#pragma shader_feature __ _FLOWDRIFT
			#pragma shader_feature __ _FLOWREFRACTION
			#include "FLGroundInc.cginc"		
			ENDCG
		}
	}
	CustomEditor "FLGroundGUI"
}
