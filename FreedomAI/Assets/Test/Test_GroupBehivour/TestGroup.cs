using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityECS;
using FreedomAI;


public class myGroupAI:UComponent
{
	//public 
	public float tempTimer = 0.0f;
}

public class GroupDemoWorld:AIWorld
{
	public override void registerAllComponent ()
	{
		base.registerAllComponent ();
		registerComponent (typeof(myGroupAI));
		registerComponent (typeof(myAI));
	} 
}

public class TestGroup
{
	public static float Test_Group_Strategy(AIEntity pSponsor,AIEntity[] pResponses)
	{
		Debug.Log ("GroupAI");
		return Random.Range (0.5f,1.0f);
	}

	public static float Test_Group_CubeStrategy(AIEntity pSponsor,AIEntity[] pResponses)
	{
		//Debug.Log ("GroupCube");
		return Random.Range (0.5f,1.0f);
	}

	public static void Test_Group_Enter (AIEntity pSponsor,AIEntity[] pResponses)
	{
		pSponsor.GetComponent<myGroupAI> ().tempTimer = 0.0f;
	}

	public static float Test_Group_Disslove (AIEntity pSponsor,AIEntity[] pResponses)
	{
		return 0.02f;
	}

	public static int[] Test_Group_Alloc(AIEntity pSponsor,AIEntity[] pResponses)
	{
		int[] ti = new int[pResponses.Length];
		for (int i = 0; i < pResponses.Length; i++) 
		{
			ti [i] = i + 1;
		}
		return ti;
	}

