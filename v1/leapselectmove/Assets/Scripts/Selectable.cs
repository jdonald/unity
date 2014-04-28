using UnityEngine;
using System.Collections;
using Leap;

public class Selectable : MonoBehaviour {
	
	Vector3 m_destination;
	bool m_selected = false;
	int m_selectionID = -1;
	GameObject m_selectionObject = null;
	
	Controller m_leapController;
	
	void OnTriggerEnter(Collider other) {
		if (other.tag != "FingerTip") return;
		m_selected = true;
		m_selectionID = other.GetComponent<LeapFinger>().m_hand.Id;
		m_selectionObject = other.gameObject;
	}
	
	void Start() {
		m_destination = transform.position;	
		m_leapController = new Controller();
	}
	
	void FixedUpdate () {
		
		Frame frame = m_leapController.Frame();
		
		if (m_selected) {
			int fingerCount = 0;
			for(int i = 0; i < frame.Hands.Count; ++i) {
				if (frame.Hands[i].Id == m_selectionID) {
					fingerCount = frame.Hands[i].Fingers.Count;
				}
			}
			
			if (fingerCount > 2 || fingerCount == 0) {
				m_selected = false;
			} else {
				m_destination = m_selectionObject.transform.position;
				m_destination.y = transform.position.y;
			}
		}
		Vector3 moveVec = (m_destination - transform.position);
		
		
		bool blocked = false;
		if(Physics.SphereCast(new Ray(transform.position, moveVec.normalized), 0.3f, 3)) {
			blocked = true;
			rigidbody.velocity *= 0.5f;
		}
		
		if (moveVec.magnitude > 2.0f && !blocked) rigidbody.AddForce(moveVec.normalized * 10.0f);
		else m_destination = transform.position;
		
		if (moveVec.magnitude > 0.1f) {
			transform.forward = moveVec;	
		}
	}
}
