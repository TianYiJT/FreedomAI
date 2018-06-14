using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FreedomAI;
using UnityECS;
using System;
using System.Reflection;
using System.Threading;

namespace FreedomAI
{

	public class MyRandom
	{
		public MyRandom()
		{
			srd = new System.Random (System.Guid.NewGuid().GetHashCode());
		}

		public int NextInt(int imin,int imax)
		{
			return srd.Next (imin,imax);
		}

		public Double NextDouble(Double dmin,Double dmax)
		{
			return dmin + (dmax - dmin) * srd.NextDouble ();
		}

		private System.Random srd;
	}

	/*
	 * 
	 * the base class of AI Gene.
	 * you can defined:every Gene and their max and min.
	 * and you also should override Decode-transfer bitbunch to Gene value
	 * Encode-transfer Gene value to bitbunch
	 * Evaluate-compute this gene power in the GA.
	 * Randomdata-init the Gene data.
	 * 
	 * 
	*/
	public class GAObject
	{
		// default position 
		public Vector3 mPosition;

		public float mfitness
		{
			get
			{
				return Evaluate ();
			}
		}
		// Gene 0-1 List 
		public bool[] mbitbunch
		{
			set
			{
				m_bitbunch = new bool[value.Length];
				for (int i = 0; i < m_bitbunch.Length; i++)
					m_bitbunch [i] = value [i];
			}

			get
			{
				return m_bitbunch;
			}
		}

		private bool[] m_bitbunch;

		public List<int> mBreakPoint = new List<int>();

		public virtual void Encode ()
		{
			
		}

		public virtual void Decode ()
		{
			
		}

		public virtual void RandomData()
		{
			
		}

		public virtual float Evaluate()
		{
			return 1.0f;
		}

		public virtual void InitBreakPoint()
		{
			
		}


		public void selfTest()
		{
			string s = "";
			for (int i = 0; i < mbitbunch.Length; i++)
			{
				if(mbitbunch[i])
					s+="1";
				else
					s+="0";
			}
			Debug.Log (s);
		}

	};

	// select the good Genes from a Gene set function 
	public delegate GAObject[] GASelector(GAObject[] pObjects,float pDyingRate,Type pGAObjectType);

	// get next Gene set from a Gene set function
	public delegate GAObject[] GABreedor(GAObject[] pObjects,float pIncreaseRate,Type pGAObjectType);

	// get a Gene be mutatored some from a Gene set function
	public delegate GAObject[] GAMutator(GAObject[] pObjects,float pMutatorRate,Type pGAObjectType);

	// all GA Population set 
	public class GAPopulationManager
	{
		private static GAPopulationManager mGAPoputionManager;

		public bool isSingleThread;

		public static GAPopulationManager getInstance()
		{
			if (mGAPoputionManager == null)
				mGAPoputionManager = new GAPopulationManager ();
			return mGAPoputionManager;
		}

		// add a Population 
		public void Add(GAPopulation pGAPopulation)
		{
			mGAPopulationSet [mCount] = pGAPopulation;
			mCount++;
		}

		// update all Population 
		public void Update()
		{
			for (int i = 0; i < mCount; i++) 
			{
				mGAPopulationSet [i].mBreedTimer += Time.deltaTime;
				if (mGAPopulationSet [i].mBreedTimer >= mGAPopulationSet [i].mBreedCycle)
				{
					mGAPopulationSet [i].mBreedTimer = 0.0f;
					mGAPopulationSet [i].Generate ();
				}
			}
		}

		public void Init()
		{
			mGAPopulationSet = new GAPopulation[100];
		}

		// Start all Population 
		public void Start()
		{
			for (int i = 0; i < mCount; i++)
			{
				mGAPopulationSet [i].Init ();
			}
		}
		/*
		private void ThreadDo(object pGAPopulation)
		{
		//	Debug.Log ("sadfas");
			Debug.Log ("sadfasf2");
			GAPopulation tGAPopulation = pGAPopulation as GAPopulation;
			Debug.Log ("sadfasf1");
			tGAPopulation.Init ();
			Debug.Log ("sadfasf");
			while (true) 
			{
				Thread.Sleep (50);
				tGAPopulation.mBreedTimer += 0.05f;
				if (tGAPopulation.mBreedTimer >= tGAPopulation.mBreedCycle) 
				{
					tGAPopulation.Generate ();
					tGAPopulation.mBreedTimer = 0.0f;
				}
			}
		}
		*/
		private GAPopulation[] mGAPopulationSet;
		//private Thread[] mGAPopulationThreadSet;

