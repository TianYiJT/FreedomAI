using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FreedomAI;
using UnityECS;

//关于盾牌兵的动画，记录者，节点，状态连线，心情


public class staComponent:UComponent
{
	//耐力值设定stamina
	public float staMax = 30.0f;
	//疲劳值上限
	public float staNow = 30.0f;
	public float staAttack = 4.0f;
	//攻击耐力消耗（每秒）
	public float staRun = 2.0f;
	// 兴奋状态下，耐力恢复加快
	public float staExcited = 2.0f;

	public float staWalk = 1.0f;
	public float staRecover = 1.0f;
	//每秒耐力恢复，也就是说，走路时，耐力保持
	public float staPower = 10.0f;
	//冲锋/出钩耐力消耗
	public float staRate = 1.0f;
	//耐力值更新速率每秒更新一次
	public float staRateInit = 1.0f;
	// 初始化
}
// 特别设置的浮点型生命值组件（暂时没用上）
public class GYTHPComponent:UComponent
{
	public float HP;
	public float HPNow;
	public float HurtNow;
}
// 用于记录反馈数值的组件
public class SMREComponent:UComponent
{
	public float staMax;
	public float staNow;
	public float HP;
	public float MaxHP;
	public Vector3 AIPos;
	public Vector3 PlayerPos;
}

public class StaSystem:USystem
{
	// 实现每一秒恢复一次耐力
	public override void Init ()
	{
		base.Init ();
		this.AddRequestComponent (typeof(staComponent));
	}

	public override void Update (UEntity uEntity)
	{
		base.Update (uEntity);
		if (uEntity.GetComponent<staComponent> ().staRate >= 0.0f)
			uEntity.GetComponent<staComponent> ().staRate -= Time.deltaTime;
		else {
			if (uEntity.GetComponent<AIEmotion> ().mtempName == "Excited") {
				uEntity.GetComponent<staComponent> ().staNow = Mathf.Min (
					uEntity.GetComponent<staComponent> ().staMax,
					uEntity.GetComponent<staComponent> ().staNow +
					uEntity.GetComponent<staComponent> ().staExcited);
			} else {
				uEntity.GetComponent<staComponent> ().staNow = Mathf.Min (
					uEntity.GetComponent<staComponent> ().staMax,
					uEntity.GetComponent<staComponent> ().staNow +
					uEntity.GetComponent<staComponent> ().staRecover);
			}
			uEntity.GetComponent<staComponent> ().staRate = 
				uEntity.GetComponent<staComponent> ().staRateInit;
		}
	}
}

// 剑、钩、盾牌管理组件ssh = sword + shield + hook
// 同时处理盾牌血量问题，自身受伤问题
public class SSHComponent:UComponent
{
	//剑、钩子、盾牌对象
	//放在身前的物品（拔刀状态）
	public GameObject swordFObj;
	public GameObject hookFObj;
	public GameObject shieldFObj;

	public GameObject hookThrowObj;
	// 用于投掷的钩子
	//收刀状态
	public GameObject swordBObj;
	public GameObject hookBObj;
	public GameObject shieldBObj;
	// 头部碰撞检测
	public GameObject headC;
	//判断是哪一个能力(之所以每个都有一个bool是因为以后若想要扩充，可以不修改之前的代码
	public bool hasHook = false;
	public bool hasCharge = false;
	// 盾牌存在
	public bool hasShield = true;
	// 判断位置
	public bool back = true;
	// 盾牌，当前盾牌耐久值
	public float shieldHP = 500.0f;
	public float shieldHPNow = 500.0f;
	public float HP = 1000.0f;
	public float HPNow = 1000.0f;
	public float hookDamage = 50.0f;
	public float swordDamage = 50.0f;

	public bool meleeActive = false;

	public float atkCount = 0f;
	// 记录攻击次数，并且按照时间递减
}

// 没有这个系统的话就很麻烦，不过盾牌什么时候会掉血呢
public class SSHSystem:USystem
{
	// 管理是剑还是钩子；武器与盾牌应处于的位置；盾牌毁坏的管理；生命值管理
	public override void Init ()
	{
		base.Init ();
		this.AddRequestComponent (typeof(SSHComponent));
	}

	public override void Update (UEntity uEntity)
	{
		base.Update (uEntity);
		// 只是借用来更新几个需要每帧调用的值...
		uEntity.GetComponent<SSHComponent> ().atkCount = 
			Mathf.Max (0f, uEntity.GetComponent<SSHComponent> ().atkCount - 0.3f * Time.deltaTime);
		// 判断盾、头部所受到的伤害
		float shieldDamage = uEntity.GetComponent<SSHComponent> ().shieldFObj.GetComponent<enemyHit> ().damage;
		shieldDamage += uEntity.GetComponent<SSHComponent> ().shieldBObj.GetComponent<enemyHit> ().damage;
		float headDamage = uEntity.GetComponent<SSHComponent> ().headC.GetComponent<enemyHit> ().damage;
		if (shieldDamage > 0.0f || headDamage > 0.0f)
			uEntity.GetComponent<getPlayer> ().engage = true;
		uEntity.GetComponent<SSHComponent> ().shieldFObj.GetComponent<enemyHit> ().damage = 0.0f;
		uEntity.GetComponent<SSHComponent> ().shieldBObj.GetComponent<enemyHit> ().damage = 0.0f;
		uEntity.GetComponent<SSHComponent> ().headC.GetComponent<enemyHit> ().damage = 0.0f;
		uEntity.GetComponent<SSHComponent> ().shieldHPNow -= shieldDamage;
		uEntity.GetComponent<SSHComponent> ().HPNow -= headDamage;

		if (uEntity.GetComponent<shieldAI> ().powerEnd == false) {
			
		}
		// 先判断盾牌是否损坏
		if (uEntity.GetComponent<SSHComponent> ().shieldHPNow < 20.0f) {
			uEntity.GetComponent<SSHComponent> ().hasShield = false;
			uEntity.GetComponent<SSHComponent> ().shieldBObj.SetActive (false);
			uEntity.GetComponent<SSHComponent> ().shieldFObj.SetActive (false);
		}

			
		if (uEntity.GetComponent<SSHComponent> ().hasHook) {
			// 有钩子
			uEntity.GetComponent<SSHComponent> ().swordBObj.SetActive (false);
			uEntity.GetComponent<SSHComponent> ().swordFObj.SetActive (false);
			if (uEntity.GetComponent<SSHComponent> ().back) {
				// 在身后
				uEntity.GetComponent<SSHComponent> ().hookBObj.SetActive (true);
				uEntity.GetComponent<SSHComponent> ().hookFObj.SetActive (false);
				if (uEntity.GetComponent<SSHComponent> ().hasShield) {
					// 盾牌没坏
					uEntity.GetComponent<SSHComponent> ().shieldBObj.SetActive (true);
					uEntity.GetComponent<SSHComponent> ().shieldFObj.SetActive (false);
				}
				
			} else {
				// 在身前
				uEntity.GetComponent<SSHComponent> ().hookFObj.SetActive (true);
				uEntity.GetComponent<SSHComponent> ().hookBObj.SetActive (false);
				if (uEntity.GetComponent<SSHComponent> ().hasShield) {
					// 盾牌没坏
					uEntity.GetComponent<SSHComponent> ().shieldFObj.SetActive (true);
					uEntity.GetComponent<SSHComponent> ().shieldBObj.SetActive (false);
				}
			}
		} else if (uEntity.GetComponent<SSHComponent> ().hasCharge) {
			// 有剑
			uEntity.GetComponent<SSHComponent> ().hookBObj.SetActive (false);
			uEntity.GetComponent<SSHComponent> ().hookFObj.SetActive (false);
			if (uEntity.GetComponent<SSHComponent> ().back) {
				// 在身后
				uEntity.GetComponent<SSHComponent> ().swordBObj.SetActive (true);
				uEntity.GetComponent<SSHComponent> ().swordFObj.SetActive (false);
				if (uEntity.GetComponent<SSHComponent> ().shieldHPNow > 10.0f) {
					// 盾牌没坏
					uEntity.GetComponent<SSHComponent> ().shieldBObj.SetActive (true);
					uEntity.GetComponent<SSHComponent> ().shieldFObj.SetActive (false);
				}

			} else {
				// 在身前
				uEntity.GetComponent<SSHComponent> ().swordFObj.SetActive (true);
				uEntity.GetComponent<SSHComponent> ().swordBObj.SetActive (false);
				if (uEntity.GetComponent<SSHComponent> ().shieldHPNow > 10.0f) {
					// 盾牌没坏
					uEntity.GetComponent<SSHComponent> ().shieldFObj.SetActive (true);
					uEntity.GetComponent<SSHComponent> ().shieldBObj.SetActive (false);
				}
			}
		}
	}
}



