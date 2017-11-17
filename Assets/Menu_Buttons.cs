using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Menu_Buttons : MonoBehaviour {
	public GameObject MenuPanel;
	public GameObject LevelSelectPanel;
	public GameObject LoginPanel;
	public GameObject SignUpPanel;
	Vector3 endPos;

	// Use this for initialization
	void Start () {
		MenuPanel.SetActive(true);
		LevelSelectPanel.SetActive(false);
		LoginPanel.SetActive(false);
		SignUpPanel.SetActive(false);
		/*GameObject buttonA = GameObject.Find("logo");

		Vector3 pos = buttonA.transform.position;
		Vector3 endPos = buttonA.transform.position;
		endPos.y += 300f;
		buttonA.transform.position = Vector3.MoveTowards(pos, endPos, 1f * Time.deltaTime);*/
		//buttonA.transform.position = Vector3.Lerp(pos, endPos, 10 * Time.deltaTime);
		//buttonA.transform.position = pos;

		//float elapsedTime = 0;
		//Vector3 startingPos = buttonA.transform.position;
		GameObject buttonA = GameObject.Find("logo");
		endPos = buttonA.transform.position;
		endPos.y += 300f;
		/*while (elapsedTime < 3f)
		{
			transform.position = Vector3.Lerp(startingPos, endPos, (elapsedTime / 3f));
			elapsedTime += Time.deltaTime;
			//yield return new WaitForEndOfFrame();
		}
		transform.position = end;*/
		Debug.Log (buttonA.transform.position);
		buttonA.transform.position = Vector3.Lerp(buttonA.transform.position ,endPos, 1000f);
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
