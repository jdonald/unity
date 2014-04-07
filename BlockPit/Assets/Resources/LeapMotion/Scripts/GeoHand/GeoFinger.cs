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

// The finger model for our geometric hand made out of various polyhedra.
public class GeoFinger : MonoBehaviour {

  const int NUM_BONES = 3;
  public GameObject[] bones = new GameObject[NUM_BONES];

  private float easing_ = 0.5f;

  void Start() {
    for (int i = 0; i < NUM_BONES; ++i)
      bones[i].rigidbody.maxAngularVelocity = Mathf.Infinity;
  }

  public void SetEasing(float easing) {
    easing_ = easing;
  }

  // Returns the center of the given bone on the finger.
  private Vector3 GetBonePosition(Finger finger, int bone) {
    return (finger.JointPosition((Finger.FingerJoint)(bone + 1)).ToUnityScaled() +
            finger.JointPosition((Finger.FingerJoint)(bone)).ToUnityScaled()) * 0.5f;
  }

  // Returns the direction the given bone is facing on the finger.
  private Vector3 GetBoneDirection(Finger finger, int bone) {
    Vector3 direction = Vector3.Scale(transform.localScale, transform.rotation * (finger.JointPosition((Finger.FingerJoint)(bone + 1)).ToUnity() -
                                                                                  finger.JointPosition((Finger.FingerJoint)(bone)).ToUnity()));
    direction.Normalize();
    return direction;
  }

  public void InitFinger (Finger finger, Vector3 palm_normal, Vector3 palm_direction) {
    Vector3 last_bone_normal = palm_normal;
    Vector3 last_bone_direction = palm_direction;

    for (int i = 0; i < NUM_BONES; ++i) {
      // Set position.
      bones[i].transform.localPosition = GetBonePosition(finger, i);

      // Set rotation.
      Vector3 bone_direction = GetBoneDirection(finger, i);
      last_bone_normal = Quaternion.FromToRotation(last_bone_direction, bone_direction) * last_bone_normal;
      bones[i].transform.rotation = Quaternion.LookRotation(bone_direction, last_bone_normal);
      last_bone_direction = bone_direction;
    }
  }

  public void UpdateFinger (Finger finger, Vector3 palm_normal, Vector3 palm_direction) {
    Vector3 last_bone_normal = palm_normal;
    Vector3 last_bone_direction = palm_direction;

    for (int i = 0; i < NUM_BONES; ++i) {
      // Set velocity.
      Vector3 current_bone_position = bones[i].transform.localPosition;
      bones[i].rigidbody.velocity = transform.TransformPoint(easing_ * (GetBonePosition(finger, i) - current_bone_position) / Time.fixedDeltaTime);

      // Set angular velocity.
      Vector3 bone_direction = GetBoneDirection(finger, i);
      Vector3 bone_normal = Quaternion.FromToRotation(last_bone_direction, bone_direction) * last_bone_normal;

      Quaternion rotate = Quaternion.LookRotation(bone_direction, bone_normal) * Quaternion.Inverse(bones[i].transform.rotation);
      float angle = 0.0f;
      Vector3 axis = Vector3.zero;
      rotate.ToAngleAxis(out angle, out axis);

      if (angle >= 180) {
        angle = 360 - angle;
        axis  = -axis;
      }
      if (angle != 0)
        bones[i].rigidbody.angularVelocity = easing_ * angle * axis;

      last_bone_direction = bone_direction;
      last_bone_normal = bone_normal;
    }
  }
}
