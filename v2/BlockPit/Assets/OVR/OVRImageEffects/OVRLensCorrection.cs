/************************************************************************************

Filename    :   OVRLensCorrection.cs
Content     :   Full screen image effect. 
				This script is used to add full-screen lens correction on a camera
				component
Created     :   January 17, 2013
Authors     :   Peter Giokaris

Copyright   :   Copyright 2014 Oculus VR, LLC. All Rights reserved.

Licensed under the Oculus VR Rift SDK License Version 3.1 (the "License"); 
you may not use the Oculus VR Rift SDK except in compliance with the License, 
which is provided at the time of installation or download, or which 
otherwise accompanies this software in either electronic or hard copy form.

You may obtain a copy of the License at

http://www.oculusvr.com/licenses/LICENSE-3.1 

Unless required by applicable law or agreed to in writing, the Oculus VR SDK 
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

************************************************************************************/

using UnityEngine;
using System.Runtime.InteropServices;

[AddComponentMenu("Image Effects/OVRLensCorrection")]

//-------------------------------------------------------------------------------------
// ***** OVRLensCorrection
//
// OVRLensCorrection contains the variables required to set material properties
// for the lens correction image effect.
//
public class OVRLensCorrection : OVRImageEffectBase 
{
	public const string strOvrLib = "OculusPlugin";
	//// -- UnityAndroid - This function is stubbed for Android.
	[DllImport(strOvrLib)]
	static extern void GetFloatv(int index, float[] buffer);

	[HideInInspector]
	public Vector2 _Center       		= new Vector2(0.5f, 0.5f);
	[HideInInspector]
	public Vector2 _ScaleIn      		= new Vector2(1.0f,  1.0f);
	[HideInInspector]
	public Vector2 _Scale        		= new Vector2(1.0f, 1.0f);	
	[HideInInspector]
	public Vector4 _HmdWarpParam 		= new Vector4(1.0f, 0.0f, 0.0f, 0.0f);
	[HideInInspector]
	public Vector4 _ChromaticAberration = new Vector4(0.996f, 0.992f, 1.014f, 1.014f);
	[HideInInspector]
	public Vector2 _DMScale				= new Vector2(0.0f, 0.0f);
	[HideInInspector]
	public Vector2 _DMOffset			= new Vector2(0.0f, 0.0f);

	[HideInInspector]
	public float dynamicScale 			= 1.0f;

	//// -- UnityAndroid
	//
	// Automatically set up all the materials and shaders needed for an 
	// OVRImageEffectBase derived class
	void Awake() 
	{
		if ( material == null )
		{
			material = new Material ( Shader.Find( "OVRLensCorrection" ) );
		}
		if ( material_CA == null ) 
		{
			material_CA = new Material ( Shader.Find( "OVRLensCorrection_CA" ) );
		}
		if ( material_MeshDistort == null )
		{
			material_MeshDistort = new Material ( Shader.Find( "Custom/OVRLensCorrection_Mesh" ) );
		}
		if ( material_MeshDistort_CA == null )
		{
			material_MeshDistort_CA = new Material ( Shader.Find( "Custom/OVRLensCorrection_Mesh_CA" ) );
		}
		if ( material_MeshDistort_CA_TW == null )
		{
			material_MeshDistort_CA_TW = new Material ( Shader.Find( "Custom/OVRLensCorrection_Mesh_CA_TW" ) );
		}
	}
	//// -- UnityAndroid

	//// -- UnityAndroid
	// 
	// Clean up the materials we created
	void OnDestroy()
	{
		if ( material != null )
		{
			Destroy( material );
		}
		if ( material_CA != null ) 
		{
			Destroy( material_CA );
		}
		if ( material_MeshDistort != null )
		{
			Destroy( material_MeshDistort );
		}
		if ( material_MeshDistort_CA != null )
		{
			Destroy( material_MeshDistort_CA );
		}
		if ( material_MeshDistort_CA_TW != null )
		{
			Destroy( material_MeshDistort_CA_TW );
		}
	}
	//// -- UnityAndroid

	//
	// Called by camera to get lens correction values
	// Use default material for this type of lens correction
	public Material GetMaterial()
	{
		material.SetVector("_HmdWarpParam",	_HmdWarpParam);

		return material;
	}

	//
	// Called by camera to get lens correction values w/Chromatic aberration
	//// -- UnityAndroid
	[HideInInspector]
	[System.NonSerialized]
	//// -- UnityAndroid
	public Material material_CA;
	public Material GetMaterial_CA()
	{
		material_CA.SetVector("_HmdWarpParam",	      _HmdWarpParam);
		material_CA.SetVector("_ChromaticAberration", _ChromaticAberration);
		
		return material_CA;
	}

	//
	// Called by camera to get lens correction values for mesh distortion
	//// -- UnityAndroid
	[HideInInspector]
	[System.NonSerialized]
	//// -- UnityAndroid
	public Material material_MeshDistort;
	public Material GetMaterial_MeshDistort()
	{
		material_MeshDistort.SetVector("_DMScale",	_DMScale * dynamicScale);
		material_MeshDistort.SetVector("_DMOffset", _DMOffset);
		return material_MeshDistort;
	}

	//
	// Called by camera to get lens correction values for mesh distortion
	//// -- UnityAndroid
	[HideInInspector]
	[System.NonSerialized]
	//// -- UnityAndroid
	public Material material_MeshDistort_CA;
	public Material GetMaterial_MeshDistort_CA()
	{
		material_MeshDistort_CA.SetVector("_DMScale",  _DMScale * dynamicScale);
		Vector2 offset = _DMOffset + (dynamicScale - 1f) * new Vector2(0.25f, 0.5f);
		material_MeshDistort_CA.SetVector("_DMOffset", offset);

		return material_MeshDistort_CA;
	}	
	
	//
	// Called by camera to get lens correction values for mesh distortion with time warp
	//// -- UnityAndroid
	[HideInInspector]
	[System.NonSerialized]
	//// -- UnityAndroid
	public Material material_MeshDistort_CA_TW;
	public Material GetMaterial_MeshDistort_CA_TW()
	{
		material_MeshDistort_CA_TW.SetVector ("_DMScale",  _DMScale);
		material_MeshDistort_CA_TW.SetVector ("_DMOffset", _DMOffset);

		var buffer = new float[16];

		var timeWarpStart = new Matrix4x4();
		GetFloatv(0, buffer);
		for (int i = 0; i < 16; ++i)
			timeWarpStart[i/4, i%4] = buffer[i];
		material_MeshDistort_CA_TW.SetMatrix ("_TimeWarpStart", timeWarpStart);
		
		var timeWarpEnd = new Matrix4x4();
		GetFloatv(1, buffer);
		for (int i = 0; i < 16; ++i)
			timeWarpEnd[i/4, i%4] = buffer[i];
		material_MeshDistort_CA_TW.SetMatrix ("_TimeWarpEnd",   timeWarpEnd);
		
		return material_MeshDistort_CA_TW;
	}	
}