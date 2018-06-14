using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityECS;
using System.Reflection;
using FreedomAI;

namespace FreedomAI
{

	// the delegate of StateEnter function 
	public delegate void StateEnter(AIEntity pEntity);

	// the delegate of StateExit function 
	public delegate void StateExit(AIEntity pEntity);

	// the delegate of StateExecuter function 
	public delegate void StateExecuter(AIEntity pEntity);

	// the delegate of StateTranfer function 
	public delegate float StateTranfer(AIEntity pEntity);

	// the delegate of StateFeedback function 
	public delegate float StateFeedbacker(AIEntity pEntity,bool isNow);

	// the delegate of StateRecorder function 
	public delegate void StateRecorder(AIEntity pEntity);

	// the delegate of AnimationPlay function
	public delegate void AnimationPlay(Animator pAnimator);

	// the delegate of EmotionExecuter function 
	public delegate float EmotionExecuter(AIEntity pEntity);

	public struct TranferNode
	{
		public StateTranfer mTranfer;
		public int id;
	}

	public struct FeedbackerNode
	{
		public StateFeedbacker mFeedbacker;
		public int id;
	}

	public struct PowerNode
	{
		public float power;
		public int id;
	}

	public class EmptyTranfer
	{
		public static float Run(AIEntity pEntity)
		{
			return 0.0f;
		}
	}

	public class EmptyExitAndEnter
	{
		public static void EmptyExit(AIEntity pEntity)
		{
			
		}

		public static void EmptyEnter(AIEntity pEntity)
		{
			
		}

	}

	public class EmptyFeedbacker
	{
		public static float Run(AIEntity pEntity,bool isNow)
		{
			return 0.0f;
		}
	}

	public class AIStateRecord
	{
		public void Run(AIEntity pEntity)
		{
			pEntity.GetComponent<AIState> ().LastEntityData.GetComponent<AIMove> ().mDirection = pEntity.GetComponent<AIMove> ().mDirection;
			pEntity.GetComponent<AIState> ().LastEntityData.GetComponent<AIMove> ().mVelocity = pEntity.GetComponent<AIMove> ().mVelocity;
			pEntity.GetComponent<AIState> ().LastEntityData.GetComponent<AIMove> ().mMoveFunc = pEntity.GetComponent<AIMove> ().mMoveFunc;
			pEntity.GetComponent<AIState> ().LastEntityData.AIPos = pEntity.AIPos;
			pEntity.GetComponent<AIState> ().LastEntityData.PlayerPos = pEntity.PlayerPos;
			pEntity.GetComponent<AIState> ().LastEntityData.GetComponent<AIAnimation> ().mtempAnim = pEntity.GetComponent<AIAnimation> ().mtempAnim;
		}
	}

	/*
	 * 
	 * the main data structure of FSM ALG.
	 * 
	*/

	public class AIState:UComponent
	{
		public StateExecuter[] mExecuter;
		public StateExit[] mExiter;
		public StateEnter[] mEnterer;
		public List<TranferNode>[] mTranfer;
		public List<FeedbackerNode>[] mFeedbacker;
		public List<PowerNode>[] mPowerEdge;
		public UEntity LastPlayerEntity;
		public AIEntity LastEntityData;

		// a dictionary for node id to animation name
		public Dictionary<int,string> mStateAnimation;
		//public List<UComponent> LastComponent;
		public int mMaxCount = 25;
		public int mtempCount = 0;
		public int tempID = 0;
		public int mCaptureFrame = 0;
		public bool mFeedbackerState = false;
		// frame data of feedback
		public float[] mframebuffer = new float[10];
		public StateFeedbacker mTempFeedbacker;
		public int mid_fir, mid_sec;
		public uint IAnyway;
		public StateRecorder mStateRecorder;
		public float timer = 0.0f;
		public string mName = "";

		public string[] mStateName;

		public string mTempName
		{
			get
			{
				return mStateName[tempID];
			}
		}

