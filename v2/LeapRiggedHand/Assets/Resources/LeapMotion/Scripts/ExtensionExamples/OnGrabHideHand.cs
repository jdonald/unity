using UnityEngine;
using System.Collections;

public class OnGrabHideHand : MonoBehaviour {
	void Update () {
		UnityHand unityHand = GetComponent<UnityHand>();
		if (unityHand.GetLeapHand().GrabStrength > 0.4f) {
			unityHand.EnableMesh(false);
		} else {
			unityHand.EnableMesh(true);
		}

	}
}
