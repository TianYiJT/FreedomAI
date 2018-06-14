using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FreedomAI;
using UnityECS;
using UnityEngine.UI;

//关于自爆兵的动画，记录者，节点，状态连线，心情

public class showAll:UComponent
{
	// 显示节点的画板和几个text组件
	public GameObject mCanvas;
	public GameObject mAction;
	public GameObject mNode;
	public GameObject mEmotion;
	public GameObject mAnimation;
	public bool singleTest = false;
	
}

public class ShowAllSystem:USystem
{
	public override void Init ()
	{
		base.Init ();
		this.AddRequestComponent (typeof(showAll));
	}

	// 要注意到之后的逻辑，在节点中，只靠是否insight来进行判断的
	public override void Update (UEntity uEntity)
	{
		base.Update (uEntity);
		if (uEntity.GetComponent<showAll> ().singleTest) {
			uEntity.GetComponent<showAll> ().mAction.GetComponent<Text> ().text = 
			"Action: " + uEntity.GetComponent<AIStrategy> ().mTempName;
			uEntity.GetComponent<showAll> ().mNode.GetComponent<Text> ().text = 
			"Node: " + uEntity.GetComponent<AIState> ().mTempName;
			uEntity.GetComponent<showAll> ().mEmotion.GetComponent<Text> ().text = 
			"Emotion: " + uEntity.GetComponent<AIEmotion> ().mtempName;
			uEntity.GetComponent<showAll> ().mAnimation.GetComponent<Text> ().text = 
			"Animation: " + uEntity.GetComponent<AIAnimation> ().mtempAnim;
		}
	}
}

// 特点：生命值低，通过跑步接近玩家，速度快难以击中，自爆伤害高。
public class bombAI : UComponent
{

	// 可视化警觉值
	public GameObject RangeObj;

	// 自爆延迟时间
	public float bombDelay = 0.3f;
	// 为了播放完死亡动画的时间
	public float deathDelay = 10.0f;
	// 速度极快
	public float runVel = 4f;
	public float walkVel = 2f;
	// 最多追玩家5秒钟，之后强制自爆
	public float duration = 10.0f;
	public float angryVel = 2f;
	// 自爆半径
	public float range = 5.0f;

	public Vector3 direction;

	//关于巡逻，直接使用圆内设点
	public Vector3 mRoundCenter = Vector3.zero;
	public float RoundSize = 20.0f;
	public Vector3 tempRoundPoint = Vector3.zero;

	// 陷阱师是500，盾兵是1000，很脆的
	public float HP = 300f;
	public float HPNow = 300f;
	// 到达自爆点
	public bool arrive = false;
	// 自爆伤害
	public int damage = 300;
	public GameObject RigHead;

	public bool drawLine = true;
	//由于走路不消耗，特此设置疲劳时间，记住这个是用来求概率值
	public float tiredTime = 60.0f;
	public float ttinit = 60.0f;
	// 每次休息时长
	public float restTime = 5.0f;
	public float rtintit = 5.0f;
	// 处于休息
	public bool rest;
	// 当玩家处于敌人视线或发出声音时，就将insight设置为true，填充只考虑Time.deltatime。。。
}


public class BombHPSystem:USystem{
	public override void Init ()
	{
		base.Init ();
		this.AddRequestComponent (typeof(bombAI));

	}

	public override void Update (UEntity uEntity)
	{
		base.Update (uEntity);
		float headDamage = uEntity.GetComponent<bombAI> ().RigHead.GetComponent<enemyHit> ().damage;
		uEntity.GetComponent<bombAI> ().RigHead.GetComponent<enemyHit> ().damage = 0.0f;
		if (headDamage > 0.0f)
			uEntity.GetComponent<getPlayer> ().engage = true;
		uEntity.GetComponent<bombAI> ().HPNow -= headDamage;
	}

}
// 最后思考了一下，通过目力发现玩家还是写在敌人组件中
public class getPlayer:UComponent
{
	// 交战中
	public bool engage = false;
	// 视线距离
	public float sightRange = 20.0f;
	// 视角为60*2度，左右两边各60度
	public float sightAngle = 60.0f;
	//考虑到声音问题，每当insight为true时，保持0.2s的持续时间，之后设置为false，因为声音的传递只有传入而没有传出...
	public bool insight = false;
	// 重置时间
	public float time = 0.2f;
	// 声音的获取
	public bool getNoise = false;
	// 可疑位置（由玩家传入）
	public Vector3 suspensionPoint = new Vector3 (0.0f, -5.0f, 0.0f);
	// y 为 -5 时，为初始点
	public Vector3 initPoint = new Vector3 (0.0f, -5.0f, 0.0f);
	// 通用属性，警觉所需时间，搜查所需时间
	public float vigilanceTime = 1.5f;
	//初始化用
	public float vtinit = 1.5f;
	public bool vigilance = false;

