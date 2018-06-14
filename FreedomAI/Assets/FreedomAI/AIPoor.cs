using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityECS;

/*
 * 
 * this Part is about AIEntity and AIPoor
 * 
 * AIEntity is a Entity be definded as AI.
 * 
 * AIPoor is the TEQ about Pooring 
 * 
*/

namespace FreedomAI
{

	/*
	 * the Base Component of AIEntity
	 * 
	 * every AIEntity have BaseAIComponent
	 * 
	 * the AI GameObject can be Find in this
	 * 
	*/

	public class BaseAIComponent:UComponent
	{
		// the AI Object Prefab
		public GameObject mAITemplate;
		// the AI GameObject
		public GameObject mAIRT;
		public int mPoorID = -1;

		// the Born Point  
		public Vector3 bornPoint = new Vector3();

		public override void Release ()
		{
			GameObject.Destroy (mAIRT);
		}

		/*
		 *  create the AI GameObject from mAITemplate
		 * 
		*/

		public override void Init ()
		{
			if (mAITemplate != null) 
			{
				mAIRT = GameObject.Instantiate (mAITemplate, bornPoint, mAITemplate.transform.rotation) as GameObject;
				mAIRT.SetActive (false);
			}
		}

	}

	public struct HurtEvent
	{
		public UEntity fir;
		public UEntity sec;
		public int hurtnumber;
	};

	public class HurtManager:UComponent
	{
		public Queue<HurtEvent> mHurtEventList = new Queue<HurtEvent>();
		public void Add(UEntity fir,UEntity sec,int number)
		{
			HurtEvent he=new HurtEvent();
			he.fir = fir;
			he.sec = sec;
			he.hurtnumber = number;
			mHurtEventList.Enqueue (he);
		}
	}

	public class HurtControlSystem:USystem
	{
		public override void Init ()
		{
			base.Init ();
			this.AddRequestComponent (typeof(HurtManager));
			power = 3000;
		}
		public override void Update (UEntity uEntity)
		{
			base.Update (uEntity);
			while(uEntity.GetComponent<HurtManager> ().mHurtEventList.Count!=0)
			{
				HurtEvent the = uEntity.GetComponent<HurtManager> ().mHurtEventList.Dequeue ();
				the.fir.GetComponent<HPComponent> ().tempHurt = the.hurtnumber;
				the.sec.GetComponent<HPComponent> ().tempHP -= the.hurtnumber;
			}
		}
	}

	public class HPComponent:UComponent
	{
		public int allHP;
		public int tempHP;
		public int tempHurt;
	}

	/*
	 * 
	 * the AI Type using by GroupBehavour
	 * 
	 * RESPONSE: the AI is a response
	 * 
	 * SPONSOR: the AI is a Sponsor
	 * 
	 * NONE: the AI is a not a response or a sponsor
	 * 
	*/

	public enum GroupMemberType
	{
		RESPONSE,
		SPONSOR,
		NONE
	}

	/*
	 * 
	 * if you want create a AI, you must create a AIEntity
	 * 
	*/

	public class AIEntity:UEntity
	{
		public GameObject mAI;
		public GameObject mPlayer;
		public Vector3 PlayerPos;
		public Vector3 AIPos;
		public Collision mCollision;
		public Collider mCollider;
		public bool mCollisionEnter = false;
		public bool mCollisionStay = false;
		public bool mCollisionExit = false;
		public bool mTriggerEnter = false;
		public bool mTriggerExit = false;
		public bool mTriggerStay = false;
		public UEntity PlayerEntity;
		public string tag;
		public bool isGrouping = false;
		public float mDissloveTimer = 0.0f;

		// AI Type using in GroupBehavour

		public GroupMemberType mMemberType
		{
			get
			{
				for (int i = 0; i < GroupManager.getInstance ().mResponseCount; i++)
				{
					if (GroupManager.getInstance ().mResponses [i].mResponse == this)
						return GroupMemberType.RESPONSE;
				}

				for (int i = 0; i < GroupManager.getInstance ().mSponsorCount; i++)
				{
					if (GroupManager.getInstance ().mSponsors [i].mSponsor == this)
						return GroupMemberType.SPONSOR;
				}

				return GroupMemberType.NONE;

			}
		}

		// all Entity set 

		private static List<AIEntity> mAllEntity;


		public static List<AIEntity> getList()
		{
			if (mAllEntity == null)
				mAllEntity = new List<AIEntity> ();
			return mAllEntity;
		}

		/*
		 * @param pEntity: Find center Entity
		 * @param riaus:Find riaus
		 * 
		 * get all AIEntity in the riaus by the center of Entity
		 * 
		 * @return:the OK AIEntity set
		 * 
		*/

		public static AIEntity[] getAllEntityWithSphere(AIEntity pEntity,float riaus)
		{
			Collider[] tColliders = Physics.OverlapSphere (pEntity.AIPos,riaus);
			List<AIEntity> tList = new List<AIEntity> ();
			for (int i = 0; i < tColliders.Length; i++) 
			{
				foreach (AIEntity aie in AIEntity.getList())
				{
					if (aie.GetComponent<BaseAIComponent>().mAIRT == tColliders [i].gameObject && aie.GetComponent<BaseAIComponent>().mAIRT != pEntity.GetComponent<BaseAIComponent>().mAIRT)
					{
						tList.Add (aie);
						break;
					}
				}
			}
			return tList.ToArray ();
		}

