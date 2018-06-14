using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FreedomAI;
using UnityECS;

public class GYTNoiseController : MonoBehaviour {


	public float range;// 控制碰撞器的大小
	SphereCollider sc;
	public bool noise;
	void Start ()
	{
		sc = GetComponent<SphereCollider> ();
		noise = false;
	}

	void OnTriggerStay(Collider other)
	{
		if (noise) {
			// 首先判断是否发出了声音
			noise = false;
		} else {
			return;
		}
		if (other.tag == "Enemy") {
			//Debug.Log ("track");
	
			List<AIEntity> theLast = AIEntity.getList ();
			AIEntity myFind = null;
			foreach (AIEntity ai in theLast) 
			{
				if (ai.GetComponent<BaseAIComponent> ().mAIRT == other.gameObject)
				{
					//Debug.Log ("give noise to the ai");
					myFind = ai;
					break;
				}
			}
			if(myFind!=null)
				myFind.GetComponent<getPlayer> ().getNoise = true;
		}


	}


	void Update ()
	{
		sc.radius = range;
	}


}
