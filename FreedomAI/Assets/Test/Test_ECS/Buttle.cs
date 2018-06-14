using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityECS;

public class Buttle : MonoBehaviour 
{
	public float ButtleVelocity;
	public GameObject character;
	public GameObject buttle;
	ECSWorld mWorld = new ECSWorld();
	AttackSystem atkSystem = new AttackSystem();
	MoveSystem mSystem = new MoveSystem();
	InputSystem iSystem = new InputSystem();
	UEntity characterMain = new UEntity();
	RenderComponent rComponent = new RenderComponent();
	MoveComponent mComponent = new MoveComponent();
	StateComponent sComponent = new StateComponent();
	BattleComponent bComponent = new BattleComponent();
	DestroySystem dSystem = new DestroySystem(); 
	// Use this for initialization
	void Start () 
	{
		mWorld.registerSystem (atkSystem);
		mWorld.registerSystem (iSystem);
		mWorld.registerSystem (mSystem);
		mWorld.registerSystem (dSystem);
		mWorld.registerEntity (characterMain);
		mWorld.registerComponent (typeof(RenderComponent));
		mWorld.registerComponent (typeof(MoveComponent));
		mWorld.registerComponent (typeof(StateComponent));
		mWorld.registerComponent (typeof(BattleComponent));
		mWorld.registerComponent (typeof(DestroyComponent));

		atkSystem.AddRequestComponent (typeof(BattleComponent));
		atkSystem.AddRequestComponent (typeof(RenderComponent));
		atkSystem.AddRequestComponent (typeof(StateComponent));

		mSystem.AddRequestComponent (typeof(MoveComponent));
		mSystem.AddRequestComponent (typeof(RenderComponent));

		iSystem.AddRequestComponent (typeof(MoveComponent));
		iSystem.AddRequestComponent (typeof(StateComponent));

		dSystem.AddRequestComponent (typeof(DestroyComponent));

		rComponent.renderObject = character;
		bComponent.bullet = buttle;bComponent.Velocity = ButtleVelocity;

		characterMain.AddComponent<RenderComponent> (rComponent);
		characterMain.AddComponent<StateComponent> (sComponent);
		characterMain.AddComponent<MoveComponent> (mComponent);
		characterMain.AddComponent<BattleComponent> (bComponent);

		//characterMain.AddComponent<RenderComponent> ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		mWorld.Update ();
	}
}
