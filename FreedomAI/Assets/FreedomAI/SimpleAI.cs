using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityECS;
using FreedomAI;


namespace FreedomAI
{
	
	public interface SimpleAIStateJudger
	{
		 string DoJudge(SimpleAI sAI);
	};


	public interface SimpleAIRunner
	{
		 void DoRun (SimpleAI sAI,string statecode);
	};

	public interface SimpleAIDestroyer
	{
		void DoDestroy(SimpleAI sAI);	
	};

	public interface SimpleAIRePairer
	{
		void DoRePair (SimpleAI sAI);
	}

	public class SimpleAI:UEntity
	{
		public  SimpleAIRunner mSimpleAIRunner;
		public  SimpleAIStateJudger mSimpleAIStateJudger;
		public SimpleAIDestroyer mSimpleAIDestroyer;
		public SimpleAIRePairer mSimpleAIRepairer;
		public  GameObject mAIRT;
		public  GameObject mAITemplate;
		public  GameObject mPlayer;
		public  Vector3 GeneratePos;
		public GAObject mCharacter;
		public string mType;

		public string mName;

		public void Init(SimpleAIRunner pSimpleAIRunner,SimpleAIStateJudger pSimpleAIStateJudger,GameObject pAITemplete,GameObject pPlayer,Vector3 pPos)
		{
			ECSWorld.MainWorld.registerEntityAfterInit (this);
			mSimpleAIRunner = pSimpleAIRunner;
			mSimpleAIStateJudger = pSimpleAIStateJudger;
			mAITemplate = pAITemplete;
			mPlayer = pPlayer;
			GeneratePos = pPos;
			mAIRT=GameObject.Instantiate (mAITemplate,GeneratePos,Quaternion.identity) as GameObject;
			this.AddComponent<LODComponent> (new LODComponent());
			SimpleAISetSingleton.getInstance ().GetComponent<SimpleAISet> ().mSimpleAIList.Add (this);
			this.AddComponent<AIMove> (new AIMove());
		}

		public void Destroy()
		{
			SimpleAISetSingleton.getInstance ().GetComponent<SimpleAISet> ().Delete (this);
			GAPopulation.allDic [this.mType].Delete (this);
		}

		public void Create()
		{
			ECSWorld.MainWorld.registerEntityAfterInit (this);
			SimpleAISetSingleton.getInstance ().GetComponent<SimpleAISet> ().mSimpleAIList.Add (this);
			GAPopulation.allDic [this.mType].Add (this);
		}

	}

	public class SimpleAISet:UComponent
	{
		public List<SimpleAI> mSimpleAIList = new List<SimpleAI>();
		public List<SimpleAI> LastFrameRemove = new List<SimpleAI> ();

		public void Delete(SimpleAI pSimpleAI)
		{
			LastFrameRemove.Add (pSimpleAI);
		}

		public SimpleAI[] FindAIsWithRiaus(Vector3 center,float maxdis,LayerMask layermask,string tag)
		{
			Collider[] myCollider = Physics.OverlapSphere (center,maxdis,layermask);
			List<SimpleAI> result = new List<SimpleAI> ();
			List<GameObject> tGameObjects = new List<GameObject> ();
			for (int i = 0; i < myCollider.Length; i++)
			{
				if (myCollider [i].gameObject.tag == tag)
				{
					tGameObjects.Add (myCollider[i].gameObject);
				}
			}
			foreach (var v1 in tGameObjects)
			{
				foreach (var v2 in mSimpleAIList)
				{
					if (v1 == v2.mAIRT) 
					{
						result.Add (v2);
						break;
					}
				}
			}
			return result.ToArray ();
		}

		public SimpleAI FindByGameObject(GameObject g)
		{
			foreach (SimpleAI sAI in mSimpleAIList)
			{
				if (sAI.mAIRT == g) 
				{
					return sAI;
				}
			}
			return null;
		}

		public SimpleAI FindWithRiaus(Vector3 center,float maxdis,LayerMask layermask,string tag)
		{
			Collider[] myCollider = Physics.OverlapSphere (center,maxdis,layermask);
			GameObject fir = null;
			for (int i = 0; i < myCollider.Length; i++)
			{
				if (myCollider [i].gameObject.tag == tag)
				{
					fir = myCollider [i].gameObject;
					break;
				}
			}
			if (fir == null) 
			{
				return null;
			}
			else
			{
				foreach (var v in mSimpleAIList)
				{
					if (v.mAIRT == fir)
					{
						return v;
					}
				}
				return null;
			}
		}

	}



	public class SimpleAISetSingleton:UEntity
	{
		private static SimpleAISetSingleton mSimpleAISetSingleton;

		public static SimpleAISetSingleton getInstance()
		{
			if (mSimpleAISetSingleton == null)
				mSimpleAISetSingleton = new SimpleAISetSingleton ();
			return mSimpleAISetSingleton;
		}

		public override void Init ()
		{
			base.Init ();
			SimpleAISetSingleton.getInstance ().AddComponent<SimpleAISet> (new SimpleAISet ());
		}

	}




	public class SimpleAIComputeSystem:USystem
	{
		public override void Init ()
		{
			base.Init ();
			this.AddRequestComponent (typeof(SimpleAISet));
		}

		public override void Update (UEntity uEntity)
		{
			base.Update (uEntity);

			List<SimpleAI> last = uEntity.GetComponent<SimpleAISet>().LastFrameRemove;
			foreach (var vAI in last)
			{
				uEntity.GetComponent<SimpleAISet> ().mSimpleAIList.Remove (vAI);
				uEntity.mWorld.deleteEntity (vAI);
				GameObject.Destroy (vAI.mAIRT);
			}

			last.Clear ();

			List<SimpleAI> tAIList = uEntity.GetComponent<SimpleAISet> ().mSimpleAIList;

			for (int i = 0; i < tAIList.Count; i++)
			{
				int lod = tAIList[i].GetComponent<LODComponent>().mLOD;
				if (lod > LODComponent.maxLOD / 2) 
				{
					if(tAIList [i].mAIRT.GetComponent<Animator> ()!=null)
						tAIList [i].mAIRT.GetComponent<Animator> ().enabled = false;
					return;
				}
				else
				{
					if(tAIList [i].mAIRT.GetComponent<Animator> ()!=null)
						tAIList [i].mAIRT.GetComponent<Animator> ().enabled = true;
				}
				string code = tAIList [i].mSimpleAIStateJudger.DoJudge (tAIList[i]);
				tAIList [i].mSimpleAIRunner.DoRun (tAIList[i],code);
			}
		}
			
	}

};
