//************************************************************************************
//
// Filename    :   OVRLensCorrection.shader
// Content     :   Full screen shader
//				   This shader warps the final camera image to match the lens curvature on the Rift.
// Created     :   January 17, 2013
// Authors     :   Peter Giokaris
//
// Copyright   :   Copyright 2014 Oculus VR, LLC. All Rights reserved.
//
// Licensed under the Oculus VR Rift SDK License Version 3.1 (the "License"); 
// you may not use the Oculus VR Rift SDK except in compliance with the License, 
// which is provided at the time of installation or download, or which 
// otherwise accompanies this software in either electronic or hard copy form.
//
// You may obtain a copy of the License at
//
// http://www.oculusvr.com/licenses/LICENSE-3.1 
//
// Unless required by applicable law or agreed to in writing, the Oculus VR SDK 
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
//************************************************************************************/

Shader "OVRLensCorrection" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "" {}
	}
	
	// Shader code pasted into all further CGPROGRAM blocks
	CGINCLUDE
	
	#include "UnityCG.cginc"
	
	struct v2f 
	{
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;
	};
	
	sampler2D _MainTex;
	
	v2f vert( appdata_img v ) 
	{
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv = v.texcoord.xy;
		return o;
	} 
	
	float2 _Center;
	float2 _ScaleIn;
    float2 _Scale;
    float4 _HmdWarpParam;
	
    // Scales input texture coordinates for distortion.
    // ScaleIn maps texture coordinates to Scales to ([-1, 1] * scaleFactor),
    // where scaleFactor compensates input for K1 and K2, to allow full screen size to be used.
    // Scale factor that fits into screen size can be determined by solving this
    // equation for Scale: 1 = Scale * (K0 + K1 * Scale^2 + K2 * Scale^4).    
    float2 HmdWarp(float2 in01)
    {
      float2 vecFromCenter = (in01 - _Center) * _ScaleIn; // Scales to [-1, 1] 
      float  rSq= vecFromCenter.x * vecFromCenter.x + vecFromCenter.y * vecFromCenter.y;
      float2 vecResult = vecFromCenter * (_HmdWarpParam.x + _HmdWarpParam.y * rSq + _HmdWarpParam.z * rSq * rSq);
      return _Center + _Scale * vecResult;
    }
	
	half4 GetColor(float2 uv)
	{
		float2 tc = HmdWarp(uv);
		
		if (any(clamp(tc, float2(0.0,0.0), float2(1.0, 1.0)) - tc))    
			return 0;
		else			
			return tex2D (_MainTex, tc);
	}
	
	half4 frag(v2f i) : COLOR 
	{
		float2 tc = i.uv;
		half4 c = 0;
		c += GetColor(tc);
		return c;
	}

	ENDCG 
	
Subshader {
 Pass {
	  ZTest Always Cull Off ZWrite Off
	  Fog { Mode off }      

      CGPROGRAM
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma vertex vert
      #pragma fragment frag
      ENDCG
  }
  
}

Fallback off
	
} // shader