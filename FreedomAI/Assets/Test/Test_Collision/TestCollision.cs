using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityECS;
using FreedomAI;

public class TestCollision : MonoBehaviour
{
	public bool isTriggerEnter = false;
	public GameObject TriggerObject = null;
	void Start ()
	{
		
	}

	void OnTriggerEnter(Collider other)
	{
		isTriggerEnter = true;
		TriggerObject = other.gameObject;
		StartCoroutine (closeTrigger());
	}
	

	void Update ()
	{
		
	}

	IEnumerator closeTrigger()
	{
		yield return new WaitForEndOfFrame ();
		isTriggerEnter = false;
		TriggerObject = null;
	}
}

public class TestCollisionAI
{
	public static float round2runaway(UEntity uEntity)
	{
		GameObject aiRT = uEntity.GetComponent<BaseAIComponent> ().mAIRT;
		TestCollision tc = null;
		if (aiRT.GetComponent<TestCollision> () != null)
			tc = aiRT.GetComponent<TestCollision> ();
		else if (aiRT.GetComponentInChildren<TestCollision> () != null)
			tc = aiRT.GetComponent<TestCollision> ();

		if (tc.TriggerObject == null)
			return 0.0f;

		if (tc.isTriggerEnter == true && tc.TriggerObject.name == "Boom")
		{
			Debug.Log (tc.TriggerObject.name);
			return 1.0f;
		}
		else
			return 0.0f;
	}
}