//public static float staVel = 1.0f;//速度标准值，通过修改此值来对所有速度进行修改
public class shieldAI:UComponent
{
	public float atkCount = 0f;
	public float dieTime = 5.0f;
	public GameObject hook;
	public Vector3 playerPos;
	// 在使用钩子时使用，表示钩子最后的位置
	public bool powerEnd = true;
	public float powerRest = 0.0f;
	// 每使用一次特殊攻击间隔的时间更新
	public float powerTimeInit = 2.0f;
	public float powerTime = 2.0f;
	//最多持续冲刺时间
	//结束出钩或冲锋的判断条件
	public float updateRate = 1.0f;
	//设置每一秒更新一次消耗值
	public float powerDelay = 1.0f;
	//使用冲锋与出钩，需要的准备时间
	public float attackRate = 1.0f;
	public float initPD = 1.0f;
	//用于初始化powerDelay
	//跑步、走步、持盾、受伤(减少)、激怒(加成)、冲锋、钩子速度
	//	public static float staVel = 1.0f;//速度标准值，通过修改此值来对所有速度进行修改
	public float runVel = 2.0f;
	public float walkVel = 1.5f;
	public float shieldVel = 1.0f;
	public float hurtVel = (-1.0f);
	public float angryVel = (+1.0f);
	public float chargeVel = 5.0f;
	public float hookVel = 20.0f;

	// 攻击结束
	public bool attackfinish = true;

	//关于巡逻，直接使用圆内设点
	public Vector3 mRoundCenter = Vector3.zero;
	public float RoundSize = 20.0f;
	public Vector3 tempRoundPoint = Vector3.zero;

	//攻击/每秒、回血速率(百分比)
	public float hitRate = 1.0f;
	public float healRate = 0.01f;

	//由于走路不消耗，特此设置疲劳时间，记住这个是用来求概率值
	public float tiredTime = 60.0f;
	public float ttinit = 60.0f;
	public float restTime = 5.0f;
	// 每次休息时长
	public float rtintit = 5.0f;
	// 处于休息
	public bool rest = false;


	// 交战中，整合到getplayer组件中
	// 当玩家处于敌人视线或发出声音时，就将insight设置为true，填充只考虑Time.deltatime。。。
	public GameObject vs;
	public AStarComputerformula mAStarComputerformula;
	public Stack<Vector3> mTempTarget;
}

public class ShieldMan
{

	//除了死亡决策，其余决策层均判断是否处于特殊攻击状态
	// 巡逻休息相关
	public static void IdleEnter (AIEntity pEntity)
	{
//		Debug.Log ("SMIdle up");
		pEntity.GetComponent<getPlayer> ().engage = false;
		pEntity.GetComponent<SSHComponent> ().HP = pEntity.GetComponent<SSHComponent> ().HPNow;
		pEntity.GetComponent<AIMove> ().mVelocity = 0.0f;
	}

	public static void IdleExit (AIEntity pEntity)
	{
	}

	public static float IdleFeedbacker (AIEntity pEntity)
	{
		return 0.0f;
	}

	public static float IdleAction (AIEntity pEntity)
	{
		// 没有使用特殊攻击，且距离过远
		if (pEntity.GetComponent<shieldAI> ().powerEnd) {
			return 0.5f;
		}
		return 0.0f;
	}

	// 接近玩家
	public static void ApproachEnter (AIEntity pEntity)
	{
//		Debug.Log ("SMApproach up");
	}

	public static void ApproachExit (AIEntity pEntity)
	{

	}

	public static float ApproachFeedbacker (AIEntity pEntity)
	{
		return 0.0f;
	}

	public static float ApproachAction (AIEntity pEntity)
	{
		// 距离中等，且处于交战（发现玩家）
		if (Vector3.Distance (pEntity.AIPos, pEntity.PlayerPos) > 6.0f &&
		    Vector3.Distance (pEntity.AIPos, pEntity.PlayerPos) < 20.0f &&
		    pEntity.GetComponent<getPlayer> ().engage) {
			return 0.6f;
		}
		return 0.0f;

	}

	// 攻击玩家
	public static void AttackEnter (AIEntity pEntity)
	{
//		Debug.Log ("SMAttack up");
	}

	public static void AttackExit (AIEntity pEntity)
	{
	}

	public static float AttackFeedbacker (AIEntity pEntity)
	{
		return 0.0f;
	}

	public static float AttackAction (AIEntity pEntity)
	{
		//距离小，处于交战，能力未发动
		if (Vector3.Distance (pEntity.AIPos, pEntity.PlayerPos) < 7.0f &&
		    pEntity.GetComponent<getPlayer> ().engage &&
		    pEntity.GetComponent<shieldAI> ().powerEnd) {
			return 0.7f;
		}
		return 0.0f;
	}

	// 逃离玩家
	public static void EscapeEnter (AIEntity pEntity)
	{
//		Debug.Log ("SMEscape up");
	}

	public static void EscapeExit (AIEntity pEntity)
	{
	}

	public static float EscapeFeedbacker (AIEntity pEntity)
	{
		return 0.0f;
	}

