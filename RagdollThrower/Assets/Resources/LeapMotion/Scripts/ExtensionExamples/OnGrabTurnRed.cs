using UnityEngine;
using System.Collections;

public class OnGrabTurnRed : MonoBehaviour {
  void Update () {
    SkinnedMeshRenderer mesh = GetComponent<RiggedHand>().GetMesh();
    if (GetComponent<RiggedHand>().GetLeapHand().GrabStrength > 0.4f) {
      mesh.renderer.material.color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
    }
    else {
      mesh.renderer.material.color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
    }
  }
}
