using UnityEngine;
using System.Collections;
using Leap;

public class HandModel : MonoBehaviour {

  public int handIndex = 0;

  const float THUMB_TRIGGER_DISTANCE = 0.7f;
  const float PINCH_DISTANCE = 2.0f;

  private Controller leap_controller_;
  private bool pinching_;
  private Collider grabbed_;

	void Start () {
   	leap_controller_ = new Controller();
    pinching_ = false;
    grabbed_ = null;
	}

  void OnPinch(Vector3 pinch_position) {
    Debug.Log("PINCH");
    pinching_ = true;

    // Check if we pinched a live human.
    GameObject human = GameObject.Find("HumanGame");
    if (human != null && (human.transform.position - pinch_position).magnitude < PINCH_DISTANCE) {
      GameObject.Find("HumanGame").GetComponent<RagdollInstantiator>().Die();
      Debug.Log("DEAD");
    }

    // Check if we pinched a movable object and grab the closest one.
    Collider[] close_things = Physics.OverlapSphere(pinch_position, PINCH_DISTANCE);
    Vector3 distance = new Vector3(PINCH_DISTANCE, 0.0f, 0.0f);
    for (int j = 0; j < close_things.Length; ++j) {
      Vector3 new_distance = pinch_position - close_things[j].transform.position;
      if (close_things[j].rigidbody != null && new_distance.magnitude < distance.magnitude) {
        grabbed_ = close_things[j];
        distance = new_distance;
      }
    }
  }

  void OnRelease() {
    Debug.Log("RELEASE");
    grabbed_ = null;
    pinching_ = false;
  }

  void UpdatePinch(Frame frame) {
    bool trigger_pinch = false;
    Vector3 thumb_tip = frame.Hands[handIndex].Fingers[0].JointPosition(Finger.FingerJoint.JOINT_TIP).ToUnityScaled();

    // Check thumb tip distance to joints on all other fingers.
    // If it's close enough, start pinching.
    for (int i = 1; i < 5 && !trigger_pinch; ++i) {
      for (int j = 0; j < 4 && !trigger_pinch; ++j) {
        Vector3 difference = frame.Hands[handIndex].Fingers[i].JointPosition((Finger.FingerJoint)(j)).ToUnityScaled() -
          thumb_tip;
        if (difference.magnitude < THUMB_TRIGGER_DISTANCE)
          trigger_pinch = true;
      }
    }

    Vector3 pinch_position = transform.TransformPoint(frame.Hands[handIndex].Fingers[0].TipPosition.ToUnityScaled());

    // Only change state if it's different.
    if (trigger_pinch && !pinching_)
      OnPinch(pinch_position);
    else if (!trigger_pinch && pinching_)
      OnRelease();

    // Accelerate what we are grabbing toward the pinch.
    if (grabbed_ != null) {
      Vector3 distance = pinch_position - grabbed_.transform.position;
      grabbed_.rigidbody.velocity = 0.95f * grabbed_.rigidbody.velocity + 15.0f * distance;
    }
  }
	
	void Update () {
  	Frame frame = leap_controller_.Frame();

    if (frame.Hands.Count > handIndex) {
      // Make sure we're showing the hand.
      Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);
      for (int i = 0; i < renderers.Length; ++i)
        renderers[i].active = true;

      // Update all the fingers.
			transform.Find("thumb").GetComponent<FingerModel>().UpdateFinger(frame.Hands[handIndex].Fingers[0]);
			transform.Find("index").GetComponent<FingerModel>().UpdateFinger(frame.Hands[handIndex].Fingers[1]);
			transform.Find("middle").GetComponent<FingerModel>().UpdateFinger(frame.Hands[handIndex].Fingers[2]);
			transform.Find("ring").GetComponent<FingerModel>().UpdateFinger(frame.Hands[handIndex].Fingers[3]);
			transform.Find("pinky").GetComponent<FingerModel>().UpdateFinger(frame.Hands[handIndex].Fingers[4]);
      transform.Find("palm").transform.localPosition = frame.Hands[handIndex].PalmPosition.ToUnityScaled();
      transform.Find("palm").transform.forward = transform.root.rotation * frame.Hands[handIndex].PalmNormal.ToUnityScaled();

      // Update pinch status and object we are pinching.
      UpdatePinch(frame);
		}
    else {
      // If there is no hand, hide it and release if we were pinching.
      Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
      for (int i = 0; i < renderers.Length; ++i)
        renderers[i].active = false;

      if (pinching_)
        OnRelease();
    }
	}
}
