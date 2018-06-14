using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityECS;
using FreedomAI;


namespace FreedomAI
{


	public struct GroupBehaviourNode
	{
		public string mRoleName;
		public uint mCount;
	}

	/*
	 * a sponsor node in Group ALG.
	 * 
	*/
	public class SponsorNode
	{
		public AIEntity mSponsor;
		public List<int> mGroupListID;
		public ResponseNode[] members;
		public int memebercount = 0;
		public float mDissloveTimer = 0.0f;
		public int tempGroupID = -1;

		public SponsorNode()
		{
			mGroupListID = new List<int> ();
			members = new ResponseNode[25];
			for (int i = 0; i < 25; i++)
				members [i] = new ResponseNode ();
		}

		/*
		 * check all the member neighbor 
		 * if have members full a list-precompute
		 * Group them by the Group Strategy function value
		 * 
		*/

		public bool checkGroup()
		{
			foreach(int i in mGroupListID)
			{
				List<GroupBehaviourNode> gbns = GroupManager.getInstance ().getGroupList (i);
				List<int> tempIDs = new List<int> ();
				bool isSucceed = true;
				// list-precompute
				foreach (GroupBehaviourNode gbn in gbns) 
				{
					int tCount = 0;
					if (mSponsor.tag == gbn.mRoleName)
						tCount++;
					for(int j=0;j<memebercount;j++)
					{
						if (tCount == gbn.mCount) 
						{
							break;
						}
						if (members [j].mResponse.tag == gbn.mRoleName)
						{
							tCount++;
							tempIDs.Add (j);
						}
					}

					if (tCount < gbn.mCount)
					{
						isSucceed = false;
						break;
					}
				}
				// having member full the list 
				if (isSucceed) 
				{
					GroupStrategyRT mStrategy = GroupManager.getInstance().Dictionaryforid2Strategy[i];
					AIEntity[] pResponses = new AIEntity[tempIDs.Count];
					for (int j = 0; j < tempIDs.Count; j++)
						pResponses [j] = members [tempIDs[j]].mResponse;

					// compute the strategy value 
					float rate = mStrategy (mSponsor,pResponses);

					if (rate*Time.deltaTime > Random.Range (0.0f, 1.0f)) 
					{
						
						// succeed, then group them

						tempGroupID = i;
						memebercount = tempIDs.Count;
						ResponseNode[] rn = new ResponseNode[25]; 
						for (int j = 0; j < memebercount; j++)
						{
							rn [j] = members [tempIDs [j]];
						}

						members = rn;

						for (int j = 0; j < memebercount; j++)
						{
							members [j].mSucceedTeam = true;
							members [j].mLeader = mSponsor;
						}

						return true;
					}
				}
			}
			return false;
		}

	};

	public class ResponseNode
	{
		public AIEntity mResponse;
		public bool mSucceedTeam = false;
		public AIEntity mLeader;
		public int mGroupID;
	};

	// Group State Node function 
	public delegate void GroupStateRT(AIEntity pSponsor,AIEntity[] pResponses,int pid);

	// Group Animation Node function 
	public delegate string GroupAnimationRT(int pid);

	//Group State Enter Node function
	public delegate void GroupStateEnter(AIEntity pSponsor,AIEntity[] pResponses,int pid);

	//Group State Exit Node function 
	public delegate void GroupStateExit(AIEntity pSponsor,AIEntity[] pResponses,int pid);

	// Grooup State Transfer Node function
	public delegate float GroupStateTransfer(AIEntity pSponsor,AIEntity[] pResponses,int pid);

	// Group Strategy Node function
	public delegate float GroupStrategyRT(AIEntity pSponsor,AIEntity[] pResponses);

	//Group Strategy Enter Node function
	public delegate void GroupStrategyEnter(AIEntity pSponsor,AIEntity[] pResponses);

	// Group Strategy Exit Node function 
	public delegate void GroupStrategyExit(AIEntity pSponsor,AIEntity[] pResponses);

	// Group Dissolve Node function
	public delegate float GroupDissolve(AIEntity pSponsor,AIEntity[] pResponses);

	// Group Alloc Node function 
	public delegate int[] GroupAllocation(AIEntity pSponsor,AIEntity[] pResponses);


	public class EmptyGroupFunc
	{

		public static void StateEnter(AIEntity pSponsor,AIEntity[] pResponses,int pid)
		{
			
		}

