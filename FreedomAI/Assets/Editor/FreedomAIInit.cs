using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FreedomAIInit : EditorWindow 
{
	[MenuItem("FreedomAI/Init")]
	static void Init()
	{
		GameObject[] allObject = GameObject.FindGameObjectsWithTag("Risk");
		for (int i = 0; i < allObject.Length; i++)
			if (!allObject [i].GetComponent<RiskObject> ())
				allObject [i].AddComponent<RiskObject> ();
		if (GameObject.Find ("AIWorld") == null) 
		{
			GameObject g = new GameObject ();
			g.name = "AIWorld";
			g.AddComponent<InfluenceMapRT> ();
		}
		if (GameObject.Find ("AILOD") == null) 
		{
			GameObject g = new GameObject ();
			g.name = "AILOD";
			g.AddComponent<AILOD> ();
		}
	}
}
