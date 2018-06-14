using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityECS;
using FreedomAI;

namespace FreedomAI
{
	/*
	 * the ant temp target 
	 * 
	*/
	public enum NowTarget:uint
	{
		HOME = 0,
		FOOD,
		NONE
	}

	/*
	 * the ant move direction 
	 * 
	*/

	public enum DirctionType:uint
	{
		DOWN = 0,
		UP,
		LEFT,
		RIGHT,
		LEFTDOWN,
		LEFTUP,
		RIGHTDOWN,
		RIGHTUP,
		NONE
	}

	/*
	 * 
	 * ant defined
	*/

	public class Ant
	{
		// temp position index in IM
		public Vector2 mPositionIndex;
		// temp ant GameObject
		public GameObject mAIRT;
		// the information about food
		public float mInfoFood;
		// the information about home
		public float mInfoHome;
		// the distance can find food or home
		public float mObserverDistance;
		// temp target
		public NowTarget mNowTarget;
		public Vector3 mNowPositionTarget;
		public Vector2 mLastDir;

		// the tile having stay
		public List<Vector2> mClosedArray = new List<Vector2> ();


		public void Add(Vector2 v)
		{
			if (!Contains (v))
				mClosedArray.Add (v);
		}

		public bool Contains(Vector2 v)
		{
			//Debug.Log (mClosedArray.Contains(v).ToString());
			return mClosedArray.Contains (v);
		}
	//	public List<Vector2> runit;
	
	}

	/*
	 * the Ant Population class
	 * you can set param about this ALG.
	 * 
	*/

	public class AntPopulation
	{

		// all ants
		public Ant[] mAnts;

		// information in the map
		private InfluenceMap mInfluenceMap;

		// the tag of home
		public string mTagHome;

		// the tag of food
		public string mTagDistination;

		// the tag of ant
		private string mTagAnt;

		public int mListIndex;

		public string mInfluenceHomeTag
		{
			get
			{
				return mTagAnt + mTagHome;
			}
		}

		public string mInfluenceDistinationTag
		{
			get
			{
				return mTagAnt + mTagDistination;
			}
		}
		// for debug 
		public int shouldWhat = -1;

		// all the ant population set
		public static List<AntPopulation> mAntPopulationList = new List<AntPopulation> ();

		public static int mNowListCount;

		// the ant population set is active or not 
		public bool isStartFind = false;

		// no using 
		public int mRadiationRiaus;

		/*
		 * @ param pTagAnt:the tag of the Ant
		 * @ param pTagHome: the tag of the home
		 * @ param pTagDistination:the tag of the food
		 * @ param ObserverDistance:the observer distance of the ant
		*/

		public void Init(string pTagAnt,string pTagHome,string pTagDistination,int ObserverDistance)
		{
			mInfluenceMap = InfluenceMap.getInstance ();
			GameObject[] mAntRTs = GameObject.FindGameObjectsWithTag (pTagAnt);
			mAnts = new Ant[mAntRTs.Length];
			for (int i = 0; i < mAnts.Length; i++)
			{
				mAnts [i] = new Ant ();
				mAnts [i].mAIRT = mAntRTs [i];
				mAnts [i].mObserverDistance = ObserverDistance;
				mAnts [i].mNowTarget = NowTarget.NONE;
				mAnts [i].mInfoFood = -1.0f;
				mAnts [i].mInfoHome = -1.0f;
				mAnts [i].mPositionIndex = mInfluenceMap.getTilefromPosition (new Vector2(mAntRTs[i].transform.position.x,mAntRTs[i].transform.position.z));
			}
			mTagHome = pTagHome;
			mTagDistination = pTagDistination;
			mTagAnt = pTagAnt;
			mAntPopulationList.Add (this);
			mListIndex = mNowListCount;
			mNowListCount++;
			mInfluenceMap.AddConverage (0.0f,pTagAnt+pTagHome,false,0.82f);
			mInfluenceMap.AddConverage (0.0f,pTagAnt+pTagDistination,false,0.82f);
		}
		// active this ant population
		public static bool StartFind(int pIndex)
		{
			if (pIndex < mNowListCount) 
			{
				mAntPopulationList [pIndex].isStartFind = true;
				for (int i = 0; i < mAntPopulationList [pIndex].mAnts.Length; i++) 
				{
					mAntPopulationList [pIndex].mAnts [i].mNowTarget = NowTarget.FOOD;
					mAntPopulationList [pIndex].mAnts [i].mNowPositionTarget = Vector3.zero;
					mAntPopulationList [pIndex].mAnts [i].mLastDir = Vector2.zero;
				}
				return true;
			}
			return false;
		}

	}