		public static void StateExit(AIEntity pSponsor,AIEntity[] pResponses,int pid)
		{
			
		}

		public static void StrategyEnter(AIEntity pSponsor,AIEntity[] pResponses)
		{
			
		}

		public static void StrategyExit(AIEntity pSponsor,AIEntity[] pResponses)
		{
			
		}

	}

	public struct EntityStrategyNode
	{
		public string tagName;
		public int id;
	}

	/*
	 * 
	 * a Global Group Strategy Manager
	 * you should add your Group Strategy(AIGroupState,GroupList) in this class
	 * you should talk this class which AI is a Sponsor,or a Responsor.
	*/

	public class GroupManager:UComponent
	{
		// all Group List
		public List<List<GroupBehaviourNode>> mGroupLists;

		// all Sponsor
		public SponsorNode[] mSponsors;

		// all responsor
		public ResponseNode[] mResponses;

		public int mSponsorCount = 0;
		public int mResponseCount = 0;

		// AI tag 2 group id list
		public Dictionary<string,List<int>> Dictionaryfortag2groupid;
		// group id 2 alloc
		public Dictionary<int,GroupAllocation> Dictionaryforid2Allocation;
		// group id 2 strategy
		public Dictionary<int,GroupStrategyRT> Dictionaryforid2Strategy;
		// group id 2 dissolve
		public Dictionary<int,GroupDissolve> Dictionaryforid2Dissolve;
		public Dictionary<int,GroupStrategyEnter> Dictionaryforid2StrategyEnter;
		public Dictionary<int,GroupStrategyExit> Dictionaryforid2StrategyExit;
		public Dictionary<EntityStrategyNode,AIGroupState> DictionaryforGroupState;


		public float mCheckDistance;
		//List<AIEntity> 

		private static GroupManager mGroupManagerSington;

		public List<GroupBehaviourNode> getGroupList(int i)
		{
			return mGroupLists [i];
		}

		public static GroupManager getInstance()
		{
			if (mGroupManagerSington == null) 
			{
				mGroupManagerSington = new GroupManager ();
			}
			return mGroupManagerSington;
		}
		// init with all array
		public GroupManager()
		{
			mGroupLists = new List<List<GroupBehaviourNode>> ();
			mSponsors = new SponsorNode[50];
			for (int i = 0; i < 50; i++)
				mSponsors [i] = new SponsorNode ();
			mResponses = new ResponseNode[500];
			for (int i = 0; i < 500; i++)
				mResponses = new ResponseNode[500];
			Dictionaryfortag2groupid = new Dictionary<string, List<int>> ();
			Dictionaryforid2Allocation = new Dictionary<int, GroupAllocation> ();
			Dictionaryforid2Dissolve = new Dictionary<int, GroupDissolve> ();
			Dictionaryforid2Strategy = new Dictionary<int, GroupStrategyRT> ();
			DictionaryforGroupState = new Dictionary<EntityStrategyNode, AIGroupState> ();
			Dictionaryforid2StrategyExit = new Dictionary<int, GroupStrategyExit> ();
			Dictionaryforid2StrategyEnter = new Dictionary<int, GroupStrategyEnter> ();
		}
		// add a group list
		public void AddGroupList(List<GroupBehaviourNode> pList,GroupDissolve pDissolve,GroupStrategyRT pStrategy,GroupAllocation pAllocation,GroupStrategyEnter pEnter,GroupStrategyExit pExit)
		{
			mGroupLists.Add (pList);
			int index = mGroupLists.IndexOf (pList);
			Dictionaryforid2Allocation.Add (index,pAllocation);
			Dictionaryforid2Dissolve.Add (index,pDissolve);
			Dictionaryforid2Strategy.Add (index,pStrategy);
			Dictionaryforid2StrategyEnter.Add (index,pEnter);
			Dictionaryforid2StrategyExit.Add (index,pExit);
		}

		public void AddSponsor(AIEntity pEntity)
		{
			SponsorNode tSponsor = new SponsorNode ();
			tSponsor.mSponsor = pEntity;
			tSponsor.mGroupListID = Dictionaryfortag2groupid [pEntity.tag];
			mSponsors [mSponsorCount] = tSponsor;
			mSponsorCount++;
		}

		public void AddResponse(AIEntity pEntity)
		{
			ResponseNode tResponseNode = new ResponseNode ();
			tResponseNode.mResponse = pEntity;
			mResponses [mResponseCount] = tResponseNode;
			mResponseCount++;
		}

