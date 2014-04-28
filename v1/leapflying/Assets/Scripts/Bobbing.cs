using UnityEngine;
using System.Collections;

public class Bobbing : MonoBehaviour {

	// Update is called once per frame
	void Update () {
		rigidbody.velocity = Vector3.up * Mathf.Sin(Time.time);
		rigidbody.angularVelocity = Vector3.up;
	}
}
