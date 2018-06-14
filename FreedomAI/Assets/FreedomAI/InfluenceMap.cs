using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FreedomAI;
using UnityECS;
using System;

namespace FreedomAI
{

	// Influence Map Computer function 
	public delegate float IMComputer(UEntity uEntity);

	// A* Computer function
	public delegate float AStarComputer(Vector3 target,Vector3 des);

	public struct AStarComputerformulaNode
	{
		public AStarComputer mAStarComputer;
		public float mBlendFunc;
	};

	// using functions generate a new function to use.
	public class AStarComputerformula
	{
		List<AStarComputerformulaNode> mAStarComputerList = new List<AStarComputerformulaNode>() ;

		float blendfuncSum = 0.0f;

		// lamuda 
		public AStarComputer EquationComputer
		{
			get
			{
				return (Vector3 target, Vector3 des) => 
				{
					float rf = 0.0f;
					foreach(var v in mAStarComputerList)
					{
						float tf = v.mAStarComputer(target,des);
						rf+=tf*v.mBlendFunc/blendfuncSum;
					}
					return rf;
				};
			}
		}

		public void Add(AStarComputer pAStarComputer,float pBlendFunc)
		{
			AStarComputerformulaNode tAStarComputerformulaNode = new AStarComputerformulaNode ();
			tAStarComputerformulaNode.mAStarComputer = pAStarComputer;
			tAStarComputerformulaNode.mBlendFunc = pBlendFunc;
			mAStarComputerList.Add (tAStarComputerformulaNode);
			blendfuncSum += pBlendFunc;
		}

	};

	// default A* function and Influence Map function 
	public class DefaultFunc
	{
		public static float AStarByEulerDistance(Vector3 target,Vector3 des)
		{
			Vector2 vp = new Vector2 (target.x-des.x,target.z-des.z);
			vp = new Vector2 (vp.x/InfluenceMap.getInstance().deltaTileSize.x,vp.y/InfluenceMap.getInstance().deltaTileSize.y);
			return vp.magnitude*10;
		}

		public static float ObastecleComputer(UEntity uEntity)
		{
			return 300;
		}


		public static float friendComputer(UEntity uEntity)
		{
			return 30;
		}

		public static float RiskComputer(UEntity uEntity)
		{
			return uEntity.GetComponent<RiskComponent> ().riskrate;
		}

		public static float AStarByRiskRate(Vector3 target,Vector3 des)
		{
			Vector2 v = InfluenceMap.getInstance ().getTilefromPosition (new Vector2(target.x,target.z));
			float f = InfluenceMap.getInstance ().getData (v,"Risk");
			return f;
		}

		public static float AStarByFriendRate(Vector3 target,Vector3 des)
		{
			Vector2 v = InfluenceMap.getInstance ().getTilefromPosition (new Vector2(target.x,target.z));
			float f = InfluenceMap.getInstance ().getData (v,"friend");
			//Debug.Log (f);
			return -f*0.3f;
		}

		public static float AStarByTerrian(Vector3 target,Vector3 des)
		{
			Vector2 v = InfluenceMap.getInstance ().getTilefromPosition (new Vector2(target.x,target.z));
			float sum = 0.0f;
			for (int i = -2; i <= 2; i++)
			{
				for (int j = -2; j <= 2; j++) 
				{
					float f = InfluenceMap.getInstance ().getData (new Vector2(v.x+i,v.y+j),"Collision");
					sum += f;
				}
			}
			sum /= 25.0f;
			return -sum*0.4f;
		}

	};

	public class RiskComponent:UComponent
	{
		public float riskrate;
	};

	// DS of A*
	public class AStarNodeItem 
	{
		
		public Vector3 pos;
	
		public int x, y;

	
		public float gCost;

		public float hCost;

		// power 
		public float fCost
		{
			get 
			{
				return gCost + hCost; 
			}
		}


		public AStarNodeItem parent = null;

		public AStarNodeItem(Vector3 pos, int x, int y) 
		{
			this.pos = pos;
			this.x = x;
			this.y = y;
		}

		public AStarNodeItem()
		{
			
		}

	}

	// using in sort 
	public class AStarNodeItemCamparer:IComparer<AStarNodeItem>
	{
		public int Compare(AStarNodeItem ni1, AStarNodeItem ni2)
		{
			if (ni1.fCost > ni2.fCost)
				return -1;
			if(ni2.fCost == ni1.fCost)
			{
				return 0;    
			}
			return 1;
		}
	};

