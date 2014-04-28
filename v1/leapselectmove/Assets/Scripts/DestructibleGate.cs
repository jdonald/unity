using UnityEngine;
using System.Collections;

public class DestructibleGate : MonoBehaviour {
	
	public float m_destroySpeed = 20.0f;
	
	void OnTriggerEnter(Collider other) {
		if (other.tag != "FingerTip") return;
		if (other.rigidbody.velocity.magnitude < m_destroySpeed) return;
		Vector3 vel = other.rigidbody.velocity;
		
		foreach(Transform t in transform.GetComponentsInChildren<Transform>()) {
			t.parent = null;
			if (t.rigidbody == null) continue; 
			Vector3 randomVec = new Vector3(Random.Range(-5.0f, 5.0f), Random.Range(-15.0f, 15.0f), Random.Range(-5.0f, 5.0f));
			t.rigidbody.isKinematic = false;
			
			if (vel.y < 0) vel.y = 0;
			t.rigidbody.velocity = vel * 0.3f;
			t.rigidbody.angularVelocity = randomVec;
		}
		
	}
}
