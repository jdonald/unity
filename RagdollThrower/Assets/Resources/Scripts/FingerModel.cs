using UnityEngine;
using System.Collections;
using Leap;

public class FingerModel : MonoBehaviour {

	public void UpdateFinger (Finger finger, Vector3 palm_normal, Vector3 palm_direction) {
    Vector3[] digit_positions = new Vector3[3];
    Vector3[] digit_directions = new Vector3[3];

    for (int i = 0; i < 3; ++i) {
      digit_positions[i] = (finger.JointPosition((Finger.FingerJoint)(i)).ToUnityScaled() +
                            finger.JointPosition((Finger.FingerJoint)(i + 1)).ToUnityScaled()) * 0.5f;
      digit_directions[i] = finger.JointPosition((Finger.FingerJoint)(i + 1)).ToUnityScaled() -
                            finger.JointPosition((Finger.FingerJoint)(i)).ToUnityScaled();
    }

    transform.Find("digit0").transform.localPosition = digit_positions[0];
    Vector3 digit0_normal = Quaternion.FromToRotation(palm_direction, digit_directions[0]) * palm_normal;
    transform.Find("digit0").transform.rotation = Quaternion.LookRotation(digit_directions[0], digit0_normal);
    transform.Find("digit1").transform.localPosition = digit_positions[1];
    Vector3 digit1_normal = Quaternion.FromToRotation(digit_directions[0], digit_directions[1]) * digit0_normal;
    transform.Find("digit1").transform.rotation = Quaternion.LookRotation(digit_directions[1], digit1_normal);
    transform.Find("digit2").transform.localPosition = digit_positions[2];
    Vector3 digit2_normal = Quaternion.FromToRotation(digit_directions[1], digit_directions[2]) * digit1_normal;
    transform.Find("digit2").transform.rotation = Quaternion.LookRotation(digit_directions[2], digit2_normal);
	}
}
