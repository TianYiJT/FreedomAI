using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FreedomAI;
using UnityECS;



public class AntDemo : MonoBehaviour 
{
	
	public GameObject Home;
	public GameObject Target;
	public GameObject prefab;
	public GameObject LeftDown;
	public GameObject RightUp;
	public int mCount;
	private AntPopulation mAntPopulation = new AntPopulation();

	public void showFood()
	{
		mAntPopulation.shouldWhat = 1;
	}

	public void showHome()
	{
		mAntPopulation.shouldWhat = 0;
	}

	void Start ()
	{
		mAntPopulation.mRadiationRiaus = 32;

		for (int i = 0; i < mCount; i++)
		{
			Vector3 tv = new Vector3 (Random.Range(LeftDown.transform.position.x,RightUp.transform.position.x),LeftDown.transform.position.y,Random.Range(LeftDown.transform.position.z,RightUp.transform.position.z));
			GameObject tg = GameObject.Instantiate (prefab,tv,Quaternion.identity) as GameObject;
			tg.tag = prefab.tag;
		}

		mAntPopulation.Init (prefab.tag,Home.tag,Target.tag,2);

		AntPopulation.StartFind (mAntPopulation.mListIndex);
	}
	

	void Update () 
	{
		
	}
}
