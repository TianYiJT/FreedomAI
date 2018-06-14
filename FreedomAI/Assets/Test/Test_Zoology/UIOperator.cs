using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityECS;
using FreedomAI;

public enum TESTZOOLOGYMOUSE
{
	ADDGRASS,
	ADDTREE,
	ADDCOW,
	DELETEGRASS,
	DELETETREE,
	DELETECOW,
	NONE
};

public class UIOperator : MonoBehaviour 
{

	private TESTZOOLOGYMOUSE mEnumType = TESTZOOLOGYMOUSE.NONE;

	private GameObject temp;

	public GameObject Cow;
	public GameObject Grass;
	public GameObject Tree;
	public GameObject Player;

	private SimpleAI sAI_;

	private Color mSrc;

	void Start () 
	{
		Player = GameObject.FindGameObjectWithTag ("Player");
		int i = 199;
		bool[] b = FreedomAIEncoderAndDecoder.int2BitBunch (i,100,400,9);
		int f1 = FreedomAIEncoderAndDecoder.BitBunch2int (b,100,400);
	}
	

	void Update ()
	{
		if (temp != null)
			GameObject.Destroy (temp);
		if (sAI_ != null&&sAI_.mAIRT!=null) 
		{
			sAI_.mAIRT.GetComponent<MeshRenderer> ().material.color = mSrc;
			sAI_ = null;
		}
		switch (mEnumType) 
		{
		case TESTZOOLOGYMOUSE.NONE:
			break;
		case TESTZOOLOGYMOUSE.DELETECOW:
			SimpleAI sAI = FindAIByRayCast ("Cow");
			if (sAI != null) 
			{
				sAI_ = sAI;
				mSrc = sAI.mAIRT.GetComponent<MeshRenderer> ().material.color;
				sAI.mAIRT.GetComponent<MeshRenderer> ().material.color = new Color (1.0f,0,0,0.4f);

				if (Input.GetMouseButton (0)) 
				{
					sAI.Destroy ();
				}
			}
			break;
		case TESTZOOLOGYMOUSE.DELETEGRASS:
			SimpleAI sAI1 = FindAIByRayCast ("Glass");
			if (sAI1 != null) 
			{
				sAI_ = sAI1;
				mSrc = sAI1.mAIRT.GetComponent<MeshRenderer> ().material.color;
				sAI1.mAIRT.GetComponent<MeshRenderer> ().material.color = new Color (1.0f,0,0,0.4f);
				if (Input.GetMouseButton (0)) 
				{
					sAI1.Destroy ();
				}
			}
			break;
		case TESTZOOLOGYMOUSE.DELETETREE:
			SimpleAI sAI2 = FindAIByRayCast ("Tree");
			if (sAI2 != null) 
			{
				sAI_ = sAI2;
				mSrc = sAI2.mAIRT.GetComponent<MeshRenderer> ().material.color;
				sAI2.mAIRT.GetComponent<MeshRenderer> ().material.color = new Color (1.0f,0,0,0.4f);
				if (Input.GetMouseButton (0)) 
				{
					sAI2.Destroy ();
				}
			}
			break;
		case TESTZOOLOGYMOUSE.ADDCOW:
			Vector3 pos = FindPointByRayCast ();
			if (pos != Vector3.zero)
			{
				temp = GameObject.Instantiate (Cow,pos,Quaternion.identity) as GameObject;
				temp.GetComponent<MeshRenderer> ().material.color = new Color (0,1,0,0.5f);
				if (Input.GetMouseButton (0))
				{
					CowObject co = new CowObject ();
					co.RandomData ();
					co.Type = true;
					co.mPosition = pos;
					co.Encode ();
					co.InitBreakPoint ();
					SimpleAI scow = RePair(new CowRePairer(),new CowRunner(),new CowJudger(),Cow,
						Player,new CowDestroyer(),"Cow",co,GAPopulation.allDic["Cow"].mTempCount);
					scow.mCharacter = co;
					GAPopulation.allDic ["Cow"].Add (scow);
				}
				else if(Input.GetMouseButton(1))
				{
					CowObject co = new CowObject ();
					co.RandomData ();
					co.Type = false;
					co.mPosition = pos;
					co.Encode ();
					co.InitBreakPoint ();
					SimpleAI scow = RePair(new CowRePairer(),new CowRunner(),new CowJudger(),Cow,
						Player,new CowDestroyer(),"Cow",co,GAPopulation.allDic["Cow"].mTempCount);
					scow.mCharacter = co;
					GAPopulation.allDic ["Cow"].Add (scow);
				}
			}
			break;
		case TESTZOOLOGYMOUSE.ADDGRASS:
			Vector3 pos1 = FindPointByRayCast ();
			if (pos1 != Vector3.zero) 
			{
				temp = GameObject.Instantiate (Grass,pos1,Quaternion.identity) as GameObject;
				temp.GetComponent<MeshRenderer> ().material.color = new Color (0,1,0,0.5f);
				if (Input.GetMouseButton (0))
				{
					glassObject go = new glassObject ();
					go.RandomData ();
					go.mColor = 0.9f;
					go.mPosition = pos1;
					go.Encode ();
					go.InitBreakPoint ();
					SimpleAI sglass = RePair(new GlassRePairer(),new GlassRunner(),new GlassJudger(),Grass,
						Player,new CowDestroyer(),"Glass",go,GAPopulation.allDic["Glass"].mTempCount);
					sglass.mCharacter = go;
					//Debug.Log (FreedomAIEncoderAndDecoder.bitbunch2String(FreedomAIEncoderAndDecoder.float2BitBunch(0.2f,0.0f,1.0f,11)));
					GAPopulation.allDic ["Glass"].Add (sglass);
				}
				else if(Input.GetMouseButton(1))
				{
					glassObject go = new glassObject ();
					go.RandomData ();
					go.mColor = Random.Range (0.1f,0.4f);
					go.mPosition = pos1;
					go.Encode ();
					go.InitBreakPoint ();
					SimpleAI sglass = RePair(new GlassRePairer(),new GlassRunner(),new GlassJudger(),Grass,
						Player,new CowDestroyer(),"Glass",go,GAPopulation.allDic["Glass"].mTempCount);
					sglass.mCharacter = go;
					GAPopulation.allDic ["Glass"].Add (sglass);
				}
			}
			break;
		case TESTZOOLOGYMOUSE.ADDTREE:
			Vector3 pos2 = FindPointByRayCast ();
			if (pos2 != Vector3.zero) 
			{
				temp = GameObject.Instantiate (Tree,pos2,Quaternion.identity) as GameObject;
				temp.GetComponent<MeshRenderer> ().material.color = new Color (0,1,0,0.5f);
				if (Input.GetMouseButton (0))
				{
					TreeObject to = new TreeObject ();
					to.RandomData ();
					to.mStrongRate = Random.Range (0.0f,0.5f);
					to.mPosition = pos2;
				//	go.mHeight = Random.Range (1.0f,1.5f);
					to.Encode ();
					to.InitBreakPoint ();
					SimpleAI sTree = RePair(new TreeRePairer(),new TreeRunner(),new TreeJudger(),Tree,
						Player,new CowDestroyer(),"Tree",to,GAPopulation.allDic["Tree"].mTempCount);
					sTree.mCharacter = to;
					GAPopulation.allDic ["Tree"].Add (sTree);
				}
				else if(Input.GetMouseButton(1))
				{
					TreeObject to = new TreeObject ();
					to.RandomData ();
					to.mStrongRate = Random.Range (0.5f,1.0f);
					to.mPosition = pos2;

					to.Encode ();
					to.InitBreakPoint ();
					SimpleAI sTree = RePair(new TreeRePairer(),new TreeRunner(),new TreeJudger(),Tree,
						Player,new CowDestroyer(),"Tree",to,GAPopulation.allDic["Tree"].mTempCount);
					sTree.mCharacter = to;
					GAPopulation.allDic ["Tree"].Add (sTree);
				}
			}
			break;
		}
	}

