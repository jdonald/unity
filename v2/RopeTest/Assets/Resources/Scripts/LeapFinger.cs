using UnityEngine;
using System.Collections;

public class LeapFinger : MonoBehaviour {

	GameObject [] m_joints = new GameObject[3];
	GameObject [] m_langes = new GameObject[3];
	GameObject m_fingerTip;
	public Color m_color;
	public int m_fingerID = 0;

	public GameObject [] GetLanges() {
		return m_langes;
	}

	public GameObject [] GetJoints() {
		return m_joints;
	}

	public GameObject GetJoint(int id) {
		return m_joints[id];
	}

	public GameObject GetFingerTip() {
		return m_fingerTip;
	}

	public void SetParent(Transform parent) {
		for (int i = 0; i < m_joints.Length; ++i) {
			m_joints[i].transform.parent = parent;
		}
		for (int i = 0; i < m_joints.Length; ++i) {
			m_langes[i].transform.parent = parent;
		}
		m_fingerTip.transform.parent = parent;
	}

	public void Show(bool shouldShow) {
		for (int i = 0; i < m_joints.Length; ++i) {
			m_joints[i].renderer.enabled = shouldShow;
			m_joints[i].collider.enabled = shouldShow;
		}
		for (int i = 0; i < m_langes.Length; ++i) {
			m_langes[i].renderer.enabled = shouldShow;
		}
		m_fingerTip.renderer.enabled = shouldShow;
		m_fingerTip.collider.enabled = shouldShow;
	}

	void OnDestroy() {
		foreach(GameObject go in m_joints) {
			Destroy(go);
		}
		foreach(GameObject go in m_langes) {
			Destroy(go);
		}
		Destroy(m_fingerTip);
	}

	void Awake() {
		// init joints
		for (int i = 0; i < m_joints.Length; ++i) {
			m_joints[i] = Instantiate(Resources.Load("Prefabs/JointSphere")) as GameObject;
			m_joints[i].renderer.material.color = m_color;
		}
		// init langes
		for (int i = 0; i < m_langes.Length; ++i) {
			m_langes[i] = Instantiate(Resources.Load("Prefabs/Lange")) as GameObject;
		}

		// init finger tip
		m_fingerTip = Instantiate(Resources.Load("Prefabs/TipSphere")) as GameObject;
	}

}
