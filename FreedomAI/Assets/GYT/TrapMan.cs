using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FreedomAI;
using UnityECS;

//// 陷阱师


// 最多持有一个，随时间恢复，最多扔三个（在整个生命周期）
public class trapComponent:UComponent
{
	public int trapNow = 0;
	//初始陷阱球数
	public float trapRate = 10.0f;
	// 10s恢复一个陷阱球
	public int trapCount = 1;
	// 最多陷阱数
}

public class TrapSystem:USystem
{
	public override void Init ()
	{
		base.Init ();
		this.AddRequestComponent (typeof(trapComponent));
	}

	public override void Update (UEntity uEntity)
	{
		base.Update (uEntity);
		if (uEntity.GetComponent<trapComponent> ().trapNow == 1)
			return;
		if (uEntity.GetComponent<trapComponent> ().trapRate >= 0.0f)
			uEntity.GetComponent<trapComponent> ().trapRate -= Time.deltaTime;
		else {
			uEntity.GetComponent<trapComponent> ().trapCount--;
			uEntity.GetComponent<trapComponent> ().trapNow = 1;
			uEntity.GetComponent<trapComponent> ().trapRate = 10.0f;

		}
	}
}

public class trapAI:UComponent
{
	// 寻找掩体时，最多寻找的时间长度
	public float runTime = 3.0f;
	public float fireRate = 1.0f;
	// 预先存储的掩体点位，暂时设置为初始掩体与当时自己所处位置的中点
	public Vector3 coverPosA = new Vector3 (0, 0, 0);
	public Vector3 coverPosB = new Vector3 (0, 0, 0);
	// 表示处于玩家火力之下的掩体点位
	public Vector3 coverNow = new Vector3 (0, 0, 0);
	// 射击频率
	public float fireRateInit = 2.0f;
	public GameObject RigHead;
	public float healRate = 0.01f;
	// 回血速率百分比
	// 播放死亡动画后，3秒删除
	public float deathTime = 3.0f;
	//跑步、走步、潜行、受伤(减少)、激怒(加成)
	public float runVel = 4.0f;
	public float walkVel = 2.0f;
	public float sneakVel = 1.0f;
	public float hurtVel = (-2.0f);
	public float angryVel = (+2.0f);

	// 开始瞄准/射击 玩家时，瞄准线宽度
	public float startWidth = 0.1f;
	public float endWidth = 0.01f;
	public float bulletVel = 10.0f;

	public float waryTime = 0.0f;
	// 摸鱼时间

	//关于巡逻，直接使用圆内设点
	public Vector3 mRoundCenter = Vector3.zero;
	public float RoundSize = 20.0f;
	public Vector3 tempRoundPoint = Vector3.zero;

	public GameObject trapBallObj;
	// 陷阱球

	public GameObject shootPoint;
	public GameObject bulletObj;
	public GameObject barrierObj;

	// 最多放三个陷阱，一堵墙
	public bool barrierHave = true;

	// 射击完了，瞄准完了，休息完了，找到掩体了
	public bool fired = false;
	public bool aimFinish = false;
	public bool waryFinish = false;
	public bool covered = false;
	// 瞄准完毕，准备射击
	//生命，当前生命
	public float HP = 500.0f;
	public float HPNow = 500.0f;



	//由于走路不消耗，特此设置疲劳时间，记住这个是用来求概率值
	public float tiredTime = 60.0f;
	public float ttinit = 60.0f;
	public float restTime = 5.0f;
	// 每次休息时长
	public float rtintit = 5.0f;
	public bool rest = false;

	public AStarComputerformula mAStarComputerformula;

	public Stack<Vector3> mTempTarget = new Stack<Vector3> ();

	public Vector3 mTempAStarNode = Vector3.zero;

}

public class TrapHPSystem:USystem
{
	public override void Init ()
	{
		base.Init ();
		this.AddRequestComponent (typeof(trapAI));

	}

