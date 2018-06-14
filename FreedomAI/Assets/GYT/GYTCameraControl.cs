using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GYTCameraControl : MonoBehaviour
{
	public GameObject player;
	public float dis = 5.0f;
	public float height = 4.0f;
	// 平滑速度，保证大于等于当前物体移动速度
	public float dampTrace = 2.0f;
	// 对于玩家朝向的偏移
	public float forwardTrace = 2.0f;
	private Transform tf;
	private Transform ptf;
	// Use this for initialization
	void Start ()
	{
		tf = GetComponent<Transform> ();
		ptf = player.GetComponent<Transform> ();
//		tf.LookAt (ptf.position);
	}
	
	// Update is called once per frame
	void LateUpdate ()
	{
		// 保证相机平滑速度与玩家移动速度一致
		dampTrace = player.GetComponent<GYTPlayerControl> ().GetVel ()+1.0f;
		// 玩家当前位置减去一个世界的前方向，在抬高一个世界的上方向，在根据比例向玩家前方进行偏移
		tf.position = Vector3.Lerp (tf.position, ptf.position -
		Vector3.forward * dis + (Vector3.up * height) + ptf.forward * forwardTrace, 
			Time.deltaTime * dampTrace);
	}
}
