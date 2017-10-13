using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;
using System;

public class DogControl : MonoBehaviour {

	private Animation animation;
	private bool shouldMove = false;
	private Rigidbody rb;
	private bool dogInScene = false;
	//private int speedHash = Animator.StringToHash("speed");
	private UnityARAnchorManager unityARAnchorManager;
	private ARPlaneAnchorGameObject currentPlane;
	private Vector3 lastTransform;


	// Use this for initialization
	void Start () {
		animation = GetComponent<Animation> ();
		rb = gameObject.GetComponent<Rigidbody> ();
		unityARAnchorManager = new UnityARAnchorManager();
	}
		
	// Update is called once per frame
	void Update () {
		if (shouldMove) {
			transform.Translate (Vector3.forward * Time.deltaTime * (transform.localScale.x * .25f));
		} 

		/*
		Vector3 fwd = transform.TransformDirection(Vector3.forward);
	
		Vector3 dir = new Vector3);

		
		if (Physics.Raycast (transform.position, down, 10)) {
			Debug.Log ("There is something in front of the object!");

		} else {
			Debug.Log ("NOPE NO PLANE!");

		}*/

		// check for plane collision

	}


	void OnCollisionEnter(Collision collision){
		List<ARPlaneAnchorGameObject> availAnchors = unityARAnchorManager.GetCurrentPlaneAnchors ();
		Debug.Log("Enter Called");
		Debug.Log ("planes now avail:" + availAnchors.Count);
	}

	void OnCollisionStay(Collision collision){
		Debug.Log("Enter Stay");    
		lastTransform = transform.position;
	}
	void OnCollisionExit(Collision collision){
		shouldMove = false;

		//animation.Stop ();
		Sit ();
		//transform.position = lastTransform;
		Debug.Log("Enter Exit");    
	}

	public void placeDog() {
		
		// check for planes
		List<ARHitTestResult> hitResults = getHitTest();

		// if plane exists, place the dog
		if (hitResults.Count == 0)
			return;
		
		ARHitTestResult result = hitResults[0];

		// get the actual game object corresponding to the hit result
		List<ARPlaneAnchorGameObject> availAnchors = unityARAnchorManager.GetCurrentPlaneAnchors ();
		ARPlaneAnchorGameObject planeObj = availAnchors.Find (plane => plane.planeAnchor.identifier == result.anchorIdentifier);
		currentPlane = planeObj;

		// set the dog on the platform
		transform.rotation = Quaternion.Euler (Vector3.zero);
		transform.position = UnityARMatrixOps.GetPosition (result.worldTransform);

		// initial animation sequence
		initialAnimationSequence();

	}

	private void initialAnimationSequence() {
		// have him sit and look at the camera
		LookAt ();
		//Jump ();
		Invoke("LayDown", 1);
	}

	public void LayDown() {
		shouldMove = false;
		animation.CrossFade ("CorgiSitToLay");
	}

	public void Sit() {
		shouldMove = false;
		animation.CrossFade ("CorgiSitIdle");
	}

	public void Walk() {
		shouldMove = true;
		animation.CrossFade ("CorgiWalk");
	}

	public void Jump() {
		shouldMove = false;
		animation.CrossFade ("CorgiJump");
		rb.AddForce (Vector3.up * 80f);
	}

	// this is just faking the gesture for now
	public void Fetch() {

		// get all the planes
		List<ARPlaneAnchorGameObject> availAnchors = unityARAnchorManager.GetCurrentPlaneAnchors ();

		if (availAnchors.Count <= 1) {
			Debug.Log ("need more planes");
			return;
		}

		// get a plane with a different from the one where the dog is sitting
		ARPlaneAnchorGameObject destPlane = availAnchors.FindLast (plane => plane.planeAnchor.identifier != currentPlane.planeAnchor.identifier);

		// face dog in direciton on new plane
		Vector3 lookAtPos = destPlane.gameObject.transform.position;
		//Set y of LookAt target to be my height.
		lookAtPos.y = transform.position.y;
		transform.LookAt (lookAtPos);

		// walk dog to edge of plane
		Walk();

		// jump to new plane

		// sit

	}

	/* this be an unnecessary function */
	private bool planeBigEnough(ARHitTestResult result, float minSize) {
		return true;
		string id = result.anchorIdentifier;
		List<ARPlaneAnchorGameObject> availAnchors = unityARAnchorManager.GetCurrentPlaneAnchors ();
		ARPlaneAnchorGameObject planeObj = availAnchors.Find (plane => plane.planeAnchor.identifier == id);
		float width = planeObj.planeAnchor.extent.x;
		float length = planeObj.planeAnchor.extent.z;
		Debug.Log ("WIDTH: " + width);
		Debug.Log ("LENGTH:" + length);
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

	private void SetLookDirection(Vector3 inputAxes) {
		// Get the camera's y rotation, then rotate inputAxes by the rotation to get up/down/left/right according to the camera
		Quaternion yRotation = Quaternion.Euler (0, Camera.main.transform.rotation.eulerAngles.y, 0);
		Vector3 lookDirection = (yRotation * inputAxes).normalized;
		transform.rotation = Quaternion.LookRotation (lookDirection);
	}

	public void LookAt() {
		transform.LookAt (Camera.main.transform.position);
		transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
	}
}
