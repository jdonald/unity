using UnityEngine;
using System.Collections;

public class CameraHitLogic : MonoBehaviour {

	int m_hp = 10;
	float m_score = 0;
	int m_currentHighScore;
	bool m_gameOver = false;

	Vector3 m_startingPos;
	float m_camshakeTimer = 0.0f;

	void Start() {
		m_startingPos = transform.position;
		m_currentHighScore = PlayerPrefs.GetInt("highscore", 0);
	}

	void OnTriggerEnter(Collider other) {
		if (m_gameOver) return;
		if (other.name == "Cube") {
			Hit();
		}
	}

	void Hit() {
		m_hp--;
		Debug.Log("ouch I'm hit: " + m_hp);
		m_camshakeTimer = 0.5f;
		GetComponent<AudioSource>().Play();

		if (m_hp == 0) {
			PlayerPrefs.SetInt("highscore", m_currentHighScore);
			PlayerPrefs.Save();
			m_gameOver = true;
			GameObject.Find("GameOverText").GetComponent<GUIText>().text = "Game Over. Press Space to Restart.";
			camera.backgroundColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
		}
	}

	void Update() {
		if (m_gameOver) {
			GameObject.Find("ScoreDisplay").GetComponent<GUIText>().text = "score: " + (int) m_score;
			GameObject.Find("HighScoreDisplay").GetComponent<GUIText>().text = "high score: " + m_currentHighScore;
			GameObject.Find("HPDisplay").GetComponent<GUIText>().text = "hp: " + m_hp;
			if (Input.GetKey(KeyCode.Space)) Application.LoadLevel(Application.loadedLevel);
			return;
		}

		m_score += Time.deltaTime;
		if ((int) m_score > m_currentHighScore) {
			m_currentHighScore = (int) m_score;
		}
		GameObject.Find("ScoreDisplay").GetComponent<GUIText>().text = "score: " + (int) m_score;
		GameObject.Find("HighScoreDisplay").GetComponent<GUIText>().text = "high score: " + m_currentHighScore;
		GameObject.Find("HPDisplay").GetComponent<GUIText>().text = "hp: " + m_hp;
		if (m_camshakeTimer > 0.0f) {
			float r = 2.9f * Time.deltaTime;
			transform.position += new Vector3(Random.Range(-r, r), Random.Range(-r, r), 0.0f);
			m_camshakeTimer -= Time.deltaTime;
			camera.backgroundColor = Color.Lerp(new Color(1.0f, 0.0f, 0.0f, 1.0f), Color.black, 1.0f - m_camshakeTimer * 2.0f);
		} else {
			m_camshakeTimer = 0.0f;
			transform.position = Vector3.Lerp(transform.position, m_startingPos, Time.deltaTime * 3.0f);
		}

	}
}
