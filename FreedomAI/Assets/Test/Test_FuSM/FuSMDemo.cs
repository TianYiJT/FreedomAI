using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityECS;
using FreedomAI;

public class FuSMDemo : MonoBehaviour {

	public GameObject AI;
	public GameObject player;
	public GameObject shootObject;
	public GameObject GrenateObject; 

	Animator animator;
	AnimationPlay runAnim;
	AnimationPlay walkAnim;
	AnimationPlay idleAnim;
	AnimationPlay shootAnim;
	StateRecorder mRecorder;
	StateExecuter runstate;
	StateExit runExit;
	StateExecuter roundstate;
	StateExecuter firestate;
	StateExit fireExit;
	StateExecuter Grenatestate;
	StateExit GreExit;
	StateExecuter runawaystate;
	StateTranfer anytorunaway;
	StateTranfer runawaytoGre;
	StateTranfer firetoGre;
	StateTranfer Gretofire;
	StateTranfer firetorun;
	StateTranfer runtofire;
	StateTranfer runtoround;
	StateTranfer roundtorun;
	StateTranfer runtogre;
	StateTranfer gretorun;
	StateFeedbacker runIN;
	StateFeedbacker runOUT;
	StateFeedbacker hurtup;
	EmotionExecuter fear;
	EmotionExecuter happy;
	EmotionExecuter tired;
	HPComponent player_hp = new HPComponent();
	AIEntity mAIEntity = new AIEntity();
	UEntity mPlayer = new UEntity();
	UEntity mPlayerLast = new UEntity ();
	HPComponent playerLast_hp = new HPComponent ();

	StrategyActioner IdleActioner;
	StrategyActioner RunAwayActioner;
	StateExecuter BattleState;
	StateTranfer Gre2Run; 


