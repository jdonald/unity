using UnityEngine;
using System.Collections;

public class ButtonTarget : MonoBehaviour {

	int m_pressedCount = 0;
	public void SetPressed(bool pressed) {
		if (pressed) ++m_pressedCount;
		else m_pressedCount = Mathf.Max(m_pressedCount - 1, 0);
	}
	
	public int GetPressCount() {
		return m_pressedCount;	
	}
	
	public bool IsPressed() {
		return (m_pressedCount > 0);
	}
}
