using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyHit : MonoBehaviour {
	// 绑定在盾牌上，用于判断收到子弹的攻击
	// Use this for initialization
	public int hit;
	public float damage;
	void Start () {
		hit = 0;
		damage = 0;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter (Collider collider)
	{
		// 采用叠加方式，每当受到伤害时，进行伤害的叠加。更简洁
		if (collider.tag == "BulletP")
		{
			damage += collider.gameObject.GetComponent<GYTBullet> ().damage;
			Destroy (collider.gameObject);
		}
	}
}
