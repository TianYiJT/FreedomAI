using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityECS;
using FreedomAI;


public class GYTGroupSM3TM1
{
	// 进入
	public static void Enter (AIEntity pSponsor, AIEntity[] pResponsers)
	{
		Debug.Log ("SM3TM1 Enter");
	}

	// 集结概率
	public static float Strategy (AIEntity pSponsor, AIEntity[]pResponsers)
	{
		// 通过距离，当前剩余生命值判断激活水平
		// 现在半径是20.0f球体，在这个范围内随机抓取符合要求的AI
		float range = 20.0f;// 表示抓取范围
		float minDis = 9999.0f;
		float maxDis = -1.0f;
		float totalDis = 0.0f;// 响应者距离自己的总距离
		float healthy = 0.0f;// 响应者和发起者的健康水平
		healthy = pSponsor.GetComponent<trapAI> ().HPNow / pSponsor.GetComponent<trapAI> ().HP;
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
			Debug.Log (pResponsers.Length);
			Debug.Log (pResponsers [i].tag);
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
		// 在这个策略中，一号位是前锋，判断他的距离（而不是四盾中的发起者，在四盾中，谁在前面并没有确定）
		float dis = Vector3.Distance (pResponsers [0].AIPos, pResponsers [0].PlayerPos);
		if (dis < 1.0f)
			return 1.0f;

		// 相较于盾兵，有些许不同
		float healthy = 0.0f;
		healthy = pSponsor.GetComponent<trapAI> ().HPNow / pSponsor.GetComponent<trapAI> ().HP;
		if (healthy < 0.1f) {
			return 0.8f;// 当有单位濒死时
		}
		for (int i = 0; i < pResponsers.Length; i++) {
			healthy = pResponsers [i].GetComponent<SSHComponent> ().HPNow / pResponsers [i].GetComponent<SSHComponent> ().HP;
			if (healthy < 0.1f) {
				return 0.8f;// 当有单位濒死时
			}
		}
		return 0.0f;// 成员均健康
	}

	// 离开
	public static void Exit (AIEntity pSponsor, AIEntity[] pResponses)
	{
		// 由于群体决策中，一切均以队长为标准，解散时，同步队员与队长数据相同，同步的部分为对应系统中的数据
		// 在这里，只需要同步耐力组件即可。关于生命值，已经自己更新好了
		Debug.Log ("SM3TM1 Exit");
		for (int i = 0; i < pResponses.Length; i++) {
			// 以1号位为准
			pResponses [i].GetComponent<staComponent> ().staNow = pResponses [i].GetComponent<staComponent> ().staNow;
		}

		// 由于可能在中途退出，别忘了把队长的枪参数调整好
		pSponsor.GetComponent<trapAI> ().shootPoint.GetComponent<GytFirePos> ().shoot = false;
		pSponsor.GetComponent<trapAI> ().shootPoint.GetComponent<GytFirePos> ().fireRate = 1000.0f;
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
		if (pid == 0)
			return "Wary";
		else
			return "Idle";
	}

	public static string Search_Anim (int pid)
	{
		return "Walk";
	}


	public static string Engage_Anim (int pid)
	{
		if (pid == 0) {
			return "Aim";
		} else
			return "ShieldStop";
	}

	// 行为节点应该可以直接复用SM4中的前半部分（不行...在方法里，组件是不同的，需要进行的设置也不同...），关于Engage节点，单独设计
	// 所有人员组成队形位置
	public static void Return_Enter (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		// 使自己先停下来
		if (pid == 0) 
		Debug.Log ("SM3TM1 Return");
		if (pid == 0) {

			pSponsor.GetComponent<AIMove> ().mVelocity = 0.0f; 
			pSponsor.GetComponent<Group4StandardAI> ().readyReturn = 0;
			pSponsor.GetComponent<Group4StandardAI> ().returnTime = 10.0f;
			// 分配每个人的位置
			GameObject ai = pSponsor.GetComponent<BaseAIComponent> ().mAIRT;
			pSponsor.GetComponent<Group4StandardAI> ().target [0] = ai.transform.position + ai.transform.forward.normalized * 1.0f;
			pSponsor.GetComponent<Group4StandardAI> ().target [1] = ai.transform.position + ai.transform.forward.normalized * 0.5f +
			pSponsor.mAI.transform.right.normalized * 0.5f;
			pSponsor.GetComponent<Group4StandardAI> ().target [2] = ai.transform.position + ai.transform.forward.normalized * 0.5f -
			pSponsor.mAI.transform.right.normalized * 0.5f;
		} else {
			pResponsers [pid - 1].GetComponent<AIMove> ().mVelocity = 0.0f;
			pResponsers [pid - 1].GetComponent<SSHComponent> ().back = true;
		}
	}