	public float searchTime = 1.5f;
	public float stinit = 1.5f;
	public bool search = false;
	public GameObject vs;
}

public class GetPlayerSystem:USystem
{
	private float time = 0.0f;
	// 重置时间
	public override void Init ()
	{
		base.Init ();
		this.AddRequestComponent (typeof(getPlayer));
	}

	// 要注意到之后的逻辑，在节点中，只靠是否insight来进行判断的
	public override void Update (UEntity uEntity)
	{
		base.Update (uEntity);
		AIEntity ai = (AIEntity)uEntity;

		float isssi = 0.0f;// 实时更新警觉值半径大小
		if (uEntity.GetComponent<getPlayer> ().engage == false && uEntity.GetComponent<getPlayer> ().vigilance) {
			isssi = uEntity.GetComponent<getPlayer> ().vigilanceTime;
		} else if (uEntity.GetComponent<getPlayer> ().engage == false && uEntity.GetComponent<getPlayer> ().search) {
			isssi = uEntity.GetComponent<getPlayer> ().searchTime + uEntity.GetComponent<getPlayer> ().vtinit;
		}
		// 可视化警觉值
		ai.GetComponent<getPlayer> ().vs.transform.localScale = new Vector3 (isssi / 4, 0.1f, isssi / 4);

		if (uEntity.GetComponent<getPlayer> ().getNoise) {
			uEntity.GetComponent<getPlayer> ().getNoise = false;
			uEntity.GetComponent<getPlayer> ().insight = true;
			uEntity.GetComponent<getPlayer> ().suspensionPoint = ai.PlayerPos;
			time = uEntity.GetComponent<getPlayer> ().time;
			return;
		}
		if (time > 0.0f) {// 没发现一次玩家，就会保持0.2s的警觉时间
			time -= Time.deltaTime;
			return;
		}

		Transform p = ai.mPlayer.transform;
		Transform a = uEntity.GetComponent<BaseAIComponent> ().mAIRT.transform;
		// 超出视线范围，修正insight为false
		if (Vector3.Distance (a.position, p.position) > uEntity.GetComponent<getPlayer> ().sightRange) {
			uEntity.GetComponent<getPlayer> ().insight = false;
			return;
		}
		// 玩家和AI的位置信息
		Vector3 v = ((AIEntity)uEntity).AIPos;
	
		// 判断角度是否符合
		if (Vector3.Angle (a.forward, p.position - a.position) > uEntity.GetComponent<getPlayer> ().sightAngle) {
			uEntity.GetComponent<getPlayer> ().insight = false;
			return;
		}
		// 发出两条射线，分别对应玩家站立、下蹲姿态
		Ray rayh = new Ray (a.position + Vector3.up * 0.8f, p.position - a.position);
		Ray rayl = new Ray (a.position + Vector3.up * 0.5f, p.position - a.position);

		float dis = Vector3.Distance (a.position, p.position);
		dis -= 0.5f;

		RaycastHit hit;
		int targetMask = LayerMask.GetMask ("Barrier");

		// 射线只检测从自身到玩家的长度有没有东西
		if (Physics.Raycast (rayh, out hit, dis, targetMask) &&
		    Physics.Raycast (rayl, out hit, dis, targetMask)) {
			// 只有两个视点都被障碍物挡住了，才是没看到玩家
			uEntity.GetComponent<getPlayer> ().insight = false;
			return;
		} else {
			uEntity.GetComponent<getPlayer> ().insight = true;
			// 更新玩家位置
			uEntity.GetComponent<getPlayer> ().suspensionPoint = ai.PlayerPos;
			time = uEntity.GetComponent<getPlayer> ().time;
			Debug.DrawLine (v + Vector3.up * 0.8f, v + Vector3.up * 0.8f + a.forward * dis, Color.red, 2.0f, true);
			Debug.DrawLine (v + Vector3.up * 0.5f, v + Vector3.up * 0.5f + a.forward * dis, Color.red, 2.0f, true);
			return;
		}
	}
}


