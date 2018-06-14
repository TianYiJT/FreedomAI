using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FreedomAI;
using UnityECS;

public class BombManDemo : MonoBehaviour
{

	public GameObject bombAI;
	public GameObject player;
	public GameObject mCanvas;
	public int randomRange = 4;
//随机生成敌人的范围，默认为4
	public int roundRange = 10;
// 巡逻圈的大小，默认为10

	public int count;

	Animator animator;
	AnimationPlay idleAnim;
	AnimationPlay restAnim;
	AnimationPlay walkAnim;
	AnimationPlay vigilanceAnim;
	AnimationPlay crazyrunAnim;
	AnimationPlay readyblowAnim;
	AnimationPlay dieAnim;

	StateRecorder mRecorder;

	StateEnter roundEnter;
	StateExecuter roundState;
	StateExit roundExit;
	StateEnter restEnter;
	StateExecuter restState;
	StateExit restExit;
	StateEnter vigilanceEnter;
	StateExecuter vigilanceState;
	StateExit vigilanceExit;
	StateEnter searchEnter;
	StateExecuter searchState;
	StateExit searchExit;
	StateEnter crazyrunEnter;
	StateExecuter crazyrunState;
	StateExit crazyrunExit;
	StateEnter delayEnter;
	StateExecuter delayState;
	StateExit delayExit;
	StateEnter boomEnter;
	StateExecuter boomState;
	StateExit boomExit;

	StateTranfer roundtorest;
	StateTranfer resttoround;
	StateTranfer roundtovigilance;
	StateTranfer vigilancetoround;
	StateTranfer resttovigilance;
	StateTranfer vigilancetosearch;
	StateTranfer searchtoround;
	StateTranfer crazyruntodelay;

	// 自爆兵不需要反馈函数

	EmotionExecuter angry;
	EmotionExecuter normal;

	StrategyEnter IdleEnter;
	StrategyExit IdleExit;
	StrategyFeedbacker IdleFeedbacker;
	StrategyActioner IdleActioner;

	StrategyEnter DoomEnter;
	StrategyExit DoomExit;
	StrategyFeedbacker DoomFeedbacker;
	StrategyActioner DoomActioner;

	StrategyEnter DeathEnter;
	StrategyExit DeathExit;
	StrategyFeedbacker DeathFeedbacker;
	StrategyActioner DeathActioner;

