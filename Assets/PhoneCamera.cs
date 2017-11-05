using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using UnityEngine.Networking;


public class PhoneCamera : MonoBehaviour {
	public RawImage background;
	public AspectRatioFitter fit;
	public GameObject sendButton;
	public GameObject cameraButton;
	public GameObject menuButton;
	public GameObject grayPanel;
	public GameObject emotionCard;
	public GameObject redoButton;

	public Text happyText;
	public Text contemptText;
	public Text neutralText;
	public Text supriseText;
	public Text sadnessText;
	public Text fearText;
	public Text disgustText;
	public Text angerText;
	public Image emojiImage;
	public Text captionEmoji;


	public Sprite happySprite;
	public Sprite contemptSprite;
	public Sprite neutralSprite;
	public Sprite surpriseSprite;
	public Sprite sadnessSprite;
	public Sprite fearSprite;
	public Sprite disgustSprite;
	public Sprite angerSprite;

	public byte[] bytes;

	private bool camAvailable;
	private WebCamTexture backCam;
	private Texture defaultBackground;

	private bool stop = false;
	private string responseData;

	[System.Serializable]
	public class FaceRectangle
	{
		public int height;
		public int left;
		public int top;
		public int width;
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
		public Scores scores;
	}

	[System.Serializable]
	public class QuestionList
	{
		public List<Face> Items;
	}


	string fixJson(string value)
	{
		value = "{\"Items\":" + value + "}";
		return value;
	}

	// Use this for initialization
	void Start () {
		grayPanel.SetActive(false);
		emotionCard.SetActive(false);
		sendButton.SetActive(false);
		happyText.enabled = false;
		contemptText.enabled = false;
		neutralText.enabled = false;
		supriseText.enabled = false;
		sadnessText.enabled = false;
		fearText.enabled = false;
		disgustText.enabled = false;
		angerText.enabled = false;
		emojiImage.enabled = false;
		captionEmoji.enabled = false;
		redoButton.SetActive(false);
		defaultBackground = background.texture;
		WebCamDevice[] devices = WebCamTexture.devices;

		if (devices.Length == 0) {
			Debug.Log ("No camera detected");
			camAvailable = false;
			return;
		}

		for (int i = 0; i < devices.Length; i++) {
			if (devices [i].isFrontFacing) {
				backCam = new WebCamTexture (devices [i].name, Screen.width, Screen.height);
			}
		}

		if (backCam == null) {
			Debug.Log ("Unable to find back camera");
			return;
		}
		backCam.Play ();
		background.texture = backCam;

		camAvailable = true;
	}
	
