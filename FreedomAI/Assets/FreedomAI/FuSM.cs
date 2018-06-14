using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityECS;
using FreedomAI;

namespace FreedomAI
{

	// Strategy Action Function
	public delegate float StrategyActioner(AIEntity pEntity);

	// Strategy Feedback Function
	public delegate float StrategyFeedbacker(AIEntity pEntity);

	// Strategy Exit Function
	public delegate void StrategyExit(AIEntity pEntity);

	// Strategy Enter Function
	public delegate void StrategyEnter(AIEntity pEntity);

	// no using at all
	public class ObstacleComponent:UComponent
	{
		public GameObject hitObject;
		public float hitDistance;
		public Vector3 target;
	}

	// no using at all
	public class ObstacleAvoidance
	{
		public static float ActionFunc(AIEntity pEntity)
		{
			GameObject tAIObject = pEntity.GetComponent<BaseAIComponent> ().mAIRT;
			Vector3 tDir = pEntity.GetComponent<AIMove> ().mDirection;
			float maxDis = 2.0f;
			RaycastHit hit = new RaycastHit();
			int layoutmask = 1 << LayerMask.NameToLayer ("Collision");
			if (Physics.Raycast (tAIObject.transform.position, tDir,out hit,maxDis,layoutmask))
			{
				Vector3 hitPos = hit.transform.position;
				float tDis = Vector3.Distance (hitPos,tAIObject.transform.position);
				pEntity.GetComponent<ObstacleComponent> ().hitObject = hit.transform.gameObject;
				pEntity.GetComponent<ObstacleComponent> ().hitDistance = tDis;
				if (tDis < 1.0f)
				{
					return 1.0f;
				}
				return 2.0f-tDis;
			}
			else
			{
				return 0.0f;
			}
		}

		public static void Strategy_Enter(AIEntity pEntity)
		{
			pEntity.GetComponent<ObstacleComponent> ().target = Vector3.zero;
		}

		public static void FSM_Avoid(AIEntity pEntity)
		{
			if (pEntity.GetComponent<ObstacleComponent> ().target == Vector3.zero)
			{
				Vector3 v1 = pEntity.GetComponent<ObstacleComponent> ().hitObject.transform.position - pEntity.AIPos;
				v1.y = 0.0f;
				Vector3 v2 = new Vector3 (1.0f,0.0f,-v1.x/v1.z);
				v2.Normalize ();
				Vector3 v3 = -v2;
				for (int i = 0; i <=10; i++) 
				{
					float tempRate = (float)i / 10.0f;
					Vector3 vdir1 = Vector3.Lerp (v1, v2, tempRate);
					vdir1.Normalize ();
					Vector3 vdir2 = Vector3.Lerp (v1,v3,tempRate);
					vdir2.Normalize ();
					float maxDis = 2.0f;
					LayerMask layoutmask = 1 << LayerMask.NameToLayer ("Collision");
					RaycastHit hit = new RaycastHit ();
					if (!Physics.Raycast (pEntity.GetComponent<BaseAIComponent> ().mAIRT.transform.position, vdir1, out hit, maxDis, layoutmask)) 
					{
						pEntity.GetComponent<ObstacleComponent> ().target = pEntity.GetComponent<BaseAIComponent> ().mAIRT.transform.position + vdir1 * maxDis;
						break;
					}
					if (!Physics.Raycast (pEntity.GetComponent<BaseAIComponent> ().mAIRT.transform.position, vdir2, out hit, maxDis, layoutmask)) 
					{
						pEntity.GetComponent<ObstacleComponent> ().target = pEntity.GetComponent<BaseAIComponent> ().mAIRT.transform.position + vdir2 * maxDis;
						break;
					}
				}
			}
			float tdis = Vector3.Distance (pEntity.GetComponent<ObstacleComponent>().target,pEntity.AIPos);
			if (tdis < 0.15f)
			{
				pEntity.GetComponent<AIMove> ().mDirection = Vector3.zero;
				pEntity.GetComponent<AIMove> ().mVelocity = 0.0f;
				return;
			}
			Vector3 tdir = pEntity.GetComponent<ObstacleComponent> ().target - pEntity.AIPos;
			tdir.y = 0.0f;
			pEntity.GetComponent<AIMove> ().mDirection = tdir.normalized;
			pEntity.GetComponent<AIMove> ().mVelocity = 5.0f;
		}
			