		public void AddKey(string tag,List<int> pids)
		{
			Dictionaryfortag2groupid.Add (tag,pids);
		}
		// add a strategy 
		public void AddStrategy(string tag,int id,AIGroupState pStateRT)
		{
			EntityStrategyNode esn = new EntityStrategyNode ();
			esn.tagName = tag;
			esn.id = id;
			DictionaryforGroupState.Add (esn,pStateRT);
		}

	}

	public class GroupManagerEntity:UEntity
	{
		private static GroupManagerEntity mGroupManagerEntitySingleton;

		public static GroupManagerEntity getInstance()
		{
			if (mGroupManagerEntitySingleton == null)
				mGroupManagerEntitySingleton = new GroupManagerEntity ();
			return mGroupManagerEntitySingleton;
		}
			
		public override void Init ()
		{
			base.Init ();
			this.AddComponent<GroupManager> (GroupManager.getInstance());
		}
	}

	// GroupBehaviour Manager system
	// update all Responsor ,sponsor with GroupManager data
	public class GroupBehaviourSystem:USystem
	{
		public override void Init ()
		{
			base.Init ();
			this.AddRequestComponent (typeof(GroupManager));
		}

		public override void Update (UEntity uEntity)
		{
			base.Update (uEntity);
			// clear responsor data
			for (int i = 0; i < uEntity.GetComponent<GroupManager> ().mResponseCount; i++)
			{
				if (!uEntity.GetComponent<GroupManager> ().mResponses [i].mSucceedTeam)
					uEntity.GetComponent<GroupManager> ().mResponses [i].mLeader = null;
			}


			for (int i = 0; i < uEntity.GetComponent<GroupManager> ().mSponsorCount; i++)
			{
				// sponsor having grouping
				if (uEntity.GetComponent<GroupManager> ().mSponsors [i].mSponsor.isGrouping) 
				{
					
					if (uEntity.GetComponent<GroupManager> ().mSponsors [i].mDissloveTimer < 1.0f)
						uEntity.GetComponent<GroupManager> ().mSponsors [i].mDissloveTimer += Time.deltaTime;
					else
					{
						//Debug.Log (uEntity.GetComponent<GroupManager> ().mSponsors [i].tempGroupID+" "+i);
						// check dissolve
						GroupDissolve tDissolve= uEntity.GetComponent<GroupManager> ().Dictionaryforid2Dissolve [uEntity.GetComponent<GroupManager> ().mSponsors [i].tempGroupID];
						AIEntity[] pEntitys = new AIEntity[uEntity.GetComponent<GroupManager> ().mSponsors[i].memebercount];
						for (int j = 0; j < uEntity.GetComponent<GroupManager> ().mSponsors [i].memebercount; j++)
							pEntitys [j] = uEntity.GetComponent<GroupManager> ().mSponsors [i].members [j].mResponse;
						float rate = tDissolve (uEntity.GetComponent<GroupManager>().mSponsors[i].mSponsor,pEntitys);
						GroupStrategyExit tExit = uEntity.GetComponent<GroupManager> ().Dictionaryforid2StrategyExit [uEntity.GetComponent<GroupManager> ().mSponsors [i].tempGroupID];
						if (Random.Range (0.0f, 1.0f) < rate)
						{
							// dissolve 
							for (int j = 0; j < uEntity.GetComponent<GroupManager> ().mSponsors [i].memebercount; j++)
							{
								uEntity.GetComponent<GroupManager> ().mSponsors [i].members [j].mResponse.isGrouping = false;
								uEntity.GetComponent<GroupManager> ().mSponsors [i].members [j].mSucceedTeam = false;
								uEntity.GetComponent<GroupManager> ().mSponsors [i].members [j].mLeader = null;
								uEntity.GetComponent<GroupManager> ().mSponsors [i].members [j].mGroupID = -1;
								uEntity.GetComponent<GroupManager> ().mSponsors [i].members [j].mResponse.mDissloveTimer = 10.0f;
							}
							uEntity.GetComponent<GroupManager> ().mSponsors [i].memebercount = 0;
							uEntity.GetComponent<GroupManager> ().mSponsors [i].mDissloveTimer = 0.0f;
							uEntity.GetComponent<GroupManager> ().mSponsors [i].mSponsor.isGrouping= false;
							uEntity.GetComponent<GroupManager> ().mSponsors [i].tempGroupID = -1;
							tExit (uEntity.GetComponent<GroupManager> ().mSponsors [i].mSponsor,pEntitys);
						}
						uEntity.GetComponent<GroupManager> ().mSponsors [i].mDissloveTimer = 0.0f;
					}
				} 
				else
				{
					uEntity.GetComponent<GroupManager> ().mSponsors [i].memebercount = 0;
					#if DEBUG
					AIEntity[] mEntity = AIEntity.getAllEntityWithSphere (uEntity.GetComponent<GroupManager> ().mSponsors [i].mSponsor,uEntity.GetComponent<GroupManager> ().mCheckDistance);
					#else
					AIEntity[] mEntity = AIEntity.getAllEntityWithSphere (uEntity.GetComponent<GroupManager> ().mSponsors [i].mSponsor,10.0f);
					#endif

					List<ResponseNode> mResponseList = new List<ResponseNode> ();


					// get real responsor neighbor sponsor

					for (int j = 0; j < mEntity.Length; j++)
					{
						
						if (mEntity [j].mDissloveTimer > 0.0f)
							continue;
						for(int k=0;k<uEntity.GetComponent<GroupManager>().mResponseCount;k++)
						{
							ResponseNode rn = uEntity.GetComponent<GroupManager> ().mResponses [k];
			
							if (rn.mResponse == mEntity [j] && rn.mLeader == null) 
							{
						
								mResponseList.Add (rn);
								break;
							}
						}
					}

					// responsor count 

					int count = Mathf.Min (mResponseList.Count,uEntity.GetComponent<GroupManager>().mSponsors[i].members.Length);
				//	Debug.Log (count);
					// init 
					for(int j=0;j<count;j++)
					{
						uEntity.GetComponent<GroupManager> ().mSponsors [i].members [j] = mResponseList [j];
						//uEntity.GetComponent<GroupManager> ().mSponsors [i].members [j].mLeader = uEntity.GetComponent<GroupManager> ().mSponsors [i].mSponsor;
					}

					uEntity.GetComponent<GroupManager> ().mSponsors [i].memebercount = count;
					// check Group 
					if (uEntity.GetComponent<GroupManager> ().mSponsors [i].checkGroup ()) 
					{
						
						/*
						for (int j = 0; j < count; j++) 
						{
							uEntity.GetComponent<GroupManager> ().mSponsors [i].members [j].mLeader = uEntity.GetComponent<GroupManager> ().mSponsors [i].mSponsor;
						}
						*/
						// enter group 
						GroupStrategyEnter tEnter = uEntity.GetComponent<GroupManager> ().Dictionaryforid2StrategyEnter [uEntity.GetComponent<GroupManager> ().mSponsors [i].tempGroupID];
						GroupAllocation tAlloc = uEntity.GetComponent<GroupManager> ().Dictionaryforid2Allocation [uEntity.GetComponent<GroupManager> ().mSponsors [i].tempGroupID];
						AIEntity[] pEntitys = new AIEntity[uEntity.GetComponent<GroupManager> ().mSponsors [i].memebercount];
						for (int j = 0; j < uEntity.GetComponent<GroupManager> ().mSponsors [i].memebercount; j++)
							pEntitys [j] = uEntity.GetComponent<GroupManager> ().mSponsors [i].members [j].mResponse;
						// enter function
						tEnter (uEntity.GetComponent<GroupManager> ().mSponsors [i].mSponsor, pEntitys);
						// alloc function 
						int[] ids = tAlloc (uEntity.GetComponent<GroupManager> ().mSponsors [i].mSponsor, pEntitys);
						// init sponsor
						uEntity.GetComponent<GroupManager> ().mSponsors [i].mSponsor.isGrouping = true;
						uEntity.GetComponent<GroupManager> ().mSponsors [i].mSponsor.GetComponent<AIGroupState> ().tempGroupID = 0;
						EntityStrategyNode esn = new EntityStrategyNode ();
						esn.id = uEntity.GetComponent<GroupManager> ().mSponsors [i].tempGroupID;
						esn.tagName = uEntity.GetComponent<GroupManager> ().mSponsors [i].mSponsor.tag;
						// AIGroupState temp 
						AIGroupState mStateRT = uEntity.GetComponent<GroupManager> ().DictionaryforGroupState [esn];

						uEntity.GetComponent<GroupManager> ().mSponsors [i].mSponsor.GetComponent<AIGroupState> ().pLeader = uEntity.GetComponent<GroupManager> ().mSponsors [i].mSponsor;
						uEntity.GetComponent<GroupManager> ().mSponsors [i].mSponsor.GetComponent<AIGroupState> ().pMembers = pEntitys;
						uEntity.GetComponent<GroupManager> ().mSponsors [i].mSponsor.GetComponent<AIGroupState> ().SimpleClone (mStateRT);
						for (int j = 0; j < uEntity.GetComponent<GroupManager> ().mSponsors [i].memebercount; j++)
						{
							// init Responsor
							uEntity.GetComponent<GroupManager> ().mSponsors [i].members [j].mGroupID = ids [j];
							uEntity.GetComponent<GroupManager> ().mSponsors [i].members [j].mResponse.isGrouping = true;
							uEntity.GetComponent<GroupManager> ().mSponsors [i].members [j].mResponse.GetComponent<AIGroupState> ().tempGroupID = ids [j];
							EntityStrategyNode esn1 = new EntityStrategyNode ();
							esn1.id = uEntity.GetComponent<GroupManager> ().mSponsors [i].tempGroupID;
							esn1.tagName = uEntity.GetComponent<GroupManager> ().mSponsors [i].mSponsor.tag;
							AIGroupState mStateRT1 = uEntity.GetComponent<GroupManager> ().DictionaryforGroupState [esn1];
							uEntity.GetComponent<GroupManager> ().mSponsors [i].members [j].mResponse.GetComponent<AIGroupState> ().pLeader = uEntity.GetComponent<GroupManager> ().mSponsors [i].mSponsor;
							uEntity.GetComponent<GroupManager> ().mSponsors [i].members [j].mResponse.GetComponent<AIGroupState> ().pMembers = pEntitys;
							uEntity.GetComponent<GroupManager> ().mSponsors [i].members [j].mResponse.GetComponent<AIGroupState> ().SimpleClone (mStateRT1);
						}
					}
				}
			}
		}
	}

