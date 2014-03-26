using UnityEngine;
using System.Collections;

public class FoodScore : MonoBehaviour {

  private bool eaten_ = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
    if (transform.position.z > GameObject.Find("Mouth").transform.position.z) {
      if (!eaten_)
        GameObject.Find("/Robot").GetComponent<Robot>().Eat();
      eaten_ = true;
    }
	}
}
