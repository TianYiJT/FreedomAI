using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FreedomAI;
using UnityECS;


public class FSMDemo : MonoBehaviour 
{
	public int count;
	public GameObject AI;
	public GameObject player;


	public StateTranfer yali;

	Animator animator;

	AnimationPlay walkAnim;
	AnimationPlay idleAnim;

	StateRecorder mRecorder;
	StateExecuter Idlestate;
	StateExecuter roundstate;
	StateExit roundExit;

	public GameObject[] initpos;

	public bool isUseLOD;



	void Start () 
	{

		walkAnim = TestFSM.FSM_Walk_Anim;
		idleAnim = TestFSM.FSM_Idle_Anim;

		mRecorder = TestFSM.FSM_Test_Recorder;

		roundstate = TestFSM.FSM_Round;
		Idlestate = TestFSM.FSM_Idle;
		roundExit = TestFSM.FSM_Run_Exit;
		yali = yaliFSM.yali_Transfer;


		for (int i = 0; i < count; i++)
		{
			HPComponent player_hp = new HPComponent();
			AIEntity mAIEntity = new AIEntity();
			UEntity mPlayer = new UEntity();
			UEntity mPlayerLast = new UEntity ();
			HPComponent playerLast_hp = new HPComponent ();

			ECSWorld.MainWorld.registerEntityAfterInit (mAIEntity);

			ECSWorld.MainWorld.registerEntityAfterInit (mPlayer);

			mAIEntity.mAI = AI;
			mAIEntity.mPlayer = player;
			mAIEntity.AIPos = initpos [Random.Range (0, initpos.Length)].transform.position;
			mAIEntity.Init ();
			mAIEntity.GetComponent<LODComponent> ().isUse = isUseLOD;
			mAIEntity.AddComponent<myAI> (new myAI());
			mAIEntity.GetComponent<myAI> ().mRoundCenter = mAIEntity.AIPos;
			mAIEntity.GetComponent<BaseAIComponent> ().mAIRT.SetActive (true);
			mAIEntity.PlayerEntity = mPlayer;
			mAIEntity.GetComponent<HPComponent> ().allHP = 500;
			mAIEntity.GetComponent<HPComponent> ().tempHP = 500;
			animator = mAIEntity.GetComponent<BaseAIComponent> ().mAIRT.GetComponent<Animator> ();
			mAIEntity.GetComponent<AIAnimation> ().mAnimator = animator;

			mAIEntity.GetComponent<AIAnimation> ().Add ("Walk",walkAnim);
			mAIEntity.GetComponent<AIAnimation> ().Add ("Idle",idleAnim);

			int id_round=mAIEntity.GetComponent<AIState> ().AddExecuter (roundstate,roundExit,EmptyExitAndEnter.EmptyEnter);

			int id_idle = mAIEntity.GetComponent<AIState> ().AddExecuter (Idlestate,EmptyExitAndEnter.EmptyExit,EmptyExitAndEnter.EmptyEnter);

			mAIEntity.GetComponent<AIState> ().AddEdge (yali,EmptyFeedbacker.Run,id_round,id_idle);

			mAIEntity.GetComponent<AIState> ().AddEdge (yali,EmptyFeedbacker.Run,id_idle,id_round);

	
			mAIEntity.GetComponent<AIState> ().AddAnimation (roundstate,"Walk");

			mAIEntity.GetComponent<AIState> ().AddAnimation (Idlestate,"Idle");

			mAIEntity.GetComponent<AIState> ().tempID = id_round;
			mAIEntity.GetComponent<AIState> ().mStateRecorder = mRecorder;
			mAIEntity.GetComponent<AIState> ().LastEntityData.AddComponent<HPComponent> (new HPComponent());
			mAIEntity.GetComponent<AIState> ().LastEntityData.PlayerEntity = mPlayerLast;
			mPlayerLast.AddComponent<HPComponent> (playerLast_hp);
			player_hp.allHP = 700;
			player_hp.tempHP = 700;
			mPlayer.AddComponent<HPComponent> (player_hp);
		}

	}
	
	// Update is called once per frame
	void Update ()
	{
		List<AIEntity> tList = AIEntity.getList ();
		foreach (AIEntity ai in tList)
		{
			ai.Log ();
		}
	}
}