		public int mCount = 0;
	}


	public class GAPopulation
	{
		// the type of this Popualation Gene class 
		public Type mObjectType;

		// the tag of this AI
		public string mRTType;

		// interface using SimpleAI
		public SimpleAIRunner mRTObjectRunner;

		// interface using SimpleAI
		public SimpleAIStateJudger mRTObjectJudger;

		// interface using SimpleAI
		public SimpleAIDestroyer mRTObjectDestroyer;

		// interface using SimpleAI
		public SimpleAIRePairer mSimpleAIRePairer;

		// this AI Prefab 
		public GameObject mPrefab;

		public GameObject mPlayer;

		// this Population max Count
		public int mMaxCount;

		// the breed time
		public float mBreedCycle;

		// time counter
		public float mBreedTimer = 0.0f;

		// mutating rate
		public float mMutatorRate;

		// increase rate
		public float mIncreaseRate;

		// dying rate 
		public float mDyingRate;

		// the select function
		public GASelector mSelector;

		// the breed function
		public GABreedor mBreedor;

		// the mutator function
		public GAMutator mMutator;

		// gene object
		private GAObject[] mGAObjectSet;
		// AI object
		private SimpleAI[] mRTObjectSet;

		public float lifeRate
		{
			get
			{
				return (float)mGAObjectSet.Length / (float)mMaxCount;
			}
		}

		public int mTempCount
		{
			get
			{
				return mGAObjectSet.Length;
			}
		}

		public static Dictionary<string,GAPopulation> allDic;

		public static void GlobalInit()
		{
			allDic = new Dictionary<string, GAPopulation> ();
		}

		public GAPopulation()
		{

		}
		// add myself in GAPopulation class 
		public void AddMySelf()
		{
			GAPopulationManager.getInstance ().Add (this);
		}

		public void Init()
		{

			mGAObjectSet = new GAObject[mMaxCount];
			allDic.Add (mRTType,this);
			for (int i = 0; i < mGAObjectSet.Length; i++)
			{
				mGAObjectSet [i] = (GAObject)mObjectType.Assembly.CreateInstance (mObjectType.FullName);		
				mGAObjectSet [i].RandomData ();
				mGAObjectSet [i].Encode ();
				mGAObjectSet [i].InitBreakPoint ();
			}

			RePairRTObject ();
		}

		public void Generate()
		{
			//select
			mGAObjectSet = mSelector (mGAObjectSet,mDyingRate,mObjectType);
			// breed
			mGAObjectSet = mBreedor (mGAObjectSet,mIncreaseRate,mObjectType);
			// mutator
			mGAObjectSet = mMutator (mGAObjectSet,mMutatorRate,mObjectType);
		
			for (int i = 0; i < mGAObjectSet.Length; i++) 
			{
				// get temp gene value
				mGAObjectSet [i].InitBreakPoint ();
				mGAObjectSet [i].Decode ();
			}
			// run this gene
			RePairRTObject ();
		}

		public void RePairRTObject()
		{
			if (mRTObjectSet != null)
			{
				for (int i = 0; i < mRTObjectSet.Length; i++)
				{
					GameObject.Destroy (mRTObjectSet [i].mAIRT);
					SimpleAISetSingleton.getInstance ().GetComponent<SimpleAISet> ().Delete (mRTObjectSet [i]);
				}
			}

			mRTObjectSet = new SimpleAI[mGAObjectSet.Length];

			for (int i = 0; i < mRTObjectSet.Length; i++)
			{
				mRTObjectSet [i] = new SimpleAI ();
				mRTObjectSet [i].Init (mRTObjectRunner,mRTObjectJudger,mPrefab,mPlayer,mGAObjectSet[i].mPosition);
				mRTObjectSet [i].mSimpleAIDestroyer = mRTObjectDestroyer;
				mRTObjectSet [i].mSimpleAIRepairer = mSimpleAIRePairer;
				mRTObjectSet [i].mType = mRTType;
				mRTObjectSet [i].mCharacter = mGAObjectSet [i];
				mRTObjectSet [i].mSimpleAIRepairer.DoRePair(mRTObjectSet[i]);
				mRTObjectSet [i].mName = mRTType + "index" + i;
			}

		}