	public static float EscapeAction (AIEntity pEntity)
	{
		// 生命值小于百分之三十.超级打击完毕，处于交战中，距离小于20，跑路
		if (pEntity.GetComponent<SSHComponent> ().HPNow < pEntity.GetComponent<SSHComponent> ().HP * 0.3f &&
		    pEntity.GetComponent<shieldAI> ().powerEnd &&
		    pEntity.GetComponent<getPlayer> ().engage &&
		    Vector3.Distance (pEntity.AIPos, pEntity.PlayerPos) < 20.0f) {
			return 0.8f;
		}
		return 0.0f;
	}

	// 封锁玩家
	public static void BlockEnter (AIEntity pEntity)
	{
//		Debug.Log ("SMBlock up");
	}

	public static void BlockExit (AIEntity pEntity)
	{

	}

	public static float BlockFeedbacker (AIEntity pEntity)
	{
		return 0.0f;
	}

	public static float BlockAction (AIEntity pEntity)
	{
		// 之后修改
		return 0.4f;
	}

	// 死亡
	public static void DeathEnter (AIEntity pEntity)
	{
//		Debug.Log ("SMDeath up");
		pEntity.GetComponent<AIMove> ().mVelocity = 0.0f;
	}

	public static void DeathExit (AIEntity pEntity)
	{
	}

	public static float DeathFeedbacker (AIEntity pEntity)
	{
		return 0.0f;
	}

	public static float DeathAction (AIEntity pEntity)
	{
		if (pEntity.GetComponent<SSHComponent> ().HPNow < 10)
			return 0.9f;
		return 0.0f;
	}

	//顺带一提，我动画的速度在动画机里已经调好了
	//特别的，出钩与攻击一个动作；冲锋与跑步一个动作
	//闲置或休息
	public static void SM_idle_Anim (Animator animator)
	{
		animator.SetBool ("Idle", true);

		animator.SetBool ("Shield", false);
		animator.SetBool ("Walk", false);
		animator.SetBool ("Run", false);
		animator.SetBool ("Attack", false);
		animator.SetBool ("Die", false);

		animator.speed = 1.0f;
	}
	//行走
	public static void SM_walk_Anim (Animator animator)
	{
		animator.SetBool ("Idle", false);
		animator.SetBool ("Shield", false);

		animator.SetBool ("Walk", true);

		animator.SetBool ("Run", false);
		animator.SetBool ("Attack", false);
		animator.SetBool ("Die", false);

		animator.speed = 2.0f;
	}
	//跑步
	public static void SM_run_Anim (Animator animator)
	{
		animator.SetBool ("Idle", false);
		animator.SetBool ("Shield", false);
		animator.SetBool ("Walk", false);

		animator.SetBool ("Run", true);

		animator.SetBool ("Attack", false);
		animator.SetBool ("Die", false);

		animator.speed = 1.0f;
	}

	// 警戒，慢步行走
	public static void SM_vigilance_Anim (Animator animator)
	{
		animator.SetBool ("Idle", false);
		animator.SetBool ("Shield", false);

		animator.SetBool ("Walk", true);

		animator.SetBool ("Run", false);
		animator.SetBool ("Attack", false);
		animator.SetBool ("Die", false);

		animator.speed = 0.5f;
	}

	//持盾行走
	public static void SM_shield_walk_Anim (Animator animator)
	{
		animator.SetBool ("Idle", false);

		animator.SetBool ("Shield", true);
		animator.SetBool ("Walk", true);

		animator.SetBool ("Run", false);
		animator.SetBool ("Attack", false);
		animator.SetBool ("Die", false);

		animator.speed = 1.0f;
	}

	//持盾退后 B = back
	public static void SM_shield_walkB_Anim (Animator animator)
	{
		animator.SetBool ("Idle", false);

		animator.SetBool ("Shield", true);
		animator.SetBool ("Walk", true);

		animator.SetBool ("Run", false);
		animator.SetBool ("Attack", false);
		animator.SetBool ("Die", false);

		animator.speed = -1.0f;
	}

	//持盾静止
	public static void SM_shield_stop_Anim (Animator animator)
	{
		animator.SetBool ("Idle", false);

		animator.SetBool ("Shield", true);

		animator.SetBool ("Walk", false);
		animator.SetBool ("Run", false);
		animator.SetBool ("Attack", false);
		animator.SetBool ("Die", false);

		animator.speed = 1.0f;
	}

	//死亡
	public static void SM_die_Anim (Animator animator)
	{
		animator.SetBool ("Idle", false);
		animator.SetBool ("Shield", false);
		animator.SetBool ("Walk", false);
		animator.SetBool ("Run", false);
		animator.SetBool ("Attack", false);

		animator.SetBool ("Die", true);

		animator.speed = 1.0f;
	}

	//出钩
	public static void SM_hook_Anim (Animator animator)
	{
		animator.SetBool ("Idle", false);
		animator.SetBool ("Shield", false);
		animator.SetBool ("Walk", false);
		animator.SetBool ("Run", false);

		animator.SetBool ("Attack", true);

		animator.SetBool ("Die", false);

		animator.speed = 0.2f;//设置的慢点，前摇动作长，玩家好反应
	}

	//冲锋
	public static void SM_charge_Anim (Animator animator)
	{
		animator.SetBool ("Idle", false);
		animator.SetBool ("Shield", false);
		animator.SetBool ("Walk", false);

		animator.SetBool ("Run", true);

		animator.SetBool ("Attack", false);
		animator.SetBool ("Die", false);

		animator.speed = 2.0f;
	}

	//攻击
	public static void SM_attack_Anim (Animator animator)
	{
		animator.SetBool ("Idle", false);
		animator.SetBool ("Shield", false);
		animator.SetBool ("Walk", false);
		animator.SetBool ("Run", false);

		animator.SetBool ("Attack", true);

		animator.SetBool ("Die", false);

		animator.speed = 1.0f;
	}

	public static void SM_Recorder (AIEntity pEntity)
	{
		//记录上一帧数据，通过一个实体（其中包含AI与player），更新旧数据

		pEntity.GetComponent<AIState> ().LastEntityData.GetComponent<SMREComponent> ().AIPos = pEntity.AIPos;
		pEntity.GetComponent<AIState> ().LastEntityData.GetComponent<SMREComponent> ().PlayerPos = pEntity.PlayerPos;
		pEntity.GetComponent<AIState> ().LastEntityData.GetComponent<SMREComponent> ().staNow = 
			pEntity.GetComponent<staComponent> ().staNow;
		pEntity.GetComponent<AIState> ().LastEntityData.GetComponent<SMREComponent> ().MaxHP =
			pEntity.mPlayer.GetComponent<GYTPlayerControl> ().MaxHP;
		pEntity.GetComponent<AIState> ().LastEntityData.GetComponent<SMREComponent> ().HP =
			pEntity.mPlayer.GetComponent<GYTPlayerControl> ().HP;
	}

	// 所有节点加入武器前后考量
	public static void SM_Round_Enter (AIEntity pEntity)
	{
//		Debug.Log ("SMRound");
		pEntity.GetComponent<shieldAI> ().tempRoundPoint = Vector3.zero;
		pEntity.GetComponent<shieldAI> ().tiredTime = pEntity.GetComponent<shieldAI> ().ttinit;
		pEntity.GetComponent<SSHComponent> ().back = true;
	}

