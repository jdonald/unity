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
public class PhysicsHand : SkeletalHand {

  public float easing = 0.5f;

  void Start() {
    palm.rigidbody.maxAngularVelocity = Mathf.Infinity;
    Collider[] colliders = gameObject.GetComponentsInChildren<Collider>();
    for (int i = 0; i < colliders.Length; ++i)
      for (int j = i + 1; j < colliders.Length; ++j)
        Physics.IgnoreCollision(colliders[i], colliders[j]);
  }

  public override void InitHand(Transform deviceTransform) {
    Rigidbody[] children = gameObject.GetComponentsInChildren<Rigidbody>();
    for (int i = 0; i < children.Length; ++i)
      children[i].detectCollisions = true;

    base.InitHand(deviceTransform);
  }

  public override void UpdateHand(Transform deviceTransform) {
    Hand leap_hand = GetLeapHand();
    // Update all the fingers and palm.
    Vector3 palm_normal = deviceTransform.rotation * leap_hand.PalmNormal.ToUnityScaled();
    Vector3 palm_direction = deviceTransform.rotation * leap_hand.Direction.ToUnityScaled();
    Vector3 palm_center = GetPalmCenter(leap_hand);

    for (int f = 0; f < NUM_FINGERS; ++f) {
      if (fingers[f] != null) {
        fingers[f].UpdateFinger(leap_hand.Fingers[f], deviceTransform, palm_normal, palm_direction);
      }
    }

    if (palm != null) {
      // Set palm velocity.
      Vector3 next_position = deviceTransform.TransformPoint(palm_center);
      palm.rigidbody.velocity = (next_position - palm.transform.position) *
                                (1 - easing) / Time.fixedDeltaTime;

      // Set palm angular velocity.
      Quaternion delta_rotation = Quaternion.LookRotation(palm_normal, palm_direction) *
                                  Quaternion.Inverse(palm.transform.rotation);
      float angle = 0.0f;
      Vector3 axis = Vector3.zero;
      delta_rotation.ToAngleAxis(out angle, out axis);

      if (angle >= 180) {
        angle = 360 - angle;
        axis = -axis;
      }
      if (angle != 0)
        palm.rigidbody.angularVelocity = (1 - easing) * angle * axis;
    }
  }

  public override void DisableHand() {
    Rigidbody[] children = gameObject.GetComponentsInChildren<Rigidbody>();
    for (int i = 0; i < children.Length; ++i)
      children[i].detectCollisions = false;
  }
}