	public override void Update (UEntity uEntity)
	{
		base.Update (uEntity);
		float headDamage = uEntity.GetComponent<trapAI> ().RigHead.GetComponent<enemyHit> ().damage;
		uEntity.GetComponent<trapAI> ().RigHead.GetComponent<enemyHit> ().damage = 0.0f;
		if (headDamage > 0.0f)
			uEntity.GetComponent<getPlayer> ().engage = true;
		uEntity.GetComponent<trapAI> ().HPNow -= headDamage;
	}

}
// 绘制瞄准时的红色线
public class redLazer:UComponent
{
	//瞄准线由宽变细，到达endwidth时，发射出子弹
	public float startWidth = 0.1f;
	public float endWidth = 0.01f;
}

public class TrapMan
{


	// 决策姬
	// 巡逻休息相关
	public static void IdleEnter (AIEntity pEntity)
	{
//		Debug.Log ("TMIdle up");
		pEntity.GetComponent<getPlayer> ().engage = false;
		pEntity.GetComponent<getPlayer> ().search = false;
		pEntity.GetComponent<getPlayer> ().vigilance = false;
		pEntity.GetComponent<trapAI> ().HP = pEntity.GetComponent<trapAI> ().HP * 0.1f +
		pEntity.GetComponent<trapAI> ().HPNow;

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
		return 0.5f;
	}


	// 攻击玩家
	public static void AttackEnter (AIEntity pEntity)
	{
//		Debug.Log ("TMAttack up");
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
		//距离小，处于交战
		if (Vector3.Distance (pEntity.AIPos, pEntity.PlayerPos) < 20.0f &&
		    pEntity.GetComponent<getPlayer> ().engage)
			return 0.6f;
		return 0.0f;
	}

	// 逃离玩家
	public static void EscapeEnter (AIEntity pEntity)
	{
//		Debug.Log ("TMEscape up");
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
		// 生命值小于百分之三十就跑
		if (pEntity.GetComponent<trapAI> ().HPNow < pEntity.GetComponent<trapAI> ().HP * 0.3f &&
		    Vector3.Distance (pEntity.AIPos, pEntity.PlayerPos) < 20.0f)
			return 0.7f;
		return 0.0f;
	}

	// 封锁玩家
	public static void BlockEnter (AIEntity pEntity)
	{

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
		return 0.4f;
	}

	// 死亡
	public static void DeathEnter (AIEntity pEntity)
	{
//		Debug.Log ("TMDeath up");
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
		if (pEntity.GetComponent<trapAI> ().HPNow < 10)
			return 0.9f;
		return 0.0f;
	}
	// 不需要耐力组件

	// 动画组件
	public static void TM_idle_Anim (Animator animator)
	{
		animator.SetBool ("Idle", true);

		animator.SetBool ("Walk", false);
		animator.SetBool ("Sneak", false);
		animator.SetBool ("Run", false);
		animator.SetBool ("Wary", false);
		animator.SetBool ("Aim", false);
		animator.SetBool ("Fire", false);
		animator.SetBool ("Die", false);

		animator.speed = 1.0f;
	}

	public static void TM_Walk_Anim (Animator animator)
	{
		animator.SetBool ("Idle", false);

		animator.SetBool ("Walk", true);

		animator.SetBool ("Sneak", false);
		animator.SetBool ("Run", false);
		animator.SetBool ("Wary", false);
		animator.SetBool ("Aim", false);
		animator.SetBool ("Fire", false);
		animator.SetBool ("Die", false);

		animator.speed = 1.0f;
	}

	public static void TM_Sneak_Anim (Animator animator)
	{
		animator.SetBool ("Idle", false);
		animator.SetBool ("Walk", false);

		animator.SetBool ("Sneak", true);

		animator.SetBool ("Run", false);
		animator.SetBool ("Wary", false);
		animator.SetBool ("Aim", false);
		animator.SetBool ("Fire", false);
		animator.SetBool ("Die", false);

		animator.speed = 1.0f;
	}

