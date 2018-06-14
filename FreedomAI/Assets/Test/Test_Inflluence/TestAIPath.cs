using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityECS;
using FreedomAI;
using UnityEngine.UI;

public class TestAIPath : MonoBehaviour 
{
	public GameObject target;
	Stack<Vector3> path = new Stack<Vector3>();
	AStarComputerformula pAStarComputerformula;
	Vector3 tempTarget;
	public float Velocity;

	private bool isStart = false;

	private float EulerDistanceRate;
	private float RiskRate;
	private float FriendRate;
	private float TerrianRate;

	public Slider s_euler;
	public Slider s_riskrate;
	public Slider s_friendrate;
	public Slider s_terrianrate;
	public Vector3 StartPoint;

	void Start ()
	{
		
	}

	public void StartFind()
	{
		EulerDistanceRate = s_euler.value;
		RiskRate = s_riskrate.value;
		FriendRate = s_friendrate.value;
		TerrianRate = s_terrianrate.value;
		pAStarComputerformula = new AStarComputerformula ();
		pAStarComputerformula.Add (DefaultFunc.AStarByEulerDistance,EulerDistanceRate);
		pAStarComputerformula.Add (DefaultFunc.AStarByRiskRate,RiskRate);
		pAStarComputerformula.Add (DefaultFunc.AStarByFriendRate,FriendRate);
		pAStarComputerformula.Add (DefaultFunc.AStarByTerrian,TerrianRate);
		tempTarget = Vector3.zero;
		isStart = true;
	}

	public void ReSet()
	{
		isStart = false;
		this.transform.position = StartPoint;
		path.Clear ();
		tempTarget = Vector3.zero;
	}

	void Update ()
	{
		//InfluenceMap.getInstance ().Show (0);
		if (!isStart)
			return;
		if (path.Count == 0&&Vector3.Distance(this.transform.position,target.transform.position)>1.0f&&tempTarget==Vector3.zero)
		{
			path = InfluenceMap.getInstance ().AStarFindPath (this.gameObject.transform.position,target.transform.position,pAStarComputerformula);
			//Debug.Log (path.Count);
		}
		if (InfluenceMap.getInstance ().isWall (InfluenceMap.getInstance ().getTilefromPosition (new Vector2 (tempTarget.x, tempTarget.z))))
		{
			path = InfluenceMap.getInstance ().AStarFindPath (this.gameObject.transform.position,target.transform.position,pAStarComputerformula);
		}
		if (tempTarget == Vector3.zero&&path!=null)
		{
			if (path.Count != 0)
				tempTarget = path.Pop ();
		}
		Vector3 v = new Vector3(tempTarget.x - this.transform.position.x,0,tempTarget.z - this.transform.position.z);
		v.Normalize ();
		if (Vector3.Distance (this.transform.position, tempTarget) > 1.0f)
		{
			this.transform.position += (v) * Time.deltaTime * Velocity;
		}
		else
			tempTarget = Vector3.zero;
	}
}
