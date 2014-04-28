using UnityEngine;
using System.Collections;

public class DestructibleGun : MonoBehaviour {

	void OnTriggerEnter(Collider other) {
		if (other.tag != "FingerTip") return;
		Vector3 vel = other.rigidbody.velocity;
		transform.parent = null;
		Debug.Log("we're hit!");
		rigidbody.isKinematic = false;
		collider.isTrigger = false;
		rigidbody.velocity = vel;
	
	}
}