	public struct GroupStateTransferNode
	{
		public int id;
		public GroupStateTransfer mTransfer;
	}

	/*
	 * againest with AIState
	 * 
	 * if AIState enable,AIGroupState not enable
	 * if AIGroupState enable,AIState not enable
	 * 
	 * the struct is same as AIState
	*/
	public class AIGroupState:UComponent
	{
		public int tempGroupID;

		public AIEntity pLeader;

		public AIEntity[] pMembers;

		public GroupStateRT[] mGroupStateRTs;
		public GroupStateEnter[] mGroupStateEnters;
		public GroupStateExit[] mGroupStateExits;
		public List<GroupStateTransferNode>[] mGroupStateTransfers;
		public List<GroupStateTransferNode> mGroupStateAnyTransfers;

		public Dictionary<int,GroupAnimationRT> mAnimationDic;

		public int tempID;

		public int GroupStateCount = 0;
		public int TransferCount = 0;

		public AIGroupState()
		{
			mGroupStateEnters = new GroupStateEnter[15];
			mGroupStateExits = new GroupStateExit[15];
			mGroupStateRTs = new GroupStateRT[15];
			mGroupStateTransfers = new List<GroupStateTransferNode>[30];
			for (int i = 0; i < 30; i++) 
			{
				mGroupStateTransfers [i] = new List<GroupStateTransferNode> ();
			}
			mAnimationDic = new Dictionary<int, GroupAnimationRT> ();
			mGroupStateAnyTransfers = new List<GroupStateTransferNode> ();
			mAnimationDic = new Dictionary<int, GroupAnimationRT> ();
			isEnable = false;
		}

