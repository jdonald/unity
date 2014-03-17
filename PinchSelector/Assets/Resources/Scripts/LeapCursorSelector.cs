using UnityEngine;
using System.Collections;
using Leap;

public class LeapCursorSelector : MonoBehaviour {
	
	Controller m_leapController;
	Color m_neutral = new Color(0.5f, 0.5f, 0.5f, 1.0f);
	Color m_hover = new Color(1.0f, 1.0f, 1.0f, 1.0f);
	Color m_selected = new Color(0.0f, 1.0f, 0.0f, 1.0f);
	GameObject m_selectedObject = null;
	
	// Use this for initialization
	void Start () {
		m_leapController = new Controller();
	}
	
	Hand GetFrontHand(Frame f) {
		float zComp = -float.MaxValue;
		Hand candidate = null;
		for(int i = 0; i < f.Hands.Count; ++i) {
			if (f.Hands[i].PalmPosition.ToUnityScaled().z > zComp) {
				candidate = f.Hands[i];
				zComp = f.Hands[i].PalmPosition.ToUnityScaled().z;
			}
		}
		return candidate;
	}
	
	// Update is called once per frame
	void Update () {
		Frame f = m_leapController.Frame();
		Hand h = GetFrontHand(f);
		if (h == null) return;
		InteractionBox box = f.InteractionBox;
		Vector3 handPos = h.PalmPosition.ToUnityScaled();
		
		transform.position = handPos;
		
		RaycastHit hit;
		Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
		
		screenPos.z = 0.0f;
		Ray r = Camera.main.ScreenPointToRay(screenPos);
		
		GameObject [] items = GameObject.FindGameObjectsWithTag("Item");
		for(int i = 0; i < items.Length; ++i) {
			if (items[i] == m_selectedObject) {
				items[i].renderer.material.color = m_selected;
			} else {
				items[i].renderer.material.color = m_hover;	
			}
		}
		
		if (Physics.Raycast(r, out hit)) {
			if (hit.collider.tag == "Item") {
				if (h.PinchStrength > 0.6f) {
					m_selectedObject = hit.collider.gameObject;	
				}
			}
		}
	}
}
