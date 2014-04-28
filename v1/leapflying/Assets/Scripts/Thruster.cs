using UnityEngine;
using System.Collections;

public class Thruster : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		Vector3 newRot = transform.localRotation.eulerAngles;
		newRot.x = transform.parent.rigidbody.velocity.magnitude * 7.0f;
		transform.localRotation = Quaternion.Euler(newRot);
		Debug.Log(newRot);
	}
}
