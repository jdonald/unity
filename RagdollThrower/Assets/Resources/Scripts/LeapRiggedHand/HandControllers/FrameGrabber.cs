using UnityEngine;
using System.Collections;
using Leap;

public class FrameGrabber : MonoBehaviour {

	Controller m_leapController;
	public Frame m_currentFrame;

	// Use this for initialization
	void Start () {
		m_leapController = new Controller();
		m_currentFrame = m_leapController.Frame();
	}
	
	// Update is called once per frame
	void Update () {
		m_currentFrame = m_leapController.Frame();
	}
}
