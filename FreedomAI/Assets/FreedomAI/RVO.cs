using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityECS;
using FreedomAI;

namespace FreedomAI
{
	public struct RVOObject
	{
		public Vector3 mpos;
		public Vector3 mvelocity;
	};

	struct RVO_BA
	{
		public Vector2 trans;
		public Vector2 leftBound;
		public Vector2 rightBound;
		public float dist;
		public float avoidRadius;
	}

	// AI move system
	public class AIMoveSystem:USystem
	{
		public string collisionTag = "Collision";
		public string AITag = "AI";
		public float maxDis = 3.0f;
		public float RoboRadius = 1.0f;
		public float radiusBuffer = 0.1f;

		private float twiceRadius;

		public override void Init ()
		{
			base.Init ();
			this.AddRequestComponent (typeof(AIMove));
			twiceRadius = 2 * (RoboRadius + radiusBuffer);
			power = 80;
		}

		public override void Update (UEntity uEntity)
		{
			base.Update (uEntity);


			//Debug.Log (uEntity.GetComponent<AIMove> ().mVelocity+"  "+uEntity.GetComponent<AIMove> ().mDirection+s);

			if (uEntity.GetComponent<AIMove> ().mVelocity==0.0f&&uEntity.GetComponent<AIMove>().mMoveFunc!=MoveFunc.JUSTROTATE)
			{
				return;
			}
			// RVO 
			if (uEntity.GetComponent<AIMove> ().mMoveFunc == MoveFunc.RVO)
			{
				GameObject tg = getGameObjectFromEntity (uEntity);
				RVOObject[] RVO_AI = FindAIEntity (tg.transform.position, maxDis,tg);
				RVOObject[] RVO_Obestacle = FindObestacle (tg.transform.position, maxDis);
				Vector2 VA = uEntity.GetComponent<AIMove> ().mDirection * uEntity.GetComponent<AIMove> ().mVelocity;
				Vector2 PA = tg.transform.position;
				ArrayList RVO_All = new ArrayList ();
				for (int j = 0; j < RVO_AI.Length; j++) 
				{
					Vector2 VB = RVO_AI [j].mvelocity;
					Vector2 PB = RVO_AI [j].mpos;
					RVO_BA BA = new RVO_BA ();
					BA.avoidRadius = twiceRadius;
					BA.trans.Set (PA.x + 0.5f * (VA.x + VB.x), PA.y + 0.5f * (VA.y + VB.y));
					BA.trans.Set (PA.x + 0.5f * (VA.x + VB.x), PA.y + 0.5f * (VA.y + VB.y));
					BA.dist = Vector2.Distance (PA, PB);
					float theta_BA = Mathf.Atan2 (PB.y - PA.y, PB.x - PA.y);
					if (twiceRadius > BA.dist)
					{
						BA.dist = twiceRadius;
					}
					float thetaBAOrt = Mathf.Asin (twiceRadius / BA.dist);
					float thetaOrtLeft = theta_BA + thetaBAOrt;
					BA.leftBound.Set (Mathf.Cos (thetaOrtLeft), Mathf.Sin (thetaOrtLeft));
					float thetaOrtRight = theta_BA - thetaBAOrt;
					BA.rightBound.Set (Mathf.Cos (thetaOrtRight), Mathf.Sin (thetaOrtRight));
					RVO_All.Add (BA);
				}

				for (int j = 0; j < RVO_Obestacle.Length; j++)
				{
					Vector2 VB = Vector2.zero;
					Vector2 PB = RVO_Obestacle [j].mpos;
					RVO_BA BA = new RVO_BA ();
					BA.trans.Set (PA.x + 0.5f * (VA.x + VB.x), PA.y + 0.5f * (VA.y + VB.y));
					BA.dist = Vector2.Distance (PA, PB);
					float theta_BA = Mathf.Atan2 (PB.y - PA.y, PB.x - PA.x);
					float rad = 0.5f * 1.5f;
					BA.avoidRadius = rad + RoboRadius;
					if (BA.avoidRadius > BA.dist)
					{
						BA.dist = BA.avoidRadius;
					}
					float thetaBAOrt = Mathf.Asin (BA.avoidRadius / BA.dist);
					float thetaOrtLeft = theta_BA + thetaBAOrt;
					BA.leftBound.Set (Mathf.Cos (thetaOrtLeft), Mathf.Sin (thetaOrtLeft));
					float thetaOrtRight = theta_BA - thetaBAOrt;
					BA.rightBound.Set (Mathf.Cos (thetaOrtRight), Mathf.Sin (thetaOrtRight));
					RVO_All.Add (BA);

				}
				Vector2 velocityMY = new Vector2 (uEntity.GetComponent<AIMove> ().mDirection.x,uEntity.GetComponent<AIMove> ().mDirection.z);
				velocityMY *= uEntity.GetComponent<AIMove> ().mVelocity;
				Vector2 velocity = intersect (velocityMY, tg.transform.position, RVO_All);
		
				uEntity.GetComponent<AIMove> ().mDirection = (new Vector3 (velocity.x, 0, velocity.y)).normalized;


				Vector3 mTranslation = (uEntity.GetComponent<AIMove>().mDirection.normalized);
				mTranslation = new Vector3 (mTranslation.x,0,mTranslation.z);
				GameObject tObject;
				if (uEntity.GetComponent<BaseAIComponent> () != null)
					tObject = uEntity.GetComponent<BaseAIComponent> ().mAIRT;
				else
					tObject = ((SimpleAI)uEntity).mAIRT;
				tObject.transform.forward = mTranslation.normalized;
				tObject.transform.position+=uEntity.GetComponent<AIMove>().mVelocity*Time.deltaTime*tObject.transform.forward;
			}
			else if (uEntity.GetComponent<AIMove> ().mMoveFunc == MoveFunc.Complex)
			{
				int layermask = 1 << LayerMask.NameToLayer ("Collision");
				GameObject tObject;
				if (uEntity.GetComponent<BaseAIComponent> () != null)
					tObject = uEntity.GetComponent<BaseAIComponent> ().mAIRT;
				else
					tObject = ((SimpleAI)uEntity).mAIRT;
				Vector4 dir1v4 = Matrix4x4.Rotate(Quaternion.Euler(0,45,0))*(new Vector4(uEntity.GetComponent<AIMove> ().mDirection.x,uEntity.GetComponent<AIMove> ().mDirection.y,uEntity.GetComponent<AIMove> ().mDirection.z,1.0f));
				Vector4 dir2v4 = Matrix4x4.Rotate(Quaternion.Euler(0,-45,0))*(new Vector4(uEntity.GetComponent<AIMove> ().mDirection.x,uEntity.GetComponent<AIMove> ().mDirection.y,uEntity.GetComponent<AIMove> ().mDirection.z,1.0f));
				Vector3 dir1 = new Vector3 (dir1v4.x,dir1v4.y,dir1v4.z);
				Vector3 dir2 = new Vector3 (dir2v4.x,dir2v4.y,dir2v4.z);
				//Vector3.
				RaycastHit hit = new RaycastHit ();
				// if raycast hit it 
				if (Physics.Raycast (tObject.transform.position, uEntity.GetComponent<AIMove> ().mDirection, out hit, 7.0f, layermask)||Physics.Raycast (tObject.transform.position,dir1, out hit, 7.0f, layermask)||Physics.Raycast (tObject.transform.position,dir2, out hit, 7.0f, layermask))
				{

					Vector3 hitNormal = hit.normal;  
					//Debug.Log (hit.transform.position+"  "+hit.normal);
					hitNormal.y = 0.0f; 
					float disToAvoid = hit.distance;  
					if (disToAvoid > 6) 
					{  
						Vector3 mTranslation = uEntity.GetComponent<AIMove> ().mDirection.normalized * uEntity.GetComponent<AIMove> ().mVelocity + hitNormal.normalized * uEntity.GetComponent<AIMove> ().mVelocity * 0.2f;
						mTranslation *= Time.deltaTime;
						tObject.transform.forward = mTranslation.normalized;
						tObject.transform.position+=mTranslation.magnitude*tObject.transform.forward;
						//	Debug.Log ("5-7");

					}  
					else if (disToAvoid > 3f) 
					{  
						
						Vector3 pullForce = Vector3.Cross (hitNormal, Vector3.up).normalized;  
						float PdotD = Vector3.Dot (uEntity.GetComponent<AIMove> ().mDirection.normalized, pullForce.normalized);
						if (Mathf.Abs (PdotD) < 0.05f)
							PdotD = 0.0f;
						if (PdotD> 0)
						{
							Vector3 mTranslation = uEntity.GetComponent<AIMove> ().mDirection.normalized * uEntity.GetComponent<AIMove> ().mVelocity;
							mTranslation += pullForce * uEntity.GetComponent<AIMove> ().mVelocity * 0.3f;
							mTranslation *= Time.deltaTime;
							mTranslation = new Vector3 (mTranslation.x,0,mTranslation.z);
							//	tObject.transform.forward = new Vector3(mTranslation.normalized.x,0,mTranslation.normalized.z);
							tObject.transform.forward = mTranslation.normalized;
							tObject.transform.position+=mTranslation.magnitude*tObject.transform.forward;
							//Debug.Log ("pull");
						} 
						else  
						{
							Vector3 mTranslation = uEntity.GetComponent<AIMove> ().mDirection.normalized;
							mTranslation += -pullForce * 0.3f;
							mTranslation = new Vector3 (mTranslation.x,0,mTranslation.z);
							//	tObject.transform.forward = new Vector3(mTranslation.normalized.x,0,mTranslation.normalized.z);
							tObject.transform.forward = mTranslation.normalized;
							tObject.transform.position+=uEntity.GetComponent<AIMove> ().mVelocity*tObject.transform.forward*Time.deltaTime;
						
						}

					}  
					else
					{ 
						Vector3 paraSpeed = Vector3.Cross (hitNormal, Vector3.up).normalized; 
						float PdotD = Vector3.Dot (uEntity.GetComponent<AIMove> ().mDirection.normalized, paraSpeed);
						if (Mathf.Abs (PdotD) < 0.05f)
							PdotD = 0.0f;
						Vector3 mTranslation = paraSpeed;
						mTranslation += uEntity.GetComponent<AIMove> ().mDirection;
						mTranslation = new Vector3 (mTranslation.x, 0, mTranslation.z);
						tObject.transform.forward = mTranslation.normalized;
						tObject.transform.position += uEntity.GetComponent<AIMove> ().mVelocity * tObject.transform.forward*Time.deltaTime;
					

					}  
				} 
				else
				{
					Vector3 mTranslation = (uEntity.GetComponent<AIMove>().mDirection.normalized);
					mTranslation = new Vector3 (mTranslation.x,0,mTranslation.z);
					tObject.transform.forward = mTranslation.normalized;
					tObject.transform.position+=tObject.transform.forward*uEntity.GetComponent<AIMove>().mVelocity*Time.deltaTime;
				
				}
			}
			else
			{
				Vector3 mTranslation = (uEntity.GetComponent<AIMove>().mDirection.normalized);
				mTranslation = new Vector3 (mTranslation.x,0,mTranslation.z);
				GameObject tObject;
				if (uEntity.GetComponent<BaseAIComponent> () != null)
					tObject = uEntity.GetComponent<BaseAIComponent> ().mAIRT;
				else
					tObject = ((SimpleAI)uEntity).mAIRT;
				tObject.transform.forward = mTranslation.normalized;
				tObject.transform.position+=tObject.transform.forward*uEntity.GetComponent<AIMove>().mVelocity*Time.deltaTime;
			}
		}

