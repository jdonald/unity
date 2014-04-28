using UnityEngine;
using System.Collections;

public class OnPinchSelect : MonoBehaviour {

	bool m_pinchLastFrame = false;
	GameObject m_selected = null;

	void OnPinchEnter(Vector3 pinchPos) {
		// perform sphere cast and assign if found.
		Collider [] candidates = Physics.OverlapSphere(pinchPos, 4.0f);

		float minDist = float.MaxValue;
		for (int i = 0; i < candidates.Length; ++i) {
			float dist = Vector3.Distance(pinchPos, candidates[i].transform.position);
			if (candidates[i].name == "Cube" && dist < minDist) {
				m_selected = candidates[i].gameObject;
				minDist = dist;
			}
		}
	}

	void OnPinchExit() {
		m_selected = null;
	}

	void Update () {
		Leap.Hand hand = GetComponent<UnityHand>().GetLeapHand();
		Vector3 thumbTipPos = GetComponent<UnityHand>().GetFingers()[0].GetComponent<UnityFinger>().GetJoints()[3].transform.position;
		bool pinching = hand.PinchStrength > 0.5f;

		if (pinching && !m_pinchLastFrame) {
			OnPinchEnter(thumbTipPos);
		}

		if (!pinching && m_pinchLastFrame) {
			OnPinchExit();
		}

		if (m_selected != null) {
			// update
			m_selected.transform.position = thumbTipPos;
		}

		m_pinchLastFrame = pinching;
	}
}
