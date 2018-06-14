using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityECS;
using FreedomAI;
using System.Reflection;
using System;

public class InfluenceMapRT : MonoBehaviour 
{
	public AIWorld aiWorld = new AIWorld();
	public string DemoWorldName;
	public Vector3 center;
	public float width;
	public float height;
	public int w_Tile;
	public int h_Tile;
	public GameObject mPlane;

	void Awake()
	{
		InfluenceMap.getInstance ().center = new Vector2 (center.x,center.z);
		InfluenceMap.getInstance ().DefaultY = center.y;
		InfluenceMap.getInstance ().height = height;
		InfluenceMap.getInstance ().width = width;
		InfluenceMap.getInstance ().htileCount = h_Tile;
		InfluenceMap.getInstance ().wtileCount = w_Tile;
		InfluenceMap.getInstance ().mPlane = mPlane;
		InfluenceMap.getInstance ().Init ();
		GAPopulation.GlobalInit ();
		GAPopulationManager.getInstance ().Init ();
		Type type = Type.GetType (DemoWorldName);
		aiWorld = (AIWorld)type.Assembly.CreateInstance (DemoWorldName);
		aiWorld.Init ();
	}

	void Update()
	{
		aiWorld.Update ();
		GAPopulationManager.getInstance ().Update ();
	}

}
