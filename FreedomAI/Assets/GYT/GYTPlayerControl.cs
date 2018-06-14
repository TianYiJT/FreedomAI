using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GYTPlayerControl : MonoBehaviour
{
	private Transform tf;
	public float sneakVel = 1.0f;
	public float walkVel = 2.0f;
	public float runVel = 4.0f;
	public float roateVel = 2.0f;
	public float HP = 100.0f;
	public float power = 10.0f;
	public float MaxHP = 100.0f;

	public float sneakRange = 2.0f;// 实际的碰撞体半径为1比较合适
	public float walkRange = 5.0f;
	public float runRange = 10.0f;
	public float shootRange = 16.0f;

	public float trapTime = 0.0f;// 由于踩到陷阱不能活动的时间

	private float h = 0.0f;
	private float v = 0.0f;

	private string[] animBool = { "Run", "Shoot", "Sneak", "Idle", "Walk", "SneakIdle" };
	private Camera mCamera;
	private Ray ray;
	private RaycastHit hit;

	private bool walkState;
	private bool runState;
	private bool sneakState;
	private bool idleState;
	private bool shootState;

	private Animator anim;
	private bool move;
	private float range;
	public GameObject FirePosL;
	public GameObject FirePosR;
	public GYTNoiseController noise;
	private GytFirePos FL;
	private GytFirePos FR;
	// 鼠标没有操作几秒后，将朝向指向位移方向
	public float revertTime = 2.0f;
	// 实际使用的恢复时间
	private float rT;
	// Use this for initialization
	void Start ()
	{
		tf = GetComponent<Transform> ();
		mCamera = GameObject.Find ("MainCamera").GetComponent<Camera> ();
		anim = GetComponent<Animator> ();
		walkState = true;
		runState = false;
		sneakState = false;
		move = false;
		rT = revertTime;
		FL = FirePosL.GetComponent<GytFirePos> ();
		FR = FirePosR.GetComponent<GytFirePos> ();

	}
	// Update is called once per frame
	void Update ()
	{
		
		Movement ();
		changAnim ();
	}

	//每帧update中调用
	public float GetVel ()
	{
		if (runState) {
			return runVel;
		} else if (sneakState) {
			return sneakVel;
		}
		return walkVel;
	}

	private void changAnim ()
	{
		string state;
		if (shootState) {
			state = "Shoot";
		} else if (!move) {
			if (sneakState) {
				state = "SneakIdle";
			} else {
				state = "Idle";
			}
		} else if (sneakState) {
			state = "Sneak";
		} else if (runState) {
			state = "Run";
		} else {
			state = "Walk";
		}


		foreach (string s in animBool) {
			if (s.Equals (state)) {
				anim.SetBool (s, true);
			} else {
				anim.SetBool (s, false);
			}

		}
	}

	void Movement ()
	{
		
		// 跑步切换
		if (Input.GetKeyDown (KeyCode.LeftShift)) {
			runState = !runState;
			sneakState = false;
		}
		// 下蹲切换
		if (Input.GetKeyDown (KeyCode.LeftControl)) {
			sneakState = !sneakState;
			runState = false;
		}
		// 
		// 获取鼠标位置
		if (Input.GetMouseButton (0)||Input.GetMouseButton (1)) {
			runState = false;

			//Debug.Log ("mouse");
			ray = mCamera.ScreenPointToRay (Input.mousePosition);
			ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			Physics.Raycast (ray, out hit); 
			rT = revertTime;
		} else {
			rT -= Time.deltaTime;
		}
		// 获取位移输入
		h = Input.GetAxisRaw ("Horizontal");
		v = Input.GetAxisRaw ("Vertical");
		Vector3 dir = (Vector3.forward * v) + (Vector3.right * h);
		// 确定面朝方向
		if (rT > 0.0f&&runState == false) {
			// 保证forward的y轴为0
			Vector3 nforward = new Vector3 (hit.point.x - tf.position.x, 0, hit.point.z - tf.position.z);
			tf.forward = Vector3.Lerp (tf.forward, nforward, Time.deltaTime * roateVel);
		} else if (Input.GetAxisRaw ("Horizontal") != 0 || Input.GetAxisRaw ("Vertical") != 0) {
			// 当鼠标长时间没有点击且...键盘有输入
			tf.forward = Vector3.Lerp (tf.forward, dir.normalized, Time.deltaTime * roateVel);
		}

		// 射击时,或者被陷阱困住时不可移动
		trapTime -= Time.deltaTime;
		if (Input.GetMouseButton (0)) {
			shootState = true;
			FL.shoot = true;
			FR.shoot = true;
			SoundTrans ();
		} else {
			shootState = false;
			FL.shoot = false;
			FR.shoot = false;
			
			// 移动的时候(安键值超过阈值且此时没有被困住)判断周围是否有敌人，传入“声音”
			if (Vector3.Magnitude (dir) > 0.1f&&trapTime<0.0f) {
				// 位移操作
				tf.Translate (dir.normalized * GetVel () * Time.deltaTime, Space.World);
				SoundTrans ();
				move = true;
			} else {
				move = false;
			}
		}

		//	


		
	}
	// 声音警告
	void SoundTrans ()
	{
		if (runState) {
			range = runRange;
		} else if (sneakState) {
			range = sneakRange;
		} else if (shootState) {
			range = shootRange;
		} else {
			range = walkRange;
		}
		noise.range = this.range;
		noise.noise = true;
	}


	void OnCollisionEnter (Collision collision)
	{
		if (collision.collider.tag == "Bullet") {
			float damage = collision.gameObject.GetComponent<GYTBullet> ().damage;
			HP -= damage;
			Destroy (collision.gameObject);
		}
	}

	void OnDrawGizmos ()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere (transform.position, range);
		Gizmos.DrawLine (transform.position+Vector3.up*0.5f,transform.position+Vector3.up*0.5f+Vector3.forward*5.0f);
	}

	void Death ()
	{
		if (HP < 0.0f) {
			
		}
			
	}
}
