using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class questions : MonoBehaviour {
	public GameObject smiley1;
	public GameObject smiley2;
	public GameObject smiley3;
	public GameObject smiley4;
	public GameObject smiley5;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void goBackToAR()
	{
		Application.LoadLevel ("UnityARKitScene");
	}
}
