using UnityEngine;
using System.Collections;
using Leap;

public class ForceCharacterController : MonoBehaviour {
	
	Controller m_leapController;
	float m_lastPushTime = 0.0f;
	float m_forcePoints = 0;
	bool m_vortexPower = false;
	bool m_pullThisFrame = false;
	GameObject m_vortexPowerup;
	GameObject m_vortexSource = null;
	
	void Start() {
		m_leapController = new Controller();
		m_vortexPowerup = GameObject.Find("VortexPowerup");
	}
	
	public void AddForcePoints(float points) {
		m_forcePoints += points;
	}
	
	public float GetForcePoints() {
		return m_forcePoints;
	}
	
	void StageOne(Hand hand) {
		if (hand.PalmVelocity.ToUnityScaled().magnitude > 4.0f) return;
		Collider [] m_pullObjects = Physics.OverlapSphere(transform.position, 10.0f);
		for(int i = 0; i < m_pullObjects.Length; ++i) {
			GameObject gObject = m_pullObjects[i].transform.gameObject;
			if (gObject.rigidbody == null) continue;
			if (gObject.name == "Player") continue;
			if (hand.Fingers.Count > 1) continue;
			
			// ignore objects not in the view frustum.
			if (GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(Camera.main), gObject.collider.bounds) == false) continue;
			
			// ignore objects that are occluded from our view.
			RaycastHit hit;
			if(Physics.Linecast(transform.position, gObject.transform.position, out hit)) {
				if (hit.transform != gObject.transform) continue;	
			}
			
			
			if (gObject.rigidbody.mass < 1.0f) {
				gObject.rigidbody.velocity = Vector3.up * Random.Range(0.5f, 0.8f) * (1.0f + (m_forcePoints / 500.0f));
				gObject.rigidbody.AddTorque(new Vector3(Random.Range(-0.02f, 0.02f), Random.Range(-0.02f, 0.02f), Random.Range(-0.02f, 0.02f)));	
			}
			else if (gObject.rigidbody.mass < 5.0f) {
				gObject.rigidbody.velocity = Vector3.up * Random.Range(0.1f, 0.2f) * (1.0f + (m_forcePoints / 500.0f));
			}
			
		}
		
		if (m_pullThisFrame == false) { 
			m_vortexSource = Instantiate(Resources.Load("VortexObject")) as GameObject;
			m_vortexSource.transform.position = transform.position + transform.forward * 2.0f;
			m_vortexSource.GetComponent<VortexFade>().SetTargetIntensity(1.0f);
			m_vortexSource.GetComponent<VortexFade>().SetTargetRadius(1.5f);
		}
		
