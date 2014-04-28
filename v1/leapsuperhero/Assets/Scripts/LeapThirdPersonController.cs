using UnityEngine;
using System.Collections;
using Leap;

public class LeapThirdPersonController : MonoBehaviour {
	
	Controller m_leapController;
	
	void Start () {
		m_leapController = new Controller();
	}
	
	void LeapNavigation() {
		Frame frame = m_leapController.Frame();
	
		bool running = false;
		if (frame.Hands.Count == 2) {
			Hand leftHand = frame.Hands[0];
			Hand rightHand = frame.Hands[1];
			
			if (leftHand.PalmPosition.x > rightHand.PalmPosition.x) {
				leftHand = rightHand;
				rightHand = frame.Hands[0];
			}
			
			if (frame.Fingers.Count > 3) {
				transform.parent.rigidbody.velocity += transform.parent.forward * 0.1f;
				transform.parent.animation.CrossFade("walk");
				running = true;
			}
			
			if (Mathf.Abs(leftHand.PalmPosition.ToUnityScaled().z - rightHand.PalmPosition.ToUnityScaled().z) > 0.3f) {
				float rotScale = leftHand.PalmPosition.ToUnityScaled().z - rightHand.PalmPosition.ToUnityScaled().z;
				rotScale *= 0.01f;
				transform.parent.RotateAround(Vector3.up, rotScale);
			}
		}
		
		if (running == false) {
			transform.parent.animation.CrossFade("idle");	
		}
	}
	
	void Update () {
		LeapNavigation();
		
	}
}
