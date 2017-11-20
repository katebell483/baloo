﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;
using System;
using UnityEngine.UI;

public class DogControl : MonoBehaviour {

	// basic dog params
	public GameObject corgi;
	private Animation animation;
	private bool shouldMove = false;
	public bool dogInScene = false;
	Collider corgiCollider;

	// UI elements
	public GameObject speechBubble;
	public GameObject fetchButton;
	public GameObject eatButton;
	public GameObject breatheButton;

	//Daniel
	public GameObject aura;

	// other game objects in the scene
	public GameObject mat;
	public GameObject ball;
	public GameObject dogFood;
	public GameObject infoBubble;
	public GameObject introPanel;
	public GameObject dogNamePanel;

	// fetching params
	private bool initialFetchSequence = false;
	public bool fetching = false;
	public bool goingToFood = false;
	private bool returnTrip = false;
	private Vector3 endFetchingPos;
	public Vector3 foodPos;
	private Vector3 startFetchingPos;
	private Vector3 fetchOrigin;
	private float speed = .3f;
	private float fraction = 0; 

	// petting params
	private bool corgiTouched = false;
	private IEnumerator sittingCoroutine;
	private bool waitingToSit = false;
	private IEnumerator layingCoroutine;

	//Daniel: breathing params
	private bool isBreathing = false;
	private bool auraGrowing = true; //true if growing, false if reducing
	private float nbBreathingCycles = 0;

	// rotating params
	private bool rotating = false;
	private Vector3 rotatingTargetPos;

	// sitting params
	public bool speechBubbleShown = false;

	// Use this for initialization
	void Start () {
		animation = corgi.GetComponent<Animation> ();

		introPanel = GameObject.FindWithTag ("introPanel");
		dogNamePanel = GameObject.FindWithTag ("dogNamePanel");
		dogNamePanel.SetActive(false);
			
		mat = GameObject.FindWithTag ("Mat");
		corgi = GameObject.FindWithTag("Corgi");
		dogFood = GameObject.FindWithTag ("dogFood");

		//Physics.IgnoreCollision(corgi.GetComponent<Collider>(), ball.GetComponent<Collider>());
		//Physics.IgnoreCollision(corgi.GetComponent<Collider>(), mat.GetComponent<Collider>());
		//corgiCollider = corgi.GetComponent<Collider>();

		speechBubble = GameObject.FindWithTag ("speechBubble");
		speechBubble.SetActive(false);
		infoBubble = GameObject.FindWithTag ("infoBubble");
		//speechBubble.SetActive(true);

		//Daniel
		aura = GameObject.FindWithTag("Aura");
		aura.SetActive(false);
	}

	// Update is called once per frame
	void Update () {


		// turn off buttons until dog in scene
		if (dogInScene) {
			Debug.Log ("setting buttons to true");
			fetchButton.SetActive (true);
			eatButton.SetActive (true);
			breatheButton.SetActive (true);
		} else {
			Debug.Log ("setting buttons to false");
			fetchButton.SetActive (false);
			eatButton.SetActive (false);
			breatheButton.SetActive (false);
		}

		//Daniel: Breathing phase:
		if (isBreathing){
			Breathe ();
		}
			
		// is the dog rotating?
		if (rotating) {
			rotateDog (rotatingTargetPos);
		}
			
		if (shouldMove && !fetching) {
			corgi.transform.Translate (Vector3.forward * Time.deltaTime * (corgi.transform.localScale.x * .25f));
			// we want to keep track of the corgis position before it leaves the plane
			startFetchingPos = corgi.transform.position;
		} 

		if (SwipeManager.Instance.IsSwiping(SwipeDirection.Down)){
			Sit ();
		}

		// PETTING
		if (!fetching && !goingToFood) {
			CheckForPetting();
		}
			
		// FETCHING
		if (fetching) {
			Fetch (); 
		}
			
		// EATING
		if (goingToFood) {
			GoToFoodAndEat();
		}
	}
		
	public void rotateDog(Vector3 targetPos) {
		Debug.Log ("ROTATING");
		Vector3 targetPoint = new Vector3(targetPos.x, corgi.transform.position.y, targetPos.z) - corgi.transform.position;
		Quaternion targetRotation = Quaternion.LookRotation (targetPoint, Vector3.up);
		corgi.transform.rotation = Quaternion.Slerp(corgi.transform.rotation, targetRotation, Time.deltaTime * 3.0f);
	}

