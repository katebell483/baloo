using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.iOS;

public class QRCodeReader : MonoBehaviour {

	[DllImport ("__Internal")]
	private static extern void ReadQRCode(long mtlTexPtr);

	[DllImport ("__Internal")]
	private static extern void GetQRCodeBounds(out IntPtr boundsPtr);

	private static float[] GetQRCodeBounds() {
		IntPtr boundsPtr;
		GetQRCodeBounds (out boundsPtr);
		float[] bounds = new float[8];
		Marshal.Copy (boundsPtr, bounds, 0, 8);
		return bounds;
	}

	private bool done = false;
	private UnityARSessionNativeInterface arSession = null;
	private GameObject corgi;
	private GameObject mat;
	private GameObject matPlane;
	private GameObject corgiHouse;
	private GameObject food;
	private GameObject dogNamePanel;

	private bool detectQR = true;
	private Vector3 camPos;

	private Vector3[] m_PointCloudData;



	// Use this for initialization
	void Start () {
		arSession = UnityARSessionNativeInterface.GetARSessionNativeInterface ();
		corgi = GameObject.FindWithTag("Corgi");
		mat = GameObject.FindWithTag("Mat");
		matPlane = GameObject.FindWithTag ("MatPlane");
		corgiHouse = GameObject.FindWithTag ("CorgiHouse");
		food = GameObject.FindWithTag ("dogFood");
		dogNamePanel = GameObject.FindWithTag ("dogNamePanel");

		UnityARSessionNativeInterface.ARFrameUpdatedEvent += ARFrameUpdated;

	}

	public void ARFrameUpdated(UnityARCamera camera) {
		m_PointCloudData = camera.pointCloudData;
	}

	// Update is called once per frame
	void Update () {
		camPos = Camera.main.transform.position;
		if (!done && detectQR) {
			ARTextureHandles handles = arSession.GetARVideoTextureHandles ();
			if (handles.textureY != System.IntPtr.Zero) {
				ReadQRCode (handles.textureY.ToInt64 ());
			}
		}
	}

	void OnReadQRCode(string arg) {
		
		if (!detectQR) {
			return;
		}

		Debug.Log ("QR CODE DETECTED:");
		Debug.Log ("SIZE PT CLOUD:" + m_PointCloudData.Length);

		float[] bounds = GetQRCodeBounds ();

		var bottomLeft  = Camera.main.ScreenToViewportPoint (new Vector3 (bounds [6], bounds [7]));

		foreach (Vector3 point in m_PointCloudData) {
			
			Vector3 worldP = Camera.main.WorldToViewportPoint (point);
			float distBL = Vector3.Distance (worldP, bottomLeft);

			Debug.Log("MATCHING: Dist BL: " + distBL);

			if (distBL < .4) {

				mat.transform.LookAt (Camera.main.transform.position);
				mat.transform.eulerAngles = new Vector3(0, mat.transform.eulerAngles.y, 0);

				mat.transform.position = point;
				matPlane.transform.localScale = new Vector3 (.05f, .05f, .05f);

				// TODO: this all seems a little out of place here
				corgi.transform.parent = null; // is this necessary?
				corgi.GetComponent<DogControl> ().InitialSequenceWrapper ();
				corgi.GetComponent<DogControl> ().dogInScene = true;
				corgi.GetComponent<DogControl> ().foodPos = food.transform.position;


				detectQR = false; 
				break;
			}
	
		}
			
		return;

	}

	public void OnSetAnchorClick(Text text) {
		if (done) {
			done = false;
			text.text = "Set Anchor";
		} else {
			done = true;
			text.text = "Retry Anchor";
		}
	}
}