public class BombMan
{
	// 对于自爆兵来说，所有状态都只能进入一次QAQ
	// 巡逻休息相关
	public static void IdleEnter (AIEntity pEntity)
	{
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

	// 自爆相关
	public static void DoomEnter (AIEntity pEntity)
	{
	}

	public static void DoomExit (AIEntity pEntity)
	{
	}

	public static float DoomFeedbacker (AIEntity pEntity)
	{
		return 0.0f;
	}

	public static float DoomAction (AIEntity pEntity)
	{
		// 处于交战状态
		if (pEntity.GetComponent<getPlayer> ().engage)
			return 0.7f;
		return 0.0f;
	}
	// 死亡相关
	public static void DeathEnter (AIEntity pEntity)
	{
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
		// 被杀死（记住，自爆延迟后，生命值自动归0）
		if (pEntity.GetComponent<bombAI> ().HPNow < 10)
			return 1.0f;
		return 0.0f;
	}
	//站立不动
	public static void BM_idle_Anim (Animator animator)
	{
		animator.SetBool ("Idle", true);
		animator.SetBool ("Rest", false);

		animator.SetBool ("Walk", false);
		animator.SetBool ("CrazyRun", false);
		animator.SetBool ("ReadyBlow", false);
		animator.SetBool ("Die", false);

		animator.speed = 1.0f;
	}
	// 蹲着休息：舍弃了，因为玩家打不到蹲着的敌人...
	public static void BM_rest_Anim (Animator animator)
	{
		animator.SetBool ("Idle", false);

		animator.SetBool ("Rest", true);

		animator.SetBool ("Walk", false);
		animator.SetBool ("CrazyRun", false);
		animator.SetBool ("ReadyBlow", false);
		animator.SetBool ("Die", false);

		animator.speed = 1.0f;
	}
	// 巡逻
	public static void BM_walk_Anim (Animator animator)
	{
		animator.SetBool ("Idle", false);
		animator.SetBool ("Rest", false);

		animator.SetBool ("Walk", true);

		animator.SetBool ("CrazyRun", false);
		animator.SetBool ("ReadyBlow", false);
		animator.SetBool ("Die", false);

		animator.speed = 1.0f;
	}
	// 警觉，因为系统自身问题，无法在保持静止的状况下转向，所以要微微设置一下警觉的动画
	public static void BM_vigilance_Anim (Animator animator)
	{
		animator.SetBool ("Idle", false);
		animator.SetBool ("Rest", false);

		animator.SetBool ("Walk", true);

		animator.SetBool ("CrazyRun", false);
		animator.SetBool ("ReadyBlow", false);
		animator.SetBool ("Die", false);

		animator.speed = 0.5f;
	}
	// 接近玩家
	public static void BM_crazyrun_Anim (Animator animator)
	{
		animator.SetBool ("Idle", false);
		animator.SetBool ("Rest", false);
		animator.SetBool ("Walk", false);

		animator.SetBool ("CrazyRun", true);

		animator.SetBool ("ReadyBlow", false);
		animator.SetBool ("Die", false);

		animator.speed = 2.0f;// 鬼畜一下...看实际情况
	}
	// 自爆前摇
	public static void BM_readyblow_Anim (Animator animator)
	{
		animator.SetBool ("Idle", false);
		animator.SetBool ("Rest", false);
		animator.SetBool ("Walk", false);
		animator.SetBool ("CrazyRun", false);

		animator.SetBool ("ReadyBlow", true);

		animator.SetBool ("Die", false);

		animator.speed = 5.0f;
	}
	// 死亡
	public static void BM_die_Anim (Animator animator)
	{
		animator.SetBool ("Idle", false);
		animator.SetBool ("Rest", false);
		animator.SetBool ("Walk", false);
		animator.SetBool ("CrazyRun", false);
		animator.SetBool ("ReadyBlow", false);

		animator.SetBool ("Die", true);

		animator.speed = 1.0f;
	}
	// 自爆兵不需要任何记录反馈...除了巡逻以外，其余均是单向路
	public static void BM_Recorder (AIEntity pEntity)
	{
	}

	// 通用型IdleAction行为
	// 巡逻
	public static void BM_Round_Enter (AIEntity pEntity)
	{
		pEntity.GetComponent<bombAI> ().tempRoundPoint = Vector3.zero;
		pEntity.GetComponent<bombAI> ().tiredTime = pEntity.GetComponent<bombAI> ().ttinit;
	}
		
	// 巡逻暂时不使用InfluenceMap，战斗时使用
	public static void BM_Round (AIEntity pEntity)
	{
		
		// 目标值为0 更新
		// 设置当前巡逻点为巡逻中心中的一点
		if (pEntity.GetComponent<bombAI> ().tempRoundPoint == Vector3.zero) {
			Vector2 trandom = Random.insideUnitCircle;
			pEntity.GetComponent<bombAI> ().tempRoundPoint = 
				pEntity.GetComponent<bombAI> ().mRoundCenter +
			(new Vector3 (trandom.x, 0, trandom.y)) *
			pEntity.GetComponent<bombAI> ().RoundSize;
		}

		// 到达目标位置 更新
		if (Vector3.Distance (pEntity.GetComponent<bombAI> ().tempRoundPoint, pEntity.AIPos) < 1.0f) {
			Vector2 trandom = Random.insideUnitCircle;
			pEntity.GetComponent<bombAI> ().tempRoundPoint = 
				pEntity.GetComponent<bombAI> ().mRoundCenter +
			(new Vector3 (trandom.x, 0, trandom.y)) *
			pEntity.GetComponent<bombAI> ().RoundSize;
		}
		Vector3 temp = pEntity.GetComponent<bombAI> ().tempRoundPoint;
		Vector3 tDir = (temp - pEntity.AIPos).normalized;
		pEntity.GetComponent<AIMove> ().mDirection = new Vector3 (tDir.x, 0, tDir.z);
		pEntity.GetComponent<AIMove> ().mVelocity = pEntity.GetComponent<bombAI> ().walkVel;
		pEntity.GetComponent<AIMove> ().mMoveFunc = MoveFunc.Complex;
		pEntity.GetComponent<bombAI> ().tiredTime -= Time.deltaTime;// 疲劳时间

	}

	public static void BM_Round_Exit (AIEntity pEntity)
	{
	}

	// 休息
	public static void BM_Rest_Enter (AIEntity pEntity)
	{
		pEntity.GetComponent<bombAI> ().restTime = pEntity.GetComponent<bombAI> ().rtintit;
		// 由于是通过巡逻时间增加概率，所以bool设置需要在这里
		pEntity.GetComponent<bombAI> ().rest = true;
		pEntity.GetComponent<AIMove> ().mVelocity = 0.0f;
	}

	public static void BM_Rest (AIEntity pEntity)
	{
		if (pEntity.GetComponent<bombAI> ().restTime > 0.0f)// 没休息完
			pEntity.GetComponent<bombAI> ().restTime -= Time.deltaTime;
		else// 休息完毕
			pEntity.GetComponent<bombAI> ().rest = false;
	}

	public static void BM_Rest_Exit (AIEntity pEntity)
	{
	}

	// 警觉
	public static void BM_Vigilance_Enter (AIEntity pEntity)
	{
		// 初始化警觉时间
		pEntity.GetComponent<getPlayer> ().vigilanceTime = pEntity.GetComponent<getPlayer> ().vtinit;
		pEntity.GetComponent<getPlayer> ().search = false;
		pEntity.GetComponent<getPlayer> ().vigilance = true;
	}

	public static void BM_Vigilance (AIEntity pEntity)
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

		// 保持朝向为可疑位置，速度为0.1
		Vector3 tDir = pEntity.GetComponent<getPlayer> ().suspensionPoint - pEntity.AIPos;
		pEntity.GetComponent<AIMove> ().mDirection = tDir.normalized;
		pEntity.GetComponent<AIMove> ().mVelocity = 0.1f;
		
	}

