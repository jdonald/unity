using UnityEngine;
using System.Collections;

public class LeapButtonRegion : MonoBehaviour {

	void OnCollisionStay(Collision collision) {
		if (collision.transform.gameObject.tag != "FingerTip") return;
		if (float.IsNaN(collision.contacts[0].point.x)) return;

		GetComponentInChildren<LeapButton>().m_regionCollisionPoint = collision.contacts[0].point;
		GetComponentInChildren<LeapButton>().m_regionCollision = true;
	}
}
