using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FreedomAI;
using UnityECS;

public class DemoWorld:AIWorld
{
	public override void registerAllComponent ()
	{
		base.registerAllComponent ();
		registerComponent (typeof(myAI));
	}
	public override void registerAllEntity ()
	{
		base.registerAllEntity ();
	}
	public override void registerAllSystem ()
	{
		base.registerAllSystem ();
	}
}

public class myAI:UComponent
{
	public float mRunVelocity = 7.0f;
	public float mWalkVelocity = 4.0f;
	public Vector3 mRoundCenter = Vector3.zero;
	public float RoundSize = 20.0f;
	public Vector3 tempRoundPoint = Vector3.zero;
	public float fireRate = 0.2f;
	public float GreRate = 0.5f;
	public GameObject fireObject;
	public float fireVelocity = 35.0f;
	public bool GrenateFinished = false;
	public GameObject GrenateObject;
	public float GrenateVelocity = 20.0f;
	public int allHP =500;
	public int tempHP = 500;
	public int Gre_number = 0;
	public float fire_timer = 0;
	public float tired = 0.0f;
	public float IdleTimer = 0.0f;
}

public class TestFSM
{

	public static void FSM_Run_Anim(Animator pAnimator)
	{
		pAnimator.SetBool ("Run",true);
		pAnimator.SetBool ("Shoot",false);
		pAnimator.SetBool ("Idle",false);
		pAnimator.speed = 1.3f;
	}

	public static void FSM_Walk_Anim(Animator pAnimator)
	{
		pAnimator.SetBool ("Run",true);
		pAnimator.SetBool ("Shoot",false);
		pAnimator.SetBool ("Idle",false);
		pAnimator.speed = 1.0f;
	}

	public static void FSM_Shoot_Anim(Animator pAnimator)
	{
		pAnimator.SetBool ("Run",false);
		pAnimator.SetBool ("Shoot",true);
		pAnimator.SetBool ("Idle",false);
		pAnimator.speed = 1.0f;
	}

	public static void FSM_Idle_Anim(Animator pAnimator)
	{
		pAnimator.SetBool ("Run",false);
		pAnimator.SetBool ("Shoot",false);
		pAnimator.SetBool ("Idle",true);
		pAnimator.speed = 1.0f;
	}

	public static void FSM_Test_Recorder(AIEntity pEntity)
	{
		pEntity.GetComponent<AIState> ().LastEntityData.AIPos = pEntity.AIPos;
		pEntity.GetComponent<AIState> ().LastEntityData.PlayerPos = pEntity.PlayerPos;
		pEntity.GetComponent<AIState> ().LastEntityData.PlayerEntity.GetComponent<HPComponent> ().tempHP = pEntity.PlayerEntity.GetComponent<HPComponent> ().tempHP;
		pEntity.GetComponent<AIState> ().LastEntityData.PlayerEntity.GetComponent<HPComponent> ().tempHurt = pEntity.PlayerEntity.GetComponent<HPComponent> ().tempHurt;
	}

	public static void FSM_Run(AIEntity pEntity)
	{
		Vector3 tDir = pEntity.PlayerPos - pEntity.AIPos;
		pEntity.GetComponent<AIMove> ().mDirection = tDir.normalized;
		pEntity.GetComponent<AIMove> ().mVelocity = pEntity.GetComponent<myAI> ().mRunVelocity;
		pEntity.GetComponent<AIMove> ().mMoveFunc = MoveFunc.RVO;
		pEntity.GetComponent<myAI> ().tired += 10.0f;
		pEntity.GetComponent<myAI> ().tempHP -= 1;

	}

	public static void FSM_Run_Exit(AIEntity pEntity)
	{
		pEntity.GetComponent<AIMove> ().mDirection = Vector3.zero;
		pEntity.GetComponent<AIMove> ().mVelocity = 0.0f;
	}

	public static void FSM_Round(AIEntity pEntity)
	{
		
		if (pEntity.GetComponent<myAI> ().tempRoundPoint == Vector3.zero)
		{
			Vector2 trandom = Random.insideUnitCircle;
			pEntity.GetComponent<myAI> ().tempRoundPoint = pEntity.GetComponent<myAI> ().mRoundCenter + (new Vector3 (trandom.x, 0, trandom.y)) * pEntity.GetComponent<myAI> ().RoundSize;
		}
		if (Vector3.Distance(pEntity.GetComponent<myAI>().tempRoundPoint,pEntity.AIPos)<1.0f)
		{
			Vector2 trandom = Random.insideUnitCircle;
			pEntity.GetComponent<myAI> ().tempRoundPoint = pEntity.GetComponent<myAI> ().mRoundCenter + (new Vector3 (trandom.x, 0, trandom.y)) * pEntity.GetComponent<myAI> ().RoundSize;
		}
		Vector3 tDir = pEntity.GetComponent<myAI> ().tempRoundPoint - pEntity.AIPos;
		pEntity.GetComponent<AIMove> ().mDirection = tDir.normalized;
		pEntity.GetComponent<AIMove> ().mVelocity = pEntity.GetComponent<myAI> ().mWalkVelocity;
		pEntity.GetComponent<AIMove> ().mMoveFunc = MoveFunc.RVO;

	}