		// init all array 
		public override void Init ()
		{
			mStateAnimation = new Dictionary<int, string> ();

			mExiter = new StateExit[25];

			mEnterer = new StateEnter[25];

			mExecuter = new StateExecuter[25];

			mTranfer = new List<TranferNode>[25];

			mStateName = new string[25];

			for (int i = 0; i < 25; i++) 
			{
				mTranfer [i] = new List<TranferNode> ();
			}

			mFeedbacker = new List<FeedbackerNode>[25];
			for (int i = 0; i < 25; i++)
			{
				mFeedbacker [i] = new List<FeedbackerNode> ();
			}

			mPowerEdge = new List<PowerNode>[25];
			for (int i = 0; i < 25; i++) 
			{
				mPowerEdge [i] = new List<PowerNode> ();
			}

			LastPlayerEntity = new UEntity ();

			LastEntityData = new AIEntity ();
			LastEntityData.PlayerEntity = LastPlayerEntity;
			if(mUEntity!=null)
				LastEntityData.mWorld = this.mUEntity.mWorld;
			IAnyway = (uint)mtempCount;
			mtempCount++;
		}

		// add a animation for the state node
		public bool AddAnimation(StateExecuter pExecuter,string pName)
		{
			int index = -1;
			for (int i = 0; i < mtempCount; i++)
			{
				if (mExecuter [i] == pExecuter)
				{
					index = i;
					break;
				}
			}
			if (index != -1) 
			{
				mStateAnimation.Add (index,pName);
				return true;
			}
			else
			{
				return false;
			}
		}

		public void AddStateName(int pid,string pName)
		{
			mStateName [pid] = pName;
		}

	
		// add state node
		public int AddExecuter(StateExecuter pExecuter,StateExit pExit,StateEnter pEnter)
		{
			mExecuter [mtempCount] = pExecuter;
			mExiter [mtempCount] = pExit;
			mEnterer [mtempCount] = pEnter;
			mtempCount++;
			return mtempCount-1;
		}

		// add a transfer about a node to another node
		public void AddEdge(StateTranfer pTranfer,StateFeedbacker pFeedbacker,int id1,int id2)
		{
			TranferNode tn = new TranferNode ();
			tn.id = id2;
			tn.mTranfer = pTranfer;

			FeedbackerNode fn = new FeedbackerNode ();
			fn.id = id2;
			fn.mFeedbacker = pFeedbacker;

			PowerNode pn = new PowerNode ();
			pn.id = id2;
			pn.power = 1.0f;

			mTranfer [id1].Add (tn);
			mFeedbacker [id1].Add (fn);
			mPowerEdge [id1].Add (pn);
		}

		// add two transferes : this node to another node,another node to this node
		public void AddDoubleEdge(StateTranfer pfir2sec,StateTranfer psec2fir,StateFeedbacker pfeedbacker1,StateFeedbacker pfeedbacker2,int id1,int id2)
		{
			AddEdge (pfir2sec,pfeedbacker1,id1,id2);
			AddEdge (psec2fir,pfeedbacker2,id2,id1);
		}

		// another param 
		public void AddEdge(StateTranfer pTranfer,StateFeedbacker pFeedbacker,StateExecuter exe1,StateExecuter exe2)
		{
			int id1 = -1;
			int id2 = -1;
			for (int i = 0; i < 25; i++) 
			{
				if (mExecuter [i] == exe1) 
				{
					id1 = i;
				}
				if (mExecuter [i] == exe2)
				{
					id2 = i;
				}
				if (id1 != -1 && id2 != -1) 
				{
					AddEdge (pTranfer,pFeedbacker,id1,id2);
					break;
				}
			}
		}

		// add a transfer with any node to this node 
		public void AddAnywayTranfer(StateTranfer pStateTranfer,StateFeedbacker pFeedbacker,int id)
		{
			AddEdge (pStateTranfer,pFeedbacker,0,id);
		}

		// SimpleClone from a AIState, using in AIStrategy
		public void SimpleClone(AIState pState)
		{
			
			mtempCount = pState.mtempCount;
			tempID = pState.tempID;
			mCaptureFrame = pState.mCaptureFrame;
			mFeedbackerState = pState.mFeedbackerState;
			for (int i = 0; i < 10; i++)
				mframebuffer [i] = pState.mframebuffer [i];
			mTempFeedbacker = pState.mTempFeedbacker;
			mid_fir = pState.mid_fir;
			mid_sec = pState.mid_sec;
			IAnyway = pState.IAnyway;
			mStateRecorder = pState.mStateRecorder;
			mStateAnimation = new Dictionary<int, string> ();
			foreach(var item in pState.mStateAnimation)
			{
				int i1 = item.Key;
				string s1 = item.Value;
				mStateAnimation.Add (i1,s1);
			}
			for (int i = 0; i < mtempCount; i++)
			{
				mPowerEdge [i].Clear ();
				for (int j = 0; j < pState.mPowerEdge [i].Count; j++)
					mPowerEdge [i].Add (pState.mPowerEdge[i][j]);
				mFeedbacker [i].Clear ();
				for (int j = 0; j < pState.mFeedbacker [i].Count; j++)
					mFeedbacker [i].Add (pState.mFeedbacker[i][j]);
				mTranfer [i].Clear ();
				for (int j = 0; j < pState.mTranfer [i].Count; j++)
					mTranfer [i].Add (pState.mTranfer[i][j]);
				mEnterer [i] = pState.mEnterer [i];
				mExiter [i] = pState.mExiter [i];
				mExecuter [i] = pState.mExecuter [i];
				mStateName [i] = pState.mStateName [i];
			}
			LastEntityData = pState.LastEntityData;
		}
			
	}