		public int AddGroupState(GroupStateRT prt,GroupStateEnter penter,GroupStateExit pexit)
		{
			mGroupStateRTs [GroupStateCount] = prt;
			mGroupStateEnters [GroupStateCount] = penter;
			mGroupStateExits [GroupStateCount] = pexit;
			GroupStateCount++;
			return GroupStateCount - 1;
		}

		public void AddTransfer (int id1,int id2,GroupStateTransfer pTransfer)
		{
			GroupStateTransferNode gstn = new GroupStateTransferNode ();
			gstn.id = id2;
			gstn.mTransfer = pTransfer;
			mGroupStateTransfers [id1].Add (gstn);
		}

		public void AddAnyTransfer(int id1,GroupStateTransfer pTransfer)
		{
			GroupStateTransferNode gstn = new GroupStateTransferNode ();
			gstn.id = id1;
			gstn.mTransfer = pTransfer;
			mGroupStateAnyTransfers.Add (gstn);
		}

		public void AddAnim(int id,GroupAnimationRT name)
		{
			mAnimationDic.Add (id,name);
		}

		public void SimpleClone(AIGroupState pGroupState)
		{
			Dictionary<int,GroupAnimationRT> mdic = new Dictionary<int, GroupAnimationRT> ();
			foreach(var item in pGroupState.mAnimationDic)
			{
				int ti = item.Key;
				GroupAnimationRT ts = item.Value;
				mdic.Add (ti,ts);
			}
			mAnimationDic = mdic;
			GroupStateCount = pGroupState.GroupStateCount;
			for (int i = 0; i < GroupStateCount; i++)
			{
				mGroupStateEnters [i] = pGroupState.mGroupStateEnters [i];
				mGroupStateExits [i] = pGroupState.mGroupStateExits [i];
				mGroupStateRTs [i] = pGroupState.mGroupStateRTs [i];
			}
			for (int i = 0; i < 30; i++) 
			{
				mGroupStateTransfers [i].Clear ();
				for (int j = 0; j < pGroupState.mGroupStateTransfers [i].Count; j++)
				{
					mGroupStateTransfers [i].Add (pGroupState.mGroupStateTransfers[i][j]);
				}
				//Debug.Log (mGroupStateTransfers[i].Count);
			}
			tempID = pGroupState.tempID;
			mGroupStateEnters [tempID] (pLeader,pMembers,tempGroupID);
		}

	}


