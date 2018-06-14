using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityECS;
using FreedomAI;

namespace FreedomAI
{



	public class glassObject:GAObject
	{
		public float mColor;
		public float mHeight;
		public int mHp;
		public int mAllhp;
		public static Vector3 mpos_leftdown;
		public static Vector3 mpos_rightup;
		public float timer = 0.0f;

		public override void Decode ()
		{


			bool[][] mAllBitBunch = FreedomAIEncoderAndDecoder.BreakBitBunch (mbitbunch,mBreakPoint);

			mColor= FreedomAIEncoderAndDecoder.BitBunch2float (mAllBitBunch[0],0.0f,1.0f);
			mHeight = FreedomAIEncoderAndDecoder.BitBunch2float (mAllBitBunch[1],1.0f,2.5f);
			mAllhp = FreedomAIEncoderAndDecoder.BitBunch2int (mAllBitBunch[2],2000,4000);
			float tx =  FreedomAIEncoderAndDecoder.BitBunch2float (mAllBitBunch[3],0.0f,1.0f);tx = (mpos_rightup.x - mpos_leftdown.x) * tx + mpos_leftdown.x;
			float tz = FreedomAIEncoderAndDecoder.BitBunch2float (mAllBitBunch [4], 0.0f, 1.0f);tz = (mpos_rightup.z - mpos_leftdown.z) * tz + mpos_leftdown.z;
			mPosition = new Vector3 (tx,mpos_rightup.y,tz); 
			mHp = mAllhp;
		}

		public override void Encode ()
		{
		//	Debug.Log (mColor);
			bool[] mColorBit = FreedomAIEncoderAndDecoder.float2BitBunch (mColor,0.0f,1.0f,11);
			bool[] mHeight_Bit = FreedomAIEncoderAndDecoder.float2BitBunch (mHeight,1.0f,2.5f,11);
			bool[] mAllhp_Bit = FreedomAIEncoderAndDecoder.int2BitBunch (mAllhp,2000,4000,11);
			bool[] mPosition_X = FreedomAIEncoderAndDecoder.float2BitBunch ((mPosition.x-mpos_leftdown.x)/(mpos_rightup.x-mpos_leftdown.x),0.0f,1.0f,11);
			bool[] mPosition_Z = FreedomAIEncoderAndDecoder.float2BitBunch ((mPosition.z-mpos_leftdown.z)/(mpos_rightup.z-mpos_leftdown.z),0.0f,1.0f,11);
			mbitbunch = FreedomAIEncoderAndDecoder.JointBitBunch (mColorBit,mHeight_Bit,mAllhp_Bit,mPosition_X,mPosition_Z);

		}

		public override void InitBreakPoint ()
		{
			mBreakPoint.Clear ();
			mBreakPoint.Add (11);
			mBreakPoint.Add (11);
			mBreakPoint.Add (11);
			mBreakPoint.Add (11);
			mBreakPoint.Add (11);
		}

		public override float Evaluate ()
		{
			return (float)(mHp) / (float)mAllhp;
		}

		public override void RandomData ()
		{
			
			mColor =  Random.Range (0.0f,1.0f);
			mHeight = Random.Range (1.0f,2.5f);
			mAllhp = Random.Range (2000,4000);mHp = mAllhp;
			mPosition = new Vector3 ();
			mPosition.x = Random.Range (mpos_leftdown.x,mpos_rightup.x);
			mPosition.z = Random.Range (mpos_leftdown.z,mpos_rightup.z);
			mPosition.y = mpos_rightup.y;
		}

		public override string ToString ()
		{
			string s = "";
			for (int i = 0; i < mbitbunch.Length; i++)
				if(mbitbunch[i])
					s +="1";
				else
					s+="0";
			return s;
		}
	}

	public class GlassJudger:SimpleAIStateJudger
	{
		public string DoJudge(SimpleAI pSimpleAI)
		{
			if(((glassObject)pSimpleAI.mCharacter).mHp<0)
				return "Die";
			else
				return "Idle";
		}
	}

