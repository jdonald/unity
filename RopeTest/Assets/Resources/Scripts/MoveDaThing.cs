using UnityEngine;
using System.Collections;
using Leap;

public class MoveDaThing : MonoBehaviour {

	Controller m_leapController;

	void Start() {
		m_leapController = new Controller();
	}

	// Update is called once per frame
	void Update () {
		Frame f = m_leapController.Frame();
		if (f.Hands.Count > 0) {
			Debug.Log(f.Hands[0].Direction.Roll);
		}
	}
}