	public class GroupStateUpdateSystem:USystem
	{
		public override void Init ()
		{
			base.Init ();
			this.AddRequestComponent (typeof(AIGroupState));
		}

		public override void Update (UEntity uEntity)
		{
			base.Update (uEntity);
			if (!uEntity.GetComponent<AIGroupState> ().isEnable)
				return;
			AIEntity pLeader = uEntity.GetComponent<AIGroupState> ().pLeader;
			AIEntity[] pMembers = uEntity.GetComponent<AIGroupState> ().pMembers;
			int pid = uEntity.GetComponent<AIGroupState> ().tempGroupID; 

			// check any

			foreach(GroupStateTransferNode gstn in uEntity.GetComponent<AIGroupState>().mGroupStateAnyTransfers)
			{
				if (pid != 0) 
				{
					break;
				}

				int id = gstn.id;
				if (id == uEntity.GetComponent<AIGroupState> ().tempID)
				{
					continue;
				}
				// update logic 
				GroupStateTransfer tTransfer = gstn.mTransfer;
				float rate = tTransfer (pLeader,pMembers,pid);
				if (rate * Time.deltaTime > Random.Range (0.0f, 1.0f))
				{
					GroupStateExit tExit = uEntity.GetComponent<AIGroupState>().mGroupStateExits[uEntity.GetComponent<AIGroupState>().tempID];
					tExit (pLeader,pMembers,pid);
					uEntity.GetComponent<AIGroupState> ().tempID = id;
					GroupStateEnter tEnter = uEntity.GetComponent<AIGroupState>().mGroupStateEnters[uEntity.GetComponent<AIGroupState>().tempID];
					tEnter (pLeader,pMembers,pid);
					GroupStateRT ttRT = uEntity.GetComponent<AIGroupState> ().mGroupStateRTs [uEntity.GetComponent<AIGroupState> ().tempID];
					ttRT (pLeader,pMembers,pid);
					GroupAnimationRT ttName = uEntity.GetComponent<AIGroupState>().mAnimationDic[uEntity.GetComponent<AIGroupState> ().tempID];
					string ttName_ = ttName (uEntity.GetComponent<AIGroupState>().tempGroupID);
					uEntity.GetComponent<AIAnimation> ().mtempAnim = ttName_;
					// async data by leader
					for (int i = 0; i < pMembers.Length; i++) 
					{
						GroupStateExit tExit_ = uEntity.GetComponent<AIGroupState>().mGroupStateExits[uEntity.GetComponent<AIGroupState>().tempID];
						int pid_ = pMembers [i].GetComponent<AIGroupState> ().tempGroupID;
						tExit (pLeader,pMembers,pid_);
						pMembers [i].GetComponent<AIGroupState> ().tempID = id;
						GroupStateEnter tEnter_ = uEntity.GetComponent<AIGroupState>().mGroupStateEnters[uEntity.GetComponent<AIGroupState>().tempID];
						tEnter_ (pLeader,pMembers,pid_);
						GroupStateRT ttRT_ = uEntity.GetComponent<AIGroupState> ().mGroupStateRTs [uEntity.GetComponent<AIGroupState> ().tempID];
						ttRT_ (pLeader,pMembers,pid_);
						GroupAnimationRT ttName__= uEntity.GetComponent<AIGroupState>().mAnimationDic[uEntity.GetComponent<AIGroupState> ().tempID];
						string ttName___= ttName__ (pid_);
						uEntity.GetComponent<AIAnimation> ().mtempAnim = ttName___;
					}

					return;
				}
			}

			// check relate

			foreach(GroupStateTransferNode gstn in uEntity.GetComponent<AIGroupState>().mGroupStateTransfers[uEntity.GetComponent<AIGroupState>().tempID])
			{
				if (pid != 0) 
				{
					break;
				}

				int id = gstn.id;
				GroupStateTransfer tTransfer = gstn.mTransfer;
				float rate = tTransfer (pLeader,pMembers,pid);
			
				if (rate * Time.deltaTime > Random.Range (0.0f, 1.0f))
				{
					GroupStateExit tExit = uEntity.GetComponent<AIGroupState>().mGroupStateExits[uEntity.GetComponent<AIGroupState>().tempID];
					tExit (pLeader,pMembers,pid);
					uEntity.GetComponent<AIGroupState> ().tempID = id;
					GroupStateEnter tEnter = uEntity.GetComponent<AIGroupState>().mGroupStateEnters[uEntity.GetComponent<AIGroupState>().tempID];
					tEnter (pLeader,pMembers,pid);
					// async data by leader
					for (int i = 0; i < pMembers.Length; i++) 
					{
						GroupStateExit tExit_ = uEntity.GetComponent<AIGroupState>().mGroupStateExits[uEntity.GetComponent<AIGroupState>().tempID];
						int pid_ = pMembers [i].GetComponent<AIGroupState> ().tempGroupID;
						tExit (pLeader,pMembers,pid_);
						pMembers [i].GetComponent<AIGroupState> ().tempID = id;
						GroupStateEnter tEnter_ = uEntity.GetComponent<AIGroupState>().mGroupStateEnters[uEntity.GetComponent<AIGroupState>().tempID];
						tEnter_ (pLeader,pMembers,pid_);
					}

					break;
				}
			}
			// run the Group State node function
			GroupStateRT tRT = uEntity.GetComponent<AIGroupState> ().mGroupStateRTs [uEntity.GetComponent<AIGroupState> ().tempID];
			tRT (pLeader,pMembers,pid);
			GroupAnimationRT tNameRT = uEntity.GetComponent<AIGroupState>().mAnimationDic[uEntity.GetComponent<AIGroupState> ().tempID];
			string tName=tNameRT (uEntity.GetComponent<AIGroupState>().tempGroupID);
			uEntity.GetComponent<AIAnimation> ().mtempAnim = tName;
		}
	}

	public class GroupTransferSingleSystem:USystem
	{
		public override void Init ()
		{
			base.Init ();
			this.AddRequestComponent (typeof(BaseAIComponent));
		}

		public override void Update (UEntity uEntity)
		{
			base.Update (uEntity);

			if (((AIEntity)uEntity).mDissloveTimer > 0.0f) 
			{
				((AIEntity)uEntity).mDissloveTimer -= Time.deltaTime;
			}

			if (((AIEntity)uEntity).isGrouping) 
			{
				uEntity.GetComponent<AIState> ().isEnable = false;
				uEntity.GetComponent<AIGroupState> ().isEnable = true;
				uEntity.GetComponent<AIStrategy> ().isEnable = false;
			}
			else 
			{
				uEntity.GetComponent<AIState> ().isEnable = true;
				uEntity.GetComponent<AIStrategy> ().isEnable = true;
				if(uEntity.GetComponent<AIGroupState>()!=null)
					uEntity.GetComponent<AIGroupState> ().isEnable = false;
			}
		}
	}




};