		private GameObject getGameObjectFromEntity(UEntity pEntity)
		{
			BaseAIComponent bAI = pEntity.GetComponent<BaseAIComponent> ();
			if (bAI != null)
				return bAI.mAIRT;
			else
				return ((SimpleAI)pEntity).mAIRT;
		}


		private RVOObject[] FindObestacle(Vector3 center,float maxdis)
		{
			Collider[] tCollider = Physics.OverlapSphere (center,maxdis);
			//Debug.Log (tCollider.Length);
			List<RVOObject> tListRVOObject = new List<RVOObject> ();
			for (int i = 0; i < tCollider.Length; i++)
			{
				if (tCollider [i].tag == collisionTag) 
				{
					RVOObject tRVOObject = new RVOObject ();
					tRVOObject.mpos = tCollider [i].transform.position;
					tRVOObject.mvelocity = Vector3.zero;
					tListRVOObject.Add (tRVOObject);
				}
			}
			return tListRVOObject.ToArray ();
		}

		private RVOObject[] FindAIEntity(Vector3 center,float maxdis,GameObject pg)
		{
			Collider[] tCollider = Physics.OverlapSphere (center,maxdis);
			List<RVOObject> tListRVOObject = new List<RVOObject> ();
			for (int i = 0; i < tCollider.Length; i++) 
			{
				float dis1 = Vector3.Distance (pg.transform.position,tCollider[i].gameObject.transform.position);
			//	Debug.Log (pg.transform.position.ToString());
				if (tCollider [i].tag == AITag&&dis1>1.0f) 
				{
					
					bool isFind = false;
					for(int j=0;j<this.mWorld.mEntityCount;j++)
					{
						if (isFind)
							break;
						BaseAIComponent bAI=mWorld.mUEntity [j].GetComponent<BaseAIComponent> ();
						if (bAI != null)
						{
							float dis = Vector3.Distance (bAI.mAIRT.transform.position,tCollider[i].gameObject.transform.position);
							if (dis<0.5f) 
							{
								//Debug.Log (dis1+"  "+dis);
								RVOObject tRVOObject = new RVOObject ();
								tRVOObject.mpos = bAI.mAIRT.transform.position;
								tRVOObject.mvelocity = mWorld.mUEntity [j].GetComponent<AIMove> ().mDirection * mWorld.mUEntity [j].GetComponent<AIMove> ().mVelocity;
								tListRVOObject.Add (tRVOObject);
								isFind = true;
							}
						}
						else
						{
							AIMove aim = mWorld.mUEntity [j].GetComponent<AIMove> ();
							if (aim != null)
							{
								GameObject g = ((SimpleAI)(mWorld.mUEntity [j])).mAIRT;
								float dis = Vector3.Distance (g.transform.position,tCollider[i].gameObject.transform.position);
								if (dis < 0.5f)
								{
									RVOObject tRVOObject = new RVOObject ();
									tRVOObject.mpos = g.transform.position;
									tRVOObject.mvelocity = aim.mDirection * aim.mVelocity;
									tListRVOObject.Add (tRVOObject);
									isFind = true;
								}
							}
						}
					}
				}
			}
			return tListRVOObject.ToArray ();
		}

