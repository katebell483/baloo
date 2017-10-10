using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;

public class DogControl : MonoBehaviour {

	private Animation animation;
	private bool shouldMove = false;
	private Rigidbody rb;

	// Use this for initialization
	void Start () {
		animation = GetComponent<Animation> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (shouldMove) {
			transform.Translate (Vector3.forward * Time.deltaTime * (transform.localScale.x * .05f));
		}

		SetPosition ();
	}

	public void SetPosition() {
		// Project from the middle of the screen to look for a hit point on the detected surfaces.
		var screenPosition = Camera.main.ScreenToViewportPoint (new Vector2 (Screen.width / 2f, Screen.height / 2f));
		ARPoint pt = new ARPoint {
			x = screenPosition.x,
			y = screenPosition.y
		};

		// Try to hit within the bounds of an existing AR plane.
		List<ARHitTestResult> hitResults = UnityARSessionNativeInterface.GetARSessionNativeInterface ().HitTest (
			pt, 
			ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent);

		if (hitResults.Count > 0) { // If a hit is found, set the position and reset the rotation.
			transform.rotation = Quaternion.Euler (Vector3.zero);
			transform.position = UnityARMatrixOps.GetPosition (hitResults[0].worldTransform);
		}
	}

	public void Walk() {
		if (!animation.isPlaying) {
			animation.Play ();
			shouldMove = true;
		} else {
			animation.Stop ();
			shouldMove = false;
		}
	}

	public void LookAt() {
		transform.LookAt (Camera.main.transform.position);
		transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
	}

	public void Bigger () {
		transform.localScale += new Vector3 (1, 1, 1);
	}

	public void Smaller () {
		if (transform.localScale.x > 1) {
			transform.localScale -= new Vector3 (1, 1, 1);
		}
	}
}
