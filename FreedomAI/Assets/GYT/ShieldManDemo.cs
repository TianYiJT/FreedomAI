using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FreedomAI;
using UnityECS;


public class ShieldManDemo : MonoBehaviour
{
	public GameObject shieldAI;
	public GameObject player;
	public GameObject hookThrow;
	public GameObject mCanvas;
	public int randomRange = 4;
	//随机生成敌人的范围，默认为4
	public int roundRange = 10;
	// 巡逻圈的大小，默认为10
	// 由于钩子和冲锋走的一个线路，所以，暂时奇数为钩子，偶数为冲锋
	public int count;

	Animator animator;
	AnimationPlay idleAnim;
	AnimationPlay walkAnim;
	AnimationPlay runAnim;
	AnimationPlay shieldWalkAnim;
	AnimationPlay shieldWalkBAnim;
	AnimationPlay shieldStopAnim;
	AnimationPlay dieAnim;
	AnimationPlay hookAnim;
	AnimationPlay chargeAnim;
	AnimationPlay attackAnim;
	AnimationPlay viglianceAnim;

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
	StateEnter runEnter;
	StateExecuter runState;
	StateExit runExit;
	StateEnter chargeEnter;
	StateExecuter chargeState;
	StateExit chargeExit;
	StateEnter hookEnter;
	StateExecuter hookState;
	StateExit hookExit;
	StateEnter attackEnter;
	StateExecuter attackState;
	StateExit attackExit;
	StateEnter runawayEnter;
	StateExecuter runawayState;
	StateExit runawayExit;
	StateEnter walkawayEnter;
	StateExecuter walkawayState;
	StateExit walkawayExit;
	StateEnter chaseEnter;
	StateExecuter chaseState;
	StateExit chaseExit;
	StateEnter shieldFEnter;
	StateExecuter shieldFState;
	StateExit shieldFExit;
	StateEnter shieldSEnter;
	StateExecuter shieldSState;
	StateExit shieldSExit;
	StateEnter shieldBEnter;
	StateExecuter shieldBState;
	StateExit shieldBExit;
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
	StateTranfer chasetoshieldF;
	StateTranfer shieldFtochase;
	StateTranfer anytohook;
	StateTranfer anytocharge;
	StateTranfer chargetoshieldF;
	StateTranfer hooktoshieldF;
	StateTranfer shieldStoattack;
	StateTranfer attacktoshieldS;
	StateTranfer shieldFtoshieldS;
	StateTranfer shieldStoshieldF;
	StateTranfer shieldBtowalkaway;
	StateTranfer walkawaytoshieldB;
	StateTranfer walkawaytorunaway;
	StateTranfer runawaytowalkaway;

	StateFeedbacker near;
	StateFeedbacker far;
	StateFeedbacker hurtup;
	StateFeedbacker staLose;
	StateFeedbacker staRecover;

	EmotionExecuter despair;
	EmotionExecuter normal;
	EmotionExecuter angry;
	EmotionExecuter excited;


	// 上层决策，策略动作机
	StrategyEnter IdleEnter;
	StrategyExit IdleExit;
	StrategyFeedbacker IdleFeedbacker;
	StrategyActioner IdleActioner;

	StrategyEnter ApproachEnter;
	StrategyExit ApproachExit;
	StrategyFeedbacker ApproachFeedbacker;
	StrategyActioner ApproachActioner;

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
		// 决策初始化
		IdleEnter = ShieldMan.IdleEnter;
		IdleExit = ShieldMan.IdleExit;
		IdleFeedbacker = ShieldMan.IdleFeedbacker;
		IdleActioner = ShieldMan.IdleAction;

		ApproachEnter = ShieldMan.ApproachEnter;
		ApproachExit = ShieldMan.ApproachExit;
		ApproachFeedbacker = ShieldMan.ApproachFeedbacker;
		ApproachActioner = ShieldMan.ApproachAction;

		AttackEnter = ShieldMan.AttackEnter;
		AttackExit = ShieldMan.AttackExit;
		AttackFeedbacker = ShieldMan.AttackFeedbacker;
		AttackActioner = ShieldMan.AttackAction;

