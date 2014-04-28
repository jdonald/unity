using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class LeapButton : MonoBehaviour {
	
	Vector3 m_originalPosition; // default position of the button when not pressed.
	
	// the furthest "down" you can push the button.
	public float m_maxDepth = 0.15f;
	
	// how far down the button has to be to be considered pressed.
	public float m_onDepth = 0.1f;
	
	// multiplier for how fast the button springs back up.
	public float m_springFactor = 0.05f;
	
	// tells us how far to go back up if there's a finger in the region
	public Vector3 m_regionCollisionPoint = Vector3.zero;
	
	// whether or not a finger/palm is colliding in the region.
	public bool m_regionCollision = false;
	
	// toggle button or held down button?
	public bool m_toggleButton = true;

	// For toggle buttons, tracks to see if the button is currently in the down state.
	bool m_isDown = false;
	
	// flag to see if I can change the button state again, is reset
	// after the button passes the onDepth threshold.
	bool m_toggleAble = true;
	
	int m_collisionCount = 0;
	
	void Start () {
		m_originalPosition = transform.position;
	}
	
	public bool IsButtonOn() {
		return m_isDown;
	}
	
	void OnTriggerStay(Collider other) {

		// only respond to fingertips
		if (other.tag != "FingerTip") return;
		
		// compute the penetration depth.
		float penDepth = Vector3.Dot(other.transform.position - transform.position, transform.forward);
		penDepth = (transform.lossyScale.z + other.transform.lossyScale.z) / 2.0f - penDepth;
		
		// push downwards along the view axis.
		transform.position -= transform.forward * penDepth;
		
		// compute the current depth value.
		float currentDepth = Vector3.Dot(transform.position - m_originalPosition, -transform.forward);
		
		if (currentDepth > m_onDepth) {
			// make the button "glow" when it's pressed down.
			renderer.material.shader = Shader.Find("Self-Illumin/Diffuse");
		}

		// enforce lower bound set toggle down state.
		if (currentDepth > m_maxDepth) {
			transform.position = m_originalPosition - transform.forward * m_maxDepth;
			if (m_toggleAble && m_toggleButton) {
				m_isDown = !m_isDown;
				m_toggleAble = false;
			}
		}

		if (m_isDown) {
			renderer.material.shader = Shader.Find("Self-Illumin/Diffuse");
		}
		
		m_collisionCount++;
		
	}

	void FixedUpdate() {
		
		// by default set the shader to not be self illuminated.
		if (!m_isDown) {
			renderer.material.shader = Shader.Find("Diffuse");
		}
		
		if (m_regionCollision) {
			float penDepth = Vector3.Dot(m_regionCollisionPoint - transform.position, transform.forward);
			transform.position += transform.forward * penDepth;
			m_regionCollision = false;
		} else {
			transform.position += transform.forward * m_springFactor;
		}
		
		// clamp to down position if button is pressed down
		if (m_isDown && Vector3.Dot(transform.position - m_originalPosition, -transform.forward) < m_onDepth) {
			transform.position = m_originalPosition - transform.forward * m_onDepth;
			// just traveled up and had to be clamped, which means it should be toggleable again.
		}

		// clamp to original position (so buttons don't levetate)
		if (Vector3.Dot (transform.position - m_originalPosition, -transform.forward) < 0) {
			transform.position = m_originalPosition;	
		}
		
		if (m_collisionCount == 0) m_toggleAble = true;
		m_collisionCount = 0;
	}
}
