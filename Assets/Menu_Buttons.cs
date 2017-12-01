using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Menu_Buttons : MonoBehaviour {
	public GameObject MenuPanel;
	public GameObject LevelSelectPanel;
	public GameObject LoginPanel;
	public GameObject SignUpPanel;
	public InputField nameField;
	public Text howFeelingText;
	Vector3 endPos;

	public Sprite scaredSelected;
	public Sprite scaredUnselected;
	public Sprite happySelected;
	public Sprite happyUnselected;
	public Sprite angrySelected;
	public Sprite angryUnselected;
	public Sprite sadSelected;
	public Sprite sadUnselected;
	public Sprite worriedSelected;
	public Sprite worriedUnselected;
	public Sprite excitedSelected;
	public Sprite excitedUnselected;
	public Sprite orangeButton;

	private bool selectedEmoji;
	private string nameUser;
	private bool isExitTime = false;

	static public int accessed = 0;    // this is reachable from everywhere

	// Use this for initialization
	void Start () {

		print ("cross scene info: " + SceneController.CrossSceneInformation);

		switch (SceneController.CrossSceneInformation) {
		case "emojis":
			MenuPanel.SetActive (false);
			LevelSelectPanel.SetActive (false);
			LoginPanel.SetActive (false);
			SignUpPanel.SetActive (true);
			isExitTime = true; 
			break;
		default:
			MenuPanel.SetActive (true);
			LevelSelectPanel.SetActive (false);
			LoginPanel.SetActive (false);
			SignUpPanel.SetActive (false);
			break;
		}

		SceneController.CrossSceneInformation = "";

		selectedEmoji = false;
		//scaredSelected = Resources.Load("Assets/scaredSelected") as Sprite;
		//scaredUnselected = Resources.Load("Assets/scaredUnselected") as Sprite;
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
		nameUser = nameField.text;
		Debug.Log (nameUser);
		MenuPanel.SetActive(false);
		SignUpPanel.SetActive(true);
		howFeelingText.text = "Hey " + nameUser +"!\n How are you feeling?";
	}

	public void ShowMenuFromLoginPanel()
	{
		Application.LoadLevel ("UnityARKitScene");
		//MenuPanel.SetActive(true);
		//LoginPanel.SetActive(false);
	}

	public void selectScared(){
		Image scaredImage = GameObject.Find("scaredButton").GetComponent<Image>();
		scaredImage.sprite = scaredSelected;
		Image happyImage = GameObject.Find("happyButton").GetComponent<Image>();
		happyImage.sprite = happyUnselected;
		Image angryImage = GameObject.Find("angryButton").GetComponent<Image>();
		angryImage.sprite = angryUnselected;
		Image sadImage = GameObject.Find("sadButton").GetComponent<Image>();
		sadImage.sprite = sadUnselected;
		Image worriedImage = GameObject.Find("worriedButton").GetComponent<Image>();
		worriedImage.sprite = worriedUnselected;
		Image excitedImage = GameObject.Find("excitedButton").GetComponent<Image>();
		excitedImage.sprite = excitedUnselected;

		Image okButtonImage = GameObject.Find("okButton").GetComponent<Image>();
		okButtonImage.sprite = orangeButton;

		selectedEmoji = true;
	}

	public void selectHappy(){
		Image scaredImage = GameObject.Find("scaredButton").GetComponent<Image>();
		scaredImage.sprite = scaredUnselected;
		Image happyImage = GameObject.Find("happyButton").GetComponent<Image>();
		happyImage.sprite = happySelected;
		Image angryImage = GameObject.Find("angryButton").GetComponent<Image>();
		angryImage.sprite = angryUnselected;
		Image sadImage = GameObject.Find("sadButton").GetComponent<Image>();
		sadImage.sprite = sadUnselected;
		Image worriedImage = GameObject.Find("worriedButton").GetComponent<Image>();
		worriedImage.sprite = worriedUnselected;
		Image excitedImage = GameObject.Find("excitedButton").GetComponent<Image>();
		excitedImage.sprite = excitedUnselected;

		Image okButtonImage = GameObject.Find("okButton").GetComponent<Image>();
		okButtonImage.sprite = orangeButton;

		selectedEmoji = true;
	}

	public void selectAngry(){
		Image scaredImage = GameObject.Find("scaredButton").GetComponent<Image>();
		scaredImage.sprite = scaredUnselected;
		Image happyImage = GameObject.Find("happyButton").GetComponent<Image>();
		happyImage.sprite = happyUnselected;
		Image angryImage = GameObject.Find("angryButton").GetComponent<Image>();
		angryImage.sprite = angrySelected;
		Image sadImage = GameObject.Find("sadButton").GetComponent<Image>();
		sadImage.sprite = sadUnselected;
		Image worriedImage = GameObject.Find("worriedButton").GetComponent<Image>();
		worriedImage.sprite = worriedUnselected;
		Image excitedImage = GameObject.Find("excitedButton").GetComponent<Image>();
		excitedImage.sprite = excitedUnselected;

		Image okButtonImage = GameObject.Find("okButton").GetComponent<Image>();
		okButtonImage.sprite = orangeButton;

		selectedEmoji = true;
	}

	public void selectSad(){
		Image scaredImage = GameObject.Find("scaredButton").GetComponent<Image>();
		scaredImage.sprite = scaredUnselected;
		Image happyImage = GameObject.Find("happyButton").GetComponent<Image>();
		happyImage.sprite = happyUnselected;
		Image angryImage = GameObject.Find("angryButton").GetComponent<Image>();
		angryImage.sprite = angryUnselected;
		Image sadImage = GameObject.Find("sadButton").GetComponent<Image>();
		sadImage.sprite = sadSelected;
		Image worriedImage = GameObject.Find("worriedButton").GetComponent<Image>();
		worriedImage.sprite = worriedUnselected;
		Image excitedImage = GameObject.Find("excitedButton").GetComponent<Image>();
		excitedImage.sprite = excitedUnselected;

		Image okButtonImage = GameObject.Find("okButton").GetComponent<Image>();
		okButtonImage.sprite = orangeButton;

		selectedEmoji = true;
	}

	public void selectWorried(){
		Image scaredImage = GameObject.Find("scaredButton").GetComponent<Image>();
		scaredImage.sprite = scaredUnselected;
		Image happyImage = GameObject.Find("happyButton").GetComponent<Image>();
		happyImage.sprite = happyUnselected;
		Image angryImage = GameObject.Find("angryButton").GetComponent<Image>();
		angryImage.sprite = angryUnselected;
		Image sadImage = GameObject.Find("sadButton").GetComponent<Image>();
		sadImage.sprite = sadUnselected;
		Image worriedImage = GameObject.Find("worriedButton").GetComponent<Image>();
		worriedImage.sprite = worriedSelected;
		Image excitedImage = GameObject.Find("excitedButton").GetComponent<Image>();
		excitedImage.sprite = excitedUnselected;

		Image okButtonImage = GameObject.Find("okButton").GetComponent<Image>();
		okButtonImage.sprite = orangeButton;

		selectedEmoji = true;
	}

	public void selectExcited(){
		Image scaredImage = GameObject.Find("scaredButton").GetComponent<Image>();
		scaredImage.sprite = scaredUnselected;
		Image happyImage = GameObject.Find("happyButton").GetComponent<Image>();
		happyImage.sprite = happyUnselected;
		Image angryImage = GameObject.Find("angryButton").GetComponent<Image>();
		angryImage.sprite = angryUnselected;
		Image sadImage = GameObject.Find("sadButton").GetComponent<Image>();
		sadImage.sprite = sadUnselected;
		Image worriedImage = GameObject.Find("worriedButton").GetComponent<Image>();
		worriedImage.sprite = worriedUnselected;
		Image excitedImage = GameObject.Find("excitedButton").GetComponent<Image>();
		excitedImage.sprite = excitedSelected;

		Image okButtonImage = GameObject.Find("okButton").GetComponent<Image>();
		okButtonImage.sprite = orangeButton;

		selectedEmoji = true;
	}

	public void ShowMenuFromSignUpPanel()
	{
		if (selectedEmoji) {
			if (isExitTime) {
				Application.Quit ();
			} else {
				Application.LoadLevel ("UnityARKitScene");
			}
		}
		//MenuPanel.SetActive(true);
		//SignUpPanel.SetActive(false);
	}

}
