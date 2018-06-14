using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FreedomAI;
using UnityECS;


public class TrapManDemo : MonoBehaviour
{
	
	public GameObject trapAI;
	public GameObject player;
	// 陷阱球子弹障碍物
	public GameObject trapBallObj;
	public GameObject bulletObj;
	public GameObject barrierObj;
	public GameObject mCanvas;
	//随机生成敌人的范围，默认为4
	public int randomRange = 4;
	// 巡逻圈的大小，默认为10
	public int roundRange = 10;

	public int count;

	Animator animator;
	AnimationPlay idleAnim;
	AnimationPlay walkAnim;
	AnimationPlay sneakAnim;
	AnimationPlay runAnim;
	AnimationPlay waryAnim;
	AnimationPlay aimAnim;
	AnimationPlay fireAnim;
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

	StateEnter waryEnter;
	StateExecuter waryState;
	StateExit waryExit;
	StateEnter barrierEnter;
	StateExecuter barrierState;
	StateExit barrierExit;
	StateEnter findcoverEnter;
	StateExecuter findcoverState;
	StateExit findcoverExit;
	StateEnter trapEnter;
	StateExecuter trapState;
	StateExit trapExit;
	StateEnter aimEnter;
	StateExecuter aimState;
	StateExit aimExit;
	StateEnter fireEnter;
	StateExecuter fireState;
	StateExit fireExit;
	StateEnter dieEnter;
	StateExecuter dieState;
	StateExit dieExit;

	StateTranfer roundtorest;
	StateTranfer resttoround;
	StateTranfer roundtovigilance;
	StateTranfer vigilancetoround;
	StateTranfer resttovigilance;
	StateTranfer vigilancetosearch;
	StateTranfer searchtoround;
	StateTranfer warytofindcover;
	StateTranfer findcovertowary;
	StateTranfer barriertofindcover;
	StateTranfer findcovertoaim;
	StateTranfer aimtofire;
	StateTranfer firetofindcover;
	StateTranfer findcovertotrap;
	StateTranfer traptofindcover;

	//之后再补充
	StateFeedbacker near;
	StateFeedbacker far;
	StateFeedbacker hurtup;

	EmotionExecuter despair;
	EmotionExecuter normal;
	EmotionExecuter angry;
	// 至此结束
	// 上层决策，策略动作机
	StrategyEnter IdleEnter;
	StrategyExit IdleExit;
	StrategyFeedbacker IdleFeedbacker;
	StrategyActioner IdleActioner;

	StrategyEnter AttackEnter;
	StrategyExit AttackExit;
	StrategyFeedbacker AttackFeedbacker;
	StrategyActioner AttackActioner;

	StrategyEnter EscapeEnter;
	StrategyExit EscapeExit;
	StrategyFeedbacker EscapeFeedbacker;
	StrategyActioner EscapeActioner;

	StrategyEnter BlockEnter;
	StrategyExit BlockExit;
	StrategyFeedbacker BlockFeedbacker;
	StrategyActioner BlockActioner;

