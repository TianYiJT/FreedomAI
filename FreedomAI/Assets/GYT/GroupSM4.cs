using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityECS;
using FreedomAI;


public class Group4StandardAI:UComponent
{
	// 标准四人方形小队
	// 暂时的设计是，该组件只对发起者有意义
	// 响应者到达指定位置了，所有的节点都可以用到
	public int readyReturn = 0;
	// 当前分配给组员的位置
	public Vector3[] target = new Vector3[3];
	// 归队至多等待时间
	public float returnTime = 10.0f;
	// 重新整理队伍的频率
	public float formAgain = 20.0f;
	// VAN全一致的行动方式
	public Vector3 dir;
	public float vel;
	public MoveFunc move;
}

public class GroupSM4
{

	// 进入
	public static void Enter (AIEntity pSponsor, AIEntity[] pResponsers)
	{
		Debug.Log ("SM4 Enter");
	}

	// 集结概率
	public static float Strategy (AIEntity pSponsor, AIEntity[]pResponsers)
	{
		// 通过距离，当前剩余生命值判断激活水平
		// 现在半径是20.0f球体，在这个范围内随机抓取符合要求的AI
		// 当不处于交战时，才会组队
		float range = 20.0f;// 表示抓取范围
		float minDis = 9999.0f;
		float maxDis = -1.0f;
		float totalDis = 0.0f;// 响应者距离自己的总距离
		float healthy = 0.0f;// 响应者和发起者的健康水平
		healthy = pSponsor.GetComponent<SSHComponent> ().HPNow / pSponsor.GetComponent<SSHComponent> ().HP;
		if (pSponsor.GetComponent<getPlayer> ().engage) {
			return 0.0f;
		}
		for (int i = 0; i < pResponsers.Length; i++) {
			if (pResponsers [i].GetComponent<getPlayer> ().engage) {
				return 0.0f;
			}
			float tDis = Vector3.Distance (pSponsor.AIPos, pResponsers [i].AIPos);
			if (tDis < minDis)
				minDis = tDis;
			if (tDis > maxDis)
				maxDis = tDis;
			totalDis += tDis / range;
			healthy += pResponsers [i].GetComponent<SSHComponent> ().HPNow / pResponsers [i].GetComponent<SSHComponent> ().HP;
		}
		// 响应者距离自己的平均距离的权值为0.7，每个人健康水平的均值权值为0.3
		return (totalDis / 3.0f) * 0.7f + (healthy / 4.0f) * 0.3f;
	}

	// 分配下标
	public static int[] Allocation (AIEntity pSponsor, AIEntity[] pResponsers)
	{
		// 发起者下标默认为0，因此响应者下标从1开始
		int[] ti = new int[pResponsers.Length];
		for (int i = 0; i < pResponsers.Length; i++) {
			ti [i] = i + 1;
		}
		return ti;
	}

	// 解散概率
	public static float Disslove (AIEntity pSponsor, AIEntity[] pResponsers)
	{
		// 先判断与玩家的距离，如果小于5.0f，解散，每个人独立攻击
		float dis = Vector3.Distance (pSponsor.AIPos, pSponsor.PlayerPos);
		if (dis < 1.0f)
			return 1.0f;


		float healthy = 0.0f;
		healthy = pSponsor.GetComponent<SSHComponent> ().HPNow / pSponsor.GetComponent<SSHComponent> ().HP;
		if (healthy < 0.1f) {
			Debug.Log ("SM4 Leader Nearly Death");
			return 0.8f;// 当有单位濒死时
		}
		for (int i = 0; i < pResponsers.Length; i++) {
			healthy = pResponsers [i].GetComponent<SSHComponent> ().HPNow / pResponsers [i].GetComponent<SSHComponent> ().HP;
			if (healthy < 0.1f) {
				Debug.Log ("SM4 Memeber Nearly Death");
				return 0.8f;// 当有单位濒死时
			}
		}
		return 0.0f;// 成员均健康
	}