	public class CollisionEntity:UEntity
	{
		public GameObject mGameObject;
		public override void Init ()
		{
			base.Init ();
			InfluenceMapTrigger tInfluenceMapTrigger = new InfluenceMapTrigger ();
			tInfluenceMapTrigger.maxInfluence = 0.0f;
			tInfluenceMapTrigger.mWhere = "Collision";
			tInfluenceMapTrigger.mGameObject = mGameObject;
			tInfluenceMapTrigger.mIMComputer = DefaultFunc.ObastecleComputer;

			tInfluenceMapTrigger.Init ();
			this.AddComponent<InfluenceMapTrigger> (tInfluenceMapTrigger);
		}	
	};

	public class InfluenceMapSingleton:UEntity
	{
		private static InfluenceMapSingleton mInfluenceMapSingleton;

		public static InfluenceMapSingleton getInstance()
		{
			if (mInfluenceMapSingleton == null)
				mInfluenceMapSingleton = new InfluenceMapSingleton ();
			return mInfluenceMapSingleton;
		}

		public override void Init ()
		{
			base.Init ();

			this.AddComponent<InfluenceMap> (InfluenceMap.getInstance());
		}

	}

	public class InfluenceMap:UComponent
	{

		public float DefaultY;

		public Vector2 center;
		public float width;
		public float height;
		public int wtileCount;
		public int htileCount;
		public float[][][] IMData;
		public int allCount = 0;
		public Dictionary<string,int> mDictionary = new Dictionary<string, int>();
		private static InfluenceMap mInfluenceMapsingleton;
		public Dictionary<int,bool> mDicStatic = new Dictionary<int, bool>();
		public Dictionary<int,float> mAttenuation = new Dictionary<int, float> ();
		public bool Debugging;
		public Vector2[] NineGrid = new Vector2[]{new Vector2(0,1),new Vector2(0,-1),
			new Vector2(1,1),new Vector2(1,0),new Vector2(1,-1),
			new Vector2(-1,1),new Vector2(-1,0),new Vector2(-1,-1)};

		public GameObject mPlane;

		public GameObject[][] mDebugGameObject;

		public float timer = 0.0f;

		public Vector2 deltaTileSize
		{
			get
			{
				return new Vector2 (width/wtileCount,height/htileCount);
			}
		}

		public static InfluenceMap getInstance()
		{
			if (mInfluenceMapsingleton == null)
				mInfluenceMapsingleton = new InfluenceMap ();
			return mInfluenceMapsingleton;
		}

		// get data by tile data and layer data
		public float getData(Vector2 v,string pWhere)
		{
			int index = mDictionary [pWhere];
			return IMData [index] [(int)v.x] [(int)v.y];
		}

		// for debug 
		public void Show(int index)
		{
			Vector2 pfir = center - new Vector2 (width/2,height/2);
			for (int i = 1; i < wtileCount; i++) 
			{
				Vector3 start = new Vector3 (pfir.x + i * deltaTileSize.x, DefaultY + 0.1f, pfir.y);
				Vector3 end = new Vector3 (pfir.x+i*deltaTileSize.x,DefaultY+0.1f,pfir.y+height);
				Debug.DrawLine (start,end,Color.red);
			}

			for (int i = 1; i < htileCount; i++) 
			{
				Vector3 start = new Vector3 (pfir.x, DefaultY + 0.1f, pfir.y+i*deltaTileSize.y);
				Vector3 end = new Vector3 (pfir.x+width,DefaultY+0.1f,pfir.y+i*deltaTileSize.y);
				Debug.DrawLine (start,end,Color.red);
			}

			for (int i = 0; i < wtileCount; i++) 
			{
				for (int j = 0; j < htileCount; j++)
				{
					if (IMData [index] [i] [j]>0.0f) 
					{
						mDebugGameObject [i] [j].SetActive (true);
						float f = IMData [index] [i] [j] / 1600.0f;
						mDebugGameObject [i] [j].GetComponent<MeshRenderer> ().material.color = Color.yellow * f;
					}
					else
					{
						mDebugGameObject [i] [j].SetActive (false);
					}
				}
			}
		}

