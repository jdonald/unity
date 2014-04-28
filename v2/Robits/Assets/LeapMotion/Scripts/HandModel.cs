/******************************************************************************\
* Copyright (C) Leap Motion, Inc. 2011-2014.                                   *
* Leap Motion proprietary and  confidential.  Not for distribution.            *
* Use subject to the terms of the Leap Motion SDK Agreement available at       *
* https://developer.leapmotion.com/sdk_agreement, or another agreement between *
* Leap Motion and you, your company or other organization.                     *
* Author: Matt Tytel
\******************************************************************************/

using UnityEngine;
using System.Collections;
using Leap;

// Interface for all hands.
public abstract class HandModel : MonoBehaviour {

  protected const int NUM_FINGERS = 5;
  private Hand hand_;

  public void SetLeapHand(Hand hand) { hand_ = hand; }
  public Hand GetLeapHand() { return hand_; }

  public abstract void InitHand(Transform deviceTransform);

  public abstract void UpdateHand(Transform deviceTransform);

  public abstract void DisableHand();
}