	public static void FSM_Idle(AIEntity pEntity)
	{
		
	}

	public static void FSM_Fire(AIEntity pEntity)
	{
		if (pEntity.GetComponent<myAI> ().fireRate > 0.0f) 
		{
			pEntity.GetComponent<myAI> ().fireRate -= Time.deltaTime;
		}
		else
		{
			pEntity.GetComponent<myAI> ().fireRate = 0.2f;
			GameObject tfire = GameObject.Instantiate (pEntity.GetComponent<myAI>().fireObject,pEntity.AIPos,Quaternion.identity) as GameObject;
			tfire.GetComponent<Rigidbody> ().velocity = pEntity.GetComponent<BaseAIComponent> ().mAIRT.transform.forward*pEntity.GetComponent<myAI>().fireVelocity;
			pEntity.GetComponent<myAI> ().tempHP -= 5;
		
		}
		pEntity.GetComponent<BaseAIComponent>().mAIRT.transform.forward = (pEntity.PlayerPos - pEntity.AIPos).normalized;
	//	pEntity.GetComponent<AIForward> ().mForward = (pEntity.PlayerPos - pEntity.AIPos).normalized;
		pEntity.GetComponent<myAI> ().fire_timer += Time.deltaTime;
		pEntity.GetComponent<myAI> ().tired += 1.0f;

	}

	public static void FSM_Fire_Exit(AIEntity pEntity)
	{
		pEntity.GetComponent<myAI> ().fire_timer = 0.0f;
		pEntity.GetComponent<myAI> ().fireRate = 0.2f;
	}

	public static void FSM_Grenate(AIEntity pEntity)
	{
		if (pEntity.GetComponent<myAI> ().GreRate > 0.0f) 
		{
			pEntity.GetComponent<myAI> ().GreRate -= Time.deltaTime;
			return;
		}
		GameObject tGrenate = GameObject.Instantiate (pEntity.GetComponent<myAI>().GrenateObject,pEntity.AIPos,Quaternion.identity) as GameObject;
		pEntity.GetComponent<BaseAIComponent>().mAIRT.transform.forward = (pEntity.PlayerPos - pEntity.AIPos).normalized;
		Vector3 dir = pEntity.GetComponent<BaseAIComponent> ().mAIRT.transform.forward.normalized + Vector3.up;
		dir.Normalize ();
		tGrenate.GetComponent<Rigidbody>().velocity = dir*pEntity.GetComponent<myAI>().GrenateVelocity;
		pEntity.GetComponent<myAI> ().Gre_number++;
		pEntity.GetComponent<myAI> ().tired += 5.0f;
		pEntity.GetComponent<myAI> ().GreRate = 0.5f;
		pEntity.GetComponent<myAI> ().tempHP -= 10;
	}

	public static void FSM_Gre_Exit(AIEntity pEntity)
	{
		pEntity.GetComponent<myAI> ().Gre_number = 0;
		pEntity.GetComponent<myAI> ().GreRate = 0.5f;
	}

	public static void FSM_Runaway(AIEntity pEntity)
	{
		//pEntity.GetComponent<myAI> ().tempHP += 2;
		Vector3 tDir = pEntity.AIPos - pEntity.PlayerPos;
		pEntity.GetComponent<AIMove> ().mDirection = tDir.normalized;
		pEntity.GetComponent<AIMove> ().mVelocity = pEntity.GetComponent<myAI> ().mWalkVelocity;
		pEntity.GetComponent<AIMove> ().mMoveFunc = MoveFunc.Complex;
		//pEntity.GetComponent<AIForward> ().mForward = tDir.normalized;
		pEntity.GetComponent<myAI> ().tired += 8.0f;
	}

	public static float FSM_Any_TransferRunAway(AIEntity pEntity)
	{
		
		if (pEntity.GetComponent<myAI> ().tempHP < pEntity.GetComponent<myAI> ().allHP * 0.5f) 
		{
			//Debug.Log ("FSM_ANY");
			float rate = (0.5f * pEntity.GetComponent<myAI> ().allHP - pEntity.GetComponent<myAI> ().tempHP) / (0.5f * pEntity.GetComponent<myAI> ().allHP);
			if (pEntity.GetComponent<AIEmotion> ().GetTempEmotion () == "Fear")
				rate *= 1.5f;
			return rate;
		}
		return 0.0f;
	}

