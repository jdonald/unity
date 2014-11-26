/************************************************************************************

Filename    :   OVRCamera.cs
Content     :   Interface to camera class
Created     :   January 8, 2013
Authors     :   Peter Giokaris, David Borel

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
using System.Collections.Generic;
using System.Collections;
using System.Runtime.InteropServices;

[RequireComponent(typeof(Camera))]

/// <summary>
/// OVRCamera is used to render into a Unity Camera class. 
/// This component handles reading the Rift tracker and positioning the camera position
/// and rotation. It also is responsible for properly rendering the final output, which
/// also the final lens correction pass.
/// </summary>
public class OVRCamera : MonoBehaviour
{
	public const string strOvrLib = "OculusPlugin";
	[DllImport(strOvrLib)]
	static extern bool SampleStartRotation();

	#region Member Variables

	#pragma warning disable 414		// The private field 'x' is assigned but its value is never used

	// PRIVATE MEMBERS
	// We will search for camera controller and set it here for access to its members
	private OVRCameraController CameraController = null;

	// PUBLIC MEMBERS
	//// -- !UnityAndroid - Distortion Mesh is not used for Android.
	// DistortionMesh is faster then pixel shader distortion method
	public readonly OVRDistortionMesh eyeMesh = new OVRDistortionMesh();
	//// -- !UnityAndroid

	//// -- UnityAndroid
	public delegate void OnCustomPostRenderEventHandler();
	public static event OnCustomPostRenderEventHandler OnCustomPostRender;
	//// -- UnityAndroid

	// camera position,	from root of camera to neck (translation only)
	[HideInInspector]
	public Vector3 NeckPosition = new Vector3(0.0f, 0.0f,  0.0f);	
	// From neck to eye (rotation and translation; x will be different for each eye)
	[HideInInspector]
	public Vector3 EyePosition  = new Vector3(0.0f, 0.09f, 0.16f);

    // True if this camera corresponds to the right eye.
    public bool RightEye = false;

	// STATIC MEMBERS
	// We will grab the actual orientation that is used by the cameras in a shared location.
	// This will allow multiple OVRCameraControllers to eventually be used in a scene, and 
	// only one orientation will be used to syncronize all camera orientation
	static private Quaternion CameraOrientation = Quaternion.identity;
	//  This is absolute camera location from vision
	static private Vector3 CameraPosition = Vector3.zero;
	// Set an offset to the camera that will adjust our location from the 
	// cameras center of origin (set when reset is called)
	static private Vector3 CameraPositionOffset = Vector3.zero;
	// Set an offset to the camera that will adjust our rotation from the 
	// camera's default orientation (set when reset is called)
	static private Quaternion CameraOrientationOffset = Quaternion.identity;

	//// -- !UnityAndroid - CameraLocalSetList is not used for Android.
	// List of game objects to update with local camera center of origin
	// {allows for objects that are at 0,0,0 relative to the camera to stay 
	// rendered in camera space, to allow for visual aid).
	static private List<OVRCameraGameObject> CameraLocalSetList = new List<OVRCameraGameObject>();
	//// -- !UnityAndroid - CameraLocalSetList is not used for Android.

	//// -- !UnityAndroid - CameraTexture is not used for Android.
	// An optional texture to which the undistorted image will be rendered.
    static public RenderTexture[] CameraTexture = new RenderTexture[2];
	//// -- !UnityAndroid - CameraTexture is not used for Android.

	#pragma warning restore 414		// The private field 'x' is assigned but its value is never used

	#endregion
	
	#region Monobehaviour Member Functions
	/// <summary>
	/// Start
	/// </summary>
	void Start()
	{		
		// Get the OVRCameraController
		CameraController = transform.parent.GetComponent<OVRCameraController>();
		
		if(CameraController == null)
		{
			Debug.LogWarning("OVRCameraController not found!");
			this.enabled = false;
			return;
		}

#if (!UNITY_ANDROID || UNITY_EDITOR)
		if (!CameraController.UseCameraTexture)
			return;

		// This will scale the render texture based on ideal resolution
		if (CameraTexture[0] == null)
			CreateRenderTexture (CameraController.CameraTextureScale);
					
		GetComponent<Camera>().targetTexture = CameraTexture[(RightEye) ? 1 : 0];
#endif
	}
	
	/// <summary>
	/// Raises the pre cull event.
	/// </summary>
	void OnPreCull()
	{
		// NOTE: Setting the camera here increases latency, but ensures
		// that all Unity sub-systems that rely on camera location before
		// being set to render are satisfied.
		if(CameraController.CallInPreRender == false)
			SetCameraOrientation();
	}
	
	/// <summary>
	/// Raises the pre render event.
	/// </summary>
	void OnPreRender()
	{
		// NOTE: Better latency performance here, but messes up water rendering and other
		// systems that rely on the camera to be set before PreCull takes place.
		if(CameraController.CallInPreRender == true)
			SetCameraOrientation();
		
		if(CameraController.WireMode == true)
			GL.wireframe = true;
	}

	/// <summary>
	/// Raises the post render event.
	/// </summary>
	// UnityAndroid:
	// The FBO will be current in OnPostRender.
	// The FBO is NOT current in OnRenderImage, so don't try to put this work there.
	void OnPostRender()
	{
		if(CameraController.WireMode == true)
			GL.wireframe = false;

#if UNITY_ANDROID
		// Allow custom code to render before we kick off the
		// plugin event for this camera.
		if ( OnCustomPostRender != null ) {
			OnCustomPostRender();
		}
#endif

#if (UNITY_ANDROID && !UNITY_EDITOR)
		// Moved vignette back to plugin for special overlay handling
		CameraController.CameraEndFrame( (RenderEventType)Camera.current.depth );
#endif
	}
	#endregion

	#region OVRCamera Functions

	/// <summary>
	/// Sets the camera orientation.
	/// </summary>
	void SetCameraOrientation()
	{	
#if (!UNITY_ANDROID || UNITY_EDITOR)
		// Main camera has a depth of 0, so it will be rendered first
		if(GetComponent<Camera>().depth == 0.0f)
		{			
			// If desired, update parent transform y rotation here
			// This is useful if we want to track the current location of
			// of the head.
			// TODO: Future support for x and z, and possibly change to a quaternion
			// NOTE: This calculation is one frame behind 
			if(CameraController.TrackerRotatesY == true)
			{
				Vector3 a = GetComponent<Camera>().transform.rotation.eulerAngles;
				a.x = 0; 
				a.z = 0;
				transform.parent.transform.eulerAngles = a;
			}
			/*
			else
			{
				// We will still rotate the CameraController in the y axis
				// based on the fact that we have a Y rotation being passed 
				// in from above that still needs to take place (this functionality
				// may be better suited to be calculated one level up)
				Vector3 a = Vector3.zero;
				float y = 0.0f;
				CameraController.GetYRotation(ref y);
				a.y = y;
				gameObject.transform.parent.transform.eulerAngles = a;
			}
			*/	

			// Get camera orientation and position from vision
			Quaternion camOrt = Quaternion.identity;
			Vector3 camPos = Vector3.zero;
			OVRDevice.GetCameraPositionOrientation(ref camPos, ref camOrt);

			if (CameraController.EnablePosition)
				CameraPosition = camPos;

			bool useOrt = (CameraController.EnableOrientation && !(CameraController.TimeWarp && CameraController.FreezeTimeWarp));
			if (useOrt)
			{
				SampleStartRotation();
				CameraOrientation = camOrt;
			}
			
			// This needs to go as close to reading Rift orientation inputs
			OVRDevice.ProcessLatencyInputs();			
		}
		
		// Calculate the rotation Y offset that is getting updated externally
		// (i.e. like a controller rotation)
		float yRotation = 0.0f;
		float xRotation = 0.0f;
		CameraController.GetYRotation(ref yRotation);
		CameraController.GetXRotation(ref xRotation );
		Quaternion qp = Quaternion.Euler(xRotation, yRotation, 0.0f);
		Vector3 dir = qp * Vector3.forward;
		qp.SetLookRotation(dir, Vector3.up);
	
		// Multiply the camera controllers offset orientation (allow follow of orientation offset)
		Quaternion orientationOffset = Quaternion.identity;
		CameraController.GetOrientationOffset(ref orientationOffset);
		qp = orientationOffset * qp * CameraOrientationOffset;
		
		// Multiply in the current HeadQuat (q is now the latest best rotation)
		Quaternion q = qp * CameraOrientation;
		
		// * * *
		// Update camera rotation
		GetComponent<Camera>().transform.rotation = q;
		
		// * * *
		// Update camera position (first add Offset to parent transform)
		GetComponent<Camera>().transform.localPosition = NeckPosition;
	
		// Adjust neck by taking eye position and transforming through q
		// Get final camera position as well as the clipping difference 
		// (to allow for absolute location of center of camera grid space)
		Vector3 newCamPos = Vector3.zero;
		CameraPositionOffsetAndClip(ref CameraPosition, ref newCamPos);

		// Update list of game objects with new CameraOrientation / newCamPos here
		// For example, this location is used to update the GridCube
		foreach(OVRCameraGameObject obj in CameraLocalSetList)
		{
			if(obj.CameraController.GetCameraDepth() == GetComponent<Camera>().depth)
			{
				// Initial difference
				Vector3 newPos = -(qp * CameraPositionOffset);
				// Final position
				newPos += GetComponent<Camera>().transform.position;
			
				// Set the game object info
				obj.CameraGameObject.transform.position = newPos;
				obj.CameraGameObject.transform.rotation = qp;
			}
		}

		// Adjust camera position with offset/clipped cam location
		GetComponent<Camera>().transform.localPosition += Quaternion.Inverse(GetComponent<Camera>().transform.parent.rotation) * qp * newCamPos;

		// PGG: Call delegate function with new CameraOrientation / newCamPos here
		// This location will be used to update the arrow pointer
		// NOTE: Code below might not be needed. Please Look at OVRVisionGuide for potential
		// location for arrow pointer
