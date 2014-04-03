using UnityEngine;
using System.Collections;
using Leap;

public class RiggedHand : MonoBehaviour {

  public bool m_stale = false;

  Hand m_rawHand;
  GameObject [] m_fingers = new GameObject[5];
  GameObject m_meshHand;
  GameObject m_rigBones;

  Quaternion offsetRightArm = Quaternion.Euler(new Vector3(90, 90, 0));
  Quaternion offsetRightHand = Quaternion.Euler(new Vector3(90, 0, 0));

  Quaternion offsetLeftArm = Quaternion.Euler(new Vector3(-90, 90, 0));
  Quaternion offsetLeftHand = Quaternion.Euler(new Vector3(-90, 0, 0));

  Vector3 m_offset;
  Transform m_parent;

  public float m_distancePalmToWrist = 0.8f;
  public float m_handScaleFactor = 1.0f;
  float m_leapExtensionScale = 1000.0f;

  public void Initialize(Hand h, Transform parent, Vector3 offset) {
    m_rawHand = h;
    if (h.IsRight) {
      m_meshHand = Instantiate(Resources.Load("LeapMotion/Prefabs/RightHandMesh")) as GameObject;
    }
    else {
      m_meshHand = Instantiate(Resources.Load("LeapMotion/Prefabs/LeftHandMesh")) as GameObject;
    }
    m_rigBones = m_meshHand.transform.Find("Bip01").Find("Bones").gameObject;
    m_parent = parent;
    m_offset = offset;
  }

  public void UpdateHand(Hand h) {
    m_rawHand = h;
    UpdateMesh();
    UpdatePhysics();
  }

  public Hand GetLeapHand() {
    return m_rawHand;
  }

  public GameObject [] GetFingers() {
    return m_fingers;
  }

  public void EnableMesh(bool enable) {
    m_rigBones.GetComponent<SkinnedMeshRenderer>().enabled = enable;
  }

  public void EnablePhysics(bool enable) {

  }

  public SkinnedMeshRenderer GetMesh() {
    return m_rigBones.GetComponent<SkinnedMeshRenderer>();
  }

  void UpdateMesh() {
    if (m_meshHand == null) Debug.LogError("Rigged Hand is null, did you call InitializeHand?");
    // get the bones from the rig
    Transform [] boneTransforms = m_rigBones.GetComponent<SkinnedMeshRenderer>().bones;

    if (m_rawHand.IsRight) {
      UpdateHandRig(boneTransforms, "R", offsetRightArm, offsetRightHand);
    }
    else {
      UpdateHandRig(boneTransforms, "L", offsetLeftArm, offsetLeftHand);
    }
    m_stale = false;
    float scale = WristToMiddleKnuckle() / 55.0f;
    scale *= m_leapExtensionScale * UnityVectorExtension.InputScale.x;
    m_meshHand.transform.localScale = new Vector3(scale, scale, scale);
  }

  Transform FindBone(Transform [] array, string boneName) {
    for (int i = 0; i < array.Length; ++i) {
      if (array[i].name == boneName) return array[i];
    }
    Debug.LogError("Bone Not Found: " + boneName);
    return null;
  }

  void UpdateHandRig(Transform [] bones, string hand, Quaternion offsetArm, Quaternion offsetHand) {

    Quaternion handRot = Quaternion.identity;
    handRot = Quaternion.LookRotation(m_rawHand.Direction.ToUnity(), -m_rawHand.PalmNormal.ToUnity()) * offsetArm * offsetHand;

    Transform handTransform = FindBone(bones, "Bip01 " + hand + " Hand");
    handTransform.rotation = m_parent.rotation * handRot;
    handTransform.position = m_parent.TransformPoint(m_offset + m_rawHand.PalmPosition.ToUnityScaled() - m_rawHand.Direction.ToUnity() * m_distancePalmToWrist);

    for (int i = 0; i < m_rawHand.Fingers.Count; ++i) {
      Finger finger = m_rawHand.Fingers[i];

      // get all the joint positions in unity space.
      Vector3 mcpPos = finger.JointPosition(Finger.FingerJoint.JOINT_MCP).ToUnityScaled();
      Vector3 pipPos = finger.JointPosition(Finger.FingerJoint.JOINT_PIP).ToUnityScaled();
      Vector3 dipPos = finger.JointPosition(Finger.FingerJoint.JOINT_DIP).ToUnityScaled();
      Vector3 tipPos = finger.JointPosition(Finger.FingerJoint.JOINT_TIP).ToUnityScaled();

      // compute finger joint rotations
      Transform mcp = FindBone(bones, "Bip01 " + hand + " Finger" + i);
      Quaternion mcpRot = Quaternion.FromToRotation(m_rawHand.Direction.ToUnity(), (pipPos - mcpPos).normalized) * handRot;
      mcp.rotation = m_parent.rotation * mcpRot;

      Transform pip = FindBone(bones, "Bip01 " + hand + " Finger" + i + "1");
      Quaternion pipRot = Quaternion.FromToRotation((pipPos - mcpPos).normalized, (dipPos - pipPos).normalized) * mcpRot;
      pip.rotation = m_parent.rotation * pipRot;

      Transform dip = FindBone(bones, "Bip01 " + hand + " Finger" + i + "2");
      dip.rotation = m_parent.rotation * Quaternion.FromToRotation((dipPos - pipPos).normalized, (tipPos - dipPos).normalized) * pipRot;

    }
  }

  float WristToMiddleKnuckle() {
    return m_rawHand.Fingers[2].Length;
  }

  void UpdatePhysics() {
    for (int i = 0; i < m_fingers.Length; ++i) {
      RiggedFinger finger = m_fingers[i].GetComponent<RiggedFinger>();
      GameObject [] joints = finger.GetJoints();
      GameObject [] bones = finger.GetBones();
      for(int j = 0; j < joints.Length; ++j) {
        Vector3 pos = m_parent.TransformPoint(m_offset + m_rawHand.Fingers[i].JointPosition((Finger.FingerJoint) j).ToUnityScaled());
        Vector3 nextPos = m_parent.TransformPoint(m_offset + m_rawHand.Fingers[i].JointPosition((Finger.FingerJoint) j + 1).ToUnityScaled());
        joints[j].transform.position = pos;
        joints[j].rigidbody.velocity = m_parent.TransformDirection(m_rawHand.Fingers[i].TipVelocity.ToUnityScaled());

        if (j < bones.Length) {
          Vector3 langePos = (pos + nextPos) * 0.5f;
          bones[j].transform.position = langePos;
          bones[j].transform.up = (nextPos - pos);
          Vector3 newScale = bones[j].transform.localScale;
          newScale.y = Mathf.Max(0.0f, (nextPos - pos).magnitude - 0.003f);
          bones[j].transform.localScale = newScale;
        }
      }
    }
  }

  // Use this for initialization
  void Awake() {
    for (int i = 0; i < m_fingers.Length; ++i) {
      m_fingers[i] = Instantiate(Resources.Load("LeapMotion/Prefabs/RiggedFinger")) as GameObject;
    }
  }

  void OnDestroy() {
    Destroy(m_meshHand);
    for (int i = 0; i < m_fingers.Length; ++i) {
      Destroy(m_fingers[i]);
    }
  }
}
