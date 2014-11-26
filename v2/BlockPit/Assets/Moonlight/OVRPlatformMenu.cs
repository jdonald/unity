/************************************************************************************

Filename    :   OVRPlatformMenu.cs
Content     :   An example of detecting a long-press and activating the global menu.
Created     :   August 14, 2014

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
using System.Collections;

public class OVRPlatformMenu : MonoBehaviour {

	public GameObject			CursorTimer;
	//set default color to same as native cursor timer
	public Color 				CursorTimerColor = new Color(0.0f, 0.643f, 1.0f, 1.0f);
	public OVRCameraController	cameraController = null;
	public float				fixedDepth = 3.0f;

	private GameObject			InstantiatedCursorTimer = null;
	private Material			CursorTimerMaterial = null;
	private float				doubleTapDelay = 0.25f;
	private float				longPressDelay = 0.75f;
	private float				homeButtonDownTime = 0.0f;

	private bool				platformUIStarted = false;

	/// <summary>
	/// Instantiate the cursor timer
	/// </summary>
	void Awake() {
		if ( cameraController == null ) {
			Debug.LogError ("ERROR: missing camera controller object on " + name);
			enabled = false;
			return;
		}
		if ( ( CursorTimer != null ) && ( InstantiatedCursorTimer == null ) ) {
			Debug.Log( "Instantiating CursorTimer" );
			InstantiatedCursorTimer = Instantiate( CursorTimer ) as GameObject;
			if ( InstantiatedCursorTimer != null ) {
				CursorTimerMaterial = InstantiatedCursorTimer.GetComponent<Renderer>().material;
				CursorTimerMaterial.SetColor ( "_Color", CursorTimerColor ); 
				InstantiatedCursorTimer.GetComponent<Renderer>().enabled = false;
			}
		}
		// reset each time we resume/start 
		platformUIStarted = false;
	}

	/// <summary>
	/// Destroy the cloned material
	/// </summary>
	void OnDestroy() {
		if ( CursorTimerMaterial != null ) {
			Destroy( CursorTimerMaterial );
		}
	}

	/// <summary>
	/// Reset when resuming
	/// </summary>
	void OnApplicationFocus() {
		platformUIStarted = false;
	}

	/// <summary>
	/// Show the confirm quit menu
	/// </summary>
	void ShowConfirmQuitMenu() {
		Debug.Log( "[PlatformUI-ConfirmQuit] Showing @ " + Time.time );
#if UNITY_ANDROID && !UNITY_EDITOR
		OVRPluginEvent.Issue( RenderEventType.PlatformUIConfirmQuit );
		platformUIStarted = true;
#endif
	}

	/// <summary>
	/// Show the platform UI global menu
	/// </summary>
	void ShowGlobalMenu() {
		Debug.Log( "[PlatformUI-Global] Showing @ " + Time.time );
#if UNITY_ANDROID && !UNITY_EDITOR
		OVRPluginEvent.Issue( RenderEventType.PlatformUI );
		platformUIStarted = true;
#endif
	}
	
	/// <summary>
	/// Tests for long-press and activates global platform menu when detected.
	/// as per the Unity integration doc, the back button responds to "mouse 1" button down/up/etc
	/// </summary>
	void Update () {
		if ( !platformUIStarted ) {
			// process input for the home button
			if ( Input.GetKeyDown (KeyCode.Escape) ) {

				CancelInvoke( "ShowConfirmQuitMenu" );
				CancelInvoke( "ShowGlobalMenu" );

				if ( Time.realtimeSinceStartup < ( homeButtonDownTime + doubleTapDelay ) ) {
					// reset so the menu doesn't pop up after resetting orientation
					homeButtonDownTime = 0.0f;
					// reset the HMT orientation
					//OVRDevice.ResetOrientation();
				} else {
					homeButtonDownTime = Time.realtimeSinceStartup;
				}
			} else if ( Input.GetKeyUp( KeyCode.Escape ) ) {
				float elapsedTime = ( Time.realtimeSinceStartup - homeButtonDownTime );
				if ( elapsedTime < longPressDelay ) {
					if ( elapsedTime >= doubleTapDelay ) {
						CancelInvoke( "ShowGlobalMenu" );
						CancelInvoke( "ShowConfirmQuitMenu" );
					} else {
						Invoke( "ShowConfirmQuitMenu", ( doubleTapDelay - elapsedTime ) );
					}
				}
				
				// reset the timer cursor any time escape released
				ResetCursor ();
			} else if ( Input.GetKey( KeyCode.Escape ) ) {
				float elapsedHomeButtonDownTime = Time.realtimeSinceStartup - homeButtonDownTime;

				if ( elapsedHomeButtonDownTime > doubleTapDelay ) {
					//Update the timer cursor using the amount of time we've held down for long press		
					UpdateCursor ( elapsedHomeButtonDownTime / longPressDelay );
				}

				// Check for long press
				if ( elapsedHomeButtonDownTime >= longPressDelay && ( homeButtonDownTime > 0.0f ) ) {
					// reset so something else doesn't trigger afterwards
					Input.ResetInputAxes();
					homeButtonDownTime = 0.0f;

					// Reset the timer cursor once long press activated
					ResetCursor ();

					CancelInvoke( "ShowConfirmQuitMenu" );
					Invoke( "ShowGlobalMenu", 0 );
				}
			} 
		}
	}

	/// <summary>
	/// Update the cursor based on how long the back button is pressed
	/// </summary>
	void UpdateCursor ( float timerRotateRatio ) {
		if ( InstantiatedCursorTimer != null ) {
			InstantiatedCursorTimer.GetComponent<Renderer>().enabled = true;

			// Clamp the rotation ratio to avoid rendering artifacts
			float alphaAmount = Mathf.Clamp ( 1.0f - timerRotateRatio, 0.0f, 1.0f );
			CursorTimerMaterial.SetFloat ( "_Cutoff", alphaAmount );

			// Draw timer at fixed distance in front of camera
			Vector3 cameraPosition = Vector3.zero;
			Vector3 cameraForward = Vector3.forward;
			if ( cameraController.GetCameraForward( ref cameraForward ) &&
			    	cameraController.GetCameraPosition( ref cameraPosition ) ){
				// cursor positions itself based on camera forward and draws at a fixed depth
				InstantiatedCursorTimer.transform.position = cameraPosition + ( cameraForward * fixedDepth );
				InstantiatedCursorTimer.transform.forward = cameraForward;
			}
		}
	}

	/// <summary>
	/// Reset the cursor
	/// </summary>
	void ResetCursor () {
		if ( InstantiatedCursorTimer != null ) {
			CursorTimerMaterial.SetFloat ( "_Cutoff", 1.0f );
			InstantiatedCursorTimer.GetComponent<Renderer>().enabled = false;
		}
	}
}