	public static void TM_Run_Anim (Animator animator)
	{
		animator.SetBool ("Idle", false);
		animator.SetBool ("Walk", false);
		animator.SetBool ("Sneak", false);

		animator.SetBool ("Run", true);

		animator.SetBool ("Wary", false);
		animator.SetBool ("Aim", false);
		animator.SetBool ("Fire", false);
		animator.SetBool ("Die", false);

		animator.speed = 1.0f;
	}
	// 蹲着休息
	public static void TM_Wary_Anim (Animator animator)
	{
		animator.SetBool ("Idle", false);

		animator.SetBool ("Walk", false);
		animator.SetBool ("Sneak", false);
		animator.SetBool ("Run", false);

		animator.SetBool ("Wary", true);

		animator.SetBool ("Aim", false);
		animator.SetBool ("Fire", false);
		animator.SetBool ("Die", false);

		animator.speed = 1.0f;
	}

	public static void TM_Aim_Anim (Animator animator)
	{
		animator.SetBool ("Idle", false);
		animator.SetBool ("Walk", false);
		animator.SetBool ("Sneak", false);
		animator.SetBool ("Run", false);
		animator.SetBool ("Wary", false);

		animator.SetBool ("Aim", true);

		animator.SetBool ("Fire", false);
		animator.SetBool ("Die", false);

		animator.speed = 1.0f;
	}

	public static void TM_Fire_Anim (Animator animator)
	{
		animator.SetBool ("Idle", false);
		animator.SetBool ("Walk", false);
		animator.SetBool ("Sneak", false);
		animator.SetBool ("Run", false);
		animator.SetBool ("Wary", false);
		animator.SetBool ("Aim", false);

		animator.SetBool ("Fire", true);

		animator.SetBool ("Die", false);

		animator.speed = 0.1f;
	}

	public static void TM_Die_Anim (Animator animator)
	{
		animator.SetBool ("Idle", false);
		animator.SetBool ("Walk", false);
		animator.SetBool ("Sneak", false);
		animator.SetBool ("Run", false);
		animator.SetBool ("Wary", false);
		animator.SetBool ("Aim", false);
		animator.SetBool ("Fire", false);

		animator.SetBool ("Die", true);

		animator.speed = 1.0f;
	}

	public static void TM_Recorder (AIEntity pEntity)
	{
		//记录上一帧数据，通过一个实体（其中包含AI与player），更新旧数据

		pEntity.GetComponent<AIState> ().LastEntityData.GetComponent<SMREComponent> ().AIPos = pEntity.AIPos;
		pEntity.GetComponent<AIState> ().LastEntityData.GetComponent<SMREComponent> ().PlayerPos = pEntity.PlayerPos;
		pEntity.GetComponent<AIState> ().LastEntityData.GetComponent<SMREComponent> ().MaxHP =
			pEntity.mPlayer.GetComponent<GYTPlayerControl> ().MaxHP;
		pEntity.GetComponent<AIState> ().LastEntityData.GetComponent<SMREComponent> ().HP =
			pEntity.mPlayer.GetComponent<GYTPlayerControl> ().HP;
	}


	// 巡逻
	public static void TM_Round_Enter (AIEntity pEntity)
	{
//		Debug.Log ("TMRound");
		pEntity.GetComponent<trapAI> ().tempRoundPoint = Vector3.zero;
		pEntity.GetComponent<trapAI> ().tiredTime = pEntity.GetComponent<trapAI> ().ttinit;
		pEntity.GetComponent<trapAI> ().mTempTarget.Clear ();
		pEntity.GetComponent<trapAI> ().mTempAStarNode = Vector3.zero;
	}


	public static void TM_Round (AIEntity pEntity)
	{
		if (pEntity.GetComponent<trapAI> ().tempRoundPoint == Vector3.zero) {
			Vector2 trandom = Random.insideUnitCircle;
			pEntity.GetComponent<trapAI> ().tempRoundPoint = pEntity.GetComponent<trapAI> ().mRoundCenter + (new Vector3 (trandom.x, 0, trandom.y)) * pEntity.GetComponent<trapAI> ().RoundSize;
		}
		if (Vector3.Distance (pEntity.GetComponent<trapAI> ().tempRoundPoint, pEntity.AIPos) < 1.0f) {
			Vector2 trandom = Random.insideUnitCircle;
			pEntity.GetComponent<trapAI> ().tempRoundPoint = pEntity.GetComponent<trapAI> ().mRoundCenter + (new Vector3 (trandom.x, 0, trandom.y)) * pEntity.GetComponent<trapAI> ().RoundSize;
		}


		Vector3 tDir = pEntity.GetComponent<trapAI> ().tempRoundPoint - pEntity.AIPos;
		pEntity.GetComponent<AIMove> ().mDirection = tDir.normalized;
		pEntity.GetComponent<AIMove> ().mVelocity = pEntity.GetComponent<trapAI> ().walkVel;
		pEntity.GetComponent<AIMove> ().mMoveFunc = MoveFunc.Simple;

		pEntity.GetComponent<trapAI> ().tiredTime -= Time.deltaTime;// 疲劳时间
	}