	public class StateControlSystem:USystem
	{
		
		public override void Init ()
		{
			base.Init ();
			this.AddRequestComponent (typeof(AIState));
			this.AddRequestComponent (typeof(AIAnimation));
		}

		public override void Update (UEntity uEntity)
		{
			if (!uEntity.GetComponent<AIState> ().isEnable)
				return;

			// update state node
			uEntity.GetComponent<AIState> ().mExecuter [uEntity.GetComponent<AIState> ().tempID]((AIEntity)uEntity);

			

			if (uEntity.GetComponent<AIState> ().mStateAnimation.Count!=0)
			{
				// update animation 
				string tName = uEntity.GetComponent<AIState> ().mStateAnimation [uEntity.GetComponent<AIState> ().tempID];
				uEntity.GetComponent<AIAnimation> ().mtempAnim = tName;
			}

			// get lod 
			int lod = uEntity.GetComponent<LODComponent> ().mLOD;

			lod = lod * lod * lod;

			if (uEntity.GetComponent<AIState> ().timer <= 0.2f*lod) 
			{
				uEntity.GetComponent<AIState> ().timer += Time.deltaTime;
				return;
			}

			uEntity.GetComponent<AIState> ().timer = 0.0f;	

			// check the logic update 

			// check any transfer
			for (int i = 0; i < uEntity.GetComponent<AIState> ().mTranfer[0].Count; i++)
			{
				if (uEntity.GetComponent<AIState> ().mTranfer [0] [i].id == uEntity.GetComponent<AIState> ().tempID) 
				{
					continue;
				}

				float tRate = uEntity.GetComponent<AIState> ().mTranfer [0] [i].mTranfer(((AIEntity)uEntity));
				float tPower = uEntity.GetComponent<AIState> ().mPowerEdge [0] [i].power;

				bool returning = false;
				if (tRate * tPower > 0.04f) 
				{
					if (Random.Range (0.0f, 1.0f) <= tRate * tPower) 
					{
				
						uEntity.GetComponent<AIState> ().mTempFeedbacker = uEntity.GetComponent<AIState> ().mFeedbacker [0] [i].mFeedbacker;
						if (uEntity.GetComponent<AIState> ().mCaptureFrame > 0) 
						{
							float sum = 0.0f;
							for (int j = 0; j < 10-uEntity.GetComponent<AIState> ().mCaptureFrame; j++) 
							{
								sum += uEntity.GetComponent<AIState> ().mframebuffer [j];
							}
							sum /= 10.0f-uEntity.GetComponent<AIState> ().mCaptureFrame;
							for (int j = 0; j < uEntity.GetComponent<AIState> ().mPowerEdge [uEntity.GetComponent<AIState> ().mid_fir].Count; j++) 
							{
								if (uEntity.GetComponent<AIState> ().mPowerEdge [uEntity.GetComponent<AIState> ().mid_fir] [j].id == uEntity.GetComponent<AIState> ().mid_sec)
								{
									PowerNode tpn = new PowerNode ();
									tpn.id = uEntity.GetComponent<AIState> ().mPowerEdge [uEntity.GetComponent<AIState> ().mid_fir] [j].id;
									tpn.power = uEntity.GetComponent<AIState> ().mPowerEdge [uEntity.GetComponent<AIState> ().mid_fir] [j].power + sum;
									if (tpn.power > 3.0f)
										tpn.power = 3.0f;
									if (tpn.power < 0.3f)
										tpn.power = 0.3f;
									uEntity.GetComponent<AIState> ().mPowerEdge [uEntity.GetComponent<AIState> ().mid_fir] [j] = tpn;
									break;
								}
							}
						}
						uEntity.GetComponent<AIState> ().mCaptureFrame = 10;
						uEntity.GetComponent<AIState> ().mid_fir = 0;
						uEntity.GetComponent<AIState> ().mid_sec = uEntity.GetComponent<AIState> ().mTranfer [0] [i].id;
						uEntity.GetComponent<AIState> ().mExiter [uEntity.GetComponent<AIState> ().tempID] ((AIEntity)uEntity);
						uEntity.GetComponent<AIState> ().tempID = uEntity.GetComponent<AIState> ().mTranfer [0] [i].id;
						uEntity.GetComponent<AIState> ().mEnterer [uEntity.GetComponent<AIState> ().tempID] ((AIEntity)uEntity);
						returning = true;
					}
				}

				if (returning)
				{
					return;
				}
			}

			// check the transfer with this state
			for (int i = 0; i < uEntity.GetComponent<AIState> ().mTranfer [uEntity.GetComponent<AIState> ().tempID].Count; i++) 
			{
				float tRate = uEntity.GetComponent<AIState> ().mTranfer [uEntity.GetComponent<AIState> ().tempID] [i].mTranfer(((AIEntity)uEntity));
				float tPower = uEntity.GetComponent<AIState> ().mPowerEdge [uEntity.GetComponent<AIState> ().tempID] [i].power;
				bool breaking = false;
				if (tRate * tPower > 0.04f)
				{
					if (Random.Range (0.0f, 1.0f) <= tRate * tPower) 
					{
						uEntity.GetComponent<AIState> ().mTempFeedbacker = uEntity.GetComponent<AIState> ().mFeedbacker [uEntity.GetComponent<AIState> ().tempID] [i].mFeedbacker;
						if (uEntity.GetComponent<AIState> ().mCaptureFrame > 0) 
						{
							float sum = 0.0f;
							for (int j = 0; j < 10-uEntity.GetComponent<AIState> ().mCaptureFrame; j++) 
							{
								sum += uEntity.GetComponent<AIState> ().mframebuffer [j];
							}
							sum /= 10.0f-uEntity.GetComponent<AIState> ().mCaptureFrame;
							for (int j = 0; j < uEntity.GetComponent<AIState> ().mPowerEdge [uEntity.GetComponent<AIState> ().mid_fir].Count; j++) 
							{
								if (uEntity.GetComponent<AIState> ().mPowerEdge [uEntity.GetComponent<AIState> ().mid_fir] [j].id == uEntity.GetComponent<AIState> ().mid_sec)
								{
									PowerNode tpn = new PowerNode ();
									tpn.id = uEntity.GetComponent<AIState> ().mPowerEdge [uEntity.GetComponent<AIState> ().mid_fir] [j].id;
									tpn.power = uEntity.GetComponent<AIState> ().mPowerEdge [uEntity.GetComponent<AIState> ().mid_fir] [j].power + sum;
									if (tpn.power > 3.0f)
										tpn.power = 3.0f;
									if (tpn.power < 0.3f)
										tpn.power = 0.3f;
									uEntity.GetComponent<AIState> ().mPowerEdge [uEntity.GetComponent<AIState> ().mid_fir] [j] = tpn;
									break;
								}
							}
						}
						uEntity.GetComponent<AIState> ().mCaptureFrame = 10;
						uEntity.GetComponent<AIState> ().mid_fir = uEntity.GetComponent<AIState> ().tempID;
						uEntity.GetComponent<AIState> ().mid_sec = uEntity.GetComponent<AIState> ().mTranfer [uEntity.GetComponent<AIState> ().tempID] [i].id;
						uEntity.GetComponent<AIState> ().mExiter [uEntity.GetComponent<AIState> ().tempID] ((AIEntity)uEntity);
						uEntity.GetComponent<AIState> ().tempID = uEntity.GetComponent<AIState> ().mTranfer [uEntity.GetComponent<AIState> ().tempID] [i].id;
						uEntity.GetComponent<AIState> ().mEnterer [uEntity.GetComponent<AIState> ().tempID] ((AIEntity)uEntity);
						breaking = true;
					}
				}
				if (breaking)
				{
					break;
				}
			}

		}
			
	}