	public class AntPopulationComponent:UComponent
	{
		public List<AntPopulation> mAntPopulationSet = AntPopulation.mAntPopulationList;
	}

	public class AntPopulationEntity:UEntity
	{
		private static AntPopulationEntity mAntPopulationInstance;

		public static AntPopulationEntity getInstance()
		{
			if (mAntPopulationInstance == null) 
			{
				mAntPopulationInstance = new AntPopulationEntity ();
			}
			return mAntPopulationInstance;
		}

		public override void Init ()
		{
			base.Init ();
			mAntPopulationInstance.AddComponent<AntPopulationComponent> (new AntPopulationComponent());
		}

	}

	public class AntPopulationSystem:USystem
	{

		
		public override void Init ()
		{
			base.Init ();
			this.AddRequestComponent (typeof(AntPopulationComponent));
		}

		public override void Update (UEntity uEntity)
		{
		//	Debug.Log ("UpdateAntSystem");

			base.Update (uEntity);


			List<AntPopulation> tList = uEntity.GetComponent<AntPopulationComponent> ().mAntPopulationSet;
			for (int i = 0; i < AntPopulation.mNowListCount; i++)
			{
				if (tList [i].isStartFind)
				{
					if (tList[i].shouldWhat == 0)
						InfluenceMap.getInstance ().Show (3);
					else if (tList[i].shouldWhat == 1)
						InfluenceMap.getInstance ().Show (4);
					UpdateAntPopulation (tList [i]);
				}	
			}
		}

		/*
		 * update all the ant operator
		 * 
		*/

		private void UpdateAntPopulation(AntPopulation pAntPopulation)
		{

			Ant[] tAnts = pAntPopulation.mAnts;
			for (int i = 0; i < tAnts.Length; i++)
			{
				tAnts [i].mPositionIndex = InfluenceMap.getInstance ().getTilefromPosition (new Vector2(tAnts[i].mAIRT.transform.position.x,tAnts[i].mAIRT.transform.position.z));
				tAnts [i].Add (tAnts[i].mPositionIndex);
				if (tAnts [i].mInfoFood > 0.0f)
					tAnts [i].mAIRT.GetComponent<MeshRenderer> ().material.color = Color.green;
				else if (tAnts [i].mInfoHome > 0.0f)
					tAnts [i].mAIRT.GetComponent<MeshRenderer> ().material.color = Color.blue;
				else
					tAnts [i].mAIRT.GetComponent<MeshRenderer> ().material.color = Color.red;

				// update ant with finding food logic or finding home logic 
				if (tAnts [i].mNowTarget == NowTarget.FOOD)
				{
					OnFood (tAnts[i],pAntPopulation);
				}
				else if (tAnts [i].mNowTarget == NowTarget.HOME) 
				{
					OnHome (tAnts[i],pAntPopulation);
				}
			}
		}

		// finding food logic 
		private void OnFood(Ant pAnt,AntPopulation ap)
		{
			Vector2 v1 = new Vector2 (pAnt.mAIRT.transform.position.x,pAnt.mAIRT.transform.position.z);
			Vector2 v2 = new Vector2 (pAnt.mNowPositionTarget.x,pAnt.mNowPositionTarget.z);

			float dis = Vector2.Distance (v1,v2);

			if (pAnt.mNowPositionTarget == Vector3.zero || dis < 0.35f) 
			{
				// update Pheromone 
				if(pAnt.mInfoHome>0.0f)
					UpdatePheromone (ap.mInfluenceHomeTag,pAnt.mInfoHome,ap.mRadiationRiaus,pAnt.mPositionIndex,1600,pAnt.mClosedArray.Count);
				GameObject g_target = FindTarget (pAnt.mAIRT.transform.position,pAnt.mObserverDistance,ap.mTagDistination);
				GameObject g_home =  FindTarget (pAnt.mAIRT.transform.position,pAnt.mObserverDistance,ap.mTagHome);
				// find the food 
				if(g_target != null)
				{
					pAnt.mInfoHome = -1.0f;
					pAnt.mInfoFood = 200.0f;
					pAnt.mNowTarget = NowTarget.HOME;
					pAnt.mNowPositionTarget = g_target.transform.position;
					pAnt.mClosedArray.Clear ();
					pAnt.mLastDir = Vector2.zero;
					return;
				}

				if (g_home != null) 
				{
					pAnt.mInfoFood = -1.0f;
					pAnt.mInfoHome = 200.0f;
				}
				// compute the new dirction 
				pAnt.mNowPositionTarget = ComputeDirction (pAnt,ap);

			}
			else
			{
				// move the ant 
				Vector3 offset = pAnt.mNowPositionTarget - pAnt.mAIRT.transform.position;
				offset.Normalize ();
				offset.y = 0;
				pAnt.mAIRT.transform.Translate (offset*Time.deltaTime*6);
			}
		}