	public static void TM_Round_Exit (AIEntity pEntity)
	{


	}
	// 休息
	public static void TM_Rest_Enter (AIEntity pEntity)
	{
//		Debug.Log ("TMRest");
		pEntity.GetComponent<trapAI> ().restTime = pEntity.GetComponent<trapAI> ().rtintit;
		// 由于是通过巡逻时间增加概率，所以bool设置需要在这里
		pEntity.GetComponent<trapAI> ().rest = true;
		pEntity.GetComponent<AIMove> ().mVelocity = 0.0f;
	}

	public static void TM_Rest (AIEntity pEntity)
	{
		if (pEntity.GetComponent<trapAI> ().restTime > 0.0f)// 没休息完
			pEntity.GetComponent<trapAI> ().restTime -= Time.deltaTime;
		else// 休息完毕
			pEntity.GetComponent<trapAI> ().rest = false;
	}

	public static void TM_Rest_Exit (AIEntity pEntity)
	{
		
	}

	// 警觉
	public static void TM_Vigilance_Enter (AIEntity pEntity)
	{
		// 初始化警觉时间
//		Debug.Log ("TMVigilance");
		pEntity.GetComponent<getPlayer> ().vigilanceTime = pEntity.GetComponent<getPlayer> ().vtinit;
		pEntity.GetComponent<getPlayer> ().search = false;
		pEntity.GetComponent<getPlayer> ().vigilance = true;
	}

	public static void TM_Vigilance (AIEntity pEntity)
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

	public static void TM_Vigilance_Exit (AIEntity pEntity)
	{
	}
	// 搜查
	public static void TM_Search_Enter (AIEntity pEntity)
	{
		// 初始化搜查时间
//		Debug.Log ("TMSearch");
		pEntity.GetComponent<getPlayer> ().searchTime = pEntity.GetComponent<getPlayer> ().stinit;
		pEntity.GetComponent<getPlayer> ().vigilance = false;
		pEntity.GetComponent<getPlayer> ().search = true;
	}

	public static void TM_Search (AIEntity pEntity)
	{
		if (Vector3.Distance (pEntity.AIPos, pEntity.PlayerPos) < 15.0f) {
			// 如果接近了玩家，直接交战(这里要特别设置，不同兵种触发距离不同)
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
			pEntity.GetComponent<AIMove> ().mVelocity = pEntity.GetComponent<trapAI> ().walkVel;

		}
		// 保持朝向为可疑位置，速度为走路速度
		Vector3 tDir = pEntity.GetComponent<getPlayer> ().suspensionPoint - pEntity.AIPos;
		pEntity.GetComponent<AIMove> ().mDirection = tDir.normalized;
		pEntity.GetComponent<AIMove> ().mVelocity = pEntity.GetComponent<trapAI> ().walkVel;
		pEntity.GetComponent<AIMove> ().mMoveFunc = MoveFunc.Complex;
	}

	public static void TM_Search_Exit (AIEntity pEntity)
	{
	}


	// 在掩体后休息...就是摸鱼
	public static void TM_Wary_Enter (AIEntity pEntity)
	{
//		Debug.Log ("TMWary");
		pEntity.GetComponent<trapAI> ().waryFinish = false;
		pEntity.GetComponent<AIMove> ().mVelocity = 0.0f;
		//随机休息1-3s（其实是给玩家喘气时间）
		pEntity.GetComponent<trapAI> ().waryTime = Random.Range (2.0f, 6.0f);

	}