	public class FrameCaptureSystem:USystem
	{
		public override void Init ()
		{
			base.Init ();
			this.AddRequestComponent (typeof(AIState));
		}
		/*
		 * if a transfer,then you should capture the next 10 frame with feedback function 
		 * 
		*/
		public override void Update (UEntity uEntity)
		{
			if (uEntity.GetComponent<AIState> ().mCaptureFrame != 0) 
			{
				int tIndex = uEntity.GetComponent<AIState> ().mCaptureFrame-1;
				float tLast = uEntity.GetComponent<AIState> ().mTempFeedbacker ((AIEntity)uEntity.GetComponent<AIState>().LastEntityData,false);
				float tNow = uEntity.GetComponent<AIState> ().mTempFeedbacker ((AIEntity)uEntity,true);
				uEntity.GetComponent<AIState> ().mframebuffer [tIndex] = Mathf.Abs(tLast)-Mathf.Abs(tNow);
				uEntity.GetComponent<AIState> ().mCaptureFrame--;
				if (uEntity.GetComponent<AIState> ().mCaptureFrame == 0) 
				{
					uEntity.GetComponent<AIState> ().mFeedbackerState = true;
				}
			}
		}

	}

	public class FrameStatistics:USystem
	{
		public override void Init ()
		{
			base.Init ();
			this.AddRequestComponent (typeof(AIState));
		}

