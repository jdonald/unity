using UnityEngine;
using System.Collections;
using System;

public class Robot : MonoBehaviour {

  public Texture eyes_on_texture;
  public Texture eyes_off_texture;
  public int wait_time = 100;
  public bool reset_on_done = false;
  public float max_chomp_movement = 2.0f;
  int time_remaining_ = 0;
	
	void Update () {
    if (time_remaining_ <= 0)
      return;

    time_remaining_--;
    float phase = 2.0f * Mathf.PI * time_remaining_ / wait_time - Mathf.PI;
    float t = 0.5f + 0.5f * Mathf.Cos(phase);
    transform.Find("Mouth").transform.localPosition = new Vector3(0, t * max_chomp_movement, 0);
    
    if (time_remaining_ == 0) {
      transform.Find("Head").renderer.material.mainTexture = eyes_off_texture;

      if (reset_on_done)
        Application.LoadLevel(0);
    }
	}

  public void Eat() {
    if (time_remaining_ <= 0) {
      time_remaining_ = wait_time;
      transform.Find("Head").renderer.material.mainTexture = eyes_on_texture;
    }
  }
}
