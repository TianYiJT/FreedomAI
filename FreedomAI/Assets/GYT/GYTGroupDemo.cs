using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FreedomAI;
using UnityECS;


public class GYTGroupDemo : MonoBehaviour
{
	// 基本五项
	GroupStrategyEnter enterGroup;
	GroupStrategyRT strategyGroup;
	GroupAllocation allocationGroup;
	GroupDissolve dissolveGroup;
	GroupStrategyExit exitGroup;

	// 行为节点
	GroupStateEnter returnEnter;
	GroupStateRT returnState;
	GroupStateExit returnExit;
	GroupStateEnter roundEnter;
	GroupStateRT roundState;
	GroupStateExit roundExit;
	GroupStateEnter restEnter;
	GroupStateRT restState;
	GroupStateExit restExit;
	GroupStateEnter vigilanceEnter;
	GroupStateRT vigilanceState;
	GroupStateExit vigilanceExit;
	GroupStateEnter searchEnter;
	GroupStateRT searchState;
	GroupStateExit searchExit;
	GroupStateEnter chaseEnter;
	GroupStateRT chaseState;
	GroupStateExit chaseExit;
	GroupStateEnter shieldFEnter;
	GroupStateRT shieldFState;
	GroupStateExit shieldFExit;

	GroupStateEnter engageEnter;
	GroupStateRT engageState;
	GroupStateExit engageExit;

	// 行为转化
	GroupStateTransfer return2round;
	GroupStateTransfer round2rest;
	GroupStateTransfer rest2round;
	GroupStateTransfer round2vigilance;
	GroupStateTransfer vigilance2round;
	GroupStateTransfer rest2vigilance;
	GroupStateTransfer vigilance2search;
	GroupStateTransfer search2round;
	GroupStateTransfer search2chase;
	GroupStateTransfer chase2round;
	GroupStateTransfer chase2shieldF;
	GroupStateTransfer shieldF2chase;

	GroupStateTransfer search2engage;
	GroupStateTransfer engage2return;


	// 群体行为中没有反馈函数