		// feedback finish,you should compute the avg of the 10 frame data
		public override void Update (UEntity uEntity)
		{
			if (uEntity.GetComponent<AIState> ().mFeedbackerState)
			{
				float sum = 0.0f;
				for (int i = 0; i < 10; i++) 
				{
					sum += uEntity.GetComponent<AIState> ().mframebuffer [i];
				}
				sum /= 10.0f;
				for (int i = 0; i < uEntity.GetComponent<AIState> ().mPowerEdge [uEntity.GetComponent<AIState> ().mid_fir].Count; i++) 
				{
					if (uEntity.GetComponent<AIState> ().mPowerEdge [uEntity.GetComponent<AIState> ().mid_fir] [i].id == uEntity.GetComponent<AIState> ().mid_sec)
					{
						PowerNode tpn = new PowerNode ();
						tpn.id = uEntity.GetComponent<AIState> ().mPowerEdge [uEntity.GetComponent<AIState> ().mid_fir] [i].id;
						tpn.power = uEntity.GetComponent<AIState> ().mPowerEdge [uEntity.GetComponent<AIState> ().mid_fir] [i].power + sum;
						if (tpn.power > 3.0f)
							tpn.power = 3.0f;
						if (tpn.power < 0.3f)
							tpn.power = 0.3f;
						uEntity.GetComponent<AIState> ().mPowerEdge [uEntity.GetComponent<AIState> ().mid_fir] [i] = tpn;
						Debug.Log (uEntity.GetComponent<AIState> ().mid_fir+" "+uEntity.GetComponent<AIState> ().mPowerEdge [uEntity.GetComponent<AIState> ().mid_fir] [i].power+" "+uEntity.GetComponent<AIState> ().mPowerEdge [uEntity.GetComponent<AIState> ().mid_fir] [i].id);
						break;
					}
				}
	
				uEntity.GetComponent<AIState> ().mFeedbackerState = false;
			}
		}

	}


	public class StateRecordSystem:USystem
	{
		
		public override void Init ()
		{
			base.Init ();
			this.AddRequestComponent (typeof(AIState));
			power = 2000;
		}

		// fill the data to lastEntity data by Recorder function 
		public override void Update (UEntity uEntity)
		{
			uEntity.GetComponent<AIState> ().mStateRecorder ((AIEntity)uEntity);
		}

	}

	public enum MoveFunc:uint
	{
		Simple = 0,
		Complex,
		RVO,
		JUSTROTATE
	};

	public class AIMove:UComponent
	{
		public Vector3 mDirection;
		public float mVelocity;
		public MoveFunc mMoveFunc;

		public override void Init ()
		{
			base.Init ();
		}