	private SimpleAI RePair(SimpleAIRePairer pRePairer,SimpleAIRunner pRunner,SimpleAIStateJudger pJudger,
		GameObject pPrefab,GameObject pPlayer,SimpleAIDestroyer pDestroyer,string pType,GAObject pGAObject,int index)
	{
		SimpleAI sAI = new SimpleAI ();
		sAI.Init (pRunner,pJudger,pPrefab,pPlayer,pGAObject.mPosition);
		sAI.mSimpleAIRepairer = pRePairer;
		sAI.mSimpleAIDestroyer = pDestroyer;
		sAI.mType = pType;
		sAI.mCharacter = pGAObject;
		sAI.mSimpleAIRepairer.DoRePair(sAI);
		sAI.mName = pType + "index" + index;
		return sAI;
	}

	public SimpleAI FindAIByRayCast(string tag)
	{
		Vector3 ScreenPoint = Input.mousePosition;
		Ray myRay = Camera.main.ScreenPointToRay (ScreenPoint);
		RaycastHit hit;

		if (Physics.Raycast (myRay, out hit, 100.0f, 1 << LayerMask.NameToLayer ("Default"))) 
		{
			if (hit.collider.tag == tag)
			{
				SimpleAI my = SimpleAISetSingleton.getInstance ().GetComponent<SimpleAISet> ().FindByGameObject (hit.collider.gameObject);
				return my;
			}
		}
		return null;
	}

	public Vector3 FindPointByRayCast()
	{
		Vector3 ScreenPoint = Input.mousePosition;
		Ray myRay = Camera.main.ScreenPointToRay (ScreenPoint);
		RaycastHit hit;
		if (Physics.Raycast (myRay, out hit, 100.0f, 1 << LayerMask.NameToLayer ("Default"))) 
		{
			if (hit.collider.tag == "Plane") 
			{
				return hit.point;
			}
		}
		return Vector3.zero;
	}


		

	public void AddCow()
	{
		mEnumType = TESTZOOLOGYMOUSE.ADDCOW;
	}

	public void AddGrass()
	{
		mEnumType = TESTZOOLOGYMOUSE.ADDGRASS;
	}

	public void AddTree()
	{
		mEnumType = TESTZOOLOGYMOUSE.ADDTREE;
	}

	public void DeleteCow()
	{
		mEnumType = TESTZOOLOGYMOUSE.DELETECOW;
	}

	public void DeleteTree()
	{
		mEnumType = TESTZOOLOGYMOUSE.DELETETREE;
	}

	public void DeleteGrass()
	{
		mEnumType = TESTZOOLOGYMOUSE.DELETEGRASS;
	}

}
