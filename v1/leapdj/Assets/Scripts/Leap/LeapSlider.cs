using UnityEngine;
using System.Collections;

public class LeapSlider : MonoBehaviour {
	
	Vector3 m_originalPosition;
	public float m_range = 1.8f;
	public Color m_startColor = Color.black;
	public Color m_endColor = Color.red;
	public bool m_interpolateColors = false;
	bool m_toggleAble = true;
	int m_collisionCount = 0;
	
	void Start () {
		m_originalPosition = transform.position;
	}
	
	void OnTriggerStay(Collider other) {
		if (other.tag != "FingerTip") return;
		
		++m_collisionCount;
		
		if (!m_toggleAble) return;
		
		// figure out which way the slider is being pushed from.
		// This code can be made more robust with additional checks.
		if (Vector3.Dot(other.transform.position - transform.position, transform.forward) < 0.0f) {
			float penDepth = Vector3.Dot(other.transform.position - transform.position, -transform.forward);
			penDepth = (transform.lossyScale.z + other.transform.lossyScale.z) / 2.0f - penDepth;
			transform.position += transform.forward * penDepth;
		} else {
			float penDepth = Vector3.Dot(other.transform.position - transform.position, transform.forward);
			penDepth = (transform.lossyScale.z + other.transform.lossyScale.z) / 2.0f - penDepth;
			transform.position -= transform.forward * penDepth;
		}
		
	}
	
	// returns slider value between 0, 1
	public float SliderValue() {
		float distAlongForward = Vector3.Dot(transform.position - m_originalPosition, transform.forward);
		distAlongForward /= m_range;
		return distAlongForward;
	}
	
	void FixedUpdate() {
		// prevent it from going out of range.
		float distAlongForward = Vector3.Dot(transform.position - m_originalPosition, transform.forward);
		if (distAlongForward < 0.0f) {
			transform.position = m_originalPosition;
			m_toggleAble = false;
		}
		
		if (distAlongForward > m_range) {
			transform.position = m_originalPosition + transform.forward * m_range;
			m_toggleAble = false;
		}
		
		if (m_collisionCount == 0) m_toggleAble = true;
		m_collisionCount = 0;
		
		float sliderVal = SliderValue();

		// interpolate between start and end colors if the flag is enabled.
		if (m_interpolateColors) renderer.material.color = Color.Lerp(m_startColor, m_endColor, sliderVal);
	}
}
