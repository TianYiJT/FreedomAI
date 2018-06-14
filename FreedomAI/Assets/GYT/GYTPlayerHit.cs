using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GYTPlayerHit : MonoBehaviour {

	public GameObject player;
	public GameObject enemy;
	private GYTPlayerControl gc;
	//private Vector3 dir;
	//private Vector3 endPos;
	private bool forceMove;
	// Use this for initialization
	void Start () {
		gc = player.GetComponent<GYTPlayerControl> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (forceMove) {
			Vector3 dir = enemy.transform.position - player.transform.position;
			if (Vector3.Magnitude(dir) > 1.0f) {
				player.transform.Translate (dir.normalized * 20.0f * Time.deltaTime, Space.World);
			}
			else
				forceMove = false;
		}
	}

	void OnTriggerEnter (Collider collider)
	{
		// 关于敌人道具的标签：子弹：BulletE、陷阱球：TrapBall、近战武器：Melee
		// 不过陷阱球是在地上的，所以说，应该绑定在脚上
		if (collider.tag == "BulletE") {
			float damage = collider.gameObject.GetComponent<GYTBullet> ().damage;
			gc.HP -= damage;
			Destroy (collider.gameObject);
		} else if (collider.tag == "Melee") {
			// 只有在敌人“攻击”时触碰到才会受到伤害
			//Debug.Log ("Melee Damage");
			float damage = collider.gameObject.GetComponent<WeaponDamage> ().damage;
			gc.HP -= damage;
		} else if (collider.tag == "TrapBall") {
			//Debug.Log ("Trap Ball");
			float damage = collider.gameObject.GetComponent<TrapBallController> ().damage;
			gc.HP -= damage;
			gc.trapTime = collider.gameObject.GetComponent<TrapBallController> ().duration;
			Destroy (collider.gameObject);

		} else if (collider.tag == "Hook") {
			forceMove = true;
			enemy = collider.gameObject.GetComponent<Hook> ().enemy;
//			endPos = collider.gameObject.GetComponent<Hook> ().enemyPos;
//			dir = endPos - player.transform.position;
			gc.trapTime = collider.gameObject.GetComponent<Hook> ().trapTime;
//			player.GetComponent<Rigidbody> ().AddForce (dir.normalized * 10.0f);
//			player.transform.Translate (dir.normalized * 20.0f * Time.deltaTime, Space.World);
			Destroy (collider.gameObject);
		}
	}
}