	public static void TM_Wary (AIEntity pEntity)
	{
		if (pEntity.GetComponent<trapAI> ().waryFinish)
			return;
		if (pEntity.GetComponent<trapAI> ().waryTime > 0.0f) {
			pEntity.GetComponent<trapAI> ().waryTime -= Time.deltaTime;
		} else {
			pEntity.GetComponent<trapAI> ().waryFinish = true;
		}
	}

	public static void TM_Wary_Exit (AIEntity pEntity)
	{
		// 老子休息好了，你们都得死
	}

	// 寻找掩体
	// 先判断有没有掩体，那么就跑向下一个掩体点，中途判断是否进入掩体
	public static void TM_Findcover_Enter (AIEntity pEntity)
	{
//		Debug.Log ("TMFindcover");
		pEntity.GetComponent<trapAI> ().runTime = 3.0f;
		// 进入这个状态，（1）就是没有掩体（2）玩家太近，舍弃掩体
		pEntity.GetComponent<trapAI> ().covered = false;
		Vector3 pA = pEntity.GetComponent<trapAI> ().coverPosA;
		Vector3 pB = pEntity.GetComponent<trapAI> ().coverPosB;
		Ray rayA = new Ray (pA, pEntity.PlayerPos - pA);
		Ray rayB = new Ray (pB, pEntity.PlayerPos - pB);
		RaycastHit hit;
		//只和障碍物层（对于敌人来说，玩家和障碍物都算是障碍物层）进行判断，并且设置距离为20
		int targetMask = LayerMask.GetMask ("Barrier");

		if (Physics.Raycast (rayA, out hit, 20.0f, targetMask)) {
			// 意味着A点是可以的
			pEntity.GetComponent<trapAI> ().coverNow = pA;
//			Debug.Log ("isA");
			return;
		} else if (Physics.Raycast (rayB, out hit, 20.0f, targetMask)) {
			pEntity.GetComponent<trapAI> ().coverNow = pB;
//			Debug.Log ("isB");
			return;
		} else {
			// 两个点都不行绝望...设置为初始值，这表示了，现在只要逃离玩家即可
			Vector3 dis = (pEntity.AIPos - pEntity.PlayerPos).normalized;
			pEntity.GetComponent<trapAI> ().coverNow = pEntity.PlayerPos + 10.0f * dis;
		}
	}

	public static void TM_Findcover (AIEntity pEntity)
	{
		// 从coverPosA、coverPosB中分别射向玩家，判断哪一条射线被阻挡，那么，就跑向那个位置，
		// 此时不处于玩家火力下
		if (pEntity.GetComponent<trapAI> ().runTime < 0.0f) {
			pEntity.GetComponent<trapAI> ().covered = true;
			return;
		} else {
			pEntity.GetComponent<trapAI> ().runTime -= Time.deltaTime;
		}

		//Debug.Log (pEntity.GetComponent<trapAI> ().coverNow.ToString());
		if (Vector3.Distance (pEntity.GetComponent<trapAI> ().coverNow, pEntity.AIPos) > 1.0f) {
			pEntity.GetComponent<AIMove> ().mDirection = pEntity.GetComponent<trapAI> ().coverNow - pEntity.AIPos;
			pEntity.GetComponent<AIMove> ().mVelocity = pEntity.GetComponent<trapAI> ().runVel;
			if (pEntity.GetComponent<AIEmotion> ().mtempName == "Despair") {
				pEntity.GetComponent<AIMove> ().mVelocity *= 0.5f;
			}
			pEntity.GetComponent<AIMove> ().mMoveFunc = MoveFunc.Complex;
		} else {
			pEntity.GetComponent<trapAI> ().covered = true;
			//Debug.Log ("Finish Find");
		}
	}

	public static void TM_Findcover_Exit (AIEntity pEntity)
	{

	}