/*
		// This will set the orientation of the arrow
		if (camera.depth == 2.0f)
		{
			// Set the location of the top node to follow the camera
			OVRMainMenu.PointerSetPosition(camera.transform.position);
			OVRMainMenu.PointerSetOrientation(camera.transform.rotation);
			Quaternion foo = Quaternion.LookRotation(-CameraPosition);
			OVRMainMenu.PointerRotatePointerGeometry(qp * foo);
		}
*/
		// move eyes out by x (IPD)
		Vector3 newEyePos = Vector3.zero;
		newEyePos.x = EyePosition.x;
		GetComponent<Camera>().transform.localPosition += GetComponent<Camera>().transform.localRotation * newEyePos;
#else
		// NOTE: On Android, camera orientation is set from OVRCameraController Update()
#endif
	}

	/// <summary>
	/// Based on offset and clip values, adjust camera position
	/// </summary>
	/// <param name="inCam">In cam.</param>
	/// <param name="outCam">Out cam.</param>
	void CameraPositionOffsetAndClip(ref Vector3 inCam, ref Vector3 outCam)
	{
		outCam = inCam - CameraPositionOffset;
	}


	///////////////////////////////////////////////////////////
	// PUBLIC FUNCTIONS
	///////////////////////////////////////////////////////////

	/// <summary>
	/// Call this in CameraController to set up the ideal FOV as
	/// defined by the SDK
	/// </summary>
	/// <returns>The ideal FOV.</returns>
	public float GetIdealVFOV()
	{
		return eyeMesh.GetIdealFOV().y;
	}

	/// <summary>
	/// Calculates the aspect ratio.
	/// </summary>
	/// <returns>The aspect ratio.</returns>
	public float CalculateAspectRatio()
	{
		Vector2 fov = eyeMesh.GetIdealFOV();
		return fov.x / fov.y;
	}

	/// <summary>
	/// Gets the ideal resolution.
	/// </summary>
	/// <returns>The ideal resolution.</returns>
	public void GetIdealResolution(ref int w, ref int h)
	{
		eyeMesh.GetIdealResolution(ref w, ref h);
	}

	/// <summary>
	/// UpdateDistortionMeshParams
	/// Query the camera fielf of view and then set up the appropriate values
	/// </summary>
	/// <param name="lc">Lc.</param>
	/// <param name="rightEye">If set to <c>true</c> right eye.</param>
	/// <param name="flipY">If set to <c>true</c> flip y.</param>
	public void UpdateDistortionMeshParams (ref OVRLensCorrection lc, bool rightEye, bool flipY)
	{
		if (CameraController == null)
			return;
		
		float fovH = GetHorizontalFOV();
		eyeMesh.SetFOV(fovH, GetComponent<Camera>().fieldOfView);

		eyeMesh.UpdateParams(ref lc, rightEye, flipY, CameraController.UseCameraTexture);
	}
	
	/// <summary>
	/// We must calculate this to return back to mesh distortion
	/// </summary>
	/// <returns>The horizontal FO.</returns>
	public float GetHorizontalFOV()
	{
//		return camera.fieldOfView * camera.aspect;

		float vFOVInRads =  GetComponent<Camera>().fieldOfView * Mathf.Deg2Rad;
		float hFOVInRads = 2 * Mathf.Atan( Mathf.Tan(vFOVInRads / 2) * GetComponent<Camera>().aspect);
		float hFOV = hFOVInRads * Mathf.Rad2Deg;

		return hFOV;
	}

	/// <summary>
	/// Creates the render texture.
	/// </summary>
	/// <param name="scale">Scale.</param>
	public void CreateRenderTexture(float scale)
	{
		// Set CameraTextureScale (increases the size of the texture we are rendering into
		// for a better pixel match when post processing the image through lens distortion)
		// If CameraTextureScale is not 1.0f, create a new texture and assign to target texture
		// Otherwise, fall back to normal camera rendering
		int w = 0;
		int h = 0;

		GetIdealResolution(ref w, ref h);

		w = (int)((float)w * scale);
		h = (int)((float)h * scale);
		
		for (int i = 0; i < 2; ++i)
		{
			if (CameraTexture[i] != null)
				DestroyImmediate(CameraTexture[i]);
				
			CameraTexture[i] = new RenderTexture(w, h, 24, (GetComponent<Camera>().hdr) ? RenderTextureFormat.ARGBFloat : RenderTextureFormat.Default);
			CameraTexture[i].antiAliasing = (QualitySettings.antiAliasing == 0) ? 1 : QualitySettings.antiAliasing;
		}
	}

	///////////////////////////////////////////////////////////
	// VISION FUNCTIONS
	///////////////////////////////////////////////////////////
	
	/// <summary>
	/// Mainly to be used to reset camera position orientation
	/// camOffset will move the center eye position to an optimal location
	/// clampX/Y/Z will zero out the offset that is used (restricts offset in a given axis)
	/// </summary>
	/// <param name="posScale">Scale for positional change.</param>
	/// <param name="posOffset">Positional offset.</param>
	/// <param name="posOffset">Positional offset.</param>
	/// <param name="posOffset">Positional offset.</param>
	static public void ResetCameraPositionOrientation(Vector3 posScale, Vector3 posOffset, Vector3 ortScale, Vector3 ortOffset)
	{
		Vector3 camPos  = Vector3.zero;
		Quaternion camO = Quaternion.identity;
		OVRDevice.GetCameraPositionOrientation(ref camPos, ref camO);

		CameraPositionOffset = Vector3.Scale(camPos, posScale) - posOffset;

		Vector3 euler = Quaternion.Inverse(camO).eulerAngles;
		CameraOrientationOffset = Quaternion.Euler(Vector3.Scale(euler, ortScale) - ortOffset);
	}
	
	/// <summary>
	/// Set offset directly (for initial positioning that reflects the players
	/// eye location
	/// </summary>
	/// <param name="offset">Offset.</param>
	static public void SetCameraPositionOffset(Vector3 offset)
	{
		CameraPositionOffset = -offset;
	}

	/// <summary>
	/// Adds to local camera set list.
	/// </summary>
	/// <param name="obj">Object.</param>
	static public void AddToLocalCameraSetList(ref OVRCameraGameObject obj)
	{
		CameraLocalSetList.Add (obj);
	}
	
	/// <summary>
	/// Removes from local camera set list.
	/// </summary>
	/// <param name="obj">Object.</param>
	static public void RemoveFromLocalCameraSetList(ref OVRCameraGameObject obj)
	{
		CameraLocalSetList.Remove (obj);
	}
	
	/// <summary>
	/// Gets the camera orientation.
	/// </summary>
	/// <param name="orientation">Orientation.</param>
	static public void GetCameraOrientation(ref Quaternion orientation)
	{
		orientation = CameraOrientation;
	}
	
	/// <summary>
	/// Gets the absolute camera from vision position.
	/// </summary>
	/// <param name="pos">Position.</param>
	static public void GetAbsoluteCameraFromVisionPosition(ref Vector3 pos)
	{
		pos = CameraPosition;
	}
	
	/// <summary>
	/// Gets the relative camera from vision position.
	/// Takes into account position offset.
	/// </summary>
	/// <param name="pos">Position.</param>
	static public void GetRelativeCameraFromVisionPosition(ref Vector3 pos)
	{
		pos = CameraPosition - CameraPositionOffset;
	}
	
	#endregion	
}

//-------------------------------------------------------------------------------------
// ***** OVRCameraGameObject

/// <summary>
/// OVR camera game object.
/// Used to extend a GameObject for updates within an OVRCamera
/// </summary>
public class OVRCameraGameObject
{
	public GameObject 		   CameraGameObject = null;
	public OVRCameraController CameraController = null;
}

