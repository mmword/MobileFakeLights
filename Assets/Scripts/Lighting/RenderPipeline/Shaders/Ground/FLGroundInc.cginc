#ifndef FL_GROUND_INC
#define FL_GROUND_INC

#include "UnityCG.cginc"

#ifdef _NORMALMAP
sampler2D _NormalTex;
#endif

#ifdef GROUND_FRAG_1
sampler2D _Tex1;
half _TexScale1;
half4 _Tint1;
#elif defined(GROUND_FRAG_2)
sampler2D _Tex1,_Tex2;
half _TexScale1,_TexScale2;
half _Contrast2;
half4 _Tint1,_Tint2;
#elif defined(GROUND_FRAG_3)
sampler2D _Tex1,_Tex2,_Tex3;
half _TexScale1,_TexScale2,_TexScale3;
half _Contrast2,_Contrast3;
half4 _Tint1,_Tint2,_Tint3;
#elif defined(GROUND_FRAG_4)
sampler2D _Tex1,_Tex2,_Tex3,_Tex4;
half _TexScale1,_TexScale2,_TexScale3,_TexScale4;
half _Contrast2,_Contrast3,_Contrast4;
half4 _Tint1,_Tint2,_Tint3,_Tint4;
#endif


struct appdata
{
	float4 vertex : POSITION;
	float4 color : COLOR;
	float4 texcoord : TEXCOORD0;
};

struct PsInput 
{
   float4 vertex : SV_POSITION;
   float4 color : TEXCOORD0;
   #if (_FLOW1 || _FLOW2 || _FLOW3 || _FLOW4 || _FLOW5)
   float4 flowDir : TEXCOORD1;
   float2 uv_Tex1 : TEXCOORD2;
   #else
   float2 uv_Tex1 : TEXCOORD1;
   #endif
};


half  _FlowSpeed;
half  _FlowIntensity;
fixed _FlowAlpha;
half  _FlowRefraction;

// macros to make dealing with the flow map option not a nightmare
#if (_FLOW1 || _FLOW2 || _FLOW3 || _FLOW4 || _FLOW5)
#define INIT_FLOW half flowInterp; float2 fuv1; float2 fuv2; Flow(IN.flowDir.xy, IN.flowDir.zw, _FlowSpeed, _FlowIntensity, fuv1, fuv2, flowInterp);
#else
#define INIT_FLOW  
#endif

// we define the function based on what channel is actively compiled for flow data - other combinations else into a standard tex2D
#if _FLOW1
#define FETCH_TEX1(_T, _UV) lerp(tex2D(_T, fuv1), tex2D(_T, fuv2), flowInterp)
#elif _DISTBLEND
#define FETCH_TEX1(_T, _UV) lerp(tex2D(_T, _UV), tex2D(_T, _UV*_DistUVScale1), dist)
#else
#define FETCH_TEX1(_T, _UV) tex2D(_T, _UV)
#endif

#if _FLOW2
#define FETCH_TEX2(_T, _UV) lerp(tex2D(_T, fuv1), tex2D(_T, fuv2), flowInterp)
#elif _DISTBLEND
#define FETCH_TEX2(_T, _UV) lerp(tex2D(_T, _UV), tex2D(_T, _UV*_DistUVScale2), dist)
#else
#define FETCH_TEX2(_T, _UV) tex2D(_T, _UV)
#endif

#if _FLOW3
#define FETCH_TEX3(_T, _UV) lerp(tex2D(_T, fuv1), tex2D(_T, fuv2), flowInterp)
#elif _DISTBLEND
#define FETCH_TEX3(_T, _UV) lerp(tex2D(_T, _UV), tex2D(_T, _UV*_DistUVScale3), dist)
#else
#define FETCH_TEX3(_T, _UV) tex2D(_T, _UV)
#endif

#if _FLOW4
#define FETCH_TEX4(_T, _UV) lerp(tex2D(_T, fuv1), tex2D(_T, fuv2), flowInterp)
#elif _DISTBLEND
#define FETCH_TEX4(_T, _UV) lerp(tex2D(_T, _UV), tex2D(_T, _UV*_DistUVScale4), dist)
#else
#define FETCH_TEX4(_T, _UV) tex2D(_T, _UV)
#endif

#if _FLOW5
#define FETCH_TEX5(_T, _UV) lerp(tex2D(_T, fuv1), tex2D(_T, fuv2), flowInterp)
#elif _DISTBLEND
#define FETCH_TEX5(_T, _UV) lerp(tex2D(_T, _UV), tex2D(_T, _UV*_DistUVScale5), dist)
#else
#define FETCH_TEX5(_T, _UV) tex2D(_T, _UV)
#endif  

// given two height values (from textures) and a height value for the current pixel (from vertex)
// compute the blend factor between the two with a small blending area between them.
half HeightBlend(half h1, half h2, half slope, half contrast)
{
   h2 = 1-h2;
   half tween = saturate( ( slope - min( h1, h2 ) ) / max(abs( h1 - h2 ), 0.001)); 
   half threshold = contrast;
   half width = 1.0 - contrast;
   return saturate( ( tween - threshold ) / max(width, 0.001) );
}

