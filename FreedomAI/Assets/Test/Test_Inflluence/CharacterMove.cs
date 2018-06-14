using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMove : MonoBehaviour
{

	CharacterController m_CharacterControl;

	public float m_Velocity;

	void Start ()
	{
		m_CharacterControl = GetComponent<CharacterController> ();
	}
	

	void Update () 
	{
		if (Input.GetKey (KeyCode.A))
			m_CharacterControl.Move (new Vector3(0,0,m_Velocity*Time.deltaTime));
		if (Input.GetKey (KeyCode.D))
			m_CharacterControl.Move (new Vector3(0,0,-m_Velocity*Time.deltaTime));
		if (Input.GetKey (KeyCode.S))
			m_CharacterControl.Move (new Vector3(-m_Velocity*Time.deltaTime,0,0));
		if (Input.GetKey (KeyCode.W))
			m_CharacterControl.Move (new Vector3(m_Velocity*Time.deltaTime,0,0));
	}

}