	void OnTriggerExit(Collider other) {
		Debug.Log (other.tag);

		if (initialFetchSequence) {
			
			Debug.Log ("HIT WALL STARTING FETCH");

			// disable colliders
			corgiCollider = corgi.GetComponent<Collider>();
			corgiCollider.enabled = false;
			Debug.Log ("HIT WALL STARTING FETCH2");

			Gallop ();
			Debug.Log ("HIT WALL STARTING FETCH3");

			fetching = true;
			initialFetchSequence = false;

		} else {
			corgi.transform.LookAt (mat.transform.position);
			Sit ();
		} 
	}
		
	public void placeDog() {

		// check for planes
		List<ARHitTestResult> hitResults = getHitTest();

		// if plane exists, place the dog
		if (hitResults.Count == 0)
			return;

		ARHitTestResult result = hitResults[0];

		// set the dog on the platform
		corgi.transform.rotation = Quaternion.Euler (Vector3.zero);
		corgi.transform.position = UnityARMatrixOps.GetPosition (result.worldTransform);

		LookAt ();

		dogInScene = true;
	}

	public void StartFetchingSequence(Vector3 ballPos) {
		Debug.Log ("sending dog to fetch");

		// mark position
		//Debug.Log ("start fetching pos " + fetchOrigin);
		Debug.Log ("ball pos " + ballPos);

		//fetchOrigin = corgi.transform.position;
		endFetchingPos = ballPos;

		rotating = true;
		rotatingTargetPos = endFetchingPos;

		// walk to the edge
		Walk ();

		// then turn and walk towards edge of platform
		initialFetchSequence = true;
	}

	public void Walk() {
		shouldMove = true;
		animation.CrossFade ("CorgiTrot");
	}

	public void Gallop() {
		shouldMove = true;
		animation.CrossFade ("CorgiGallop");
	}

	public void Jump() {
		shouldMove = false;
		animation.CrossFade ("CorgiJump");
	}

	public void Lay() {
		shouldMove = false;
		animation.CrossFade ("CorgiLayIdle");
	}

	public void LayDown() {
		shouldMove = false;
		animation.CrossFade ("CorgiSitToLay");
	}

	public void Sit() {
		shouldMove = false;
		animation.CrossFade ("CorgiSitIdle");
	}

	public IEnumerator WaitAndSit(float waitTime) {
		Debug.Log ("waiting to sit");
		waitingToSit = true;
		yield return new WaitForSeconds (waitTime);
		waitingToSit = false;
		Sit ();
	}

	public IEnumerator WaitAndLay(float waitTime) {
		Debug.Log ("waiting to lay");
		yield return new WaitForSeconds(waitTime); 
		Lay ();
	}

	public void CheckForPetting() {
		if (Input.touchCount > 0) {

			Touch touch = Input.touches [0];
			Vector3 pos = touch.position;

			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay (pos); 

			if (touch.phase == TouchPhase.Began) {
				if (Physics.Raycast (ray, out hit) && hit.transform.gameObject.tag == "Corgi") {
					Debug.Log ("corgi touched");
					corgiTouched = true;
					if (waitingToSit)
						StopCoroutine (sittingCoroutine);
					layingCoroutine = WaitAndLay (.5f);
					StartCoroutine (layingCoroutine);
				}
			}

			if (touch.phase == TouchPhase.Ended && corgiTouched) {
				sittingCoroutine = WaitAndSit (3.0f);
				StartCoroutine (sittingCoroutine);
				corgiTouched = false;
			}
		}
	}

	public void StartEatingSequence() {
		Debug.Log ("GOING TO BOWL");
		goingToFood = true;
		rotatingTargetPos = dogFood.transform.position;
		rotating = true;
		Walk ();
	}
		
	public void GoToFoodAndEat() {
		
		// move dog forward
		fraction += Time.deltaTime * .025f;
		Vector3 currPos  = Vector3.Lerp(corgi.transform.position, foodPos, fraction);

		//check distance
		float distance = Math.Abs(Vector3.Distance(currPos, foodPos));
		Debug.Log ("DISTANCE: " + distance);

		// got to food so stop + eat
		if (distance < .06) {
			Debug.Log ("GOT TO FOOD!");
			rotating = false;
			fraction = 0;
			goingToFood = false;
			EatWrapper ();
		} else {
			corgi.transform.position = currPos;
		}
	}

	public void EatWrapper() {
		StartCoroutine(Eat());
	}

	public IEnumerator Eat() {
		Debug.Log ("EATING");
		shouldMove = false;
		animation.CrossFade ("CorgiEat");
		yield return new WaitForSeconds(5f); // waits 5 seconds
		Sit();
	}