void Flow(float2 uv, half2 flow, half speed, float intensity, out float2 uv1, out float2 uv2, out half interp)
{
   float2 flowVector = (flow * 2.0 - 1.0) * intensity;
   
   float timeScale = _Time.y * speed;
   float2 phase = frac(float2(timeScale, timeScale + .5));

   uv1 = (uv - flowVector * half2(phase.x, phase.x));
   uv2 = (uv - flowVector * half2(phase.y, phase.y));
   
   interp = abs(0.5 - phase.x) / 0.5;
}

PsInput GroundVert (appdata v) 
{
	PsInput o;
    #if (_FLOW1 || _FLOW2 || _FLOW3 || _FLOW4 || _FLOW5)
    //o.flowDir.xy = v.texcoord.xy;
    //o.flowDir.zw = v.texcoord.zw;
	o.flowDir = v.texcoord;
    #endif
    
    #if (_FLOW1)
    o.flowDir.xy *= _TexScale1;
    #endif
    #if (_FLOW2)
    o.flowDir.xy *= _TexScale2;
    #endif
    #if (_FLOW3)
    o.flowDir.xy *= _TexScale3; 
    #endif
    #if (_FLOW4)
    o.flowDir.xy *= _TexScale4;
    #endif
    #if (_FLOW5)
    o.flowDir.xy *= _TexScale5;
    #endif

	o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv_Tex1 = v.texcoord.xy;
    o.color = v.color;
	return o;
}

#ifdef GROUND_FRAG_1

fixed4 GroundFrag1Pass (PsInput IN)
{
	 float2 uv1 = IN.uv_Tex1 * _TexScale1;
	 INIT_FLOW
	 #if _FLOWDRIFT
	 fixed4 c1 = FETCH_TEX1(_Tex1, uv1);
	 #else
	 fixed4 c1 = tex2D(_Tex1, uv1);
	 #endif
	 fixed4 c = c1 * _Tint1;
	 c.a = 1;
	 return c;
}

inline fixed4 GroundFrag1(PsInput IN) : SV_Target
{
	return GroundFrag1Pass(IN);
}

#elif defined(GROUND_FRAG_2)

fixed4 GroundFrag2Pass (PsInput IN)
{
	 float2 uv1 = IN.uv_Tex1 * _TexScale1;
	 float2 uv2 = IN.uv_Tex1 * _TexScale2;
	 INIT_FLOW
	 #if _FLOWDRIFT
	 fixed4 c1 = FETCH_TEX1(_Tex1, uv1);
	 fixed4 c2 = FETCH_TEX2(_Tex2, uv2);
	 #else
	 fixed4 c1 = tex2D(_Tex1, uv1);
	 fixed4 c2 = tex2D(_Tex2, uv2);
	 #endif
	 half b1 = HeightBlend(c1.a, c2.a, IN.color.r, _Contrast2);

	 // flow refraction; use difference in depth to control refraction amount, refetch all previous color textures if not parallaxing
	 #if _FLOW2
		b1 *= _FlowAlpha;
		#if _FLOWREFRACTION && _NORMALMAP
		   half4 rn = FETCH_TEX2 (_NormalTex, uv2) - 0.5;
		   uv1 += rn.xy * b1 * _FlowRefraction;
		   c1 = FETCH_TEX1(_Tex1, uv1);
		#endif
	 #endif

	fixed4 c = lerp(c1 * _Tint1, c2 * _Tint2, b1);
	c.a = 1;
	return c;
}

inline fixed4 GroundFrag2(PsInput IN) : SV_Target
{
	return GroundFrag2Pass(IN);
}

#elif defined(GROUND_FRAG_3)

