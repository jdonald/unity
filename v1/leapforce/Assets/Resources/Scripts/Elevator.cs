using UnityEngine;
using System.Collections;

public class Elevator : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	void OnTriggerStay(Collider other) {
		if (other.name == "Player") {
			transform.root.position = Vector3.Lerp(transform.root.position, transform.root.position + new Vector3(0, 67, 0), 0.001f);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
