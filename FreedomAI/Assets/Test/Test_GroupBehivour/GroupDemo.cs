using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FreedomAI;
using UnityECS;
public class GroupDemo : MonoBehaviour
{

	public GameObject groupAIDemo;

	public GameObject groupAICube;

	public GameObject position;

   Animator animator;

	public int count;

	public int count1;

	GroupStateRT group_return;
	GroupStateRT group_round;
	GroupStateTransfer return2round;
	GroupStrategyRT groupStrategy;
	GroupStrategyEnter GStrategyEnter;
	GroupDissolve GDissolve;
	GroupAllocation GAlloc;
	StateExecuter Round;
	StateExit RoundExit;
	StateExecuter Idle;
	StateExit IdleExit;
	StateTranfer Idle2Round;
	StateTranfer Round2Idle;
	AnimationPlay walkPlay;
	AnimationPlay IdlePlay;

	GroupStateRT group_return_cube;

	GroupStrategyRT group_cube_strategy;

	void Start () 
	{

		group_return = TestGroup.Test_Group_Return;
		group_round = TestGroup.Test_Group_Round;
		return2round = TestGroup.Test_Group_Return2Round;
		groupStrategy = TestGroup.Test_Group_Strategy;
		GStrategyEnter = TestGroup.Test_Group_Enter;
		GDissolve = TestGroup.Test_Group_Disslove;
		GAlloc = TestGroup.Test_Group_Alloc;
		Round = TestGroup.Test_Round;
		RoundExit = TestGroup.Test_Round_Exit;
		Idle = TestGroup.Test_Idle;
		IdleExit = TestGroup.Test_Idle_Exit;
		Idle2Round = TestGroup.Test_Idle2Round;
		Round2Idle = TestGroup.Test_Round2Idle;
		walkPlay = TestFSM.FSM_Walk_Anim;
		IdlePlay = TestFSM.FSM_Idle_Anim;
		group_cube_strategy = TestGroup.Test_Group_CubeStrategy;
		group_return_cube = TestGroup.Test_Group_Return_Cube;


		GroupBehaviourNode gbn = new GroupBehaviourNode ();gbn.mCount = 4;gbn.mRoleName = "Test";

		List<GroupBehaviourNode> gbnList = new List<GroupBehaviourNode> (); gbnList.Add (gbn);

		GroupManager.getInstance ().AddGroupList (gbnList,GDissolve,groupStrategy,GAlloc,GStrategyEnter,EmptyGroupFunc.StrategyExit);




		AIGroupState aiGroupState = new AIGroupState ();

		int id_group_return=aiGroupState.AddGroupState (group_return,TestGroup.Test_Group_Return_Enter,EmptyGroupFunc.StateExit);
		int id_group_round=aiGroupState.AddGroupState (group_round,EmptyGroupFunc.StateEnter,EmptyGroupFunc.StateExit);

		aiGroupState.AddAnim (id_group_return,TestGroup.Test_Group_Return_Anim);
		aiGroupState.AddAnim (id_group_round,TestGroup.Test_Group_Round_Anim);

		aiGroupState.AddTransfer (id_group_return,id_group_round,return2round);
		aiGroupState.tempID = id_group_return;
		GroupManager.getInstance ().AddStrategy ("Test",0,aiGroupState);


		GroupBehaviourNode gbn_cube1 = new GroupBehaviourNode ();gbn_cube1.mCount = 2;gbn_cube1.mRoleName="Test";
		GroupBehaviourNode gbn_cube2 = new GroupBehaviourNode ();gbn_cube2.mCount = 3;gbn_cube2.mRoleName="Test1";

		List<GroupBehaviourNode> gbncubeList = new List<GroupBehaviourNode> (); gbncubeList.Add (gbn_cube1);gbncubeList.Add (gbn_cube2);

		GroupManager.getInstance ().AddGroupList (gbncubeList,GDissolve,group_cube_strategy,GAlloc,GStrategyEnter,EmptyGroupFunc.StrategyExit);

		AIGroupState aiGroupState1 = new AIGroupState ();
		int id_cube_group = aiGroupState1.AddGroupState (group_return_cube,TestGroup.Test_Group_Return_Enter,EmptyGroupFunc.StateExit);
		aiGroupState1.AddAnim (id_group_return,TestGroup.Test_Group_Return_Anim);
		aiGroupState1.AddAnim (id_group_round,TestGroup.Test_Group_Round_Anim);
		aiGroupState1.tempID = id_cube_group;
		GroupManager.getInstance ().AddStrategy ("Test1",1,aiGroupState1);



		List<int> key1 = new List<int>();key1.Add (1);
		GroupManager.getInstance ().AddKey ("Test1",key1);

		List<int> key = new List<int>(); key.Add (0); 
		GroupManager.getInstance ().AddKey ("Test",key);

		GroupManager.getInstance ().mCheckDistance = 20.0f;


		for (int i = 0; i < count; i++)
		{
			AIEntity pEntity = new AIEntity ();
			pEntity.tag = "Test";
			UEntity mPlayer = new UEntity();
			UEntity mPlayerLast = new UEntity ();
			pEntity.mAI = groupAIDemo;
			pEntity.mPlayer = position;
			pEntity.AIPos = position.transform.position + new Vector3 (Random.Range(-20,20),0,Random.Range(-20,20));
			ECSWorld.MainWorld.registerEntityAfterInit (pEntity);
			pEntity.Init ();
			pEntity.AddComponent<myAI> (new myAI ());
			pEntity.AddComponent<myGroupAI> (new myGroupAI());
			pEntity.GetComponent<BaseAIComponent> ().mAIRT.SetActive (true);
			pEntity.PlayerEntity = mPlayer;
			animator = pEntity.GetComponent<BaseAIComponent> ().mAIRT.GetComponent<Animator> ();
			pEntity.GetComponent<AIAnimation> ().mAnimator = animator;
			pEntity.GetComponent<AIAnimation> ().Add ("Idle",IdlePlay);
			pEntity.GetComponent<AIAnimation> ().Add ("Walk",walkPlay);
			pEntity.GetComponent<AIAnimation>().mtempAnim = "Walk";
			int id_round = pEntity.GetComponent<AIState> ().AddExecuter (Round,RoundExit,EmptyExitAndEnter.EmptyEnter);
			//int id_idle = pEntity.GetComponent<AIState> ().AddExecuter (Idle,IdleExit,EmptyExitAndEnter.EmptyEnter);
			//pEntity.GetComponent<AIState> ().AddEdge (Round2Idle,EmptyFeedbacker.Run,id_round,id_idle);
			//pEntity.GetComponent<AIState> ().AddEdge (Idle2Round,EmptyFeedbacker.Run,id_idle,id_round);
			pEntity.GetComponent<AIState> ().AddAnimation (Round,"Walk");
			pEntity.GetComponent<AIState> ().AddAnimation (Idle,"Idle");
			pEntity.GetComponent<AIState> ().tempID = id_round;
			pEntity.GetComponent<AIState> ().mStateRecorder = EmptyExitAndEnter.EmptyEnter;
			if (i == 0)
				GroupManager.getInstance ().AddSponsor (pEntity);
			else
				GroupManager.getInstance ().AddResponse (pEntity);

		}

		for (int i = 0; i < count1; i++)
		{
			AIEntity pEntity = new AIEntity ();
			pEntity.tag = "Test1";
			UEntity mPlayer = new UEntity();
			UEntity mPlayerLast = new UEntity ();
			pEntity.mAI = groupAICube;
			pEntity.mPlayer = position;
			pEntity.AIPos = position.transform.position + new Vector3 (Random.Range(-20,20),0,Random.Range(-20,20));
			ECSWorld.MainWorld.registerEntityAfterInit (pEntity);
			pEntity.Init ();
			pEntity.AddComponent<myAI> (new myAI ());
			pEntity.AddComponent<myGroupAI> (new myGroupAI());
			pEntity.GetComponent<BaseAIComponent> ().mAIRT.SetActive (true);
			pEntity.PlayerEntity = mPlayer;

			pEntity.GetComponent<AIAnimation>().mtempAnim = "Walk";
			int id_round = pEntity.GetComponent<AIState> ().AddExecuter (Round,RoundExit,EmptyExitAndEnter.EmptyEnter);

			pEntity.GetComponent<AIState> ().tempID = id_round;
			pEntity.GetComponent<AIState> ().mStateRecorder = EmptyExitAndEnter.EmptyEnter;

			if (i == 0)
				GroupManager.getInstance ().AddSponsor (pEntity);
			else
				GroupManager.getInstance ().AddResponse (pEntity);

		}

	}

	void Update () 
	{
		
	}
}


