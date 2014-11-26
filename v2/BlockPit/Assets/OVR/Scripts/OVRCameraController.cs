/************************************************************************************

Filename    :   OVRCameraController.cs
Content     :   Camera controller interface. 
				This script is used to interface the OVR cameras.
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

//-------------------------------------------------------------------------------------
// ***** OVRCameraController

/// <summary>
/// OVR camera controller.
/// OVRCameraController is a component that allows for easy handling of the lower level cameras.
/// It is the main interface between Unity and the cameras. 
/// This is attached to a prefab that makes it easy to add a Rift into a scene.
///
/// All camera control should be done through this component.
///
/// </summary>
public class OVRCameraController : MonoBehaviour
{
	public const string strOvrLib = "OculusPlugin";
	//// -- UnityAndroid - This function is stubbed for Android.
	[DllImport(strOvrLib)]
	static extern void EnableTimeWarp(bool isEnabled);

	#pragma warning disable 414		// The private field 'x' is assigned but its value is never used

	// PRIVATE MEMBERS
	private bool   UpdateCamerasDirtyFlag = false;
	private bool   UpdateDistortionDirtyFlag = false;
	private Camera CameraLeft, CameraRight = null;
	private float  AspectRatio = 1.0f;						

	//// -- UnityAndroid
	public bool Monoscopic = false;			// if true, only render the left eye camera
	private bool HasSkybox = false;			// true if any cameras on the controller will render 
											// a skybox or a skybox is present in the render settings
	private const int EyeBufferCount = 3;	// triple buffer for max overlap
	private const int CurrEyeBufferIdx = 0;
	private const int NextEyeBufferIdx = 1;
	private int[] EyeBufferNum = { 0, 0 };	// curr frame, next frame
	static private RenderTexture[]	CameraTextures = new RenderTexture[EyeBufferCount * 2];	// one for each eye
	static private int[] CameraTextureIds = new int[EyeBufferCount * 2];            		// one for each eye
	private const int EyeResolution = 1024;
	private int TimeWarpViewNumber = 0;
	//// -- UnityAndroid

	// Initial orientation of the camera, can be used to always set the 
	// zero orientation of the cameras to follow a set forward facing orientation.
	private Quaternion OrientationOffset = Quaternion.identity;	
	// Set Y rotation here; this will offset the y rotation of the cameras. 
	private float YRotation = 0.0f;
	private float XRotation = 0.0f;

	#pragma warning restore 414		// The private field 'x' is assigned but its value is never used

	// IPD
	[SerializeField]
	private float  		ipd 		= 0.064f; 				// in millimeters
	public 	float 		IPD
	{
		get{return ipd;}
		set{ipd = value; UpdateDistortionDirtyFlag = true;}
	}

	// VERTICAL FOV
	[SerializeField]
	private float  		verticalFOV = 90.0f;	 			// in degrees
	public 	float		VerticalFOV
	{
		get{return verticalFOV;}
		set
		{
			verticalFOV = Mathf.Clamp(value, 40.0f, 170.0f);
			UpdateDistortionDirtyFlag = true;
		}
	}

	// If true, renders to a RenderTexture to allow super-sampling.
	public bool UseCameraTexture = false;

	// A constant multiple of the ideal resolution, which enables supersampling for higher image quality.
	public float CameraTextureScale = 1.0f;

	// SCALE RENDER TARGET
	[SerializeField]
	private float		scaleRenderTarget = 1.0f;
	public	float 		ScaleRenderTarget
	{
		get{return scaleRenderTarget;}
		set
		{
			scaleRenderTarget = value;
			if(scaleRenderTarget > 1.0f)
				scaleRenderTarget = 1.0f;
			else if (scaleRenderTarget < 0.01f)
				scaleRenderTarget = 0.01f;

			// We will call this initially to grab the serialized value
			SetScaleRenderTarget();
		}
	}

	// Camera positioning:
	// CameraRootPosition will be used to calculate NeckPosition and Eye Height
	public Vector3 		CameraRootPosition = new Vector3(0.0f, 1.0f, 0.0f);					
	// From CameraRootPosition to neck
	public Vector3 		NeckPosition      = new Vector3(0.0f, 0.7f,  0.0f);
	//// -- UnityAndroid
	// From neck to eye (rotation and translation; x will be different for each eye, based on IPD)
	// TODO: This should come from the profile, we currently don't have this support on Android
	public Vector3 		EyeCenterPosition = new Vector3(0.0f, 0.15f, 0.09f);
	//// -- UnityAndroid


	#pragma warning disable 414		// The private field 'x' is assigned but its value is never used
	//// -- UnityAndroid
    // This is returned by the time warp rendering, which samples the head tracker
    // right at vsync time, so moving objects will move consistently while head motion
    // is occuring.
    private Quaternion CameraOrientation = Quaternion.identity;
	//// -- UnityAndroid
	#pragma warning restore 414		// The private field 'x' is assigned but its value is never used

	// Use player eye height as set in the Rift config tool
	public  bool 		UsePlayerEyeHeight     = false;
	private bool 		PrevUsePlayerEyeHeight = false;
	// Set this transform with an object that the camera orientation should follow.
	// NOTE: Best not to set this with the OVRCameraController IF TrackerRotatesY is
	// on, since this will lead to uncertain output
	public Transform 	FollowOrientation = null;
	// Set to true if we want the rotation of the camera controller to be influenced by tracker
	public bool  		TrackerRotatesY	= false;
	// Use this to enable / disable Tracker orientation
	public bool         EnableOrientation = true;
	// Use this to enable / disable Tracker position
	public bool         EnablePosition = true;
	// Use this to turn on/off Prediction
	public bool			PredictionOn 	= true;
	// Use this to decide where tracker sampling should take place
	// Setting to true allows for better latency, but some systems
	// (such as Pro water) will break
	public bool			CallInPreRender = false;
	// Use this to turn on wire-mode
	public bool			WireMode  		= false;
	// Turn lens distortion on/off; use Chromatic Aberration in lens distortion calculation
	public bool 		LensCorrection  	= true;
	public bool 		Chromatic			= true;

	// Automatically adjusts output to compensate for rendering latency.
	[SerializeField]
	private bool		timeWarp = false;
	public bool 		TimeWarp
	{
		get { return timeWarp; }
		set
		{
			timeWarp = value;
			EnableTimeWarp(value);
		}
	}

	// If true, then TimeWarp freezes the start view.
	public bool 		FreezeTimeWarp		= false;
	public bool 		FlipCorrectionInY	= false;

	// UNITY CAMERA FIELDS
	// Set the background color for both cameras
	[SerializeField]
	private Color 		backgroundColor = new Color(0.192f, 0.302f, 0.475f, 1.0f);
	public  Color       BackgroundColor
	{
		get{return backgroundColor;}
		set{backgroundColor = value; UpdateCamerasDirtyFlag = true;}
	}
	// Set the near and far clip plane for both cameras
	[SerializeField]
	private float 		nearClipPlane   = 0.15f;
	public  float 		NearClipPlane
	{
		get{return nearClipPlane;}
		set{nearClipPlane = value; UpdateCamerasDirtyFlag = true;}
	}
	[SerializeField]
	private float 		farClipPlane    = 1000.0f;  
	public  float 		FarClipPlane
	{
		get{return farClipPlane;}
		set{farClipPlane = value; UpdateCamerasDirtyFlag = true;}
	}

	// * * * * * * * * * * * * *
		
#if (UNITY_ANDROID && !UNITY_EDITOR)
	[NonSerialized]
	private static OVRVolumeControl VolumeController = null;

	// To take advantage of Unity's multithreaded rendering, most of the plugin's
	// work needs to take place on the render thread, triggered by calls to
	// GL.IssuePluginEvent( eventNum );

	[DllImport("OculusPlugin")]
	private static extern void OVR_SetInitVariables(IntPtr activity, IntPtr vrActivityClass );

	[DllImport("OculusPlugin")]
	private static extern void OVR_SetEyeParms( int fbWidth, int fbHeight );

	[DllImport("OculusPlugin")]
	private static extern bool OVR_GetSensorState( bool monoscopic,
	                                              ref float w,
	                                              ref float x,
	                                              ref float y,
	                                              ref float z,
	                                              ref float fov,
	                                              ref int viewNumber  );

	// Get this from Unity on startup so we can call Activity java functions
	static private bool androidJavaInit = false;
	static private AndroidJavaObject activity;
	static private AndroidJavaClass javaVrActivityClass;

	static bool focused = false;

	void OnPause()
	{
		OVRModeManager.LeaveVRMode();

		RenderTexture.active = null;

		for ( int i = 0; i < EyeBufferCount * 2; i++ )
		{
			if ( ( CameraTextures[i] != null ) && CameraTextures[i].IsCreated() )
			{
				CameraTextures[i].Release();
				CameraTextureIds[i] = 0;
			}
		}
	}
	IEnumerator OnResume()
	{
		yield return null;		// delay 1 frame to allow Unity enough time to create the windowSurface

		// When the app is paused and subsequently resumed, we may lose render texture contents.
		// Test for this case and record the new texture id's
		for ( int i = 0; i < EyeBufferCount * 2; i++ )
		{
		//	DebugUtils.Assert( CameraTextures[i] != null );
			if ( ( CameraTextures[i] != null ) && !CameraTextures[i].IsCreated() )
			{
				CameraTextures[i].Create(); // upload to the GPU so we can get the hardware id
				// Note: calling GetNativeTexturePtr() with mt-rendering will synchronize with the rendering thread,
				// set up texture ids only at initialization time.
				CameraTextureIds[i] = CameraTextures[i].GetNativeTextureID();
			}
		}

		OVRModeManager.EnterVRMode();
	}

	void OnApplicationPause( bool pause )
	{
		Debug.Log( "OnApplicationPause() " + pause );
		if ( pause ) {
			focused = false;
			OnPause();
		} else {
			if ( !focused ) {
				Debug.Log( "Calling resume from OnApplicationPause" );
				focused = true;
				StartCoroutine( OnResume() );
			}
		}
	}

	void OnApplicationFocus( bool focus )
	{
		// OnApplicationFocus() does not appear to be called 
		// consistently while OnApplicationPause is. Moved
		// functionality to OnApplicationPause().

		Debug.Log( "OnApplicationFocus() " + focus );
		if ( focus && !focused ) {
			//Debug.Log( "Calling resume from OnApplicationFocus()" );
			focused = true;
			StartCoroutine( OnResume() );
		}
	}

	void OnApplicationQuit()
	{
		Debug.Log( "OnApplicationQuit" );

		// This will trigger the shutdown on the render thread
		OVRPluginEvent.Issue( RenderEventType.ShutdownRenderThread );
	}

	IEnumerator CallPluginEndOfFrame()
	{
		while( true )
		{
			yield return new WaitForEndOfFrame();
			
			// Pass down the TimeWarpViewNumber aquired during CameraController.Update()
			// to link which sensor reading is associated with these images.
			OVRPluginEvent.IssueWithData( RenderEventType.TimeWarp, TimeWarpViewNumber );
		}
	}

	/// <summary>
	/// Creates a popup dialog that shows when volume changes.
	/// </summary>
	private static void InitVolumeController()
	{
		Debug.Log( "InitVolumeController()" );
		if ( VolumeController == null )
		{
			Debug.Log( "Creating volume controller..." );
			// Create the volume control popup
			GameObject go = GameObject.Instantiate( Resources.Load( "OVRVolumeController" ) ) as GameObject;
			if ( go != null )
			{
				VolumeController = go.GetComponent<OVRVolumeControl>();
			}
			else
			{
				Debug.LogError( "Unable to instantiate volume controller" );
			}
		}
	}
#endif

	/// <summary>
	/// Enable this instance.
	/// </summary>
	void OnEnable()
	{
#if (UNITY_ANDROID && !UNITY_EDITOR)
		// Start the coroutine to issue EndFrame events for the cameras
		StartCoroutine( CallPluginEndOfFrame() );

		if ( VolumeController != null )
		{
			VolumeController.UpdatePosition( CameraRight.transform.rotation, CameraRight.transform.position );
		}
#endif
	}

	/// <summary>
	/// Disable this instance.
	/// </summary>
	void OnDisable()
	{
#if (UNITY_ANDROID && !UNITY_EDITOR)
		StopAllCoroutines();
#endif
	}

	void OnDestroy()
	{
#if (UNITY_ANDROID && !UNITY_EDITOR)
		RenderTexture.active = null;
#endif
	}

	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake()
	{
#if (!UNITY_ANDROID || UNITY_EDITOR)
		//// -- UnityAndroid
		// create the distortion camera component and camera (only on PC)
		if (this.enabled)
		{
			if (GetComponent<OVRDistortionCamera>() == null)
			{
				gameObject.AddComponent<OVRDistortionCamera>();
			}
		}
		//// -- UnityAndroid
#endif

		// Get the cameras
		OVRCamera[] ovrcameras = gameObject.GetComponentsInChildren<OVRCamera>( true );
		
		for (int i = 0; i < ovrcameras.Length; i++)
		{
			// assign the right and left cameras, check the camera name for outdated prefabs
			if(ovrcameras[i].RightEye || ( string.Compare( ovrcameras[i].name, "cameraright", true ) == 0 ) )
			{
				ovrcameras[i].RightEye = true;
				SetCameras(CameraLeft, ovrcameras[i].GetComponent<Camera>());
			}
			else
			{
				SetCameras(ovrcameras[i].GetComponent<Camera>(), CameraRight);
			}

			if ( ( ovrcameras[i].GetComponent<Camera>().clearFlags == CameraClearFlags.Skybox ) && ( ( ovrcameras[i].gameObject.GetComponent<Skybox>() != null ) || ( RenderSettings.skybox != null ) ) )
			{
				HasSkybox = true;
				Debug.Log ( "Skybox Clear Required" );
			}
		}

		if ((CameraLeft == null) || (CameraRight == null))
			Debug.LogWarning("WARNING: Unity Cameras in OVRCameraController not found!");

#if (UNITY_ANDROID && !UNITY_EDITOR)
		// Disable screen dimming
		Screen.sleepTimeout = SleepTimeout.NeverSleep;

		Application.targetFrameRate = 60;

		// don't allow the app to run in the background
		Application.runInBackground = false;

		// don't enable gyro, it is not used and triggers expensive display calls
		if ( Input.gyro.enabled )
		{
			Debug.LogError( "*** Auto-disabling Gyroscope ***" );
			Input.gyro.enabled = false;
		}

		// don't enable antiAliasing on the main window display, it may cause
		// bad behavior with various tiling controls.
		if ( QualitySettings.antiAliasing > 1 )
		{
			Debug.LogError( "*** Main Display should have 0 samples ***" );
		}

		// Only perform this check when the CameraController is enabled
		// to allow for toggling stereo / non-stero camers for testing.
		if ( this.enabled )
		{
			// Make sure there isn't an OVRDistortion camera on Android
			// this is done primarly to clean up old camera controller prefabs
			Camera[] cameras = gameObject.GetComponentsInChildren<Camera>();
			for (int i = 0; i < cameras.Length; i++)
			{
				if ( (cameras[i] != CameraLeft) && ( cameras[i] != CameraRight ) )
				{
					Debug.LogWarning("WARNING: Extra camera on OVRCameraController found!");
					cameras[i].cullingMask = 0;	// cull everything
					cameras[i].clearFlags = CameraClearFlags.Nothing;
					Destroy( cameras[i] );
				}
			}
		}

		CameraLeft.depth = (int)RenderEventType.LeftEyeEndFrame;
		CameraRight.depth = (int)RenderEventType.RightEyeEndFrame;

		// When rendering monoscopic, we will use the left camera render
		// for both eyes.
		if ( Monoscopic )
		{
			//CameraRight.enabled = false;
			CameraRight.cullingMask = 0;	// cull everything
			CameraRight.clearFlags = CameraClearFlags.Nothing;
		}

		if ( !androidJavaInit )
		{
			AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
			javaVrActivityClass = new AndroidJavaClass("com.oculusvr.vrlib.VrActivity");
			// Prepare for the RenderThreadInit()
			OVR_SetInitVariables( activity.GetRawObject(), javaVrActivityClass.GetRawClass() );

			androidJavaInit = true;
		}

		InitVolumeController();
#endif	
	}
	
	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start()
	{		
		// Get the required Rift information needed to set cameras
		InitCameraControllerVariables();
		
		// Initialize the cameras
		UpdateCamerasDirtyFlag = true;

#if (!UNITY_ANDROID || UNITY_EDITOR)
		UpdateDistortionDirtyFlag = true;
		SetScaleRenderTarget();
		SetMaximumVisualQuality();

		EnableTimeWarp(timeWarp);
#else
		OVR_SetEyeParms( EyeResolution, EyeResolution );

		// Initialize all the rendertargets we are going to use for async time warp
		for ( int i = 0; i < EyeBufferCount * 2; i++ )
		{
			if ( ( CameraTextures[i] == null ) || !CameraTextures[i].IsCreated() )
			{
				// TODO: honor use24BitDepthBuffer in PlayerSettings if we can get access to that info
				// NOTE: Android appears to ignore the use24BitDepthBuffer flag and we always get a 24
				// bit window depth buffer.
				// As a minor optimization, the depth buffer precision may be set to 16-bits.
				CameraTextures[i] = new RenderTexture(EyeResolution, EyeResolution, 24);
				CameraTextures[i].antiAliasing = 2;
				CameraTextures[i].Create(); // upload to the GPU so we can get the hardware id
				// Note: calling GetNativeTexturePtr() with mt-rendering will synchronize with the rendering thread,
				// set up texture ids only at initialization time.
				CameraTextureIds[i] = CameraTextures[i].GetNativeTextureID();
			}
		}

		// This will trigger the init on the render thread
		OVRPluginEvent.Issue( RenderEventType.InitRenderThread );

		SetMaximumVisualQuality();
#endif
	}
		
	/// <summary>
	/// Inits the camera controller variables.
	/// Made public so that it can be called by classes that require information about the
	/// camera to be present when initing variables in 'Start'
	/// </summary>
	public void InitCameraControllerVariables()
	{
		// Get the IPD value (distance between eyes in meters)
		OVRDevice.GetIPD(ref ipd);

#if (!UNITY_ANDROID || UNITY_EDITOR)
		// Using the calculated FOV, based on distortion parameters, yeilds the best results.
		// However, public functions will allow to override the FOV if desired
		VerticalFOV = CameraLeft.GetComponent<OVRCamera>().GetIdealVFOV();
		// Get aspect ratio as well
		AspectRatio = CameraLeft.GetComponent<OVRCamera>().CalculateAspectRatio();
#endif

		// Get our initial world orientation of the cameras from the scene (we can grab it from 
		// the set FollowOrientation object or this OVRCameraController gameObject)
		if(FollowOrientation != null)
			OrientationOffset = FollowOrientation.rotation;
		else
			OrientationOffset = transform.rotation;
	}

	/// <summary>
	/// Sets the scale render target.
	/// </summary>
	void SetScaleRenderTarget()
	{
		if((CameraLeft != null && CameraRight != null))
		{
			float scale = (UseCameraTexture) ? ScaleRenderTarget : 1f;

			// Aquire and scale the cameras
			OVRCamera[] cameras = gameObject.GetComponentsInChildren<OVRCamera>();
			for (int i = 0; i < cameras.Length; i++)
			{
				float w = (UseCameraTexture) ? scale : 0.5f * scale;
				float x = (UseCameraTexture) ? 0f : (cameras[i].RightEye) ? 0.5f : 0f;
				cameras[i].GetComponent<Camera>().rect = new Rect(x, 0f, w, scale);
			}
			
			// Aquire and Scale the lens correction components
			OVRLensCorrection[] lc = gameObject.GetComponentsInChildren<OVRLensCorrection>();
			for (int i = 0; i < lc.Length; i++)
			{
				lc[i].dynamicScale = scale;
			}
		}
	}
	
	/// <summary>
	/// Updates the cameras.
	/// </summary>
	void Update()
	{
		// Values that influence the stereo camera orientation up and above the tracker
		if(FollowOrientation != null)
			OrientationOffset = FollowOrientation.rotation;

		// Handle positioning of eye height and other things here
		UpdatePlayerEyeHeight();

		// Configure left and right cameras
		float eyePositionOffset = -IPD * 0.5f;
		ConfigureCamera(CameraLeft, eyePositionOffset);

		eyePositionOffset       = IPD * 0.5f;
		ConfigureCamera(CameraRight, eyePositionOffset);
		
		// Turn off dirty flag
		UpdateCamerasDirtyFlag = false;
		UpdateDistortionDirtyFlag = false;

#if (UNITY_ANDROID && !UNITY_EDITOR)
		// Fetch the latest head orientation for this frame
		float w = 0, x = 0, y = 0, z = 0;
		float fov = 90.0f;
		OVR_GetSensorState( Monoscopic, ref w, ref x, ref y, ref z, ref fov, ref TimeWarpViewNumber);

		// OVR_GetSensorState provides the proper FOV for the device.
		// NOTE: For VR, you cannot change the FOV (make more visible to the user through the lenses)
		// from what the device actually provides, or it won't feel right as you move around.
		CameraRight.fieldOfView = fov;
		CameraLeft.fieldOfView = fov;

		// Change the co-ordinate system from right-handed to Unity left-handed
		CameraOrientation.w = w;
		CameraOrientation.x = -x;
		CameraOrientation.y = -y;
		CameraOrientation.z = z;

		// We are going to cycle between three render targets for the cameras
		// so the async time warp can use one to render the distorted screen, have
		// a second one being rendered by the gpu, and the game can be issuing commands
		// for a third.
		EyeBufferNum[CurrEyeBufferIdx] = EyeBufferNum[NextEyeBufferIdx];
		EyeBufferNum[NextEyeBufferIdx] = (EyeBufferNum[NextEyeBufferIdx] + 1) % EyeBufferCount;
		CameraLeft.targetTexture = CameraTextures[EyeBufferNum[CurrEyeBufferIdx] * 2 + 0];
		CameraRight.targetTexture = CameraTextures[EyeBufferNum[CurrEyeBufferIdx] * 2 + 1];

        Quaternion qp = Quaternion.identity;
        Vector3 dir = Vector3.forward;

        // Calculate the rotation Y offset that is getting updated externally
        // (i.e. like a controller rotation)
        float yRotation = 0.0f;
        float xRotation = 0.0f;
        GetYRotation(ref yRotation);
        GetXRotation(ref xRotation);
        qp = Quaternion.Euler(xRotation, yRotation, 0.0f);
        dir = qp * Vector3.forward;
        qp.SetLookRotation(dir, Vector3.up);

        Quaternion ccq = qp;
        Quaternion cq = qp;

        // Multiply the camera controllers offset orientation (allow follow of orientation offset)
        Quaternion orientationOffset = Quaternion.identity;
        GetOrientationOffset(ref orientationOffset);
        cq = orientationOffset * cq;

        // Multiply in the current HeadQuat (now the latest best rotation) 
        ccq = ccq * CameraOrientation;
        cq = cq * CameraOrientation;

        // If desired, update parent transform y rotation here
        // This is useful if we want to track the current location of
        // of the head.
        // TODO: Future support for x and z, and possibly change to a quaternion
        if(TrackerRotatesY == true)
        {
			// FIXME: Optimize
			ccq = Quaternion.Euler( 0.0f, cq.eulerAngles.y, 0.0f );
        }

        // * * *
        // Update camera rotation
		transform.rotation = ccq;
        CameraLeft.transform.rotation = cq;
        CameraRight.transform.rotation = cq;

        // * * *
        // Update camera position (first add Offset to parent transform)
        // Adjust neck by taking eye position and transforming through q
        CameraLeft.transform.position = 
			transform.position + NeckPosition
                + cq * ( EyeCenterPosition + new Vector3( IPD * -0.5f, 0.0f, 0.0f ) );

        CameraRight.transform.position = 
            transform.position + NeckPosition
                + cq * ( EyeCenterPosition + new Vector3( IPD * 0.5f, 0.0f, 0.0f ) );

		if ( VolumeController != null )
		{
			VolumeController.UpdatePosition( cq, CameraRight.transform.position );
		}
#endif
	}

    /// <summary>
	/// Configures the camera.
	/// </summary>
	/// <returns><c>true</c>, if camera was configured, <c>false</c> otherwise.</returns>
	/// <param name="camera">Camera.</param>
	/// <param name="eyePositionOffset">Eye position offset.</param>
	bool ConfigureCamera(Camera camera, float eyePositionOffset)
	{
#if (!UNITY_ANDROID || UNITY_EDITOR)
		OVRCamera cam = camera.GetComponent<OVRCamera>();

		if (UpdateDistortionDirtyFlag)
		{
			// Always set  camera fov and aspect ration
			camera.fieldOfView = VerticalFOV;
			camera.aspect      = AspectRatio;
				
			// Push params also into the mesh distortion instance (if there is one)
			OVRLensCorrection lc = camera.GetComponent<OVRLensCorrection>();
			cam.UpdateDistortionMeshParams(ref lc, (camera == CameraRight), FlipCorrectionInY);

			if (!UseCameraTexture && SystemInfo.graphicsDeviceVersion.Contains ("Direct3D"))	// this doesn't work with graphics emulation enabled (ie for Android)
				lc._DMScale = new Vector2(lc._DMScale.x, -lc._DMScale.y);
		}

		if (UpdateCamerasDirtyFlag)
		{
			// Background color
			camera.backgroundColor = BackgroundColor;
			
			// Clip Planes
			camera.nearClipPlane = NearClipPlane;
			camera.farClipPlane = FarClipPlane;
			
			// Set camera variables that pertain to the neck and eye position
			// NOTE: We will want to add a scale value here in the event that the player 
			// grows or shrinks in the world. This keeps head modelling behaviour
			// accurate
			cam.NeckPosition = NeckPosition;
			cam.EyePosition = new Vector3(eyePositionOffset, 0f, 0f);
		}
#else
		// Camera.targetTexture will be set each frame in Update to a different buffer
		// to allow overlap with async time warp.
		camera.fieldOfView = 90.0f;
		camera.aspect = 1.0f;
		camera.rect = new Rect(0, 0, 1, 1);	// Does this matter when targetTexture is set?

		if (UpdateCamerasDirtyFlag)
		{
			// Background color
			camera.backgroundColor = BackgroundColor;
			
			// Clip Planes
			camera.nearClipPlane = NearClipPlane;
			camera.farClipPlane = FarClipPlane;

			// If we don't clear the color buffer with a glClear, tiling GPUs
			// will be forced to do an "unresolve" and read back the color buffer information.
			// The clear is free on PowerVR, and possibly Mali, but it is a performance cost
			// on Adreno, and we would be better off if we had the ability to discard/invalidate
			// the color buffer instead of clearing.

			// NOTE: The color buffer is not being invalidated in skybox mode, forcing an additional,
			// wasted color buffer read before the skybox is drawn.
			camera.clearFlags = ( HasSkybox ) ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
		}
#endif

		return true;
	}
	
	/// <summary>
	/// Updates the height of the player eye.
	/// </summary>
	void UpdatePlayerEyeHeight()
	{
		if((UsePlayerEyeHeight == true) && (PrevUsePlayerEyeHeight == false))
		{
			// Calculate neck position to use based on Player configuration
			float  peh = 0.0f;

#if false
			if(OVRDevice.GetPlayerEyeHeight(ref peh) != false)
#else
			// Match behavior of 0.4 CAPI Desktop plugin. On
			// O.3 Desktop, false will be returned when the
			// profile is not present, even though a valid
			// default eye height is returned.
			OVRDevice.GetPlayerEyeHeight( ref peh );
#endif
			{
				NeckPosition.y = peh - CameraRootPosition.y;
			}
		}
		
		PrevUsePlayerEyeHeight = UsePlayerEyeHeight;
	}
	
	///////////////////////////////////////////////////////////
	// PUBLIC FUNCTIONS
	///////////////////////////////////////////////////////////

	/// <summary>
	/// Sets the cameras - Should we want to re-target the cameras
	/// </summary>
	/// <param name="cameraLeft">Camera left.</param>
	/// <param name="cameraRight">Camera right.</param>
	public void SetCameras(Camera cameraLeft, Camera cameraRight)
	{
		CameraLeft  = cameraLeft;
		CameraRight = cameraRight;

#if (!UNITY_ANDROID || UNITY_EDITOR)
		var dc = GetComponent<OVRDistortionCamera>();		
		if (dc != null)
		{
			dc.CameraLeft = CameraLeft;
			dc.CameraRight = CameraRight;
		}
#endif
		UpdateCamerasDirtyFlag = true;
	}
	
	/// <summary>
	/// Gets the IPD.
	/// </summary>
	/// <param name="ipd">Ipd.</param>
	public void GetIPD(ref float ipd)
	{
		ipd = IPD;
	}
	/// <summary>
	/// Sets the IPD.
	/// </summary>
	/// <param name="ipd">Ipd.</param>
	public void SetIPD(float ipd)
	{
		IPD = ipd;
		UpdateDistortionDirtyFlag = true;
	}
			
	/// <summary>
	/// Gets the vertical FOV.
	/// </summary>
	/// <param name="verticalFOV">Vertical FO.</param>
	public void GetVerticalFOV(ref float verticalFOV)
	{
		verticalFOV = VerticalFOV;
	}
	/// <summary>
	/// Sets the vertical FOV.
	/// </summary>
	/// <param name="verticalFOV">Vertical FO.</param>
	public void SetVerticalFOV(float verticalFOV)
	{
		VerticalFOV = verticalFOV;
		UpdateDistortionDirtyFlag = true;
	}
	
	/// <summary>
	/// Gets the aspect ratio.
	/// </summary>
	/// <param name="aspecRatio">Aspec ratio.</param>
	public void GetAspectRatio(ref float aspecRatio)
	{
		aspecRatio = AspectRatio;
	}
	/// <summary>
	/// Sets the aspect ratio.
	/// </summary>
	/// <param name="aspectRatio">Aspect ratio.</param>
	public void SetAspectRatio(float aspectRatio)
	{
		AspectRatio = aspectRatio;
		UpdateDistortionDirtyFlag = true;
	}
		
	/// <summary>
	/// Gets the camera root position.
	/// </summary>
	/// <param name="cameraRootPosition">Camera root position.</param>
	public void GetCameraRootPosition(ref Vector3 cameraRootPosition)
	{
		cameraRootPosition = CameraRootPosition;
	}
	/// <summary>
	/// Sets the camera root position.
	/// </summary>
	/// <param name="cameraRootPosition">Camera root position.</param>
	public void SetCameraRootPosition(ref Vector3 cameraRootPosition)
	{
		CameraRootPosition = cameraRootPosition;
		UpdateCamerasDirtyFlag = true;
	}

	/// <summary>
	/// Gets the neck position.
	/// </summary>
	/// <param name="neckPosition">Neck position.</param>
	public void GetNeckPosition(ref Vector3 neckPosition)
	{
		neckPosition = NeckPosition;
	}
	/// <summary>
	/// Sets the neck position.
	/// </summary>
	/// <param name="neckPosition">Neck position.</param>
	public void SetNeckPosition(Vector3 neckPosition)
	{
		// This is locked to the NeckPosition that is set by the
		// Player profile.
		if(UsePlayerEyeHeight != true)
		{
			NeckPosition = neckPosition;
			UpdateCamerasDirtyFlag = true;
		}
	}

	/// <summary>
	/// Gets the orientation offset.
	/// </summary>
	/// <param name="orientationOffset">Orientation offset.</param>
	public void GetOrientationOffset(ref Quaternion orientationOffset)
	{
		orientationOffset = OrientationOffset;
	}
	/// <summary>
	/// Sets the orientation offset.
	/// </summary>
	/// <param name="orientationOffset">Orientation offset.</param>
	public void SetOrientationOffset(Quaternion orientationOffset)
	{
		OrientationOffset = orientationOffset;
	}
	
	/// <summary>
	/// Gets the Y rotation.
	/// </summary>
	/// <param name="yRotation">Y rotation.</param>
	public void GetYRotation(ref float yRotation)
	{
		yRotation = YRotation;
	}
	/// <summary>
	/// Sets the Y rotation.
	/// </summary>
	/// <param name="yRotation">Y rotation.</param>
	public void SetYRotation(float yRotation)
	{
		YRotation = yRotation;
	}

	/// <summary>
	/// Gets the X rotation.
	/// </summary>
	/// <param name="xRotation">X rotation.</param>
    public void GetXRotation(ref float xRotation)
    {
        xRotation = XRotation;
    }
	/// <summary>
	/// Sets the X rotation. Normally this should not be set by the game except
	/// for debugging.
	/// </summary>
	/// <param name="xRotation">X rotation.</param>
    public void SetXRotation(float xRotation)
    {
        XRotation = xRotation;
    }	
	
	/// <summary>
	/// Gets the tracker rotates y flag.
	/// </summary>
	/// <param name="trackerRotatesY">Tracker rotates y.</param>
	public void GetTrackerRotatesY(ref bool trackerRotatesY)
	{
		trackerRotatesY = TrackerRotatesY;
	}
	/// <summary>
	/// Sets the tracker rotates y flag.
	/// </summary>
	/// <param name="trackerRotatesY">If set to <c>true</c> tracker rotates y.</param>
	public void SetTrackerRotatesY(bool trackerRotatesY)
	{
		TrackerRotatesY = trackerRotatesY;
	}
	
	// GetCameraOrientationEulerAngles
	/// <summary>
	/// Gets the camera orientation euler angles.
	/// </summary>
	/// <returns><c>true</c>, if camera orientation euler angles was gotten, <c>false</c> otherwise.</returns>
	/// <param name="angles">Angles.</param>
	public bool GetCameraOrientationEulerAngles(ref Vector3 angles)
	{
		if(CameraRight == null)
			return false;
		
		angles = CameraRight.transform.rotation.eulerAngles;
		return true;
	}
	
	/// <summary>
	/// Gets the camera orientation.
	/// </summary>
	/// <returns><c>true</c>, if camera orientation was gotten, <c>false</c> otherwise.</returns>
	/// <param name="quaternion">Quaternion.</param>
	public bool GetCameraOrientation(ref Quaternion quaternion)
	{
		if(CameraRight == null)
			return false;
		
		quaternion = CameraRight.transform.rotation;
		return true;
	}

	/// <summary>
	/// Gets the camera forward vector.
	/// </summary>
	/// <param name="vector">Vector3.</param>
	public bool GetCameraForward( ref Vector3 vector )
	{
		if(CameraRight == null)
			return false;

		vector = CameraRight.transform.rotation * Vector3.forward;
		return true;
	}

	/// <summary>
	/// Gets the camera Ray from the position and forward
	/// </summary>
	/// <param name="ray">Ray.</param>
	public bool GetCameraRay( ref Ray ray )
	{
		if(CameraRight == null)
			return false;

		ray = new Ray( CameraRight.transform.position, CameraRight.transform.rotation * Vector3.forward );
		return true;
	}

	/// <summary>
	/// Gets the camera position.
	/// </summary>
	/// <returns><c>true</c>, if camera position was gotten, <c>false</c> otherwise.</returns>
	/// <param name="position">Position.</param>
	public bool GetCameraPosition(ref Vector3 position)
	{
		if(CameraRight == null)
			return false;
		
		position = CameraRight.transform.position;
	
		return true;
	}
	
	/// <summary>
	/// Gets the camera.
	/// </summary>
	/// <param name="camera">Camera.</param>
	public void GetCamera(ref Camera camera)
	{
		camera = CameraRight;
	}
	
	/// <summary>
	/// Attachs a game object to the right (main) camera.
	/// </summary>
	/// <returns><c>true</c>, if game object to camera was attached, <c>false</c> otherwise.</returns>
	/// <param name="gameObject">Game object.</param>
	public bool AttachGameObjectToCamera(ref GameObject gameObject)
	{
		if(CameraRight == null)
			return false;

		gameObject.transform.parent = CameraRight.transform;
	
		return true;
	}

	/// <summary>
	/// Detachs the game object from the right (main) camera.
	/// </summary>
	/// <returns><c>true</c>, if game object from camera was detached, <c>false</c> otherwise.</returns>
	/// <param name="gameObject">Game object.</param>
	public bool DetachGameObjectFromCamera(ref GameObject gameObject)
	{
		if((CameraRight != null) && (CameraRight.transform == gameObject.transform.parent))
		{
			gameObject.transform.parent = null;
			return true;
		}				
		
		return false;
	}

	/// <summary>
	/// Gets the camera depth.
	/// </summary>
	/// <returns>The camera depth.</returns>
	public float GetCameraDepth()
	{
		return CameraRight.depth;
	}

	// Get Misc. values from CameraController
	
	/// <summary>
	/// Gets the height of the player eye.
	/// </summary>
	/// <returns><c>true</c>, if player eye height was gotten, <c>false</c> otherwise.</returns>
	/// <param name="eyeHeight">Eye height.</param>
	public bool GetPlayerEyeHeight(ref float eyeHeight)
	{
		eyeHeight = CameraRootPosition.y + NeckPosition.y;  
		
		return true;
	}

	/// <summary>
	/// Sets the maximum visual quality.
	/// </summary>
	public void SetMaximumVisualQuality()
	{
		QualitySettings.softVegetation  = 		true;
		QualitySettings.maxQueuedFrames = 		0;			// currently only implemented for D3D renderer
		QualitySettings.anisotropicFiltering = 	AnisotropicFiltering.ForceEnable;
#if (UNITY_ANDROID && !UNITY_EDITOR)
		QualitySettings.vSyncCount = 0;	// JDC: we sync in the time warp, so we don't want unity syncing elsewhere
#endif
	}

	/// <summary>
	/// Issues Camera EndFrame Plugin Event
	/// </summary>
	public void CameraEndFrame( RenderEventType eventType )
	{
		int offs = (eventType == RenderEventType.LeftEyeEndFrame) ? 0 : 1;
		int texId = CameraTextureIds[EyeBufferNum[CurrEyeBufferIdx] * 2 + offs];
		//Debug.Log( "CameraTextureId[ " + offs + "] = " + texId );
		OVRPluginEvent.IssueWithData( eventType, texId );
	}

	/// <summary>
	/// Leaves the application and returns to the launcher/dashboard with confirmation
	/// </summary>
	public void ReturnToLauncher() {
#if UNITY_ANDROID && !UNITY_EDITOR
		// show the platform UI quit prompt
		OVRPluginEvent.Issue( RenderEventType.PlatformUIConfirmQuit );
#endif	
	}
}

public class OVRModeManager
{
	public static void EnterVRMode()
	{
#if (UNITY_ANDROID && !UNITY_EDITOR)
		OVRPluginEvent.Issue( RenderEventType.Resume );
#endif
	}
	
	public static void LeaveVRMode()
	{
#if (UNITY_ANDROID && !UNITY_EDITOR)
		OVRPluginEvent.Issue( RenderEventType.Pause );
#endif
	}
}