	//Daniel
	public void AuraWarper(){
		StartCoroutine(Aura());
	}
	public IEnumerator Aura() {
		yield return new WaitForSeconds(3f); // waits 3 seconds
		auraGrowing = false;
	}

	public void BarkLong() {
		Debug.Log ("Barking dog !");
		shouldMove = false;
		animation.CrossFade ("CorgiIdleBarkingLong");
		isBreathing = true;
	}

	public void Bark(){
		shouldMove = false;
		animation.CrossFade ("CorgiIdleBarking");
		isBreathing = true;
	}

	public void Fetch() {
		
		// move the dog forward
		fraction += Time.deltaTime * speed;

		// originally start from the last point that the dog was on the plane
		Vector3 fetchingPos = Vector3.Lerp (startFetchingPos, endFetchingPos, fraction);

		// if dog reaches its destination that means it either has to turn around of fetching done
		if (fetchingPos == endFetchingPos) {
			Debug.Log ("made it to the ball!");

			if (returnTrip) {
				Debug.Log ("fetching complete!");
				fetching = false;
				returnTrip = false;
				fraction = 0;

				Debug.Log ("dropping ball!");
				LookAt ();
				Sit ();

				// destroy current fetch ball
				// TODO: do something more clever here
				ball.transform.parent = null;
				Destroy (ball);

				// restore corgi settings
				rotating = false;
				corgiCollider = corgi.GetComponent<Collider>();
				corgiCollider.enabled = true;

			} else {
				Debug.Log ("turning around");
				returnTrip = true;
				fraction = 0;
				startFetchingPos = endFetchingPos;
				endFetchingPos = mat.transform.position;
				Debug.Log(endFetchingPos);
				Debug.Log(startFetchingPos);

				// rotate
				rotating = true;
				rotatingTargetPos = mat.transform.position;

				// grab the ball by parenting it to the dog
				Debug.Log ("grabbing ball");
				ball = GameObject.FindWithTag ("Ball");
				ball.transform.parent = corgi.transform;
			}
		} else {
			corgi.transform.position = fetchingPos;
		}
	}

	public void Breathe(){
		aura.transform.position = corgi.transform.position;
		aura.SetActive (true);

		// End of the 3 cycles of breathing => re-initialize the parameters
		if (nbBreathingCycles >= 3) {
			isBreathing = false;
			nbBreathingCycles = 0;
			auraGrowing = false;
			aura.SetActive (false);
			Sit();
			LookAt ();
		}
		// After 1 cycle Baloo is Barking less
		else if (nbBreathingCycles >= 1) {
			Bark ();
		}

		// Transformation of the sphere
		if (auraGrowing && aura.transform.localScale.x < 4.5) {
			infoBubble.GetComponentInChildren<Text> ().text = "Breathe in to calm Baloo";
			aura.transform.localScale += new Vector3 (0.01F, 0.01F, 0.01F);
		} else if (auraGrowing && aura.transform.localScale.x >= 4.5) {
			infoBubble.GetComponentInChildren<Text> ().text = "Hold your breath";
			AuraWarper ();
		} else if (!auraGrowing && aura.transform.localScale.x > 1.5) {
			infoBubble.GetComponentInChildren<Text> ().text = "Breathe out slowly";
			aura.transform.localScale -= new Vector3 (0.01F, 0.01F, 0.01F);
		} else if (aura.transform.localScale.x <= 1.5) {
			auraGrowing = true;
			nbBreathingCycles += 1;
		}
	}

	public void InitialSequenceWrapper() {
		StartCoroutine(InitialSequence());
	}

	public IEnumerator InitialSequence() {
		Debug.Log ("Dog starting initial sequence");
		//infoBubble.GetComponentInChildren<Text>().text = "I'm here!";
		Walk ();
		yield return new WaitForSeconds(3.5f); // waits 3.5 seconds
		Sit();
		LookAt ();
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
		corgi.transform.LookAt (Camera.main.transform.position);
		corgi.transform.eulerAngles = new Vector3(0, corgi.transform.eulerAngles.y, 0);
	}

	public void goBackToMenu(){
		Application.LoadLevel ("menu");
	}

	public void hideIntroPanel(){
		Debug.Log ("HERE!!!");
		introPanel.SetActive (false);
	}

	public void hideDogNamePanel(){
		dogNamePanel.SetActive (false);
	}


	public void goBackToQuestion(){
		if (!speechBubbleShown) {
			speechBubble.SetActive(true);
		}
		else {
			speechBubble.SetActive(false);
		}
		speechBubbleShown = !speechBubbleShown;

		//Application.LoadLevel ("questions");
	}
		
	public void goBackToCamera(){
		Application.LoadLevel ("camera");
	}
}