		public static float FSM_Battle_Avoid(AIEntity pEntity)
		{
			if (pEntity.GetComponent<ObstacleComponent> ().hitObject.tag != "Battleable") 
			{
				return 1.0f;
			}
			else
			{
				return 0.0f;
			}
		}

		public static float FSM_Avoid_Battle(AIEntity pEntity)
		{
			if (pEntity.GetComponent<ObstacleComponent> ().hitObject.tag == "Battleable") 
			{
				return 1.0f;
			}
			else
			{
				return 0.0f;
			}
		}

	};

	public class EmptyStrategyFeedbacker
	{
		public static float Run(AIEntity pEntity)
		{
			return 0.0f;
		}	
	};

	public class EmptyStrategyEnter
	{
		public static void Run(AIEntity pEntity)
		{
			
		}
	};

	public class EmptyStrategyExit
	{
		public static void Run(AIEntity pEntity)
		{
			
		}
	};

	/*
	 * every Strategy should bind to AIStrategy
	 * every Strategy should bind with a AIState also
	 * 
	*/
	public class AIStrategy:UComponent
	{
		// all Strategy this AI set
		public StrategyActioner[] mStrategyActioner;
		public StrategyFeedbacker[] mStrategyFeedbacker;
		public StrategyEnter[] mStrategyEnter;
		public StrategyExit[] mStrategyExit;
		// all AIState bind with these Strategy 
		public AIState[] mAIState;
		public float[] mPower;
		private int maxCount = 25;
		public int tempCount =0;
		public  int tempID;
		// cache 
		public  int IDBuffer;
		public  int BufferFrame = 0;
		public  int mFrameCaptureCounter = 10;
		// frame data using in feedback
		public float[] bufferdata = new float[10];
		public bool mFrameCaptureStart = false;
		public int LastID;
		public float timer;

		public string[] mStateName;

		public string mTempName
		{
			get
			{
				return mStateName [tempID];
			}
		}
		// init with all array
		public override void Init ()
		{
			base.Init ();
			mStrategyActioner = new StrategyActioner[maxCount];
			mStrategyFeedbacker = new StrategyFeedbacker[maxCount];
			mStrategyEnter = new StrategyEnter[maxCount];
			mStrategyExit = new StrategyExit[maxCount];
			mAIState = new AIState[maxCount];
			mPower = new float[maxCount];
			mStateName = new string[maxCount];
			for (int i = 0; i < maxCount; i++) 
			{
				mPower[i] = 1.0f;
			}
			IDBuffer = -1;
			//InitAvoid ();
		}
		// add a Strategy bind with a AIState
		public int AddStrategy(StrategyActioner pStrategyActioner,StrategyEnter pStrategyEnter,StrategyExit pStrategyExit,StrategyFeedbacker pStrategyFeedbacker,AIState pAIState)
		{
			if (tempCount < maxCount)
			{
				mStrategyActioner [tempCount] = pStrategyActioner;
				mStrategyFeedbacker [tempCount] = pStrategyFeedbacker;
				mStrategyEnter [tempCount] = pStrategyEnter;
				mStrategyExit [tempCount] = pStrategyExit;
				mAIState[tempCount] = pAIState;
				tempCount++;
				return tempCount-1;
			}
			return -1;
		}

		public void AddName(int pid,string pName)
		{
			mStateName [pid] = pName;
		}

		public int AddStrategy(StrategyActioner pStrategyActioner,AIState aiState)
		{
			return AddStrategy (pStrategyActioner,EmptyStrategyEnter.Run,EmptyStrategyExit.Run,EmptyStrategyFeedbacker.Run,aiState);
		}
		// set a entry point for AIStrategy
		public void SetEntry(int pID)
		{
			tempID = pID;
			mUEntity.GetComponent<AIState> ().SimpleClone (mAIState[tempID]);
			mAIState [tempID].mEnterer [mAIState [tempID].tempID]((AIEntity)mUEntity);
		}
		// no using now 
		public void InitAvoid(StateExecuter pStateExecuter,StateEnter pStateEnter,StateExit pStateExit,StateRecorder pStateRecorder,AIEntity pLast)
		{
			StrategyActioner AvoidActioner = ObstacleAvoidance.ActionFunc;
			StateExecuter AvoidState = ObstacleAvoidance.FSM_Avoid;
			AIState aiState = new AIState ();
			aiState.Init ();
			int id_battle = aiState.AddExecuter (pStateExecuter,pStateExit,pStateEnter);
			int id_avoid = aiState.AddExecuter (AvoidState,EmptyExitAndEnter.EmptyExit,EmptyExitAndEnter.EmptyEnter);
			aiState.AddAnimation (pStateExecuter,"Attack");
			aiState.AddAnimation (AvoidState,"Walk");
			aiState.tempID = id_avoid;
			StateTranfer tAvoid_Battle = ObstacleAvoidance.FSM_Avoid_Battle;
			StateTranfer tBattle_Avoid = ObstacleAvoidance.FSM_Battle_Avoid;
			aiState.AddEdge (tAvoid_Battle,EmptyFeedbacker.Run,id_avoid,id_battle);
			aiState.AddEdge (tBattle_Avoid,EmptyFeedbacker.Run,id_battle,id_avoid);
			StrategyEnter tAvoidEnter = ObstacleAvoidance.Strategy_Enter;
			aiState.mStateRecorder = pStateRecorder;
			aiState.LastEntityData = pLast;
			AddStrategy (AvoidActioner,tAvoidEnter,EmptyStrategyExit.Run,EmptyStrategyFeedbacker.Run,aiState);
		}

	};