	//直接将代码中的myAI换为shieldAI
	public static void SM_Round (AIEntity pEntity)
	{
		if (pEntity.GetComponent<shieldAI> ().tempRoundPoint == Vector3.zero) {
			Vector2 trandom = Random.insideUnitCircle;
			pEntity.GetComponent<shieldAI> ().tempRoundPoint = 
				pEntity.GetComponent<shieldAI> ().mRoundCenter + (new Vector3 (trandom.x, 0, trandom.y)) * pEntity.GetComponent<shieldAI> ().RoundSize;
		}
		if (Vector3.Distance (pEntity.GetComponent<shieldAI> ().tempRoundPoint, pEntity.AIPos) < 1.0f) {
			Vector2 trandom = Random.insideUnitCircle;
			pEntity.GetComponent<shieldAI> ().tempRoundPoint = pEntity.GetComponent<shieldAI> ().mRoundCenter + (new Vector3 (trandom.x, 0, trandom.y)) * pEntity.GetComponent<shieldAI> ().RoundSize;
		}
		Vector3 tDir = pEntity.GetComponent<shieldAI> ().tempRoundPoint - pEntity.AIPos;
		pEntity.GetComponent<AIMove> ().mDirection = tDir.normalized;
		pEntity.GetComponent<AIMove> ().mVelocity = pEntity.GetComponent<shieldAI> ().walkVel;
		pEntity.GetComponent<AIMove> ().mMoveFunc = MoveFunc.Complex;

		pEntity.GetComponent<staComponent> ().staNow -= pEntity.GetComponent<staComponent> ().staWalk * Time.deltaTime;
		pEntity.GetComponent<shieldAI> ().tiredTime -= Time.deltaTime;// 疲劳时间

	}

	public static void SM_Round_Exit (AIEntity pEntity)
	{
	}

	// 休息
	public static void SM_Rest_Enter (AIEntity pEntity)
	{
//		Debug.Log ("SMRest");
		pEntity.GetComponent<shieldAI> ().restTime = pEntity.GetComponent<shieldAI> ().rtintit;
		// 由于是通过巡逻时间增加概率，所以bool设置需要在这里
		pEntity.GetComponent<shieldAI> ().rest = true;
		pEntity.GetComponent<SSHComponent> ().back = true;
		pEntity.GetComponent<AIMove> ().mVelocity = 0.0f;
	}

	public static void SM_Rest (AIEntity pEntity)
	{
		if (pEntity.GetComponent<shieldAI> ().restTime > 0.0f)// 没休息完
			pEntity.GetComponent<shieldAI> ().restTime -= Time.deltaTime;
		else// 休息完毕
			pEntity.GetComponent<shieldAI> ().rest = false;
	}

	public static void SM_Rest_Exit (AIEntity pEntity)
	{
	}

	// 警觉
	public static void SM_Vigilance_Enter (AIEntity pEntity)
	{
//		Debug.Log ("SMVigilance");
		// 初始化警觉时间
		pEntity.GetComponent<getPlayer> ().vigilanceTime = pEntity.GetComponent<getPlayer> ().vtinit;
		pEntity.GetComponent<getPlayer> ().search = false;
		pEntity.GetComponent<getPlayer> ().vigilance = true;
		pEntity.GetComponent<SSHComponent> ().back = true;
	}

	public static void SM_Vigilance (AIEntity pEntity)
	{
		// 逻辑：初始警觉值为1倍，玩家在敌人视线内或敌人听到玩家声音就会增加之，加到2倍进入搜查；同理，看不见就会减少警觉值，到0进入巡逻
		float vMax = pEntity.GetComponent<getPlayer> ().vtinit * 2;
		if (pEntity.GetComponent<getPlayer> ().vigilanceTime > vMax) {
			// 警觉条充满，进入搜查
			pEntity.GetComponent<getPlayer> ().search = true;
			return;
		}	
		if (pEntity.GetComponent<getPlayer> ().vigilanceTime < 0.0f) {
			// 警觉条归零，进入巡逻
			pEntity.GetComponent<getPlayer> ().vigilance = false;
			return;
		}

		if (pEntity.GetComponent<getPlayer> ().insight)//视线内，填充
			pEntity.GetComponent<getPlayer> ().vigilanceTime += Time.deltaTime;
		else//视线外，减少
			pEntity.GetComponent<getPlayer> ().vigilanceTime -= Time.deltaTime;

		// 保持朝向为可疑位置，速度为0
		Vector3 tDir = pEntity.GetComponent<getPlayer> ().suspensionPoint - pEntity.AIPos;
		pEntity.GetComponent<AIMove> ().mDirection = tDir.normalized;
		pEntity.GetComponent<AIMove> ().mVelocity = 0.0f;
		pEntity.GetComponent<AIMove> ().mMoveFunc = MoveFunc.JUSTROTATE;

	}

	public static void SM_Vigilance_Exit (AIEntity pEntity)
	{
	}

	// 搜查
	public static void SM_Search_Enter (AIEntity pEntity)
	{
//		Debug.Log ("SMSearch");
		// 初始化搜查时间
		pEntity.GetComponent<getPlayer> ().searchTime = pEntity.GetComponent<getPlayer> ().stinit;
		pEntity.GetComponent<getPlayer> ().vigilance = false;
		pEntity.GetComponent<getPlayer> ().search = true;// 这里不写也行
		pEntity.GetComponent<SSHComponent> ().back = true;// 还没交战，先放在背后
	}

	public static void SM_Search (AIEntity pEntity)
	{
		if (Vector3.Distance (pEntity.AIPos, pEntity.PlayerPos) < 1.0f) {
			// 如果接近了玩家，直接交战
			pEntity.GetComponent<getPlayer> ().engage = true;
			return;
		}
		// 寻路到达可疑位置，与警觉不同的是只有到达可疑位置，才会降低搜查条，然而巡路途中，若玩家在视线中，会增加搜查条的
		float sMax = pEntity.GetComponent<getPlayer> ().stinit * 2;
		if (pEntity.GetComponent<getPlayer> ().searchTime > sMax) {
			// 搜查条充满，进入交战
			pEntity.GetComponent<getPlayer> ().engage = true;
			return;
		}	
		if (pEntity.GetComponent<getPlayer> ().searchTime < 0.0f) {
			// 搜查条归零，进入巡逻（是的，直接进入巡逻）
			pEntity.GetComponent<getPlayer> ().search = false;
			return;
		}
		if (pEntity.GetComponent<getPlayer> ().insight) {//视线内，填充
			pEntity.GetComponent<getPlayer> ().searchTime += Time.deltaTime;
		}

		if (Vector3.Distance (pEntity.AIPos, pEntity.GetComponent<getPlayer> ().suspensionPoint) < 1.0f &&
		    pEntity.GetComponent<getPlayer> ().insight == false) {//到达目的地且视线内无玩家，减少
			pEntity.GetComponent<getPlayer> ().searchTime -= Time.deltaTime;
			pEntity.GetComponent<AIMove> ().mVelocity = 0.1f;
		} else {
			// 逐步走向可疑位置
			pEntity.GetComponent<AIMove> ().mVelocity = pEntity.GetComponent<shieldAI> ().walkVel;

		}
		// 保持朝向为可疑位置，速度为0
		Vector3 tDir = pEntity.GetComponent<getPlayer> ().suspensionPoint - pEntity.AIPos;
		pEntity.GetComponent<AIMove> ().mDirection = tDir.normalized;
		pEntity.GetComponent<AIMove> ().mVelocity = pEntity.GetComponent<shieldAI> ().walkVel;
		pEntity.GetComponent<AIMove> ().mMoveFunc = MoveFunc.Complex;
	}