		// destroy a AI should be delete in GAPopulation 
		public void Delete(SimpleAI pSimpleAI)
		{
			GAObject[] tGAObjects = new GAObject[mGAObjectSet.Length-1];
			SimpleAI[] tSimpleAIs = new SimpleAI[mRTObjectSet.Length-1];
			int index = 0;
			for (int i = 0; i < mGAObjectSet.Length; i++) 
			{
				if (mRTObjectSet [i] == pSimpleAI)
					continue;
				tGAObjects [index] = mGAObjectSet [i];
				tSimpleAIs [index] = mRTObjectSet [i];
				index++;
			}
			mGAObjectSet = tGAObjects;
			mRTObjectSet = tSimpleAIs;
//			Debug.Log (mRTObjectSet.Length);
		}
		// add a object new 
		public void Add(SimpleAI pSimpleAI)
		{
			GAObject[] tGAObjects = new GAObject[mGAObjectSet.Length+1];
			SimpleAI[] tSimpleAIs = new SimpleAI[mRTObjectSet.Length+1];
			for (int i = 0; i < mGAObjectSet.Length; i++) 
			{
				tGAObjects [i] = mGAObjectSet [i];
				tSimpleAIs [i] = mRTObjectSet [i];
			}
			tGAObjects [tGAObjects.Length - 1] = pSimpleAI.mCharacter;
			tSimpleAIs [tGAObjects.Length - 1] = pSimpleAI;
			mGAObjectSet = tGAObjects;
			mRTObjectSet = tSimpleAIs;
		}

	};

	public class ZoologyComponent:UComponent
	{
		public Vector3 mPosition;
		public Vector2 mSize;
		public bool mState;
	}
	// no using 
	public class ZoologySystem:USystem
	{
		public override void Init ()
		{
			base.Init ();
			this.AddRequestComponent (typeof(ZoologyComponent));
		}

		public override void Update (UEntity uEntity)
		{
			base.Update (uEntity);
			Vector3 pos = uEntity.GetComponent<ZoologyComponent> ().mPosition;
			float maxRiaus = Mathf.Max (uEntity.GetComponent<ZoologyComponent>().mSize.x,uEntity.GetComponent<ZoologyComponent>().mSize.y);
			SimpleAI[] tSimpleAIs=SimpleAISetSingleton.getInstance ().GetComponent<SimpleAISet> ().FindAIsWithRiaus (pos,maxRiaus,1<<LayerMask.NameToLayer("Default"),"AI");
			List<GAPopulation> tList = new List<GAPopulation> ();
			for (int i = 0; i < tSimpleAIs.Length; i++) 
			{
				if(!tList.Contains(GAPopulation.allDic[tSimpleAIs[i].mType]))
				{
					tList.Add (GAPopulation.allDic[tSimpleAIs[i].mType]);	
				}	
			}
			float avgRate = 0.0f;
			foreach (var v in tList)
			{
				avgRate += v.lifeRate;
			}
			avgRate /= tList.Count;
			if (avgRate < 0.2f)
				uEntity.GetComponent<ZoologyComponent> ().mState = false;
			else
				uEntity.GetComponent<ZoologyComponent> ().mState = true;
		}

	}
	// default encode and decode function
	public class FreedomAIEncoderAndDecoder
	{
		//for debug-transfer bitbunch to string
		public static string bitbunch2String(bool[] bitbunch)
		{
			string s = "";
			for (int i = 0; i < bitbunch.Length; i++)
			{
				if(bitbunch[i])
					s+="1";
				else
					s+="0";
			}
			return s;
		}
		// joint many bitbunch to a big bitbunch
		public static bool[] JointBitBunch(params bool[][] allBitbunchs)
		{
			int allLength = 0;
			for (int i = 0; i < allBitbunchs.Length; i++)
				if(allBitbunchs[i]!=null)
					allLength += allBitbunchs [i].Length;
			bool[] result = new bool[allLength];
			int index = 0;
			for (int i = 0; i < allBitbunchs.Length; i++) 
			{
				if (allBitbunchs [i] != null) 
				{
					for (int j = 0; j < allBitbunchs [i].Length; j++) 
					{
						result [index] = allBitbunchs [i] [j];
						index++;
					}
				}
			}
			return result;
		}
		// break a big bitbunch to many bitbunch
		public static bool[][] BreakBitBunch(bool[] allBitbunchs,List<int> breakPoint)
		{
			bool[][] result = new bool[breakPoint.Count][];
			int tempIndex = 0;
			int tempLoc = 0;
			//Debug.Log (bitbunch2String(allBitbunchs));
			foreach (var i in breakPoint) 
			{
				bool[] first = new bool[i];
				for (int j = 0; j < i; j++) 
				{
					first [j] = allBitbunchs [tempIndex + j];
				}

				tempIndex += i;
				result [tempLoc] = new bool[i];
				for (int j = 0; j < i; j++)
					result [tempLoc] [j] = first [j];
				tempLoc++;
			}
			return result;
		}

