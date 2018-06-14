using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FreedomAI;
using UnityECS;

public class GYTWorld:AIWorld
{
	public override void registerAllComponent ()
	{
		base.registerAllComponent ();
		registerComponent (typeof(bombAI));

		registerComponent (typeof(shieldAI));
		registerComponent (typeof(staComponent));
		registerComponent (typeof(SSHComponent));
		registerComponent (typeof(SMREComponent));
		registerComponent(typeof(GYTHPComponent));

		registerComponent (typeof(trapAI));
		registerComponent (typeof(trapComponent));

		registerComponent (typeof(getPlayer));

		registerComponent (typeof(Group4StandardAI));

		registerComponent (typeof(showAll));
	}

	public override void registerAllEntity ()
	{
		base.registerAllEntity ();
	}

	public override void registerAllSystem ()
	{
		base.registerAllSystem ();
		registerSystem (new StaSystem ());
		registerSystem (new TrapSystem ());
		registerSystem ( new SSHSystem());
		registerSystem (new GetPlayerSystem ());
		registerSystem (new TrapHPSystem ());
		registerSystem (new ShowAllSystem());
		registerSystem (new BombHPSystem ());
	}
}