	public static void SM_Search_Exit (AIEntity pEntity)
	{
	}

	public static void SM_Chase_Enter (AIEntity pEntity)
	{
//		Debug.Log ("SMChase");
		//追逐，不持盾
		//考虑到盾牌的操作（设计一下，可以有两个盾牌，这是只是设置它们是否显示，然后只在进入时设置即可，之前的代码也是...）
		//关于盾牌存在的判定...在所有项目的update中单独加入...它和死亡的感觉还不太一样，
		pEntity.GetComponent<SSHComponent> ().back = true;
	}

	public static void SM_Chase (AIEntity pEntity)
	{

		Vector3 tDir = pEntity.PlayerPos - pEntity.AIPos;
		pEntity.GetComponent<AIMove> ().mDirection = tDir.normalized;
		pEntity.GetComponent<AIMove> ().mVelocity = pEntity.GetComponent<shieldAI> ().runVel;
		pEntity.GetComponent<AIMove> ().mMoveFunc = MoveFunc.Complex;//simple穿模寻路 complex避障寻路

		pEntity.GetComponent<staComponent> ().staNow -= pEntity.GetComponent<staComponent> ().staRun * Time.deltaTime;

	}

	public static void SM_Chase_Exit (AIEntity pEntity)
	{

	}

	public static void SM_ShieldF_Enter (AIEntity pEntity)
	{
//		Debug.Log ("SMShieldF");
		//持盾，接近玩家F=forward
		pEntity.GetComponent<SSHComponent> ().back = false;
	}

	public static void SM_ShieldF (AIEntity pEntity)
	{

		Vector3 tDir = pEntity.PlayerPos - pEntity.AIPos;
		
		pEntity.GetComponent<AIMove> ().mDirection = tDir.normalized;
		if (pEntity.GetComponent<SSHComponent> ().hasShield) {
			pEntity.GetComponent<AIMove> ().mVelocity = pEntity.GetComponent<shieldAI> ().shieldVel;
			if (pEntity.GetComponent<AIEmotion> ().mtempName == "Angry") {
				pEntity.GetComponent<AIMove> ().mVelocity += pEntity.GetComponent<shieldAI> ().angryVel;
			}
		} else
			pEntity.GetComponent<AIMove> ().mVelocity = pEntity.GetComponent<shieldAI> ().walkVel;
		pEntity.GetComponent<AIMove> ().mMoveFunc = MoveFunc.Complex;//simple穿模寻路 complex避障寻路

		pEntity.GetComponent<staComponent> ().staNow -= pEntity.GetComponent<staComponent> ().staWalk * Time.deltaTime;
	}

	public static void SM_ShieldF_Exit (AIEntity pEntity)
	{

	}


	public static void SM_Charge_Enter (AIEntity pEntity)
	{
		//Debug.Log ("SMCharge");
		pEntity.GetComponent<SSHComponent> ().back = true;
		//或许...在状态外部进入代码里，判断耐力是否够用？
		pEntity.GetComponent<staComponent> ().staNow -= pEntity.GetComponent<staComponent> ().staPower;
		Vector3 tDir = pEntity.PlayerPos - pEntity.AIPos;

		pEntity.GetComponent<AIMove> ().mDirection = tDir.normalized;
		pEntity.GetComponent<AIMove> ().mVelocity = 0;

		pEntity.GetComponent<AIMove> ().mMoveFunc = MoveFunc.JUSTROTATE;//穿模寻路
		pEntity.GetComponent<shieldAI> ().powerEnd = false;//设置不可退出
		//初始化超级充能时间
		pEntity.GetComponent<shieldAI> ().powerDelay = pEntity.GetComponent<shieldAI> ().initPD;
		pEntity.GetComponent<shieldAI> ().powerTime = pEntity.GetComponent<shieldAI> ().powerTimeInit;
		pEntity.mAI.GetComponent<Charge> ().stop = false;
	}

	public static void SM_Charge (AIEntity pEntity)
	{	
		//首先，要有个前摇
		if (pEntity.GetComponent<shieldAI> ().powerDelay > 0.0f) {
			pEntity.GetComponent<shieldAI> ().powerDelay -= Time.deltaTime;
		} else {
			pEntity.GetComponent<AIMove> ().mVelocity = pEntity.GetComponent<shieldAI> ().chargeVel;
			pEntity.GetComponent<AIMove> ().mMoveFunc = MoveFunc.Complex;
			pEntity.GetComponent<shieldAI> ().powerTime -= Time.deltaTime;
			//这里保持判断碰撞，当碰撞后，设置bool值为true退出冲锋状态

			if (pEntity.GetComponent<shieldAI> ().powerTime < 0.0f ||
			    pEntity.GetComponent<BaseAIComponent> ().mAIRT.GetComponent<Charge> ().stop) {
				//Debug.Log ("chargeEnd");
				pEntity.GetComponent<shieldAI> ().powerEnd = true;
			}
		}
	}

	public static void SM_Charge_Exit (AIEntity pEntity)
	{
		pEntity.GetComponent<shieldAI> ().powerTime = 20.0f;
	}

	//在hook中途，保持方向不变，速度为零
	public static void SM_Hook_Enter (AIEntity pEntity)
	{
		//Debug.Log ("SMHook");
		pEntity.GetComponent<SSHComponent> ().back = false;
		pEntity.GetComponent<staComponent> ().staNow -= pEntity.GetComponent<staComponent> ().staPower;
		Vector3 tDir = pEntity.PlayerPos - pEntity.AIPos;
		pEntity.GetComponent<shieldAI> ().powerDelay = pEntity.GetComponent<shieldAI> ().initPD;
		pEntity.GetComponent<AIMove> ().mDirection = tDir.normalized;
		pEntity.GetComponent<AIMove> ().mVelocity = 0;

		pEntity.GetComponent<AIMove> ().mMoveFunc = MoveFunc.JUSTROTATE;	//穿模寻路
		pEntity.GetComponent<shieldAI> ().powerEnd = false;//设置不可退出
		GameObject hook = GameObject.Instantiate (pEntity.GetComponent<SSHComponent> ().hookThrowObj,
			                  pEntity.AIPos + Vector3.up * 0.8f, pEntity.GetComponent<BaseAIComponent> ().mAIRT.transform.rotation);
		hook.GetComponent<Hook> ().enemyPos = pEntity.AIPos;
		hook.GetComponent<Hook> ().enemy = pEntity.GetComponent<BaseAIComponent> ().mAIRT;
		hook.GetComponent<Rigidbody> ().AddForce (tDir * 100.0f);
		hook.GetComponent<Hook> ().active = true;

		pEntity.GetComponent<shieldAI> ().powerDelay = 3.0f;
	}

