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

public class HandController : MonoBehaviour {

  class HandData {
    public int hand_id;
    public HandModel model;
  }

  public HandModel handModel;
  public HandModel handPhysicsModel;

  private Controller leap_controller_;
  private int num_active_hands_;
  private List<HandData> hands_;
  private List<HandData> physics_hands_;

  void Start () {
    leap_controller_ = new Controller();
    num_active_hands_ = 0;
    hands_ = new List<HandData>();
    physics_hands_ = new List<HandData>();
  }

  HandData CreateHand(HandModel model) {
    HandModel hand_model = Instantiate(model, transform.position, transform.rotation)
                           as HandModel;
    hand_model.transform.localScale = transform.localScale;

    HandData hand_data = new HandData();
    hand_data.model = hand_model;
    hand_data.hand_id = -1;

    return hand_data;
  }

  void UpdateModels(List<HandData> all_hand_data, HandModel model, HandList leap_hands) {
    // Show and update active hands.
    int num_hands = leap_hands.Count;
    int h = 0;
    for (; h < num_hands; ++h) {
      Hand leap_hand = leap_hands[h];

      // Create the hand if it doesn't exist.
      if (h >= all_hand_data.Count)
        all_hand_data.Add(CreateHand(model));
      
      HandData hand_data = all_hand_data[h];
      hand_data.model.SetLeapHand(leap_hand);

      // TODO: Mirror the right hand from the left.
      /*
      if (leap_hand.IsLeft)
        hand_data.model.transform.localScale = transform.localScale;
      else {
        // Negating scale and rotating 180 will mirror the object.
        hand_data.model.transform.rotation = Quaternion.AngleAxis(180, Vector3.left) * hand_data.model.transform.rotation;
        hand_data.model.transform.localScale = -transform.localScale;
      }
      */

      // Decide to Init or Update.
      int id = leap_hand.Id;
      if (num_active_hands_ <= h || id != hand_data.hand_id) {
        hand_data.model.InitHand(transform);
        hand_data.hand_id = id;
      }
      else
        hand_data.model.UpdateHand(transform);
    }

    // Hide inactive hands.
    for (; h < num_active_hands_; ++h) {
      all_hand_data[h].model.DisableHand();
    }
  }

  void FixedUpdate () {
    Frame frame = leap_controller_.Frame();
    
    UpdateModels(hands_, handModel, frame.Hands);
    UpdateModels(physics_hands_, handPhysicsModel, frame.Hands);

    num_active_hands_ = frame.Hands.Count;
  }
}