	// Update is called once per frame
	void Update () {
		if (!stop) {
			if (!camAvailable)
				return;

			float ratio = (float)backCam.width / (float)backCam.height;
			fit.aspectRatio = ratio;

			float scaleY = backCam.videoVerticallyMirrored ? 1f : -1f;
			background.rectTransform.localScale = new Vector3 (1f, scaleY, 1f);

			int orient = -backCam.videoRotationAngle;
			background.rectTransform.localEulerAngles = new Vector3 (0, 0, orient);
		}
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

	public void SaveImage()
	{
		
		//Create a Texture2D with the size of the rendered image on the screen.
		Texture2D texture = new Texture2D(background.texture.width, background.texture.height, TextureFormat.ARGB32, false);


		//Save the image to the Texture2D
		texture.SetPixels(backCam.GetPixels());
		texture.Apply();

		//Encode it as a PNG.
		bytes = texture.EncodeToPNG();


		if (!stop) {
			Texture2D tex = null;
			tex = new Texture2D(1, 1);
			tex.LoadImage(bytes);

			Texture2D newTexture = rotate (texture);
			bytes = newTexture.EncodeToPNG();

			background.rectTransform.localScale = new Vector3 (1f, -1f, 1f);
			background.texture = texture;

			sendButton.SetActive(true);
			stop = true;
		} else {
			background.texture = backCam;
			backCam.Play ();
			stop = false;
			sendButton.SetActive(false);
		}
	}

	IEnumerator Upload(byte[] bytes){
		string EMOTIONKEY = "6370fdd9f7d64a00a9b81cab6078db32";
		string emotionURL = "https://westus.api.cognitive.microsoft.com/emotion/v1.0/recognize";


		var headers = new Dictionary<string, string>() {
			{ "Ocp-Apim-Subscription-Key", EMOTIONKEY },
			{ "Content-Type", "application/octet-stream" }
		};

		WWW www = new WWW(emotionURL, bytes, headers);

		yield return www;
		responseData = www.text;
		responseData = fixJson(responseData);

		QuestionList listFaces = JsonUtility.FromJson<QuestionList> (responseData);

		if (listFaces.Items.Count != 0) {
			int anger = Mathf.RoundToInt ((float)listFaces.Items [0].scores.anger * 100);
			int contempt = Mathf.RoundToInt ((float)listFaces.Items [0].scores.contempt * 100);
			int disgust = Mathf.RoundToInt ((float)listFaces.Items [0].scores.disgust * 100);
			int fear = Mathf.RoundToInt ((float)listFaces.Items [0].scores.fear * 100);
			int happiness = Mathf.RoundToInt ((float)listFaces.Items [0].scores.happiness * 100);
			int neutral = Mathf.RoundToInt ((float)listFaces.Items [0].scores.neutral * 100);
			int sadness = Mathf.RoundToInt ((float)listFaces.Items [0].scores.sadness * 100);
			int suprise = Mathf.RoundToInt ((float)listFaces.Items [0].scores.surprise * 100);

			double maxValue = 0.0;
			string maxName = "";
			Sprite maxSprite = angerSprite;

			if (maxValue < listFaces.Items [0].scores.anger) {
				maxValue = listFaces.Items [0].scores.anger;
				maxName = "ANGER";
				maxSprite = angerSprite;
			}
			if (maxValue < listFaces.Items [0].scores.contempt) {
				maxValue = listFaces.Items [0].scores.contempt;
				maxName = "CONTEMPT";
				maxSprite = contemptSprite;
			}
			if (maxValue < listFaces.Items [0].scores.disgust) {
				maxValue = listFaces.Items [0].scores.disgust;
				maxName = "DISGUST";
				maxSprite = disgustSprite;
			}
			if (maxValue < listFaces.Items [0].scores.fear) {
				maxValue = listFaces.Items [0].scores.fear;
				maxName = "FEAR";
				maxSprite = fearSprite;
			}
			if (maxValue < listFaces.Items [0].scores.happiness) {
				maxValue = listFaces.Items [0].scores.happiness;
				maxName = "HAPPY";
				maxSprite = happySprite;
			}
			if (maxValue < listFaces.Items [0].scores.neutral) {
				maxValue = listFaces.Items [0].scores.neutral;
				maxName = "NEUTRAL";
				maxSprite = neutralSprite;
			}
			if (maxValue < listFaces.Items [0].scores.sadness) {
				maxValue = listFaces.Items [0].scores.sadness;
				maxName = "SAD";
				maxSprite = sadnessSprite;
			}
			if (maxValue < listFaces.Items [0].scores.surprise) {
				maxValue = listFaces.Items [0].scores.surprise;
				maxName = "SURPRISE";
				maxSprite = surpriseSprite;
			}

			happyText.text = happiness.ToString () + "%";
			contemptText.text = contempt.ToString () + "%";
			neutralText.text = neutral.ToString () + "%";
			supriseText.text = suprise.ToString () + "%";
			sadnessText.text = sadness.ToString () + "%";
			fearText.text = fear.ToString () + "%";
			disgustText.text = disgust.ToString () + "%";
			angerText.text = anger.ToString () + "%";

			captionEmoji.text = maxName;

			emojiImage.sprite = maxSprite;
		} else {
			happyText.text = "0%";
			contemptText.text = "0%";
			neutralText.text = "0%";
			supriseText.text = "0%";
			sadnessText.text = "0%";
			fearText.text = "0%";
			disgustText.text = "0%";
			angerText.text = "0%";

			captionEmoji.text = "NO FACE";
			emojiImage.sprite = null;
		}
	}

	public void sendImage(){
		grayPanel.SetActive(true);
		emotionCard.SetActive(true);
		happyText.enabled = true;
		contemptText.enabled = true;
		neutralText.enabled = true;
		supriseText.enabled = true;
		sadnessText.enabled = true;
		fearText.enabled = true;
		disgustText.enabled = true;
		angerText.enabled = true;
		emojiImage.enabled = true;
		captionEmoji.enabled = true;
		redoButton.SetActive(true);


		StartCoroutine(Upload (bytes));
	}

	public void TakeImage(){
		grayPanel.SetActive(false);
		emotionCard.SetActive(false);
		happyText.enabled = false;
		contemptText.enabled = false;
		neutralText.enabled = false;
		supriseText.enabled = false;
		sadnessText.enabled = false;
		fearText.enabled = false;
		disgustText.enabled = false;
		angerText.enabled = false;
		emojiImage.enabled = false;
		captionEmoji.enabled = false;
		redoButton.SetActive(false);

		background.texture = backCam;
		backCam.Play ();
		stop = false;
		sendButton.SetActive(false);

	}
	public void goBackAR(){
		//Create a Texture2D with the size of the rendered image on the screen.
		Texture2D texture = new Texture2D(background.texture.width, background.texture.height, TextureFormat.ARGB32, false);


		//Save the image to the Texture2D
		texture.SetPixels(backCam.GetPixels());
		texture.Apply();

		//Encode it as a PNG.
		bytes = texture.EncodeToPNG();

		if (!stop) {
			Texture2D tex = null;
			tex = new Texture2D(1, 1);
			tex.LoadImage(bytes);

			Texture2D newTexture = rotate (texture);
			bytes = newTexture.EncodeToPNG();

			background.rectTransform.localScale = new Vector3 (1f, -1f, 1f);
			background.texture = texture;

			sendButton.SetActive(true);
			stop = true;
		} 
		backCam.Stop ();
		Application.LoadLevel ("UnityARKitScene");
	}
}