	public static void SM_Hook (AIEntity pEntity)
	{
		// 关于钩子的判断，就放在钩子里边吧...保持5s之后退出。
		if (pEntity.GetComponent<shieldAI> ().powerDelay > 0.0f) {
			pEntity.GetComponent<shieldAI> ().powerDelay -= Time.deltaTime;
		} else {
			pEntity.GetComponent<shieldAI> ().powerEnd = true;
		}
	}

	public static void SM_Hook_Exit (AIEntity pEntity)
	{
		pEntity.GetComponent<shieldAI> ().powerTime = 20.0f;
	}


	public static void SM_ShieldS_Enter (AIEntity pEntity)
	{
//		Debug.Log ("SMShieldS");
		//持盾站立，或者是接近玩家，或者是精力值耗尽S=stop
		pEntity.GetComponent<SSHComponent> ().back = false;
	}

	public static void SM_ShieldS (AIEntity pEntity)
	{
		// 保持朝向xs
		Vector3 tDir = pEntity.PlayerPos - pEntity.AIPos;
		pEntity.GetComponent<AIMove> ().mDirection = tDir.normalized;
		pEntity.GetComponent<AIMove> ().mVelocity = 0.0f;
		pEntity.GetComponent<AIMove> ().mMoveFunc = MoveFunc.JUSTROTATE;//simple穿模寻路 complex避障寻路
	}

	public static void SM_ShieldS_Exit (AIEntity pEntity)
	{

	}

	public static void SM_Attack_Enter (AIEntity pEntity)
	{
//		Debug.Log ("SMAttack");
		// 攻击一次总共需要的时间
		pEntity.GetComponent<shieldAI> ().attackRate = 1.1f;// 动画长度为1.042s
		pEntity.GetComponent<shieldAI> ().attackfinish = false;
		pEntity.GetComponent<SSHComponent> ().back = false;
	}

	public static void SM_Attack (AIEntity pEntity)
	{
		//保证攻击方向，但自身不动
		Vector3 tDir = pEntity.PlayerPos - pEntity.AIPos;
		pEntity.GetComponent<AIMove> ().mDirection = tDir.normalized;
		pEntity.GetComponent<AIMove> ().mVelocity = 0;
		pEntity.GetComponent<staComponent> ().staNow -= pEntity.GetComponent<staComponent> ().staAttack * Time.deltaTime;

		if (pEntity.GetComponent<shieldAI> ().attackRate > 0.0f) {
			pEntity.GetComponent<shieldAI> ().attackRate -= Time.deltaTime;
		} else {
			
			pEntity.GetComponent<shieldAI> ().attackfinish = true;
		}
	}

	public static void SM_Attack_Exit (AIEntity pEntity)
	{
		pEntity.GetComponent<SSHComponent> ().atkCount += 1.0f;
	}

	public static void SM_ShieldB_Enter (AIEntity pEntity)
	{
//		Debug.Log ("SMShieldB");
		//持盾，远离玩家B=back
		pEntity.GetComponent<SSHComponent> ().back = false;
	}

	public static void SM_ShieldB (AIEntity pEntity)
	{
		Vector3 tDir = pEntity.PlayerPos - pEntity.AIPos;
		pEntity.GetComponent<AIMove> ().mDirection = tDir.normalized;
		pEntity.GetComponent<AIMove> ().mVelocity = -pEntity.GetComponent<shieldAI> ().walkVel;
		pEntity.GetComponent<AIMove> ().mMoveFunc = MoveFunc.Complex;//simple穿模寻路 complex避障寻路
	}

	public static void SM_ShieldB_Exit (AIEntity pEntity)
	{
	}
	// 走路离开
	public static void SM_Walkaway_Enter (AIEntity pEntity)
	{
		//Debug.Log ("SMWalkaway");
		pEntity.GetComponent<SSHComponent> ().back = true;
	}

	public static void SM_Walkaway (AIEntity pEntity)
	{
		Vector3 tDir = pEntity.AIPos - pEntity.PlayerPos;
		pEntity.GetComponent<AIMove> ().mDirection = tDir.normalized;
		pEntity.GetComponent<AIMove> ().mVelocity = pEntity.GetComponent<shieldAI> ().walkVel;
		pEntity.GetComponent<AIMove> ().mMoveFunc = MoveFunc.Complex;//simple穿模寻路 complex避障寻路

		pEntity.GetComponent<staComponent> ().staNow -= pEntity.GetComponent<staComponent> ().staWalk * Time.deltaTime;
	}

	public static void SM_Walkaway_Exit (AIEntity pEntity)
	{
	}
	// 跑步离开
	public static void SM_Runaway_Enter (AIEntity pEntity)
	{
		//Debug.Log ("SMRunaway");
		pEntity.GetComponent<SSHComponent> ().back = true;
	}

	public static void SM_Runaway (AIEntity pEntity)
	{
		Vector3 tDir = pEntity.AIPos - pEntity.PlayerPos;
		pEntity.GetComponent<AIMove> ().mDirection = tDir.normalized;
		pEntity.GetComponent<AIMove> ().mVelocity = pEntity.GetComponent<shieldAI> ().runVel;
		pEntity.GetComponent<AIMove> ().mMoveFunc = MoveFunc.Complex;//simple穿模寻路 complex避障寻路

		pEntity.GetComponent<staComponent> ().staNow -= pEntity.GetComponent<staComponent> ().staRun * Time.deltaTime;
	}

	public static void SM_Runaway_Exit (AIEntity pEntity)
	{
	}

	public static void SM_Die_Enter (AIEntity pEntity)
	{
		//Debug.Log ("SMDie");
	}

	public static void SM_Die (AIEntity pEntity)
	{
		if (pEntity.GetComponent<shieldAI> ().dieTime > 0.0f) {
			pEntity.GetComponent<shieldAI> ().dieTime -= Time.deltaTime; 
		} else {
			GameObject.Destroy (pEntity.GetComponent<BaseAIComponent> ().mAIRT);
			ECSWorld.MainWorld.deleteEntity (pEntity);
		}
	}

	public static void SM_Die_Exit (AIEntity pEntity)
	{
	}

	//状态间转化的权值计算函数
	//注意一下，冲锋与出钩为持续型状态，除非触发效果，否则无法退出的
	// 巡逻转换（基本通用）
	public static float SM_Round_Rest (AIEntity pEntity)
	{
		// 随着巡逻时间加长，t逐渐减少
		float t = pEntity.GetComponent<shieldAI> ().tiredTime;
		float tmax = pEntity.GetComponent<shieldAI> ().ttinit;
		return Mathf.Min (1.0f, (tmax - t) / tmax);
	}