	// 离开
	public static void Exit (AIEntity pSponsor, AIEntity[] pResponses)
	{
		// 由于群体决策中，一切均以队长为标准，解散时，同步队员与队长数据相同，同步的部分为对应系统中的数据
		Debug.Log ("SM4 Exit");
	
		// 在这里，需要同步耐力组件和交战设置即可。关于生命值，已经自己更新好了
		for (int i = 0; i < pResponses.Length; i++) {
			pResponses [i].GetComponent<staComponent> ().staNow = pSponsor.GetComponent<staComponent> ().staNow;
			pResponses [i].GetComponent<getPlayer> ().engage = pSponsor.GetComponent<getPlayer> ().engage;
			pResponses [i].GetComponent<getPlayer> ().vigilance = pSponsor.GetComponent<getPlayer> ().vigilance;
			pResponses [i].GetComponent<getPlayer> ().search = pSponsor.GetComponent<getPlayer> ().search;
		}
	}

	// 组行为动画机
	public static string Return_Anim (int pid)
	{
		if (pid == 0)
			return "Idle";
		else
			return "Walk";
	}

	public static string Round_Anim (int pid)
	{
		return "Walk";
	}

	public static string Rest_Anim (int pid)
	{
		return "Idle";
	}

	public static string Vigilance_Anim (int pid)
	{
		return "Idle";
	}

	public static string Search_Anim (int pid)
	{
		return "Walk";
	}

	public static string Chase_Anim (int pid)
	{
		return "Run";
	}

	public static string ShieldF_Anim (int pid)
	{
		return "ShieldWalk";
	
	}

	public static string ShieldS_Anim (int pid)
	{
		return "ShieldStop";
	}

	// 组行为节点

	// 所有人员组成队形位置
	public static void Return_Enter (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		// 使自己先停下来
		if (pid == 0)
			Debug.Log ("SM4 Return");
		if (pid == 0) {
			
			pSponsor.GetComponent<AIMove> ().mVelocity = 0.0f; 
			pSponsor.GetComponent<Group4StandardAI> ().readyReturn = 0;
			pSponsor.GetComponent<Group4StandardAI> ().returnTime = 10.0f;
			pSponsor.GetComponent<Group4StandardAI> ().target [0] = pSponsor.AIPos + pSponsor.mAI.transform.forward.normalized * 1f;
			pSponsor.GetComponent<Group4StandardAI> ().target [1] = pSponsor.AIPos + pSponsor.mAI.transform.forward.normalized * 0.5f -
			pSponsor.mAI.transform.right.normalized * 0.5f;
			pSponsor.GetComponent<Group4StandardAI> ().target [2] = pSponsor.AIPos + pSponsor.mAI.transform.forward.normalized * 0.5f +
			pSponsor.mAI.transform.right.normalized * 0.5f;
			pSponsor.GetComponent<SSHComponent> ().back = true;
		} else {
			pResponsers [pid - 1].GetComponent<AIMove> ().mVelocity = 0.0f;
			pResponsers [pid - 1].GetComponent<SSHComponent> ().back = true;
		}
	}

	public static void Return (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		if(pid == 0)
		pSponsor.GetComponent<Group4StandardAI> ().returnTime -= Time.deltaTime;
		if (pid != 0) {
			float dis = Vector3.Distance (pSponsor.GetComponent<Group4StandardAI> ().target [pid - 1], 
				            pResponsers [pid - 1].AIPos);
			// 当接近目标时，设置速度为0
			if (dis < 0.1f) {
				pResponsers [pid - 1].GetComponent<AIMove> ().mVelocity = 0.0f;
				return;
			}
			

			Vector3 dir = pSponsor.GetComponent<Group4StandardAI> ().target [pid - 1] - pResponsers [pid - 1].AIPos;
			pResponsers [pid - 1].GetComponent<AIMove> ().mDirection = dir.normalized;
			pResponsers [pid - 1].GetComponent<AIMove> ().mVelocity = pResponsers [pid - 1].GetComponent<shieldAI> ().walkVel;
			pResponsers [pid - 1].GetComponent<AIMove> ().mMoveFunc = MoveFunc.Complex;
		}

	}