		// transfer a float value to a bitbunch 
		public static bool[] float2BitBunch(float f,float fmin,float fmax,int bitCount)
		{
			bool[] tempBit = new bool[bitCount];
			if (f < fmin)
				f = fmin;
			else if (f > fmax)
				f = fmax;

			int allCount = (int)Mathf.Pow (2,bitCount);
			float rate = (f - fmin) / (fmax - fmin);
			int tempCount = (int)((float)allCount * (float)rate);
			if (tempCount >= allCount)
				tempCount=allCount-1;
			string s = Convert.ToString (tempCount,2);
			int index = bitCount - 1;

			for (int i = s.Length-1; i >=0; i--)
			{

				if (s [i] == '1')
					tempBit [index] = true;
				else
					tempBit [index] = false;
				index--;
			}
			return tempBit;
		}

		// transfer a bitbunch to a float value 
		public static float BitBunch2float(bool[] bitbunch,float fmin,float fmax)
		{
			int Length = bitbunch.Length;
			int sum = 0;
			for (int i = 0; i < bitbunch.Length; i++)
			{
				if (bitbunch [i])
					sum += (int)Mathf.Pow (2,Length-i-1);
			}
			int max = (int)Mathf.Pow (2,Length);
			return (fmax - fmin) * ((float)sum / (float)max) + fmin;
		}

		// transfer a int value to a bitbunch
		public static bool[] int2BitBunch(int i1,int imin,int imax,int bitcount)
		{
			bool[] tempBit = new bool[bitcount];
			if (i1 < imin)
				i1 = imin;
			else if (i1 > imax)
				i1 = imax;
			
			int itemp = i1 - imin;
			string s = Convert.ToString (itemp,2);
			int index = bitcount - 1;
			for (int i = s.Length-1; i >=0; i--)
			{

				if (s [i] == '1')
					tempBit [index] = true;
				else
					tempBit [index] = false;
				index--;
			}
			return tempBit;
		}

		// transfer a bool to bitbunch 
		public static bool[] bool2BitBunch(bool b)
		{
			bool[] bresult = new bool[1];
			bresult[0] = b;
			return bresult;
		}

		public static bool BitBunch2bool (bool[] b)
		{
			return b [0];
		}