	public class GlassRunner:SimpleAIRunner
	{
		public void DoRun(SimpleAI sAI,string code)
		{
			if (code == "Die")
			{
				sAI.Destroy ();
			}
			glassObject go = (glassObject)sAI.mCharacter;
			if (go.timer < 1.0f) 
			{
				go.timer += Time.deltaTime;
				return;
			}
			SimpleAI[] SimpleAIs = SimpleAISetSingleton.getInstance ().GetComponent<SimpleAISet> ().FindAIsWithRiaus (sAI.mAIRT.transform.position,5.0f,1<<LayerMask.NameToLayer("Default"),"Tree");
			go.timer = 0.0f;

			go.mHp -= 50;
			for (int i = 0; i < SimpleAIs.Length; i++) 
			{
				TreeObject to = (TreeObject)SimpleAIs [i].mCharacter;
				if (to.mTempLife > 1500) 
				{
					go.mHp += (int)(to.mTempLife / 50.0f);
				}
			}

		}
	}

	public class GlassDestroyer:SimpleAIDestroyer
	{
		public void DoDestroy(SimpleAI sAI)
		{
			SimpleAISetSingleton.getInstance ().GetComponent<SimpleAISet> ().Delete (sAI);
			GameObject.Destroy (sAI.mAIRT);
			GAPopulation.allDic [sAI.mType].Delete (sAI);
		}
	}

	public class CowObject:GAObject
	{
		public float mStrongRate;

		public float mVelocity;

		public int mLife;

		public int mTempLife;

		public static Vector3 mpos_leftdown;
		public static Vector3 mpos_rightup;

		public float timer = 0.0f; 
		public SimpleAI mGlassTarget = null;
		public float RoundTimer = 0.0f;
		public float EatTimer = 0.0f;
		public int lastTime = 0;
		public bool Type;
		public Vector3 RoundTarget = Vector3.zero;

		public override void Decode ()
		{
			bool[][] mAllBitBunch = FreedomAIEncoderAndDecoder.BreakBitBunch (mbitbunch,mBreakPoint);
			string s = "";
			mStrongRate = FreedomAIEncoderAndDecoder.BitBunch2float (mAllBitBunch[0],0.0f,1.0f);
			mVelocity = FreedomAIEncoderAndDecoder.BitBunch2float (mAllBitBunch[1],6.0f,9.0f);
			mLife = FreedomAIEncoderAndDecoder.BitBunch2int (mAllBitBunch[2],1000,2000);mTempLife = mLife;
			float tx =  FreedomAIEncoderAndDecoder.BitBunch2float (mAllBitBunch[3],0.0f,1.0f);tx = (mpos_rightup.x - mpos_leftdown.x) * tx + mpos_leftdown.x;
			float tz = FreedomAIEncoderAndDecoder.BitBunch2float (mAllBitBunch [4], 0.0f, 1.0f);tz = (mpos_rightup.z - mpos_leftdown.z) * tz + mpos_leftdown.z;
			Type = FreedomAIEncoderAndDecoder.BitBunch2bool (mAllBitBunch[5]);
			mPosition = new Vector3 (tx,mpos_rightup.y,tz); 
		}

		public override void Encode ()
		{
			bool[] tStrongRate_bit = FreedomAIEncoderAndDecoder.float2BitBunch (mStrongRate,0.0f,1.0f,10);
			bool[] tVelocity_bit = FreedomAIEncoderAndDecoder.float2BitBunch (mVelocity,6.0f,9.0f,10);
			bool[] tLife = FreedomAIEncoderAndDecoder.int2BitBunch (mLife,1000,2000,10);
			bool[] mPosition_X = FreedomAIEncoderAndDecoder.float2BitBunch ((mPosition.x-mpos_leftdown.x)/(mpos_rightup.x-mpos_leftdown.x),0.0f,1.0f,11);
			bool[] mPosition_Z = FreedomAIEncoderAndDecoder.float2BitBunch ((mPosition.z-mpos_leftdown.z)/(mpos_rightup.z-mpos_leftdown.z),0.0f,1.0f,11);
			bool[] mType = FreedomAIEncoderAndDecoder.bool2BitBunch (Type);
			mbitbunch = FreedomAIEncoderAndDecoder.JointBitBunch (tStrongRate_bit,tVelocity_bit,tLife,mPosition_X,mPosition_Z,mType);

		}

		public override void InitBreakPoint ()
		{
			mBreakPoint.Clear ();
			mBreakPoint.Add (10);
			mBreakPoint.Add (10);
			mBreakPoint.Add (10);
			mBreakPoint.Add (11);
			mBreakPoint.Add (11);
			mBreakPoint.Add (1);
		}