	// Use this for initialization
	void Start ()
	{
		// 决策机
		IdleEnter = BombMan.IdleEnter;
		IdleExit = BombMan.IdleExit;
		IdleFeedbacker = BombMan.IdleFeedbacker;
		IdleActioner = BombMan.IdleAction;

		DoomEnter = BombMan.DoomEnter;
		DoomExit = BombMan.DoomExit;
		DoomFeedbacker = BombMan.DoomFeedbacker;
		DoomActioner = BombMan.DoomAction;

		DeathEnter = BombMan.DeathEnter;
		DeathExit = BombMan.DeathExit;
		DeathFeedbacker = BombMan.DeathFeedbacker;
		DeathActioner = BombMan.DeathAction;

		// 动画
		idleAnim = BombMan.BM_idle_Anim;
		restAnim = BombMan.BM_rest_Anim;
		walkAnim = BombMan.BM_walk_Anim;
		vigilanceAnim = BombMan.BM_vigilance_Anim;
		crazyrunAnim = BombMan.BM_crazyrun_Anim;
		readyblowAnim = BombMan.BM_readyblow_Anim;
		dieAnim = BombMan.BM_die_Anim;

		mRecorder = BombMan.BM_Recorder;

		roundEnter = BombMan.BM_Round_Enter;
		roundState = BombMan.BM_Round;
		roundExit = BombMan.BM_Round_Exit;
		restEnter = BombMan.BM_Rest_Enter;
		restState = BombMan.BM_Rest;
		restExit = BombMan.BM_Rest_Exit;
		vigilanceEnter = BombMan.BM_Vigilance_Enter;
		vigilanceState = BombMan.BM_Vigilance;
		vigilanceExit = BombMan.BM_Vigilance_Exit;
		searchEnter = BombMan.BM_Search_Enter;
		searchState = BombMan.BM_Search;
		searchExit = BombMan.BM_Search_Exit;
		crazyrunEnter = BombMan.BM_Crazyrun_Enter;
		crazyrunState = BombMan.BM_Crazyrun;
		crazyrunExit = BombMan.BM_Crazyrun_Exit;
		delayEnter = BombMan.BM_Delay_Enter;
		delayState = BombMan.BM_Delay;
		delayExit = BombMan.BM_Delay_Exit;
		boomEnter = BombMan.BM_Boom_Enter;
		boomState = BombMan.BM_Boom;
		boomExit = BombMan.BM_Boom_Exit;

		roundtorest = BombMan.BM_Round_Rest;
		resttoround = BombMan.BM_Rest_Round;
		roundtovigilance = BombMan.BM_Rest_Vigilance;
		vigilancetoround = BombMan.BM_Vigilance_Round;
		resttovigilance = BombMan.BM_Rest_Vigilance;
		vigilancetosearch = BombMan.BM_Vigilance_Search;
		searchtoround = BombMan.BM_Search_Round;
		crazyruntodelay = BombMan.BM_Crazyrun_Delay;

		// 情感
		normal = BombMan.BM_Normal_Emotion;
		angry = BombMan.BM_Angry_Emotion;

		// 生成指定个数单位
		for (int i = 0; i < count; i++) {
			AIEntity mAIEntity = new AIEntity ();
			UEntity mPlayer = new UEntity ();
			UEntity mPlayerLast = new UEntity ();

			ECSWorld.MainWorld.registerEntityAfterInit (mAIEntity);

			ECSWorld.MainWorld.registerEntityAfterInit (mPlayer);

			// 在init前，完成这三个初始化
			mAIEntity.mAI = bombAI;
			mAIEntity.mPlayer = player;
			mAIEntity.PlayerEntity = mPlayer;

			mAIEntity.AIPos = new Vector3 (
				gameObject.transform.position.x + Random.Range (-randomRange, randomRange), 0, 
				gameObject.transform.position.z + Random.Range (-randomRange, randomRange));
			mAIEntity.Init ();

			mAIEntity.AddComponent<bombAI> (new bombAI ());
			mAIEntity.AddComponent<getPlayer> (new getPlayer ());
			mAIEntity.AddComponent<showAll> (new showAll ());

			mAIEntity.GetComponent<BaseAIComponent> ().mAIRT.SetActive (true);

			animator = mAIEntity.GetComponent<BaseAIComponent> ().mAIRT.GetComponent<Animator> ();
			mAIEntity.GetComponent<AIAnimation> ().mAnimator = animator;

			mAIEntity.GetComponent<AIAnimation> ().Add ("Idle", idleAnim);
			mAIEntity.GetComponent<AIAnimation> ().Add ("Rest", restAnim);
			mAIEntity.GetComponent<AIAnimation> ().Add ("Walk", walkAnim);
			mAIEntity.GetComponent<AIAnimation> ().Add ("Vigilance", vigilanceAnim);
			mAIEntity.GetComponent<AIAnimation> ().Add ("Crazyrun", crazyrunAnim);
			mAIEntity.GetComponent<AIAnimation> ().Add ("Readyblow", readyblowAnim);
			mAIEntity.GetComponent<AIAnimation> ().Add ("Die", dieAnim);

			mAIEntity.GetComponent<AIAnimation> ().mtempAnim = "Idle";

			mAIEntity.GetComponent<AIEmotion> ().InsertEmotion ("Normal");
			mAIEntity.GetComponent<AIEmotion> ().InsertEmotion ("Angry");

			mAIEntity.GetComponent<AIEmotion> ().InsertEdge ("Normal", normal);
			mAIEntity.GetComponent<AIEmotion> ().InsertEdge ("Angry", angry);

			// 上段完

			// 巡逻决策
			AIState state_idle = new AIState ();
			state_idle.Init ();
			state_idle.mStateRecorder = EmptyExitAndEnter.EmptyEnter;

			int idle_round = state_idle.AddExecuter (roundState, roundExit, roundEnter);
			int idle_rest = state_idle.AddExecuter (restState, restExit, restEnter);
			int idle_vigilance = state_idle.AddExecuter (vigilanceState, vigilanceExit, vigilanceEnter);
			int idle_search = state_idle.AddExecuter (searchState, searchExit, searchEnter);
			
			state_idle.AddStateName (idle_round, "Round");
			state_idle.AddStateName (idle_rest, "Rest");
			state_idle.AddStateName (idle_vigilance, "Vigilance");
			state_idle.AddStateName (idle_search, "Search");

			state_idle.AddDoubleEdge (roundtorest, resttoround, EmptyFeedbacker.Run, 
				EmptyFeedbacker.Run, idle_round, idle_rest);
			state_idle.AddDoubleEdge (roundtovigilance, vigilancetoround, EmptyFeedbacker.Run,
				EmptyFeedbacker.Run, idle_round, idle_vigilance);
			state_idle.AddEdge (resttovigilance, EmptyFeedbacker.Run, idle_rest, idle_vigilance);
			state_idle.AddEdge (vigilancetosearch, EmptyFeedbacker.Run, idle_vigilance, idle_search);
			state_idle.AddEdge (searchtoround, EmptyFeedbacker.Run, idle_search, idle_round);

			state_idle.AddAnimation (roundState, "Walk");
			state_idle.AddAnimation (restState, "Rest");
			state_idle.AddAnimation (vigilanceState, "Vigilance");
			state_idle.AddAnimation (searchState, "Walk");

			state_idle.tempID = idle_round;

			int id1 = mAIEntity.GetComponent<AIStrategy> ().AddStrategy (IdleActioner, IdleEnter, IdleExit, IdleFeedbacker, state_idle);
			mAIEntity.GetComponent<AIStrategy> ().AddName (id1, "Idle");


			// 自爆决策
			AIState state_doom = new AIState ();
			state_doom.Init ();
			state_doom.mStateRecorder = EmptyExitAndEnter.EmptyEnter;

			int doom_crazyrun = state_doom.AddExecuter (crazyrunState, crazyrunExit, crazyrunEnter);
			int doom_delay = state_doom.AddExecuter (delayState, delayExit, delayEnter);

			state_doom.AddStateName (doom_crazyrun, "CrazyRun");
			state_doom.AddStateName (doom_delay, "Delay");

			state_doom.AddEdge (crazyruntodelay, EmptyFeedbacker.Run, doom_crazyrun, doom_delay);

			state_doom.AddAnimation (crazyrunState, "Crazyrun");
			state_doom.AddAnimation (delayState, "Readyblow");

			state_doom.tempID = doom_crazyrun;

			int id2 = mAIEntity.GetComponent<AIStrategy> ().AddStrategy (DoomActioner, DoomEnter, DoomExit, DoomFeedbacker, state_doom);
			mAIEntity.GetComponent<AIStrategy> ().AddName (id2, "Doom");

			// 死亡决策
			AIState state_death = new AIState ();
			state_death.Init ();
			state_death.mStateRecorder = EmptyExitAndEnter.EmptyEnter;

			int death_boom = state_death.AddExecuter (boomState, boomExit, boomEnter);
			state_death.AddStateName (death_boom, "Boom");

			state_death.AddAnimation (boomState, "Die");

			state_death.tempID = death_boom;

			int id3 = mAIEntity.GetComponent<AIStrategy> ().AddStrategy (DeathActioner, DeathEnter, DeathExit, DeathFeedbacker, state_death);
			mAIEntity.GetComponent<AIStrategy> ().AddName (id3, "Death");

			mAIEntity.GetComponent<AIStrategy> ().SetEntry (id1);


			mAIEntity.GetComponent<bombAI> ().mRoundCenter = gameObject.transform.position +
			new Vector3 (Random.Range (-roundRange, roundRange), 0, 
				Random.Range (-roundRange, roundRange));
			
			GameObject rt = mAIEntity.GetComponent<BaseAIComponent> ().mAIRT;

			foreach (Transform t in rt.GetComponentsInChildren<Transform>()) {
				if (t.name == "vs") {
					mAIEntity.GetComponent<getPlayer> ().vs = t.gameObject;
				} else if (t.name == "RigHead") {
					mAIEntity.GetComponent<bombAI> ().RigHead = t.gameObject;
				}
			}

			mAIEntity.GetComponent<showAll> ().mCanvas = mCanvas;
			if (count == 1)
				mAIEntity.GetComponent<showAll> ().singleTest = true;
			foreach (Transform t in mCanvas.GetComponentsInChildren<Transform>()) {
				if (t.name == "Action") {
					mAIEntity.GetComponent<showAll> ().mAction = t.gameObject;
				} else if (t.name == "Node") {
					mAIEntity.GetComponent<showAll> ().mNode = t.gameObject;
				} else if (t.name == "Emotion") {
					mAIEntity.GetComponent<showAll> ().mEmotion = t.gameObject;
				} else if (t.name == "Animation") {
					mAIEntity.GetComponent<showAll> ().mAnimation = t.gameObject;
				}
			}

			mAIEntity.tag = "BombMan";
			GroupManager.getInstance ().AddResponse (mAIEntity);

		}



	}
	
	// Update is called once per frame
	void Update ()
	{
	}

	void OnDrawGizmos ()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere (transform.position, 0.5f);
	}
}
