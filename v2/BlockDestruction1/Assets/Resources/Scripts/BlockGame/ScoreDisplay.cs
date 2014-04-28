using UnityEngine;
using System.Collections;

public class ScoreDisplay : MonoBehaviour {

	public int m_score = 0;
	
	// Update is called once per frame
	void Update () {
		GetComponent<GUIText>().text = "score: " + m_score;
	}
}