		EscapeEnter = ShieldMan.EscapeEnter;
		EscapeExit = ShieldMan.EscapeExit;
		EscapeFeedbacker = ShieldMan.EscapeFeedbacker;
		EscapeActioner = ShieldMan.EscapeAction;

		BlockEnter = ShieldMan.BlockEnter;
		BlockExit = ShieldMan.BlockExit;
		BlockFeedbacker = ShieldMan.BlockFeedbacker;
		BlockActioner = ShieldMan.BlockAction;

		DeathEnter = ShieldMan.DeathEnter;
		DeathExit = ShieldMan.DeathExit;
		DeathFeedbacker = ShieldMan.DeathFeedbacker;
		DeathActioner = ShieldMan.DeathAction;

		// 动画
		idleAnim = ShieldMan.SM_idle_Anim;
		walkAnim = ShieldMan.SM_walk_Anim;
		runAnim = ShieldMan.SM_run_Anim;
		shieldWalkAnim = ShieldMan.SM_shield_walk_Anim;
		shieldWalkBAnim = ShieldMan.SM_shield_walkB_Anim;
		shieldStopAnim = ShieldMan.SM_shield_stop_Anim;
		dieAnim = ShieldMan.SM_die_Anim;
		hookAnim = ShieldMan.SM_hook_Anim;
		chargeAnim = ShieldMan.SM_charge_Anim;
		attackAnim = ShieldMan.SM_attack_Anim;
		viglianceAnim = ShieldMan.SM_vigilance_Anim;

		mRecorder = ShieldMan.SM_Recorder;

		// 节点
		roundEnter = ShieldMan.SM_Round_Enter;
		roundState = ShieldMan.SM_Round;
		roundExit = ShieldMan.SM_Round_Exit;
		restEnter = ShieldMan.SM_Rest_Enter;
		restState = ShieldMan.SM_Rest;
		restExit = ShieldMan.SM_Rest_Exit;
		vigilanceEnter = ShieldMan.SM_Vigilance_Enter;
		vigilanceState = ShieldMan.SM_Vigilance;
		vigilanceExit = ShieldMan.SM_Vigilance_Exit;
		searchEnter = ShieldMan.SM_Search_Enter;
		searchState = ShieldMan.SM_Search;
		searchExit = ShieldMan.SM_Search_Exit;

		chargeEnter = ShieldMan.SM_Charge_Enter;
		chargeState = ShieldMan.SM_Charge;
		chargeExit = ShieldMan.SM_Charge_Exit;
		hookEnter = ShieldMan.SM_Hook_Enter;
		hookState = ShieldMan.SM_Hook;
		hookExit = ShieldMan.SM_Hook_Exit;
		attackEnter = ShieldMan.SM_Attack_Enter;
		attackState = ShieldMan.SM_Attack;
		attackExit = ShieldMan.SM_Attack_Exit;
		runawayEnter = ShieldMan.SM_Runaway_Enter;
		runawayState = ShieldMan.SM_Runaway;
		runawayExit = ShieldMan.SM_Runaway_Exit;
		walkawayEnter = ShieldMan.SM_Walkaway_Enter;
		walkawayState = ShieldMan.SM_Walkaway;
		walkawayExit = ShieldMan.SM_Walkaway_Exit;
		chaseEnter = ShieldMan.SM_Chase_Enter;
		chaseState = ShieldMan.SM_Chase;
		chaseExit = ShieldMan.SM_Chase_Exit;
		shieldFEnter = ShieldMan.SM_ShieldF_Enter;
		shieldFState = ShieldMan.SM_ShieldF;
		shieldFExit = ShieldMan.SM_ShieldF_Exit;
		shieldSEnter = ShieldMan.SM_ShieldS_Enter;
		shieldSState = ShieldMan.SM_ShieldS;
		shieldSExit = ShieldMan.SM_ShieldS_Exit;
		shieldBEnter = ShieldMan.SM_ShieldB_Enter;
		shieldBState = ShieldMan.SM_ShieldB;
		shieldBExit = ShieldMan.SM_ShieldB_Exit;
		dieEnter = ShieldMan.SM_Die_Enter;
		dieState = ShieldMan.SM_Die;
		dieExit = ShieldMan.SM_Die_Exit;

