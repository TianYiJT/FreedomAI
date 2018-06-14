using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordTest : MonoBehaviour {

	public GameObject SB;// sword
	public GameObject SF;
	public GameObject DB;// shield
	public GameObject DF;
	public bool bSF;
	public bool bDF;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (bSF) {
			SF.SetActive (true);
			SB.SetActive (false);
		} else {
			SF.SetActive (false);
			SB.SetActive (true);
		}
		if (bDF) {
			DF.SetActive (true);
			DB.SetActive (false);
		} else {
			DF.SetActive (false);
			DB.SetActive (true);
		}

	}
}
