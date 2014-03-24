using UnityEngine;
using System.Collections;
using Leap;

public class PinchGrab : MonoBehaviour {

	SkeletalHandController m_sHandController;
	RiggedHandController m_rHandController;

  const int MAX_HANDS = 2;
  Collider[] grabbed_ = new Collider[MAX_HANDS];
  bool[] pinching_ = new bool[MAX_HANDS];

	// Use this for initialization
	void Start () {
		m_sHandController = GetComponent<SkeletalHandController>();
		m_rHandController = GetComponent<RiggedHandController>();
    for (int i = 0; i < MAX_HANDS; ++i) {
      grabbed_[i] = null;
      pinching_[i] = false;
    }
	}

  void OnPinch(int hand, Vector3 pinch_position) {
    pinching_[hand] = true;
    Collider[] close_things = Physics.OverlapSphere(pinch_position, 4.0f);

    Vector3 distance = new Vector3(4.0f, 0.0f, 0.0f);
    for (int j = 0; j < close_things.Length; ++j) {
      Vector3 new_distance = pinch_position - close_things[j].transform.position;
      if (close_things[j].rigidbody != null && close_things[j].tag != "Joint" &&  close_things[j].tag != "Tip" &&
          new_distance.magnitude < distance.magnitude) {
        grabbed_[hand] = close_things[j];
        distance = new_distance;
      }
    }
  }

  void OnRelease(int hand) {
    grabbed_[hand] = null;
    pinching_[hand] = false;
  }

	// uses LateUpdate to make sure we have the most current hand data.
	void LateUpdate () {
		GameObject [] hands = m_sHandController.GetHandGameObjects();
		for (int i = 0; i < hands.Length && i < MAX_HANDS; ++i) {
			Hand h = hands[i].GetComponent<LeapHand>().GetRawHand();
			LeapRiggedHand riggedhand = m_rHandController.GetHandGameObjectById(h.Id).GetComponent<LeapRiggedHand>();
			SkinnedMeshRenderer mesh = riggedhand.GetMesh();
      Vector3 pinch_position = hands[i].GetComponent<LeapHand>().GetFingers()[0].GetComponent<LeapFinger>().GetFingerTip().transform.position;

			if (h.PinchStrength > 0.4f && !pinching_[i])
        OnPinch(i, pinch_position);
      else if (pinching_[i])
        OnRelease(i);

      if (grabbed_[i] != null) {
        Vector3 distance = pinch_position - grabbed_[i].transform.position;
        grabbed_[i].rigidbody.velocity = 0.95f * grabbed_[i].rigidbody.velocity + 6.0f * distance;
      }
		}
	}
}
