using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using System.Reflection;
using System;


namespace UnityECS
{
	// data structure using in ECS
	public class BitBunch
	{
		public bool[] mBitArray = new bool[200];
		public int mMaxCount = 200;
		public int mCount = 0;

		public bool GetBit(int mx)
		{
			return mBitArray[mx];
		}

		public void SetCount(int count)
		{
			if (count > mMaxCount) 
			{
				mBitArray = new bool[count * 2];
				mCount = count;
				for (int i = 0; i < mCount; i++)
					mBitArray [i] = false;
			}
			else
			{
				mCount = count;
				for (int i = 0; i < mCount; i++)
					mBitArray [i] = false;
			}
		}
			
		public void AddBit(bool mb)
		{
			if (mCount < mMaxCount)
			{
				mBitArray [mCount] = mb;
				mCount++;
			}
			else
			{
				mMaxCount *= 2;
				bool[] tBitArray = new bool[mMaxCount];
				for (int i = 0; i < mCount; i++)
					tBitArray [i] = mBitArray [i];
				mBitArray = tBitArray;
				mBitArray [mCount] = mb;
				mCount++;
			}
		}

		public void SetBit(bool res,int mx)
		{
			mBitArray [mx] = res;
		}

		public static bool Equal(BitBunch mbfir,BitBunch mbsec)
		{
			if (mbfir.mCount != mbsec.mCount) 
			{
				return false;
			}
			for (int i = 0; i < mbfir.mCount; i++) 
			{
				if(mbfir.GetBit(i)!=mbsec.GetBit(i))
				{
					return false;
				}
			}
			return true;
		}

		public static BitBunch And(BitBunch mbfir,BitBunch mbsec)
		{
			BitBunch tBitBunch = new BitBunch ();
			for (int i = 0; i < mbfir.mCount; i++)
				tBitBunch.AddBit (mbfir.GetBit(i)&mbsec.GetBit(i));
			return tBitBunch;
		}

		public override string ToString ()
		{
			string s = "";
			for(int i=0;i<mCount;i++)
				if(mBitArray[i])
					s+="1";
				else
					s+="0";
			return s;
		}

	};

	public class UEntity
	{
		public ECSWorld mWorld;

		public uint mWorldID;

		public BitBunch mAllBitBunch = new BitBunch();
		public UComponent[] mUComponent = new UComponent[1024];
		int mMaxComponentCount = 1024;
		public int mComponentCount = 0;
		// get the Component in this Entity by type
		public T GetComponent<T>() where T:UComponent 
		{
			Type requestType = typeof(T);
			for (int i = 0; i < mComponentCount; i++)
			{
				Type tType = mUComponent [i].GetType ();
				if (tType == requestType)
				{
					return (T)mUComponent [i];
				}
			}
			return null;
		}
		// add this type Component to Entity
		public void AddComponent<T>(T mT)where T:UComponent
		{
			if (mComponentCount < mMaxComponentCount)
			{
				mUComponent [mComponentCount] = mT;
				mUComponent [mComponentCount].mUEntity = this;
				if (mWorld != null) 
				{
					uint tid = mWorld.GetComponentID (mT.GetType ());
					mAllBitBunch.SetBit (true, (int)tid);
				}
				mComponentCount++;
			}
			else
			{
				mMaxComponentCount *= 2;
				UComponent[] tComponent = new UComponent[mMaxComponentCount];
				for (int i = 0; i < mComponentCount; i++)
					tComponent [i] = mUComponent [i];
				mUComponent = tComponent;
				mUComponent [mComponentCount] = mT;
				mUComponent [mComponentCount].mUEntity = this;
				if (mWorld != null) 
				{
					uint tid = mWorld.GetComponentID (mT.GetType ());
					mAllBitBunch.SetBit (true, (int)tid);
				}
				mComponentCount++;
			}
		}
		// delete the Component in this Entity by type
		public bool DestroyComponent<T>()where T:UComponent
		{
			uint tid = mWorld.GetComponentID (typeof(T));
			mAllBitBunch.SetBit (false,(int)tid);

			for (int i = 0; i < mComponentCount; i++)
			{
				if (mUComponent [i].GetType () == typeof(T)) 
				{
					for (int j = i + 1; j < mComponentCount; j++) 
					{
						mUComponent [j - 1] = mUComponent [j];
					}
					mComponentCount--;
					return true;
				}
			}
			return false;
		}
		// release resource
		public void Release()
		{
			for (int i = 0; i < mComponentCount; i++) 
			{
				mUComponent [i].Release ();
			}
			mWorld.deleteEntity (this);
		}

