using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICollision : MonoBehaviour 
{
	
	[HideInInspector]
	public Collision mCollision;
	[HideInInspector]
	public Collider mCollider;
	[HideInInspector]
	public bool mCollisionEnter = false;
	[HideInInspector]
	public bool mCollisionStay = false;
	[HideInInspector]
	public bool mCollisionExit = false;
	[HideInInspector]
	public bool mTriggerEnter = false;
	[HideInInspector]
	public bool mTriggerExit = false;
	[HideInInspector]
	public bool mTriggerStay = false;

	void Start ()
	{
		mCollider = null;
		mCollision = null;
	}

	void OnCollisionEnter(Collision collision)
	{
		mCollision = collision;
		mCollisionEnter = true;
	}

	void OnCollisionExit(Collision collision)
	{
		mCollision = collision;
		mCollisionExit = true;
	}

	void OnCollisionStay(Collision collision)
	{
		mCollision = collision;
		mCollisionStay = true;
	}

	void OnTriggerEnter(Collider other)
	{
		mCollider = other;
		mTriggerEnter = true;
	}

	void OnTriggerExit(Collider other)
	{
		mCollider = other;
		mTriggerExit = true;
	}

	void OnTriggerStay(Collider other)
	{
		mCollider = other;
		mTriggerStay = true;
	}

	void Update ()
	{
		mCollisionEnter = false;
		mCollisionExit = false;
		mCollisionStay = false;
		mTriggerEnter = false;
		mTriggerExit = false;
		mTriggerStay = false;
		mCollision = null;
		mCollider = null;
	}
}
