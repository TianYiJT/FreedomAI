using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityECS;
using FreedomAI;

public class RiskObject : MonoBehaviour 
{

	public float risk;
	public float maxDistance;
	UEntity mUEntity;
	// Use this for initialization
	void Start ()
	{
		UEntity mUEntity = new UEntity ();
		ECSWorld.MainWorld.registerEntityAfterInit (mUEntity);
		RiskComponent rComponent = new RiskComponent ();
		rComponent.riskrate = risk;
		mUEntity.AddComponent<RiskComponent> (rComponent);
		InfluenceMapTrigger tInfluenceMapTrigger = new InfluenceMapTrigger ();
		tInfluenceMapTrigger.mWhere = "Risk";
		tInfluenceMapTrigger.maxInfluence = maxDistance;
		tInfluenceMapTrigger.mGameObject = this.gameObject;
		tInfluenceMapTrigger.mIMComputer = DefaultFunc.RiskComputer;

		tInfluenceMapTrigger.Init ();
		mUEntity.AddComponent<InfluenceMapTrigger> (tInfluenceMapTrigger);

	}

	void StartAMonent()
	{

	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}
}
