using UnityEngine;
using System.Collections;

public class LeapTurnTableRegion : MonoBehaviour {
	void OnCollisionStay(Collision collision) {
		if (collision.transform.gameObject.tag != "FingerTip") return;
		if (float.IsNaN(collision.contacts[0].point.x)) return;

		GetComponentInChildren<LeapTurnTable>().m_regionCollisionPoint = collision.contacts[0].point;
		GetComponentInChildren<LeapTurnTable>().m_regionCollision = true;
	}
}
