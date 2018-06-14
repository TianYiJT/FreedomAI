using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityECS;
using FreedomAI;


public class CollisionDemo : MonoBehaviour
{
	StateExecuter Round;
	StateExecuter Runaway;
	StateTranfer round2runaway;
	Animator tAnimator;
	AIEntity mAIEntity;
	void Start () 
	{
		tAnimator = GetComponent<Animator> ();
		Round = TestFSM.FSM_Round;
		Runaway = TestFSM.FSM_Runaway;
		round2runaway = TestCollisionAI.round2runaway;
		mAIEntity = new AIEntity ();
		ECSWorld.MainWorld.registerEntityAfterInit (mAIEntity);
		mAIEntity.Init ();
		mAIEntity.GetComponent<BaseAIComponent> ().mAIRT = this.gameObject;
		mAIEntity.AddComponent<myAI> (new myAI());
		mAIEntity.GetComponent<BaseAIComponent> ().mAIRT.SetActive (true);
		mAIEntity.GetComponent<AIAnimation> ().mAnimator = tAnimator;
		mAIEntity.GetComponent<AIAnimation> ().Add ("Run",TestFSM.FSM_Run_Anim);
		int id1=mAIEntity.GetComponent<AIState> ().AddExecuter (Round,EmptyExitAndEnter.EmptyExit,EmptyExitAndEnter.EmptyEnter);
		int id2=mAIEntity.GetComponent<AIState> ().AddExecuter (Runaway,EmptyExitAndEnter.EmptyExit,EmptyExitAndEnter.EmptyEnter);
		mAIEntity.GetComponent<AIState> ().tempID = id1;
		mAIEntity.GetComponent<AIState> ().AddEdge (round2runaway,EmptyFeedbacker.Run,id1,id2);
		mAIEntity.GetComponent<AIState> ().AddAnimation (Round,"Run");
		mAIEntity.GetComponent<AIState> ().AddAnimation (Runaway,"Run");
		mAIEntity.GetComponent<AIState> ().mStateRecorder = EmptyExitAndEnter.EmptyEnter;
	}

	void Update () 
	{
		mAIEntity.Log ();	
	}
}
