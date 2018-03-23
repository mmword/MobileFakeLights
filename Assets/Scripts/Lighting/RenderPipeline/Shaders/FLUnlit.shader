Shader "FlatLightRP/FLUnlit"
{
	Properties
	{
		//base
		_Color("Color", Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		//alpha
		_Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
		_Intensity("Intensity",Float) = 1
		// Blending state
        [HideInInspector] _Mode("__mode", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
		[HideInInspector] _Billboard("__billboard", Float) = 0.0
		
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "RenderPipeline" = "FlatLightRenderPipeline" }
		LOD 100

		// color pass
		Pass
		{
			Tags{ "LightMode" = "FLUnlit" }
			Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON
			#pragma shader_feature _ _BILLBOARD_ON
			#pragma multi_compile_instancing
			#include "FLInc.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			half4 _Color;
			half _Intensity;

			v2f vert (appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				float4 vert = v.vertex;
				#if defined(_BILLBOARD_ON)
				vert.xyz = Billboard(v.uv);
				#endif
				o.vertex = UnityObjectToClipPos(vert);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				col = col * _Color;
				return OutputColorSpaceWithAlpha(col * _Intensity,col.a);
			}
			ENDCG
		}
		
		
	}
	CustomEditor "FLLightingGUI"
}
