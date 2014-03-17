using UnityEngine;	
using System.Collections;
using System.Collections.Generic;
using Leap;

using System;
using System.IO;

public class RiggedHandController : MonoBehaviour {

	Dictionary<int, GameObject> m_hands = new Dictionary<int, GameObject>();
	Controller m_leapController;
	bool m_paused = false;
	public Transform m_parent;
	public Vector3 m_offset;

	// Use this for initialization
	void Start () {
		m_leapController = new Controller();
	}

	Hand FindFrontLeftHand(Frame f) {
		Hand h = null;
		float compVal = -float.MaxValue;
		for (int i = 0; i < f.Hands.Count; ++i) {
			if (f.Hands[i].IsLeft && f.Hands[i].PalmPosition.ToUnityScaled().z > compVal) {
				compVal = f.Hands[i].PalmPosition.ToUnityScaled().z;
				h = f.Hands[i];
			}
		}
		return h;
	}
	
	Hand FindFrontRightHand(Frame f) {
		Hand h = null;
		float compVal = -float.MaxValue;
		for (int i = 0; i < f.Hands.Count; ++i) {
			if (f.Hands[i].IsRight && f.Hands[i].PalmPosition.ToUnityScaled().z > compVal) {
				compVal = f.Hands[i].PalmPosition.ToUnityScaled().z;
				h = f.Hands[i];
			}
		}
		return h;
	}

	public void ShowHands(bool shouldShow) {
		foreach(KeyValuePair<int, GameObject> h in m_hands) {
			h.Value.GetComponent<LeapRiggedHand>().Draw(shouldShow);
		}

	}

	public void SetPause(bool pause) {
		m_paused = pause;
	}
	
	// Update is called once per frame
	void Update () {
		if (m_leapController == null) return;

		if (m_paused) return;
		
		// mark exising hands as stale.
		foreach(KeyValuePair<int, GameObject> h in m_hands) {
			h.Value.GetComponent<LeapRiggedHand>().m_stale = true;
		}

		Frame f = m_leapController.Frame();

		// see what hands the leap sees and mark matching hands as not stale.
		for(int i = 0; i < f.Hands.Count; ++i) {
			
			GameObject hand;
			
			// see if hand existed before
			if (m_hands.TryGetValue(f.Hands[i].Id, out hand)) {
				
				// HACK to get around the id not resetting with handedness reset bug.
				if (f.Hands[i].IsRight != hand.GetComponent<LeapRiggedHand>().IsRight()) {
					Debug.LogWarning("handedness not matching");
					continue;
				}
				// if it did then just update its position and joint positions.
				hand.GetComponent<LeapRiggedHand>().UpdateRig(f.Hands[i]);
				if (f.Hands[i].GrabStrength > 0.4f) {
					hand.GetComponent<LeapRiggedHand>().m_riggedHand.transform.Find("Bip01").Find("Bones").GetComponent<SkinnedMeshRenderer>().material.color = new Color(0.15f, 0.15f, 0.15f, 0.1f);
				} else {
					hand.GetComponent<LeapRiggedHand>().m_riggedHand.transform.Find("Bip01").Find("Bones").GetComponent<SkinnedMeshRenderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 0.8f);
				}

			} else {
				// else create new hand
				hand = Instantiate(Resources.Load("Prefabs/LeapRiggedHand")) as GameObject;
				hand.GetComponent<LeapRiggedHand>().InitializeHand(f.Hands[i].IsRight, m_parent, m_offset);
				// push it into the dictionary.
				m_hands.Add(f.Hands[i].Id, hand);
			}
			
		}

		// clear out stale hands.
		List<int> staleIDs = new List<int>();
		foreach(KeyValuePair<int, GameObject> h in m_hands) {
			if (h.Value.GetComponent<LeapRiggedHand>().m_stale) {
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