		// transfer a bitbunch to a int value
		public static int BitBunch2int(bool[] b,int imin,int imax)
		{
			int Length = b.Length;
			int sum = 0;
			for (int i = 0; i < b.Length; i++)
			{
				if (b [i])
					sum += (int)Mathf.Pow (2,Length-i-1);
			}
			return imin + sum;
		}

	}
	// default GA function
	public class DefaultGAFunc
	{
		// select function by Roulette
		public static GAObject[] RouletteSelector(GAObject[] pGAObject,float pDyingRate,Type pGAObjectType)
		{

			int nowLength = (int)(pGAObject.Length * pDyingRate);
			if (nowLength == pGAObject.Length&&nowLength!=0)
			{
				nowLength--;
			}
			GAObject[] nowObject = new GAObject[nowLength];
			int index = 0;
			int RSIndex = UnityEngine.Random.Range(0,pGAObject.Length);
			float maxfitness = -1.0f;
			float minfitness = 999.0f;
			for (int i = 0; i < pGAObject.Length; i++)
			{
				if (pGAObject [i].mfitness > maxfitness)
				{
					maxfitness = pGAObject [i].mfitness;
				}
				if (pGAObject [i].mfitness < minfitness)
				{
					minfitness = pGAObject [i].mfitness;
				}
			}
			List<int> succeedIndex = new List<int> ();
			while (index < nowLength)
			{

				if (succeedIndex.Contains (RSIndex))
				{
					RSIndex++;
					RSIndex = RSIndex % pGAObject.Length;
					continue;
				}
				float trate = (pGAObject [RSIndex].mfitness-minfitness) / (maxfitness-minfitness);

				if (UnityEngine.Random.Range (0.0f, 1.0f) < trate) 
				{
					nowObject [index++] = pGAObject [RSIndex];
					succeedIndex.Add (RSIndex);
					RSIndex++;
					RSIndex = RSIndex % pGAObject.Length;
				}
				else 
				{
					RSIndex++;
					RSIndex = RSIndex % pGAObject.Length;
				}
			}

			//Debug.Log (s);
			return nowObject;
		}
		// breed by single point insert
		public static GAObject[] SinglePointInsertBreedor(GAObject[] pGAObject,float pIncreseseRate,Type pGAObjectType)
		{
			int nowLength = (int)(pGAObject.Length * pIncreseseRate);
			if (nowLength % 2 == 1)
				nowLength++;
			GAObject[] nowObject = new GAObject[nowLength];
			for (int i = 0; i < nowLength; i++)
			{
				nowObject [i] = (GAObject)pGAObjectType.Assembly.CreateInstance (pGAObjectType.FullName);
			}
			int index = 0;
			while (index < nowLength)
			{
				GAObject mother = pGAObject[UnityEngine.Random.Range(0,pGAObject.Length)];
				GAObject father = pGAObject[UnityEngine.Random.Range(0,pGAObject.Length)];
				bool[] bitbunch1 = new bool[mother.mbitbunch.Length];
				bool[] bitbunch2 = new bool[father.mbitbunch.Length];
				for (int i = 0; i < bitbunch1.Length; i++) 
				{
					if (i < bitbunch1.Length / 2) 
					{
						bitbunch1 [i] = mother.mbitbunch [i];
						bitbunch2 [i] = father.mbitbunch [i];
					}
					else 
					{
						bitbunch1 [i] = father.mbitbunch [i];
						bitbunch2 [i] = mother.mbitbunch [i];
					}
				}
				nowObject [index].mbitbunch = bitbunch1;
				nowObject [index + 1].mbitbunch = bitbunch2;
				index = index + 2;
			}
			return nowObject;
		}

		public static GAObject[] SimpleMutator(GAObject[] pGAObject,float pMutatoRate,Type pGAObjectType)
		{
			for (int i = 0; i < pGAObject.Length; i++)
			{
				if (UnityEngine.Random.Range (0.0f, 1.0f) < Mathf.Pow(pMutatoRate,0.5f))
				{
					for (int j = 0; j < pGAObject[i].mbitbunch.Length; j++) 
					{
						if (UnityEngine.Random.Range (0.0f, 1.0f) < Mathf.Pow (pMutatoRate, 0.5f))
						{
							pGAObject [i].mbitbunch [j] = !pGAObject [i].mbitbunch [j];
						}
					}
				}
			}
			return pGAObject;
		}

	}




};