	// Use this for initialization
	void Start ()
	{
		enterGroup = GroupSM4.Enter;
		strategyGroup = GroupSM4.Strategy;
		allocationGroup = GroupSM4.Allocation;
		dissolveGroup = GroupSM4.Disslove;
		exitGroup = GroupSM4.Exit;

		returnEnter = GroupSM4.Return_Enter;
		returnState = GroupSM4.Return;
		returnExit = GroupSM4.Return_Exit;
		roundEnter = GroupSM4.Round_Enter;
		roundState = GroupSM4.Round;
		roundExit = GroupSM4.Round_Exit;
		restEnter = GroupSM4.Rest_Enter;
		restState = GroupSM4.Rest;
		restExit = GroupSM4.Rest_Exit;
		vigilanceEnter = GroupSM4.Vigilance_Enter;
		vigilanceState = GroupSM4.Vigilance;
		vigilanceExit = GroupSM4.Vigilance_Exit;
		searchEnter = GroupSM4.Search_Enter;
		searchState = GroupSM4.Search;
		searchExit = GroupSM4.Search_Exit;
		chaseEnter = GroupSM4.Chase_Enter;
		chaseState = GroupSM4.Chase;
		chaseExit = GroupSM4.Chase_Exit;
		shieldFEnter = GroupSM4.ShieldF_Enter;
		shieldFState = GroupSM4.ShieldF;
		shieldFExit = GroupSM4.ShieldF_Exit;

		return2round = GroupSM4.Return2Round;
		round2rest = GroupSM4.Round2Rest;
		rest2round = GroupSM4.Rest2Round;
		round2vigilance = GroupSM4.Round2Vigilance;
		vigilance2round = GroupSM4.Vigilance2Round;
		rest2vigilance = GroupSM4.Rest2Vigilance;
		vigilance2search = GroupSM4.Vigilance2Search;
		search2round = GroupSM4.Serach2Round;
		search2chase = GroupSM4.Search2Chase;
		chase2round = GroupSM4.Chase2Round;
		chase2shieldF = GroupSM4.Chase2ShieldF;
		shieldF2chase = GroupSM4.ShieldF2Chase;

		GroupBehaviourNode gbnSM4 = new GroupBehaviourNode ();
		gbnSM4.mCount = 4;
		gbnSM4.mRoleName = "ShieldMan";

		List<GroupBehaviourNode> gbnList0 = new List<GroupBehaviourNode> ();
		gbnList0.Add (gbnSM4);

		GroupManager.getInstance ().AddGroupList (gbnList0, dissolveGroup, strategyGroup, allocationGroup, 
			enterGroup, exitGroup);

		AIGroupState aiGroupState0 = new AIGroupState ();

		int id0_return = aiGroupState0.AddGroupState (returnState, returnEnter, returnExit);
		int id0_round = aiGroupState0.AddGroupState (roundState, roundEnter, roundExit);
		int id0_rest = aiGroupState0.AddGroupState (restState, restEnter, restExit);
		int id0_vigilance = aiGroupState0.AddGroupState (vigilanceState, vigilanceEnter, vigilanceExit);
		int id0_search = aiGroupState0.AddGroupState (searchState, searchEnter, searchExit);
		int id0_chase = aiGroupState0.AddGroupState (chaseState, chaseEnter, chaseExit);
		int id0_shieldF = aiGroupState0.AddGroupState (shieldFState, shieldFEnter, shieldFExit);

		aiGroupState0.AddAnim (id0_return, GroupSM4.Return_Anim);
		aiGroupState0.AddAnim (id0_round, GroupSM4.Round_Anim);
		aiGroupState0.AddAnim (id0_rest, GroupSM4.Rest_Anim);
		aiGroupState0.AddAnim (id0_vigilance, GroupSM4.Vigilance_Anim);
		aiGroupState0.AddAnim (id0_search, GroupSM4.Search_Anim);
		aiGroupState0.AddAnim (id0_chase, GroupSM4.Chase_Anim);
		aiGroupState0.AddAnim (id0_shieldF, GroupSM4.ShieldF_Anim);


		aiGroupState0.AddTransfer (id0_return, id0_round, return2round);
		aiGroupState0.AddTransfer (id0_round, id0_rest, round2rest);
		aiGroupState0.AddTransfer (id0_rest, id0_round, rest2round);
		aiGroupState0.AddTransfer (id0_round, id0_vigilance, round2vigilance);
		aiGroupState0.AddTransfer (id0_vigilance, id0_round, vigilance2round);
		aiGroupState0.AddTransfer (id0_rest, id0_vigilance, rest2vigilance);
		aiGroupState0.AddTransfer (id0_vigilance, id0_search, vigilance2search);
		aiGroupState0.AddTransfer (id0_search, id0_round, search2round);
		aiGroupState0.AddTransfer (id0_search, id0_chase, search2chase);
		aiGroupState0.AddTransfer (id0_chase, id0_round, chase2round);
		aiGroupState0.AddTransfer (id0_chase, id0_shieldF, chase2shieldF);
		aiGroupState0.AddTransfer (id0_shieldF, id0_chase, shieldF2chase);

		aiGroupState0.tempID = id0_return;

		GroupManager.getInstance ().AddStrategy ("ShieldMan", 0, aiGroupState0);


		// 二号队伍的绑定
		enterGroup = GYTGroupSM3TM1.Enter;
		strategyGroup = GYTGroupSM3TM1.Strategy;
		allocationGroup = GYTGroupSM3TM1.Allocation;
		dissolveGroup = GYTGroupSM3TM1.Disslove;
		exitGroup = GYTGroupSM3TM1.Exit;

		returnEnter = GYTGroupSM3TM1.Return_Enter;
		returnState = GYTGroupSM3TM1.Return;
		returnExit = GYTGroupSM3TM1.Return_Exit;
		roundEnter = GYTGroupSM3TM1.Round_Enter;
		roundState = GYTGroupSM3TM1.Round;
		roundExit = GYTGroupSM3TM1.Round_Exit;
		restEnter = GYTGroupSM3TM1.Rest_Enter;
		restState = GYTGroupSM3TM1.Rest;
		restExit = GYTGroupSM3TM1.Rest_Exit;
		vigilanceEnter = GYTGroupSM3TM1.Vigilance_Enter;
		vigilanceState = GYTGroupSM3TM1.Vigilance;
		vigilanceExit = GYTGroupSM3TM1.Vigilance_Exit;
		searchEnter = GYTGroupSM3TM1.Search_Enter;
		searchState = GYTGroupSM3TM1.Search;
		searchExit = GYTGroupSM3TM1.Search_Exit;
		engageEnter = GYTGroupSM3TM1.Engage_Enter;
		engageState = GYTGroupSM3TM1.Engage;
		engageExit = GYTGroupSM3TM1.Engage_Exit;

		return2round = GYTGroupSM3TM1.Return2Round;
		round2rest = GYTGroupSM3TM1.Round2Rest;
		rest2round = GYTGroupSM3TM1.Rest2Round;
		round2vigilance = GYTGroupSM3TM1.Round2Vigilance;
		vigilance2round = GYTGroupSM3TM1.Vigilance2Round;
		rest2vigilance = GYTGroupSM3TM1.Rest2Vigilance;
		vigilance2search = GYTGroupSM3TM1.Vigilance2Search;
		search2round = GYTGroupSM3TM1.Serach2Round;
		search2engage = GYTGroupSM3TM1.Search2Engage;
		engage2return = GYTGroupSM3TM1.Engage2Return;

		GroupBehaviourNode gbnSM3 = new GroupBehaviourNode ();
		gbnSM3.mCount = 3;
		gbnSM3.mRoleName = "ShieldMan";

		GroupBehaviourNode gbnTM1 = new GroupBehaviourNode ();
		gbnTM1.mCount = 1;
		gbnTM1.mRoleName = "TrapMan";

		List<GroupBehaviourNode> gbnList1 = new List<GroupBehaviourNode> ();
		gbnList1.Add (gbnSM3);
		gbnList1.Add (gbnTM1);

		GroupManager.getInstance ().AddGroupList (gbnList1, dissolveGroup, strategyGroup, allocationGroup, 
			enterGroup, exitGroup);

		AIGroupState aiGroupState1 = new AIGroupState ();

		int id1_return = aiGroupState1.AddGroupState (returnState, returnEnter, returnExit);
		int id1_round = aiGroupState1.AddGroupState (roundState, roundEnter, roundExit);
		int id1_rest = aiGroupState1.AddGroupState (restState, restEnter, restExit);
		int id1_vigilance = aiGroupState1.AddGroupState (vigilanceState, vigilanceEnter, vigilanceExit);
		int id1_search = aiGroupState1.AddGroupState (searchState, searchEnter, searchExit);
		int id1_engage = aiGroupState1.AddGroupState (engageState, engageEnter, engageExit);

		aiGroupState1.AddAnim (id1_return, GYTGroupSM3TM1.Return_Anim);
		aiGroupState1.AddAnim (id1_round, GYTGroupSM3TM1.Round_Anim);
		aiGroupState1.AddAnim (id1_rest, GYTGroupSM3TM1.Rest_Anim);
		aiGroupState1.AddAnim (id1_vigilance, GYTGroupSM3TM1.Vigilance_Anim);
		aiGroupState1.AddAnim (id1_search, GYTGroupSM3TM1.Search_Anim);
		aiGroupState1.AddAnim (id1_engage, GYTGroupSM3TM1.Engage_Anim);

		aiGroupState1.AddTransfer (id1_return, id1_round, return2round);
		aiGroupState1.AddTransfer (id1_round, id1_rest, round2rest);
		aiGroupState1.AddTransfer (id1_rest, id1_round, rest2round);
		aiGroupState1.AddTransfer (id1_round, id1_vigilance, round2vigilance);
		aiGroupState1.AddTransfer (id1_vigilance, id1_round, vigilance2round);
		aiGroupState1.AddTransfer (id1_rest, id1_vigilance, rest2vigilance);
		aiGroupState1.AddTransfer (id1_vigilance, id1_search, vigilance2search);
		aiGroupState1.AddTransfer (id1_search, id1_round, search2round);
		aiGroupState1.AddTransfer (id1_search, id1_engage, search2engage);
		aiGroupState1.AddTransfer (id1_engage, id1_return, engage2return);

		aiGroupState1.tempID = id1_return;

		GroupManager.getInstance ().AddStrategy ("TrapMan", 1, aiGroupState1);

		List<int> key0 = new List<int> ();
		key0.Add (0);
		GroupManager.getInstance ().AddKey ("ShieldMan", key0);

		List<int> key1 = new List<int> ();
		key1.Add (1);
		GroupManager.getInstance ().AddKey ("TrapMan", key1);

		// 用于设置判定球的半径
		GroupManager.getInstance ().mCheckDistance = 20.0f;
	}


	// Update is called once per frame
	void Update ()
	{
		
	}
}