	public static void Test_Round(AIEntity pEntity)
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
		pEntity.GetComponent<AIMove> ().mMoveFunc = MoveFunc.Complex;
		//pEntity.GetComponent<myAI> ().tired = 0.0f;
		//Debug.Log ("sadsafa");
	}

	public static void Test_Round_Exit(AIEntity pEntity)
	{
		pEntity.GetComponent<AIMove> ().mDirection = Vector3.zero;
		pEntity.GetComponent<AIMove> ().mVelocity = 0.0f;
	}

	public static string Test_Group_Return_Anim(int pid)
	{
		if (pid == 0)
			return "Idle";
		else
			return "Walk";
	}

	public static string Test_Group_Round_Anim(int pid)
	{
		return "Walk";
	}

	public static void Test_Idle(AIEntity pEntity)
	{
		pEntity.GetComponent<myAI> ().IdleTimer += Time.deltaTime;
	}

	public static void Test_Idle_Exit(AIEntity pEntity)
	{
		pEntity.GetComponent<myAI> ().IdleTimer = 0.0f;
	}

	public static float Test_Round2Idle(AIEntity pEntity)
	{
		return 0.05f;
	}

	public static float Test_Idle2Round(AIEntity pEntity)
	{
		return pEntity.GetComponent<myAI> ().IdleTimer / 10.0f;
	}

	public static void Test_Group_Return_Enter(AIEntity pLeader,AIEntity[] pMembers,int pid)
	{
		pLeader.GetComponent<AIMove> ().mDirection = Vector3.zero;
		pLeader.GetComponent<AIMove> ().mVelocity = 0.0f;
		for (int i = 0; i < pMembers.Length; i++)
		{
			pMembers [i].GetComponent<AIMove> ().mDirection = Vector3.zero;
			pMembers [i].GetComponent<AIMove> ().mVelocity = 0.0f;
		}
	}

	public static void Test_Group_Return(AIEntity pLeader,AIEntity[] pMembers,int pid)
	{

		Vector3 target = Vector3.zero;
		if (pid == 0) 
		{
			return;
		}
		else if (pid == 1)
			target = pLeader.AIPos - pLeader.mAI.transform.forward.normalized * 3.0f;
		else if(pid==2)
			target = pLeader.AIPos - pLeader.mAI.transform.forward.normalized * 3.0f+pLeader.mAI.transform.right.normalized*2.0f;
		else if(pid==3)
			target = pLeader.AIPos - pLeader.mAI.transform.forward.normalized * 3.0f-pLeader.mAI.transform.right.normalized*2.0f;
		if (Vector3.Distance (target, pMembers [pid - 1].AIPos) < 0.5f)
			return;
		Vector3 dir = target - pMembers [pid-1].AIPos;
		pMembers [pid-1].GetComponent<AIMove> ().mDirection = dir.normalized;
		pMembers [pid-1].GetComponent<AIMove> ().mVelocity = pMembers[pid-1].GetComponent<myAI> ().mWalkVelocity;
	}

	public static void Test_Group_Return_Cube(AIEntity pLeader,AIEntity[] pMembers,int pid)
	{
		Vector3 target = Vector3.zero;
		if (pid == 0) 
		{
			return;
		}
		else if (pid == 1)
			target = pLeader.AIPos - pLeader.mAI.transform.forward.normalized * 10.0f+pLeader.mAI.transform.right.normalized*10.0f;
		else if(pid==2)
			target = pLeader.AIPos - pLeader.mAI.transform.forward.normalized * 10.0f-pLeader.mAI.transform.right.normalized*10.0f;
		else if(pid==3)
			target = pLeader.AIPos + pLeader.mAI.transform.forward.normalized * 10.0f-pLeader.mAI.transform.right.normalized*10.0f;
		else if(pid==4)
			target = pLeader.AIPos + pLeader.mAI.transform.forward.normalized * 10.0f+pLeader.mAI.transform.right.normalized*10.0f;
		if (Vector3.Distance (target, pMembers [pid - 1].AIPos) < 0.5f) 
		{
			pMembers [pid - 1].GetComponent<AIMove> ().mVelocity = 0.0f;
			pMembers [pid - 1].GetComponent<AIMove> ().mDirection = Vector3.zero;
			return;
		}
		Vector3 dir = target - pMembers [pid-1].AIPos;
		pMembers [pid-1].GetComponent<AIMove> ().mDirection = dir.normalized;
		pMembers [pid-1].GetComponent<AIMove> ().mVelocity = pMembers[pid-1].GetComponent<myAI> ().mWalkVelocity;
//		Debug.Log ("sasdf");
	}

	public static float Test_Group_Return2Round(AIEntity pLeader,AIEntity[] pMembers,int pid)
	{
		Vector3 target1, target2, target3;
		target1 = pLeader.AIPos - pLeader.mAI.transform.forward.normalized * 3.0f;
		target2 = pLeader.AIPos - pLeader.mAI.transform.forward.normalized * 3.0f+pLeader.mAI.transform.right.normalized*2.0f;
		target3 = pLeader.AIPos - pLeader.mAI.transform.forward.normalized * 3.0f-pLeader.mAI.transform.right.normalized*2.0f;
		float dis1, dis2, dis3;
		dis1 = Vector3.Distance (target1,pMembers[0].AIPos);
		dis2 = Vector3.Distance (target2,pMembers[1].AIPos);
		dis3 = Vector3.Distance (target3,pMembers[2].AIPos);
	//	Debug.Log (dis1+"  "+dis2+"  "+dis3);
		if (dis1 < 1.5f && dis2 < 1.5f && dis3 < 1.5f)
			return 1.0f;
		else
			return 0.0f;
	}

	public static void Test_Group_Round(AIEntity pLeader,AIEntity[] pMembers,int pid)
	{
		if (pLeader.GetComponent<myAI> ().tempRoundPoint == Vector3.zero)
		{
			Vector2 trandom = Random.insideUnitCircle;
			pLeader.GetComponent<myAI> ().tempRoundPoint = pLeader.GetComponent<myAI> ().mRoundCenter + (new Vector3 (trandom.x, 0, trandom.y)) * pLeader.GetComponent<myAI> ().RoundSize;
		}
		if (Vector3.Distance(pLeader.GetComponent<myAI>().tempRoundPoint,pLeader.AIPos)<1.0f)
		{
			Vector2 trandom = Random.insideUnitCircle;
			pLeader.GetComponent<myAI> ().tempRoundPoint = pLeader.GetComponent<myAI> ().mRoundCenter + (new Vector3 (trandom.x, 0, trandom.y)) * pLeader.GetComponent<myAI> ().RoundSize;
		}
		Vector3 tDir = pLeader.GetComponent<myAI> ().tempRoundPoint - pLeader.AIPos;
		pLeader.GetComponent<AIMove> ().mDirection = tDir.normalized;
		pLeader.GetComponent<AIMove> ().mVelocity = pLeader.GetComponent<myAI> ().mWalkVelocity;
		for (int i = 0; i < pMembers.Length; i++) 
		{
			pMembers[i].GetComponent<AIMove> ().mDirection = tDir.normalized;
			pMembers[i].GetComponent<AIMove> ().mVelocity = pMembers[i].GetComponent<myAI> ().mWalkVelocity;
		}
	}

}
