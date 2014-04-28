using UnityEngine;
using System.Collections;

public class VortexFade : MonoBehaviour {

	
	bool m_fadeDestroy = false;
	float m_targetRadius = 10.0f;
	float m_targetIntensity = 2.5f;
	
	public void SetTargetRadius(float radius) {
		m_targetRadius = radius;	
	}
	
	public void SetTargetIntensity(float intensity) {
		m_targetIntensity = intensity;	
	}
	
	public void FadeDestroy() {
		m_fadeDestroy = true;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (m_fadeDestroy) {
			light.range = Mathf.Lerp(light.range, 0.0f, 0.2f);
			if (light.range < 0.01f) {
				Destroy(transform.gameObject);	
			}
		} else {
			light.range = Mathf.Lerp(light.range, m_targetRadius, 0.2f);
			light.intensity = Mathf.Lerp(light.intensity, m_targetIntensity, 0.2f);
		}
	}
}
