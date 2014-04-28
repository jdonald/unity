/******************************************************************************\
* Copyright (C) Leap Motion, Inc. 2011-2013.                                   *
* Leap Motion proprietary and  confidential.  Not for distribution.            *
* Use subject to the terms of the Leap Motion SDK Agreement available at       *
* https://developer.leapmotion.com/sdk_agreement, or another agreement between *
* Leap Motion and you, your company or other organization.                     *
\******************************************************************************/

using UnityEngine;
using System.Collections;

public class LeapJoint : MonoBehaviour {
	
	public GameObject[] m_jointPeices;

	void Awake () {
		m_jointPeices = new GameObject[2];
		for(int i = 0; i < m_jointPeices.Length; ++i)
		{
			m_jointPeices[i] = Instantiate(Resources.Load("Prefabs/Leap/SimpleLeapSegment")) as GameObject;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