		public override float Evaluate ()
		{
			return (float)(mTempLife-1000) / (float)2000;
		}

		public override void RandomData ()
		{
			System.Random srd = new System.Random ();

			mStrongRate = Random.Range (0.0f,1.0f);
			mVelocity = Random.Range (6.0f,9.0f);
			mLife = Random.Range (1000,2000);mTempLife = mLife;
			mPosition = new Vector3 ();
			mPosition.x = Random.Range (mpos_leftdown.x,mpos_rightup.x);
			mPosition.z = Random.Range (mpos_leftdown.z,mpos_rightup.z);
			mPosition.y = mpos_rightup.y;
			if (Random.Range (0, 2) == 0)
				Type = false;
			else
				Type = true;
		}

		public override string ToString ()
		{
			return mStrongRate.ToString () + "  " + mVelocity.ToString () + "  " + mLife.ToString() + "  " + mPosition.ToString(); 
		}

	}
		
	public class CowJudger:SimpleAIStateJudger
	{
		public string DoJudge(SimpleAI sAI)
		{
			if (((CowObject)sAI.mCharacter).mTempLife < 0)
				return "Die";
			else
			{
				if ((int)(((CowObject)sAI.mCharacter).timer / 8.0f) % 2 == 0)
					return "FindFood";
				else
					return "Round";
			}
		}	
	};

	public class GlassRePairer:SimpleAIRePairer
	{
		public void DoRePair(SimpleAI sAI)
		{
			//glassObject tgo = sAI.mCharacter;
			sAI.mAIRT.GetComponent<MeshRenderer> ().material.color = Color.Lerp(Color.yellow,Color.green,((glassObject)sAI.mCharacter).mColor);
			//sAI.mAIRT.GetComponent<MeshRenderer> ().material.color = Color.Lerp(((glassObject)sAI.mCharacter).mColor,Color.yellow,(((glassObject)sAI.mCharacter).mHeight-1.0f)/1.5f);
			sAI.mAIRT.transform.localScale = new Vector3 (((glassObject)sAI.mCharacter).mAllhp/2000.0f,((glassObject)sAI.mCharacter).mHeight,((glassObject)sAI.mCharacter).mAllhp/2000.0f);
			sAI.mAIRT.tag = "Glass";
		}
	}

	public class CowRePairer:SimpleAIRePairer
	{
		public void DoRePair(SimpleAI sAI)
		{
			sAI.mAIRT.transform.localScale = new Vector3 (1+((CowObject)sAI.mCharacter).mStrongRate,1+((CowObject)sAI.mCharacter).mStrongRate,1+((CowObject)sAI.mCharacter).mStrongRate);
			if (((CowObject)(sAI.mCharacter)).Type) 
			{
				sAI.mAIRT.GetComponent<MeshRenderer> ().material.color = Color.red;
			}
			else
			{
				sAI.mAIRT.GetComponent<MeshRenderer> ().material.color = Color.blue;
			}
		}
	}

	public class CowRunner:SimpleAIRunner
	{

		private float ComputeGreen(SimpleAI sAI)
		{
			GameObject g = sAI.mAIRT;
			Color c = g.GetComponent<MeshRenderer> ().material.color;
			return 1.0f - c.r;
		}

