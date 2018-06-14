using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapBallController : MonoBehaviour {

	public Vector3 endPos;//最终停止的位置
	public bool active;
	public float damage = 10.0f;
	public float duration = 5.0f;
	// Use this for initialization
	void Start () {
		//endPos = gameObject.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		if (active) {
		if (Vector3.Distance (gameObject.transform.position, endPos) > 1.0f) {
			Vector3 dir = endPos - gameObject.transform.position;
			gameObject.transform.Translate (dir.normalized * 10.0f * Time.deltaTime, Space.World);
		} else {
			// 到达位置后，不断旋转
			gameObject.transform.Rotate(Vector3.down*30.0f*Time.deltaTime,Space.World);
		}
	}
	}
}
