using UnityEngine;
using System.Collections;

public class LevelBehavior : MonoBehaviour {
	private float _minX;
	private float _maxX;
	private GameObject _childLevel;
	private ButtonBehavior _lastSelectedOptionButton;
	private ButtonBehavior _spawnedChildOrigin;
	private bool _wasActiveLastFrame = false;
	private bool _highlighted = false;
	private bool _cleared = false;
	private LeapManager _leapManager;

	// Use this for initialization
	void Start () {
		if(gameObject.transform.childCount > 0)
		{
			_minX = gameObject.transform.GetChild(0).GetChild(0).renderer.bounds.min.x;
			_maxX = gameObject.transform.GetChild(0).GetChild(0).renderer.bounds.max.x;
		}
		else
		{
			Debug.LogError("Level has no buttons.");
		}

		_leapManager = (GameObject.Find("LeapManager") as GameObject).GetComponent(typeof(LeapManager)) as LeapManager;
	}

	// Update is called once per frame
	void Update () 
	{

		if(_childLevel != null) //The child menu has deactivated itself. Bubble up.
		{
			LevelBehavior child = (_childLevel.GetComponent(typeof(LevelBehavior)) as LevelBehavior);

			if(child == null || child.cleared == true)
			{
				_childLevel = null;
				clearSelf();
				return;
			}
		}

		if(_leapManager.pointerPositionScreenToWorld.x >= _minX && _leapManager.pointerPositionScreenToWorld.x <= _maxX)
		{
			_wasActiveLastFrame = true;

			if(_childLevel != null) { (_childLevel.GetComponent(typeof(LevelBehavior)) as LevelBehavior).clearChild(); }

			for(int i = 0; i < gameObject.transform.childCount; i++)
			{
				ButtonBehavior button = gameObject.transform.GetChild(i).GetComponent(typeof(ButtonBehavior)) as ButtonBehavior;

				button.changeState(ButtonBehavior.State.NORMAL);

				if(button.selected)
				{
					if(button._nextLinkage != null && 
					   button._selectionAction == "")
					{
						_lastSelectedOptionButton = null;
						if(button != _spawnedChildOrigin)
						{
							spawnLevel(button);
						}
						else
						{
							(_childLevel.GetComponent(typeof(LevelBehavior)) as LevelBehavior).highlight();
						}
					}
					else if(button._selectionAction != "" &&
					        button._nextLinkage == null)
					{
						clearChild();
						_lastSelectedOptionButton = button;
					}
				}
			}
		}
		else if(_wasActiveLastFrame)
		{
			if(_highlighted)
			{
				_highlighted = false;
			}
			else
			{
				_wasActiveLastFrame = false;

				if(_lastSelectedOptionButton != null && _leapManager.pointerPositionScreenToWorld.x > _maxX)
				{
					//Send button action up to a static listener:
					//...
					//Then do this:
					_lastSelectedOptionButton.changeState(ButtonBehavior.State.SELECTED);
					clearSelf(_lastSelectedOptionButton);
				}
				else
				{
					for(int i = 0; i < gameObject.transform.childCount; i++)
					{
						ButtonBehavior button = gameObject.transform.GetChild(i).GetComponent(typeof(ButtonBehavior)) as ButtonBehavior;

						if(button == _spawnedChildOrigin) { button.changeState(ButtonBehavior.State.SELECTED_PATH); }
						else { button.changeState(ButtonBehavior.State.DISABLED); }
					}
				}
			}
		}
	}

	void spawnLevel(ButtonBehavior button)
	{
		if(_childLevel != null) { clearChild(); }
		_childLevel = Instantiate(button._nextLinkage, 
		                                  new Vector3(_maxX, gameObject.transform.position.y, gameObject.transform.position.z), 
		                                  Quaternion.identity) as GameObject;
		_childLevel.layer = 8;
		_spawnedChildOrigin = button;
	}

	public void clearChild()
	{
		if(_childLevel != null) 
		{ 
			LevelBehavior child = (_childLevel.GetComponent(typeof(LevelBehavior)) as LevelBehavior);

			if(child != null && child.cleared != true)
			{
				child.clearSelf(); 
			}
			_childLevel = null;
			_spawnedChildOrigin = null;
		}
	}

	public void clearSelf(ButtonBehavior delayedDestory = null)
	{
		_cleared = true;

		clearChild();
		for(int i = 0; i < gameObject.transform.childCount; i++)
		{
			ButtonBehavior button = gameObject.transform.GetChild(i).GetComponent(typeof(ButtonBehavior)) as ButtonBehavior;
			Destroy(button.gameObject, ((button == delayedDestory) ? 1.5f : 0.0f));
		}

		Destroy(gameObject, 1.5f);
	}

	public void highlight()
	{
		_highlighted = true;
		_wasActiveLastFrame = true;

		for(int i = 0; i < gameObject.transform.childCount; i++)
		{
			ButtonBehavior button = gameObject.transform.GetChild(i).GetComponent(typeof(ButtonBehavior)) as ButtonBehavior;
			button.changeState(ButtonBehavior.State.HIGHLIGHT);
		}
	}	

	public bool cleared 
	{
		get { return _cleared; }
	}
}
