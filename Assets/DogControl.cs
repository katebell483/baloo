using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;

public class DogControl : MonoBehaviour {

	private Animation animation;
	private bool shouldMove = false;
	private Rigidbody rb;
	private bool dogInScene = false;
	//private int speedHash = Animator.StringToHash("speed");
	private List<ARPlaneAnchor> availablePlanes;
	//delegate void ARAnchorAdded(ARPlaneAnchor anchorData);
	//ARAnchorAdded ARAnchorAddedEvent;

	void  Awake() {
		UnityARSessionNativeInterface.ARAnchorAddedEvent += AddAnchor;			
	}

	public void AddAnchor(ARPlaneAnchor pAnchor) {
		Debug.Log ("ADDING PLANE");
		Debug.Log (pAnchor.identifier);
		try {
			availablePlanes.Add(pAnchor);
		} catch {
			Debug.Log ("failed to add");
		}
		//availablePlanes.Add (pAnchor);
	}

	// Use this for initialization
	void Start () {
		animation = GetComponent<Animation> ();
		rb = gameObject.GetComponent<Rigidbody> ();
	}
		
	// Update is called once per frame
	void Update () {
		/*
		if (shouldMove) {
			transform.Translate (Vector3.forward * Time.deltaTime * (transform.localScale.x * .05f));
		}*/

		if (!dogInScene) {
			//placeDog ();
		}

	}

	private void placeDog() {
		// check to see if there is platform wide enough for dog to land
		float minSize = 1.0f;
		List<ARHitTestResult> hitResults = getHitTest();
		if (hitResults.Count == 0)
			return;
		ARHitTestResult result = hitResults[0];
		Debug.Log ("TESTING PLANE!");
		if (planeBigEnough(result, minSize) && !dogInScene) {
			Debug.Log ("PLANE BIG ENOUGH! PUTTING DOG IN SCENE");
		
			dogInScene = true;
			//transform.rotation = Quaternion.Euler (Vector3.zero);
			//transform.position = UnityARMatrixOps.GetPosition (result.worldTransform);
			// make the dog face the camera
			//LookAt ();
		}
	}

	private bool planeBigEnough(ARHitTestResult result, float minSize) {
		Debug.Log("1");
		string id = result.anchorIdentifier;
		Debug.Log("2");

		ARPlaneAnchor hitPlane = availablePlanes.Find (plane => plane.identifier == id);
		Debug.Log("3");

		float width = hitPlane.extent.x;
		Debug.Log("4");
		float length = hitPlane.extent.z;
		Debug.Log ("WIDTH: " + width);
		Debug.Log ("LENGTH: " + length);
		return(width > minSize && length > minSize);
	}

	private List<ARHitTestResult> getHitTest() {

		// Project from the middle of the screen to look for a hit point on the detected surfaces.
		var screenPosition = Camera.main.ScreenToViewportPoint (new Vector2 (Screen.width / 2f, Screen.height / 2f));
		ARPoint pt = new ARPoint {
			x = screenPosition.x,
			y = screenPosition.y
		};

		// Try to hit within the bounds of an existing AR plane.
		List<ARHitTestResult> results = UnityARSessionNativeInterface.GetARSessionNativeInterface ().HitTest (
			pt, 
			ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent);

		return results;
	}

	public void LookAt() {
		transform.LookAt (Camera.main.transform.position);
		transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
	}

	/*
	public void ARAnchorUpdated(ARPlaneAnchor anchorData) {
		float width = anchorData.extent.x;
		float length = anchorData.extent.z;
		if (width > 1 && length > 1) {
			availablePlanes.Add (anchorData);
		}
	}
	*/
}