		if (hand.Fingers.Count < 2) {
			m_pullThisFrame = true;	
		} else {
			m_pullThisFrame = false;
		}
	}
	
	void StageTwo(Hand hand) {
		if (Time.time - m_lastPushTime < 1.0f) return;
		Collider [] m_pullObjects = Physics.OverlapSphere(transform.position, 10.0f);
		for(int i = 0; i < m_pullObjects.Length; ++i) {
			GameObject gObject = m_pullObjects[i].transform.gameObject;
			if (gObject.rigidbody == null) continue;
			if (gObject.name == "Player") continue;
			
			// ignore objects not in the view frustum.
			if (GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(Camera.main), gObject.collider.bounds) == false) continue;
			
			// ignore objects that are occluded from our view.
			RaycastHit hit;
			if(Physics.Linecast(transform.position, gObject.transform.position, out hit)) {
				if (hit.transform != gObject.transform) continue;	
			}
			
			if (hand.Fingers.Count < 2) {
				gObject.rigidbody.velocity = Vector3.up * Random.Range(0.5f, 0.8f);
				gObject.rigidbody.AddTorque(new Vector3(Random.Range(-0.02f, 0.02f), Random.Range(-0.02f, 0.02f), Random.Range(-0.02f, 0.02f)));
			}

			float z = hand.PalmVelocity.ToUnityScaled().z * 0.5f;
			float x = hand.PalmVelocity.ToUnityScaled().x * 0.5f;
			
			if (z > 10.0f && hand.PalmPosition.ToUnity().z > -2.0f) {
				gObject.rigidbody.velocity += transform.rotation * Vector3.forward * Mathf.Clamp(z, -15.0f, 20.0f);
				m_lastPushTime = Time.time;
			} else if (Mathf.Abs(x) > 15.0f && hand.Fingers.Count < 2) {
				// only allow horizontal throw when fist is closed.
				gObject.rigidbody.velocity += transform.rotation * Vector3.right * Mathf.Clamp(x, -10.0f, 10.0f);
				m_lastPushTime = Time.time;	
			}
		}
		
		if (m_pullThisFrame == false) { 
			m_vortexSource = Instantiate(Resources.Load("VortexObject")) as GameObject;
			m_vortexSource.transform.position = transform.position + transform.forward * 2.0f;
			m_vortexSource.GetComponent<VortexFade>().SetTargetIntensity(1.5f);
			m_vortexSource.GetComponent<VortexFade>().SetTargetRadius(3.0f);
		}
		
		if (hand.Fingers.Count < 2) {
			m_pullThisFrame = true;	
		} else {
			m_pullThisFrame = false;
		}
	}
	
	void StageThree(Hand hand) {
		if (Time.time - m_lastPushTime < 1.0f) return;
		Collider [] m_pullObjects = Physics.OverlapSphere(transform.position, 15.0f);
		for(int i = 0; i < m_pullObjects.Length; ++i) {
			GameObject gObject = m_pullObjects[i].transform.gameObject;
			if (gObject.rigidbody == null) continue;
			if (gObject.name == "Player") continue;
			
			// ignore objects not in the view frustum.
			if (GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(Camera.main), gObject.collider.bounds) == false) continue;
			
			// ignore objects that are occluded from our view.
			RaycastHit hit;
			if(Physics.Linecast(transform.position, gObject.transform.position, out hit)) {
				if (hit.transform != gObject.transform) continue;
			}
			
			if (hand.Fingers.Count < 2) {
				gObject.rigidbody.velocity = Vector3.up * Random.Range(0.9f, 1.8f);
				gObject.rigidbody.AddTorque(new Vector3(Random.Range(-0.02f, 0.02f), Random.Range(-0.02f, 0.02f), Random.Range(-0.02f, 0.02f)));
				if (m_vortexSource != null) {
					Vector3 toVortex = (m_vortexSource.transform.position - gObject.transform.position).normalized * 3.5f;
					toVortex.y = 0;
					gObject.rigidbody.velocity += toVortex;
					float temp = -toVortex.x;
					toVortex.x = toVortex.z;
					toVortex.z = temp;
					gObject.rigidbody.velocity += toVortex;	
				}
			} else if (m_vortexSource) {
				// just went from grabbing to not grabbing. "let go" gesture
				m_lastPushTime = Time.time;	
				gObject.rigidbody.velocity = transform.rotation * hand.PalmVelocity.ToUnityScaled() * 0.7f;
			}
		}
		
		if (m_pullThisFrame == false) { 
			m_vortexSource = Instantiate(Resources.Load("VortexObject")) as GameObject;
			m_vortexSource.light.color = new Color(1, 0, 0, 1);
			m_vortexSource.GetComponent<VortexFade>().SetTargetIntensity(2.5f);
			m_vortexSource.GetComponent<VortexFade>().SetTargetRadius(15.0f);
		}
		
		if (m_vortexSource != null) {
			Vector3 handPosBias = transform.rotation * hand.PalmPosition.ToUnityScaled();
			handPosBias.y = 0;
			Vector3 forwardBias = transform.forward * 5.0f;
			forwardBias.y = 0;
			m_vortexSource.transform.position = transform.position + forwardBias + handPosBias * 0.8f;
		}
		
		if (hand.Fingers.Count < 2) {
			m_pullThisFrame = true;	
		} else {
			m_pullThisFrame = false;	
		}
	}

	void ControllerUpdate() {
		transform.RotateAround(Vector3.up, Input.GetAxis("RightHorizontal") * 0.1f);
		transform.RotateAround(transform.right, Input.GetAxis("RightVertical") * 0.1f);
		Vector3 view = transform.forward;
		view.y = 0;
		rigidbody.AddForce(view.normalized * Input.GetAxis("Vertical") * 25.0f);
		rigidbody.AddForce(transform.right * Input.GetAxis("Horizontal") * 25.0f);
	}
	
	void FixedUpdate () {
		
		ControllerUpdate();
		
		Frame frame = m_leapController.Frame();
		for (int i = 0; i < frame.Hands.Count; ++i) {
			if (m_vortexPower) StageThree(frame.Hands[i]);
			else if (m_forcePoints < 200.0f) StageOne(frame.Hands[i]);
			else StageTwo(frame.Hands[i]);
		}
		
		// code dealing with the vortex powerup item
		if (!m_vortexPower) {
			if ((transform.position - m_vortexPowerup.transform.position).magnitude < 5.0f) {
				m_vortexPowerup.transform.position = Vector3.Lerp(m_vortexPowerup.transform.position, transform.position, 0.1f);	
			}
			if ((transform.position - m_vortexPowerup.transform.position).magnitude < 0.1f) {
				m_vortexPower = true;
				GameObject.Destroy(m_vortexPowerup);
				m_vortexPowerup = null;
			}
		}
		
		if (frame.Hands.Count == 0)	m_pullThisFrame = false;
		
		if (m_pullThisFrame == false) {
			if (m_vortexSource != null) {
				// call fade out on m_vortexSource
				m_vortexSource.GetComponent<VortexFade>().FadeDestroy();
				m_vortexSource = null;
			}
		}
		
	}
}