		//转换
		roundtorest = ShieldMan.SM_Round_Rest;
		resttoround = ShieldMan.SM_Rest_Round;
		roundtovigilance = ShieldMan.SM_Rest_Vigilance;
		vigilancetoround = ShieldMan.SM_Vigilance_Round;
		resttovigilance = ShieldMan.SM_Rest_Vigilance;
		vigilancetosearch = ShieldMan.SM_Vigilance_Search;
		searchtoround = ShieldMan.SM_Search_Round;
		chasetoshieldF = ShieldMan.SM_Chase_ShieldF;
		shieldFtochase = ShieldMan.SM_ShieldF_Chase;
		anytohook = ShieldMan.SM_Any_Hook;
		anytocharge = ShieldMan.SM_Any_Charge;
		chargetoshieldF = ShieldMan.SM_Charge_ShieldF;
		hooktoshieldF = ShieldMan.SM_Hook_ShieldF;
		shieldStoattack = ShieldMan.SM_ShieldS_Attack;
		attacktoshieldS = ShieldMan.SM_Attack_ShieldS;
		shieldFtoshieldS = ShieldMan.SM_ShieldF_ShieldS;
		shieldStoshieldF = ShieldMan.SM_ShieldS_ShieldF;
		shieldBtowalkaway = ShieldMan.SM_ShieldB_Walkaway;
		walkawaytoshieldB = ShieldMan.SM_Walkaway_ShieldB;
		walkawaytorunaway = ShieldMan.SM_Walkaway_Runaway;
		runawaytowalkaway = ShieldMan.SM_Runaway_Walkaway;

		// 决策反馈函数
		near = ShieldMan.SM_Near;
		far = ShieldMan.SM_Far;
		staLose = ShieldMan.SM_StaLose;
		staRecover = ShieldMan.SM_StaRecover;
		hurtup = ShieldMan.SM_HurtUp;

		// 情感
		despair = ShieldMan.SM_Despair_Emotion;
		normal = ShieldMan.SM_Normal_Emotion;
		angry = ShieldMan.SM_Angry_Emotion;
		excited = ShieldMan.SM_Excited_Emotion;

