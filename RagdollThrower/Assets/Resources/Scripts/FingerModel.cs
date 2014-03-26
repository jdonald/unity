using UnityEngine;
using System.Collections;
using Leap;

public class FingerModel : MonoBehaviour {

	public void UpdateFinger (Finger finger) {
    Vector3[] digit_positions = new Vector3[3];
    Vector3[] digit_directions = new Vector3[3];
    for (int i = 0; i < 3; ++i) {
      digit_positions[i] = (finger.JointPosition((Finger.FingerJoint)(i)).ToUnityScaled() +
                            finger.JointPosition((Finger.FingerJoint)(i + 1)).ToUnityScaled()) * 0.5f;
      digit_directions[i] = finger.JointPosition((Finger.FingerJoint)(i + 1)).ToUnityScaled() -
                            finger.JointPosition((Finger.FingerJoint)(i)).ToUnityScaled();
    }

    transform.Find("digit0").transform.localPosition = digit_positions[0];
    transform.Find("digit0").transform.forward = -1.0f * (transform.root.rotation * digit_directions[0]);
    transform.Find("digit1").transform.localPosition = digit_positions[1];
    transform.Find("digit1").transform.forward = -1.0f * (transform.root.rotation * digit_directions[1]);
    transform.Find("digit2").transform.localPosition = digit_positions[2];
    transform.Find("digit2").transform.forward = -1.0f * (transform.root.rotation * digit_directions[2]);
	}
}
