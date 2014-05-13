using UnityEngine;
using System.Collections;
using Leap;

public class BlockScoreLogic : MonoBehaviour {

	bool m_hasBeenHit = false;
	public void ResetScore() {
		m_hasBeenHit = false;
	}

	void OnCollisionEnter(Collision c) {
		if (m_hasBeenHit) return;
		m_hasBeenHit = true;
		GameObject.Find("Score").GetComponent<ScoreDisplay>().m_score++;
	}

	void Update() {
		// clean up after ourselves...
		if (transform.position.z < -20.0f) Destroy(gameObject);
	}
}
