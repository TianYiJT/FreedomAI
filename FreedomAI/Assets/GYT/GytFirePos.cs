using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GytFirePos : MonoBehaviour {

	public GameObject bullet;
	private Transform firePos;
	public float fireRate;
	public int ammo;
	public int vel;
	public bool shoot;

	public float nextOne;
	// Use this for initialization
	void Start () {
		nextOne = 0.5f;
		firePos = GetComponent<Transform> ();
	}

	// Update is called once per frame
	void Update () {
		if (shoot) {//Input.GetMouseButton (0)
			if (nextOne < 0.0f) {
				Fire ();
				nextOne = fireRate;
			} else {
				nextOne -= Time.deltaTime;
			}
		}
	}

	void Fire(){
		GameObject nbullet = Instantiate (bullet, firePos.position, firePos.rotation);
		nbullet.GetComponent<GYTBullet> ().vel = vel;

	}

	void OnDrawGizmos(){
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere (transform.position, 0.01f);
	}
}