		// finding home logic 
		private void OnHome(Ant pAnt,AntPopulation ap)
		{
			Vector2 v1 = new Vector2 (pAnt.mAIRT.transform.position.x,pAnt.mAIRT.transform.position.z);
			Vector2 v2 = new Vector2 (pAnt.mNowPositionTarget.x,pAnt.mNowPositionTarget.z);

			float dis = Vector2.Distance (v1,v2);

			//Debug.Log ("OnHome");
			if (pAnt.mNowPositionTarget == Vector3.zero || dis < 0.35f) 
			{
				if(pAnt.mInfoFood>0.0f)
					UpdatePheromone (ap.mInfluenceDistinationTag,pAnt.mInfoFood,ap.mRadiationRiaus,pAnt.mPositionIndex,1600,pAnt.mClosedArray.Count);
				GameObject g_target = FindTarget (pAnt.mAIRT.transform.position,pAnt.mObserverDistance,ap.mTagDistination);
				GameObject g_home =  FindTarget (pAnt.mAIRT.transform.position,pAnt.mObserverDistance,ap.mTagHome);
				if (g_home != null)
				{
					pAnt.mInfoFood = -1.0f;
					pAnt.mInfoHome = 200.0f;
					pAnt.mNowTarget = NowTarget.FOOD;
					pAnt.mNowPositionTarget = g_home.transform.position;
					pAnt.mClosedArray.Clear ();
					pAnt.mLastDir = Vector2.zero;
					return;
				}

				if (g_target != null)
				{
					pAnt.mInfoFood = 200.0f;
					pAnt.mInfoHome = -1.0f;
				}

				pAnt.mNowPositionTarget = ComputeDirction (pAnt,ap);
			}
			else
			{
				Vector3 offset = pAnt.mNowPositionTarget - pAnt.mAIRT.transform.position;
				offset.Normalize ();
				offset.y = 0;
				pAnt.mAIRT.transform.Translate (offset*Time.deltaTime*6);
			}
		}

		// find the home or food 
		private GameObject FindTarget(Vector3 mCenter,float riaus,string code)
		{
			Collider[] tAllCollider = Physics.OverlapSphere (mCenter,riaus);
			GameObject tg = null;
			foreach (Collider c in tAllCollider)
			{
				if (c.tag == code)
				{
					tg = c.transform.gameObject;
					break;
				}
			}
			return tg;
		}

		// gauss function 
		private float Gauss(Vector2 v)
		{
			return Mathf.Exp(-(v.x*v.x/2.0f+v.y*v.y/2.0f));		
		}