	void Start () 
	{
		ECSWorld.MainWorld.registerEntityAfterInit (mAIEntity);
		ECSWorld.MainWorld.registerEntityAfterInit (mPlayer);
		IdleActioner = FuSMTest.IdleAction;
		RunAwayActioner = FuSMTest.RunAwayAction;
		BattleState = FuSMTest.Battle;
		Gre2Run = FuSMTest.Gre2Runaway;
		runAnim = TestFSM.FSM_Run_Anim;
		walkAnim = TestFSM.FSM_Walk_Anim;
		idleAnim = TestFSM.FSM_Idle_Anim;
		shootAnim = TestFSM.FSM_Shoot_Anim;
		mRecorder = TestFSM.FSM_Test_Recorder;
		runstate = TestFSM.FSM_Run;
		roundstate = TestFSM.FSM_Round;
		runExit = TestFSM.FSM_Run_Exit;
		firestate = TestFSM.FSM_Fire;
		fireExit = TestFSM.FSM_Fire_Exit;
		Grenatestate = TestFSM.FSM_Grenate;
		GreExit = TestFSM.FSM_Gre_Exit;
		runawaystate = TestFSM.FSM_Runaway;
		anytorunaway = TestFSM.FSM_Any_TransferRunAway;
		firetoGre = TestFSM.FSM_Fire_Gre;
		Gretofire = TestFSM.FSM_Gre_Fire;
		firetorun = TestFSM.FSM_Fire_Run;
		runtofire = TestFSM.FSM_Run_Fire;
		runtoround = TestFSM.FSM_Run_Round;
		roundtorun = TestFSM.FSM_Round_Run;
		runtogre = TestFSM.FSM_Run_Gre;
		gretorun = TestFSM.FSM_Gre_Run;
		runIN = TestFSM.FSM_RunIN;
		runOUT = TestFSM.FSM_RunOUT;
		hurtup = TestFSM.FSM_HurtUp;
		fear = TestFSM.FSM_Fear;
		happy = TestFSM.FSM_Happy;
		tired = TestFSM.FSM_Tired;
		//	mAIEntity.forward_Object = forward_Object;
		mAIEntity.mAI = AI;
		mAIEntity.mPlayer = player;
		mAIEntity.PlayerEntity = mPlayer;
		mAIEntity.AIPos = player.transform.position + new Vector3 (Random.Range(-10,10),0,Random.Range(-10,10));
		mAIEntity.Init ();
		mAIEntity.AddComponent<myAI> (new myAI());
		mAIEntity.GetComponent<BaseAIComponent> ().mAIRT.SetActive (true);
		mAIEntity.PlayerEntity = mPlayer;
		mAIEntity.GetComponent<HPComponent> ().allHP = 500;
		mAIEntity.GetComponent<HPComponent> ().tempHP = 500;
		animator = mAIEntity.GetComponent<BaseAIComponent> ().mAIRT.GetComponent<Animator> ();
		mAIEntity.GetComponent<AIAnimation> ().mAnimator = animator;
		mAIEntity.GetComponent<AIAnimation> ().Add ("Run",runAnim);
		mAIEntity.GetComponent<AIAnimation> ().Add ("Walk",walkAnim);
		mAIEntity.GetComponent<AIAnimation> ().Add ("Idle",idleAnim);
		mAIEntity.GetComponent<AIAnimation> ().Add ("Attack",shootAnim);
		mAIEntity.GetComponent<AIAnimation>().mtempAnim = "Run";
		mAIEntity.GetComponent<AIEmotion> ().InsertEmotion ("Happy");
		mAIEntity.GetComponent<AIEmotion> ().InsertEmotion ("Fear");
		mAIEntity.GetComponent<AIEmotion> ().InsertEmotion ("Tired");
		mAIEntity.GetComponent<AIEmotion> ().InsertEdge ("Happy",happy);
		mAIEntity.GetComponent<AIEmotion> ().InsertEdge ("Fear",fear);
		mAIEntity.GetComponent<AIEmotion> ().InsertEdge ("Tired",tired);



		AIState state_idle = new AIState ();
		state_idle.Init ();
		int idle_round=state_idle.AddExecuter (roundstate,EmptyExitAndEnter.EmptyExit,EmptyExitAndEnter.EmptyEnter);
		int idle_run=state_idle.AddExecuter (runstate,runExit,EmptyExitAndEnter.EmptyEnter);
		int idle_fire=state_idle.AddExecuter (firestate,fireExit,EmptyExitAndEnter.EmptyEnter);
		state_idle.AddDoubleEdge (roundtorun,runtoround,runIN,EmptyFeedbacker.Run,idle_round,idle_run);
		state_idle.AddDoubleEdge (runtofire,firetorun,hurtup,runIN,idle_run,idle_fire);
		state_idle.AddAnimation (roundstate,"Walk");
		state_idle.AddAnimation (runstate,"Run");
		state_idle.AddAnimation (firestate,"Attack");
		state_idle.tempID = idle_round;
		//state_idle.mName = "Idle";
		state_idle.mStateRecorder = mRecorder;
		state_idle.LastEntityData.AddComponent<HPComponent> (new HPComponent());
		state_idle.LastEntityData.PlayerEntity = mPlayerLast;
		int id2=mAIEntity.GetComponent<AIStrategy> ().AddStrategy (IdleActioner,EmptyStrategyEnter.Run,EmptyStrategyExit.Run,EmptyStrategyFeedbacker.Run,state_idle);

		AIState state_runaway = new AIState ();
		state_runaway.Init ();
		int idle_runaway=state_runaway.AddExecuter (runawaystate,EmptyExitAndEnter.EmptyExit,EmptyExitAndEnter.EmptyEnter);
		int idle_gre=state_runaway.AddExecuter (Grenatestate,GreExit,EmptyExitAndEnter.EmptyEnter);
		state_runaway.AddDoubleEdge (anytorunaway,Gre2Run,runOUT,runOUT,idle_gre,idle_runaway);
		state_runaway.AddAnimation (runawaystate,"Run");
		state_runaway.AddAnimation (Grenatestate,"Idle");
		state_runaway.tempID = idle_runaway;
		state_runaway.mStateRecorder = mRecorder;
		state_runaway.LastEntityData.AddComponent<HPComponent> (new HPComponent());
		state_runaway.LastEntityData.PlayerEntity = mPlayerLast;
		int id1=mAIEntity.GetComponent<AIStrategy> ().AddStrategy (RunAwayActioner,EmptyStrategyEnter.Run,EmptyStrategyExit.Run,EmptyStrategyFeedbacker.Run,state_runaway);
	//	mAIEntity.GetComponent<AIStrategy> ().InitAvoid (BattleState,EmptyExitAndEnter.EmptyEnter,EmptyExitAndEnter.EmptyExit,mRecorder,state_runaway.LastEntityData);
		mAIEntity.GetComponent<AIStrategy> ().SetEntry (id2);


		mPlayerLast.AddComponent<HPComponent> (playerLast_hp);
		mAIEntity.GetComponent<myAI> ().fireObject = shootObject;
		mAIEntity.GetComponent<myAI> ().GrenateObject = GrenateObject;
		mAIEntity.GetComponent<myAI>().mRoundCenter=player.transform.position + new Vector3 (Random.Range(-10,10),0,Random.Range(-10,10));
		player_hp.allHP = 700;
		player_hp.tempHP = 700;
		mPlayer.AddComponent<HPComponent> (player_hp);
	}

}
