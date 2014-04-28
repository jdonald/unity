using UnityEngine;
using System.Collections;
using Leap;

public class LeapHand : MonoBehaviour {

	GameObject [] m_fingers = new GameObject[5];
	GameObject [] m_knuckleLanges = new GameObject[6];
	GameObject m_wristJoint;

	bool m_initialized = false;

	public bool m_stale = false;

	public void UpdateFingers(Hand h, Transform parent, Vector3 offset) {
		for (int i = 0; i < m_fingers.Length; ++i) {
			LeapFinger finger = m_fingers[i].GetComponent<LeapFinger>();
			GameObject [] joints = finger.GetJoints();
			GameObject [] langes = finger.GetLanges();
			for(int j = 0; j < joints.Length; ++j) {
				Vector3 pos = parent.TransformPoint(offset + h.Fingers[i].JointPosition((Finger.FingerJoint) j).ToUnityScaled());
				Vector3 nextPos = parent.TransformPoint(offset + h.Fingers[i].JointPosition((Finger.FingerJoint) j + 1).ToUnityScaled());
				joints[j].transform.position = pos;
				joints[j].rigidbody.velocity = parent.TransformDirection(h.Fingers[i].TipVelocity.ToUnityScaled());
				Vector3 langePos = (pos + nextPos) * 0.5f;
				langes[j].transform.position = langePos;
				langes[j].transform.up = (nextPos - pos);
				Vector3 newScale = langes[j].transform.localScale;
				newScale.y = Mathf.Max(0.0f, (nextPos - pos).magnitude - 0.003f);
				langes[j].transform.localScale = newScale;
			}
			Vector3 tipPos = parent.TransformPoint(offset + h.Fingers[i].JointPosition(Finger.FingerJoint.JOINT_TIP).ToUnityScaled());
			finger.GetFingerTip().transform.position = tipPos;
			finger.GetFingerTip().rigidbody.velocity = parent.TransformDirection(h.Fingers[i].TipVelocity.ToUnityScaled());
			finger.m_fingerID = h.Fingers[i].Id;
		}
	}

	public void UpdateKnuckleLanges() {
		for (int i = 0; i < m_fingers.Length - 1; ++i) {
			Vector3 start = m_fingers[i].GetComponent<LeapFinger>().GetJoint(0).transform.position;
			Vector3 end = m_fingers[i + 1].GetComponent<LeapFinger>().GetJoint(0).transform.position;
			m_knuckleLanges[i].transform.position = (start + end) * 0.5f;
			m_knuckleLanges[i].transform.up = (start - end);
			Vector3 newScale = m_knuckleLanges[i].transform.localScale;
			newScale.y = Mathf.Max(0.0f, (start - end).magnitude - 0.003f);
			m_knuckleLanges[i].transform.localScale = newScale;
		}
		// get the last joint by reflecting the mcb bone of the thumb
		// across the hand direction.
		Vector3 d = transform.position - m_fingers[0].GetComponent<LeapFinger>().GetJoint(0).transform.position;
		Vector3 n = -transform.forward;
		Vector3 r = d - (2 * Vector3.Dot(d, n)) * n;
		m_wristJoint.transform.position = transform.position + r;
		Vector3 pinkymcb = m_fingers[4].GetComponent<LeapFinger>().GetJoint(0).transform.position;
		m_knuckleLanges[4].transform.position = (pinkymcb + m_wristJoint.transform.position) * 0.5f;
		m_knuckleLanges[4].transform.up = (pinkymcb - m_wristJoint.transform.position);
		Vector3 s = m_knuckleLanges[4].transform.localScale;
		s.y = Mathf.Max(0.0f, (pinkymcb - m_wristJoint.transform.position).magnitude);
		m_knuckleLanges[4].transform.localScale = s;

		Vector3 thumbmcb = m_fingers[0].GetComponent<LeapFinger>().GetJoint(0).transform.position;
		m_knuckleLanges[5].transform.position = (m_wristJoint.transform.position + thumbmcb) * 0.5f;
		m_knuckleLanges[5].transform.up = (m_wristJoint.transform.position - thumbmcb);
		s = m_knuckleLanges[5].transform.localScale;
		s.y = Mathf.Max(0.0f, (m_wristJoint.transform.position - thumbmcb).magnitude);
		m_knuckleLanges[5].transform.localScale = s;
	}

	public void Enable() {
		if (!m_initialized) return;
		for(int i = 0; i < m_fingers.Length; ++i) {
			m_fingers[i].GetComponent<LeapFinger>().Show(true);
		}
		for (int i = 0; i < m_knuckleLanges.Length; ++i) {
			m_knuckleLanges[i].renderer.enabled = true;
		}
		m_wristJoint.renderer.enabled = true;
		m_wristJoint.collider.enabled = true;
		renderer.enabled = true;
		collider.enabled = true;
	}

	public void Disable() {
		if (!m_initialized) return;
		for(int i = 0; i < m_fingers.Length; ++i) {
			m_fingers[i].GetComponent<LeapFinger>().Show(false);
		}
		for (int i = 0; i < m_knuckleLanges.Length; ++i) {
			m_knuckleLanges[i].renderer.enabled = false;
		}
		m_wristJoint.renderer.enabled = false;
		m_wristJoint.collider.enabled = false;
		renderer.enabled = false;
		collider.enabled = false;
	}

	void OnDestroy() {
		foreach(GameObject go in m_fingers) {
			Destroy(go);
		}
		foreach(GameObject go in m_knuckleLanges) {
			Destroy(go);
		}
		Destroy(m_wristJoint);
	}
	
	void Start () {
		// initialize the fingers.
		for (int i = 0; i < m_fingers.Length; ++i) {
			m_fingers[i] = Instantiate(Resources.Load("Prefabs/Finger")) as GameObject;
		}

		// initialize knuckle langes
		for (int i = 0; i < m_knuckleLanges.Length; ++i) {
			m_knuckleLanges[i] = Instantiate(Resources.Load("Prefabs/Lange")) as GameObject;
		}
		m_wristJoint = Instantiate(Resources.Load("Prefabs/JointSphere")) as GameObject;
		m_initialized = true;
	}

}
