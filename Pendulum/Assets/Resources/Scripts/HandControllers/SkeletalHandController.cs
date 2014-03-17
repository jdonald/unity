using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Leap;

public class SkeletalHandController : MonoBehaviour {

	Controller m_leapController;
	Dictionary<int, GameObject> m_hands = new Dictionary<int, GameObject>();
	
	public bool m_enabled = false;
	public Transform m_parent;
	public Vector3 m_offset;

	void Start () {
		m_leapController = new Controller();
	}

	void Update () {

		Frame f = m_leapController.Frame();

		// mark exising hands as stale.
		foreach(KeyValuePair<int, GameObject> h in m_hands) {
			h.Value.GetComponent<LeapHand>().m_stale = true;
		}

		// see what hands the leap sees and mark matching hands as not stale.
		for(int i = 0; i < f.Hands.Count; ++i) {
			GameObject hand;
			m_hands.TryGetValue(f.Hands[i].Id, out hand);
			
			// see if hand existed before
			if (hand != null) {
				// if it did then just update its position and joint positions.

				hand.transform.position = m_parent.TransformPoint(m_offset + f.Hands[i].PalmPosition.ToUnityScaled());
				hand.transform.forward = f.Hands[i].Direction.ToUnity();

				LeapHand leapHand = hand.GetComponent<LeapHand>();
				leapHand.m_stale = false;
				leapHand.UpdateFingers(f.Hands[i], m_parent, m_offset);
				leapHand.UpdateKnuckleLanges();

			} else {
				// else create new hand
				hand = Instantiate(Resources.Load("Prefabs/Hand")) as GameObject;
				// push it into the dictionary.
				m_hands.Add(f.Hands[i].Id, hand);
			}

		}

		// clear out stale hands.
		List<int> staleIDs = new List<int>();
		foreach(KeyValuePair<int, GameObject> h in m_hands) {
			if (h.Value.GetComponent<LeapHand>().m_stale) {
				Destroy(h.Value);
				// set for removal from dictionary.
				staleIDs.Add(h.Key);
			}
		}
		foreach(int id in staleIDs) {
			m_hands.Remove(id);
		}
	}
}