		public void Clear()
		{
			mComponentCount = 0;
			mAllBitBunch.SetCount ((int)mWorld.mComponentCount);
		}

		public virtual void Init()
		{
			
		}

		public override string ToString ()
		{
			string s = "";
			for (int i = 0; i < mComponentCount; i++)
				s += mUComponent [i].GetType ().Name;
			return s;
		}

	};

	public class UComponent
	{
		public UEntity mUEntity;
		// the component is be using or not 
		public bool isEnable = true;
		public virtual void Release()
		{
			
		}

		public virtual void Init()
		{
			
		}

		public virtual UComponent Clone()
		{
			return null;
		}



	};

	public class USystem
	{
		public ECSWorld mWorld;
		// the key of indexing Entity in world
		public BitBunch mRequestBitBunch = new BitBunch();
		public List<UEntity> mListEntity = new List<UEntity> ();
		// the power of system using update sort 
		public uint power =0;
		public string name;

		// add a key compoennt
		public void AddRequestComponent(Type mt)
		{
			uint tid = mWorld.GetComponentID (mt);
			mRequestBitBunch.SetBit (true,(int)tid);
		} 

		public virtual void Update (UEntity uEntity)
		{
	
		}

		public virtual void Init()
		{
			this.name = this.GetType ().Name;
		}

