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
public class SkeletalFinger : MonoBehaviour {

  protected const int NUM_BONES = 3;
  public GameObject[] bones = new GameObject[NUM_BONES];

  // Returns the center of the given bone on the finger.
  protected Vector3 GetBonePosition(Finger finger, int bone) {
    return (finger.JointPosition((Finger.FingerJoint)(bone + 1)).ToUnityScaled() +
            finger.JointPosition((Finger.FingerJoint)(bone)).ToUnityScaled()) * 0.5f;
  }

  // Returns the direction the given bone is facing on the finger.
  protected Vector3 GetBoneDirection(Finger finger, int bone) {
    Vector3 difference = finger.JointPosition((Finger.FingerJoint)(bone + 1)).ToUnity() -
                         finger.JointPosition((Finger.FingerJoint)(bone)).ToUnity();
    difference.Normalize();
    return difference;
  }

  protected void JumpToPosition(Finger finger, Transform deviceTransform,
                                Vector3 palm_normal, Vector3 palm_direction) {
    Vector3 last_bone_normal = palm_normal;
    Vector3 last_bone_direction = palm_direction;

    for (int i = 0; i < NUM_BONES; ++i) {
      if (bones[i] != null) {
        // Set position.
        bones[i].transform.position = deviceTransform.TransformPoint(GetBonePosition(finger, i));

        // Set rotation.
        Vector3 bone_direction = deviceTransform.rotation * GetBoneDirection(finger, i);
        last_bone_normal = Quaternion.FromToRotation(last_bone_direction, bone_direction) * last_bone_normal;
        bones[i].transform.rotation = Quaternion.LookRotation(bone_direction, -last_bone_normal);
        last_bone_direction = bone_direction;
      }
    }
  }

  public void InitFinger(Finger finger, Transform deviceTransform,
                         Vector3 palm_normal, Vector3 palm_direction) {
    JumpToPosition(finger, deviceTransform, palm_normal, palm_direction);
  }

  public virtual void UpdateFinger(Finger finger, Transform deviceTransform,
                                   Vector3 palm_normal, Vector3 palm_direction) {
    JumpToPosition(finger, deviceTransform, palm_normal, palm_direction);
  }
}
