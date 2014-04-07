/******************************************************************************\
* Copyright (C) Leap Motion, Inc. 2011-2014.                                   *
* Leap Motion proprietary and  confidential.  Not for distribution.            *
* Use subject to the terms of the Leap Motion SDK Agreement available at       *
* https://developer.leapmotion.com/sdk_agreement, or another agreement between *
* Leap Motion and you, your company or other organization.                     *
* Author: Matt Tytel
\******************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using Leap;

// Controller for the geometric hand.
public class GeoLeap : MonoBehaviour {

  public GeoHand handModel;

  private Controller leap_controller_;
  private int num_active_hands_;
  private List<GeoHand> hands_;
  private List<int> hand_ids_;

  void Start () {
    leap_controller_ = new Controller();
    num_active_hands_ = 0;
    hands_ = new List<GeoHand>();
    hands_.Add(handModel);
    hand_ids_ = new List<int>();
    hand_ids_.Add(-1);
  }
  
  void FixedUpdate () {
    Frame frame = leap_controller_.Frame();

    // Show and update active hands.
    int h = 0;
    for (; h < frame.Hands.Count; ++h) {
      if (h >= hands_.Count) {
        hands_.Add(Instantiate(handModel, transform.position, Quaternion.identity) as GeoHand);
        hands_[h].transform.localScale = transform.localScale;
        hands_[h].transform.localRotation = transform.localRotation;
        hands_[h].transform.parent = transform;
        hand_ids_.Add(-1);
      }

      if (num_active_hands_ <= h || frame.Hands[h].Id != hand_ids_[h]) {
        hands_[h].InitHand(frame.Hands[h]);
        hand_ids_[h] = frame.Hands[h].Id;
      }
      else
        hands_[h].UpdateHand(frame.Hands[h]);
    }

    // Hide inactive hands.
    for (; h < num_active_hands_; ++h)
      hands_[h].HideHand();

    num_active_hands_ = frame.Hands.Count;
  }
}
