using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Menu_Buttons : MonoBehaviour {
	public GameObject MenuPanel;
	public GameObject LevelSelectPanel;
	public GameObject LoginPanel;
	public GameObject SignUpPanel;

	// Use this for initialization
	void Start () {
		MenuPanel.SetActive(true);
		LevelSelectPanel.SetActive(false);
		LoginPanel.SetActive(false);
		SignUpPanel.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ShowLevelPanel()
	{
		MenuPanel.SetActive(false);
		LevelSelectPanel.SetActive(true);
	}

	public void ShowMenuPanel()
	{
		MenuPanel.SetActive(true);
		LevelSelectPanel.SetActive(false);
	}

	public void ShowLoginPanel()
	{
		
		MenuPanel.SetActive(false);
		LoginPanel.SetActive(true);
	}

	public void ShowSignUpPanel()
	{
		MenuPanel.SetActive(false);
		SignUpPanel.SetActive(true);
	}

	public void ShowMenuFromLoginPanel()
	{
		Application.LoadLevel ("UnityARKitScene");
		//MenuPanel.SetActive(true);
		//LoginPanel.SetActive(false);
	}

	public void ShowMenuFromSignUpPanel()
	{
		Application.LoadLevel ("UnityARKitScene");
		//MenuPanel.SetActive(true);
		//SignUpPanel.SetActive(false);
	}

}