		public override void Init ()
		{
			base.Init ();
			IMData = new float[11][][];
			for (int i = 0; i < 11; i++) 
			{
				IMData[i] = new float[wtileCount][];
				for (int j = 0; j < wtileCount; j++)
				{
					IMData[i][j] = new float[htileCount];
					for (int k = 0; k < htileCount; k++) 
					{
						IMData [i] [j] [k] = 0.0f;
					}
				}
			}
			mDebugGameObject = new GameObject[wtileCount][];
			Vector3 pfir = new Vector3 (center.x-width/2,DefaultY,center.y-height/2);
			Vector3 prefabScale = mPlane.transform.localScale;
			for (int i = 0; i < mDebugGameObject.Length; i++)
			{
				mDebugGameObject[i] = new GameObject[htileCount];
				for (int j = 0; j < mDebugGameObject [i].Length; j++)
				{
					Vector3 myPoint = new Vector3(pfir.x+(i+0.5f)*deltaTileSize.x,DefaultY+0.2f,pfir.z+(j+0.5f)*deltaTileSize.y);
					mDebugGameObject [i][j] = GameObject.Instantiate (mPlane,myPoint,Quaternion.identity) as GameObject;
					mDebugGameObject [i] [j].transform.localScale = new Vector3 (1.0f/wtileCount*prefabScale.x,1.0f,1.0f/htileCount*prefabScale.z);
					mDebugGameObject [i] [j].SetActive (false);
				}
			}
			AddConverage (0.0f,"Collision",false,0.0f);
			AddConverage (0.0f, "friend", false,0.0f);
			AddConverage (0.0f,"Risk",false,0.0f);
		}

		// add layer 
		public void AddConverage(float defaultValue,string name,bool isStatic,float Attenuation)
		{
			if (allCount > 10)
				return;
			mDictionary.Add (name,allCount);
			mDicStatic.Add (allCount,isStatic);
			mAttenuation.Add (allCount,Attenuation);
			allCount++;
		}

		// get tile by a object position
		public Vector2 getTilefromPosition(Vector2 pcenter)
		{
			Vector2 pfir = center - new Vector2 (width/2,height/2)+new Vector2(deltaTileSize.x/2,deltaTileSize.y/2);
			Vector2 delta = new Vector2 (width/wtileCount,height/htileCount);
			Vector2 size = new Vector2 ((int)((pcenter-pfir).x/delta.x+0.5f),(int)((pcenter-pfir).y/delta.y+0.5f));
			return size;
		}

		//  get a max value in neighbor
		public float getMax(Vector2 pcenter,float priaus,int index)
		{
			float Max = -10.0f;

			int R = (int)Mathf.Max (priaus/deltaTileSize.x,priaus/deltaTileSize.y);

			for (int i = -R; i <= R; i++)
			{
				for (int j = -R; j <= R; j++)
				{
					int ti = Mathf.Clamp ((int)pcenter.x+i,0,wtileCount-1);
					int tj = Mathf.Clamp ((int)pcenter.y+j,0,htileCount-1);
					if (IMData [index] [ti] [tj] > Max)
						Max = IMData [index] [ti] [tj];
				}
			}
			return Max;
		}

		// A* ALG.
		/*
		 * @ param src: start point
		 * @ param des: end point
		 * @param  pAStarComputer: find path method
		 * @return : a list point as Stack
		 * 
		*/
		public Stack<Vector3> AStarFindPath(Vector3 src,Vector3 des,AStarComputer pAStarComputer)
		{
			PriorityQueue<AStarNodeItem> openList = new PriorityQueue<AStarNodeItem> (10,new AStarNodeItemCamparer());
			List<AStarNodeItem> closedList = new List<AStarNodeItem> ();
			Stack<Vector3> pathLast = new Stack<Vector3> ();
			Vector2 start = getTilefromPosition (new Vector2(src.x,src.z));
			Vector2 end = getTilefromPosition (new Vector2(des.x,des.z));

			int count = 0;

			if (!(start.x == end.x && start.y == end.y))
			{
				AStarNodeItem AStarforStart = new AStarNodeItem (src,(int)start.x,(int)start.y);
				AStarforStart.gCost = 0;
				AStarforStart.hCost = pAStarComputer (src,des);
				openList.Push (AStarforStart);
			}

	
			while(openList.Count!=0)
			{

				if (count >= 1000)
				{
					break;
				}
				AStarNodeItem tAStarNodeItem = openList.Pop ();
				
			
				
				if (tAStarNodeItem.x == end.x && tAStarNodeItem.y == end.y) 
				{
			
					while(tAStarNodeItem.parent!=null)
					{
						int index = mDictionary ["Collision"];
				
						pathLast.Push (tAStarNodeItem.pos);
						tAStarNodeItem = tAStarNodeItem.parent;
					}
					return pathLast;
				}

			
				closedList.Add (tAStarNodeItem);

				for (int i = 0; i < NineGrid.Length; i++) 
				{
					if (tAStarNodeItem.x + NineGrid [i].x < 0 || tAStarNodeItem.x + NineGrid [i].x >= wtileCount || tAStarNodeItem.y + NineGrid [i].y >= htileCount || tAStarNodeItem.y + NineGrid [i].y < 0)
						continue;
					Vector2 tv = new Vector2 (tAStarNodeItem.x+NineGrid[i].x,tAStarNodeItem.y+NineGrid[i].y);
					Vector3 tp = GetPositionByTile (tv);
					AStarNodeItem ttAStarNodeItem = new AStarNodeItem (tp,(int)tv.x,(int)tv.y);
					if (isLastContain (closedList, ttAStarNodeItem) || isWall (new Vector2 (tAStarNodeItem.x + NineGrid [i].x, tAStarNodeItem.y + NineGrid [i].y)))
						continue;
					
					ttAStarNodeItem.parent = tAStarNodeItem;
					ttAStarNodeItem.gCost = (int)(NineGrid [i].magnitude*10);
					ttAStarNodeItem.hCost = pAStarComputer (tp,des);
					openList.Push (ttAStarNodeItem);
				}
				count++;
			}
			//Debug.Log ("NEW");
			return new Stack<Vector3>();

		}

