using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Leap;

public class LeapPannable : MonoBehaviour {
	
	int m_collisionCount = 0;
	Controller m_leapController;
	Vector3 m_avgNormal = Vector3.zero;
	HashSet<int> m_collisionHandIDs = new HashSet<int>();

	void Start() {
		m_leapController = new Controller();
	}
	
	void OnCollisionStay(Collision collision) {
		if (collision.gameObject.tag != "FingerTip") return;
		m_collisionCount++;	
		for (int i = 0; i < collision.contacts.Length; ++i) {
			m_avgNormal += collision.contacts[i].normal;
		}

		m_collisionHandIDs.Add(collision.transform.GetComponent<LeapFinger>().m_hand.Id);
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
			if (Vector3.Dot(m_avgNormal.normalized, avgVelocity) > -0.1f) {
				Camera.main.rigidbody.velocity = -avgVelocity;
			}
		}
	}
	
	void FixedUpdate () {
		Frame frame = m_leapController.Frame();
		if (m_collisionCount > 1) {
			Translate(frame);
		}
		
		m_collisionCount = 0;
		m_collisionHandIDs.Clear();
		m_avgNormal = Vector3.zero;
	}
}
