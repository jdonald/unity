/************************************************************************************

Filename    :   MathUtils.cs
Content     :   Math-related utility functions.
Created     :   March 5, 2014
Authors     :   Jonathan E. Wright

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
using System.Runtime.InteropServices;

public static class MathUtils
{
	public const float COSINE_5_DEGREES = 0.996194698f;
	public const float COSINE_22_PT_5_DEGREES = 0.9238795325f;
	public const float COSINE_30_DEGREES = 0.86602540378f;
	public const float COSINE_45_DEGREES = 0.7071067812f;
	public const float COSINE_60_DEGREES = 0.5f;
	public const float RAD2DEG = 180.0f / Mathf.PI;
	public const float DEG2RAD = Mathf.PI / 180.0f;
	
	public const float FLT_SMALL_NUMBER = 1.0842021724855044e-019f;
	public const float FLT_SMALLEST_NON_DENORMAL = 1.1754943508222875e-038f;

	//======================
	// SineFromCosine
	// Derives sine from a cosine using trigonometric identities. Note that
	// the dot product of two normalized vectors is the cosine of the angle
	// between them -- so this can also derive a sine given a dot product.
	//======================
	public static float SineFromCosine( float cosine )
	{
		return Mathf.Sqrt( Mathf.Abs( 1.0f - ( cosine * cosine ) ) );
	}

	//======================
	// CosineOfHalfAngleFromCosine
	// Given the cosine of an angle, this will return the cosine of 1/2 the angle.
	//======================
	public static float CosineOfHalfAngleFromCosine( float cosine )
	{
		return Mathf.Sqrt( ( 1.0f + cosine ) * 0.5f );
	}
	
	//======================
	// CosineOfDoubleAngleFromCosine
	// Returns the cosine of the angle which is 2x the angle represented by cosine.
	//======================
	public static float CosineOfDoubleAngleFromCosine( float cosine )
	{
		return 2.0f * ( cosine * cosine ) - 1.0f;
	}
	
	//======================
	// Normalize
	// Normalizes a quaternion.  This can be necessary to repair floating point error.
	//======================
	public static void Normalize( ref Quaternion q )
	{
		float mag = Mathf.Sqrt( q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w );
		// if the magnitude is becoming very small, let us know that we're risking a denormal
		DebugUtils.Assert( mag > FLT_SMALLEST_NON_DENORMAL );	
		float inverseMag = 1.0f /  mag;
		q.x *= inverseMag;
		q.y *= inverseMag;
		q.z *= inverseMag;
		q.w *= inverseMag;
	}
	
	//======================
	// QuaternionFromAxisAndDotProduct
	// Constructs a quaternion from the axis and a dot product representing 
	// the rotation around the axis.  This will always construct a rotation
	// in the positive direction.
	//======================
	public static Quaternion QuaternionFromAxisAndDotProduct( Vector3 axis, float dot )
	{
		Quaternion q;
		q.w = CosineOfHalfAngleFromCosine( dot );
		float halfAngleSine = SineFromCosine( q.w );
		q.x = axis.x * halfAngleSine;
		q.y = axis.y * halfAngleSine;
		q.z = axis.z * halfAngleSine;
		return q;
	}
	
	//======================
	// QuaternionFromFwd
	// Constructs a quaternion rotation from a forward and up axis.
	//======================
	public static Quaternion QuaternionFromFwd( Vector3 fwd, Vector3 up )
	{
		return Quaternion.LookRotation( fwd, up );
	}
	
	//======================
	// RotationBetweenToVectors
	// Constructs a quaternion that will rotate the from vector to the to vector.
	// This assumes from and to are normalized.
	//======================
	public static Quaternion RotationBetweenTwoVectors( Vector3 from, Vector3 to )
	{
/*	
		float dot = Vector3.Dot( from, to );
		if ( dot > 0.9999f )
		{
			DebugUtils.Assert( dot <= 0.9999f );
			return Quaternion.identity;
		}

		Quaternion q;		
		q.w = CosineOfHalfAngleFromCosine( dot );
		float sineHalfAngle = SineFromCosine( q.w );

		// get a vector orthogonal to both from and to
		Vector3 axis = Vector3.Cross( from, to );
		DebugUtils.Assert( axis.magnitude > 0.0001f );		
		q.x = axis.x * sineHalfAngle;
		q.y = axis.y * sineHalfAngle;
		q.z = axis.z * sineHalfAngle;
		
		return q;
*/	
		Quaternion q;
		q.x = from.y * to.z - from.z * to.y;
		q.y = from.z * to.x - from.x * to.z;
		q.z = from.x * to.y - from.y * to.x;
		q.w = to.x * from.x + to.y * from.y + to.z * from.z + 1.0f;
		DebugUtils.Assert( q.w > 1e-18f );
		Normalize( ref q );
		return q;
	}
	
	//======================
	// ProjectOntoPlane
	// Returns vector v projected onto a plane with the specified normal.
	//======================
	public static Vector3 ProjectOntoPlane( Vector3 v, Vector3 normal )
	{
		float dot = Vector3.Dot( v, normal );
		return v - ( normal * dot );
	}
	
	//======================
	// ProjectOntoPlaneAndNormalize
	// Returns a normalized vector v projected onto a plane with the specified normal.
	//======================	
	public static Vector3 ProjectOntoPlaneAndNormalize( Vector3 v, Vector3 normal )
	{
		float dot = Vector3.Dot( v, normal );
		Vector3 outv = v - ( normal * dot );
		outv.Normalize();
		return outv;
	}	

	//======================
	// QuaternionIsNormalized
	// Returns true if the quaternion is normalized within epsilon.
	//======================	
	public static bool QuaternionIsNormalized( Quaternion a, float epsilon )
	{
		float len = ( a.x * a.x + a.y * a.y + a.z * a.z + a.w * a.w );
		return Mathf.Abs( 1.0f - len ) < epsilon;
	}
	
	//======================
	// QuaternionDot
	// Returns the dot product of two quaternions.  Note this is not the cosine
	// of the angle between the quaternions' forward vectors, but the cosine of
	// half the angle between them.
	//======================
	public static float QuaternionDot( Quaternion a, Quaternion b )
	{
		DebugUtils.Assert( QuaternionIsNormalized( a, 0.001f ) );
		DebugUtils.Assert( QuaternionIsNormalized( b, 0.001f ) );
		return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
	}
	
	//======================
	// CosineOfAngleBetweenQuaternions
	// Returns the cosine of the angle between quaternions.  This just finds
	// the double angle for the quaternion dot product.
	//======================
	public static float CosineOfAngleBetweenQuaternions( Quaternion a, Quaternion b )
	{
		float dot = QuaternionDot( a, b );
		return CosineOfDoubleAngleFromCosine( dot );
	}
	
	//======================
	// Clamp
	// Clamps a quaternion rotation so that it remains within the specified angle
	// from identity.
	//======================
	public static bool Clamp( ref Quaternion q, float cosineOfClampAngle )
	{
		float cosineOfHalfClampAngle = CosineOfHalfAngleFromCosine( cosineOfClampAngle );
		if ( q.w >= cosineOfHalfClampAngle )
		{		
			return false;	// already inside of the clamp
		}
		if ( q.w > 0.99999f )
		{
			q = Quaternion.identity;
			return true;
		}		
		
		float s = SineFromCosine( q.w );
		DebugUtils.Assert( s > MathUtils.FLT_SMALLEST_NON_DENORMAL );
	
		Vector3 axis;
		axis.x = q.x / s;
		axis.y = q.y / s;
		axis.z = q.z / s;
		axis.Normalize ();
		
		float sineOfHalfClampAngle = SineFromCosine( cosineOfHalfClampAngle );
		q.x = axis.x * sineOfHalfClampAngle;
		q.y = axis.y * sineOfHalfClampAngle;
		q.z = axis.z * sineOfHalfClampAngle;
		q.w = cosineOfHalfClampAngle;
		return true;
	}
	
	//======================
	// Forward
	// Returns the forward axis of the quaternion.
	// This is just a helper to get the forward axis of a quaternion.
	//======================
	public static Vector3 Forward( Quaternion q )
	{
		return q * Vector3.forward;
	}
	
	//======================
	// ClampOnAxis
	// Clamps the passed quaternion to a maximum angle on the specified axis.  Returns true
	// if the quaternion was outside of the max angle (i.e. was clamped). This is useful for
	// AI to know when to turn their body, vs. head.
	//======================
	public static bool ClampOnAxis( ref Quaternion q, Vector3 axis, float cosineOfClampAngle )
	{
		Vector3 qfwd = Forward( q );
		qfwd = ProjectOntoPlaneAndNormalize( qfwd, axis );
		float dot = Vector3.Dot( Vector3.forward, qfwd );
		if ( dot >= cosineOfClampAngle )
		{
			return false;	// already inside of the constraint angle on this axis
		}
		// construct a quaternion that is the clamped rotation on the correct side
		Quaternion clampRot = QuaternionFromAxisAndDotProduct( axis, cosineOfClampAngle );
		float rightDot = Vector3.Dot( Vector3.right, qfwd );
		if ( rightDot < 0.0f )
		{
			clampRot = Quaternion.Inverse( clampRot );
		}
		// find the difference between the quaternion (projected onto axis) and the
		// clamped rotation, then rotate the full (unprojected) quaternion back by
		// this difference.
		Quaternion projected = QuaternionFromAxisAndDotProduct( axis, dot );
		if ( rightDot < 0.0f )
		{
			projected = Quaternion.Inverse( projected );
		}
		Quaternion deltaq = Quaternion.Inverse( clampRot ) * projected;
		q = Quaternion.Inverse( deltaq ) * q;
		
		return true;	// clamped
	}
}