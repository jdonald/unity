using UnityEngine;
using System.Collections;

public class TrooperButton : MonoBehaviour {
	
	public ButtonTarget m_buttonTarget = null;
	
	void OnTriggerEnter(Collider other) {
		if (other.name != "Trooper") return;
		renderer.material.shader = Shader.Find("Self-Illumin/Diffuse");
		if (m_buttonTarget) {
			m_buttonTarget.SetPressed(true);
		}
	}
	
	void OnTriggerExit(Collider other) {
		if (other.name != "Trooper") return;
		
		renderer.material.shader = Shader.Find("Diffuse");
		if (m_buttonTarget) {
			m_buttonTarget.SetPressed(false);
		}	
	}
	
	void Start () {
		renderer.material.color = new Color((217f / 255f), (79f/255f), (48f/255f));
	}
	
	void FixedUpdate() {

	}
}