	public static float SM_Rest_Round (AIEntity pEntity)
	{
		// 休息够了
		if (pEntity.GetComponent<shieldAI> ().rest == false)
			return 1.0f;
		return 0.0f;
	}

	public static float SM_Rest_Vigilance (AIEntity pEntity)
	{
		if (pEntity.GetComponent<getPlayer> ().insight)
			return 1.0f;
		return 0.0f;
	}

	public static float SM_Round_Vigilance (AIEntity pEntity)
	{
		if (pEntity.GetComponent<getPlayer> ().insight)
			return 1.0f;
		return 0.0f;
	}

	public static float SM_Vigilance_Round (AIEntity pEntity)
	{
		if (pEntity.GetComponent<getPlayer> ().vigilance)
			return 0.0f;
		return 1.0f;
	}

	public static float SM_Vigilance_Search (AIEntity pEntity)
	{
		if (pEntity.GetComponent<getPlayer> ().search)
			return 1.0f;
		return 0.0f;
	}

	public static float SM_Search_Round (AIEntity pEntity)
	{
		if (pEntity.GetComponent<getPlayer> ().search)
			return 0.0f;
		return 1.0f;
	}

	//靠近玩家状态机，距离较远
	public static float SM_Chase_ShieldF (AIEntity pEntity)
	{
		//根据玩家的面朝方向,相隔距离计算
		float sta = pEntity.GetComponent<staComponent> ().staNow / pEntity.GetComponent<staComponent> ().staMax;
		if (sta < 0.1f)
			return 1.0f;//精力过低，必须休息
		Transform p = pEntity.mPlayer.transform;
		Transform a = pEntity.mAI.transform;
		//使用20.0f作为判断阈值
		float dis = Vector3.Distance (p.position, a.position);
		if (dis > 10.0f)
			return 0.0f;//距离过远，无需持盾
		else
			return 1.0f;

	}

	public static float SM_ShieldF_Chase (AIEntity pEntity)
	{
		//其实就是逆向路径函数的取反加1(实际情况会很鬼畜...所以要将他们的值设置的有些许不同）
		Transform p = pEntity.mPlayer.transform;
		Transform a = pEntity.mAI.transform;
		//使用20.0f作为判断阈值
		float dis = Vector3.Distance (p.position, a.position);
		if (dis < 15.0f)
			return 0.0f;//距离过近，需持盾
		float sta = pEntity.GetComponent<staComponent> ().staNow / pEntity.GetComponent<staComponent> ().staMax;
		if (sta > 0.6f)
			return 0.5f;//精力充足，可以跑步接近
		return 0.0f;
	}


	public static float SM_Any_Hook (AIEntity pEntity)
	{
		if (pEntity.GetComponent<shieldAI> ().powerTime >= 1.0f) {
			pEntity.GetComponent<shieldAI> ().powerTime -= Time.deltaTime;
			return 0.0f;
		}
		//设定的话，是可以做到射程为20.0f，使用条件是小于15.0f
		if (pEntity.GetComponent<SSHComponent> ().hasHook == false)
			return 0.0f;//没有钩子
		if (pEntity.GetComponent<shieldAI> ().powerEnd == false)
			return 0.0f;//处于出钩或冲锋状态中
		if (pEntity.GetComponent<staComponent> ().staNow < pEntity.GetComponent<staComponent> ().staPower)
			return 0.0f;// 耐力值不够
		float dis = Vector3.Distance (pEntity.AIPos, pEntity.PlayerPos);
		if (dis > 11.0f && dis < 20.0f) {
			Ray ray = new Ray (pEntity.AIPos, pEntity.PlayerPos - pEntity.AIPos);
			RaycastHit hit;
			//只和障碍物层（对于敌人来说，玩家和障碍物都算是障碍物层）进行判断，并且设置距离为20
			int targetMask = LayerMask.GetMask ("Barrier");

			if (Physics.Raycast (ray, out hit, 20.0f, targetMask)) {
				if (hit.transform.tag == "Player")
					return 0.9f;
				else
					return 0.1f;
			}
		}
		return 0.1f;//可以理解为AI判断失误（的概率^-^
	}

	public static float SM_Any_Charge (AIEntity pEntity)
	{
		if (pEntity.GetComponent<shieldAI> ().powerTime >= 1.0f) {
			pEntity.GetComponent<shieldAI> ().powerTime -= Time.deltaTime;
			return 0.0f;
		}
		if (pEntity.GetComponent<SSHComponent> ().hasCharge == false)
			return 0.0f;//没有冲锋
		if (pEntity.GetComponent<shieldAI> ().powerEnd == false)
			return 0.0f;//处于出钩或冲锋状态中
		if (pEntity.GetComponent<staComponent> ().staNow < pEntity.GetComponent<staComponent> ().staPower)
			return 0.0f;// 耐力不够
		float dis = Vector3.Distance (pEntity.AIPos, pEntity.PlayerPos);
		if (dis > 11.0f && dis < 20.0f) {
			Ray ray = new Ray (pEntity.AIPos, pEntity.PlayerPos - pEntity.AIPos);
			RaycastHit hit;
			//只和障碍物层（对于敌人来说，玩家和障碍物都算是障碍物层）进行判断，并且设置距离为20
			int targetMask = LayerMask.GetMask ("Barrier");

			if (Physics.Raycast (ray, out hit, 20.0f, targetMask)) {
				if (hit.transform.tag == "Player")
					return 0.9f;
				else
					return 0.1f;
			}
		}
		return 0.1f;//可以理解为AI判断失误的概率^-^
	}

	public static float SM_Charge_ShieldF (AIEntity pEntity)
	{
		if (pEntity.GetComponent<shieldAI> ().powerEnd)
			return 1.0f;
		else
			return 0.0f;
	}

	public static float SM_Hook_ShieldF (AIEntity pEntity)
	{
		return SM_Charge_ShieldF (pEntity);
	}


	//攻击状态机，距离较近
	public static float SM_ShieldS_Attack (AIEntity pEntity)
	{
		float staNow = pEntity.GetComponent<staComponent> ().staNow;
		float staAttack = pEntity.GetComponent<staComponent> ().staAttack;
		float staMax = pEntity.GetComponent<staComponent> ().staMax;
		float dis = Vector3.Distance (pEntity.AIPos, pEntity.PlayerPos);
		if (dis > 0.5)//距离不在攻击范围内，攻击无用
			return 0.0f;
		// 攻击过后，剩余耐力越多，攻击可能越大
		float value = Mathf.Max (0.0f, (staNow - staAttack * 1.5f) / (staMax - staAttack * 1.5f));
		if (pEntity.GetComponent<AIEmotion> ().mtempName == "Excited") {
			return value * 1.5f;//为兴奋状态时，攻击可能性获得加强
		}

		return value;
		
	}

	public static float SM_Attack_ShieldS (AIEntity pEntity)
	{
		// 播完动画进入停止
		if (pEntity.GetComponent<shieldAI> ().attackfinish)
			return 1.0f;
		return 0.0f;
		
	}

