using UnityEngine;
using System.Collections;
using Leap;

public class ForceCharacterControllerLeap : MonoBehaviour {
	
	Controller m_leapController;
	float m_forcePoints = 0;
	float m_lastPushTime = 0.0f;
	
	void Start() {
		m_leapController = new Controller();
	}
	
	public void AddForcePoints(float points) {
		m_forcePoints += points;
	}
	
	public float GetForcePoints() {
		return m_forcePoints;
	}
	
	bool HandOpen(Hand hand) {
		return hand.Fingers.Count > 1;	
	}
	
	// returns condition for if force should affect this object.
	bool ObjectInteractable(GameObject obj) {
		if (obj.rigidbody == null) return false;
		if (obj.name == "Player") return false;
		
		// ignore objects not in the view frustum.
		if (GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(Camera.main), obj.collider.bounds) == false) {
			return false;
		}
		
		// ignore objects that are occluded from our view.
		RaycastHit hit;
		if(Physics.Linecast(transform.position, obj.transform.position, out hit)) {
			if (hit.transform != obj.transform) return false;	
		}
		
		return true;
	}
	
	void Levetate() {
		if (Time.time - m_lastPushTime < 1.0f) return;
		StageOneLevetate();
	}
	
	void StageOneLevetate() {
		Collider [] m_pullObjects = Physics.OverlapSphere(transform.position, 10.0f);
		for(int i = 0; i < m_pullObjects.Length; ++i) {
			GameObject gObject = m_pullObjects[i].transform.gameObject;
			
			if (ObjectInteractable(gObject) == false) continue;
			
			if (gObject.rigidbody.mass < 1.0f) {
				gObject.rigidbody.velocity = Vector3.up * Random.Range(0.5f, 0.8f) * (1.0f + (m_forcePoints / 500.0f));
				gObject.rigidbody.AddTorque(new Vector3(Random.Range(-0.02f, 0.02f), Random.Range(-0.02f, 0.02f), Random.Range(-0.02f, 0.02f)));	
			}
			else if (gObject.rigidbody.mass < 5.0f) {
				gObject.rigidbody.velocity = Vector3.up * Random.Range(0.3f, 0.4f) * (1.0f + (m_forcePoints / 500.0f));
				gObject.rigidbody.AddTorque(new Vector3(Random.Range(-0.01f, 0.01f), Random.Range(-0.01f, 0.01f), Random.Range(-0.01f, 0.01f)));
			}
			
		}
	}
	
	void MovePlayer(Hand leftHand, Hand rightHand) {
		
		// move forward starting at z = 0 and then scaling based on how far.
		Vector3 leftPos = leftHand.PalmPosition.ToUnityScaled();
		Vector3 rightPos = rightHand.PalmPosition.ToUnityScaled();
		float translateBias = leftPos.z + rightPos.z;
		if (leftPos.z > 0 || rightPos.z > 0 && (HandOpen(leftHand) || HandOpen(rightHand))) {
			Vector3 moveVelocity = transform.forward;
			moveVelocity.y = 0;
			rigidbody.velocity += moveVelocity * (translateBias * Mathf.Log(Mathf.Abs(translateBias))) * 0.1f;
		}
		
		// moving backwards should be slower
		if (leftPos.z < -1 && rightPos.z < -1 && HandOpen(leftHand) && HandOpen(rightHand)) {
			Vector3 moveVelocity = transform.forward;
			moveVelocity.y = 0;
			rigidbody.velocity += moveVelocity * (translateBias) * 0.05f;
		}
		
		// process rotation
		float yDelta = leftPos.y - rightPos.y;
		transform.RotateAround(Vector3.up, yDelta * 0.01f);	
	}
	
	void OneHand(Hand hand) {
		
	}
	
	void PushObjects(Hand leftHand, Hand rightHand) {
		if (Time.time - m_lastPushTime < 1.0f) return;
		Collider [] m_pullObjects = Physics.OverlapSphere(transform.position, 10.0f);
		for(int i = 0; i < m_pullObjects.Length; ++i) {
			GameObject gObject = m_pullObjects[i].transform.gameObject;
			
			if (ObjectInteractable(gObject) == false) continue;
			
			float z = Mathf.Max(leftHand.PalmVelocity.ToUnityScaled().z, rightHand.PalmVelocity.ToUnityScaled().z);
			gObject.rigidbody.velocity += transform.rotation * Vector3.forward * Mathf.Clamp(z, -15.0f, 20.0f);
			m_lastPushTime = Time.time;
		}
	}
	
	bool PushVelocity(Hand hand) {
		return hand.PalmVelocity.ToUnityScaled().z > 10.0f;	
	}
	
	void TwoHand(Hand hand1, Hand hand2) {
		Hand leftHand = hand1;
		Hand rightHand = hand2;
		if (hand1.PalmPosition.x > hand2.PalmPosition.x) {
			leftHand = hand2;
			rightHand = hand1;
		}
		
		MovePlayer(leftHand, rightHand);
		
		// if both hands close, use the force!
		if (!HandOpen(leftHand) && !HandOpen(rightHand)) {
			Levetate();
		}
		
		if (PushVelocity(leftHand) && PushVelocity(rightHand)) {
			PushObjects(leftHand, rightHand);
		}

	}
	
	void FixedUpdate () {
	
		Frame frame = m_leapController.Frame();

		if (frame.Hands.Count == 1) {
			OneHand(frame.Hands[0]);
		} else if (frame.Hands.Count == 2) {
			TwoHand(frame.Hands[0], frame.Hands[1]);
		}
		
	}
}