		public void DoRun(SimpleAI sAI,string code)
		{
			string s = code;

			((CowObject)sAI.mCharacter).timer += Time.deltaTime;
			if (((int)((CowObject)sAI.mCharacter).timer) - ((CowObject)sAI.mCharacter).lastTime > 0) 
			{
				((CowObject)sAI.mCharacter).mTempLife -= 30 + (int)(10*((CowObject)sAI.mCharacter).mStrongRate);
				((CowObject)sAI.mCharacter).lastTime = ((int)((CowObject)sAI.mCharacter).timer);
			}
			if (code == "Die") 
			{
				sAI.Destroy ();
			}
			else if (code == "Round")
			{
				((CowObject)sAI.mCharacter).mGlassTarget = null;
				if (((CowObject)sAI.mCharacter).RoundTimer > 0.0f && (((CowObject)sAI.mCharacter)).RoundTarget != Vector3.zero)
				{
					float distance = Vector3.Distance(sAI.mAIRT.transform.position,((CowObject)sAI.mCharacter).RoundTarget);
					if (distance > 1.5f) 
					{
						sAI.GetComponent<AIMove> ().mDirection = ((CowObject)sAI.mCharacter).RoundTarget - sAI.mAIRT.transform.position;
						sAI.GetComponent<AIMove> ().mDirection.y = 0.0f;
						sAI.GetComponent<AIMove> ().mVelocity = ((CowObject)sAI.mCharacter).mVelocity;
					}
					else
					{
						sAI.GetComponent<AIMove> ().mVelocity = 0.0f;
					}
					((CowObject)sAI.mCharacter).RoundTimer -= Time.deltaTime;
				}
				else
				{
					((CowObject)sAI.mCharacter).RoundTimer = 4.5f;
					((CowObject)sAI.mCharacter).RoundTarget = sAI.GeneratePos + new Vector3 (Random.Range(-10,10),0,Random.Range(-10,10));
				}
			} 
			else 
			{
				((CowObject)sAI.mCharacter).RoundTarget = Vector3.zero;
				((CowObject)sAI.mCharacter).RoundTimer = 0.0f;
				if (((CowObject)sAI.mCharacter).mGlassTarget == null||((CowObject)sAI.mCharacter).mGlassTarget.mAIRT==null) 
				{
					SimpleAI[] SimpleAIs = SimpleAISetSingleton.getInstance ().GetComponent<SimpleAISet> ().FindAIsWithRiaus (sAI.mAIRT.transform.position,9.5f,1<<LayerMask.NameToLayer("Default"),"Glass");
					int startIndex = Random.Range (0,SimpleAIs.Length);
					bool Type = ((CowObject)sAI.mCharacter).Type;
					for (int i = startIndex; i < SimpleAIs.Length; i++)
					{
						float f2 = ComputeGreen (SimpleAIs[i]);
						if (f2 > 0.5f && f2 < 0.75f)
							f2 += 0.25f;
						else if (f2 < 0.5f && f2 > 0.25f)
							f2 -= 0.25f;
						
						if (Type)
						{
							f2 = f2;
							if (f2 < 0.15f)
								f2 -= 0.11f;
						}
						else 
						{
							f2 = 1.0f - f2;
							if (f2 < 0.15f)
								f2 -= 0.11f;
						}

						if (Random.Range (0.0f, 1.0f) < f2) 
						{
							((CowObject)sAI.mCharacter).mGlassTarget = SimpleAIs [i];
							//Debug.Log (SimpleAIs[i].mAIRT.GetComponent<MeshRenderer>().material.color+"  "+Type.ToString());
							break;
						}
					}
					s+="Finding";
				}
				else
				{
					float distance = Vector3.Distance(sAI.mAIRT.transform.position,((CowObject)sAI.mCharacter).mGlassTarget.mAIRT.transform.position);
					if (distance>1.5f)
					{
						sAI.GetComponent<AIMove> ().mDirection = ((CowObject)sAI.mCharacter).mGlassTarget.mAIRT.transform.position - sAI.mAIRT.transform.position;
						sAI.GetComponent<AIMove> ().mDirection.y = 0.0f;
						sAI.GetComponent<AIMove> ().mVelocity = ((CowObject)sAI.mCharacter).mVelocity;
			
					}
					else
					{
						sAI.GetComponent<AIMove> ().mVelocity = 0.0f;
						if (((CowObject)sAI.mCharacter).EatTimer < 1.0f)
						{
							((CowObject)sAI.mCharacter).EatTimer += Time.deltaTime;
						}
						else 
						{
							((CowObject)sAI.mCharacter).mTempLife += 40;
							((CowObject)sAI.mCharacter).mTempLife = Mathf.Min (((CowObject)sAI.mCharacter).mTempLife,((CowObject)sAI.mCharacter).mLife);
							((glassObject)((CowObject)sAI.mCharacter).mGlassTarget.mCharacter).mHp -= 120;
						}
					}
				}
			}
		}	
	};

	public class CowDestroyer:SimpleAIDestroyer
	{
		public void DoDestroy(SimpleAI sAI)
		{

		}
	}


	public class TreeObject:GAObject
	{
		
		public int mLife;
		public int mTempLife;

		public float mStrongRate;

		public static Vector3 mpos_leftdown;
		public static Vector3 mpos_rightup;