		// update the Pheromone
		private void UpdatePheromone(string code,float f,int radioriaus,Vector2 center,float max,int pGoingTile)
		{
			int index = InfluenceMap.getInstance ().mDictionary [code];
			int ti = Mathf.Clamp ((int)center.x,0,InfluenceMap.getInstance ().wtileCount-1);
			int tj = Mathf.Clamp ((int)center.y,0,InfluenceMap.getInstance ().htileCount-1);
			// update the Pheromone of this tile with the going distance
			if(pGoingTile<radioriaus)
				InfluenceMap.getInstance ().IMData [index] [ti] [tj] += f * (1-(float)pGoingTile/(float)radioriaus);
			
			if (InfluenceMap.getInstance ().IMData [index] [ti] [tj] > max)
				InfluenceMap.getInstance ().IMData [index] [ti] [tj] = max;
	
		}
		// compute new dirction 
		private Vector3 ComputeDirction(Ant pAnt,AntPopulation ap)
		{
			
			Vector3 result = Vector3.zero;

			Vector2 resultNineGrid = Vector2.zero;

			int index = -1;

			if (pAnt.mNowTarget == NowTarget.FOOD)
				index = InfluenceMap.getInstance ().mDictionary [ap.mInfluenceDistinationTag];
			else
				index = InfluenceMap.getInstance ().mDictionary [ap.mInfluenceHomeTag];
			
			float tMax = InfluenceMap.getInstance ().getMax (pAnt.mPositionIndex,1,index);

			// find the information 
			if (tMax > 0.0f) 
			{
				//Debug.Log (pAnt.mNowTarget.ToString()+" "+tMax+" infoFood:"+pAnt.mInfoFood+" infoHome"+pAnt.mInfoHome);

				float minusRate = 0.0002f;
				if (Random.Range (0.0f, 1.0f) < minusRate) 
				{
					// just a little rate for random dir 
					int RandomIndex = Random.Range(0,InfluenceMap.getInstance().NineGrid.Length);

					resultNineGrid = InfluenceMap.getInstance ().NineGrid [RandomIndex];

					if (pAnt.mLastDir != Vector2.zero)
						resultNineGrid = pAnt.mLastDir;

					float minusRate_ = 0.2f;

					if (Random.Range (0.0f, 1.0f) < minusRate_)
					{
						RandomIndex = Random.Range(0,InfluenceMap.getInstance().NineGrid.Length);
						resultNineGrid = resultNineGrid = InfluenceMap.getInstance ().NineGrid [RandomIndex];
					}

					int count = 0;

					while ((InfluenceMap.getInstance().isWall(resultNineGrid+pAnt.mPositionIndex)||pAnt.Contains(resultNineGrid+pAnt.mPositionIndex))&&count<10) 
					{
						RandomIndex = Random.Range(0,InfluenceMap.getInstance().NineGrid.Length);
						resultNineGrid = InfluenceMap.getInstance ().NineGrid [RandomIndex];
						count++;
					}
						
				}
				else
				{
					// compute the new dir by RW with rate

					float sum = 0.0f;

					float max = -1.0f;
					for (int i = 0; i < InfluenceMap.getInstance ().NineGrid.Length; i++)
					{
						Vector2 delta = InfluenceMap.getInstance ().NineGrid [i];
						Vector2 real = pAnt.mPositionIndex + delta;
						if(real.x>=0&&real.x<InfluenceMap.getInstance().wtileCount&&real.y>=0&&real.y<InfluenceMap.getInstance().htileCount)
							sum += InfluenceMap.getInstance ().IMData [index] [(int)real.x] [(int)real.y];
					}

					int index1 = Random.Range(0,InfluenceMap.getInstance ().NineGrid.Length);

					for (int i = 0; i < InfluenceMap.getInstance ().NineGrid.Length; i++)
					{
						int realI = (i + index1) % InfluenceMap.getInstance ().NineGrid.Length;
						Vector2 delta = InfluenceMap.getInstance ().NineGrid [realI];
						Vector2 real = pAnt.mPositionIndex + delta;
						float tRate = 0.0f;

						if(real.x>=0&&real.x<InfluenceMap.getInstance().wtileCount&&real.y>=0&&real.y<InfluenceMap.getInstance().htileCount)
							tRate = InfluenceMap.getInstance ().IMData [index] [(int)real.x] [(int)real.y] / sum;
						
						if (Random.Range (0.0f, 1.0f) < tRate&&!InfluenceMap.getInstance().isWall(delta+pAnt.mPositionIndex))
						{
							resultNineGrid = new Vector2 (delta.x,delta.y);
							break;
						}

					}

				
				}
			}
			else
			{

				// no find the information so the new dirtion is a random dir
				int RandomIndex = Random.Range(0,InfluenceMap.getInstance().NineGrid.Length);

				resultNineGrid = InfluenceMap.getInstance ().NineGrid [RandomIndex];

				if (pAnt.mLastDir != Vector2.zero)
					resultNineGrid = pAnt.mLastDir;

				float minusRate_ = 0.2f;

				if (Random.Range (0.0f, 1.0f) < minusRate_)
				{
					RandomIndex = Random.Range(0,InfluenceMap.getInstance().NineGrid.Length);
					resultNineGrid = resultNineGrid = InfluenceMap.getInstance ().NineGrid [RandomIndex];
				}

				int count = 0;

				while ((InfluenceMap.getInstance().isWall(resultNineGrid+pAnt.mPositionIndex)||pAnt.Contains(resultNineGrid+pAnt.mPositionIndex))&&count<10) 
				{
					RandomIndex = Random.Range(0,InfluenceMap.getInstance().NineGrid.Length);
					resultNineGrid = InfluenceMap.getInstance ().NineGrid [RandomIndex];
					count++;
				}
				//Debug.Log (pAnt.Contains(resultNineGrid+pAnt.mPositionIndex));
			}

			pAnt.mLastDir = new Vector2 (resultNineGrid.x,resultNineGrid.y);

		
			float tx = pAnt.mPositionIndex.x + resultNineGrid.x;
			float ty = pAnt.mPositionIndex.y + resultNineGrid.y;

			tx = Mathf.Clamp (tx,0,InfluenceMap.getInstance().wtileCount-1);
			ty = Mathf.Clamp (ty,0,InfluenceMap.getInstance().htileCount-1);

			Vector2 tile = new Vector2 (tx,ty);

		//	Debug.Log (pAnt.Contains(tile));

			result = InfluenceMap.getInstance ().GetPositionByTile (tile);

			return result;
		}

	}






};