fixed4 GroundFrag3Pass (PsInput IN) : SV_Target
{
	 float2 uv1 = IN.uv_Tex1 * _TexScale1;
	 float2 uv2 = IN.uv_Tex1 * _TexScale2;
	 float2 uv3 = IN.uv_Tex1 * _TexScale3;
	 INIT_FLOW
	 #if _FLOWDRIFT
	 fixed4 c1 = FETCH_TEX1(_Tex1, uv1);
	 fixed4 c2 = FETCH_TEX2(_Tex2, uv2);
	 fixed4 c3 = FETCH_TEX3(_Tex3, uv3);
	 #else
	 fixed4 c1 = tex2D(_Tex1, uv1);
	 fixed4 c2 = tex2D(_Tex2, uv2);
	 fixed4 c3 = tex2D(_Tex3, uv3);
	 #endif
	 half b1 = HeightBlend(c1.a, c2.a, IN.color.r, _Contrast2);
	 fixed h1 =  lerp(c1.a, c2.a, b1);
	 half b2 = HeightBlend(h1, c3.a, IN.color.g, _Contrast3);

	 // flow refraction; use difference in depth to control refraction amount, refetch all previous color textures if not parallaxing
	 #if _FLOW2
		b1 *= _FlowAlpha;
		#if _FLOWREFRACTION && _NORMALMAP
		   half4 rn = FETCH_TEX2 (_NormalTex, uv2) - 0.5;
		   uv1 += rn.xy * b1 * _FlowRefraction;
		   c1 = FETCH_TEX1(_Tex1, uv1);
		#endif
	 #endif
	 #if _FLOW3
		b2 *= _FlowAlpha;
		#if _FLOWREFRACTION && _NORMALMAP
		   half4 rn = FETCH_TEX3 (_Normal3, uv3) - 0.5;
		   uv1 += rn.xy * b1 * _FlowRefraction;
		   uv2 += rn.xy * b2 * _FlowRefraction; 
		   c1 = FETCH_TEX1(_Tex1, uv1);
		   c2 = FETCH_TEX2(_Tex2, uv2);
		#endif
	 #endif

	fixed4 c = lerp(lerp(c1 * _Tint1, c2 * _Tint2, b1), c3 * _Tint3, b2);
	c.a = 1;
	return c;
}

inline fixed4 GroundFrag3(PsInput IN) : SV_Target
{
	return GroundFrag3Pass(IN);
}

#elif defined(GROUND_FRAG_4)

fixed4 GroundFrag4Pass (PsInput IN) : SV_Target
{
	 float2 uv1 = IN.uv_Tex1 * _TexScale1;
	 float2 uv2 = IN.uv_Tex1 * _TexScale2;
	 float2 uv3 = IN.uv_Tex1 * _TexScale3;
	 float2 uv4 = IN.uv_Tex1 * _TexScale4;
	 INIT_FLOW
	 #if _FLOWDRIFT
	 fixed4 c1 = FETCH_TEX1(_Tex1, uv1);
	 fixed4 c2 = FETCH_TEX2(_Tex2, uv2);
	 fixed4 c3 = FETCH_TEX3(_Tex3, uv3);
	 fixed4 c4 = FETCH_TEX4(_Tex4, uv4);
	 #else
	 fixed4 c1 = tex2D(_Tex1, uv1);
	 fixed4 c2 = tex2D(_Tex2, uv2);
	 fixed4 c3 = tex2D(_Tex3, uv3);
	 fixed4 c4 = tex2D(_Tex4, uv4);
	 #endif
	 half b1 = HeightBlend(c1.a, c2.a, IN.color.r, _Contrast2);
	 fixed h1 = lerp(c1.a, c2.a, b1);
	 half b2 = HeightBlend(h1, c3.a, IN.color.g, _Contrast3);
	 fixed h2 = lerp(h1, c2.a, b1);
	 half b3 = HeightBlend(h2, c4.a, IN.color.b, _Contrast4);

	 // flow refraction; use difference in depth to control refraction amount, refetch all previous color textures if not parallaxing
	 #if _FLOW2
		b1 *= _FlowAlpha;
		#if _FLOWREFRACTION && _NORMALMAP
		   half4 rn = FETCH_TEX2 (_NormalTex, uv2) - 0.5;
		   uv1 += rn.xy * b1 * _FlowRefraction;
		   c1 = FETCH_TEX1(_Tex1, uv1);
		#endif
	 #endif
	 #if _FLOW3
		b2 *= _FlowAlpha;
		#if _FLOWREFRACTION && _NORMALMAP
		   half4 rn = FETCH_TEX3 (_Normal3, uv3) - 0.5;
		   uv1 += rn.xy * b1 * _FlowRefraction;
		   uv2 += rn.xy * b2 * _FlowRefraction; 
		   c1 = FETCH_TEX1(_Tex1, uv1);
		   c2 = FETCH_TEX2(_Tex2, uv2);
		#endif
	 #endif
	#if _FLOW4
		b3 *= _FlowAlpha;
		#if _FLOWREFRACTION && _NORMALMAP
		   half4 rn = FETCH_TEX4 (_Normal4, uv4) - 0.5;
		   uv1 += rn.xy * b1 * _FlowRefraction;
		   uv2 += rn.xy * b2 * _FlowRefraction;
		   uv3 += rn.xy * b3 * _FlowRefraction;
		   c1 = FETCH_TEX1(_Tex1, uv1);
		   c2 = FETCH_TEX2(_Tex2, uv2);
		   c3 = FETCH_TEX3(_Tex3, uv3);
		#endif
	 #endif

	fixed4 c = lerp(lerp(lerp(c1 * _Tint1, c2 * _Tint2, b1), c3 * _Tint3, b2), c4 * _Tint4, b3);
	c.a = 1;
	return c;
}

inline fixed4 GroundFrag4(PsInput IN) : SV_Target
{
	return GroundFrag4Pass(IN);
}
#endif

#endif