using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityECS;

public class StateComponent:UComponent
{
	public bool AttackState = false;
}

public class BattleComponent:UComponent
{
	public GameObject bullet;
	public float Velocity;
}

public class MoveComponent:UComponent
{
	public Vector3 dir;
	public float velocity;
}

public class RenderComponent:UComponent
{
	public GameObject renderObject;
	public override void Release ()
	{
		GameObject.Destroy (renderObject);
	}
}

public class DestroyComponent:UComponent
{
	public float DestroyTime;
}


public class DestroySystem:USystem
{
	public override void Update (UEntity uEntity)
	{
		if (uEntity.GetComponent<DestroyComponent> ().DestroyTime > 0)
		{
			uEntity.GetComponent<DestroyComponent> ().DestroyTime -= Time.deltaTime;
		}
		else
		{
			uEntity.Release ();
		}
	}
}

public class AttackSystem:USystem
{
	public override void Update (UEntity uEntity)
	{
		if (uEntity.GetComponent<StateComponent> ().AttackState)
		{
			GameObject g = uEntity.GetComponent<RenderComponent> ().renderObject;
			UEntity tUEntity = new UEntity ();
			mWorld.registerEntity (tUEntity);
			tUEntity.mAllBitBunch.SetCount ((int)mWorld.mComponentCount);
			MoveComponent mc = new MoveComponent ();
			mc.dir = g.transform.forward;
			mc.velocity = uEntity.GetComponent<BattleComponent> ().Velocity;
			tUEntity.AddComponent<MoveComponent> (mc);
			GameObject g1 = GameObject.Instantiate (uEntity.GetComponent<BattleComponent> ().bullet, g.transform.position, Quaternion.identity);
			RenderComponent rc = new RenderComponent ();
			rc.renderObject = g1;
			DestroyComponent dc = new DestroyComponent ();dc.DestroyTime = 5.0f;
			tUEntity.AddComponent<RenderComponent> (rc);
			tUEntity.AddComponent<DestroyComponent> (dc);
			uEntity.GetComponent<StateComponent> ().AttackState = false;
		}
	}
}



public class MoveSystem:USystem
{
	public override void Update (UEntity uEntity)
	{
		GameObject renderobject = uEntity.GetComponent<RenderComponent> ().renderObject;
		renderobject.transform.Translate (uEntity.GetComponent<MoveComponent>().dir*uEntity.GetComponent<MoveComponent>().velocity*Time.deltaTime);
	} 
}

public class InputSystem:USystem
{
	public override void Update(UEntity uEntity)
	{
		//Debug.Log ("asd");
		uEntity.GetComponent<MoveComponent> ().dir = Vector3.zero;
		uEntity.GetComponent<MoveComponent> ().velocity = 0.0f;
		if (Input.GetKeyDown (KeyCode.Q)) 
		{
			uEntity.GetComponent<StateComponent> ().AttackState = true;
		}
		if (Input.GetKey (KeyCode.W)) 
		{
			uEntity.GetComponent<MoveComponent> ().dir += Vector3.left;
			uEntity.GetComponent<MoveComponent> ().velocity = 3.0f;
		}
		if (Input.GetKey (KeyCode.A)) 
		{
			uEntity.GetComponent<MoveComponent> ().dir += Vector3.up;
			uEntity.GetComponent<MoveComponent> ().velocity = 3.0f;
		}
		if (Input.GetKey (KeyCode.S)) 
		{
			uEntity.GetComponent<MoveComponent> ().dir += Vector3.down;
			uEntity.GetComponent<MoveComponent> ().velocity = 3.0f;
		}
		if (Input.GetKey (KeyCode.D)) 
		{
			uEntity.GetComponent<MoveComponent> ().dir += Vector3.right;
			uEntity.GetComponent<MoveComponent> ().velocity = 3.0f;
		}
	}
}