	// 攻击状态机，持盾接近到持盾静止
	public static float SM_ShieldF_ShieldS (AIEntity pEntity)
	{
		float staNow = pEntity.GetComponent<staComponent> ().staNow;
		float staAttack = pEntity.GetComponent<staComponent> ().staAttack;
		float staMax = pEntity.GetComponent<staComponent> ().staMax;
		float dis = Vector3.Distance (pEntity.AIPos, pEntity.PlayerPos);
		if (staNow / staMax < 0.1f)// 当耐力值过低时，休息一下
			return 1.0f;
		if (dis > 0.5f)
			return 0.0f;//距离不在攻击范围内，不可停止
		// 距离也够近了，耐力值也够多了
		return 1.0f;
	}

	public static float SM_ShieldS_ShieldF (AIEntity pEntity)
	{
		float staNow = pEntity.GetComponent<staComponent> ().staNow;
		float staAttack = pEntity.GetComponent<staComponent> ().staAttack;
		float staMax = pEntity.GetComponent<staComponent> ().staMax;
		if (staNow / staMax < 0.4f)// 当耐力值过低,休息一会再出发
			return 0.0f;
		float dis = Vector3.Distance (pEntity.AIPos, pEntity.PlayerPos);
		if (dis > 0.8f)
			return 1.0f;//距离不在攻击范围内，接近玩家
		else
			return 0.0f;
	}


	//逃亡，生命值过低
	//理论上说，只要玩家一扭头，马上就跑，跑累了，才考虑举盾格挡的
	public static float SM_ShieldB_Walkaway (AIEntity pEntity)
	{
		//首先是盾牌是否存活QAQ（刚性），与玩家的距离（软性）
		if (!pEntity.GetComponent<SSHComponent> ().hasShield)
			return 1.0f;
		Transform p = pEntity.mPlayer.transform;
		Transform a = pEntity.mAI.transform;
		return 1.0f - Vector3.Angle (p.forward, p.position - a.position) / 180.0f;
	}

	public static float SM_Walkaway_ShieldB (AIEntity pEntity)
	{
		return 1.0f - SM_ShieldB_Walkaway (pEntity);
	}

	public static float SM_Walkaway_Runaway (AIEntity pEntity)
	{
		if (pEntity.GetComponent<AIEmotion> ().mtempName == "Despair") {
			return 0.0f;// 陷入绝望
		}
		float staNow = pEntity.GetComponent<staComponent> ().staNow;
		float staRun = pEntity.GetComponent<staComponent> ().staRun;
		float staMax = pEntity.GetComponent<staComponent> ().staMax;

		if (staNow / staMax > 0.4f)// 耐力值大才考虑跑步
			return 1.0f;
		else
			return 0.0f;
	}

	public static float SM_Runaway_Walkaway (AIEntity pEntity)
	{
		if (pEntity.GetComponent<AIEmotion> ().mtempName == "Despair") {
			return 1.0f;// 陷入绝望
		}
		float staNow = pEntity.GetComponent<staComponent> ().staNow;
		float staRun = pEntity.GetComponent<staComponent> ().staRun;
		float staMax = pEntity.GetComponent<staComponent> ().staMax;

		if (staNow / staMax < 0.1f)// 耐力值小，只能走了
			return 1.0f;
		else// 进行耐力值剩余考验
			return 0.0f;
	}


	// 反馈函数
	// 当这一帧获取的数据比上一帧小时，获得正反馈...
	// 接近玩家
	public static float SM_Near (AIEntity pEntity, bool isNow)
	{
		float dis;
		if (isNow)
			dis = Vector3.Distance (pEntity.AIPos, pEntity.PlayerPos);
		else
			dis = Vector3.Distance (pEntity.GetComponent<SMREComponent> ().AIPos, 
				pEntity.GetComponent<SMREComponent> ().PlayerPos);
		return dis / 40.0f;
	}
	// 远离玩家
	public static float SM_Far (AIEntity pEntity, bool isNow)
	{
		return 1.0f - SM_Near (pEntity, isNow);
	}
	// 耐力值下降
	public static float SM_StaLose (AIEntity pEntity, bool isNow)
	{
		//Debug.Log (pEntity.);
		float staMax;
		float staNow;
		if (isNow) {
			staMax = pEntity.GetComponent<staComponent> ().staMax;
			staNow = pEntity.GetComponent<staComponent> ().staNow;
		} else {
			staMax = pEntity.GetComponent<SMREComponent> ().staMax;
			staNow = pEntity.GetComponent<SMREComponent> ().staNow;
		}

		if (staMax > 0)
			return staNow / staMax;
		//当耐力减少时，返回值减少，获得正反馈（耐力值减少）
		return 0.0f;
	}
	// 耐力值恢复
	public static float SM_StaRecover (AIEntity pEntity, bool isNow)
	{
		return 1.0f - SM_StaLose (pEntity, isNow);




	}
	// 对玩家造成伤害
	public static float SM_HurtUp (AIEntity pEntity, bool isNow)
	{
		float allhp;
		float hp;
		if (isNow) {
			allhp = pEntity.mPlayer.GetComponent<GYTPlayerControl> ().MaxHP;
			hp = pEntity.mPlayer.GetComponent<GYTPlayerControl> ().HP;
		} else {
			allhp = pEntity.GetComponent<SMREComponent> ().MaxHP;
			hp = pEntity.GetComponent<SMREComponent> ().HP;
			
		}
		if (allhp > 0)
			return   hp / allhp;
		else
			return 0.0f;
	}

	//情感
	public static float SM_Despair_Emotion (AIEntity pEntity)
	{
		// 生命值低时，进入绝望状态，此状态只有在逃跑决策中有效...跑步速度降低
		if (pEntity.GetComponent<SSHComponent> ().HPNow / pEntity.GetComponent<SSHComponent> ().HP < 0.1f)
			return 0.9f;
		return 0.0f;
	}

	public static float SM_Normal_Emotion (AIEntity pEntity)
	{
		// 保持一颗平常心
		return 0.4f;
	}

	public static float SM_Angry_Emotion (AIEntity pEntity)
	{
		// 只有盾牌存在时才会进入
		if (pEntity.GetComponent<SSHComponent> ().hasShield) {
			// 盾牌接近损坏时，进入愤怒，速度加强，一般来说，当玩家保持距离对盾牌进行损害时，加强AI速度
			if (pEntity.GetComponent<SSHComponent> ().shieldHPNow / pEntity.GetComponent<SSHComponent> ().shieldHP < 0.5f)
				return 0.8f;
		}
		return 0.0f;
	}

	public static float SM_Excited_Emotion (AIEntity pEntity)
	{
		// 短时间内攻击次数越多，越兴奋
		// 当攻击计数达到3以上时，将其超过的值除以5作为权值计算
		if (pEntity.GetComponent<SSHComponent> ().atkCount > 3.0f)
			return Mathf.Min (1.0f, (pEntity.GetComponent<SSHComponent> ().atkCount - 3f) / 5f);
		return 0.0f;
	}
}