using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FreedomAI;
using UnityECS;

public class TestAILODAndAIPoor : MonoBehaviour 
{

	UEntity AIPoorEntity;

	public GameObject AITemplate;

	public int RowCount;

	public int ColumnCount;

	public float allTime;

	public Color Color1;

	public Color Color2;

	private bool isStart = false;

	private float timer = 0.0f;

	private bool isUsingPoor = false;

	public GameObject LeftDown;

	public GameObject RightUp;

	private GameObject[] allGameObjects;

	private AIEntity[] allEntitys;

	public void StartWork()
	{
		isStart = true;
		timer = 0.0f;
	}

	public void usePoor()
	{
		isUsingPoor = true;
	}

	public void nousePoor()
	{
		isUsingPoor = false;
	}


	void Start ()
	{
		
		AIPoorEntity = new UEntity ();

		ECSWorld.MainWorld.registerEntityAfterInit (AIPoorEntity);

		AIPoorComponent tAIPoorComponent = new AIPoorComponent ();

		tAIPoorComponent = new AIPoorComponent ();

		tAIPoorComponent.mAI_Template = AITemplate;

		tAIPoorComponent.Init ();

		AIPoorEntity.AddComponent<AIPoorComponent> (tAIPoorComponent);

		allGameObjects = new GameObject[RowCount*ColumnCount];

		allEntitys = new AIEntity[RowCount * ColumnCount];

	}
	

	void Update () 
	{
		if (!isStart)
			return;
		timer += Time.deltaTime;
		if (isUsingPoor) 
		{
			int index = 0;
			for (int i = 0; i < ColumnCount; i++)
			{
				for (int j = 0; j < RowCount; j++)
				{
					if (allGameObjects [index] != null)
						GameObject.Destroy (allGameObjects[index]);
					index++;
				}
			}

			index = 0;

			for (int i = 0; i < ColumnCount; i++)
			{
				for (int j = 0; j < RowCount; j++)
				{
					float x1 = (RightUp.transform.position.x - LeftDown.transform.position.x) * ((float)i) / (float)ColumnCount+LeftDown.transform.position.x;
					float z1 = (RightUp.transform.position.z - LeftDown.transform.position.z) * ((float)j) / (float)RowCount+LeftDown.transform.position.z;
					Vector3 v1 = new Vector3 (x1,RightUp.transform.position.y,z1);
					allGameObjects [index] = GameObject.Instantiate (AITemplate,v1,Quaternion.identity) as GameObject;
					allGameObjects [index].GetComponent<MeshRenderer> ().material.color = Color.Lerp (Color1,Color2,timer/allTime);
					index++;
				}
			}

		}
		else
		{
			int index = 0;
			for (int i = 0; i < ColumnCount; i++)
			{
				for (int j = 0; j < RowCount; j++)
				{
					if (allEntitys [index] != null) 
					{
						AIPoorEntity.GetComponent<AIPoorComponent> ().DestroyEntity (allEntitys[index]);
					}
					index++;
				}
			}

			index = 0;

			for (int i = 0; i < ColumnCount; i++)
			{
				for (int j = 0; j < RowCount; j++)
				{
					float x1 = (RightUp.transform.position.x - LeftDown.transform.position.x) * ((float)i) / (float)ColumnCount+LeftDown.transform.position.x;
					float z1 = (RightUp.transform.position.z - LeftDown.transform.position.z) * ((float)j) / (float)RowCount+LeftDown.transform.position.z;
					Vector3 v1 = new Vector3 (x1,RightUp.transform.position.y,z1);
					allEntitys [index] = AIPoorEntity.GetComponent<AIPoorComponent> ().InstantiateEntity ();
					allEntitys [index].GetComponent<BaseAIComponent> ().mAIRT.transform.position = v1;
					allEntitys [index].GetComponent<BaseAIComponent> ().mAIRT.GetComponent<MeshRenderer> ().material.color = Color.Lerp (Color1,Color2,timer/allTime);
					index++;
				}
			}

		}
	}

}
