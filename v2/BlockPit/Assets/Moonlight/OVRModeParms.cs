/************************************************************************************

Filename    :   OVRResetOrientation.cs
Content     :   Helper component that can be dropped onto a GameObject to assist
			:	in resetting device orientation
Created     :   July 10, 2014
Authors     :   G

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
using System.Runtime.InteropServices;	// required for DllImport

public class OVRModeParms : MonoBehaviour {

	/*
		The July S5 SDK supported a mechanism to lock minimum clock rates
		for the CPU and GPU. However, the system could still choose to ramp the
		rates up under some internal algorithm. Turning values down wouldn't
		necessarily make the app run slower at a steady state -- it will start
		out slower, but may ramp back up to nearly the same values.

		The Note4 SDK provides an api to lock a fixed clock level for the CPU and GPU.
		For devices with builds which do not support this api, we fall back to using
		the minimum clock rate api.
	*/
	
	[DllImport("OculusPlugin")]
	// Set the fixed CPU clock level.
	private static extern void OVR_VrModeParms_SetCpuLevel( int cpuLevel );
	
	[DllImport("OculusPlugin")]
	// Set the fixed GPU clock level.
	private static extern void OVR_VrModeParms_SetGpuLevel( int gpuLevel );

	[DllImport("OculusPlugin")]
	// If true, when the application detects it has been throttled, the Platform
	// UI will display a dismissable warning, then returns to the app in powersave
	// mode at 30FPS.
	// If false, the Platform UI will display a non-dismissable error message
	// indicating the user must leave VR.
	// Set to false if the application continues to judder even when forced to
	// 30FPS in power save mode.
	private static extern void OVR_VrModeParms_SetAllowPowerSave( bool allow );

	[DllImport("OculusPlugin")]
	// Returns true if the application has been throttled.
	private static extern bool OVR_IsPowerSaveActive();

	[DllImport("OculusPlugin")]
	// Support to fix 60/30/20 FPS frame rate for consistency or power savings.
	private static extern void OVR_TW_SetMinimumVsyncs( OVRTimeWarpUtils.VsyncMode mode );

#region Member Variables

	public OVRGamepadController.Button	resetButton = OVRGamepadController.Button.X;	

#endregion

	/// <summary>
	/// Invoke power state mode test.
	/// </summary>
	void Start() {
		// Call TestPowerLevelState after 10 seconds 
		// and repeats every 10 seconds.
		InvokeRepeating ( "TestPowerStateMode", 10, 10.0f );
	}

	/// <summary>
	/// Change default vr mode parms.
	/// Call in Awake() before the plugin issues EnterVrMode setup
	/// </summary>
	void Awake() {
#if (UNITY_ANDROID && !UNITY_EDITOR)
		// De-clock to reduce power and thermal load.

		// Performance mode (default)
		OVR_VrModeParms_SetCpuLevel( 3 );
		OVR_VrModeParms_SetGpuLevel( 1 );
		OVR_TW_SetMinimumVsyncs( OVRTimeWarpUtils.VsyncMode.VSYNC_30FPS );
    Debug.Log("jdonald up-clocking the CPU here YO");
    Debug.Log("YO YO YO");
    Debug.Log("YO YO YO");
    Debug.Log("YO YO YO");
    Debug.Log("YO YO YO");
    Debug.Log("YO YO YO");

		// Power-save levels
		//OVR_VrModeParms_SetCpuLevel( 0 );
		//OVR_VrModeParms_SetGpuLevel( 0 );
		//OVR_TW_SetMinimumVsyncs( OVRTimeWarpUtils.VsyncMode.VSYNC_30FPS );

		// Enable Power Save Mode Handling
		OVR_VrModeParms_SetAllowPowerSave( false );
#endif
	}

	/// <summary>
	/// Change default vr mode parms dynamically.
	/// </summary>
	void Update() {

		// NOTE: some of the buttons defined in OVRGamepadController.Button are not available on the Android game pad controller
		if (/* Input.GetButtonDown( OVRGamepadController.ButtonNames[(int)resetButton] )*/ false ) {
			//*************************
			// Dynamically change VrModeParms cpu and gpu level.
			// NOTE: Reset will cause 1 frame of flicker as it leaves
			// and re-enters Vr mode.
			//*************************
#if (UNITY_ANDROID && !UNITY_EDITOR)
			OVR_VrModeParms_SetCpuLevel( 0 );
			OVR_VrModeParms_SetGpuLevel( 1 );
			OVRPluginEvent.Issue( RenderEventType.ResetVrModeParms );
#endif
		}
	}

	/// <summary>
	/// Check current power state mode.
	/// </summary>
	void TestPowerStateMode()
	{
#if (UNITY_ANDROID && !UNITY_EDITOR)
		//*************************
		// Check power-level state mode
		//*************************
		if ( OVR_IsPowerSaveActive() )
		{
			// The device has been throttled
			Debug.Log( "POWER SAVE MODE ACTIVATED" );
		}
#endif
	}

}
