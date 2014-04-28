using UnityEngine;
using System.Collections;

public class FingerRenderer : MonoBehaviour {
	private GameObject finger;
	private LeapManager _leapManager;

	// Use this for initialization
	void Start () {
		finger = gameObject.transform.GetChild(0).gameObject as GameObject;
		 _leapManager = (GameObject.Find("LeapManager") as GameObject).GetComponent(typeof(LeapManager)) as LeapManager;
	}
	
	// Update is called once per frame
	void Update () {
		if(_leapManager.pointerAvailible)
		{
			finger.SetActive(true);

			finger.transform.localPosition = new Vector3(
				_leapManager.pointerPositionScreenToWorld.x,
				_leapManager.pointerPositionScreenToWorld.y,
				finger.transform.localPosition.z);
		}
		else
		{
			finger.SetActive(false);
		}
	}
}