	public struct actionNode
	{
		public int mid;
		public float action;
	};

	// every AIEntity having AIStrategy should be control by this System

	public class StrategyController:USystem
	{
		public override void Init ()
		{
			base.Init ();
			this.AddRequestComponent (typeof(AIStrategy));
			this.AddRequestComponent (typeof(AIState));
		}

		public override void Update (UEntity uEntity)
		{
			base.Update (uEntity);
			if (!uEntity.GetComponent<AIStrategy> ().isEnable)
				return;
			AIEntity pEntity = (AIEntity)uEntity;
			// get lod now 
			int lod = uEntity.GetComponent<LODComponent> ().mLOD;
			// compute logic update velocity
			lod = lod * lod * lod;
			// update logic with lod 
			if (pEntity.GetComponent<AIStrategy> ().timer <= 0.2f*lod) 
			{
				pEntity.GetComponent<AIStrategy> ().timer += Time.deltaTime; 
				return;
			}

			pEntity.GetComponent<AIStrategy> ().timer = 0.0f;
			// having cache ?
			if (pEntity.GetComponent<AIStrategy> ().IDBuffer != -1) 
			{
				if (pEntity.GetComponent<AIStrategy> ().BufferFrame != 0) 
				{
					pEntity.GetComponent<AIStrategy> ().BufferFrame--;
				}
				else
				{
					pEntity.GetComponent<AIStrategy> ().IDBuffer = -1;
				}
				return;
			}				
			//the min value of the strategy action 
			float minValue = 0.15f;
			// the max
			actionNode tActionNode1 = new actionNode ();
			tActionNode1.action = 0.0f;
			tActionNode1.mid = -1;
			// the second max
			actionNode tActionNode2 = new actionNode ();
			tActionNode2.action = 0.0f;
			tActionNode2.mid = -1;

			for (int i = 0; i < pEntity.GetComponent<AIStrategy> ().tempCount; i++)
			{
				float tempRate = pEntity.GetComponent<AIStrategy> ().mStrategyActioner [i](pEntity);
				tempRate *= pEntity.GetComponent<AIStrategy> ().mPower [i];
				if (tempRate > tActionNode1.action)
				{
					tActionNode2.action = tActionNode1.action;
					tActionNode2.mid = tActionNode1.mid;
					tActionNode1.action = tempRate;
					tActionNode1.mid = i;
				}
				else if (tempRate > tActionNode2.action)
				{
					tActionNode2.action = tempRate;
					tActionNode2.mid = i;
				}
			}

			if (tActionNode1.action > minValue) 
			{
				if (tActionNode1.mid == pEntity.GetComponent<AIStrategy> ().tempID)
				{
					return;
				}
				// transfer, so compute the last frame data
				if (pEntity.GetComponent<AIStrategy> ().mFrameCaptureCounter != 10)
				{
					float sum = 0.0f;
					for (int i = 0; i < 10 - pEntity.GetComponent<AIStrategy> ().mFrameCaptureCounter; i++) 
					{
						sum += pEntity.GetComponent<AIStrategy> ().bufferdata [i];
					}
					sum /= 10 - pEntity.GetComponent<AIStrategy> ().mFrameCaptureCounter;
					pEntity.GetComponent<AIStrategy> ().mPower [pEntity.GetComponent<AIStrategy> ().LastID] += sum;
					if (pEntity.GetComponent<AIStrategy> ().mPower [pEntity.GetComponent<AIStrategy> ().LastID] > 3.0f)
						pEntity.GetComponent<AIStrategy> ().mPower [pEntity.GetComponent<AIStrategy> ().LastID] = 3.0f;
					if (pEntity.GetComponent<AIStrategy> ().mPower [pEntity.GetComponent<AIStrategy> ().LastID] < 0.3f)
						pEntity.GetComponent<AIStrategy> ().mPower [pEntity.GetComponent<AIStrategy> ().LastID] = 0.3f;
					pEntity.GetComponent<AIStrategy> ().mFrameCaptureCounter = 10;
				}
				pEntity.GetComponent<AIStrategy> ().LastID = pEntity.GetComponent<AIStrategy> ().tempID;
				pEntity.GetComponent<AIStrategy> ().mFrameCaptureStart = true;

				for (int i = 0; i < pEntity.GetComponent<AIState> ().mtempCount; i++) 
				{
					for (int j = 0; j < pEntity.GetComponent<AIStrategy> ().mAIState [pEntity.GetComponent<AIStrategy> ().tempID].mPowerEdge [i].Count; j++) 
					{
						PowerNode pnt = new PowerNode ();
						pnt.id = pEntity.GetComponent<AIState> ().mPowerEdge [i] [j].id;
						pnt.power = pEntity.GetComponent<AIState> ().mPowerEdge [i] [j].power;
						pEntity.GetComponent<AIStrategy> ().mAIState [pEntity.GetComponent<AIStrategy> ().tempID].mPowerEdge [i] [j] = pnt;
					}
				}
				// update Strategy 
				pEntity.GetComponent<AIStrategy> ().mStrategyExit[pEntity.GetComponent<AIStrategy>().tempID](pEntity);
				pEntity.GetComponent<AIStrategy> ().SetEntry (tActionNode1.mid);
				pEntity.GetComponent<AIStrategy> ().mStrategyEnter[pEntity.GetComponent<AIStrategy>().tempID](pEntity);
				// update cache 
				if (tActionNode1.action - tActionNode2.action > 0.3f)
				{
					pEntity.GetComponent<AIStrategy> ().IDBuffer = pEntity.GetComponent<AIStrategy> ().tempID;
					pEntity.GetComponent<AIStrategy> ().BufferFrame = 6;
				}
			}
		}

	}

