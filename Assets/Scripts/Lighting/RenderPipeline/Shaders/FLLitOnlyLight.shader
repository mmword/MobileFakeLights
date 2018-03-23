Shader "FlatLightRP/FLLitOnlyLight"
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

		// color pass
		Pass
		{
			Blend OneMinusDstColor One
			Tags{ "LightMode" = "FLLitLight" }

			CGPROGRAM
			#pragma vertex vert_l
			#pragma fragment frag_l
			#pragma shader_feature _ _INTENSITY
			#define NUM_STEPS 1
			#include "FLLightTracingBase.cginc"
			
			struct v2fo
			{
				half4 uv : TEXCOORD0;
				half4 fPos : TEXCOORD1;
				half3 wDir : TEXCOORD2;
				half4 vcol : TEXCOORD3;
				half4 params : TEXCOORD4;
				float4 vertex : SV_POSITION;
			};
			
			v2fo vert_l (appdata v)
			{
				v2fo o;
				o.vertex = OnGridPos(v.vertex);
				o.uv.xy = TRANSFORM_TEX(v.uv.xy, _MainTex);
				o.uv.zw = v.uv.zw;
				o.vcol = v.color;
				o.fPos = ComputeScreenPos(o.vertex);
				o.wDir = normalize(v.cPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz);
				o.params = v.params;
				return o;
			}
			
			fixed4 frag_l (v2fo i) : SV_Target
			{
				fixed4 vcol = i.vcol;
				fixed4 col = tex2D(_MainTex, i.uv.xy);
				half3 fPos = i.fPos.xyz;///i.fPos.w; - needed for perspective proj
				half4 obstracleSrc = tex2D(ENVObstracleTex, fPos);
				
				const half brightness = i.params.w;
				const half nlIntensity = i.params.z;

				half3 dir = i.wDir;
				half4 data = obstracleSrc;
				half obstracle = 1-data.w;
				half nl = max(0,dot(dir,data.xyz*2-1)) * obstracle;
				half shadowRes = saturate(1-(nl * nlIntensity));
				half3 color = col.rgb * vcol.rgb * vcol.a * brightness;

				#if defined(_INTENSITY)
				color *= (sin(_Time.w*i.uv.z*10)*i.uv.w*5+0.5);
				#endif

				half3 resultColor = color * shadowRes;
				OUTPUT_COLOR(half4(resultColor,1));
			}
			ENDCG
		}
		
		
	}
	CustomEditor "FLLitLightGUI"
}
