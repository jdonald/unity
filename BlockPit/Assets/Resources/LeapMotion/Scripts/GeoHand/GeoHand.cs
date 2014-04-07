/******************************************************************************\
* Copyright (C) Leap Motion, Inc. 2011-2014.                                   *
* Leap Motion proprietary and  confidential.  Not for distribution.            *
* Use subject to the terms of the Leap Motion SDK Agreement available at       *
* https://developer.leapmotion.com/sdk_agreement, or another agreement between *
* Leap Motion and you, your company or other organization.                     *
* Author: Matt Tytel
\******************************************************************************/

using UnityEngine;
using System.Collections;
using Leap;

// The model for our geometric hand made out of various polyhedra.
public class GeoHand : MonoBehaviour {

  const float PALM_CENTER_OFFSET = 20.0f;
  const int NUM_FINGERS = 5;

  public float easing = 0.5f;
  public GameObject palm;
  public GeoFinger[] fingers = new GeoFinger[NUM_FINGERS];

  void Start() {
    palm.rigidbody.maxAngularVelocity = Mathf.Infinity;
  }

  Vector3 GetPalmPosition(Hand hand) {
    return (hand.PalmPosition.ToUnityScaled() -
            hand.Direction.ToUnityScaled() * PALM_CENTER_OFFSET);
  }

  public void InitHand(Hand hand) {
    // Make sure we're showing the hand.
    Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);
    for (int i = 0; i < renderers.Length; ++i)
      renderers[i].gameObject.SetActive(true);

    // Initialize all the fingers and palm.
    Vector3 palm_normal = transform.rotation * hand.PalmNormal.ToUnityScaled();
    Vector3 palm_direction = transform.rotation * hand.Direction.ToUnityScaled();
    Vector3 palm_position = GetPalmPosition(hand);

    for (int f = 0; f < NUM_FINGERS; ++f) {
      if (fingers[f] != null)
        fingers[f].InitFinger(hand.Fingers[f], palm_normal, palm_direction);
    }

    if (palm != null) {
      palm.transform.localPosition = palm_position;
      palm.transform.forward = transform.rotation * hand.PalmNormal.ToUnityScaled();
    }
  }

  public void UpdateHand(Hand hand) {
    // Update all the fingers and palm.
    Vector3 palm_normal = transform.rotation * hand.PalmNormal.ToUnityScaled();
    Vector3 palm_direction = transform.rotation * hand.Direction.ToUnityScaled();
    Vector3 palm_position = GetPalmPosition(hand);

    for (int f = 0; f < NUM_FINGERS; ++f) {
      if (fingers[f] != null) {
        fingers[f].SetEasing(easing);
        fingers[f].UpdateFinger(hand.Fingers[f], palm_normal, palm_direction);
      }
    }

    if (palm != null) {
      // Set palm velocity.
      Vector3 current_position = palm.transform.localPosition;
      palm.rigidbody.velocity = transform.TransformPoint(easing * (palm_position - current_position) / Time.fixedDeltaTime);

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
  }

  public void HideHand() {
    Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
    for (int i = 0; i < renderers.Length; ++i)
      renderers[i].gameObject.SetActive(false);
  }
}
