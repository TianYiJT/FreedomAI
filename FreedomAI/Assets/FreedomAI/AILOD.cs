using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityECS;
using FreedomAI;


/*
 * 
 * this Part is about Level of Detail of the AI.
 * you can Find how every LOD of AI to Compute 
 * 
*/
namespace FreedomAI
{
	public class LODComponent:UComponent
	{
		// Temp AI LOD key
		public int mLOD;
		// the max value of LOD
		public static int maxLOD;
		// LOD of AI be Computed or not 
		public bool isUse = true;
	};

	public class LODUpdateSystem:USystem
	{
		public static float[] lodDistance;

		public static GameObject mPlayer;

		/*
		 *  Bind the LODComponent to LODUpdateSystem
		*/
		public override void Init ()
		{
			base.Init ();
			this.AddRequestComponent (typeof(LODComponent));
		}

		/*
		 * @param uEntity: Entity having LODComponent in temp World 
		 * Compute LOD for this AI
		*/

		public override void Update (UEntity uEntity)
		{
			base.Update (uEntity);
			if (lodDistance.Length == 0)
				return;
			if (mPlayer == null)
				return;
			if (!uEntity.GetComponent<LODComponent> ().isUse)
			{
				uEntity.GetComponent<LODComponent> ().mLOD = 1;
				return;
			}
			GameObject tObject = getObjectByEntity (uEntity);
			// Compute by Distance with Player(Camera)
			float dis = Vector3.Distance (mPlayer.transform.position,tObject.transform.position);
			for (int i = 0; i < lodDistance.Length; i++)
			{
				if (dis < lodDistance [i]) 
				{
					uEntity.GetComponent<LODComponent> ().mLOD = i + 1;
					return;
				}
			}
			uEntity.GetComponent<LODComponent> ().mLOD = lodDistance.Length + 1;
		}

		/*
		 * @param uEntity: Entity 
		 * 
		 * @return: the AI GameObject with this Entity  
		 * 
		*/

		private GameObject getObjectByEntity(UEntity uEntity)
		{
			if (uEntity.GetComponent<BaseAIComponent> ()!=null)
				return uEntity.GetComponent<BaseAIComponent> ().mAIRT;
			else
				return ((SimpleAI)uEntity).mAIRT;
		}

	}

};

public class AILOD:MonoBehaviour
{
	public float[] LODDistance;

	/*
	 * 
	 * initlize the LOD in Runtime 
	 * 
	*/

	void Start()
	{
		LODUpdateSystem.lodDistance = LODDistance;
		LODComponent.maxLOD = LODDistance.Length+1;
		LODUpdateSystem.mPlayer = GameObject.FindGameObjectWithTag ("Player");
	}
}


