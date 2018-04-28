using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;

public class UserData {
	public string username, emoji_start, emoji_end, sentiment_start, sentiment_end;
	public double total_session_time;
	public DateTime date;
	public int points, level;

	public UserData() {
	}

	public UserData(string username, string emoji_start, string emoji_end, string sentiment_start, string sentiment_end, double total_session_time, DateTime date, int points, int level) {
		this.username = username;
		this.emoji_start = emoji_start;
		this.emoji_end = emoji_end;
		this.sentiment_end = sentiment_end;
		this.sentiment_start = sentiment_start;
		this.total_session_time = total_session_time;
		this.date = date;
		this.points = points;
		this.level = level;
	}
}

public class Menu_Buttons : MonoBehaviour {
	public GameObject MenuPanel;
	public GameObject LevelSelectPanel;
	public GameObject LoginPanel;
	public GameObject SignUpPanel;
	public GameObject FinalPanel;
	public GameObject LoadingPanel;
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

	//Used for camera 
	private bool camAvailable;
	private WebCamTexture frontCamTexture;
	public RawImage frontCam;
	public AspectRatioFitter fit;
	public byte[] bytes;
	public bool check;

	// db
	public DatabaseReference mDatabaseRef;
	private FirebaseDatabase mDatabase;

	static public int accessed = 0;    // this is reachable from everywhere

	// Use this for initialization
	void Start () {
		check = false;
		print ("cross scene info: " + SceneController.CrossSceneInformation);

		// set up db
		FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://baloo-bfbb8.firebaseio.com/");
		mDatabase = FirebaseDatabase.DefaultInstance;
		mDatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;
		FirebaseApp.LogLevel = LogLevel.Debug;

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

		// Camera Start
		WebCamDevice[] devices = WebCamTexture.devices;

		if (devices.Length == 0) {
			Debug.Log ("No camera detected");
			camAvailable = false;
			return;
		}

		for (int i = 0; i < devices.Length; i++) {
			if (devices [i].isFrontFacing) {
				Debug.Log ("FRONT CAMERA DETECTED");
				frontCamTexture = new WebCamTexture (devices [i].name, Screen.width, Screen.height);
			}
		}

		if (frontCamTexture == null) {
			Debug.Log ("Unable to find back camera");
			return;
		}
		frontCamTexture.Play ();
		frontCam.texture = frontCamTexture;

		camAvailable = true;

		//scaredSelected = Resources.Load("Assets/scaredSelected") as Sprite;
		//scaredUnselected = Resources.Load("Assets/scaredUnselected") as Sprite;
	}