	// 是的，很蛋疼...
	public static void TM_Aim_Enter (AIEntity pEntity)
	{
//		Debug.Log ("TMAim");
		pEntity.GetComponent<trapAI> ().fireRate = pEntity.GetComponent<trapAI> ().fireRateInit;
		if (pEntity.GetComponent<AIEmotion> ().mtempName == "Angry") {
			pEntity.GetComponent<trapAI> ().fireRate *= 0.5f;
		}
		pEntity.GetComponent<trapAI> ().aimFinish = false;
	}

	public static void TM_Aim (AIEntity pEntity)
	{
		if (pEntity.GetComponent<trapAI> ().fireRate > 0.0f) {
			// 若没进入射击时间
			pEntity.GetComponent<trapAI> ().fireRate -= Time.deltaTime;
			Vector3 dir = pEntity.PlayerPos - pEntity.AIPos;

			pEntity.GetComponent<AIMove> ().mDirection = dir.normalized;
			pEntity.GetComponent<AIMove> ().mVelocity = 0;
			pEntity.GetComponent<AIMove> ().mMoveFunc = MoveFunc.JUSTROTATE;

		} else {
			pEntity.GetComponent<trapAI> ().aimFinish = true;
		}
	}

	public static void TM_Aim_Exit (AIEntity pEntity)
	{
	}

	// 射击，确定方向，初始化射击时间
	public static void TM_Fire_Enter (AIEntity pEntity)
	{
//		Debug.Log ("TMFire");
		pEntity.GetComponent<trapAI> ().fired = false;
		pEntity.GetComponent<trapAI> ().fireRate = -1f;
		Vector3 dir = pEntity.PlayerPos - pEntity.AIPos;

		pEntity.GetComponent<AIMove> ().mDirection = dir.normalized;
		pEntity.GetComponent<AIMove> ().mVelocity = 0;
		pEntity.GetComponent<AIMove> ().mMoveFunc = MoveFunc.JUSTROTATE;
	}

	// 实时更新：弹道方向、宽度、面朝方向
	public static void TM_Fire (AIEntity pEntity)
	{
		// 需要进行一定的预判，不然很难打中
		if (pEntity.GetComponent<trapAI> ().fired)
			return;//就射一发，巴雷特警告！
		if (pEntity.GetComponent<trapAI> ().fireRate < 0.0f) {
			pEntity.GetComponent<trapAI> ().shootPoint.GetComponent<GytFirePos> ().shoot = true;
			pEntity.GetComponent<trapAI> ().shootPoint.GetComponent<GytFirePos> ().nextOne = 0.1f;
			pEntity.GetComponent<trapAI> ().fireRate = 1.0f;
		} else {
			pEntity.GetComponent<trapAI> ().fireRate -= Time.deltaTime;
			if (pEntity.GetComponent<trapAI> ().fireRate < 0.1f) {
				pEntity.GetComponent<trapAI> ().fired = true;
				pEntity.GetComponent<trapAI> ().shootPoint.GetComponent<GytFirePos> ().shoot = false;
			}
				
		}
	}


	public static void TM_Fire_Exit (AIEntity pEntity)
	{

	}

	// 设置陷阱
	public static void TM_Trap_Enter (AIEntity pEntity)
	{
//		Debug.Log ("TMTrap");

	}

	public static void TM_Trap (AIEntity pEntity)
	{
		if (pEntity.GetComponent<trapComponent> ().trapNow == 0)
			return;
		// 设置陷阱球的终止位置，设置为从陷阱师出发一段距离
		Vector3 dir = pEntity.AIPos - pEntity.PlayerPos;
		GameObject trapBall = GameObject.Instantiate (pEntity.GetComponent<trapAI> ().trapBallObj,
			                      pEntity.AIPos + Vector3.up * 1.0f, pEntity.GetComponent<BaseAIComponent> ().mAIRT.transform.rotation);
		//设置最终位置为陷阱师和玩家之间的一个位置
		trapBall.GetComponent<TrapBallController> ().endPos = pEntity.PlayerPos + dir * 0.3f;
		trapBall.GetComponent<TrapBallController> ().active = true;

		//各种操作...需要补全
		pEntity.GetComponent<trapComponent> ().trapNow = 0;
	}

	public static void TM_Trap_Exit (AIEntity pEntity)
	{

	}

