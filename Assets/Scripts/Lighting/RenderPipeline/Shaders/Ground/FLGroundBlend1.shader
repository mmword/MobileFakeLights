﻿Shader "FlatLightRP/Ground/FLGroundBlend1"
{
	Properties
	{
		_Tex1 ("Albedo + Height", 2D) = "white" {}
	    _Tint1 ("Tint", Color) = (1, 1, 1, 1)
		_TexScale1 ("Texture Scale", Float) = 1 
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
			#define GROUND_FRAG_1
			#pragma vertex GroundVert
			#pragma fragment GroundFrag1
			#pragma shader_feature __ _NORMALMAP
			#pragma shader_feature __ _FLOW1
			#pragma shader_feature __ _FLOWDRIFT
			#pragma shader_feature __ _FLOWREFRACTION
			#include "FLGroundInc.cginc"
			ENDCG
		}
	}
	CustomEditor "FLGroundGUI"
}
