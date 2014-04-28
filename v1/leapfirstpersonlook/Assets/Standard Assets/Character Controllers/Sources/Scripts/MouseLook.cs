using UnityEngine;
using System.Collections;
using Leap;

/// MouseLook rotates the transform based on the mouse delta.
/// Minimum and Maximum values can be used to constrain the possible rotation

/// To make an FPS style character:
/// - Create a capsule.
/// - Add the MouseLook script to the capsule.
///   -> Set the mouse look to use LookX. (You want to only turn character but not tilt it)
/// - Add FPSInputController script to the capsule
///   -> A CharacterMotor and a CharacterController component will be automatically added.

/// - Create a camera. Make the camera a child of the capsule. Reset it's transform.
/// - Add a MouseLook script to the camera.
///   -> Set the mouse look to use LookY. (You want the camera to tilt up and down like a head. The character already turns.)
[AddComponentMenu("Camera-Control/Mouse Look")]
public class MouseLook : MonoBehaviour {

	public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
	public RotationAxes axes = RotationAxes.MouseXAndY;
	public float sensitivityX = 15F;
	public float sensitivityY = 15F;

	public float minimumX = -360F;
	public float maximumX = 360F;

	public float minimumY = -60F;
	public float maximumY = 60F;
	
	Controller controller; // leap Controller

	float rotationY = 0F;
	
	int lastFrameFingerCount = 0;

	void Update ()
	{
		Frame frame = controller.Frame();
		
		float xInput = Input.GetAxis("Mouse X");
		float yInput = Input.GetAxis("Mouse Y");
		
		Vector3 averageFingerVelocity = Vector3.zero;
		
		for (int i = 0; i < frame.Fingers.Count; ++i) {
			averageFingerVelocity += frame.Fingers[i].TipVelocity.ToUnityScaled();	
		}
		
		averageFingerVelocity /= frame.Fingers.Count;
		
		float zFactor = Mathf.Abs(averageFingerVelocity.z);
		float xFactor = averageFingerVelocity.x * averageFingerVelocity.x + 1.0f;
		float yFactor = averageFingerVelocity.y * averageFingerVelocity.y + 1.0f;
		
		bool notPullingOrPushing = zFactor < xFactor && zFactor < yFactor;
		if (frame.Hands.Count > 0) {
			Vector3 palmPosition = frame.Hands[0].PalmPosition.ToUnityScaled();
			Vector3 palmVelocity = frame.Hands[0].PalmVelocity.ToUnityScaled();
			if (palmPosition.z > 0.0 && palmVelocity.magnitude < 8.0f && frame.Fingers.Count > 2) {
				xInput = averageFingerVelocity.x * Mathf.Abs(averageFingerVelocity.x);
				xInput *= 0.1f;
				yInput = averageFingerVelocity.y * Mathf.Abs(averageFingerVelocity.y);
				yInput *= 0.1f;
			}
			
			// perform a pull.
			if (frame.Fingers.Count < 1) {
				GameObject [] pullboxes = GameObject.FindGameObjectsWithTag("Pullable");
				for(int i = 0; i < pullboxes.Length; ++i) {
					if (pullboxes[i].renderer.isVisible) {
						Vector3 pullObjectToMe = transform.position - pullboxes[i].transform.position;
						pullboxes[i].rigidbody.velocity += pullObjectToMe.normalized * (3.0f / Mathf.Max(1.0f, pullObjectToMe.magnitude));
					}
				}
			}
		}
		
		if (axes == RotationAxes.MouseXAndY)
		{
			
			float rotationX = transform.localEulerAngles.y + xInput * sensitivityX;
			
			rotationY += yInput * sensitivityY;	
			rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
			
			transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
		}
		else if (axes == RotationAxes.MouseX)
		{
			transform.Rotate(0, xInput * sensitivityX, 0);
		}
		else
		{
			rotationY += yInput * sensitivityY;
			rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
			
			transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
		}
		lastFrameFingerCount = frame.Fingers.Count;
	}
	
	void Start ()
	{
		// Make the rigid body not change rotation
		if (rigidbody)
			rigidbody.freezeRotation = true;
		
		controller = new Controller(); // init leap controller
		UnityEngine.Screen.showCursor = false;
		UnityEngine.Screen.lockCursor = true;
	}
}