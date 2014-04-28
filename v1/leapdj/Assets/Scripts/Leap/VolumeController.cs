using UnityEngine;
using System.Collections;

public class VolumeController : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		AudioListener.volume = GameObject.Find("VolumeSlider").GetComponent<LeapSlider>().SliderValue();
	}
}
