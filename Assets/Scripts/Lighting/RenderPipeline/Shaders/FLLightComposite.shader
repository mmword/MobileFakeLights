Shader "FlatLightRP/FLLightComposite"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Ambient("Ambient",Color) = (0,0,0,0)
		_SampleDistBlur("Blur Sample Dist",Range(0.1,3)) = 1
		_SampleDistEmission("Emission Sample Dist",Range(0.1,3)) = 1
		_EmissionBlend("Emission Blend",Range(0,1)) = 0.25
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always
		
		CGINCLUDE
		
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};
			
			sampler2D _MainTex;
			sampler2D ENVLightTex;
			//sampler2D ENVObstracleTex;
			
		ENDCG
		
		Pass // Invisible spaces pass
		{
			Blend DstColor Zero
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#define NUM_STEPS 80
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag (v2f vIn) : SV_Target
			{
				half pos = 0;
				half shadow = 0;
				half sub = 1.0/NUM_STEPS;
				fixed4 obstracleSrc = tex2D(_MainTex, vIn.uv);
				for(int i = 0; i < NUM_STEPS; i++)
				{
					pos += sub; 
					half4 obstacle = tex2D(_MainTex,lerp( half2(0.5,0.5),vIn.uv,pos));
					shadow = min(1,shadow + (1-obstacle.a));
				}
				//shadow = (1 - (shadow * obstracleSrc.r));
				shadow = 1 - shadow;
				return shadow;
			}
			ENDCG
		}
		
		Pass // Blur pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			half2 _TexelSize;
			half _SampleDistBlur;
			
			struct v2f_a
			{
				half2 uv : TEXCOORD0;
				half4 texs0 : TEXCOORD1;
				half4 texs1 : TEXCOORD2;
				float4 vertex : SV_POSITION;
			};
			
			v2f_a vert (appdata v)
			{
				v2f_a o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				half2 uv = v.uv;
				half3 d = _TexelSize.xyx * half3(1.0, 1.0, 0.0);
				o.texs0.xy = uv - d.xz * _SampleDistBlur;
				o.texs0.zw = uv + d.xz * _SampleDistBlur;
				o.texs1.xy = uv - d.zy * _SampleDistBlur;
				o.texs1.zw = uv + d.zy * _SampleDistBlur;
				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag (v2f_a i) : SV_Target
			{
				//half4 obstacle = tex2D(ENVObstracleTex, i.uv); 
				
				half4 col = tex2D(_MainTex,i.uv);
				col += tex2D(_MainTex,i.texs0.xy);
				col += tex2D(_MainTex,i.texs0.zw);
				col += tex2D(_MainTex,i.texs1.xy);
				col += tex2D(_MainTex,i.texs1.zw);
				col /= 5;
				
				col = col;// * obstacle.r;
				return col;
			}
			ENDCG
		}
		
		Pass // Ambient pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			half2 _TexelSize;
			half4 _Ambient;
			half _SampleDistEmission;
			half _EmissionBlend;
			
			struct v2f_a
			{
				half2 uv : TEXCOORD0;
				half4 texs0 : TEXCOORD1;
				half4 texs1 : TEXCOORD2;
				float4 vertex : SV_POSITION;
			};
			
			v2f_a vert (appdata v)
			{
				v2f_a o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texs0.xy = v.uv + _TexelSize * _SampleDistEmission;
				o.texs0.zw = v.uv - _TexelSize * _SampleDistEmission;
				o.texs1.xy = v.uv + _TexelSize * _SampleDistEmission;
				o.texs1.zw = v.uv - _TexelSize * _SampleDistEmission;
				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag (v2f_a i) : SV_Target
			{
				half4 col = tex2D(_MainTex,i.uv);
				half4 srcCol = col;
				col = min(col,tex2D(_MainTex,i.texs0.xy));
				col = min(col,tex2D(_MainTex,i.texs0.zw));
				col = min(col,tex2D(_MainTex,i.texs1.xy));
				col = min(col,tex2D(_MainTex,i.texs1.zw));

				col.rgb = saturate(col.rgb + _Ambient.rgb * _Ambient.a);
				return lerp(srcCol,col,_EmissionBlend);
			}
			ENDCG
		}
		
		Pass // Composite pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			float2 ENVLightOffset;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 scene = tex2D(_MainTex, i.uv);
				half4 lights = tex2D(ENVLightTex,i.uv + ENVLightOffset);
				half3 col = (scene.rgb * lights.rgb);
				return fixed4(col,1);
			}
			ENDCG
		}
		
		Pass // Composite Ambient pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			float2 ENVLightOffset;
			half4 _Ambient;
			half _EmissionBlend;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 scene = tex2D(_MainTex, i.uv);
				half4 lights = tex2D(ENVLightTex,i.uv + ENVLightOffset);
				half3 col = (scene.rgb * lights.rgb);
				half3 amb = saturate(col.rgb + _Ambient.rgb * _Ambient.a);
				col = lerp(col,amb,_EmissionBlend);
				return fixed4(col,1); 
			}
			ENDCG
		}
		
		
	}
}
