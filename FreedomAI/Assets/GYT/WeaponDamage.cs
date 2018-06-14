using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponDamage : MonoBehaviour {
	public float damage;
	public bool active;
	public float duration;// 用于标识陷阱球的封锁时间
	// Use this for initialization
	void Start () {
		active = false;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
