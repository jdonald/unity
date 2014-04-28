using UnityEngine;
using System.Collections;
using Leap;

public class LeapSuperHeroController : MonoBehaviour {
	
	Controller m_leapController;
	bool m_charging = false;
	bool m_gliding = false;
	float m_chargingTime = 0.0f;
	float m_timeSinceLastRelease = 0.0f;
	float m_minimumChargeReleaseTime = 2.0f;
	
	void Start () {
		m_leapController = new Controller();
		//transform.parent.animation["jump_pose"].time = 0.5f;
		transform.parent.animation["jump_pose"].speed = 0.1f;
		transform.parent.animation["jump_pose"].wrapMode = WrapMode.ClampForever;
	}
	
	void LeapNavigation() {
		Frame frame = m_leapController.Frame();
	
		if (frame.Hands.Count == 2) {
			Hand leftHand = frame.Hands[0];
			Hand rightHand = frame.Hands[1];
			
			if (leftHand.PalmPosition.x > rightHand.PalmPosition.x) {
				leftHand = rightHand;
				rightHand = frame.Hands[0];
			}
			
			Vector3 avgPalmDir = leftHand.Direction.ToUnityScaled() + rightHand.Direction.ToUnityScaled();
			
			if (frame.Fingers.Count > 3) {
				
				if (m_gliding == false) {
					transform.parent.rigidbody.velocity += transform.parent.forward * 0.15f;
				} else {
					transform.parent.rigidbody.velocity += transform.parent.up * 0.6f;
					transform.parent.rigidbody.velocity -= Vector3.up * 0.01f;
					if (Vector3.Dot(transform.parent.forward, -Vector3.up) < 0.95f) {
						transform.parent.forward = Vector3.Slerp(transform.parent.forward, -Vector3.up, 0.1f);
					}
				}
				
				// just released a charge, see which way the hands are moving
				// to decide what to do.
				if (m_charging) {
					float speed = leftHand.PalmVelocity.ToUnityScaled().magnitude + rightHand.PalmVelocity.ToUnityScaled().magnitude;
					if (speed > 10.0f) {
						transform.parent.rigidbody.velocity += transform.parent.up * m_chargingTime * 20.0f;
					}
					m_timeSinceLastRelease = 0.0f;

					m_chargingTime = 0.0f;
					m_charging = false;
				}
			} else {
				if (m_timeSinceLastRelease > m_minimumChargeReleaseTime) {
					m_chargingTime += Time.deltaTime;
					transform.parent.rigidbody.velocity *= 0.1f;
					transform.parent.animation.Play("jump_pose");
					ParticleSystem emit = GetComponentInChildren<ParticleSystem>();
					emit.Play();
					m_charging = true;
				}
			}
			
			if (Mathf.Abs(leftHand.PalmPosition.ToUnityScaled().z - rightHand.PalmPosition.ToUnityScaled().z) > 0.3f) {
				float rotScale = leftHand.PalmPosition.ToUnityScaled().z - rightHand.PalmPosition.ToUnityScaled().z;
				rotScale *= 0.01f;
				transform.parent.RotateAround(Vector3.up, rotScale);
			}
		} else {
			m_charging = false;	
		}
		
		if (!m_charging || m_gliding) {
			ParticleSystem emit = GetComponentInChildren<ParticleSystem>();
			emit.Stop();
		}
		
		m_timeSinceLastRelease += Time.deltaTime;
	}
	
	void DetectGliding() {
		RaycastHit hit;
		m_gliding = true;
		if (Physics.Raycast(new Ray(transform.parent.position, -Vector3.up), 5.0f)) {
			// set alignment of the character.
			Quaternion target = Quaternion.Euler(new Vector3(0, transform.parent.rotation.eulerAngles.y, 0));
			transform.parent.rotation = Quaternion.Slerp(transform.parent.rotation, target, 0.1f);
			m_gliding = false;
		}
	}
	
	void FixedUpdate () {
		DetectGliding();
		LeapNavigation();
		
		if (transform.parent.rigidbody.velocity.magnitude > 5.5f && !m_gliding && !m_charging) {
			transform.parent.animation.CrossFade("walk");
			transform.parent.animation["walk"].speed = transform.parent.rigidbody.velocity.magnitude * 0.4f;
		} else if (transform.parent.rigidbody.velocity.magnitude > 0.5f && !m_gliding && !m_charging) {
			transform.parent.animation.CrossFade("walk");
			transform.parent.animation["walk"].speed = 1f;
		} else if (!m_charging) {
			transform.parent.animation.CrossFade("idle");	
		}
	}
}