	StrategyEnter DeathEnter;
	StrategyExit DeathExit;
	StrategyFeedbacker DeathFeedbacker;
	StrategyActioner DeathActioner;
	// Use this for initialization
	void Start ()
	{


		// 决策机
		IdleEnter = TrapMan.IdleEnter;
		IdleExit = TrapMan.IdleExit;
		IdleFeedbacker = TrapMan.IdleFeedbacker;
		IdleActioner = TrapMan.IdleAction;

		AttackEnter = TrapMan.AttackEnter;
		AttackExit = TrapMan.AttackExit;
		AttackFeedbacker = TrapMan.AttackFeedbacker;
		AttackActioner = TrapMan.AttackAction;

		EscapeEnter = TrapMan.EscapeEnter;
		EscapeExit = TrapMan.EscapeExit;
		EscapeFeedbacker = TrapMan.EscapeFeedbacker;
		EscapeActioner = TrapMan.EscapeAction;

		BlockEnter = TrapMan.BlockEnter;
		BlockExit = TrapMan.BlockExit;
		BlockFeedbacker = TrapMan.BlockFeedbacker;
		BlockActioner = TrapMan.BlockAction;

		DeathEnter = TrapMan.DeathEnter;
		DeathExit = TrapMan.DeathExit;
		DeathFeedbacker = TrapMan.DeathFeedbacker;
		DeathActioner = TrapMan.DeathAction;

		idleAnim = TrapMan.TM_idle_Anim;
		walkAnim = TrapMan.TM_Walk_Anim;
		sneakAnim = TrapMan.TM_Sneak_Anim;
		runAnim = TrapMan.TM_Run_Anim;
		waryAnim = TrapMan.TM_Wary_Anim;
		aimAnim = TrapMan.TM_Aim_Anim;
		fireAnim = TrapMan.TM_Fire_Anim;
		dieAnim = TrapMan.TM_Die_Anim;

		mRecorder = TrapMan.TM_Recorder;

		// 节点
		roundEnter = TrapMan.TM_Round_Enter;
		roundState = TrapMan.TM_Round;
		roundExit = TrapMan.TM_Round_Exit;
		restEnter = TrapMan.TM_Rest_Enter;
		restState = TrapMan.TM_Rest;
		restExit = TrapMan.TM_Rest_Exit;
		vigilanceEnter = TrapMan.TM_Vigilance_Enter;
		vigilanceState = TrapMan.TM_Vigilance;
		vigilanceExit = TrapMan.TM_Vigilance_Exit;
		searchEnter = TrapMan.TM_Search_Enter;
		searchState = TrapMan.TM_Search;
		searchExit = TrapMan.TM_Search_Exit;

		waryEnter = TrapMan.TM_Wary_Enter;
		waryState = TrapMan.TM_Wary;
		waryExit = TrapMan.TM_Wary_Exit;
		findcoverEnter = TrapMan.TM_Findcover_Enter;
		findcoverState = TrapMan.TM_Findcover;
		findcoverExit = TrapMan.TM_Findcover_Exit;
		aimEnter = TrapMan.TM_Aim_Enter;
		aimState = TrapMan.TM_Aim;
		aimExit = TrapMan.TM_Aim_Exit;
		fireEnter = TrapMan.TM_Fire_Enter;
		fireState = TrapMan.TM_Fire;
		fireExit = TrapMan.TM_Fire_Exit;
		trapEnter = TrapMan.TM_Trap_Enter;
		trapState = TrapMan.TM_Trap;
		trapExit = TrapMan.TM_Trap_Exit;
		barrierEnter = TrapMan.TM_Barrier_Enter;
		barrierState = TrapMan.TM_Barrier;
		barrierExit = TrapMan.TM_Barrier_Exit;
		dieEnter = TrapMan.TM_Die_Enter;
		dieState = TrapMan.TM_Die;
		dieExit = TrapMan.TM_Die_Exit;

		// 转换
		roundtorest = TrapMan.TM_Round_Rest;
		resttoround = TrapMan.TM_Rest_Round;
		roundtovigilance = TrapMan.TM_Rest_Vigilance;
		vigilancetoround = TrapMan.TM_Vigilance_Round;
		resttovigilance = TrapMan.TM_Rest_Vigilance;
		vigilancetosearch = TrapMan.TM_Vigilance_Search;
		searchtoround = TrapMan.TM_Search_Round;


		warytofindcover = TrapMan.TM_Wary_Findcover;
		findcovertowary = TrapMan.TM_Findcover_Wary;
		barriertofindcover = TrapMan.TM_Barrier_Findcover;
		findcovertoaim = TrapMan.TM_Findcover_Aim;
		aimtofire = TrapMan.TM_Aim_Fire;
		firetofindcover = TrapMan.TM_Fire_Findcover;
		findcovertotrap = TrapMan.TM_Findcover_Trap;
		traptofindcover = TrapMan.TM_Trap_Findcover;

		// 决策反馈函数
		near = TrapMan.TM_Near;
		far = TrapMan.TM_Far;
		hurtup = TrapMan.TM_HurtUp;

		despair = TrapMan.TM_Despair_Emotion;
		normal = TrapMan.TM_Normal_Emotion;
		angry = TrapMan.TM_Angry_Emotion;

		for (int i = 0; i < count; i++) {
			AIEntity mAIEntity = new AIEntity ();
			UEntity mPlayer = new UEntity ();
			UEntity mPlayerLast = new UEntity ();

			ECSWorld.MainWorld.registerEntityAfterInit (mAIEntity);

			ECSWorld.MainWorld.registerEntityAfterInit (mPlayer);

			mAIEntity.mAI = trapAI;
			mAIEntity.mPlayer = player;
			mAIEntity.PlayerEntity = mPlayer;
			mAIEntity.AIPos = new Vector3 (gameObject.transform.position.x + Random.Range (-randomRange, randomRange), 0, 
				gameObject.transform.position.z + Random.Range (-randomRange, randomRange));

			mAIEntity.Init ();

			mAIEntity.AddComponent<trapAI> (new trapAI ());
			mAIEntity.GetComponent<trapAI> ().mAStarComputerformula = new AStarComputerformula ();
			mAIEntity.GetComponent<trapAI> ().mAStarComputerformula.Add (DefaultFunc.AStarByEulerDistance, 1.0f);
			mAIEntity.AddComponent<trapComponent> (new trapComponent ());
			mAIEntity.AddComponent<getPlayer> (new getPlayer ());
			mAIEntity.AddComponent<GYTHPComponent> (new GYTHPComponent ());
			mAIEntity.AddComponent<showAll> (new showAll ());

			mAIEntity.GetComponent<BaseAIComponent> ().mAIRT.SetActive (true);

			mAIEntity.GetComponent<GYTHPComponent> ().HP = mAIEntity.GetComponent<trapAI> ().HP;
			mAIEntity.GetComponent<GYTHPComponent> ().HPNow = mAIEntity.GetComponent<trapAI> ().HP;

			animator = mAIEntity.GetComponent<BaseAIComponent> ().mAIRT.GetComponent<Animator> ();
			mAIEntity.GetComponent<AIAnimation> ().mAnimator = animator;

			mAIEntity.GetComponent<AIAnimation> ().Add ("Idle", idleAnim);
			mAIEntity.GetComponent<AIAnimation> ().Add ("Walk", walkAnim);
			mAIEntity.GetComponent<AIAnimation> ().Add ("Sneak", sneakAnim);
			mAIEntity.GetComponent<AIAnimation> ().Add ("Run", runAnim);
			mAIEntity.GetComponent<AIAnimation> ().Add ("Wary", waryAnim);
			mAIEntity.GetComponent<AIAnimation> ().Add ("Aim", aimAnim);
			mAIEntity.GetComponent<AIAnimation> ().Add ("Fire", fireAnim);
			mAIEntity.GetComponent<AIAnimation> ().Add ("Die", dieAnim);
			mAIEntity.GetComponent<AIAnimation> ().mtempAnim = "Idle";

			mAIEntity.GetComponent<AIEmotion> ().InsertEmotion ("Despair");
			mAIEntity.GetComponent<AIEmotion> ().InsertEmotion ("Normal");
			mAIEntity.GetComponent<AIEmotion> ().InsertEmotion ("Angry");

			mAIEntity.GetComponent<AIEmotion> ().InsertEdge ("Despair", despair);
			mAIEntity.GetComponent<AIEmotion> ().InsertEdge ("Normal", normal);
			mAIEntity.GetComponent<AIEmotion> ().InsertEdge ("Angry", angry);
		

			// 巡逻决策
			AIState state_idle = new AIState ();
			state_idle.Init ();
			state_idle.mName = "TMidle";
			state_idle.mStateRecorder = mRecorder;
			state_idle.LastEntityData.AddComponent<SMREComponent> (new SMREComponent ());

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
			state_idle.AddEdge (vigilancetosearch, near, idle_vigilance, idle_search);
			state_idle.AddEdge (searchtoround, EmptyFeedbacker.Run, idle_search, idle_round);

			state_idle.AddAnimation (roundState, "Walk");
			state_idle.AddAnimation (restState, "Wary");
			state_idle.AddAnimation (vigilanceState, "Idle");
			state_idle.AddAnimation (searchState, "Walk");

			state_idle.tempID = idle_round;

			int id1 = mAIEntity.GetComponent<AIStrategy> ().AddStrategy (IdleActioner, IdleEnter, IdleExit, IdleFeedbacker, state_idle);
			mAIEntity.GetComponent<AIStrategy> ().AddName (id1, "Idle");

			// 射击，陷阱
			AIState state_attack = new AIState ();
			state_attack.Init ();
			state_attack.mName = "TMattack";
			state_attack.mStateRecorder = mRecorder;
			state_attack.LastEntityData.AddComponent<SMREComponent> (new SMREComponent ());

			int attack_wary = state_attack.AddExecuter (waryState, waryExit, waryEnter);
			int attack_barrier = state_attack.AddExecuter (barrierState, barrierExit, barrierEnter);
			int attack_findcover = state_attack.AddExecuter (findcoverState, findcoverExit, findcoverEnter);
			int attack_trap = state_attack.AddExecuter (trapState, trapExit, trapEnter);
			int attack_aim = state_attack.AddExecuter (aimState, aimExit, aimEnter);
			int attack_fire = state_attack.AddExecuter (fireState, fireExit, fireEnter);

			state_attack.AddStateName (attack_wary, "Wary");
			state_attack.AddStateName (attack_barrier, "Barrier");
			state_attack.AddStateName (attack_findcover, "Findcover");
			state_attack.AddStateName (attack_trap, "Trap");
			state_attack.AddStateName (attack_aim, "Aim");
			state_attack.AddStateName (attack_fire, "Fire");

			state_attack.AddEdge (barriertofindcover, near, attack_barrier, attack_findcover);
			// 玩家接近的时候，寻找掩体价值更高;玩家远离时，可以休息一下
			state_attack.AddDoubleEdge (warytofindcover, findcovertowary, near, far, attack_wary, attack_findcover);
			state_attack.AddEdge (findcovertoaim, far, attack_findcover, attack_aim);
			state_attack.AddEdge (aimtofire, hurtup, attack_aim, attack_fire);
			state_attack.AddEdge (firetofindcover, near, attack_fire, attack_findcover);
			state_attack.AddDoubleEdge (findcovertotrap, traptofindcover, EmptyFeedbacker.Run, near, attack_findcover, attack_trap);

			state_attack.AddAnimation (waryState, "Wary");
			state_attack.AddAnimation (barrierState, "Idle");
			state_attack.AddAnimation (findcoverState, "Run");
			state_attack.AddAnimation (trapState, "Wary");
			state_attack.AddAnimation (aimState, "Aim");
			state_attack.AddAnimation (fireState, "Fire");

			state_attack.tempID = attack_barrier;// 造掩体真他么开心啊

			int id2 = mAIEntity.GetComponent<AIStrategy> ().AddStrategy (AttackActioner, AttackEnter, AttackExit, AttackFeedbacker, state_attack);
			mAIEntity.GetComponent<AIStrategy> ().AddName (id2, "Attack");


			// 逃跑决策
			AIState state_escape = new AIState ();
			state_escape.Init ();
			state_escape.mName = "TMescape";
			state_escape.mStateRecorder = mRecorder;
			state_escape.LastEntityData.AddComponent<SMREComponent> (new SMREComponent ());

			int escape_findcover = state_escape.AddExecuter (findcoverState, findcoverExit, findcoverEnter);
			int escape_trap = state_escape.AddExecuter (trapState, trapExit, trapEnter);
			int escape_wary = state_escape.AddExecuter (waryState, waryExit, waryEnter);

			state_escape.AddStateName (escape_findcover, "Findcover");
			state_escape.AddStateName (escape_trap, "Trap");
			state_escape.AddStateName (escape_wary, "Wary");

			state_escape.AddDoubleEdge (warytofindcover, findcovertowary, near, far, attack_wary, attack_findcover);
			state_escape.AddDoubleEdge (findcovertotrap, traptofindcover, EmptyFeedbacker.Run, EmptyFeedbacker.Run, attack_findcover, attack_trap);

			state_escape.AddAnimation (waryState, "Wary");
			state_escape.AddAnimation (trapState, "Idle");
			state_escape.AddAnimation (findcoverState, "Run");

			state_escape.tempID = escape_findcover;


			int id3 = mAIEntity.GetComponent<AIStrategy> ().AddStrategy (EscapeActioner, EscapeEnter, EscapeExit, EscapeFeedbacker, state_escape);
			mAIEntity.GetComponent<AIStrategy> ().AddName (id3, "Escape");
			// 死亡决策
			AIState state_death = new AIState ();
			state_death.Init ();
			state_death.mName = "TMdeath";
			state_death.mStateRecorder = mRecorder;
			state_death.LastEntityData.AddComponent<SMREComponent> (new SMREComponent ());

			int death_die = state_death.AddExecuter (dieState, dieExit, dieEnter);
			state_death.AddStateName (death_die, "Die");

			state_death.AddAnimation (dieState, "Die");

			state_death.tempID = death_die;

			int id6 = mAIEntity.GetComponent<AIStrategy> ().AddStrategy (DeathActioner, DeathEnter, DeathExit, DeathFeedbacker, state_death);
			mAIEntity.GetComponent<AIStrategy> ().AddName (id6, "Death");


			mAIEntity.GetComponent<AIStrategy> ().SetEntry (id1);

			mAIEntity.GetComponent<trapAI> ().mRoundCenter = player.transform.position +
			new Vector3 (Random.Range (-roundRange, roundRange), 0, Random.Range (-roundRange, roundRange));


			mAIEntity.GetComponent<trapAI> ().trapBallObj = trapBallObj;
			mAIEntity.GetComponent<trapAI> ().bulletObj = bulletObj;
			mAIEntity.GetComponent<trapAI> ().barrierObj = barrierObj;

			GameObject rt = mAIEntity.GetComponent<BaseAIComponent> ().mAIRT;
			// 寻找，并赋值警戒圆盘和射击位置
			foreach (Transform t in rt.GetComponentsInChildren<Transform>()) {
				if (t.name == "vs") {
					mAIEntity.GetComponent<getPlayer> ().vs = t.gameObject;
				} else if (t.name == "FirePos") {
					mAIEntity.GetComponent<trapAI> ().shootPoint = t.gameObject;
				} else if (t.name == "RigHead") {
					mAIEntity.GetComponent<trapAI> ().RigHead = t.gameObject;
				}
			}
			// 组行为，一般都是发起者
			mAIEntity.tag = "TrapMan";
			if (i % 2 == 0) {
				mAIEntity.AddComponent<Group4StandardAI> (new Group4StandardAI ());
				GroupManager.getInstance ().AddSponsor (mAIEntity);
			} else {
				GroupManager.getInstance ().AddResponse (mAIEntity);
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