		public void Destroy()
		{
			AIEntity.getList ().Remove (this);
			GameObject.Destroy (mAI);
			Release ();
		}

		/*
		 * 
		 * add the must component to AIEntity 
		 * 
		*/

		public override void Init ()
		{
			BaseAIComponent tBAIComponent = new BaseAIComponent ();
			tBAIComponent.mAITemplate = mAI;
			tBAIComponent.bornPoint = AIPos;
			tBAIComponent.Init ();
			AIMove mAIMove = new AIMove ();
			AIAnimation mAIAnimation = new AIAnimation ();
			HPComponent hpc = new HPComponent ();
			AIState aiState = new AIState ();
			AIEmotion mEmotion = new AIEmotion ();
			AIStrategy mAIStrategy = new AIStrategy (); 
			ObstacleComponent mObstacleComponent = new ObstacleComponent ();
			AIGroupState aiGroupState = new AIGroupState ();

			this.AddComponent<BaseAIComponent> (tBAIComponent);
			this.AddComponent<AIMove> (mAIMove);
			this.AddComponent<AIAnimation> (mAIAnimation);
			this.AddComponent<HPComponent> (hpc);
			this.AddComponent<AIState> (aiState);
			this.AddComponent<AIEmotion> (mEmotion);
			this.AddComponent<AIStrategy> (mAIStrategy);
			this.AddComponent<ObstacleComponent> (mObstacleComponent);
			this.AddComponent<AIGroupState> (aiGroupState);
			this.AddComponent<LODComponent> (new LODComponent());

			mAIStrategy.Init ();
			aiGroupState.Init ();
			aiState.Init ();
			mAIAnimation.Init ();
			mEmotion.Init ();

			InfluenceMapTrigger tInfluenceMapTrigger = new InfluenceMapTrigger ();
			tInfluenceMapTrigger.maxInfluence = 4.0f;
			tInfluenceMapTrigger.mWhere = "friend";
			tInfluenceMapTrigger.mGameObject = mAI;
			tInfluenceMapTrigger.mIMComputer = DefaultFunc.friendComputer;
			tInfluenceMapTrigger.Init ();

			UEntity tEntity = new UEntity ();
			tEntity.AddComponent<InfluenceMapTrigger> (tInfluenceMapTrigger);
			this.mWorld.registerEntityAfterInit(tEntity);
			AIEntity.getList ().Add (this);
		}
		// for Debug
		public void Log()
		{
			Color Color1 = Color.red;
			Color Color2 = Color.blue;
			int lod = this.GetComponent<LODComponent> ().mLOD;
			int alllod = LODComponent.maxLOD;
			this.GetComponent<BaseAIComponent> ().mAIRT.GetComponentInChildren<SkinnedMeshRenderer> ().material.color = Color.Lerp (Color1, Color2, (float)lod / (float)alllod);
		}
	}

	/*
	 * AI Poor 
	*/

	public class AIPoorComponent:UComponent
	{
		public GameObject mAI_Template;
		public AIEntity[] mAIPoor = new AIEntity[3000];
		public int mMaxCount = 3000;
		private int mTempCount = 0;


		public override void Init ()
		{
			for (int i = 0; i < mMaxCount; i++) 
			{
				mAIPoor [i] = new AIEntity ();
				mAIPoor [i].mAI = mAI_Template;
				mAIPoor [i].mWorld = ECSWorld.MainWorld;
				mAIPoor [i].Init ();
				mAIPoor [i].GetComponent<BaseAIComponent> ().mPoorID = i;
			}
		}

		public override void Release ()
		{
			for (int i = 0; i < mMaxCount; i++) 
			{
				mAIPoor [i].Release ();
			}
		}

		/*
		 * sleep a AI by id
		 * 
		*/

		public void DestroyEntity(int id)
		{
			mUEntity.mWorld.deleteEntity (mAIPoor[id]);
			mAIPoor [id].GetComponent<BaseAIComponent> ().mAIRT.SetActive (false);
			AIEntity temp = mAIPoor [id];
			mAIPoor [id] = mAIPoor[mTempCount-1];
			mAIPoor [mTempCount - 1] = temp;
			mAIPoor [id].GetComponent<BaseAIComponent> ().mPoorID = id;
			mAIPoor [mTempCount - 1].GetComponent<BaseAIComponent> ().mPoorID = mTempCount - 1;
			mTempCount--;
		}

		public void DestroyEntity(AIEntity pEntity)
		{
			int index = -1;
			for (int i = 0; i < 3000; i++) 
			{
				if (mAIPoor [i] == pEntity) 
				{
					index = i;
					break;
				}
			}
			if (index != -1)
			{
				DestroyEntity (index);
			}
		}

		/*
		 * wake a AI
		 * 
		*/
		public AIEntity InstantiateEntity()
		{
			mAIPoor [mTempCount].mAllBitBunch.SetCount ((int)mUEntity.mWorld.mComponentCount);
			mUEntity.mWorld.registerEntity (mAIPoor[mTempCount]);
			mAIPoor [mTempCount].GetComponent<BaseAIComponent> ().mAIRT.SetActive (true);
			mTempCount++;
			return mAIPoor [mTempCount - 1];
		}

	}




};