		private Vector2 intersect(Vector2 velocity,Vector2 position, ArrayList RVO_All)
		{
			Vector2 VAPost = Vector2.zero;
			Vector2 newVel = Vector2.zero;
			bool suit = true;
			float normVelo = velocity.magnitude;
			ArrayList suitableVelo = new ArrayList();
			ArrayList unSuitableVelo = new ArrayList();
			float PI2 = Mathf.PI*2;
			for(float theta = 0f; theta < PI2; theta += 0.1f)
			{
				float velStep = normVelo / 10.0f;
				for(float rad = 0.02f;rad< normVelo + 0.02f; rad += velStep)
				{
					newVel.Set(rad*Mathf.Cos(theta),rad * Mathf.Sin(theta));
					suit = true;
					foreach(RVO_BA BA in RVO_All)
					{
						Vector2 dif=Vector2.zero;
						dif.Set(newVel.x + position.x - BA.trans.x, newVel.y + position.y -BA.trans.y);
						float theta_diff = Mathf.Atan2(dif.y, dif.x);
						float theta_right = Mathf.Atan2(BA.rightBound.y, BA.rightBound.x);
						float theta_left = Mathf.Atan2(BA.leftBound.y, BA.leftBound.x);
						if (inBetween(theta_right,theta_diff,theta_left))
						{
							suit = false;
							break;
						}

					}
					if (suit)
					{
						suitableVelo.Add(newVel);
					}
					else
					{
						unSuitableVelo.Add(newVel);
					}
				}
			}
			newVel = velocity;
			suit = true;
			foreach (RVO_BA BA in RVO_All)
			{
				Vector2 dif = Vector2.zero;
				dif.Set(newVel.x + position.x - BA.trans.x, newVel.y + position.y - BA.trans.y);
				float theta_diff = Mathf.Atan2(dif.y, dif.x);
				float theta_right = Mathf.Atan2(BA.rightBound.y, BA.rightBound.x);
				float theta_left = Mathf.Atan2(BA.leftBound.y, BA.leftBound.x);
				if (inBetween(theta_right, theta_diff, theta_left))
				{
					suit = false;
					break;
				}

			}
			if (suit)
			{

				suitableVelo.Add(newVel);
			}
			else
			{
				unSuitableVelo.Add(newVel);
			}

			if(suitableVelo.Count > 0)
			{
				//Debug.Log(myRobo.name + " has " + suitableVelo.Count + " suitable velo");
				VAPost = min(suitableVelo, velocity);

			}
			else
			{
				IDictionary<Vector2,float> tc_V = new Dictionary<Vector2, float>();
				foreach(Vector2 unsuitV in unSuitableVelo)
				{
					tc_V[unsuitV] = 0;
					ArrayList tc = new ArrayList();

					foreach (RVO_BA BA in RVO_All)
					{
						Vector2 dif = Vector2.zero;
						float rad = BA.avoidRadius;
						dif.Set(unsuitV.x + position.x - BA.trans.x, unsuitV.y + position.y - BA.trans.y);
						float theta_dif = Mathf.Atan2(dif.y, dif.x);
						float theta_right = Mathf.Atan2(BA.rightBound.y, BA.rightBound.x);
						float theta_left= Mathf.Atan2(BA.leftBound.y, BA.leftBound.x);
						if (inBetween(theta_right, theta_dif, theta_left))
						{
							float small_theta = Mathf.Abs(theta_dif -0.5f*(theta_left+ theta_right));
							float temp = Mathf.Abs(BA.dist * Mathf.Sin(small_theta));
							if (temp >= rad)
							{
								rad = temp;
							}
							float big_theta = Mathf.Asin(Mathf.Abs(BA.dist * Mathf.Sin(small_theta)) / rad);
							float dist_tg = Mathf.Abs(BA.dist * Mathf.Cos(small_theta)) - Mathf.Abs(rad * Mathf.Cos(big_theta));
							if(dist_tg < 0)
							{
								dist_tg = 0;
							}
							float tc_v = dist_tg / dif.magnitude;
							tc.Add(tc_v);
						}

					}
					tc_V[unsuitV] = min(tc) + 0.001f;
				}
				float WT = 0.2f;
				VAPost = (Vector2)unSuitableVelo[0];
				float lastKey = 0f;
				foreach (Vector2 v in unSuitableVelo)
				{

					Vector2 temp = v - velocity;
					float key = ((WT / tc_V[v])) + temp.magnitude;
					if (!VAPost.Equals(v))
					{
						if (key< lastKey)
						{
							lastKey = key;
							VAPost = v;
						}
					}else
					{
						lastKey = key;
						VAPost = v;
					}
				}
			}
			return VAPost;
		}

