using UnityEngine;
using System.Collections;
using Leap;

public class BlockReturnLogic : MonoBehaviour {
	Vector3 m_originalPos;
	Quaternion m_originalRot;

	Controller m_leapController;

	void Start() {
		m_originalPos = transform.position;
		m_originalRot = transform.rotation;
		m_leapController = new Controller();
	}

	void Update() {
		Frame f = m_leapController.Frame();
		bool pinch = false;
		for (int i = 0; i < f.Hands.Count; ++i) {
			if (f.Hands[i].PinchStrength > 0.6f) {
				pinch = true;
				break;
			}
		}
		if (pinch) {
			float returnSpeed = 5.0f;
			rigidbody.velocity = Vector3.Lerp(rigidbody.velocity, Vector3.zero, Time.deltaTime * returnSpeed);
			rigidbody.angularVelocity = Vector3.Lerp(rigidbody.angularVelocity, Vector3.zero, Time.deltaTime * returnSpeed);

			transform.position = Vector3.Lerp(transform.position, m_originalPos, Time.deltaTime * returnSpeed);
			transform.rotation = Quaternion.Slerp(transform.rotation, m_originalRot, Time.deltaTime * returnSpeed);
			
			BlockScoreLogic bsLogic = GetComponent<BlockScoreLogic>();
			if (bsLogic != null) {
				bsLogic.ResetScore();
				GameObject.Find("Score").GetComponent<ScoreDisplay>().m_score = 0;
			}
		}
	}
}
