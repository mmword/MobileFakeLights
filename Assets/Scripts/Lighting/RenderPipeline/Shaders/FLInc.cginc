#ifndef FLINC
#define FLINC

#include "UnityCG.cginc"

#if defined(UNITY_COLORSPACE_GAMMA) && defined(GAMMA_TO_LINEAR)
    #define FL_GAMMA_TO_LINEAR(gammaColor) gammaColor * gammaColor
    #define FL_LINEAR_TO_GAMMA(linColor) sqrt(linColor)
    #define OUTPUT_COLOR(color) return half4(FL_LINEAR_TO_GAMMA(color.rgb), color.a)
#else
    #define FL_GAMMA_TO_LINEAR(color) color
    #define FL_LINEAR_TO_GAMMA(color) color
    #define OUTPUT_COLOR(color) return color
#endif

#if defined(_ALPHATEST_ON)
half _Cutoff;
#endif
			
float3 Billboard(float2 uv)
{
	float2 offset = uv * 2 - 1;
	return (UNITY_MATRIX_V[0].xyz * offset.x + UNITY_MATRIX_V[1].xyz * offset.y) * 0.5;
}

half3 SafeNormalize(half3 inVec)
{
    half dp3 = max(1.e-4h, dot(inVec, inVec));
    return inVec * rsqrt(dp3);
}

half4 OutputColorSpaceWithAlpha(half4 output, half alphaFromSurface)
{
	#if defined(_ALPHATEST_ON)
		clip (alphaFromSurface - _Cutoff);
	#endif
	#if defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
		output.a = alphaFromSurface;
	#else
		UNITY_OPAQUE_ALPHA(output.a);
	#endif
    OUTPUT_COLOR(output);
}

half4 OutputColorSpace(half4 output)
{
	OUTPUT_COLOR(output);
}

#endif