	public class StrategyCapturer:USystem
	{
		public override void Init ()
		{
			base.Init ();
			this.AddRequestComponent (typeof(AIStrategy));
			this.AddRequestComponent (typeof(AIState));
		}
		// the some as AIState
		public override void Update (UEntity uEntity)
		{
			base.Update (uEntity);
			if (uEntity.GetComponent<AIStrategy> ().mFrameCaptureStart) 
			{
				if (uEntity.GetComponent<AIStrategy> ().mFrameCaptureCounter == 0) 
				{
					uEntity.GetComponent<AIStrategy> ().mFrameCaptureStart = false;
					return;
				}
				int tempID = uEntity.GetComponent<AIStrategy> ().LastID;
				StrategyFeedbacker tempFeedbacker = uEntity.GetComponent<AIStrategy> ().mStrategyFeedbacker [tempID];
				float rate1 = tempFeedbacker ((AIEntity)uEntity);
				float rate2 = tempFeedbacker (uEntity.GetComponent<AIState>().LastEntityData);
				uEntity.GetComponent<AIStrategy> ().bufferdata [10 - uEntity.GetComponent<AIStrategy> ().mFrameCaptureCounter] = rate1 - rate2;
			}
		}

	};

	public class StrategyComputer:USystem
	{
		
		public override void Init ()
		{
			base.Init ();
			this.AddRequestComponent (typeof(AIStrategy));
		}

		// the same as AIState
		public override void Update (UEntity uEntity)
		{
			base.Update (uEntity);
			if (uEntity.GetComponent<AIStrategy> ().mFrameCaptureCounter == 0) 
			{
				float sum = 0.0f;
				for (int i = 0; i < 10; i++) 
				{
					sum += uEntity.GetComponent<AIStrategy> ().bufferdata [i];
				}
				sum /= 10.0f;
				uEntity.GetComponent<AIStrategy> ().mPower [uEntity.GetComponent<AIStrategy> ().LastID] += sum;
				uEntity.GetComponent<AIStrategy> ().mFrameCaptureCounter = 10;
				uEntity.GetComponent<AIStrategy> ().mFrameCaptureStart = false;
			}
		}

	}

};