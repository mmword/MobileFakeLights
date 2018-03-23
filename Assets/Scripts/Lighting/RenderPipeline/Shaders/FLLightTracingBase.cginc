#ifndef FLLIGHT_TRACE_BASE
#define FLLIGHT_TRACE_BASE

#pragma glsl_no_auto_normalization

#include "FLInc.cginc"

struct appdata
{
	float4 vertex : POSITION;
	half4 uv : TEXCOORD0;
	half3 cPos : TEXCOORD1;
	half4 params : TEXCOORD2;
	half4 color : COLOR0;
};

struct v2f
{
	half4 uv : TEXCOORD0;
	half4 cPos : TEXCOORD1;
	half4 fPos : TEXCOORD2;
	half3 wDir : TEXCOORD3;
	half4 vcol : TEXCOORD4;
	half4 params : TEXCOORD5;
	float4 vertex : SV_POSITION;
};

sampler2D _MainTex;
sampler2D ENVObstracleTex;
float4 _MainTex_ST;
float4x4 _GridPos;

#define STEP_SIZE 1.0 / NUM_STEPS

inline float4 OnGridPos(float4 pos)
{
	pos = mul(unity_ObjectToWorld,float4(pos.xyz,1));
	return mul(_GridPos,pos);
}

v2f vert (appdata v)
{
	v2f o;
	//o.vertex = UnityObjectToClipPos(v.vertex);
	o.vertex = OnGridPos(v.vertex);
	o.uv.xy = TRANSFORM_TEX(v.uv.xy, _MainTex);
	o.uv.zw = v.uv.zw;
	o.fPos = ComputeScreenPos(o.vertex);
	o.cPos = ComputeScreenPos(mul(UNITY_MATRIX_VP,float4(v.cPos,1))); // need store in to uv1 for avoid batching problems
	o.wDir = normalize(v.cPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz);
	o.vcol = v.color;
	o.params = v.params;
	
	return o;
}


fixed4 frag (v2f i) : SV_Target
{	
	// sample the texture
	fixed4 vcol = i.vcol;
	fixed4 col = tex2D(_MainTex, i.uv.xy);
	half3 fPos = i.fPos.xyz;///i.fPos.w; - needed for perspective proj
	half3 cPos = i.cPos.xyz;///i.cPos.w; - needed for perspective proj
	half aspect = half2(_ScreenParams.x/_ScreenParams.y, 1);
	half sub = STEP_SIZE;
	half len = length((fPos - cPos)*aspect);
	//half m = 0.5*sub*len;
	half pos = 0;
	half shadow = 0;
	half4 obstracleSrc = tex2D(ENVObstracleTex, fPos);
	
	for(int k = 0; k < NUM_STEPS; k++)
	{
		pos += sub; 
		half4 obstacle = tex2D(ENVObstracleTex, lerp(cPos, fPos, pos));
		shadow = min(1,shadow + (1-obstacle.a));
	}
		
	const half Shadowness = i.params.x;
	const half FadeOut = i.params.y;
	const half brightness = i.params.w;
	const half nlIntensity = i.params.z;
	
	half3 dir = i.wDir;//normalize(i.p0-i.p1);
	half4 data = obstracleSrc;
	half obstracle = 1-data.w;
	half nl = max(0,dot(dir,data.xyz*2-1)) * obstracle;
	half shadowRes = saturate((1-shadow) + nl * nlIntensity);
	half3 color = col.rgb * vcol.rgb * vcol.a * brightness;
	
	#if defined(_INTENSITY)
	color *= (sin(_Time.w*i.uv.z*10)*i.uv.w*5+0.5);
	#endif

	half3 resultColor = lerp(color,color * shadowRes,Shadowness);
	resultColor = lerp(resultColor,(half3)0,pow(len,FadeOut));
	
    OUTPUT_COLOR(half4(resultColor,1));
}


#endif