	public static void Return (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		if (pid == 0)
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
			if (pid == 1) {
				// 当为1号位，更新耐力，之后也是如此
				pResponsers [pid - 1].GetComponent<staComponent> ().staNow -= pResponsers [pid - 1].GetComponent<staComponent> ().staWalk * Time.deltaTime;
			}
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
		Debug.Log ("SM3TM1 Round");
		if (pid == 0) {
			pSponsor.GetComponent<trapAI> ().tempRoundPoint = Vector3.zero;
			pSponsor.GetComponent<trapAI> ().tiredTime = pSponsor.GetComponent<trapAI> ().ttinit;
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

			if (pSponsor.GetComponent<trapAI> ().tempRoundPoint == Vector3.zero) {
				Vector2 trandom = Random.insideUnitCircle;
				pSponsor.GetComponent<trapAI> ().tempRoundPoint = 
					pSponsor.GetComponent<trapAI> ().mRoundCenter + (new Vector3 (trandom.x, 0, trandom.y)) *
				pSponsor.GetComponent<trapAI> ().RoundSize;
			}
			if (Vector3.Distance (pSponsor.GetComponent<trapAI> ().tempRoundPoint, pSponsor.AIPos) < 1.0f) {
				Vector2 trandom = Random.insideUnitCircle;
				pSponsor.GetComponent<trapAI> ().tempRoundPoint = 
					pSponsor.GetComponent<trapAI> ().mRoundCenter + (new Vector3 (trandom.x, 0, trandom.y)) *
				pSponsor.GetComponent<trapAI> ().RoundSize;
			}
			// 更新运动数据
			pSponsor.GetComponent<Group4StandardAI> ().dir = (pSponsor.GetComponent<trapAI> ().tempRoundPoint - pSponsor.AIPos).normalized;
			pSponsor.GetComponent<Group4StandardAI> ().vel = pSponsor.GetComponent<trapAI> ().walkVel;
			pSponsor.GetComponent<Group4StandardAI> ().move = MoveFunc.Complex;
		}
		// 分配每个人的位置，由于大家一致行动，所以不用考虑归队问题
		if (pid == 0) {
			pSponsor.GetComponent<AIMove> ().mDirection = pSponsor.GetComponent<Group4StandardAI> ().dir;
			pSponsor.GetComponent<AIMove> ().mVelocity = pSponsor.GetComponent<Group4StandardAI> ().vel;
			pSponsor.GetComponent<AIMove> ().mMoveFunc = pSponsor.GetComponent<Group4StandardAI> ().move;
			pSponsor.GetComponent<trapAI> ().tiredTime -= Time.deltaTime;// 疲劳时间
		} else {
			pResponsers [pid - 1].GetComponent<AIMove> ().mDirection = pSponsor.GetComponent<Group4StandardAI> ().dir;
			pResponsers [pid - 1].GetComponent<AIMove> ().mVelocity = pSponsor.GetComponent<Group4StandardAI> ().vel;
			pResponsers [pid - 1].GetComponent<AIMove> ().mMoveFunc = pSponsor.GetComponent<Group4StandardAI> ().move;
		}
		if (pid == 1) {
			pResponsers [pid - 1].GetComponent<staComponent> ().staNow -= pResponsers [pid - 1].GetComponent<staComponent> ().staWalk * Time.deltaTime;
		}
	}

	public static void Round_Exit (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		if (pid == 0) 
			Debug.Log ("SM3TM1 Round");
	}

	public static void Rest_Enter (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		if (pid == 0) 
		Debug.Log ("SM3TM1 Rest");
		if (pid == 0) {
			pSponsor.GetComponent<trapAI> ().restTime = pSponsor.GetComponent<trapAI> ().rtintit;
			pSponsor.GetComponent<trapAI> ().rest = true;
			pSponsor.GetComponent<AIMove> ().mVelocity = 0.0f;
		} else {
			pResponsers [pid - 1].GetComponent<AIMove> ().mVelocity = 0.0f;
			pResponsers [pid - 1].GetComponent<SSHComponent> ().back = true;
		}

	}

