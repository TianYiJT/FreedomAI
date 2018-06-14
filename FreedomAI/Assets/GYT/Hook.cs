using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hook : MonoBehaviour {

	public bool stop;
	public bool hitPlayer;
	public float trapTime;
	public GameObject enemy;
	public float dTime = 3.0f;// 三秒后自动销毁
	public Vector3 enemyPos;// 当钩子击中玩家时，将玩家拉到敌人的位置
	public bool active = false;
	// Use this for initialization
	void Start () {
		stop = false;
		hitPlayer = false;
		trapTime = 5.0f;
		dTime = 3.0f;
	}
	
	// Update is called once per frame
	void Update () {
		if (active) {
		if (dTime < 0.0f)
			Destroy (this.gameObject);
		else
			dTime -= Time.deltaTime;
		}
	}

	void OnTriggerEnter (Collider collider)
	{
		if (collider.tag == "Player") {
			Debug.Log ("chargePlayer");
			stop = true;
			hitPlayer = true;
		} else if (collider.tag == "Barrier") {
			Debug.Log ("chargeBarrier");
			stop = true;
			hitPlayer = false;
			Destroy (this.gameObject);
		}

	}
}
