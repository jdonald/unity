using UnityEngine;
using System.Collections;

public class SlidingDoor : MonoBehaviour {
	
	public int m_buttonCount = 1;
	ButtonTarget m_buttonTarget;
	Vector3 m_originalPos;
	
	void Start() {
		m_buttonTarget = GetComponent<ButtonTarget>();
		m_originalPos = transform.position;
	}
	
	void FixedUpdate () {
		if (m_buttonTarget.GetPressCount() >= m_buttonCount) {
			transform.position = Vector3.Lerp(transform.position, m_originalPos + transform.forward * transform.localScale.z, 0.05f);
		} else 
		{
			transform.position = Vector3.Lerp(transform.position, m_originalPos, 0.05f);
		}
	}
}