	public static void Rest (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		if (pid == 0) {
			if (pSponsor.GetComponent<trapAI> ().restTime > 0.0f)// 没休息完
				pSponsor.GetComponent<trapAI> ().restTime -= Time.deltaTime;
			else// 休息完毕
				pSponsor.GetComponent<trapAI> ().rest = false;
		}
	}

	public static void Rest_Exit (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{

	}

	public static void Vigilance_Enter (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		Debug.Log ("SM3TM1 Vigilance");
		if (pid == 0) {
			pSponsor.GetComponent<getPlayer> ().vigilanceTime = pSponsor.GetComponent<getPlayer> ().vtinit;
			pSponsor.GetComponent<getPlayer> ().search = false;
			pSponsor.GetComponent<getPlayer> ().vigilance = true;
			pSponsor.GetComponent<AIMove> ().mVelocity = 0.0f;
			pSponsor.GetComponent<AIMove> ().mMoveFunc = MoveFunc.JUSTROTATE;
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
		Debug.Log ("SM3TM1 Search");
		if (pid == 0) {
			// 初始化搜查时间
			pSponsor.GetComponent<getPlayer> ().searchTime = pSponsor.GetComponent<getPlayer> ().stinit;
			pSponsor.GetComponent<getPlayer> ().vigilance = false;
			pSponsor.GetComponent<getPlayer> ().search = true;// 这里不写也行
		} else {
			pResponsers [pid - 1].GetComponent<SSHComponent> ().back = true;
		}
	}

	public static void Search (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		if (pid == 0) {

			if (Vector3.Distance (pSponsor.AIPos, pSponsor.PlayerPos) < 4.0f) {
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
				pSponsor.GetComponent<Group4StandardAI> ().vel = pSponsor.GetComponent<trapAI> ().walkVel * Time.deltaTime;
				pSponsor.GetComponent<Group4StandardAI> ().move = MoveFunc.Complex;
				pSponsor.GetComponent<Group4StandardAI> ().dir = (pSponsor.GetComponent<getPlayer> ().suspensionPoint - pSponsor.AIPos).normalized;
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

		if (pid == 1) {
			pResponsers [pid - 1].GetComponent<staComponent> ().staNow -= 
				pResponsers [pid - 1].GetComponent<staComponent> ().staWalk * Time.deltaTime;
		}

	}

	public static void Search_Exit (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{

	}

	public static void Engage_Enter (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		Debug.Log ("SM3TM1 Engage Enter");
		if (pid == 0) {
			pSponsor.GetComponent<trapAI> ().shootPoint.GetComponent<GytFirePos> ().fireRate = 2.0f;
			pSponsor.GetComponent<trapAI> ().shootPoint.GetComponent<GytFirePos> ().shoot = true;
			Debug.Log ("Shoot "+pSponsor.GetComponent<trapAI> ().shootPoint.GetComponent<GytFirePos> ().shoot);
		} else {
			pResponsers [pid - 1].GetComponent<SSHComponent> ().back = false;
		}
	}

	public static void Engage (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		// 123号位盾兵负责进行掩护，保证可以保护到陷阱师的前方，0号位陷阱师只是射击
		// 通过获得陷阱师的前方，进行类似于return节点的操作来实现
		if (pid == 0) {
			pSponsor.GetComponent<trapAI> ().shootPoint.GetComponent<GytFirePos> ().shoot = true;
			pSponsor.GetComponent<Group4StandardAI> ().dir = (pSponsor.PlayerPos - pSponsor.AIPos).normalized;
			pSponsor.GetComponent<Group4StandardAI> ().move = MoveFunc.Complex;//simple穿模寻路 complex避障寻路

			// 以一号位盾兵为基准
			if (pResponsers [0].GetComponent<SSHComponent> ().hasShield) {
				pSponsor.GetComponent<Group4StandardAI> ().vel = pResponsers [0].GetComponent<shieldAI> ().shieldVel;
			} else
				pSponsor.GetComponent<Group4StandardAI> ().vel = pResponsers [0].GetComponent<shieldAI> ().walkVel;
			pResponsers [0].GetComponent<staComponent> ().staNow -= pResponsers [0].GetComponent<staComponent> ().staWalk * Time.deltaTime;
			GameObject ai = pSponsor.GetComponent<BaseAIComponent> ().mAIRT;
			pSponsor.GetComponent<Group4StandardAI> ().target [0] = ai.transform.position + ai.transform.forward.normalized * 1.0f;
			pSponsor.GetComponent<Group4StandardAI> ().target [1] = ai.transform.position + ai.transform.forward.normalized * 0.5f +
				pSponsor.mAI.transform.right.normalized * 0.5f;
			pSponsor.GetComponent<Group4StandardAI> ().target [2] = ai.transform.position + ai.transform.forward.normalized * 0.5f -
				pSponsor.mAI.transform.right.normalized * 0.5f;
		}
		if (pid == 0) {
			pSponsor.GetComponent<AIMove> ().mDirection = pSponsor.GetComponent<Group4StandardAI> ().dir;
			pSponsor.GetComponent<AIMove> ().mVelocity = 0.0f;
			pSponsor.GetComponent<AIMove> ().mMoveFunc = MoveFunc.JUSTROTATE;
			// 和之前不同的是，这里是持续射击，所以，在初始化设置好就可以了
		} else {
			// 判断各个盾兵是否到达指定位置
			float dis = Vector3.Distance (pSponsor.GetComponent<Group4StandardAI> ().target [pid - 1], 
				            pResponsers [pid - 1].AIPos);
			// 当接近目标时，设置速度为0
			if (dis < 0.1f) {
				// 到达位置，保证朝向正确
				pResponsers [pid - 1].GetComponent<AIMove> ().mDirection = pSponsor.GetComponent<Group4StandardAI> ().dir;
				pResponsers [pid - 1].GetComponent<AIMove> ().mVelocity = 0.0f;
				pResponsers [pid - 1].GetComponent<AIMove> ().mMoveFunc = MoveFunc.JUSTROTATE;
			} else {
				// 没到位置，继续位移
				Debug.Log("Change Place");
				Vector3 dir = pSponsor.GetComponent<Group4StandardAI> ().target [pid - 1] - pResponsers [pid - 1].AIPos;
				pResponsers [pid - 1].GetComponent<AIMove> ().mDirection = dir.normalized;
				pResponsers [pid - 1].GetComponent<AIMove> ().mVelocity = pSponsor.GetComponent<Group4StandardAI> ().vel;
				pResponsers [pid - 1].GetComponent<AIMove> ().mMoveFunc = MoveFunc.Complex;
			}
		}
		if (pid == 1) {
			pResponsers [pid - 1].GetComponent<staComponent> ().staNow -= pResponsers [pid - 1].GetComponent<staComponent> ().staWalk * Time.deltaTime;
		}

	}

	public static void Engage_Exit (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		// 把射击关闭
		Debug.Log ("SM3TM1 Engage Exit");
		if (pid == 0) {
		pSponsor.GetComponent<trapAI> ().shootPoint.GetComponent<GytFirePos> ().fireRate = 1000.0f;
		pSponsor.GetComponent<trapAI> ().shootPoint.GetComponent<GytFirePos> ().shoot = false;
		}
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
			if (Vector3.Distance (pSponsor.GetComponent<Group4StandardAI> ().target [i], pResponsers [i].AIPos) > 0.5f) {
				return 0.0f;
			}
		}
		Debug.Log ("return all done");
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
		float t = pSponsor.GetComponent<trapAI> ().tiredTime;
		float tmax = pSponsor.GetComponent<trapAI> ().ttinit;
		return Mathf.Min (1.0f, (tmax - t) / tmax);
	}

	public static float Rest2Round (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		if (pSponsor.GetComponent<trapAI> ().rest == false)
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

	public static float Search2Engage (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		if (pSponsor.GetComponent<getPlayer> ().engage)
			return 1.0f;
		return 0.0f;
	}


	public static float Engage2Return (AIEntity pSponsor, AIEntity[] pResponsers, int pid)
	{
		if (Vector3.Distance (pSponsor.AIPos, pSponsor.PlayerPos) > 30.0f)
			return 1.0f;
		return 0.0f;
	}
}