	public static void BM_Vigilance_Exit (AIEntity pEntity)
	{
	}

	// 搜查
	public static void BM_Search_Enter (AIEntity pEntity)
	{
		// 初始化搜查时间
		pEntity.GetComponent<getPlayer> ().searchTime = pEntity.GetComponent<getPlayer> ().stinit;
		pEntity.GetComponent<getPlayer> ().vigilance = false;
		pEntity.GetComponent<getPlayer> ().search = true;// 这里不写也行
	}

	public static void BM_Search (AIEntity pEntity)
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

		if (Vector3.Distance (pEntity.AIPos, pEntity.GetComponent<getPlayer> ().suspensionPoint) < 1.0f && pEntity.GetComponent<getPlayer> ().insight == false) {//到达目的地，减少
			pEntity.GetComponent<getPlayer> ().searchTime -= Time.deltaTime;
			pEntity.GetComponent<AIMove> ().mVelocity = 0.1f;
		} else {
			pEntity.GetComponent<AIMove> ().mVelocity = pEntity.GetComponent<bombAI> ().walkVel;
			
		}

		// 保持朝向为可疑位置，速度为走步速度
		Vector3 tDir = pEntity.GetComponent<getPlayer> ().suspensionPoint - pEntity.AIPos;
		pEntity.GetComponent<AIMove> ().mDirection = tDir.normalized;
		pEntity.GetComponent<AIMove> ().mMoveFunc = MoveFunc.Complex;
	}

	public static void BM_Search_Exit (AIEntity pEntity)
	{
	}

	//Enter：进入状态时的操作；类似于Update函数；Exit：退出状态时的操作
	public static void BM_Crazyrun_Enter (AIEntity pEntity)
	{
		//Debug.Log ("CrazyRun");
	}

	public static void BM_Crazyrun (AIEntity pEntity)
	{
		if (Vector3.Distance (pEntity.AIPos, pEntity.PlayerPos) < 0.5f) {// 若已接近玩家
			pEntity.GetComponent<bombAI> ().arrive = true;
		}
		if (pEntity.GetComponent<bombAI> ().duration > 0.0f) {
			// 若还没到强制自爆时间
			Vector3 tDir = pEntity.PlayerPos - pEntity.AIPos;
			pEntity.GetComponent<AIMove> ().mDirection = tDir.normalized;
			pEntity.GetComponent<AIMove> ().mVelocity = pEntity.GetComponent<bombAI> ().runVel;
			// 当生命值较低时，进入愤怒状态，移动速度加快
			if (pEntity.GetComponent<AIEmotion> ().GetTempEmotion () == "Angry")
				pEntity.GetComponent<AIMove> ().mVelocity += pEntity.GetComponent<bombAI> ().angryVel;
			pEntity.GetComponent<AIMove> ().mMoveFunc = MoveFunc.Complex;	//设置寻路方式
			pEntity.GetComponent<bombAI> ().duration -= Time.deltaTime;
		} else {
			pEntity.GetComponent<bombAI> ().arrive = true;
		}

	}

	public static void BM_Crazyrun_Exit (AIEntity pEntity)
	{
		// Nooo...要炸了！
		pEntity.GetComponent<AIMove> ().mVelocity = 0.0f;
	}

	// 自爆前摇，结束后，将生命值归零
	public static void BM_Delay_Enter (AIEntity pEntity)
	{
		//Debug.Log ("Delay");
	}

	public static void BM_Delay (AIEntity pEntity)
	{
		// 判断倒计时
		if (pEntity.GetComponent<bombAI> ().bombDelay > 0.0f) {
			pEntity.GetComponent<bombAI> ().bombDelay -= Time.deltaTime;
		} else {
			pEntity.GetComponent<bombAI> ().HPNow = 0;
		}
			
	}

	public static void BM_Delay_Exit (AIEntity pEntity)
	{

	}

	// 有趣的是，死亡时，必定自爆的...
	public static void BM_Boom_Enter (AIEntity pEntity)
	{
		//Debug.Log ("Boom");
		Transform p = pEntity.mPlayer.transform;
		Transform a = pEntity.GetComponent<BaseAIComponent> ().mAIRT.transform;
		Ray rayh = new Ray (a.position + Vector3.up * 0.8f, p.position - a.position + Vector3.up * 0.8f);
		Ray rayl = new Ray (a.position + Vector3.up * 0.5f, p.position - a.position + Vector3.up * 0.5f);
		RaycastHit hit;
		int targetMask = LayerMask.GetMask ("Barrier");
		if (Physics.Raycast (rayh, out hit, pEntity.GetComponent<getPlayer> ().sightRange, targetMask) &&
		    Physics.Raycast (rayl, out hit, pEntity.GetComponent<getPlayer> ().sightRange, targetMask)) {
			// 只有两个视点都被障碍物挡住了，才是没炸到玩家
			pEntity.mPlayer.GetComponent<HPComponent> ().tempHP -= pEntity.GetComponent<bombAI> ().damage;
			//Debug.Log ("bombdamage!");
		}
	}

	public static void BM_Boom (AIEntity pEntity)
	{
		if (pEntity.GetComponent<bombAI> ().deathDelay > 0.0f) {
			pEntity.GetComponent<bombAI> ().deathDelay -= Time.deltaTime;
		} else {
			GameObject.Destroy (pEntity.GetComponent<BaseAIComponent> ().mAIRT);
			ECSWorld.MainWorld.deleteEntity (pEntity);
			
		}
	}

	public static void BM_Boom_Exit (AIEntity pEntity)
	{
	}

	// 巡逻转换（基本通用）
	public static float BM_Round_Rest (AIEntity pEntity)
	{
		// 随着巡逻时间加长，t逐渐减少
		float t = pEntity.GetComponent<bombAI> ().tiredTime;
		float tmax = pEntity.GetComponent<bombAI> ().ttinit;
		return Mathf.Min (1.0f, (tmax - t) / tmax);
	}

	public static float BM_Rest_Round (AIEntity pEntity)
	{
		// 休息够了
		if (pEntity.GetComponent<bombAI> ().rest == false)
			return 1.0f;
		return 0.0f;
	}

	public static float BM_Rest_Vigilance (AIEntity pEntity)
	{
		//Debug.Log (pEntity.ToString());
		if (pEntity.GetComponent<getPlayer> ().insight)
			return 1.0f;
		return 0.0f;
	}

	public static float BM_Round_Vigilance (AIEntity pEntity)
	{
		if (pEntity.GetComponent<getPlayer> ().insight)
			return 1.0f;
		return 0.0f;
	}

	public static float BM_Vigilance_Round (AIEntity pEntity)
	{
		if (pEntity.GetComponent<getPlayer> ().vigilance)
			return 0.0f;
		return 1.0f;
	}

	public static float BM_Vigilance_Search (AIEntity pEntity)
	{
		if (pEntity.GetComponent<getPlayer> ().search)
			return 1.0f;
		return 0.0f;
	}

	public static float BM_Search_Round (AIEntity pEntity)
	{
		if (pEntity.GetComponent<getPlayer> ().search)
			return 0.0f;
		return 1.0f;
	}

	public static float BM_Crazyrun_Delay (AIEntity pEntity)
	{
		if (pEntity.GetComponent<bombAI> ().arrive)
			return 1.0f;
		return 0.0f;
	}

	public static float BM_Angry_Emotion (AIEntity pEntity)
	{
		if (pEntity.GetComponent<bombAI> ().HPNow / pEntity.GetComponent<bombAI> ().HP < 0.3f)
			return 1.0f;
		return 0.1f;
	}

	public static float BM_Normal_Emotion (AIEntity pEntity)
	{
		return 0.5f;
	}

}