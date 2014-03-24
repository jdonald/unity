using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Leap;

public class SkeletalHandController : MonoBehaviour {

	Dictionary<int, GameObject> m_hands = new Dictionary<int, GameObject>();
	
	public bool m_enabled = false;
	public Transform m_parent;
	public Vector3 m_offset;

	public GameObject [] GetHandGameObjects() {
		return m_hands.Values.ToArray();
	}

	void Update () {

		// mark exising hands as stale.
		foreach(KeyValuePair<int, GameObject> h in m_hands) {
			h.Value.GetComponent<LeapHand>().m_stale = true;
		}

		Frame f = GetComponent<FrameGrabber>().m_currentFrame;

		// see what hands the leap sees and mark matching hands as not stale.
		for(int i = 0; i < f.Hands.Count; ++i) {
			GameObject hand;

			if (m_hands.TryGetValue(f.Hands[i].Id, out hand) == false) {
				//create new hand
				hand = Instantiate(Resources.Load("Prefabs/Hand")) as GameObject;
				hand.GetComponent<LeapHand>().SetRawHand(f.Hands[i]);
				// push it into the dictionary.
				m_hands.Add(f.Hands[i].Id, hand);
			}

			// HACK to get around the id not resetting with handedness reset bug.
			if (f.Hands[i].IsRight != hand.GetComponent<LeapHand>().GetRawHand().IsRight) {
				Debug.LogWarning("handedness not matching");
				continue;
			}


			hand.transform.position = m_parent.TransformPoint(m_offset + f.Hands[i].PalmPosition.ToUnityScaled());
			hand.transform.forward = f.Hands[i].Direction.ToUnity();

			LeapHand leapHand = hand.GetComponent<LeapHand>();
			leapHand.m_stale = false;
			leapHand.UpdateFingers(f.Hands[i], m_parent, m_offset);
			leapHand.UpdateKnuckleLanges();
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
