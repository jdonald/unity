using UnityEngine;
using System.Collections;

public class Harvester : MonoBehaviour {
	
	void OnTriggerStay(Collider other) {
		if (other.name != "Gem") return;	
		particleSystem.enableEmission = true;
	}
	void FixedUpdate () {
		particleSystem.enableEmission = false;
	}
}
