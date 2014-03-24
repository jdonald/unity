using UnityEngine;
using System.Collections;

public class HandDataExample : MonoBehaviour {

	SkeletalHandController m_sHandController;

	// Use this for initialization
	void Start () {
		m_sHandController = GetComponent<SkeletalHandController>();
	}
	
	// Update is called once per frame
	void Update () {
		GameObject [] hands = m_sHandController.GetHandGameObjects();
		for (int i = 0; i < hands.Length; ++i) {
			Vector3 unityWorldHandPosition = hands[i].transform.position;
			GameObject [] fingers = hands[i].GetComponent<LeapHand>().GetFingers();
			for (int j = 0; j < fingers.Length; ++j) {
				LeapFinger finger = fingers[i].GetComponent<LeapFinger>();
				Vector3 unityWorldTipPosition = finger.GetFingerTip().transform.position;
				Vector3 unityWorldmcpPosition = finger.GetJoint(0).transform.position;
			}
		}
	}
}
