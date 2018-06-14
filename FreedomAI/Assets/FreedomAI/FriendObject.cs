using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FreedomAI;
using UnityECS;


public class FriendObject : MonoBehaviour
{

	public float maxDistance;

	UEntity mUEntity;

	void Start ()
	{
		mUEntity = new UEntity ();
		ECSWorld.MainWorld.registerEntityAfterInit (mUEntity);
		InfluenceMapTrigger tInfluenceMapTrigger = new InfluenceMapTrigger ();
		tInfluenceMapTrigger.mWhere = "friend";
		tInfluenceMapTrigger.maxInfluence = maxDistance;
		tInfluenceMapTrigger.mGameObject = this.gameObject;
		tInfluenceMapTrigger.mIMComputer = DefaultFunc.friendComputer;

		mUEntity.AddComponent<InfluenceMapTrigger> (tInfluenceMapTrigger);
		tInfluenceMapTrigger.Init ();
	//	ECSWorld.MainWorld.registerEntityAfterInit (mUEntity);
	}
	


	void Update ()
	{
		
	}
}
