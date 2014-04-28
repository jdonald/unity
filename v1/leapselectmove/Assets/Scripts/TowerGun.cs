using UnityEngine;
using System.Collections;

public class TowerGun : MonoBehaviour {
	
	ButtonTarget m_buttonTarget;
	// Use this for initialization
	void Start () {
		m_buttonTarget = GetComponent<ButtonTarget>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		
		Transform [] guns = GetComponentsInChildren<Transform>();
		for(int i = 0; i < guns.Length; ++i) {
			if (guns[i].name != "Gun") continue;
			
			Vector3 scale = guns[i].localScale;
			if (m_buttonTarget.IsPressed()) scale.z = Mathf.Lerp(scale.z, 0.05f, 0.01f);
			else scale.z = Mathf.Lerp(scale.z, 1.5f, 0.01f);
			
			guns[i].localScale = scale;
			
		}
		transform.RotateAround(Vector3.up, 0.1f);
	}
}
