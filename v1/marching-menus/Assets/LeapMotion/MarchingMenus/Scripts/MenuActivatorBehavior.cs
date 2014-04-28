using UnityEngine;
using System.Collections;

public class MenuActivatorBehavior : MonoBehaviour {
	public GameObject _Root;
	public Position _placement = Position.TOP_LEFT; //We use this to determine the active detection zone.

	public enum Position { TOP_LEFT }; //We could add more of these and change the detection logic.

	private bool _cancelArmed = false;
	private bool _activateArmed = false;
	private GameObject _menu;
	private LeapManager _leapManager;

	// Use this for initialization
	void Start () {
		_leapManager = (GameObject.Find("LeapManager") as GameObject).GetComponent(typeof(LeapManager)) as LeapManager;
	}
	
	// Update is called once per frame
	void Update () {
		if(_menu == null)
		{
			if(inActiveZone())
			{
				if(_activateArmed)
				{
					_activateArmed = false;
					_menu = Instantiate(_Root, new Vector3(gameObject.transform.position.x,
					                                     gameObject.transform.position.y,
					                                     gameObject.transform.position.z - 1),
					                   Quaternion.identity) as GameObject;
					_menu.layer = 8;
					gameObject.renderer.enabled = false;
				}
			}
			else
			{
				_activateArmed = true;
			}
		}
		else
		{
			if((_menu.GetComponent(typeof(LevelBehavior)) as LevelBehavior) == null || (_menu.GetComponent(typeof(LevelBehavior)) as LevelBehavior).cleared) //If the menu has self destructed.
			{
				Destroy(_menu, 5.0f);
				_menu = null;
				gameObject.renderer.enabled = true;
			}

			if(_cancelArmed == false && inActiveZone() == false && _leapManager.pointerPositionScreenToWorld.x > gameObject.transform.position.x + 0.2) { _cancelArmed = true; }

			if(_leapManager.pointerPositionScreenToWorld.x < gameObject.transform.position.x) 
			{ 
				(_menu.GetComponent(typeof(LevelBehavior)) as LevelBehavior).clearChild(); 
				(_menu.GetComponent(typeof(LevelBehavior)) as LevelBehavior).highlight(); 
			}

			if(_cancelArmed == true && _leapManager.pointerPositionScreenToWorld.x < gameObject.transform.position.x) 
			{
				_cancelArmed = false;
				clearMenu(); 
				gameObject.renderer.enabled = true;
			}
		}
	}

	bool inActiveZone()
	{
		Vector2 point = _leapManager.pointerPositionScreenToWorld;

		Rect box = new Rect(
			gameObject.transform.position.x, 
			gameObject.transform.position.y, 
			gameObject.renderer.bounds.size.x, 
			gameObject.renderer.bounds.size.y);


		if(_placement == Position.TOP_LEFT)
		{
			if(	point.x <= box.x + box.width &&
			   point.y >= box.y + (-1*(box.height)))
			{
				return true;
			}
		}

		return false;
	}

	void clearMenu()
	{
		if(_menu != null) {
			(_menu.GetComponent(typeof(LevelBehavior)) as LevelBehavior).clearSelf(); 
			Destroy(_menu);
			_menu = null;
		}
	}
}