using UnityEngine;
using System.Collections;

public class PointsAdder : MonoBehaviour {
	
	ForceCharacterController m_player;
	
	void Start() {
		m_player = GameObject.Find("Player").GetComponent<ForceCharacterController>();
	}
	
	void OnCollisionEnter(Collision colInfo) {
		if (m_player == null) return;
		
		if (colInfo.relativeVelocity.magnitude > 1.0f) {
			m_player.AddForcePoints(colInfo.relativeVelocity.magnitude);
			Debug.Log(m_player.GetForcePoints());
		}
	}
}
