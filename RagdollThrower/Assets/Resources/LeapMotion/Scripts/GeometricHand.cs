using UnityEngine;
using System.Collections;
using Leap;

public class GeometricHand : MonoBehaviour {

  const float PALM_CENTER_OFFSET = 20.0f;
  const int NUM_FINGERS = 5;

  public int handIndex = 0;
  public float easing = 0.5f;
  public GameObject palm;
  public GeometricFinger[] fingers = new GeometricFinger[NUM_FINGERS];

  private Controller leap_controller_;
  private int hand_id_;

	void Start() {
   	leap_controller_ = new Controller();
    hand_id_ = -1;
    palm.rigidbody.maxAngularVelocity = Mathf.Infinity;
	}

  Vector3 GetPalmPosition(Hand hand) {
    return hand.PalmPosition.ToUnityScaled() - hand.Direction.ToUnityScaled() * PALM_CENTER_OFFSET;
  }

  void InitHand(Hand hand) {
    // Make sure we're showing the hand.
    Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);
    for (int i = 0; i < renderers.Length; ++i)
      renderers[i].gameObject.SetActive(true);

    // Initialize all the fingers and palm.
    Vector3 palm_normal = hand.PalmNormal.ToUnityScaled();
    Vector3 palm_direction = hand.Direction.ToUnityScaled();
    Vector3 palm_position = GetPalmPosition(hand);

    for (int f = 0; f < NUM_FINGERS; ++f)
      fingers[f].InitFinger(hand.Fingers[f], palm_normal, palm_direction);

    palm.transform.localPosition = palm_position;
    transform.Find("palm").transform.forward = transform.rotation * hand.PalmNormal.ToUnityScaled();
  }

  void UpdateHand(Hand hand) {
    // Update all the fingers and palm.
    Vector3 palm_normal = hand.PalmNormal.ToUnityScaled();
    Vector3 palm_direction = hand.Direction.ToUnityScaled();
    Vector3 palm_position = GetPalmPosition(hand);

    for (int f = 0; f < NUM_FINGERS; ++f) {
      fingers[f].SetEasing(easing);
      fingers[f].UpdateFinger(hand.Fingers[f], palm_normal, palm_direction);
    }

    // Set palm velocity.
    palm.rigidbody.velocity = easing * (palm_position - transform.Find("palm").transform.localPosition) / Time.fixedDeltaTime;

    // Set palm angular velocity.
    Quaternion rotate = Quaternion.LookRotation(palm_normal, palm_direction) * Quaternion.Inverse(palm.transform.rotation);
    float angle = 0.0f;
    Vector3 axis = Vector3.zero;
    rotate.ToAngleAxis(out angle, out axis);

    if (angle >= 180) {
      angle = 360 - angle;
      axis = -axis;
    }
    if (angle != 0)
      palm.rigidbody.angularVelocity = easing * angle * axis;
  }

  void HideHand() {
    Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
    for (int i = 0; i < renderers.Length; ++i)
      renderers[i].gameObject.SetActive(false);
  }

	void FixedUpdate () {
  	Frame frame = leap_controller_.Frame();

    if (frame.Hands.Count > handIndex) {
      Hand hand = frame.Hands[handIndex];
      if (hand.Id != hand_id_) {
        hand_id_ = hand.Id;
        InitHand(hand);
      }

      UpdateHand(hand);
    }
    else
      HideHand();
	}
}
