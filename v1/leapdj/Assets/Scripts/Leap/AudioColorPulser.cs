using UnityEngine;
using System.Collections;
using System.Linq;

public class AudioColorPulser : MonoBehaviour {
	
	
	float [] m_leftSamples = new float[64];
	float [] m_rightSamples = new float[64];
	
	public Color m_pulseColor;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		AudioListener.GetSpectrumData(m_leftSamples, 0, FFTWindow.BlackmanHarris);
		AudioListener.GetSpectrumData(m_rightSamples, 1, FFTWindow.BlackmanHarris);
		
		float sumTotal = 0.0f;
		for (int i = 0; i < 4; ++i) {
			sumTotal += Mathf.Abs(m_leftSamples[i]);	
		}
		
		for (int i = 0; i < 4; ++i) {
			sumTotal += Mathf.Abs(m_rightSamples[i]);	
		}
		
		sumTotal = Mathf.Min(1, sumTotal);
		
		if (sumTotal > 0.5f) {
			renderer.material.color = m_pulseColor * sumTotal;
			this.GetComponentInChildren<Light>().intensity = sumTotal;
		} else {
			renderer.material.color = m_pulseColor * 0.3f;
			this.GetComponentInChildren<Light>().intensity = 0.3f;
		}
	}
}