		public override UComponent Clone ()
		{
			AIMove tAIMove = new AIMove ();
			tAIMove.mDirection = mDirection;
			tAIMove.mVelocity = mVelocity;
			tAIMove.mMoveFunc = mMoveFunc;
			return (UComponent)tAIMove;
		}

	}
	/*
	public class AIForward:UComponent
	{
		public Vector3 mForward;
	}

	public class AIForwardSystem:USystem
	{
		public override void Init ()
		{
			this.AddRequestComponent (typeof(AIForward));
			power = 2500;
		}

		public override void Update (UEntity uEntity)
		{
			base.Update (uEntity);
			Vector3 tempForward = ((AIEntity)uEntity).forward_Object.transform.position - ((AIEntity)uEntity).GetComponent<BaseAIComponent> ().mAIRT.transform.position;
			Vector3 requestForward = uEntity.GetComponent<AIForward> ().mForward;
		//	Debug.Log (tempForward.normalized.ToString()+"   "+((AIEntity)uEntity).GetComponent<BaseAIComponent> ().mAIRT.transform.forward+"   "+requestForward.normalized);


		//	((AIEntity)uEntity).GetComponent<BaseAIComponent> ().mAIRT.transform.Rotate (new Vector3(0,getAngle(tempForward.normalized,requestForward.normalized),0));
		//	((AIEntity)uEntity).GetComponent<BaseAIComponent> ().mAIRT.transform.LookAt (((AIEntity)uEntity).GetComponent<BaseAIComponent> ().mAIRT.transform.position+requestForward);

		}

		float getAngle(Vector3 src,Vector3 des)
		{
			return Vector3.Angle (new Vector3(src.x,0,src.z),new Vector3(des.x,0,des.z));
		}

	}
	*/	
	/*
	public class AIMoveSystem:USystem
	{
		
		public override void Init ()
		{
			this.AddRequestComponent (typeof(AIMove));
		//	this.AddRequestComponent (typeof(BaseAIComponent));
		}

		public override void Update (UEntity uEntity)
		{
			if (uEntity.GetComponent<AIMove> ().mVelocity==0.0f)
			{
				return;
			}
			if (uEntity.GetComponent<AIMove> ().mMoveFunc == MoveFunc.Simple) 
			{
				Vector3 mTranslation = (uEntity.GetComponent<AIMove>().mDirection.normalized*uEntity.GetComponent<AIMove>().mVelocity*Time.deltaTime);
				mTranslation = new Vector3 (mTranslation.x,0,mTranslation.z);
				GameObject tObject;
				if (uEntity.GetComponent<BaseAIComponent> () != null)
					tObject = uEntity.GetComponent<BaseAIComponent> ().mAIRT;
				else
					tObject = ((SimpleAI)uEntity).mAIRT;
				//if(tObject==)
			//	tObject.transform.forward = new Vector3(mTranslation.normalized.x,0,mTranslation.normalized.z);
			//	tObject.transform.LookAt(mTranslation+tObject.transform.position,Vector3.up);
				tObject.transform.forward = mTranslation.normalized;
				tObject.transform.position+=mTranslation.magnitude*tObject.transform.forward;
			//	Debug.Log (tObject.transform.forward.normalized.ToString()+" "+mTranslation.normalized.ToString());
			}
			else
			{
				int layermask = 1 << LayerMask.NameToLayer ("Collision");
				GameObject tObject;
				if (uEntity.GetComponent<BaseAIComponent> () != null)
					tObject = uEntity.GetComponent<BaseAIComponent> ().mAIRT;
				else
					tObject = ((SimpleAI)uEntity).mAIRT;
				Vector4 dir1v4 = Matrix4x4.Rotate(Quaternion.Euler(0,45,0))*(new Vector4(uEntity.GetComponent<AIMove> ().mDirection.x,uEntity.GetComponent<AIMove> ().mDirection.y,uEntity.GetComponent<AIMove> ().mDirection.z,1.0f));
				Vector4 dir2v4 = Matrix4x4.Rotate(Quaternion.Euler(0,-45,0))*(new Vector4(uEntity.GetComponent<AIMove> ().mDirection.x,uEntity.GetComponent<AIMove> ().mDirection.y,uEntity.GetComponent<AIMove> ().mDirection.z,1.0f));
				Vector3 dir1 = new Vector3 (dir1v4.x,dir1v4.y,dir1v4.z);
				Vector3 dir2 = new Vector3 (dir2v4.x,dir2v4.y,dir2v4.z);
				//Vector3.
				RaycastHit hit = new RaycastHit ();
				if (Physics.Raycast (tObject.transform.position, uEntity.GetComponent<AIMove> ().mDirection, out hit, 7.0f, layermask)||Physics.Raycast (tObject.transform.position,dir1, out hit, 7.0f, layermask)||Physics.Raycast (tObject.transform.position,dir2, out hit, 7.0f, layermask))
				{

					Vector3 hitNormal = hit.normal;  
					//Debug.Log (hit.transform.position+"  "+hit.normal);
					hitNormal.y = 0.0f; 
					float disToAvoid = hit.distance;  
					if (disToAvoid > 6) 
					{  
						Vector3 mTranslation = uEntity.GetComponent<AIMove> ().mDirection.normalized * uEntity.GetComponent<AIMove> ().mVelocity + hitNormal.normalized * uEntity.GetComponent<AIMove> ().mVelocity * 0.2f;
						mTranslation *= Time.deltaTime;
						tObject.transform.forward = mTranslation.normalized;
						tObject.transform.position+=mTranslation.magnitude*tObject.transform.forward;
					//	Debug.Log ("5-7");
						//Debug.Log ("5");
					}  
					else if (disToAvoid > 3f) 
					{  
						//Debug.Log ("2-5");
						Vector3 pullForce = Vector3.Cross (hitNormal, Vector3.up).normalized;  
						float PdotD = Vector3.Dot (uEntity.GetComponent<AIMove> ().mDirection.normalized, pullForce.normalized);
						if (Mathf.Abs (PdotD) < 0.05f)
							PdotD = 0.0f;
						if (PdotD> 0)
						{
							Vector3 mTranslation = uEntity.GetComponent<AIMove> ().mDirection.normalized * uEntity.GetComponent<AIMove> ().mVelocity;
							mTranslation += pullForce * uEntity.GetComponent<AIMove> ().mVelocity * 0.3f;
							mTranslation *= Time.deltaTime;
							mTranslation = new Vector3 (mTranslation.x,0,mTranslation.z);
						//	tObject.transform.forward = new Vector3(mTranslation.normalized.x,0,mTranslation.normalized.z);
							tObject.transform.forward = mTranslation.normalized;
							tObject.transform.position+=mTranslation.magnitude*tObject.transform.forward;
							//Debug.Log ("pull");
						} 
						else  
						{
							Vector3 mTranslation = uEntity.GetComponent<AIMove> ().mDirection.normalized * uEntity.GetComponent<AIMove> ().mVelocity;
							mTranslation += -pullForce * uEntity.GetComponent<AIMove> ().mVelocity * 0.3f;
							mTranslation *= Time.deltaTime;
							mTranslation = new Vector3 (mTranslation.x,0,mTranslation.z);
						//	tObject.transform.forward = new Vector3(mTranslation.normalized.x,0,mTranslation.normalized.z);
							tObject.transform.forward = mTranslation.normalized;
							tObject.transform.position+=mTranslation.magnitude*tObject.transform.forward;
							//Debug.Log ("pick");
						}
								
					}  
					else
					{ 
						//Debug.Log ("0-2");
						Vector3 paraSpeed = Vector3.Cross (hitNormal, Vector3.up).normalized; 
						float PdotD = Vector3.Dot (uEntity.GetComponent<AIMove> ().mDirection.normalized, paraSpeed);
						if (Mathf.Abs (PdotD) < 0.05f)
							PdotD = 0.0f;
						//Debug.Log (paraSpeed+"  "+uEntity.GetComponent<AIMove> ().mDirection.normalized);

							//Debug.Log (Vector3.Dot (uEntity.GetComponent<AIMove> ().mDirection.normalized, paraSpeed));
						Vector3 mTranslation = paraSpeed * uEntity.GetComponent<AIMove> ().mVelocity;
						mTranslation += uEntity.GetComponent<AIMove> ().mDirection * uEntity.GetComponent<AIMove> ().mVelocity;
						mTranslation = new Vector3 (mTranslation.x, 0, mTranslation.z);
						mTranslation *= Time.deltaTime;
							//	tObject.transform.forward = new Vector3(mTranslation.normalized.x,0,mTranslation.normalized.z);
						tObject.transform.forward = mTranslation.normalized;
						tObject.transform.position += mTranslation.magnitude * tObject.transform.forward;
							//	Debug.Log ("pick1");
						

					}  
				} 
				else
				{
					Vector3 mTranslation = (uEntity.GetComponent<AIMove>().mDirection.normalized*uEntity.GetComponent<AIMove>().mVelocity*Time.deltaTime);
					mTranslation = new Vector3 (mTranslation.x,0,mTranslation.z);
				//	tObject.transform.forward = new Vector3(mTranslation.normalized.x,0,mTranslation.normalized.z);
					tObject.transform.forward = mTranslation.normalized;
					tObject.transform.position+=mTranslation.magnitude*tObject.transform.forward;
					//Debug.Log ("pick3");
				}
			}
		}
			
	}
	*/
	public class AIAnimation:UComponent
	{
		public Animator mAnimator;
		Dictionary<string,AnimationPlay> mDictionary = new Dictionary<string, AnimationPlay> ();
		// 当前动画名字
		public string mtempAnim;

