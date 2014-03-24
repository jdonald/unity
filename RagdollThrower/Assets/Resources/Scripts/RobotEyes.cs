using UnityEngine;
using System.Collections;

public class RobotEyes : MonoBehaviour {

  public Texture on_texture;
  public Texture off_texture;
  public int wait_time = 100;
  public bool reset_on_done = false;
  int love_wait_ = 0;

	void Start () {
	}
	
	void Update () {
    love_wait_--;
    if (love_wait_ == 0) {
      renderer.material.mainTexture = off_texture;

      if (reset_on_done)
        Application.LoadLevel(0);
    }
	}

  public void Eat() {
    love_wait_ = wait_time;
    renderer.material.mainTexture = on_texture;
  }
}
