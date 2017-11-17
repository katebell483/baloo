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
	private GameObject ball;
	private GameObject food;

	private bool detectQR = true;
	private Vector3 camPos;

	// Use this for initialization
	void Start () {
		arSession = UnityARSessionNativeInterface.GetARSessionNativeInterface ();
		corgi = GameObject.FindWithTag("Corgi");
		mat = GameObject.FindWithTag("Mat");
		matPlane = GameObject.FindWithTag ("MatPlane");
		corgiHouse = GameObject.FindWithTag ("CorgiHouse");
		ball = GameObject.FindWithTag ("Ball");
		food = GameObject.FindWithTag ("dogFood");
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
		float[] bounds = GetQRCodeBounds ();

		var topLeft     = Camera.main.ScreenToViewportPoint (new Vector3 (bounds [0], bounds [1]));
		var topRight    = Camera.main.ScreenToViewportPoint (new Vector3 (bounds [2], bounds [3]));
		var bottomRight = Camera.main.ScreenToViewportPoint (new Vector3 (bounds [4], bounds [5]));
		var bottomLeft  = Camera.main.ScreenToViewportPoint (new Vector3 (bounds [6], bounds [7]));

		HitTest (topLeft, topRight, bottomRight, bottomLeft);
	}

	private void HitTest(Vector3 topLeft, Vector3 topRight, Vector3 bottomRight, Vector3 bottomLeft) {
		Dictionary<string, List<ARHitTestResult>> results = new Dictionary<string, List<ARHitTestResult>>();
		HitTest (topLeft, results);
		HitTest (topRight, results);
		HitTest (bottomRight, results);
		HitTest (bottomLeft, results);

		foreach (var result in results) {
			List<ARHitTestResult> list = result.Value;
			if (list.Count == 4) {
				var worldTopLeft     = UnityARMatrixOps.GetPosition (list[0].worldTransform);
				var worldBottomRight = UnityARMatrixOps.GetPosition (list[2].worldTransform);
				var worldBottomLeft  = UnityARMatrixOps.GetPosition (list[3].worldTransform);

				var bottomToTop = worldTopLeft - worldBottomLeft;
				var leftToRight = worldBottomRight - worldBottomLeft;

				Debug.Log ("PLACING DOG");
				mat.transform.forward = bottomToTop;
				mat.transform.position = worldBottomLeft + (bottomToTop + leftToRight) * 0.5f;
				matPlane.transform.localScale = new Vector3(.05f, .05f, .05f);
				Vector3 center = matPlane.GetComponent<Renderer> ().bounds.center;

				// TODO: this all seems a little out of place here
				corgi.transform.parent = null; // is this necessary?
				corgi.GetComponent<DogControl> ().InitialSequenceWrapper ();
				corgi.GetComponent<DogControl> ().dogInScene = true;
				Debug.Log ("QR CODE ORIG BALL POS: " + ball.transform.position);
				ball.GetComponent<BallMaker> ().setOrigPos(ball.transform.position);
				corgi.GetComponent<DogControl>().foodPos = food.transform.position;

				detectQR = false; 

				break;
			}
		}

	}

	private void HitTest(Vector3 point, Dictionary<string, List<ARHitTestResult>> results) {
		List<ARHitTestResult> hitResults = arSession.HitTest (
			new ARPoint { x = point.x, y = point.y },
			ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent);

		foreach (var hitResult in hitResults) {
			string anchorIdentifier = hitResult.anchorIdentifier;
			List<ARHitTestResult> list;
			if (!results.TryGetValue (anchorIdentifier, out list)) {
				list = new List<ARHitTestResult> ();
				results.Add (anchorIdentifier, list);
			}
			list.Add (hitResult);
		}
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