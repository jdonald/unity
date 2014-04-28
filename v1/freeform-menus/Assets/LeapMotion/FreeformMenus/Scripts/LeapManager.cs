using UnityEngine;
using System.Collections;
using Leap;

public class LeapManager2 : MonoBehaviour {
	public static Vector3 _frontFingerPosition_Screen;
	public static Vector3 _frontFingerPosition;
	public static bool _frontFingerActive;
	public static ScaleState _scaleState;
	public static float _scaleRate;

	public float _scaleMax;
	public AnimationCurve _scaleFilter;
	public int _scaleWindow;
	public float _scaleBound;
	public Vector3 _leapMin; //Minimum bounds for interaction space
	public Vector3 _leapMax; //Maximium bounds for interaction space
	public Vector3 _worldMin; //0 in leap space = this is world space
	public Vector3 _worldMax; //1 in leap space = this in world space
	
	private Controller _controller = new Controller();
	private Camera cam;

	public enum ScaleState { IN, OUT, NONE };

	// Use this for initialization
	void Start () {
		cam = gameObject.transform.parent.GetComponent(typeof(Camera)) as Camera;
	}
	
	// Update is called once per frame
	void Update () {
		Frame frame = _controller.Frame();
		
		if(frame.Fingers.Count > 0)
		{
			//LeapManager._frontFingerActive = true;
			//LeapManager._frontFingerPosition = toWorld(to3DNormal(frame.Fingers.Frontmost.TipPosition));
			//LeapManager._frontFingerPosition_Screen = toScreen(to3DNormal(frame.Fingers.Frontmost.TipPosition));
		}
		else
		{
			//LeapManager._frontFingerActive = false;
		}

		checkScaling(frame, _controller.Frame(_scaleWindow));
		//calculateHandSpread(frame);
	}
	
	Vector3 to3DNormal(Vector tipPosition)
	{
		return new Vector3(
			-1*((tipPosition.x - _leapMin.x) / (_leapMin.x - _leapMax.x)),
			-1*((tipPosition.y - _leapMin.y) / (_leapMin.y - _leapMax.y)),
			((tipPosition.z - _leapMin.z) / (_leapMin.z - _leapMax.z))
			);
	}

	Vector3 toWorld(Vector3 normalPosition)
	{
		return new Vector3(_worldMin.x + (normalPosition.x * _worldMax.x),
		                   _worldMin.y + (normalPosition.y * _worldMax.y),
		                   _worldMin.z + (normalPosition.z * _worldMax.z));
	}


	Vector2 toScreen(Vector3 normalPosition)
	{
		return new Vector2(normalPosition.x * cam.orthographicSize,
		                   (normalPosition.y-0.5f) * (cam.orthographicSize / cam.aspect));
	}

	void checkScaling(Frame frame, Frame startFrame)
	{
		float logScale = Mathf.Log(frame.ScaleFactor(startFrame)) * 100;

		//LeapManager._scaleProb = frame.ScaleProbability(startFrame);

		if(frame.Hands.Count == 2)
		{
			int sign = 1;

			if(logScale < 0) sign = -1;

			float norm = Mathf.Clamp((Mathf.Abs(logScale) - _scaleBound) / (_scaleMax - _scaleBound), 0.0f, 1.0f);
			float postFilter = _scaleFilter.Evaluate(norm) * sign;

			_scaleRate = postFilter;
		}
		else
		{
			_scaleRate = 0;
		}
	}
}