		private bool inBetween(float thetaRight, float thetaDif,float thetaLeft)
		{
			if(Mathf.Abs(thetaRight- thetaLeft)<= Mathf.PI)
			{
				if(thetaRight<=thetaDif && thetaDif<= thetaLeft)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				if(thetaLeft <0 && thetaRight> 0)
				{
					thetaLeft += 2 * Mathf.PI;
					if (thetaDif < 0)
					{
						thetaDif += 2 * Mathf.PI;
					}
					if (thetaRight <= thetaDif && thetaDif <= thetaLeft)
					{
						return true;
					}
					else
					{
						return false;
					}
				}
				if (thetaLeft > 0 && thetaRight< 0)
				{
					thetaRight += 2 * Mathf.PI;
					if (thetaDif < 0)
					{
						thetaDif += 2 * Mathf.PI;
					}
					if (thetaLeft <= thetaDif && thetaDif <= thetaRight )
					{
						return true;
					}
					else
					{
						return false;
					}
				}
			}
			return true;
		}

		private Vector2 min(ArrayList mylist, Vector2 desVel)
		{
			Vector2 result = -desVel;
			float diff =2*desVel.magnitude;
			foreach(Vector2 vel in mylist)
			{
				Vector2 diffVec = vel - desVel;
				if (diffVec.magnitude < diff)
				{
					result = vel;
					diff = diffVec.magnitude;
				}
			}
			return result;
		}

		private float min(ArrayList mylist)
		{

			float diff = (float)mylist[0];
			foreach (float vel in mylist)
			{

				if (vel < diff)
				{
					diff = vel;

				}
			}
			return diff;
		}

	}
}
