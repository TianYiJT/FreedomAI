using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FreedomAI;
using UnityECS;

public class FuSMTest
{
	public static float IdleAction(AIEntity pEntity)
	{
		return 0.52f;
	}

	public static float RunAwayAction(AIEntity pEntity)
	{
		int allHP = pEntity.GetComponent<myAI> ().allHP;
		int tempHP = pEntity.GetComponent<myAI> ().tempHP;
	//	Debug.Log ((float)(allHP - tempHP) / (float)allHP);
		return (float)(allHP - tempHP) / (float)allHP;
	}

	public static void Battle(AIEntity pEntity)
	{
		GameObject tObject = pEntity.GetComponent<ObstacleComponent> ().hitObject;
		pEntity.GetComponent<BaseAIComponent> ().mAIRT.transform.forward = tObject.transform.position - pEntity.GetComponent<BaseAIComponent> ().mAIRT.transform.position;
		if(tObject!=null)
			GameObject.Destroy (tObject);
	}

	public static float Gre2Runaway(AIEntity pEntity)
	{
		float dis = Vector3.Distance (pEntity.AIPos,pEntity.mPlayer.transform.position);
		if (dis > 10.0f)
			return 0.0f;
		return (10.0f - dis) / 10.0f;
	}

}