		public override UComponent Clone ()
		{
			return base.Clone ();
		}

		public override void Release ()
		{
			base.Release ();
		}

		public void Add(string pName,AnimationPlay pPlay)
		{
			mDictionary.Add (pName,pPlay);	
		}

		public AnimationPlay Get(string pName)
		{
			return mDictionary [pName];
		}

	}

	public class AIAnimationPlaySystem:USystem
	{
		public override void Init ()
		{
			this.AddRequestComponent (typeof(AIAnimation));
		}

		public override void Update (UEntity uEntity)
		{
			int lod = uEntity.GetComponent<LODComponent> ().mLOD;
			Animator tAnimator = uEntity.GetComponent<AIAnimation> ().mAnimator;

			if (tAnimator == null)
				return;
			// get lod value 
			if (lod > ((float)LODComponent.maxLOD) / 3.0f * 2.0f)
				tAnimator.enabled = false;
			else
				tAnimator.enabled = true;
			/*
			if (lod > (LODComponent.maxLOD)/2.0f)
				return;
			*/
			AnimationPlay tAnim = uEntity.GetComponent<AIAnimation> ().Get (uEntity.GetComponent<AIAnimation>().mtempAnim);
			// update animation 
			tAnim (tAnimator);
		}

	}

	public struct EmotionNode
	{
		public uint id;
		public EmotionExecuter mEmotionExecuter;
	}

