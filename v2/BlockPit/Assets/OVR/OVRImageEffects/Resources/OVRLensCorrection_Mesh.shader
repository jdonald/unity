//************************************************************************************
//
// Filename    :   OVRLensCorrection_Mesh.shader
// Content     :   Full screen shader
//				   This shader warps the final camera image to match the lens curvature on the Rift.
// Created     :   February 14, 2014
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

Shader "Custom/OVRLensCorrection_Mesh"
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
		float2 uv  : TEXCOORD0;
		float4 c   : COLOR0;
	};
	
	sampler2D _MainTex;
	
	float2 _DMScale;
	float2 _DMOffset;
	
	v2f vert( appdata_img v ) 
	{
		v2f o;
		
		o.pos = v.vertex;
		o.uv  = v.texcoord.xy * _DMScale + _DMOffset;
		o.c   = o.pos.z;
		
		return o;
	} 
		
	half4 frag(v2f i) : COLOR 
	{
    	return tex2D(_MainTex, i.uv) * i.c;
	}

	ENDCG 
	
Subshader 
{
 Pass 
 {
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