		// another param
		public Stack<Vector3> AStarFindPath(Vector3 src,Vector3 des,AStarComputerformula pAStarComputerformula)
		{
			return AStarFindPath (src, des, pAStarComputerformula.EquationComputer);
		}




		public bool isLastContain(List<AStarNodeItem> pList,AStarNodeItem pNode)
		{
			foreach (var v in pList)
			{
				if (v.x == pNode.x && v.y == pNode.y)
					return true;
			}
			return false;
		}

		// is this tile a wall or not 
		public bool isWall(Vector2 v)
		{
			int index = mDictionary ["Collision"];



			if (v.x < 0 || v.x > wtileCount - 1 || v.y < 0 || v.y > htileCount - 1)
				return true;


			int ti = Mathf.Clamp ((int)v.x,0,wtileCount-1);

			int tj = Mathf.Clamp ((int)v.y,0,htileCount-1);

			if (IMData [index] [ti] [tj] > 100)
			{
	
				return true;
			}
			else
			{
				return false;
			}
		}

		// get a position by tile 

		public Vector3 GetPositionByTile(Vector2 p)
		{
			Vector2 leftdown = new Vector2 (center.x-width/2,center.y-height/2);
			return new Vector3 (leftdown.x+(p.x+0.4f)*width/wtileCount,DefaultY,leftdown.y+(p.y+0.4f)*height/htileCount);
		}

	};

	/*
	 * 
	 * IM data trigger 
	 * if you want to storage data in IM,you should add this to a GameObject
	 * 
	*/

	public class InfluenceMapTrigger:UComponent
	{
		public IMComputer mIMComputer;
		public float maxInfluence;
		public string mWhere;
		public GameObject mGameObject;

		public override void Init ()
		{
			base.Init ();
			string tWhere = mWhere;
			//Debug.Log (tWhere);
			InfluenceMap tInfluenceMap = InfluenceMap.getInstance ();
			int index = tInfluenceMap.mDictionary [tWhere];
			bool isStatic = tInfluenceMap.mDicStatic [index];
			if (isStatic)
			{
				Vector3 position = mGameObject.GetComponent<MeshRenderer> ().bounds.center;
				Vector3 size = mGameObject.GetComponent<MeshRenderer> ().bounds.size;
			
				Vector2 LeftDown = new Vector2 (position.x-size.x/2,position.z-size.z/2);
				Vector2 tileLeftDown = InfluenceMap.getInstance ().getTilefromPosition (LeftDown);
				Vector2 RightUp = new Vector2 (position.x+size.x/2,position.z+size.z/2);
				Vector2 tileRightUp = InfluenceMap.getInstance ().getTilefromPosition (RightUp);
			
				float IMdata = mIMComputer (mUEntity);


				for (int i = (int)tileLeftDown.x; i <= tileRightUp.x; i++) 
				{
					for (int j = (int)tileLeftDown.y; j <= tileRightUp.y; j++)
					{
						Vector2 tTile = new Vector2 (i,j);
						fillTile (tTile,index,IMdata);
					}
				}
				

			}
		}

