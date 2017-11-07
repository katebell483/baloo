﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;
using System;

public class DogControl : MonoBehaviour {

	private Animation animation;
	private bool shouldMove = false;
	private Rigidbody rb;
	//private bool dogInScene = false;
	//private int speedHash = Animator.StringToHash("speed");
	private UnityARAnchorManager unityARAnchorManager;
	private ARPlaneAnchorGameObject currentPlane;
	private Vector3 lastTransform;

	public GameObject mat;
	public GameObject corgi;
	public GameObject ball;
	private bool fetching = false;


	private Vector3 endFetchingPos;
	private Vector3 startFetchingPos;
	public float speed = .5f;
	private float fraction = 0; 
	private bool returnTrip = false;

	// Use this for initialization
	void Start () {
		animation = GetComponent<Animation> ();
		rb = gameObject.GetComponent<Rigidbody> ();
		unityARAnchorManager = new UnityARAnchorManager();
		mat = GameObject.FindWithTag ("Mat");
		corgi = GameObject.FindWithTag("Corgi");
	}

	// Update is called once per frame
	void Update () {
		if (shouldMove) {
			transform.Translate (Vector3.forward * Time.deltaTime * (transform.localScale.x * .25f));
		} 

		if (SwipeManager.Instance.IsSwiping(SwipeDirection.Down)){
			Sit ();
		}

		if (fetching) {
			
			Debug.Log("fetching currently");

			// move the dog forward
			fraction += Time.deltaTime * speed;

			Vector3 fetchingPos = Vector3.Lerp (startFetchingPos, endFetchingPos, fraction);
			Debug.Log ("_________________________________________________");
			Debug.Log ("fraction " + fraction);
			Debug.Log ("fetchingPos " + fetchingPos);
			Debug.Log ("endfetchingPos " + endFetchingPos);
			Debug.Log ("_________________________________________________");

			if (fetchingPos == endFetchingPos) {
				Debug.Log ("made it to the ball!");

				if (returnTrip) {
					Debug.Log ("fetching complete!");
					fetching = false;
					returnTrip = false;
					fraction = 0;

					Debug.Log ("dropping ball!");
					ball.transform.parent = null;

				} else {
					Debug.Log ("turning around");
					returnTrip = true;
					fraction = 0;
					endFetchingPos = startFetchingPos;
					startFetchingPos = fetchingPos;

					Debug.Log ("grabbing ball");
					ball = GameObject.FindWithTag ("Ball");
					GameObject corgi = GameObject.FindWithTag ("Corgi");

					Debug.Log ("ball pos " + ball.transform.position);
					ball.transform.parent = corgi.transform;
				}
			} else {
				transform.position = fetchingPos;
			}
		}
			
	}

	void OnTriggerExit(Collider other) {
		Debug.Log (other.tag);
		transform.LookAt (mat.transform.position);
		Sit ();
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

	}

	public void fetchBall(Vector3 ballPos) {
		Debug.Log ("sending dog to fetch");

		startFetchingPos = transform.position;
		Debug.Log ("start fetching pos " + startFetchingPos);

		endFetchingPos = ballPos;
		Debug.Log ("end fetching pos " + endFetchingPos);

		fetching = true;
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
		animation.CrossFade ("CorgiTrot");
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
		transform.rotation = Quaternion.LookRotation (lookDirection);
	}

	public void LookAt() {
		transform.LookAt (Camera.main.transform.position);
		transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
	}

	public void goBackToMenu(){
		Application.LoadLevel ("menu");
	}

	public void goBackToQuestion(){
		Application.LoadLevel ("questions");
	}

	public void goBackToCamera(){
		Application.LoadLevel ("camera");
	}
}