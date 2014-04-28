using UnityEngine;
using System.Collections;
using Leap;

public class LeapSceneHandsPanner : MonoBehaviour {
	Controller m_leapController;
	void Start () {
		m_leapController = new Controller();
	}
	
	void FixedUpdate () {
		Frame frame = m_leapController.Frame();

		// find the hand with the most fingers.
		for(int i = 0; i < frame.Hands.Count; ++i) {
			if (frame.Hands[i].Fingers.Count < 3) continue;
			
			float xVal = frame.Hands[i].PalmPosition.ToUnityScaled().x * 0.1f;
			xVal *= Mathf.Abs(xVal);
			
			if (frame.Hands[i].PalmPosition.ToUnityScaled().x < -6f) {
				Camera.main.rigidbody.velocity += new Vector3(xVal, 0, 0);	
			}
			
			if (frame.Hands[i].PalmPosition.ToUnityScaled().x > 6f) {
				Camera.main.rigidbody.velocity += new Vector3(xVal, 0, 0);	
			}
			
			float zVal = frame.Hands[i].PalmPosition.ToUnityScaled().z * 0.1f;
			zVal *= Mathf.Abs(zVal);
			
			if (frame.Hands[i].PalmPosition.ToUnityScaled().z < -4f) {
				Camera.main.rigidbody.velocity += new Vector3(0, 0, zVal);
			}
			
			if (frame.Hands[i].PalmPosition.ToUnityScaled().z > 4f) {
				Camera.main.rigidbody.velocity += new Vector3(0, 0, zVal);	
			}	
		}
	}
}
