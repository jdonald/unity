using UnityEngine;
using System.Collections;

public class DestructibleChildren : MonoBehaviour {
	
	
	void OnCollisionEnter(Collision colInfo) {
		if (colInfo.relativeVelocity.magnitude > 10.0f) {
			Transform [] children = GetComponentsInChildren<Transform>();
			for(int i = 0; i < children.Length; ++i) {
				if (children[i] == transform) continue;
				children[i].gameObject.AddComponent<Rigidbody>();
				children[i].rigidbody.velocity = rigidbody.velocity;
				children[i].transform.parent = null;
			}
		}
	}

}
