using UnityEngine;
using System.Collections;
using Leap;

public class HandModel : MonoBehaviour {

  public int handIndex = 0;
  private Controller leap_controller_;

	void Start () {
   	leap_controller_ = new Controller();
	}

	void Update () {
  	Frame frame = leap_controller_.Frame();

    if (frame.Hands.Count > handIndex) {
      // Make sure we're showing the hand.
      Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);
      for (int i = 0; i < renderers.Length; ++i)
        renderers[i].gameObject.SetActive(true);

      // Update all the fingers.
      Hand hand = frame.Hands[handIndex];

			transform.Find("thumb").GetComponent<FingerModel>().UpdateFinger(hand.Fingers[0], hand.PalmNormal.ToUnityScaled(), hand.Direction.ToUnityScaled());
			transform.Find("index").GetComponent<FingerModel>().UpdateFinger(hand.Fingers[1], hand.PalmNormal.ToUnityScaled(), hand.Direction.ToUnityScaled());
			transform.Find("middle").GetComponent<FingerModel>().UpdateFinger(hand.Fingers[2], hand.PalmNormal.ToUnityScaled(), hand.Direction.ToUnityScaled());
			transform.Find("ring").GetComponent<FingerModel>().UpdateFinger(hand.Fingers[3], hand.PalmNormal.ToUnityScaled(), hand.Direction.ToUnityScaled());
			transform.Find("pinky").GetComponent<FingerModel>().UpdateFinger(hand.Fingers[4], hand.PalmNormal.ToUnityScaled(), hand.Direction.ToUnityScaled());

      Vector3 ring_base = hand.Fingers[3].JointPosition(Finger.FingerJoint.JOINT_MCP).ToUnityScaled();
      Vector3 thumb_base = hand.Fingers[0].JointPosition(Finger.FingerJoint.JOINT_MCP).ToUnityScaled();
      transform.Find("palm").transform.localPosition = (ring_base + thumb_base) / 2.0f;
      transform.Find("palm").transform.forward = transform.rotation * hand.PalmNormal.ToUnityScaled();
		}
    else {
      // If there is no hand, hide it.
      Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
      for (int i = 0; i < renderers.Length; ++i)
        renderers[i].gameObject.SetActive(false);
    }
	}
}
