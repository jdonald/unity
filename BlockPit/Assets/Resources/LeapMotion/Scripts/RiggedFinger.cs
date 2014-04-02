using UnityEngine;
using System.Collections;

public class RiggedFinger : MonoBehaviour {

  GameObject [] m_joints = new GameObject[4];
  GameObject [] m_bones = new GameObject[3]; 

  public GameObject [] GetJoints() {
    return m_joints;
  }

  public GameObject [] GetBones() {
    return m_bones;
  }

  public void EnablePhysics(bool enable) {
    for (int i = 0; i < m_joints.Length; ++i) {
      m_joints[i].collider.enabled = enable;
    }
    for (int i = 0; i < m_bones.Length; ++i) {
      m_bones[i].collider.enabled = enable;
    }
  }

  void Awake() {
    for (int i = 0; i < m_joints.Length; ++i) {
      m_joints[i] = Instantiate(Resources.Load("LeapMotion/Prefabs/RiggedJoint")) as GameObject;
    }
    for (int i = 0; i < m_bones.Length; ++i) {
      m_bones[i] = Instantiate(Resources.Load("LeapMotion/Prefabs/RiggedBone")) as GameObject;
    }
  }

  void OnDestroy() {
    for (int i = 0; i < m_joints.Length; ++i) {
      Destroy(m_joints[i]);
    }
    for (int i = 0; i < m_bones.Length; ++i) {
      Destroy(m_bones[i]);
    }
  }
}