	public static float FSM_RunAway_Grenerate(AIEntity pEntity)
	{
		
		float dis = Vector3.Distance (pEntity.AIPos,pEntity.PlayerPos);
		if (dis < 5.0f||dis>15.0f) 
		{
			return 0.0f;
		}
		else
		{
			float maxrate = 0.1f;
			maxrate *= (15.0f - dis) / 10.0f;
			return maxrate;
		}
	}

	public static float FSM_Gre_Fire(AIEntity pEntity)
	{
		return (float)pEntity.GetComponent<myAI> ().Gre_number / 20.0f;
	}

	public static float FSM_Fire_Gre(AIEntity pEntity)
	{
		return pEntity.GetComponent<myAI> ().fire_timer / 10.0f;
	}

	public static float FSM_Fire_Run(AIEntity pEntity)
	{
		float dis = Vector3.Distance (pEntity.AIPos,pEntity.PlayerPos);
		if (dis < 8.0f) 
		{
			return 0.0f;
		}
		else
		{
			return (dis - 8.0f) *0.1f;
		}
	}

	public static float FSM_Run_Fire(AIEntity pEntity)
	{
		float dis = Vector3.Distance (pEntity.AIPos,pEntity.PlayerPos);
		if (pEntity.GetComponent<AIEmotion> ().GetTempEmotion () == "Happy")
			return (20.0f - dis) * 0.1f*1.5f;
		else
			return (20.0f - dis) * 0.1f;
	}

	public static float FSM_Gre_Run(AIEntity pEntity)
	{
		float dis = Vector3.Distance (pEntity.AIPos,pEntity.PlayerPos);
		if (dis < 5.0f) 
		{
			return 0.0f;
		}
		else
		{
			return (dis - 5.0f) *0.1f;
		}
	}

	public static float FSM_Run_Gre(AIEntity pEntity)
	{
		float dis = Vector3.Distance (pEntity.AIPos,pEntity.PlayerPos);
		if (pEntity.GetComponent<AIEmotion> ().GetTempEmotion () == "Happy")
			return (20.0f - dis) * 0.1f*1.5f;
		else
			return (20.0f - dis) * 0.1f;
	}

	public static float FSM_Round_Run(AIEntity pEntity)
	{
		float dis = Vector3.Distance (pEntity.AIPos,pEntity.PlayerPos);
		if (dis > 15.0f) 
		{
			return 0.0f;
		}
		else
		{
			return (15.0f - dis) * 0.1f;
		}
	}

	public static float FSM_Run_Round(AIEntity pEntity)
	{
		float dis = Vector3.Distance (pEntity.AIPos,pEntity.PlayerPos);
		if (dis < 5.0f) 
		{
			return 0.0f;
		}
		else
		{
			if (pEntity.GetComponent<AIEmotion> ().GetTempEmotion () == "tired")
				return (dis-10.0f) * 0.1f *1.5f;
			else
				return (dis-10.0f) * 0.1f;
		}
	}

	public static float FSM_RunIN(AIEntity pEntity,bool isNow)
	{
		float dis = Vector3.Distance (pEntity.AIPos,pEntity.PlayerPos);
		return dis / 40.0f;
	}

	public static float FSM_RunOUT(AIEntity pEntity,bool isNow)
	{
		float dis = Vector3.Distance (pEntity.AIPos,pEntity.PlayerPos);
		return (40.0f-dis) / 40.0f;
	}

	public static float FSM_HurtUp(AIEntity pEntity,bool isNow)
	{
		int PlayerHP = pEntity.PlayerEntity.GetComponent<HPComponent> ().tempHP;
		int allHP = pEntity.PlayerEntity.GetComponent<HPComponent> ().allHP;
		if (allHP > 0)
			return (allHP - PlayerHP) / allHP;
		else
			return 0.0f;
	}

	public static float FSM_Fear(AIEntity pEntity)
	{
		int tempHP = pEntity.GetComponent<myAI> ().tempHP;
		int allHP = pEntity.GetComponent<myAI> ().allHP;
		if (tempHP > allHP * 0.6f)
			return 0.0f;
		else
			return (0.6f * allHP - tempHP) / (0.6f * allHP);
	}

	public static float FSM_Happy(AIEntity pEntity)
	{
		int tempHurt = pEntity.GetComponent<HPComponent> ().tempHurt;
		tempHurt += (int)(1000.0f-pEntity.GetComponent<myAI>().tired);
		return tempHurt / 1000.0f;
	}

	public static float FSM_Tired(AIEntity pEntity)
	{
		float tempTired = pEntity.GetComponent<myAI> ().tired;
		return tempTired / 800.0f;
	}

}