	// 设置障碍物
	public static void TM_Barrier_Enter (AIEntity pEntity)
	{
//		Debug.Log ("TMBarrier");

	}

	public static void TM_Barrier (AIEntity pEntity)
	{
		// 在与玩家之间生成一堵半高掩体
		if (pEntity.GetComponent<trapAI> ().barrierHave == false)
			return;
		// 首先把头部转向玩家，以免在生成掩体时角度不对
		Vector3 dir = pEntity.PlayerPos - pEntity.AIPos;
		pEntity.GetComponent<AIMove> ().mMoveFunc = MoveFunc.JUSTROTATE;
		pEntity.GetComponent<AIMove> ().mDirection = dir.normalized;
		GameObject tbarrier = GameObject.Instantiate (pEntity.GetComponent<trapAI> ().barrierObj, 
			                      pEntity.AIPos + dir.normalized * 1.0f,
			                      pEntity.GetComponent<BaseAIComponent> ().mAIRT.transform.rotation);
		CollisionEntity tEntity = new CollisionEntity ();
		tEntity.mGameObject = tbarrier;
		ECSWorld.MainWorld.registerEntityAfterInit (tEntity);
		tEntity.Init ();
		//	tEntity.GetComponent<InfluenceMapTrigger> ().Init ();
		tbarrier.transform.right = dir.normalized;
		pEntity.GetComponent<trapAI> ().coverPosA = tbarrier.transform.position + dir.normalized * -1.5f;
		pEntity.GetComponent<trapAI> ().coverPosB = tbarrier.transform.position + dir.normalized * 1.5f;		
		pEntity.GetComponent<trapAI> ().barrierHave = false;
	}

	public static void TM_Barrier_Exit (AIEntity pEntity)
	{
		//
	}

	public static void TM_Die_Enter (AIEntity pEntity)
	{
//		Debug.Log ("TMDie");
	}

	public static void TM_Die (AIEntity pEntity)
	{
		if (pEntity.GetComponent<trapAI> ().deathTime > 0.0f) {
			pEntity.GetComponent<trapAI> ().deathTime -= Time.deltaTime;
		} else {
			GameObject.Destroy (pEntity.GetComponent<BaseAIComponent> ().mAIRT);
			ECSWorld.MainWorld.deleteEntity (pEntity);
			
		}
	}

	public static void TM_Die_Exit (AIEntity pEntity)
	{
	}


	// 状态转换
	// 巡逻转换（基本通用）
	public static float TM_Round_Rest (AIEntity pEntity)
	{
		// 随着巡逻时间加长，t逐渐减少
		float t = pEntity.GetComponent<trapAI> ().tiredTime;
		float tmax = pEntity.GetComponent<trapAI> ().ttinit;
		return Mathf.Min (1.0f, (tmax - t) / tmax);
	}

	public static float TM_Rest_Round (AIEntity pEntity)
	{
		// 休息够了
		if (pEntity.GetComponent<trapAI> ().rest == false)
			return 1.0f;
		return 0.0f;
	}

	public static float TM_Rest_Vigilance (AIEntity pEntity)
	{
		if (pEntity.GetComponent<getPlayer> ().insight)
			return 1.0f;
		return 0.0f;
	}

	public static float TM_Round_Vigilance (AIEntity pEntity)
	{
		if (pEntity.GetComponent<getPlayer> ().insight)
			return 1.0f;
		return 0.0f;
	}

	public static float TM_Vigilance_Round (AIEntity pEntity)
	{
		if (pEntity.GetComponent<getPlayer> ().vigilance)
			return 0.0f;
		return 1.0f;
	}

	public static float TM_Vigilance_Search (AIEntity pEntity)
	{
		if (pEntity.GetComponent<getPlayer> ().search)
			return 1.0f;
		return 0.0f;
	}

	public static float TM_Search_Round (AIEntity pEntity)
	{
		if (pEntity.GetComponent<getPlayer> ().search)
			return 0.0f;
		return 1.0f;
	}
	// 通用结束

