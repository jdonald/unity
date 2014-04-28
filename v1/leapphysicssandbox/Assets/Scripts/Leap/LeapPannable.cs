using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Leap;

public class LeapPannable : MonoBehaviour {
	
	int m_collisionCount = 0;
	Controller m_leapController;
	Vector3 m_fingersVelocity = Vector3.zero;
	HashSet<int> m_collisionHandIDs = new HashSet<int>();
	float m_lastDistance = 0.0f;
	float m_originalHeight;
	bool m_scaling = false;
	int lastHandCount = 0;
	
	void Start() {
		m_leapController = new Controller();
		m_originalHeight = Camera.main.transform.position.y;
	}
	
	void OnTriggerStay(Collider other) {
		if (other.tag != "FingerTip") return;
		m_collisionCount++;
		m_fingersVelocity += other.rigidbody.velocity;
		m_collisionHandIDs.Add(other.GetComponent<LeapFinger>().m_hand.Id);
	}
	
	void Translate(Frame frame) {
		Vector3 avgVelocity = Vector3.zero;
		int avgCount = 0;
		for(int i = 0; i < frame.Fingers.Count; ++i) {
			if (m_collisionHandIDs.Contains(frame.Fingers[i].Hand.Id)) {
				avgVelocity += frame.Fingers[i].TipVelocity.ToUnityScaled();
				avgCount++;
			}
		}
		if (avgCount > 0) {
			avgVelocity /= avgCount;
			avgVelocity = new Vector3(avgVelocity.x, 0, avgVelocity.z);
			Camera.main.rigidbody.velocity = -avgVelocity;
		}
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		Frame frame = m_leapController.Frame();
		if (m_collisionCount > 2) {
			Translate(frame);
		}
		
		m_collisionCount = 0;
		m_collisionHandIDs.Clear();
	}
}