		// 生成指定个数单位
		for (int i = 0; i < count; i++) {
			AIEntity mAIEntity = new AIEntity ();
			UEntity mPlayer = new UEntity ();
			UEntity mPlayerLast = new UEntity ();

			ECSWorld.MainWorld.registerEntityAfterInit (mAIEntity);

			ECSWorld.MainWorld.registerEntityAfterInit (mPlayer);

			mAIEntity.mAI = shieldAI;
			mAIEntity.mPlayer = player;
			mAIEntity.PlayerEntity = mPlayer;

			mAIEntity.AIPos = new Vector3 (
				gameObject.transform.position.x + Random.Range (-randomRange, randomRange), 0, 
				gameObject.transform.position.z + Random.Range (-randomRange, randomRange));

			mAIEntity.Init ();

			mAIEntity.AddComponent<shieldAI> (new shieldAI ());
			mAIEntity.AddComponent<staComponent> (new staComponent ());
			mAIEntity.AddComponent<SSHComponent> (new SSHComponent ());
			mAIEntity.AddComponent<showAll> (new showAll ());
			mAIEntity.AddComponent<getPlayer> (new getPlayer ());
			mAIEntity.AddComponent<GYTHPComponent> (new GYTHPComponent ());

			mAIEntity.GetComponent<BaseAIComponent> ().mAIRT.SetActive (true);

			mAIEntity.GetComponent<GYTHPComponent> ().HP = mAIEntity.GetComponent<SSHComponent> ().HP;
			mAIEntity.GetComponent<GYTHPComponent> ().HPNow = mAIEntity.GetComponent<SSHComponent> ().HP;

			animator = mAIEntity.GetComponent<BaseAIComponent> ().mAIRT.GetComponent<Animator> ();
			mAIEntity.GetComponent<AIAnimation> ().mAnimator = animator;

			mAIEntity.GetComponent<AIAnimation> ().Add ("Idle", idleAnim);
			mAIEntity.GetComponent<AIAnimation> ().Add ("Walk", walkAnim);
			mAIEntity.GetComponent<AIAnimation> ().Add ("Run", runAnim);
			mAIEntity.GetComponent<AIAnimation> ().Add ("ShieldWalk", shieldWalkAnim);
			mAIEntity.GetComponent<AIAnimation> ().Add ("ShieldWalkBack", shieldWalkBAnim);
			mAIEntity.GetComponent<AIAnimation> ().Add ("ShieldStop", shieldStopAnim);
			mAIEntity.GetComponent<AIAnimation> ().Add ("Die", dieAnim);
			mAIEntity.GetComponent<AIAnimation> ().Add ("Hook", hookAnim);
			mAIEntity.GetComponent<AIAnimation> ().Add ("Charge", chargeAnim);
			mAIEntity.GetComponent<AIAnimation> ().Add ("Attack", attackAnim);
			mAIEntity.GetComponent<AIAnimation> ().mtempAnim = "Idle";

			mAIEntity.GetComponent<AIEmotion> ().InsertEmotion ("Despair");
			mAIEntity.GetComponent<AIEmotion> ().InsertEmotion ("Normal");
			mAIEntity.GetComponent<AIEmotion> ().InsertEmotion ("Angry");
			mAIEntity.GetComponent<AIEmotion> ().InsertEmotion ("Excited");

			mAIEntity.GetComponent<AIEmotion> ().InsertEdge ("Despair", despair);
			mAIEntity.GetComponent<AIEmotion> ().InsertEdge ("Normal", normal);
			mAIEntity.GetComponent<AIEmotion> ().InsertEdge ("Angry", angry);
			mAIEntity.GetComponent<AIEmotion> ().InsertEdge ("Excited", excited);


			// 巡逻决策
			AIState state_idle = new AIState ();
			state_idle.Init ();
			state_idle.mName = "SMidle";
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
			// 一个反馈，距离接近
			state_idle.AddEdge (vigilancetosearch, near, idle_vigilance, idle_search);
			state_idle.AddEdge (searchtoround, EmptyFeedbacker.Run, idle_search, idle_round);

			state_idle.AddAnimation (roundState, "Walk");
			state_idle.AddAnimation (restState, "Idle");
			state_idle.AddAnimation (vigilanceState, "Idle");
			state_idle.AddAnimation (searchState, "Walk");

			state_idle.tempID = idle_round;

			int id1 = mAIEntity.GetComponent<AIStrategy> ().AddStrategy (IdleActioner, IdleEnter, IdleExit, IdleFeedbacker, state_idle);
			mAIEntity.GetComponent<AIStrategy> ().AddName (id1, "Idle");


			// 接近决策
			AIState state_approach = new AIState ();
			state_approach.Init ();
			state_approach.mName = "SMapproach";
			state_approach.mStateRecorder = mRecorder;
			state_approach.LastEntityData.AddComponent<SMREComponent> (new SMREComponent ());

			int approach_hook = state_approach.AddExecuter (hookState, hookExit, hookEnter);
			int approach_charge = state_approach.AddExecuter (chargeState, chargeExit, chargeEnter);
			int approach_chase = state_approach.AddExecuter (chaseState, chaseExit, chaseEnter);
			int approach_shieldF = state_approach.AddExecuter (shieldFState, shieldFExit, shieldFEnter);

			state_approach.AddStateName (approach_hook, "Hook");
			state_approach.AddStateName (approach_charge, "Charge");
			state_approach.AddStateName (approach_chase, "Chase");
			state_approach.AddStateName (approach_shieldF, "ShieldF");

			// 首先考虑到出钩和冲撞是个开关节点，执行完动作后就会退出，不需要反馈
			state_approach.AddAnywayTranfer (anytohook, EmptyFeedbacker.Run, approach_hook);
			state_approach.AddAnywayTranfer (anytocharge, EmptyFeedbacker.Run, approach_charge);

			state_approach.AddEdge (hooktoshieldF, EmptyFeedbacker.Run, approach_hook, approach_shieldF);
			state_approach.AddEdge (chargetoshieldF, EmptyFeedbacker.Run, approach_charge, approach_shieldF);

			state_approach.AddDoubleEdge (chasetoshieldF, shieldFtochase, staRecover, near, approach_chase, approach_shieldF);

			state_approach.AddAnimation (hookState, "Hook");// 注意，绑定的是放慢版的出拳
			state_approach.AddAnimation (chargeState, "Charge");
			state_approach.AddAnimation (chaseState, "Run");
			state_approach.AddAnimation (shieldFState, "ShieldWalk");

			state_approach.tempID = approach_chase;


			int id2 = mAIEntity.GetComponent<AIStrategy> ().AddStrategy (ApproachActioner, ApproachEnter, ApproachExit, ApproachFeedbacker, state_approach);
			mAIEntity.GetComponent<AIStrategy> ().AddName (id2, "Approach");

			// 攻击决策
			AIState state_attack = new AIState ();
			state_attack.Init ();
			state_attack.mName = "SMattack";
			state_attack.mStateRecorder = mRecorder;
			state_attack.LastEntityData.AddComponent<SMREComponent> (new SMREComponent ());
			// 排列顺序(word中)，从左到右，从上到下
			int attack_shieldF = state_attack.AddExecuter (shieldFState, shieldFExit, shieldFEnter);
			int attack_attack = state_attack.AddExecuter (attackState, attackExit, attackEnter);
			int attack_shieldS = state_attack.AddExecuter (shieldSState, shieldSExit, shieldSEnter);

			state_attack.AddStateName (attack_shieldF, "ShieldF");
			state_attack.AddStateName (attack_attack, "Attack");
			state_attack.AddStateName (attack_shieldS, "ShieldS");

			// 举盾接近到停止，可以理解为为了恢复体力；停止到举盾接近，可以理解为为了接近
			state_attack.AddDoubleEdge (shieldFtoshieldS, shieldStoshieldF, staRecover, near, attack_shieldF, attack_shieldS);
			// 前部略，后部则是因为攻击节点，造成伤害最吼
			state_attack.AddDoubleEdge (attacktoshieldS, shieldStoattack, staRecover, hurtup, attack_attack, attack_shieldS);
			state_attack.AddAnimation (shieldFState, "ShieldWalk");
			state_attack.AddAnimation (attackState, "Attack");
			state_attack.AddAnimation (shieldSState, "ShieldStop");

			state_attack.tempID = attack_shieldF;// 先接近总是没毛病

			int id3 = mAIEntity.GetComponent<AIStrategy> ().AddStrategy (AttackActioner, AttackEnter, AttackExit, AttackFeedbacker, state_attack);
			mAIEntity.GetComponent<AIStrategy> ().AddName (id3, "Attack");

			// 逃跑
			AIState state_escape = new AIState ();
			state_escape.Init ();
			state_escape.mName = "SMescape";
			state_escape.mStateRecorder = mRecorder;
			state_escape.LastEntityData.AddComponent<SMREComponent> (new SMREComponent ());

			int escape_shieldB = state_escape.AddExecuter (shieldBState, shieldFExit, shieldFEnter);
			int escape_walkaway = state_escape.AddExecuter (walkawayState, walkawayExit, walkawayEnter);
			int escape_runaway = state_escape.AddExecuter (runawayState, runawayExit, runawayEnter);

			state_escape.AddStateName (escape_shieldB, "ShieldB");
			state_escape.AddStateName (escape_walkaway, "Walkaway");
			state_escape.AddStateName (escape_runaway, "Runaway");

			state_escape.AddDoubleEdge (shieldBtowalkaway, walkawaytoshieldB, far, far, escape_shieldB, escape_walkaway);
			state_escape.AddDoubleEdge (walkawaytorunaway, runawaytowalkaway, far, staRecover, escape_walkaway, escape_runaway);

			state_escape.AddAnimation (shieldBState, "ShieldWalkBack");
			state_escape.AddAnimation (walkawayState, "Walk");
			state_escape.AddAnimation (runawayState, "Run");

			state_escape.tempID = escape_shieldB;

			int id4 = mAIEntity.GetComponent<AIStrategy> ().AddStrategy (EscapeActioner, EscapeEnter, EscapeExit, EscapeFeedbacker, state_escape);
			mAIEntity.GetComponent<AIStrategy> ().AddName (id4, "Escape");
//			AIState state_block = new AIState ();
//			// 中间先空着，不好说。现在的问题是，群体性行为考虑写在一个函数中，目前只有一个行为...
//
//			int id5 = mAIEntity.GetComponent<AIStrategy> ().AddStrategy (BlockActioner, BlockEnter, BlockExit, BlockFeedbacker, state_block);


			// 死亡决策
			AIState state_death = new AIState ();
			state_death.Init ();
			state_death.mName = "SMdeath";
			state_death.mStateRecorder = mRecorder;
			state_death.LastEntityData.AddComponent<SMREComponent> (new SMREComponent ());

			int death_die = state_death.AddExecuter (dieState, dieExit, dieEnter);

			state_death.AddStateName (death_die, "Die");

			state_death.AddAnimation (dieState, "Die");

			state_death.tempID = death_die;

			int id6 = mAIEntity.GetComponent<AIStrategy> ().AddStrategy (DeathActioner, DeathEnter, DeathExit, DeathFeedbacker, state_death);
			mAIEntity.GetComponent<AIStrategy> ().AddName (id6, "Death");

			mAIEntity.GetComponent<AIStrategy> ().SetEntry (id1);

			mAIEntity.GetComponent<shieldAI> ().mRoundCenter = gameObject.transform.position +
			new Vector3 (Random.Range (-roundRange, roundRange), 0, 
				Random.Range (-roundRange, roundRange));

			// 组件的赋值
			GameObject rt = mAIEntity.GetComponent<BaseAIComponent> ().mAIRT;

			foreach (Transform t in rt.GetComponentsInChildren<Transform>()) {
				if (t.name == "vs") {
					mAIEntity.GetComponent<getPlayer> ().vs = t.gameObject;
				} else if (t.gameObject.name == "ShieldFront") {
					mAIEntity.GetComponent<SSHComponent> ().shieldFObj = t.gameObject;
				} else if (t.gameObject.name == "ShieldBack") {
					mAIEntity.GetComponent<SSHComponent> ().shieldBObj = t.gameObject;
				} else if (t.gameObject.name == "SwordFront") {
					mAIEntity.GetComponent<SSHComponent> ().swordFObj = t.gameObject;
				} else if (t.gameObject.name == "SwordBack") {
					mAIEntity.GetComponent<SSHComponent> ().swordBObj = t.gameObject;
				} else if (t.gameObject.name == "HookFront") {
					mAIEntity.GetComponent<SSHComponent> ().hookFObj = t.gameObject;
				} else if (t.gameObject.name == "HookBack") {
					mAIEntity.GetComponent<SSHComponent> ().hookBObj = t.gameObject;
				} else if (t.gameObject.name == "HeadC") {
					mAIEntity.GetComponent<SSHComponent> ().headC = t.gameObject;
				}
			}
			mAIEntity.GetComponent<SSHComponent> ().hookThrowObj = hookThrow;

			// 为武器伤害赋值
			mAIEntity.GetComponent<SSHComponent> ().swordFObj.GetComponent<WeaponDamage> ().damage = 
				mAIEntity.GetComponent<SSHComponent> ().swordDamage;
			mAIEntity.GetComponent<SSHComponent> ().hookFObj.GetComponent<WeaponDamage> ().damage = 
				mAIEntity.GetComponent<SSHComponent> ().hookDamage;
			// 偶数冲锋,奇数钩子
			if (i % 2 == 0) {
				mAIEntity.GetComponent<SSHComponent> ().hasHook = false;
				mAIEntity.GetComponent<SSHComponent> ().hasCharge = true;
			} else {
				mAIEntity.GetComponent<SSHComponent> ().hasHook = true;
				mAIEntity.GetComponent<SSHComponent> ().hasCharge = false;
			}
			// 关于组行为的新加代码，在几个盾兵中，生成一个发起者
			mAIEntity.tag = "ShieldMan";
			if (i % 6 == 0) {
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
