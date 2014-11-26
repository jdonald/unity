/************************************************************************************

Filename    :   OVRGamepadController.cs
Content     :   Interface to gamepad controller
Created     :   January 8, 2013
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
using System;
using System.Runtime.InteropServices;


//-------------------------------------------------------------------------------------
// ***** OVRGamepadController
//

/// <summary>
/// OVRGamepadController is an interface class to a gamepad controller.
/// </summary>
public class OVRGamepadController : MonoBehaviour
{	
	//-------------------------
	// Input enums
	public enum Axis { 
		None = -1,
		LeftXAxis = 0, 
		LeftYAxis, 
		RightXAxis, 
		RightYAxis, 
		LeftTrigger, 
		RightTrigger,
		Max 
	};
	
	public enum Button { 
		None = -1,
		A = 0,
		B, 
		X, 
		Y, 
		Up, 
		Down, 
		LeftShoulder, 
		RightShoulder, 
		Start, 
		Back, 
		LStick, 
		RStick, 
		L1, 
		R1,
		Max
	};
	

    public static string[] DefaultAxisNames = new string[(int)Axis.Max] 
	{
		"Left_X_Axis", 
		"Left_Y_Axis", 
		"Right_X_Axis", 
		"Right_Y_Axis", 
		"LeftTrigger", 
		"RightTrigger"
	};

    public static string[] DefaultButtonNames = new string[(int)Button.Max] 
	{
		"Button A", 
		"Button B", 
		"Button X", 
		"Button Y", 
		"Up", 
		"Down", 
		"Left Shoulder", 
		"Right Shoulder", 
		"Start", 
		"Back", 
		"LStick", 
		"RStick", 
		"LeftShoulder", 
		"RightShoulder"
	};

    public static int[] DefaultButtonIds = new int[(int)Button.Max]
	{
		0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13
	};

    public static string[] AxisNames = null;
    public static string[] ButtonNames = null;

    static OVRGamepadController()
    {
        SetAxisNames(DefaultAxisNames);
        SetButtonNames(DefaultButtonNames);
    }

	public static void SetAxisNames( string [] axisNames )
	{
		AxisNames = axisNames;
	}
	
	public static void SetButtonNames( string [] buttonNames )
	{
		ButtonNames = buttonNames;
	}

	public delegate float ReadAxisDelegate( Axis axis );
	public delegate bool  ReadButtonDelegate( Button button );
	
	public static ReadAxisDelegate ReadAxis = DefaultReadAxis;
	public static ReadButtonDelegate ReadButton = DefaultReadButton;
	
	/// <summary>
	/// GPC_GetAxis
	/// The default a delegate for retrieving axis info.
	/// </summary>
	/// <returns>The current value of the axis.</returns>
	/// <param name="axis">Axis.</param>
	public static float DefaultReadAxis( Axis axis)
	{
		return Input.GetAxis( AxisNames[(int)axis] );
	}
	
	public static float GPC_GetAxis( Axis axis ) 
	{
		return ReadAxis( axis );
	}
	
	public static void SetReadAxisDelegate( ReadAxisDelegate del )
	{
		ReadAxis = del;
	}

	/// <summary>
	/// GPC_GetButton
	/// </summary>
	/// <returns><c>true</c>, if c_ get button was GPed, <c>false</c> otherwise.</returns>
	/// <param name="button">Button.</param>
	public static bool DefaultReadButton( Button button )
	{
		return Input.GetButton( ButtonNames[(int)button] );
	}
	
	public static bool GPC_GetButton( Button button )
	{
		return ReadButton( button );
	}

	public static void SetReadButtonDelegate( ReadButtonDelegate del )
	{
		ReadButton = del;
	}
	
}