		private void fillTile(Vector2 tile,int index,float IMdata)
		{
			for (int i = (int)(tile.x - maxInfluence / 2); i <= (int)(tile.x + maxInfluence / 2); i++) 
			{
				for(int j = (int)(tile.y - maxInfluence / 2); j <= (int)(tile.y + maxInfluence / 2); j++)
				{
					int ti = Mathf.Clamp (i,0,InfluenceMap.getInstance().wtileCount-1);
					int tj = Mathf.Clamp (j,0,InfluenceMap.getInstance().htileCount-1);
					Vector2 v;
					if (maxInfluence > 0.0005f)
						v = new Vector2 ((ti - tile.x) / maxInfluence, (tj - tile.y) / maxInfluence);
					else
						v = new Vector2 (0,0);
					InfluenceMap.getInstance ().IMData [index] [ti] [tj] += IMdata * Gauss (v);

				}
			}
		}

		private float Gauss(Vector2 v)
		{
			return Mathf.Exp(-(v.x*v.x/2.0f+v.y*v.y/2.0f));		
		}

	};



	public class InfluenceMapUpdateSystem:USystem
	{
		
		public override void Init ()
		{
			base.Init ();
			this.AddRequestComponent (typeof(InfluenceMapTrigger));
			power = 90;
			name = "InfluenceMapUpdateSystem";
		}	

		public override void Update (UEntity uEntity)
		{
			base.Update (uEntity);


			if (uEntity.GetComponent<LODComponent> ()!=null) 
			{
				int lod = uEntity.GetComponent<LODComponent> ().mLOD;
				if (lod > LODComponent.maxLOD / 2)
					return;
			}



			string tWhere = uEntity.GetComponent<InfluenceMapTrigger> ().mWhere;
			InfluenceMap tInfluenceMap = InfluenceMap.getInstance ();
			int index = tInfluenceMap.mDictionary [tWhere];
			bool isStatic = tInfluenceMap.mDicStatic [index];
			if (!isStatic)
			{
				Vector3 position;
				Vector3 size;
				if (uEntity.GetComponent<InfluenceMapTrigger> ().mGameObject.GetComponent<MeshRenderer> ())
				{
					position = uEntity.GetComponent<InfluenceMapTrigger>().mGameObject.GetComponent<MeshRenderer> ().bounds.center;
					size =  uEntity.GetComponent<InfluenceMapTrigger>().mGameObject.GetComponent<MeshRenderer> ().bounds.size;
				}
				else
				{
					position = uEntity.GetComponent<InfluenceMapTrigger>().mGameObject.GetComponentInChildren<MeshRenderer> ().bounds.center;
					size =  uEntity.GetComponent<InfluenceMapTrigger>().mGameObject.GetComponentInChildren<MeshRenderer> ().bounds.size;
				}


				Vector2 LeftDown = new Vector2 (position.x-size.x/2,position.z-size.z/2);
				Vector2 tileLeftDown = InfluenceMap.getInstance ().getTilefromPosition (LeftDown);
				Vector2 RightUp = new Vector2 (position.x+size.x/2,position.z+size.z/2);
				Vector2 tileRightUp = InfluenceMap.getInstance ().getTilefromPosition (RightUp);
				float IMdata = uEntity.GetComponent<InfluenceMapTrigger> ().mIMComputer (uEntity);
				float maxInfluence = uEntity.GetComponent<InfluenceMapTrigger> ().maxInfluence;

			

				for (int i = (int)tileLeftDown.x; i <= tileRightUp.x; i++) 
				{
					for (int j = (int)tileLeftDown.y; j <= tileRightUp.y; j++)
					{
						Vector2 tTile = new Vector2 (i,j);
						fillTile (tTile,index,IMdata,maxInfluence);
					}
				}
					
			}

		}

		private void fillTile(Vector2 tile,int index,float IMdata,float maxInfluence)
		{
			for (int i = (int)(tile.x - maxInfluence / 2); i <= (int)(tile.x + maxInfluence / 2); i++) 
			{
				for(int j = (int)(tile.y - maxInfluence / 2); j <= (int)(tile.y + maxInfluence / 2); j++)
				{
					int ti = Mathf.Clamp (i,0,InfluenceMap.getInstance().wtileCount-1);
					int tj = Mathf.Clamp (j,0,InfluenceMap.getInstance().htileCount-1);
					Vector2 v;
					if (maxInfluence > 0.0005f)
						v = new Vector2 ((ti - tile.x) / maxInfluence, (tj - tile.y) / maxInfluence);
					else
						v = new Vector2 (0,0);
					InfluenceMap.getInstance ().IMData [index] [ti] [tj] += IMdata * Gauss (v);

				}
			}
		}