	public static void Return_Exit (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{

	}

	public static void Round_Enter (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		// 以领导者为基准
		if (pid == 0)
			Debug.Log ("SM4 Round");
		if (pid == 0) {
			pSponsor.GetComponent<shieldAI> ().tempRoundPoint = Vector3.zero;
			pSponsor.GetComponent<shieldAI> ().tiredTime = pSponsor.GetComponent<shieldAI> ().ttinit;
			pSponsor.GetComponent<SSHComponent> ().back = true;
			pSponsor.GetComponent<Group4StandardAI> ().formAgain = 20.0f;
		} else {
			pResponsers [pid - 1].GetComponent<SSHComponent> ().back = true;
		}
	}

	public static void Round (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		// 只通过领队生成新的巡逻目标，其他人完全同步运动方式
		if (pid == 0) {
			pSponsor.GetComponent<Group4StandardAI> ().formAgain -= Time.deltaTime;
			//Debug.Log (pSponsor.GetComponent<staComponent> ().staNow);
			if (pSponsor.GetComponent<shieldAI> ().tempRoundPoint == Vector3.zero) {
				Vector2 trandom = Random.insideUnitCircle;
				pSponsor.GetComponent<shieldAI> ().tempRoundPoint = 
				pSponsor.GetComponent<shieldAI> ().mRoundCenter + (new Vector3 (trandom.x, 0, trandom.y)) *
				pSponsor.GetComponent<shieldAI> ().RoundSize;
			}
			if (Vector3.Distance (pSponsor.GetComponent<shieldAI> ().tempRoundPoint, pSponsor.AIPos) < 1.0f) {
				Vector2 trandom = Random.insideUnitCircle;
				pSponsor.GetComponent<shieldAI> ().tempRoundPoint = 
				pSponsor.GetComponent<shieldAI> ().mRoundCenter + (new Vector3 (trandom.x, 0, trandom.y)) *
				pSponsor.GetComponent<shieldAI> ().RoundSize;
			}
			// 更新运动数据
			pSponsor.GetComponent<Group4StandardAI> ().dir = (pSponsor.GetComponent<shieldAI> ().tempRoundPoint - pSponsor.AIPos).normalized;
			pSponsor.GetComponent<Group4StandardAI> ().vel = pSponsor.GetComponent<shieldAI> ().walkVel;
			pSponsor.GetComponent<Group4StandardAI> ().move = MoveFunc.Complex;
		}
		// 分配每个人的位置，由于大家一致行动，所以不用考虑归队问题
		if (pid == 0) {
			pSponsor.GetComponent<AIMove> ().mDirection = pSponsor.GetComponent<Group4StandardAI> ().dir;
			pSponsor.GetComponent<AIMove> ().mVelocity = pSponsor.GetComponent<Group4StandardAI> ().vel;
			pSponsor.GetComponent<AIMove> ().mMoveFunc = pSponsor.GetComponent<Group4StandardAI> ().move;
			pSponsor.GetComponent<staComponent> ().staNow -= pSponsor.GetComponent<staComponent> ().staWalk * Time.deltaTime;
			pSponsor.GetComponent<shieldAI> ().tiredTime -= Time.deltaTime;// 疲劳时间
		} else {
			pResponsers [pid - 1].GetComponent<AIMove> ().mDirection = pSponsor.GetComponent<Group4StandardAI> ().dir;
			pResponsers [pid - 1].GetComponent<AIMove> ().mVelocity = pSponsor.GetComponent<Group4StandardAI> ().vel;
			pResponsers [pid - 1].GetComponent<AIMove> ().mMoveFunc = pSponsor.GetComponent<Group4StandardAI> ().move;
		}
	}

	public static void Round_Exit (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		
	}

	public static void Rest_Enter (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		if (pid == 0)
			Debug.Log ("SM4 Rest");
		if (pid == 0) {
			pSponsor.GetComponent<shieldAI> ().restTime = pSponsor.GetComponent<shieldAI> ().rtintit;
			pSponsor.GetComponent<shieldAI> ().rest = true;
			pSponsor.GetComponent<AIMove> ().mVelocity = 0.0f;
			pSponsor.GetComponent<SSHComponent> ().back = true;
		} else {
			pResponsers [pid - 1].GetComponent<AIMove> ().mVelocity = 0.0f;
			pResponsers [pid - 1].GetComponent<SSHComponent> ().back = true;
		}

	}

	public static void Rest (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		if (pid == 0) {
			if (pSponsor.GetComponent<shieldAI> ().restTime > 0.0f)// 没休息完
				pSponsor.GetComponent<shieldAI> ().restTime -= Time.deltaTime;
			else// 休息完毕
				pSponsor.GetComponent<shieldAI> ().rest = false;
		}
	}

	public static void Rest_Exit (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{

	}

	public static void Vigilance_Enter (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		if (pid == 0)
			Debug.Log ("SM4 Vigilance");
		if (pid == 0) {
			pSponsor.GetComponent<getPlayer> ().vigilanceTime = pSponsor.GetComponent<getPlayer> ().vtinit;
			pSponsor.GetComponent<getPlayer> ().search = false;
			pSponsor.GetComponent<getPlayer> ().vigilance = true;
			pSponsor.GetComponent<AIMove> ().mVelocity = 0.0f;
			pSponsor.GetComponent<AIMove> ().mMoveFunc = MoveFunc.JUSTROTATE;
			pSponsor.GetComponent<SSHComponent> ().back = true;
		} else {
			pResponsers [pid - 1].GetComponent<AIMove> ().mVelocity = 0.0f;
			pResponsers [pid - 1].GetComponent<AIMove> ().mMoveFunc = MoveFunc.JUSTROTATE;
			pResponsers [pid - 1].GetComponent<SSHComponent> ().back = true;
		}
	}

	public static void Vigilance (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		if (pid == 0) {
			// 逻辑：初始警觉值为1倍，玩家在敌人视线内或敌人听到玩家声音就会增加之，加到2倍进入搜查；同理，看不见就会减少警觉值，到0进入巡逻
			float vMax = pSponsor.GetComponent<getPlayer> ().vtinit * 2;
			if (pSponsor.GetComponent<getPlayer> ().vigilanceTime > vMax) {
				// 警觉条充满，进入搜查
				pSponsor.GetComponent<getPlayer> ().search = true;
				return;
			}	
			if (pSponsor.GetComponent<getPlayer> ().vigilanceTime < 0.0f) {
				// 警觉条归零，进入巡逻
				pSponsor.GetComponent<getPlayer> ().vigilance = false;
				return;
			}

			if (pSponsor.GetComponent<getPlayer> ().insight)//视线内，填充
			pSponsor.GetComponent<getPlayer> ().vigilanceTime += Time.deltaTime;
			else//视线外，减少
			pSponsor.GetComponent<getPlayer> ().vigilanceTime -= Time.deltaTime;

			// 保持朝向为可疑位置，速度为0
			pSponsor.GetComponent<Group4StandardAI> ().dir = (pSponsor.GetComponent<getPlayer> ().suspensionPoint - pSponsor.AIPos).normalized;
		}

		// 跟新运动方式，速度一直为0已经在Enter函数中设置好了
		if (pid == 0) {
			pSponsor.GetComponent<AIMove> ().mDirection = pSponsor.GetComponent<Group4StandardAI> ().dir;
		} else {
			pResponsers [pid - 1].GetComponent<AIMove> ().mDirection = pSponsor.GetComponent<Group4StandardAI> ().dir;
		}
	}

	public static void Vigilance_Exit (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{

	}

	public static void Search_Enter (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		if (pid == 0)
			Debug.Log ("SM4 Search");
		if (pid == 0) {
			// 初始化搜查时间
			pSponsor.GetComponent<getPlayer> ().searchTime = pSponsor.GetComponent<getPlayer> ().stinit;
			pSponsor.GetComponent<getPlayer> ().vigilance = false;
			pSponsor.GetComponent<getPlayer> ().search = true;// 这里不写也行
			pSponsor.GetComponent<SSHComponent> ().back = true;// 还没交战，先放在背后
		} else {
			pResponsers [pid - 1].GetComponent<SSHComponent> ().back = true;
		}
	}

	public static void Search (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		if (pid == 0) {

			if (Vector3.Distance (pSponsor.AIPos, pSponsor.PlayerPos) < 2.0f) {
				// 如果接近了玩家，直接交战
				pSponsor.GetComponent<getPlayer> ().engage = true;
				return;
			}
			// 寻路到达可疑位置，与警觉不同的是只有到达可疑位置，才会降低搜查条，然而巡路途中，若玩家在视线中，会增加搜查条的
			float sMax = pSponsor.GetComponent<getPlayer> ().stinit * 2;
			if (pSponsor.GetComponent<getPlayer> ().searchTime > sMax) {
				// 搜查条充满，进入交战
				pSponsor.GetComponent<getPlayer> ().engage = true;
				return;
			}	
			if (pSponsor.GetComponent<getPlayer> ().searchTime < 0.0f) {
				// 搜查条归零，进入巡逻（是的，直接进入巡逻）
				pSponsor.GetComponent<getPlayer> ().search = false;
				return;
			}
			if (pSponsor.GetComponent<getPlayer> ().insight) {//视线内，填充
				pSponsor.GetComponent<getPlayer> ().searchTime += Time.deltaTime;
			}

			if (Vector3.Distance (pSponsor.AIPos, pSponsor.GetComponent<getPlayer> ().suspensionPoint) < 2.0f &&
			    pSponsor.GetComponent<getPlayer> ().insight == false) {//到达目的地且视线内无玩家，减少
				pSponsor.GetComponent<getPlayer> ().searchTime -= Time.deltaTime;
				pSponsor.GetComponent<Group4StandardAI> ().vel = 0.0f;
				pSponsor.GetComponent<Group4StandardAI> ().move = MoveFunc.JUSTROTATE;
			} else {
				// 逐步走向可疑位置
				pSponsor.GetComponent<Group4StandardAI> ().vel = pSponsor.GetComponent<shieldAI> ().walkVel;
				pSponsor.GetComponent<Group4StandardAI> ().move = MoveFunc.Complex;
				pSponsor.GetComponent<Group4StandardAI> ().dir = (pSponsor.GetComponent<getPlayer> ().suspensionPoint - pSponsor.AIPos).normalized;
				pSponsor.GetComponent<staComponent> ().staNow -= pSponsor.GetComponent<staComponent> ().staWalk * Time.deltaTime;
			}
		}
		// 分配每个人的位置，由于大家一致行动，所以不用考虑归队问题
		if (pid == 0) {
			pSponsor.GetComponent<AIMove> ().mDirection = pSponsor.GetComponent<Group4StandardAI> ().dir;
			pSponsor.GetComponent<AIMove> ().mVelocity = pSponsor.GetComponent<Group4StandardAI> ().vel;
			pSponsor.GetComponent<AIMove> ().mMoveFunc = pSponsor.GetComponent<Group4StandardAI> ().move;
		} else {
			pResponsers [pid - 1].GetComponent<AIMove> ().mDirection = pSponsor.GetComponent<Group4StandardAI> ().dir;
			pResponsers [pid - 1].GetComponent<AIMove> ().mVelocity = pSponsor.GetComponent<Group4StandardAI> ().vel;
			pResponsers [pid - 1].GetComponent<AIMove> ().mMoveFunc = pSponsor.GetComponent<Group4StandardAI> ().move;
		}
	}

	public static void Search_Exit (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{

	}

	public static void Chase_Enter (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		if (pid == 0)
			Debug.Log ("SM4 Chase");
		if (pid == 0) {
			pSponsor.GetComponent<SSHComponent> ().back = true;
		} else {
			pResponsers [pid - 1].GetComponent<SSHComponent> ().back = true;
		}
	}

	public static void Chase (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		if (pid == 0) {
			pSponsor.GetComponent<Group4StandardAI> ().dir = (pSponsor.PlayerPos - pSponsor.AIPos).normalized;
			pSponsor.GetComponent<Group4StandardAI> ().vel = pSponsor.GetComponent<shieldAI> ().runVel;
			pSponsor.GetComponent<Group4StandardAI> ().move = MoveFunc.Complex;//simple穿模寻路 complex避障寻路

			pSponsor.GetComponent<staComponent> ().staNow -= pSponsor.GetComponent<staComponent> ().staRun * Time.deltaTime;
		}

		if (pid == 0) {
			pSponsor.GetComponent<AIMove> ().mDirection = pSponsor.GetComponent<Group4StandardAI> ().dir;
			pSponsor.GetComponent<AIMove> ().mVelocity = pSponsor.GetComponent<Group4StandardAI> ().vel;
			pSponsor.GetComponent<AIMove> ().mMoveFunc = pSponsor.GetComponent<Group4StandardAI> ().move;
		} else {
			pResponsers [pid - 1].GetComponent<AIMove> ().mDirection = pSponsor.GetComponent<Group4StandardAI> ().dir;
			pResponsers [pid - 1].GetComponent<AIMove> ().mVelocity = pSponsor.GetComponent<Group4StandardAI> ().vel;
			pResponsers [pid - 1].GetComponent<AIMove> ().mMoveFunc = pSponsor.GetComponent<Group4StandardAI> ().move;
		}
	}

	public static void Chase_Exit (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{

	}

	public static void ShieldF_Enter (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		if (pid == 0)
			Debug.Log ("SM4 ShieldF");
		if (pid == 0) {
			pSponsor.GetComponent<SSHComponent> ().back = false;
		} else {
			pResponsers [pid - 1].GetComponent<SSHComponent> ().back = false;
		}
	}

	public static void ShieldF (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		//Debug.Log (pSponsor.GetComponent<staComponent> ().staNow);
		if (pid == 0) {
			pSponsor.GetComponent<Group4StandardAI> ().dir = (pSponsor.PlayerPos - pSponsor.AIPos).normalized;
			pSponsor.GetComponent<Group4StandardAI> ().move = MoveFunc.Complex;//simple穿模寻路 complex避障寻路
			if (pSponsor.GetComponent<SSHComponent> ().hasShield) {
				pSponsor.GetComponent<Group4StandardAI> ().vel = pSponsor.GetComponent<shieldAI> ().shieldVel;
			} else
				pSponsor.GetComponent<Group4StandardAI> ().vel = pSponsor.GetComponent<shieldAI> ().walkVel;
			pSponsor.GetComponent<staComponent> ().staNow -= pSponsor.GetComponent<staComponent> ().staWalk * Time.deltaTime;
		}

		if (pid == 0) {
			pSponsor.GetComponent<AIMove> ().mDirection = pSponsor.GetComponent<Group4StandardAI> ().dir;
			pSponsor.GetComponent<AIMove> ().mVelocity = pSponsor.GetComponent<Group4StandardAI> ().vel;
			pSponsor.GetComponent<AIMove> ().mMoveFunc = pSponsor.GetComponent<Group4StandardAI> ().move;
		} else {
			pResponsers [pid - 1].GetComponent<AIMove> ().mDirection = pSponsor.GetComponent<Group4StandardAI> ().dir;
			pResponsers [pid - 1].GetComponent<AIMove> ().mVelocity = pSponsor.GetComponent<Group4StandardAI> ().vel;
			pResponsers [pid - 1].GetComponent<AIMove> ().mMoveFunc = pSponsor.GetComponent<Group4StandardAI> ().move;
		}
	}

	public static void ShieldF_Exit (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{

	}

	public static void ShieldS_Enter (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		if (pid == 0) {
			pSponsor.GetComponent<SSHComponent> ().back = false;
			pSponsor.GetComponent<AIMove> ().mVelocity = 0.0f;
			pSponsor.GetComponent<AIMove> ().mMoveFunc = MoveFunc.JUSTROTATE;
		} else {
			pResponsers [pid - 1].GetComponent<SSHComponent> ().back = false;
			pResponsers [pid - 1].GetComponent<AIMove> ().mVelocity = 0.0f;
			pResponsers [pid - 1].GetComponent<AIMove> ().mMoveFunc = MoveFunc.JUSTROTATE;
		}
	}

	public static void ShieldS (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		if (pid == 0)
			Debug.Log ("SM4 ShieldS");
		if (pid == 0) {
			pSponsor.GetComponent<Group4StandardAI> ().dir = (pSponsor.PlayerPos - pSponsor.AIPos).normalized;
		}
		if (pid == 0) {
			pSponsor.GetComponent<AIMove> ().mDirection = pSponsor.GetComponent<Group4StandardAI> ().dir;
		} else {
			pResponsers [pid - 1].GetComponent<AIMove> ().mDirection = pSponsor.GetComponent<Group4StandardAI> ().dir;
		}
	}

	public static void ShieldS_Exit (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{

	}

	// 节点转化连线，只在领导者中调用
	public static float Return2Round (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		// 超过等待时间
		if (pSponsor.GetComponent<Group4StandardAI> ().returnTime < 0.0f) {
			Debug.Log ("return outoftime");
			return 1.0f;
		}
		for (int i = 0; i < pResponsers.Length; i++) {
			// 判断各个响应者是否到达指定位置
			if (Vector3.Distance (pSponsor.GetComponent<Group4StandardAI> ().target [i], pResponsers [i].AIPos) > 1.5f) {
				return 0.0f;
			}
		}
		//Debug.Log ("return all done");
		return 0.8f;
	}

	public static float Round2Return (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		if (pSponsor.GetComponent<Group4StandardAI> ().formAgain < 0.0f)
			return 1.0f;
		return 0.0f;
	}


	public static float Round2Rest (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		float t = pSponsor.GetComponent<shieldAI> ().tiredTime;
		float tmax = pSponsor.GetComponent<shieldAI> ().ttinit;
		return Mathf.Min (1.0f, (tmax - t) / tmax);
	}

	public static float Rest2Round (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		if (pSponsor.GetComponent<shieldAI> ().rest == false)
			return 1.0f;
		return 0.0f;
	}

	public static float Round2Vigilance (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		if (pSponsor.GetComponent<getPlayer> ().insight)
			return 1.0f;
		return 0.0f;
	}

	public static float Vigilance2Round (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		if (pSponsor.GetComponent<getPlayer> ().vigilance)
			return 0.0f;
		return 1.0f;
	}

	public static float Rest2Vigilance (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		if (pSponsor.GetComponent<getPlayer> ().insight)
			return 1.0f;
		return 0.0f;
	}

	public static float Vigilance2Search (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		if (pSponsor.GetComponent<getPlayer> ().search)
			return 1.0f;
		return 0.0f;
	}

	public static float Serach2Round (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		if (pSponsor.GetComponent<getPlayer> ().search)
			return 0.0f;
		return 1.0f;
	}

	public static float Search2Chase (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		if (pSponsor.GetComponent<getPlayer> ().engage)
			return 1.0f;
		return 0.0f;
	}

	public static float Chase2Round (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		if (Vector3.Distance (pSponsor.AIPos, pSponsor.PlayerPos) > 30.0f)
			return 1.0f;
		return 0.0f;
	}

	public static float Chase2ShieldF (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		
		float sta = pSponsor.GetComponent<staComponent> ().staNow / pSponsor.GetComponent<staComponent> ().staMax;
		if (sta < 0.1f)
			return 1.0f;//精力过低，必须休息
		Transform p = pSponsor.mPlayer.transform;
		Transform a = pSponsor.mAI.transform;
		//使用20.0f作为判断阈值
		float dis = Vector3.Distance (pSponsor.AIPos, pSponsor.PlayerPos);
		if (dis > 8.0f)
			return 0.0f;//距离过远，无需持盾
		else
			return 1.0f;
	}

	public static float ShieldF2Chase (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		//其实就是逆向路径函数的取反加1(实际情况会很鬼畜...所以要将他们的值设置的有些许不同）
		//使用20.0f作为判断阈值
		float dis = Vector3.Distance (pSponsor.AIPos, pSponsor.PlayerPos);
		if (dis < 10.0f)
			return 0.0f;//距离过近，需持盾
		float sta = pSponsor.GetComponent<staComponent> ().staNow / pSponsor.GetComponent<staComponent> ().staMax;
		if (sta > 0.3f)
			return 1.0f;//精力充足，可以跑步接近
		return 0.0f;
	}
}


