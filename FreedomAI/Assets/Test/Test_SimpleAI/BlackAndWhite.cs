using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityECS;
using FreedomAI;

public class BlackComponent:UComponent
{
	public SimpleAI firWhite;
	public float timer1 = 0.0f;
	public float timer2 = 0.0f;
	public Vector3 target;
}

public class WhiteComponent:UComponent
{
	public SimpleAI firWhite;
	public bool mather;
	public float timer1 = 0.0f;
	public float timer2 = 0.0f;
	public Vector3 target;
}

public class BlackJudge:SimpleAIStateJudger
{
	public string DoJudge(SimpleAI sAI)
	{
		float maxdis = 3.0f;
		SimpleAI gALL = SimpleAISetSingleton.getInstance ().GetComponent<SimpleAISet> ().FindWithRiaus (sAI.mAIRT.transform.position,maxdis,1<<LayerMask.NameToLayer("Default"),"White");
	//	sAI.GetComponent<BlackComponent> ().firWhite = gALL;
		sAI.GetComponent<BlackComponent> ().timer1 += Time.deltaTime;
		if (sAI.GetComponent<BlackComponent> ().timer1 >= 1.0f) 
		{
			if (gALL != null)
			{
				sAI.GetComponent<BlackComponent> ().timer1 = 0.0f;
				sAI.GetComponent<BlackComponent> ().firWhite = gALL;
				return "Attack";
			}
			else
				return "Run";
		}
		else
			return "Run";
	}
}

public class BlackRunner:SimpleAIRunner
{
	public void DoRun(SimpleAI sAI,string code)
	{
		if (code == "Attack") {
			
				sAI.GetComponent<BlackComponent> ().timer1 = 0.0f;
				if(sAI.GetComponent<BlackComponent> ().firWhite!=null)
					SimpleAISetSingleton.getInstance ().GetComponent<SimpleAISet> ().Delete (sAI.GetComponent<BlackComponent> ().firWhite);
		}
		else
		{
			if (sAI.GetComponent<BlackComponent> ().timer2 >= 7.0f) {
				sAI.GetComponent<BlackComponent> ().timer2 = 0.0f;
				sAI.GetComponent<BlackComponent> ().target = sAI.GeneratePos + new Vector3 (Random.Range(-15,15),0,Random.Range(-15,15));
			} else {
				sAI.GetComponent<BlackComponent> ().timer2 += Time.deltaTime;
			}
			sAI.GetComponent<AIMove> ().mDirection = sAI.GetComponent<BlackComponent> ().target - sAI.mAIRT.transform.position;
			sAI.GetComponent<AIMove> ().mVelocity = 5.0f;
		}
	}
}

public class WhiteJudge:SimpleAIStateJudger
{
	public string DoJudge(SimpleAI sAI)
	{
		float maxdis = 3.0f;
		SimpleAI gALL = SimpleAISetSingleton.getInstance ().GetComponent<SimpleAISet> ().FindWithRiaus (sAI.mAIRT.transform.position,maxdis,1<<LayerMask.NameToLayer("Default"),"White");
		//	sAI.GetComponent<BlackComponent> ().firWhite = gALL;
		sAI.GetComponent<WhiteComponent> ().timer1 += Time.deltaTime;
		if (sAI.GetComponent<WhiteComponent> ().timer1 >= 15.0f) 
		{
			if (gALL != null)
			{
				sAI.GetComponent<WhiteComponent> ().timer1 = 0.0f;
				sAI.GetComponent<WhiteComponent> ().firWhite = gALL;
				return "Generate";
			}
			else
				return "Run";
		}
		else
			return "Run";
	}
}

public class WhiteRunner:SimpleAIRunner
{
	public void DoRun(SimpleAI sAI,string code)
	{
		if (code == "Generate") {
				if (sAI.GetComponent<WhiteComponent> ().firWhite != null&&sAI.GetComponent<WhiteComponent>().mather) 
				{
					Vector3 genepos = (sAI.mAIRT.transform.position + sAI.GetComponent<WhiteComponent> ().firWhite.mAIRT.transform.position)/2;
					SimpleAI tAI = new SimpleAI ();
					sAI.mWorld.registerEntityAfterInit (tAI);
					tAI.Init (sAI.mSimpleAIRunner,sAI.mSimpleAIStateJudger,sAI.mAITemplate,sAI.mPlayer,genepos);
					tAI.AddComponent<WhiteComponent> (new WhiteComponent ());
					tAI.GetComponent<WhiteComponent> ().mather = (Random.Range (0, 2) % 2 == 0) ? true : false;

				}
		}
		else
		{
			if (sAI.GetComponent<WhiteComponent> ().timer2 >= 5.0f) {
				sAI.GetComponent<WhiteComponent> ().timer2 = 0.0f;
				sAI.GetComponent<WhiteComponent> ().target = sAI.GeneratePos + new Vector3 (Random.Range(-15,15),0,Random.Range(-15,15));
			} else {
				sAI.GetComponent<WhiteComponent> ().timer2 += Time.deltaTime;
			}
			sAI.GetComponent<AIMove> ().mDirection = sAI.GetComponent<WhiteComponent> ().target - sAI.mAIRT.transform.position;
			sAI.GetComponent<AIMove> ().mVelocity = 5.0f;
		}
	}
}

public class blackAndWhiteWorld:AIWorld
{
	public override void registerAllComponent ()
	{
		base.registerAllComponent ();
		registerComponent (typeof(WhiteComponent));
		registerComponent (typeof(BlackComponent));
	}	
};

