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

	public GameObject corgi;

	// Use this for initialization
	void Start () {
		animation = GetComponent<Animation> ();
		rb = gameObject.GetComponent<Rigidbody> ();
		unityARAnchorManager = new UnityARAnchorManager();
		corgi = transform.Find ("Corgi").gameObject;
	}

	// Update is called once per frame
	void Update () {
		if (shouldMove) {
			corgi.transform.Translate (Vector3.forward * Time.deltaTime * (transform.localScale.x * .25f));
		} 
	}
		
	void OnCollisionEnter(Collision collision){
		List<ARPlaneAnchorGameObject> availAnchors = unityARAnchorManager.GetCurrentPlaneAnchors ();
		Debug.Log("Enter Called");
		Debug.Log ("planes now avail:" + availAnchors.Count);
	}

	void OnCollisionStay(Collision collision){
		Debug.Log("Enter Stay");    
		lastTransform = corgi.transform.position;
	}
	void OnCollisionExit(Collision collision){
		shouldMove = false;
		Sit ();
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
		corgi.transform.rotation = Quaternion.Euler (Vector3.zero);
		corgi.transform.position = UnityARMatrixOps.GetPosition (result.worldTransform);

		/*
		try {
			//DebqrcodePlane.transform.GetChild();
			//plane = qrcodePlane.transform.Find ("Plane").gameObject;
			//Transform transforms = qrcodePlane.transform;
			foreach(Transform child in transform) {
				Debug.Log(child.name);
			}
			//Debug.Log (plane);
			corgi.transform.position = dogPlane.transform.position;
		} catch (Exception ex) {
			Debug.Log (ex.ToString());
		}
		*/

		// initial animation sequence
		//initialAnimationSequence();

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
		corgi.transform.rotation = Quaternion.LookRotation (lookDirection);
	}

	public void LookAt() {
		corgi.transform.LookAt (Camera.main.transform.position);
		corgi.transform.eulerAngles = new Vector3(0, corgi.transform.eulerAngles.y, 0);
	}
}