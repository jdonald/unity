using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Leap;

public class LeapHandController : MonoBehaviour {

  [SerializeField]
  Transform m_parent;

  [SerializeField]
  Vector3 m_offset;

  Controller m_leapController;

  Dictionary<int, GameObject> m_hands = new Dictionary<int, GameObject>();

  public GameObject [] GetHandGameObjects() {
    return m_hands.Values.ToArray();
  }

  void Start () {
    m_leapController = new Controller();
  }

  void Update () {
    Frame f = m_leapController.Frame();

    // mark exising hands as stale.
    foreach(KeyValuePair<int, GameObject> h in m_hands) {
      h.Value.GetComponent<RiggedHand>().m_stale = true;
    }

        // see what hands the leap sees and mark matching hands as not stale.
    for(int i = 0; i < f.Hands.Count; ++i) {
      GameObject hand;

      if (m_hands.TryGetValue(f.Hands[i].Id, out hand) == false) {
        //create new hand
        hand = Instantiate(Resources.Load("LeapMotion/Prefabs/RiggedHand")) as GameObject;
        hand.GetComponent<RiggedHand>().Initialize(f.Hands[i], m_parent, m_offset);
        // push it into the dictionary.
        m_hands.Add(f.Hands[i].Id, hand);
      }

      // HACK to get around the id not resetting with handedness reset bug.
      if (f.Hands[i].IsRight != hand.GetComponent<RiggedHand>().GetLeapHand().IsRight) {
        Debug.LogWarning("handedness not matching");
        continue;
      }

      hand.GetComponent<RiggedHand>().UpdateHand(f.Hands[i]);

    }

    // clear out stale hands.
    List<int> staleIDs = new List<int>();
    foreach(KeyValuePair<int, GameObject> h in m_hands) {
      if (h.Value.GetComponent<RiggedHand>().m_stale) {
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