	// Update is called once per frame
	void Update () {
		/*
		if (!camAvailable)
			return;

		float ratio = (float)frontCamTexture.width / (float)frontCamTexture.height;
		fit.aspectRatio = ratio;

		float scaleY = frontCamTexture.videoVerticallyMirrored ? 1f : -1f;
		frontCam.rectTransform.localScale = new Vector3 (1f, scaleY, 1f);

		int orient = -frontCamTexture.videoRotationAngle;
		frontCam.rectTransform.localEulerAngles = new Vector3 (0, 0, orient);
		*/
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
		UserMetrics.Username = nameUser; // save the username
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

	public void setEmojiMetric(string emoji) {
		if (isExitTime) {
			UserMetrics.Emoji_end = emoji; 
			Debug.Log("EXIT EMOJI: " + UserMetrics.Emoji_end);
		} else {
			UserMetrics.Emoji_start = emoji;
			Debug.Log("START EMOJI: " + UserMetrics.Emoji_start);
		}
	}

	public void selectScared(){
		setEmojiMetric ("scared");
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
		setEmojiMetric ("happy");
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
		setEmojiMetric ("angry");
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
		setEmojiMetric ("sad");
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
		setEmojiMetric ("worried");
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
		setEmojiMetric ("excited");
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

	public static Texture2D rotate(Texture2D t)
	{
		Texture2D newTexture = new Texture2D(t.height, t.width, t.format, false);

		for(int i=0; i<t.width; i++)
		{
			for(int j=0; j<t.height; j++)
			{
				newTexture.SetPixel(j, i, t.GetPixel(t.width-i, j));
			}
		}
		newTexture.Apply();
		return newTexture;
	}

	string fixJson(string value)
	{
		value = "{\"Items\":" + value + "}";
		return value;
	}

	[System.Serializable]
	public class FaceRectangle
	{
		public int height;
		public int left;
		public int top;
		public int width;
	}

	[System.Serializable]
	public class FaceAttributes
	{
		public Scores emotion;
	}

	[System.Serializable]
	public class Scores
	{
		public double anger;
		public double contempt;
		public double disgust;
		public double fear;
		public double happiness;
		public double neutral;
		public double sadness;
		public double surprise;
	}

	[System.Serializable]
	public class Face
	{
		public FaceRectangle faceRectangle;
		public String faceId;
		public FaceAttributes faceAttributes;
	}

	[System.Serializable]
	public class QuestionList
	{
		public List<Face> Items;
	}

	IEnumerator Upload(byte[] bytes){
		string EMOTIONKEY = "5839f361664e4363b4c793a14d23e089";
		string emotionURL = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/detect?returnFaceLandmarks=false&returnFaceAttributes=emotion";

		var headers = new Dictionary<string, string>() {
			{ "Ocp-Apim-Subscription-Key", EMOTIONKEY },
			{ "Content-Type", "application/octet-stream" }
		};

		WWW www = new WWW(emotionURL, bytes, headers);

		yield return www;
		string responseData = www.text;
		responseData = fixJson(responseData);
		Debug.Log (responseData);

		string maxName = "";

		QuestionList listFaces = JsonUtility.FromJson<QuestionList> (responseData);

		if (listFaces.Items.Count != 0) {
			/*
			int anger = Mathf.RoundToInt ((float)listFaces.Items [0].faceAttributes.emotion.anger * 100);
			int contempt = Mathf.RoundToInt ((float)listFaces.Items [0].faceAttributes.emotion.contempt * 100);
			int disgust = Mathf.RoundToInt ((float)listFaces.Items [0].faceAttributes.emotion.disgust * 100);
			int fear = Mathf.RoundToInt ((float)listFaces.Items [0].faceAttributes.emotion.fear * 100);
			int happiness = Mathf.RoundToInt ((float)listFaces.Items [0].faceAttributes.emotion.happiness * 100);
			int neutral = Mathf.RoundToInt ((float)listFaces.Items [0].faceAttributes.emotion.neutral * 100);
			int sadness = Mathf.RoundToInt ((float)listFaces.Items [0].faceAttributes.emotion.sadness * 100);
			int suprise = Mathf.RoundToInt ((float)listFaces.Items [0].faceAttributes.emotion.surprise * 100);
			*/

			double maxValue = 0.0;

			if (maxValue < listFaces.Items [0].faceAttributes.emotion.anger) {
				maxValue = listFaces.Items [0].faceAttributes.emotion.anger;
				maxName = "ANGER";
			}
			if (maxValue < listFaces.Items [0].faceAttributes.emotion.contempt) {
				maxValue = listFaces.Items [0].faceAttributes.emotion.contempt;
				maxName = "CONTEMPT";
			}
			if (maxValue < listFaces.Items [0].faceAttributes.emotion.disgust) {
				maxValue = listFaces.Items [0].faceAttributes.emotion.disgust;
				maxName = "DISGUST";
			}
			if (maxValue < listFaces.Items [0].faceAttributes.emotion.fear) {
				maxValue = listFaces.Items [0].faceAttributes.emotion.fear;
				maxName = "FEAR";
			}
			if (maxValue < listFaces.Items [0].faceAttributes.emotion.happiness) {
				maxValue = listFaces.Items [0].faceAttributes.emotion.happiness;
				maxName = "HAPPY";
			}
			if (maxValue < listFaces.Items [0].faceAttributes.emotion.neutral) {
				maxValue = listFaces.Items [0].faceAttributes.emotion.neutral;
				maxName = "NEUTRAL";
			}
			if (maxValue < listFaces.Items [0].faceAttributes.emotion.sadness) {
				maxValue = listFaces.Items [0].faceAttributes.emotion.sadness;
				maxName = "SAD";
			}
			if (maxValue < listFaces.Items [0].faceAttributes.emotion.surprise) {
				maxValue = listFaces.Items [0].faceAttributes.emotion.surprise;
				maxName = "SURPRISE";
			}
			Debug.Log ("=================");
			Debug.Log (maxName);
			Debug.Log ("=================");
		} else {
			Debug.Log ("=================");
			Debug.Log ("NO FACE DETECTED");
			Debug.Log ("=================");
		}

		if (isExitTime) {
			UserMetrics.Face_emotion_end = maxName;
			Debug.Log ("END FACE EMOTION " + UserMetrics.Face_emotion_end);
		} else {
			UserMetrics.Face_emotion_start = maxName;
			Debug.Log ("START FACE EMOTION " + UserMetrics.Face_emotion_start);
		}

		check = true;
	}

	private void saveMetrics() {

		UserMetrics.Time_in_app = Time.realtimeSinceStartup;
		UserMetrics.Date = System.DateTime.Now;

		UserData metrics = new UserData(UserMetrics.Username, 
			UserMetrics.Emoji_start, 
			UserMetrics.Emoji_end, 
			UserMetrics.Face_emotion_start, 
			UserMetrics.Face_emotion_end, 
			UserMetrics.Time_in_app, 
			UserMetrics.Date,
			UserMetrics.Points,
			UserMetrics.Level);
		string json = JsonUtility.ToJson(metrics);

		Debug.Log ("JSON FOR DB " + json);

		string key = mDatabaseRef.Child("metrics").Push().Key;
		mDatabaseRef.Child (key).SetRawJsonValueAsync (json);
	}

	public void ShowMenuFromSignUpPanel() {

		// show loading
		if (!isExitTime) {
			SignUpPanel.SetActive (false);
			LoadingPanel.SetActive (true);
		}

		//Create a Texture2D with the size of the rendered image on the screen.
		Texture2D texture = new Texture2D(frontCam.texture.width, frontCam.texture.height, TextureFormat.ARGB32, false);

		//Save the image to the Texture2D
		texture.SetPixels(frontCamTexture.GetPixels());
		Debug.Log (texture);
		texture.Apply();

		//Encode it as a PNG.
		bytes = texture.EncodeToPNG();

		Texture2D tex = null;
		tex = new Texture2D(1, 1);
		tex.LoadImage(bytes);

		Texture2D newTexture = rotate (texture);
		bytes = newTexture.EncodeToPNG();

		frontCam.rectTransform.localScale = new Vector3 (1f, -1f, 1f);
		frontCam.texture = texture;

		check = false;
		if (selectedEmoji) {

			frontCamTexture.Stop ();		

			//StartCoroutine (Upload (bytes));
			StartCoroutine(loadARKitSceneAndFaceDetection());

		} else {
			StartCoroutine (Upload (bytes));
		}
	}

	public void ExitApplication() {
		Application.Quit ();
	}

	IEnumerator loadARKitSceneAndFaceDetection()
	{

		yield return StartCoroutine(Upload (bytes));

		if (isExitTime && check) {
			saveMetrics();
			SignUpPanel.SetActive (false);
			FinalPanel.SetActive (true);
		} else if(check) {
			Application.LoadLevel ("UnityARKitScene");
		}
	}
}