	public class AIEmotion:UComponent
	{
		public uint mtempID = 0;

		public List<EmotionNode> mEmotion;

		public uint tempCount = 0;

		public Dictionary<string,uint> mDictionary;

		public Dictionary<uint,string> mInverseDictionary;

		public float timer = 0.0f;


		// temp Emotion Name
		public string mtempName
		{
			get
			{
				return mInverseDictionary[mtempID];
			}
		}

		public override void Init ()
		{
			base.Init ();
			mEmotion = new List<EmotionNode>();

			mDictionary = new Dictionary<string, uint> ();
			mInverseDictionary = new Dictionary<uint, string> ();
		}

		public void InsertEmotion(string name)
		{
			mDictionary.Add (name,tempCount);
			mInverseDictionary.Add (tempCount,name);
			tempCount++;
		}

		public void InsertEdge(string name1,EmotionExecuter pEmotionExecuter)
		{
			uint id1 = mDictionary [name1];
			EmotionNode en = new EmotionNode ();
			en.id = id1;
			en.mEmotionExecuter = pEmotionExecuter;
			mEmotion.Add (en);
		}

		public string GetTempEmotion()
		{
			return mInverseDictionary [mtempID];
		}

	}


	public class EmotionControlSystem:USystem
	{



		public override void Init ()
		{
			base.Init ();
			this.AddRequestComponent (typeof(AIEmotion));
		}

		public override void Update (UEntity uEntity)
		{
			base.Update (uEntity);

			int lod = uEntity.GetComponent<LODComponent>().mLOD;

			if (lod > LODComponent.maxLOD / 2)
				return;

			if (uEntity.GetComponent<AIEmotion> ().timer < 1.0f)
			{
				uEntity.GetComponent<AIEmotion> ().timer += Time.deltaTime;
				return;
			}

			// update emotion 
			uEntity.GetComponent<AIEmotion> ().timer = 0.0f;

			for (int i = 0; i < uEntity.GetComponent<AIEmotion> ().mEmotion.Count; i++) 
			{
				EmotionExecuter tEmotionExecuter = uEntity.GetComponent<AIEmotion> ().mEmotion[i].mEmotionExecuter;
				float tactive = tEmotionExecuter ((AIEntity)(uEntity));
				if (Random.Range(0.0f,1.0f)<tactive) 
				{
					uEntity.GetComponent<AIEmotion> ().mtempID = uEntity.GetComponent<AIEmotion> ().mEmotion[i].id;
					break;
				}
			}

		}

	}

	public class AIDataRunSystem:USystem
	{
		public override void Init ()
		{
			base.Init ();
			this.AddRequestComponent (typeof(BaseAIComponent));
		}
		public override void Update (UEntity uEntity)
		{
			base.Update (uEntity);
			if(((AIEntity)uEntity).mPlayer!=null)
				((AIEntity)uEntity).PlayerPos = ((AIEntity)uEntity).mPlayer.transform.position;
			((AIEntity)uEntity).AIPos = uEntity.GetComponent<BaseAIComponent> ().mAIRT.transform.position;
			GameObject tObject = uEntity.GetComponent<BaseAIComponent> ().mAIRT;
			uEntity.GetComponent<HPComponent> ().tempHurt = 0;
		}
	}


};
