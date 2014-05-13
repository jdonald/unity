using UnityEngine;
using System.Collections;

public class WaveGenerator : MonoBehaviour {

	// Use this for initialization
	void Start () {
		InvokeRepeating("SpawnWave", 0, 3);
	}

	void SpawnWave() {
		GameObject obj = Instantiate(Resources.Load("Prefabs/BlockGame/Wave")) as GameObject;
		obj.transform.position = new Vector3(0, 0, 30);
		Rigidbody [] children = obj.GetComponentsInChildren<Rigidbody>();

		for(int i = 0; i < children.Length; ++i) {
			children[i].velocity = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-3.0f, -8.0f));
			children[i].angularVelocity = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
		}
	}
	
}
