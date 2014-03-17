using UnityEngine;
using System.Collections;

public class OnGrabTurnRed : MonoBehaviour {
	void Update () {
		SkinnedMeshRenderer mesh = GetComponent<UnityHand>().GetMesh();
		if (GetComponent<UnityHand>().GetLeapHand().GrabStrength > 0.4f) {
			mesh.renderer.material.color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
		} else {
			mesh.renderer.material.color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
		}
	}
}
