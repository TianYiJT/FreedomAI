using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Charge : MonoBehaviour {

	public bool stop;
	public bool hitPlayer;
	// Use this for initialization
	void Start () {
		stop = false;
		hitPlayer = false;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnCollisionEnter(Collision collision){
		if (collision.transform.tag == "Player") {
			Debug.Log ("chargePlayer");
			stop = true;
			hitPlayer = true;
		} else if (collision.transform.tag == "Barrier") {
			Debug.Log ("chargeBarrier");
			stop = true;
			hitPlayer = false;
		}
	}
}