	public static float TM_Wary_Findcover (AIEntity pEntity)
	{
		
		if (pEntity.GetComponent<trapAI> ().waryFinish)
			return 0.8f;// 休息够了，进入寻找掩体的概率就更大了
		return 0.2f;// 本宝宝还没休息够
	}

	public static float TM_Findcover_Wary (AIEntity pEntity)
	{
		if (pEntity.GetComponent<trapAI> ().covered == false)
			return 0.0f;
		// 在掩体后喘气
		float dis = Vector3.Distance (pEntity.AIPos, pEntity.PlayerPos);
		if (dis < 10.0f)
			return 0.5f;
		if (dis > 20.0f)
			return 1.0f;
		return Mathf.Min (0.5f, (dis - 10) / 10);//也就是说，当玩家跑到10-20距离时，休息几率上升，但是依然警觉（蹲着）
	}

	public static float TM_Barrier_Findcover (AIEntity pEntity)
	{
		// 没有携带掩体直接进入
		if (pEntity.GetComponent<trapAI> ().barrierHave == false)
			return 1.0f;
		return 0.0f;
	}

	public static float TM_Findcover_Aim (AIEntity pEntity)
	{
		//安全距离，射击范围内(40.0f)，距离越远，加权越大
		if (pEntity.GetComponent<trapAI> ().covered == false) {
			return 0.0f;
		}
		float dis = Vector3.Distance (pEntity.AIPos, pEntity.PlayerPos);
		if (dis <= 40.0f)
			return Mathf.Max (0.5f, (dis / 40.0f));
		else
			return 0.0f;
	}

	public static float TM_Aim_Fire (AIEntity pEntity)
	{
		if (pEntity.GetComponent<trapAI> ().aimFinish)
			return 1.0f;//瞄准完毕就射击
		else
			return 0.0f;
	}

	public static float TM_Fire_Findcover (AIEntity pEntity)
	{
		// 射击完就转换
		if (pEntity.GetComponent<trapAI> ().fired)
			return 1.0f;
		return 0.0f;

	}

	public static float TM_Trap_Findcover (AIEntity pEntity)
	{
		// 这个转化主要是判断陷阱有没有布置好...布置好后直接找掩体
		if (pEntity.GetComponent<trapComponent> ().trapNow == 0)
			return 1.0f;
		else
			return 0.0f;
	}

	public static float TM_Findcover_Trap (AIEntity pEntity)
	{
		// 陷阱用光
		if (pEntity.GetComponent<trapComponent> ().trapCount == 0)
			return 0.0f;
		// 陷阱尚未恢复
		if (pEntity.GetComponent<trapComponent> ().trapNow == 0)
			return 0.0f;
		float dis = Vector3.Distance (pEntity.AIPos, pEntity.PlayerPos);
		// 过远，陷阱没有作用
		if (dis > 20.0f)
			return 0.0f;
		// 距离越近，转换可能越大
		return (20.0f - dis) / 20.0f;
	}

	// 反馈函数,当这一帧获取的数据比上一帧小时，获得正反馈...
	// 接近玩家
	public static float TM_Near (AIEntity pEntity, bool isNow)
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
	public static float TM_Far (AIEntity pEntity, bool isNow)
	{
		return 1.0f - TM_Near (pEntity, isNow);
	}

	// 对玩家造成伤害
	public static float TM_HurtUp (AIEntity pEntity, bool isNow)
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

	public static float TM_Angry_Emotion (AIEntity pEntity)
	{
		// 当生气时，强化火力（瞄准时间减少）
		if (pEntity.GetComponent<trapAI> ().HPNow / pEntity.GetComponent<trapAI> ().HP < 0.5f)
			return 1.0f;
		return 0.1f;
	}

	public static float TM_Normal_Emotion (AIEntity pEntity)
	{
		return 0.5f;
	}

	public static float TM_Despair_Emotion (AIEntity pEntity)
	{
		// 生命值低时，进入绝望状态，此状态只有在逃跑决策中有效...跑步速度降低
		if (pEntity.GetComponent<trapAI> ().HPNow / pEntity.GetComponent<trapAI> ().HP < 0.1f)
			return 0.9f;
		return 0.0f;
	}

}