		private float Gauss(Vector2 v)
		{
			return Mathf.Exp(-(v.x*v.x/2.0f+v.y*v.y/2.0f));		
		}

	};

	public class InfluenceMapFlushSystem:USystem
	{
		public override void Init ()
		{
			base.Init ();
			this.AddRequestComponent (typeof(InfluenceMap));
			power = 89;
			name = "InfluenceMapFlushSystem";
		}

		public override void Update (UEntity uEntity)
		{
			base.Update (uEntity);


			if (uEntity.GetComponent<InfluenceMap> ().timer < 1.0f)
			{
				uEntity.GetComponent<InfluenceMap> ().timer += Time.deltaTime;
			}
			else
			{
				uEntity.GetComponent<InfluenceMap> ().timer = 0.0f;
				for (int i = 0; i < uEntity.GetComponent<InfluenceMap> ().allCount; i++)
				{
					float tAtten = uEntity.GetComponent<InfluenceMap> ().mAttenuation [i];
					if (!uEntity.GetComponent<InfluenceMap> ().mDicStatic [i]&&tAtten!=0.0f) 
					{
						for (int j = 0; j < uEntity.GetComponent<InfluenceMap> ().wtileCount; j++) 
						{
							for (int k = 0; k < uEntity.GetComponent<InfluenceMap> ().htileCount; k++) 
							{
								uEntity.GetComponent<InfluenceMap> ().IMData [i] [j] [k] *= tAtten;
								if (uEntity.GetComponent<InfluenceMap> ().IMData [i] [j] [k] < 1.0f)
									uEntity.GetComponent<InfluenceMap> ().IMData [i] [j] [k] = 0.0f;
							}
						}
					}
				}
			}

			for (int i = 0; i < uEntity.GetComponent<InfluenceMap> ().allCount; i++)
			{
				float tAtten = uEntity.GetComponent<InfluenceMap> ().mAttenuation [i];
				if (!uEntity.GetComponent<InfluenceMap> ().mDicStatic [i]&&tAtten==0.0f) 
				{
					for (int j = 0; j < uEntity.GetComponent<InfluenceMap> ().wtileCount; j++) 
					{
						for (int k = 0; k < uEntity.GetComponent<InfluenceMap> ().htileCount; k++) 
						{
							uEntity.GetComponent<InfluenceMap> ().IMData [i] [j] [k] = 0.0f;
						}
					}
				}
			}



		}

	};


	public class InfluenceMapShowSystem:USystem
	{
		
		public override void Init ()
		{
			base.Init ();
		}

		public override void Update (UEntity uEntity)
		{
			base.Update (uEntity);
		}


	};

	public class PriorityQueue<T> where T:new()
	{
		IComparer<T> comparer;
		T[] heap = new T[10];

		public int Count { get; private set; } 

		public PriorityQueue() : this(null) { }
		public PriorityQueue(int capacity) : this(capacity, null) { }
		public PriorityQueue(IComparer<T> comparer) : this(16, comparer) { }

		public PriorityQueue(int capacity, IComparer<T> comparer)
		{
			this.comparer = (comparer == null) ? Comparer<T>.Default : comparer;
			this.heap = new T[capacity];
		}



		public void Push(T v)
		{
			
			if (Count >= heap.Length) 
				Array.Resize(ref heap, Count * 2);
			

			heap[Count] = v;

			SiftUp(Count++);
		}

		public void Clear()
		{
			Count = 0;
		}

		public T Pop()
		{
			var v = Top();
			heap[0] = heap[--Count];
			if (Count > 0) 
				SiftDown(0);
			return v;
		}

		public T Top()
		{
			if (Count > 0)
				return heap [0];
			return new T();
		}

		void SiftUp(int n)
		{
			var v = heap[n];
			for (var n2 = n / 2; n > 0 && comparer.Compare(v, heap[n2]) > 0; n = n2, n2 /= 2) heap[n] = heap[n2];
			heap[n] = v;
		}

		void SiftDown(int n)
		{
			var v = heap[n];
			for (var n2 = n * 2; n2 < Count; n = n2, n2 *= 2)
			{
				if (n2 + 1 < Count && comparer.Compare(heap[n2 + 1], heap[n2]) > 0) n2++;
				if (comparer.Compare(v, heap[n2]) >= 0) break;
				heap[n] = heap[n2];
			}
			heap[n] = v;
		}

	}

}

