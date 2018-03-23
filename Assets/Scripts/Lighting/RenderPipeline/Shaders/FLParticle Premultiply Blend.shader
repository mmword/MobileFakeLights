// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "FlatLightRP/FLAlpha Blended Premultiply" {
Properties {
    _MainTex ("Particle Texture", 2D) = "white" {}
    _LightScale ("Light Scale", Range(0.01,3.0)) = 1.0
	_DistToPlaneFade("Dist To Plane Fade", Range(0.01,3.0)) = 1.0
	_LightIntensity("Light Intensity",Range(0.0,3.0)) = 1.0
	_ParticleIntensity("Particle Intensity",Range(0.0,3.0)) = 1.0
}

Category {
    //Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
	Tags { "RenderType"="Opaque" "RenderPipeline" = "FlatLightRenderPipeline" }
    ColorMask RGB
    Cull Off Lighting Off ZWrite Off
	
	CGINCLUDE
	
	#include "FLInc.cginc"

	float4 _MainTex_ST;
	sampler2D _MainTex;
	fixed4 _TintColor;


	struct appdata_t {
		float4 vertex : POSITION;
		fixed4 color : COLOR;
		float2 texcoord : TEXCOORD0;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};
	
	struct v2f {
		float4 vertex : SV_POSITION;
		fixed4 color : COLOR;
		float2 texcoord : TEXCOORD0;
	};
	
	ENDCG

    SubShader {
        Pass {
			Blend One OneMinusSrcAlpha
			Tags{ "LightMode" = "FLLitLight" } // draw light
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_particles
			#pragma multi_compile_instancing

			half _LightScale;
			half _DistToPlaneFade;
			half _LightIntensity;

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
				float3 vert = v.vertex;
				float2 offset = v.texcoord * 2 - 1;
				vert.xyz += UNITY_MATRIX_V[0].xyz * _LightScale * v.color.a * offset.x;
				vert.xyz += UNITY_MATRIX_V[1].xyz * _LightScale * v.color.a * offset.y;
                o.vertex = UnityObjectToClipPos(vert);
                o.color = max(0,v.color - _DistToPlaneFade * v.vertex.y);
				o.color.a = v.color.a;
                o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                half4 col = i.color * tex2D(_MainTex, i.texcoord) * i.color.a * _LightIntensity;
				return OutputColorSpace(col);
            }
            ENDCG
        }
		
		Pass {
			Blend SrcAlpha OneMinusSrcAlpha
			Tags{ "LightMode" = "FLUnlit" } // draw point // "RenderType"="Transparent"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_particles
			
			half _ParticleIntensity;

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                half4 col = i.color * tex2D(_MainTex, i.texcoord) * i.color.a * _ParticleIntensity;
				return OutputColorSpace(col);
            }
            ENDCG
        }
    }
}
}
