using UnityEngine;
using System.Collections;

public class LeapTurnTable : MonoBehaviour {

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
	
	public GameObject m_onButton = null;
	public GameObject m_lowPassButton = null;
	public GameObject m_highPassButton = null;
	
	public bool m_isLeftChannel = false;
	
	Vector3 m_oldTouchPosition = Vector3.zero;

	// For toggle buttons, tracks to see if the button is currently in the down state.
	bool m_isDown = false;
	
	// used for checking to see if someone was pressing on the turntable last frame so
	// we don't try to seek on the first touch.
	bool m_newPushDown = false;
	GameObject m_firstTouchObject;
	
	bool m_seeking = false;
	
	void Start () {
		m_originalPosition = transform.position;
		GetComponent<AudioSource>().velocityUpdateMode = AudioVelocityUpdateMode.Dynamic;
	}
	
	void OnTriggerEnter(Collider other) {
		if (other.tag != "FingerTip") return;
		
		m_newPushDown = true;
	}
	
	void OnTriggerStay(Collider other) {

		// only respond to fingertips
		if (other.tag != "FingerTip") return;
		
		// logic to prevent multiple fingers from screweing up seeking 
		if (!(m_newPushDown || (m_firstTouchObject == other.gameObject))) return;
		m_firstTouchObject = other.gameObject;
		
		// compute the penetration depth.
		float penDepth = Vector3.Dot(other.transform.position - transform.position, transform.up);
		penDepth = (transform.lossyScale.y + other.transform.lossyScale.z) / 2.0f - penDepth;
		
		// push downwards along the view axis.
		transform.position -= transform.up * penDepth;
		
		// compute the current depth value.
		float currentDepth = Vector3.Dot(transform.position - m_originalPosition, -transform.up);
		
		renderer.material.shader = Shader.Find("Self-Illumin/Diffuse");
		
		if (currentDepth > m_onDepth) {
			m_isDown = true;
		}

		// enforce lower bound set toggle down state.
		if (currentDepth > m_maxDepth) {
			transform.position = m_originalPosition - transform.up * m_maxDepth;
		}
		
		// check to see if the finger has move enough to enable seeking the track.
		Vector3 centerToPrev = 	(m_oldTouchPosition - transform.position).normalized;
		Vector3 centerToCurr = (other.transform.position - transform.position).normalized;
		float dist = Mathf.Abs(Vector3.Dot(other.transform.position - transform.position, other.transform.forward));
		
		if ((centerToCurr - centerToPrev).magnitude > 0 && !m_newPushDown && dist > 0.1f) {
			float angle = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot (centerToPrev, centerToCurr));
			if (Mathf.Abs(angle) > 5.0f)  {
				if (Vector3.Dot (transform.up, Vector3.Cross (centerToPrev, centerToCurr)) < 0.0f) {
					angle = -angle;
				}
				
				GetComponent<AudioSource>().pitch = angle * 0.3f;
				transform.Rotate(Vector3.up * angle);
				m_seeking = true;
			}
		}
		
		m_oldTouchPosition = other.transform.position;
		m_newPushDown = false;
	}
	
	// checks to see if the filter buttons are pressed and sets the 
	// appropriate audio filters.
	void CheckFilterButtons() {
		if (m_lowPassButton.GetComponent<LeapButton>().IsButtonOn()) {
			GetComponent<AudioLowPassFilter>().enabled = true;
		} else {
			GetComponent<AudioLowPassFilter>().enabled = false;	
		}
		
		if (m_highPassButton.GetComponent<LeapButton>().IsButtonOn()) {
			GetComponent<AudioHighPassFilter>().enabled = true;
		} else {
			GetComponent<AudioHighPassFilter>().enabled = false;	
		}	
	}
	
	// check the cross fader and adjust our audio source volume.
	void CheckCrossFader() {
		float sliderValue = GameObject.Find("CrossFader").GetComponent<LeapSlider>().SliderValue();
		if (m_isLeftChannel) {
			if (sliderValue < 0.5f) { 
				GetComponent<AudioSource>().volume = sliderValue * 2.0f;
			} else {
				GetComponent<AudioSource>().volume = 1.0f;	
			}
		} else {
			if (sliderValue > 0.5f) {
				GetComponent<AudioSource>().volume = (1.0f - sliderValue) * 2.0f;
			} else {
				GetComponent<AudioSource>().volume = 1.0f;	
			}
		}	
	}

	void FixedUpdate() {
		
		if (!m_isDown) {
			renderer.material.shader = Shader.Find("Diffuse");
		}
		
		CheckFilterButtons();
		CheckCrossFader();
		
		if (m_onButton.GetComponent<LeapButton>().IsButtonOn()) {
			// compute the current depth value.
			float currentDepth = Vector3.Dot(transform.position - m_originalPosition, -transform.up);
			// normalized depression from 0, 1
			float depression = Mathf.Max(0, Mathf.Sqrt(Mathf.Sqrt((m_maxDepth - currentDepth) / m_maxDepth)));
			
			if (m_seeking == false) GetComponent<AudioSource>().pitch = depression;
			
			transform.Rotate(Vector3.up * Time.deltaTime * 128.0f * depression);
		} else {
			GetComponent<AudioSource>().pitch = 0.0f;
		}
		
		if (m_regionCollision) {
			float penDepth = Vector3.Dot(m_regionCollisionPoint - transform.position, transform.up);
			transform.position += transform.up * penDepth;
			m_regionCollision = false;
		} else {
			transform.position += transform.up * m_springFactor;
		}
		
		// clamp to down position if button is pressed down
		if (m_isDown && Vector3.Dot(transform.position - m_originalPosition, -transform.up) < m_onDepth) {
			transform.position = m_originalPosition - transform.up * m_onDepth;
		}

		// clamp to original position (so buttons don't levetate)
		if (Vector3.Dot (transform.position - m_originalPosition, -transform.up) < 0) {
			transform.position = m_originalPosition;	
		}

		m_isDown = false;
		m_seeking = false;
	}
}
