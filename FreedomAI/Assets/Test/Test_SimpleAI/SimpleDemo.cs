using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityECS;
using FreedomAI;

public class SimpleDemo : MonoBehaviour {

	public GameObject black;
	public GameObject white;
	public GameObject instance;
	public int blackcount;
	public int whitecount;
//	blackAndWhiteWorld bwWorld = new blackAndWhiteWorld();

	void Start () 
	{
	//	bwWorld.Init ();
		for (int i = 0; i < blackcount; i++)
		{
			SimpleAI blackAI = new SimpleAI ();
			ECSWorld.MainWorld.registerEntityAfterInit (blackAI);
			blackAI.Init (new BlackRunner(),new BlackJudge(),black,null,instance.transform.position+new Vector3(Random.Range(-20,20),0,Random.Range(-20,20)));
			blackAI.AddComponent<BlackComponent> (new BlackComponent());

		}
		for (int i = 0; i < whitecount; i++)
		{
			SimpleAI whiteAI = new SimpleAI ();
			ECSWorld.MainWorld.registerEntityAfterInit (whiteAI);
			whiteAI.Init (new WhiteRunner(),new WhiteJudge(),white,null,instance.transform.position+new Vector3(Random.Range(-20,20),0,Random.Range(-20,20)));
			whiteAI.AddComponent<WhiteComponent> (new WhiteComponent());
			whiteAI.GetComponent<WhiteComponent> ().mather = (i % 2 == 0) ? true : false;

		}
	}
		
}
