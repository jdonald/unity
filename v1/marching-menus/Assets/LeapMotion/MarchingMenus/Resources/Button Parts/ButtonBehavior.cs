using UnityEngine;
using System.Collections;

public class ButtonBehavior : MonoBehaviour {
	public Sprite _normal;
	public Sprite _disabled;
	public Sprite _selected_path;
	public string _selectionAction; //Game Action to be performed when this item is selected.
	public GameObject _selectedSprite;
	public GameObject _nextLinkage; //Next level to be opened when this item is selected.
	public GameObject _stinger_type;

	public enum State { NORMAL, DISABLED, SELECTED_PATH, HIGHLIGHT, SELECTED };

	private State _currentState = State.NORMAL;
	private bool _selected = false;
	
	private GameObject _backing;
	private GameObject _fillBar;
	private GameObject _label;
	private GameObject _stinger;
	private LeapManager _leapManager;
	
	// Use this for initialization
	void Start () {
		_backing = 	gameObject.transform.GetChild(0).gameObject;
		_fillBar = 	_backing.transform.GetChild(0).gameObject;
		_label = 	gameObject.transform.GetChild(1).gameObject;

		if(_selectionAction != "" && _nextLinkage != null) { Debug.LogWarning("Button with both selection action and linkage detected."); }
		_leapManager = (GameObject.Find("LeapManager") as GameObject).GetComponent(typeof(LeapManager)) as LeapManager;
	}
	
	// Update is called once per frame
	void Update () {
		if(_currentState == State.NORMAL)
		{
			if(_leapManager.pointerAvailible &&
			   containsPoint(_leapManager.pointerPositionScreenToWorld))
			{
				_selected = true;

				if(_nextLinkage == null && _selectionAction != "")
				{
					if(!_stinger)
					{
						_stinger = Instantiate(_stinger_type, new Vector3(_backing.transform.position.x + _backing.renderer.bounds.size.x, 
						                                                  gameObject.transform.position.y, 
						                                                  gameObject.transform.position.z), 
						                       Quaternion.identity) as GameObject;
						_stinger.layer = 8;
						_stinger.transform.localScale = new Vector3(0,
						                                            1,
						                                            1);
					}

					if(_stinger.transform.localScale.x < 1.0f)
					{
						_stinger.transform.localScale = new Vector3(_stinger.transform.localScale.x + (5.0f * Time.deltaTime),
						                                            1,
						                                            1);
					}

					_fillBar.SetActive(true);
					var percent = Mathf.Min (1, Mathf.Max(0, (_leapManager.pointerPositionScreenToWorld.x - _backing.renderer.bounds.min.x) / (_backing.renderer.bounds.size.x)));

					_fillBar.transform.localScale = new Vector3(percent,
					                                           1,
					                                           1);
				}
				else if(_nextLinkage != null && _selectionAction == "")
				{

					_fillBar.SetActive(true);
					_fillBar.transform.localScale = new Vector3(1,
					                                            1,
					                                            1);
				}
			}
			else
			{
				if(_stinger)
				{
					Destroy(_stinger);
					_stinger = null;
				}
				_selected = false;
				_fillBar.SetActive(false);
			}
		}
		else
		{
			if(_stinger)
			{
				Destroy(_stinger);
				_stinger = null;
			}
		}
	}

	private bool containsPoint(Vector2 point)
	{
		Rect box = new Rect(
			gameObject.transform.position.x, 
			gameObject.transform.position.y, 
			gameObject.transform.GetChild(0).renderer.bounds.size.x, 
			gameObject.transform.GetChild(0).renderer.bounds.size.y);

		if(	point.x <= box.x + box.width && point.x >= box.x &&
		   point.y >= box.y + (-1*(box.height)) && point.y <= box.y)
		{
			return true;
		}
		
		return false;
	}

	public void changeState(State newState)
	{
		if(newState != _currentState && _currentState != State.SELECTED)
		{
			switch(newState)
			{
			case State.NORMAL:
				(_backing.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer).sprite = _normal;
				break;
			case State.DISABLED:
				_fillBar.SetActive(false);
				(_backing.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer).sprite = _disabled;
				break;
			case State.SELECTED_PATH:
				_fillBar.SetActive(false);
				(_backing.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer).sprite = _selected_path;
				break;
			case State.HIGHLIGHT:
				_fillBar.SetActive(false);
				(_backing.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer).sprite = _normal;
				break;
			case State.SELECTED:
				_fillBar.SetActive(true);
				_fillBar.transform.localScale = new Vector3(1,
				                                            1,
				                                            1);
				(_backing.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer).sprite = _normal;
				break;
			default:
				Debug.LogError("Unrecognized State passed to changeState(State newState)");
				return;
				break;
			}

			_currentState = newState;
		}
	}

	public State currentState
	{
		get { return _currentState; }
		set { _currentState = value; }
	}

	public bool selected
	{
		get { return _selected; }
	}
}