		public float timer = 0.0f;

		public override void RandomData ()
		{
			base.RandomData ();
			mPosition = new Vector3 ();
			mPosition.x = Random.Range (mpos_leftdown.x,mpos_rightup.x);
			mPosition.z = Random.Range (mpos_leftdown.z,mpos_rightup.z);
			mPosition.y = mpos_rightup.y;
			mStrongRate = Random.Range (0.0f,1.0f);

			mLife = Random.Range (1000,2000);
			mTempLife = mLife;

		}

		public override void Encode ()
		{
			base.Encode ();
			bool[] posxbit = FreedomAIEncoderAndDecoder.float2BitBunch (mPosition.x,mpos_leftdown.x,mpos_rightup.x,11);
			bool[] poszbit = FreedomAIEncoderAndDecoder.float2BitBunch (mPosition.z,mpos_leftdown.z,mpos_rightup.z,11);
			bool[] lifebit = FreedomAIEncoderAndDecoder.int2BitBunch (mLife,1000,2000,10);
			bool[] strongbit = FreedomAIEncoderAndDecoder.float2BitBunch (mStrongRate,0.0f,1.0f,11);
			mbitbunch = FreedomAIEncoderAndDecoder.JointBitBunch (posxbit,poszbit,lifebit,strongbit);
		}

		public override void Decode ()
		{
			base.Decode ();
			bool[][] mAllBitBunch = FreedomAIEncoderAndDecoder.BreakBitBunch (mbitbunch,mBreakPoint);
			float posx = FreedomAIEncoderAndDecoder.BitBunch2float (mAllBitBunch[0],mpos_leftdown.x,mpos_rightup.x);
			float posz = FreedomAIEncoderAndDecoder.BitBunch2float (mAllBitBunch[1],mpos_leftdown.z,mpos_rightup.z);
			int life = FreedomAIEncoderAndDecoder.BitBunch2int (mAllBitBunch[2],1000,2000);
			float strong = FreedomAIEncoderAndDecoder.BitBunch2float (mAllBitBunch[3],0.0f,1.0f);
			mPosition.x = posx;
			mPosition.y = mpos_leftdown.y;
			mPosition.z = posz;
			mLife = life;mTempLife = mLife;
			mStrongRate = strong;
		}

		public override void InitBreakPoint ()
		{
			base.InitBreakPoint ();
			mBreakPoint.Add (11);
			mBreakPoint.Add (11);
			mBreakPoint.Add (10);
			mBreakPoint.Add (11);
		}

		public override float Evaluate ()
		{
			return (mTempLife-1000.0f) / 1000.0f;
		}

	}

	public class TreeJudger:SimpleAIStateJudger
	{
		public string DoJudge(SimpleAI pSimpleAI)
		{
			if(((TreeObject)pSimpleAI.mCharacter).mTempLife<0)
				return "Die";
			else
				return "Idle";
		}
	}


	public class TreeRunner:SimpleAIRunner
	{
		public void DoRun(SimpleAI sAI,string code)
		{
			if (code == "Die")
			{
				sAI.Destroy ();
			}

			TreeObject to = (TreeObject)sAI.mCharacter;
			if (to.timer < 1.0f) 
			{
				to.timer += Time.deltaTime;
				return;
			}
			SimpleAI[] SimpleAIs = SimpleAISetSingleton.getInstance ().GetComponent<SimpleAISet> ().FindAIsWithRiaus (sAI.mAIRT.transform.position,5.0f,1<<LayerMask.NameToLayer("Default"),"Glass");
			to.timer = 0.0f;
			to.mTempLife -= 180+(int)(30*to.mStrongRate);
			for (int i = 0; i < SimpleAIs.Length; i++)
			{
				glassObject go = (glassObject)SimpleAIs [i].mCharacter;
				if (go.mHp > 500) 
				{
					to.mTempLife += (int)(go.mHp / 500.0f);
					//Debug.Log ("Grass");
				}
			}
		}
	}

	public class TreeRePairer:SimpleAIRePairer
	{
		public void DoRePair(SimpleAI sAI)
		{
			TreeObject to = (TreeObject)sAI.mCharacter;
			sAI.mAIRT.transform.localScale = sAI.mAIRT.transform.localScale * (to.mStrongRate+1.0f);
		}
	}

};
