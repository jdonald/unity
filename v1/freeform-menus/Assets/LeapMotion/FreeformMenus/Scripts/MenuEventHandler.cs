using UnityEngine;
using System.Collections;

public class MenuEventHandler : MonoBehaviour {
	TextMesh eventText;
	int i = 0;
	
	void Start () { 
		eventText = gameObject.GetComponent(typeof(TextMesh)) as TextMesh;
	}

	public void recieveMenuEvent(MenuBehavior.ButtonAction action)
	{
		++i;
		eventText.text = "Events:\n" + i + ": " + action.ToString() + eventText.text.Substring(7);
	}
}
