using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GYTBullet : MonoBehaviour {
	// 关于子弹速度，其实是和枪械有很大关系，
	public float destroyTime= 10.0f;
	public float damage= 10.0f;
	public float vel = -1.0f;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (vel > 0.0f) {
			gameObject.GetComponent<Rigidbody> ().AddForce (transform.forward * vel);
			vel = -1.0f;
		}
			
		if (destroyTime < 0.0f)
			Destroy (this.gameObject);
		destroyTime -= Time.deltaTime;
	}
}
