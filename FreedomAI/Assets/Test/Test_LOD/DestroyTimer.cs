using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyTimer : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Invoke ("DestroyEmmm",4.0f);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void DestroyEmmm()
	{
		Destroy (this.gameObject);
	}
}