		public static bool operator < (USystem uSystem1,USystem uSystem2)
		{
			if (uSystem1.power < uSystem2.power)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static bool operator > (USystem uSystem1,USystem uSystem2)
		{
			if (uSystem1.power > uSystem2.power)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

	};

	public class ECSWorld
	{
	    USystem[] mUSystem=new USystem[1024];
		public UEntity[] mUEntity=new UEntity[1024];
		public uint mSystemCount=0;
		public uint mEntityCount=0;
		public uint mMaxSystemCount = 1024;
		public uint mMaxEntityCount = 1024;
		Dictionary<Type,uint> mDictionary = new Dictionary<Type, uint>();
		public uint mComponentCount = 0;
		public static ECSWorld MainWorld;
		// register the system
		public void registerSystem(USystem uSystem)
		{
			if (mSystemCount < mMaxSystemCount)
			{
				mUSystem [mSystemCount] = uSystem;
				mUSystem [mSystemCount].mWorld = this;
				mSystemCount++;
			}
			else
			{
				mMaxSystemCount *= 2;
				USystem[] uSystems = new USystem[mMaxSystemCount];
				for (int i = 0; i < mSystemCount; i++)
					uSystems [i] = mUSystem [i];
				mUSystem = uSystems;
				mUSystem [mSystemCount] = uSystem;
				mUSystem [mSystemCount].mWorld = this;
				mSystemCount++;
			}
		}
		// register the entity
		public void registerEntity(UEntity uEntity)
		{
			if (mEntityCount < mMaxEntityCount)
			{
				mUEntity [mEntityCount] = uEntity;
				mUEntity [mEntityCount].mWorld = this;
				mUEntity [mEntityCount].mWorldID = mEntityCount;
				mEntityCount++;
			}
			else
			{
				mMaxEntityCount *= 2;
				UEntity[] uEntitys = new UEntity[mMaxEntityCount];
				for (int i = 0; i < mEntityCount; i++)
					uEntitys [i] = mUEntity [i];
				mUEntity = uEntitys;
				mUEntity [mEntityCount] = uEntity;
				mUEntity [mEntityCount].mWorld = this;
				mUEntity [mEntityCount].mWorldID = mEntityCount;
				mEntityCount++;
			}
		}
		// register the entity after world initing
		public void registerEntityAfterInit(UEntity uEntity)
		{
			this.registerEntity (uEntity);
			uEntity.mAllBitBunch.SetCount ((int)this.mComponentCount);
		}

		// delete the entity
		public void deleteEntity(UEntity uEntity)
		{
			for (int i = (int)uEntity.mWorldID; i < mEntityCount-1; i++) 
			{
				mUEntity [i] = mUEntity [i + 1];
				mUEntity [i].mWorldID = (uint)i;
			}
			mEntityCount--;
		}

		// register the component 
		public void registerComponent(Type t)
		{
			mDictionary.Add (t,mComponentCount);
			mComponentCount++;
			for (int i = 0; i < mSystemCount; i++)
				mUSystem [i].mRequestBitBunch.SetCount ((int)mComponentCount);
			for (int i = 0; i < mEntityCount; i++)
				mUEntity [i].mAllBitBunch.SetCount ((int)mComponentCount);
		}

		public uint GetComponentID(Type t)
		{
			return mDictionary [t];
		}

		// Find the Entity having the key bitbunch
		List<UEntity> FindAllEntity(BitBunch mBitBunch)
		{
			List<UEntity> mList = new List<UEntity> ();
			for (int i = 0; i < mEntityCount; i++)
			{
				if (BitBunch.Equal(BitBunch.And (mBitBunch, mUEntity [i].mAllBitBunch),mBitBunch))
				{
					mList.Add (mUEntity[i]);
				}
			}
			return mList;
		}

		// update all system

		public void Update()
		{	
			
			for (int i = 0; i < mSystemCount; i++)
			{

				if (mUSystem [i].mListEntity.Count != 0) 
				{
					for (int j = 0; j < mUSystem[i].mListEntity.Count; j++) 
					{
						mUSystem [i].Update (mUSystem[i].mListEntity[j]);
					}
					continue;
				}

				List<UEntity> tList = FindAllEntity (mUSystem[i].mRequestBitBunch);
				for (int j = 0; j < tList.Count; j++) 
				{
					mUSystem [i].Update (tList[j]);
				}
			}
	
		}

		public virtual void registerAllEntity()
		{
			
		}

		public virtual void registerAllSystem()
		{
			
		}

		public virtual void registerAllComponent()
		{
			
		}

		public void Init()
		{
			registerAllSystem ();
			registerAllEntity ();
			registerAllComponent ();

			for (int i = 0; i < mSystemCount; i++) 
			{
				mUSystem [i].Init ();
			}

			for (int i = 0; i < mSystemCount; i++) 
			{
				for (int j = i+1; j < mSystemCount; j++)
				{
					if (mUSystem [i].power > mUSystem [j].power)
					{
						USystem tSystem = mUSystem [i];
						mUSystem [i] = mUSystem [j];
						mUSystem [j] = tSystem;
					}
				}
			}

			//ECSSort.QuickSortRelax (mUSystem,0,(int)mSystemCount-1);

			for (int i = 0; i < mEntityCount; i++)
			{
				mUEntity [i].Init ();
			}

			MainWorld = this;
			MainWorld.ToString ();
		}

		public class ECSSort
		{
			
			public static void QuickSortRelax<T>(IList<T> data)where T:USystem
			{
				 QuickSortRelax(data, 0, data.Count - 1);
			}

			public static void QuickSortRelax<T>(IList<T> data, int low, int high)where T:USystem
			{
				 if (low >= high) 
					return;
				 T temp = data[(low + high) / 2];
				 int i = low - 1, j = high + 1;
				 while (true)
				 {
					while (data[++i] < temp) ;
					while (data[--j] > temp) ;
					if (i >= j) 
						break;
					Swap(data, i, j);
				 }
				QuickSortRelax(data, j + 1, high);
				QuickSortRelax(data, low, i - 1);
			}

			public static void Swap<T>(IList<T> data,int a,int b)
			{
				T temp = data [a];
				data [a] = data [b];
				data [b] = temp;
			}

		}
	};
};