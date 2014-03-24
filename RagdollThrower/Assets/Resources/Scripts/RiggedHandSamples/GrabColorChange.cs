using UnityEngine;
using System.Collections;
using Leap;

public class GrabColorChange : MonoBehaviour {

	SkeletalHandController m_sHandController;
	RiggedHandController m_rHandController;

	// Use this for initialization
	void Start () {
		m_sHandController = GetComponent<SkeletalHandController>();
		m_rHandController = GetComponent<RiggedHandController>();
	}

	// uses LateUpdate to make sure we have the most current hand data.
	void LateUpdate () {
		GameObject [] hands = m_sHandController.GetHandGameObjects();
		for (int i = 0; i < hands.Length; ++i) {
			Hand h = hands[i].GetComponent<LeapHand>().GetRawHand();
			LeapRiggedHand riggedhand = m_rHandController.GetHandGameObjectById(h.Id).GetComponent<LeapRiggedHand>();
			SkinnedMeshRenderer mesh = riggedhand.GetMesh();
			if (h.GrabStrength > 0.4f) {
				mesh.renderer.material.color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
			} else {
				mesh.renderer.material.color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
			}
		}
	}
}
