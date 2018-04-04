using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;
using System;
using UnityEngine.UI;

public class DogControl : MonoBehaviour {

	// basic dog params
	public GameObject corgi;
	private bool shouldMove = false;
	public bool dogInScene = false;
	Collider corgiCollider;
	private Transform initialCorgiTransform;
	private Animator animator;
	public int level;

	// UI elements
	public GameObject fetchButton;
	public GameObject syringeButton;
	public GameObject eatButton;
	public GameObject breatheButton;
	public GameObject pillButton;
	public GameObject bandaidButton;
	public GameObject infoBubble;
	public GameObject breatheInstructionButton1;
	public GameObject breatheInstructionButton2;

	// panels
	public GameObject introPanel;
	public GameObject dogNamePanel;
	public GameObject exitPanel;
	public GameObject levelPanel;
	public bool isReadyForQRDetection;

	// Breathing objects
	public GameObject aura;

	// other game objects in the scene
	public GameObject mat;
	public GameObject ball;
	public GameObject dogFood;
	public GameObject propFrisbee;
	public GameObject dogHouse;
	public GameObject syringe;
	public GameObject pill;
	public GameObject bandaid;
	public GameObject dragObject;
	public GameObject dragObjectCube;
	public GameObject gObj; //used for drag object interaction

	// walking params
	private float walkSpeed = .25f;

	// injection params
	private bool injectionDone = false;
	public bool dragObjectActive = false; //if the drag object is setActive = true
	public bool dragObjectDraggable = false;

	// fetching params
	private bool initialFetchSequence = false;
	public bool fetching = false;
	private bool returnTrip = false;
	private Vector3 endFetchingPos;
	private Vector3 startFetchingPos;
	private Vector3 fetchOrigin;
	private float speed = .3f;
	private float fraction = 0; 
	private int numFetches = 0;
	private Transform oldParent;

	// eating params
	public bool goingToFood = false;
	public Vector3 foodPos;
	private int numEatingEvents = 0;
	private bool isLastEatingEvent = false;

	// Random Behavior parameters
	public bool randomBehavior = false; //when the user is not playing with Baloo, it does its random stuff (divided in static actions if isRandomWalking is false, or non-static actions in the contrary)
	public bool isRandomlyWalking = false;
	public float randX, randZ;
	public int numberOfRandomTargetReached = 0;
	public float randomStaticTime = 5f; // The random time of a static action between 2 random walks
	public string randomStaticAction= "Sit";

	// going home params
	private bool isGoingHome = false;

	// petting params
	private bool corgiTouched = false;
	private IEnumerator sittingCoroutine;
	private bool waitingToSit = false;
	private IEnumerator layingCoroutine;

	// breathing params
	private bool isBreathing = false;
	private bool isBreathingTest = false;
	private bool auraGrowing = true; //true if growing, false if reducing
	private float nbBreathingCycles = 0;
	private int numMeditationEvents = 0;
	public bool hasBreathed = false; // true if the breathing feature has been done already

	// blinking params
	public float blinkRate = 1.0F;
	private float nextBlinkCheck = 0.0F;
	private float blinkVal = 0.0F;
	private bool isBlinking = false;
	private bool blinkOpen = false;
	private bool blinkClose = false;

	// rotating params
	public bool rotating = false;
	private Vector3 rotatingTargetPos;

	// sitting params
	public bool isSitting = false;


	// drag object auxiliary objects
	Plane objPlane;
	Vector3 m0;
	Vector3 dragObjectPos;

	Ray GenerateMouseRay(Vector3 touchPos){
		Vector3 mousePosFar = new Vector3 (touchPos.x, touchPos.y, Camera.main.farClipPlane);
		Vector3 mousePosNear = new Vector3 (touchPos.x, touchPos.y, Camera.main.nearClipPlane);
		Vector3 mousePosF = Camera.main.ScreenToWorldPoint (mousePosFar);
		Vector3 mousePosN = Camera.main.ScreenToWorldPoint (mousePosNear);
		Ray mr = new Ray (mousePosN, mousePosF - mousePosN);
		return mr;
	}


	// level settings
	//void level

	// quit app when in background
	void OnApplicationPause(bool pauseStatus){
		Application.Quit ();
	}

	// Use this for initialization
	void Start () {
		
		animator = corgi.GetComponent<Animator> ();

		/* panels */
		levelPanel.SetActive (true);
		introPanel.SetActive(false);
		dogNamePanel.SetActive(false);
		exitPanel.SetActive(false);
	}

	// Update is called once per frame
	void Update () {


		//Daniel
		/* // Return the number of fingers on the screen
		int fingerCount = 0;
		foreach (Touch touch in Input.touches) {
			if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled)
				fingerCount++;
		}
		if (fingerCount > 0)
			Debug.Log("User has " + fingerCount + " finger(s) touching the screen");
		*/

		// drag object feature
		if (dragObjectActive) {
			dragObjectAnimate ();
		}

		// blinking
		if (isBlinking) {
			Blink ();
		} else {
			BlinkCycle ();
		}

		// turn off buttons until dog in scene
		if (dogInScene) {
			showButtons ();
		} else {
			hideButtons ();
		}

		// even rotating but no more distance walk in place
		if (rotating && !shouldMove) {
			//WalkInPlace ();
		}
	
		//Breathing phase:
		if (isBreathing) {
			Breathe ();
		} else if (isBreathingTest) {
			BreatheTest (); 
		} else {
			aura.SetActive (false);
		}

		if (isGoingHome) {
			GoHome ();
		}
			
		// is the dog rotating?
		if (rotating) {
			rotateDog (rotatingTargetPos);
		}
			
		if (shouldMove && !fetching && !randomBehavior) {
			corgi.transform.Translate (Vector3.forward * Time.deltaTime * walkSpeed);
			// we want to keep track of the corgis position before it leaves the plane
			startFetchingPos = corgi.transform.position;	
		} 
			
		if (SwipeManager.Instance.IsSwiping(SwipeDirection.Down) && !waitingToSit) {
			Sit ();
		}

		/*

		// PETTING
		if (!fetching && !goingToFood) {
			CheckForPetting();
		}*/
			
		// FETCHING
		if (fetching) {
			Fetch (); 
		}
			
		// EATING
		if (goingToFood) {
			GoToFoodAndEat();
		}
	}

	private void setDragObject() {
		syringe = GameObject.FindWithTag ("syringe");
		pill = GameObject.FindWithTag ("pill");
		bandaid = GameObject.FindWithTag ("bandaid");
		Debug.Log ("LEVEL " + level);
		switch (level) {
		case 1:
			dragObject = syringe;
			break;
		case 2:
			dragObject = pill;
			break;
		case 3:
			dragObject = bandaid;
			break;
		}

		syringe.SetActive (false);
		pill.SetActive (false);
		bandaid.SetActive (false);
	}

	private void showButtons() {
		eatButton.SetActive (true);
		breatheButton.SetActive (true);
		float width = Screen.width / 3f;
		float width2 = Screen.width / 4f;

		switch(level) {
		case 1:
			syringeButton.SetActive (true);
			SetTransformX (syringeButton, 5 * width/2f);
			SetTransformX (eatButton, width/2f);
			SetTransformX (breatheButton, 3 * width/2f);
			break;
		case 2: 
			fetchButton.SetActive (true); 
			pillButton.SetActive (true);
			SetTransformX (pillButton, 7 * width2/2);
			SetTransformX (eatButton, width2/2);
			SetTransformX (breatheButton, 3 * width2/2);
			SetTransformX (fetchButton, 5 * width2/2);
			break;
		case 3:
			fetchButton.SetActive (true); 
			bandaidButton.SetActive (true);
			SetTransformX (pillButton, 7 * width2/2);
			SetTransformX (eatButton, width2/2);
			SetTransformX (breatheButton, 3 * width2/2);
			SetTransformX (fetchButton, 5 * width2/2);
			break;
		}
	}

	private void SetTransformX (GameObject g, float n) {
		g.transform.position = new Vector3(n, g.transform.position.y, g.transform.position.z);
	}

	private void hideButtons() {
		fetchButton.SetActive (false);
		syringeButton.SetActive (false);
		eatButton.SetActive (false);
		breatheButton.SetActive (false);
	}

	public void BlinkCycle() {
		// get current time and subtract old time
		if(Time.time < nextBlinkCheck) 
			return;

		nextBlinkCheck = Time.time + blinkRate;
		int idx = UnityEngine.Random.Range (1, 4);

		// if its modulo 3 blink (aka have dog blink every 3 seconds)
		if (idx % 3 == 0) {
			isBlinking = true;
			blinkClose = true;
			Blink ();
		}
	}

	public void Blink() {
		SkinnedMeshRenderer corgi_mesh = GameObject.FindWithTag ("pup_blend_mesh").GetComponent<SkinnedMeshRenderer> ();

		if (blinkClose && blinkVal > 95) {
			blinkOpen = true;
			blinkClose = false;
		}

		// do the animation either open or closed
		if (blinkClose) {
			blinkVal = blinkVal + 5f;
		} else if (blinkOpen) {
			blinkVal = blinkVal - 5f;
		}
			
		if (blinkOpen && blinkVal < 10) {
			isBlinking = false;
		}

		corgi_mesh.SetBlendShapeWeight (0, blinkVal);
	}

	public void InitialSequenceWrapper() {
		initialCorgiTransform = corgi.transform;
		StartCoroutine(InitialSequence());
	}

	public IEnumerator InitialSequence() {
		Debug.Log ("Dog starting initial sequence");
		yield return new WaitForSeconds(1.0f); 
		PlaceDog();
		BarkOnce ();
		Sit ();
	}
		
	public void rotateDog(Vector3 targetPos) {
		//Debug.Log ("ROTATING");
		Vector3 targetPoint = new Vector3(targetPos.x, corgi.transform.position.y, targetPos.z) - corgi.transform.position;
		Quaternion targetRotation = Quaternion.LookRotation (targetPoint, Vector3.up);
		corgi.transform.rotation = Quaternion.Slerp(corgi.transform.rotation, targetRotation, Time.deltaTime * 10.0f);
		if(targetRotation == corgi.transform.rotation) {
			rotating = false;
		}
	}

	void OnTriggerExit(Collider other) {
		Debug.Log (other.tag);
		/*
		if (initialFetchSequence) {
			Debug.Log ("HIT WALLs starting to run");

			// disable colliders
			corgiCollider = corgi.GetComponent<Collider> ();
			corgiCollider.enabled = false;

			Run (); 

			fetching = true;
			initialFetchSequence = false;

		} else if (isRandomlyWalking) {
			Debug.Log ("HIT WALL Randomly walking");
			rotatingTargetPos = mat.transform.position;
			rotating = true;
		} else {
			Debug.Log ("HIT WALL GENERAL CASE: " + other.tag);
			corgi.transform.LookAt (mat.transform.position);
			//Sit ();
			WalkToIdle();
		}*/
	}

	public void StartFetchingSequence(Vector3 ballPos) {
		Debug.Log ("sending dog to fetch");
		numFetches += 1;

		//fetchOrigin = corgi.transform.position;
		endFetchingPos = ballPos;

		rotating = true;
		rotatingTargetPos = endFetchingPos;

		// walk to the edge
		//Walk ();

		// then turn and walk towards edge of platform
		//initialFetchSequence = true;

		// disable colliders
		//corgiCollider = corgi.GetComponent<Collider> ();
		//corgiCollider.enabled = false;

		Run (); 

		fetching = true;
	}

	public void Walk() {
		Debug.Log ("walking!");
		isSitting = false;
		shouldMove = true;
		animator.Play ("Walk");
	}

	public void WalkInPlace() {
		Walk ();
		shouldMove = false;
	}

	public void Eat() {
		Debug.Log ("Eating!");
		isSitting = false;
		shouldMove = false;
		animator.Play ("StartEating");
	}

	public void StopEat() {
		Debug.Log ("Eating Done!");
		shouldMove = false;
		animator.Play ("StopEating");
	}

	public void Run() {
		Debug.Log ("RUNNING");
		isSitting = false;
		shouldMove = true;
		animator.Play ("Run");
	}

	public void Sit() {
		Debug.Log ("SIT DOWN");
		if (isSitting)
			return;
		shouldMove = false;
		isSitting = true;
		animator.Play ("SitDown");
	}

	public void Bark() {
		Debug.Log ("Barking");
		isSitting = false;
		shouldMove = false;
		animator.Play ("Bark");
	}

	public void BarkOnce() {
		Debug.Log ("Bark Once");
		isSitting = false;
		shouldMove = false;
		animator.Play ("BarkOnce");
	}

	public void Idle() {
		Debug.Log ("IDLE");
		isSitting = false;
		shouldMove = false;
		animator.Play ("Idle");
	}
		
	public IEnumerator WalkAndSit() {
		Walk ();
		yield return new WaitForSeconds (1f);
		Sit();
	}

	public IEnumerator WaitAndSit(float waitTime, bool isLaying) {
		Debug.Log ("waiting to sit");
		waitingToSit = true;
		yield return new WaitForSeconds (waitTime);
		waitingToSit = false;
		Sit();

	}

	public void StartGoingHome() {
		// rotate towards home
		isGoingHome = true;
		rotating = true;
		rotatingTargetPos = dogHouse.transform.position;
		Walk ();
	}

	public void GoHome() {
		//check distance
		float distance = Math.Abs(Vector3.Distance(corgi.transform.position, dogHouse.transform.position));
		Debug.Log ("DISTANCE: " + distance);

		if (distance < .01 && !rotating) {
			Debug.Log ("CORGI HOME");
			isGoingHome = false;
			StartCoroutine (startExitSequence ());
		} else if (distance > .01) {
			// move dog forward
			Debug.Log ("CORGI STILL GOING HOME");
			fraction += Time.deltaTime * .015f;
			Vector3 currPos = Vector3.Lerp (corgi.transform.position, dogHouse.transform.position, fraction);
			corgi.transform.position = currPos;
		} else {
			Debug.Log ("waiting for rotation to stop");
		}

	}

	public IEnumerator startExitSequence() {
		Debug.Log ("starting exit sequence");
		corgi.transform.rotation = initialCorgiTransform.rotation;
		//LayDown();
		shouldMove = false;
		yield return new WaitForSeconds (1.5f);
		Debug.Log ("exit sequence complete");
		exitPanel.SetActive (true);
	}
		
	/*
	public void CheckForPetting() {
		if (Input.touchCount > 0) {

			Touch touch = Input.touches [0];
			Vector3 pos = touch.position;

			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay (pos); 

			if (touch.phase == TouchPhase.Began) {
				if (Physics.Raycast (ray, out hit) && hit.transform.gameObject.tag == "Corgi") {
					Debug.Log ("corgi touched");
					randomBehavior = false;
					isRandomlyWalking = false;
					corgiTouched = true;
					if (waitingToSit)
						StopCoroutine (sittingCoroutine);
					layingCoroutine = WaitAndLay (.1f);
					StartCoroutine (layingCoroutine);
				}
			}

			if (touch.phase == TouchPhase.Ended && corgiTouched) {
				sittingCoroutine = WaitAndSit (2.5f, true);
				StartCoroutine (sittingCoroutine);
				corgiTouched = false;
			}
		}
	}*/


	public void StartDragObjectSequence(){
		Debug.Log ("StartDragObjectSequence called");
		if (!injectionDone) {
			dragObject.SetActive (true);
			dragObjectActive = true; // drag object appears
			dragObjectDraggable = true; // we can move it
			Vector3 dragObjectInitPos = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y - .1f, Camera.main.transform.position.z);
			dragObject.transform.position = dragObjectInitPos + Camera.main.transform.forward * 0.25f;
			dragObjectAnimate ();
		}
	}

	public void dragObjectAnimate(){
		dragObject.transform.LookAt (corgi.transform.position);
		dragObject.transform.Rotate (0, 90, 0);

		dragObjectPos = corgi.transform.position - dragObject.transform.position;
		Debug.Log ("drag object distance: "+ dragObjectPos.sqrMagnitude);
		if (dragObjectPos.sqrMagnitude < 0.04) { // was .02
			dragObjectDraggable = false;
		}

		if (!dragObjectDraggable) {
			// if we are close enough from the dog, we stop moving it and the animation starts
			dragObjectCube = dragObject.transform.GetChild (0).gameObject;
			if (dragObjectCube.transform.localPosition.x > -65 && level == 1) {
				dragObjectCube.transform.position -= dragObjectCube.transform.right * 0.0005f; // only do this animation for level one syringe
			} else {
				dragObject.SetActive (false);
				dragObjectActive = false;
				dragObjectDraggable = false;
				injectionDone = true;
				StartCoroutine (postInjectionMsg ("Not my favorite,\n but I know it helps me get well"));
			}
		} else {
			if (Input.touchCount > 0) {
				if (Input.GetTouch (0).phase == TouchPhase.Began) {
					Ray mouseRay = GenerateMouseRay (Input.GetTouch (0).position);
					RaycastHit hit;

					if (Physics.Raycast (mouseRay.origin, mouseRay.direction, out hit)) {
						//gObj = hit.collider.transform.gameObject;
						//gObj = hit.transform.gameObject;
						gObj = dragObject;
						Debug.Log ("tag gObj: " + gObj.tag);
						Debug.Log ("tag collider: " + hit.collider.transform.tag);
						objPlane = new Plane (Camera.main.transform.forward * -1, gObj.transform.position);
						// calc touch offset
						Ray mRay = Camera.main.ScreenPointToRay (Input.GetTouch (0).position);
						float rayDistance;
						objPlane.Raycast (mRay, out rayDistance);
						m0 = gObj.transform.position - mRay.GetPoint (rayDistance);
					}
				} else if (Input.GetTouch (0).phase == TouchPhase.Moved && gObj) {
					Ray mRay = Camera.main.ScreenPointToRay (Input.GetTouch (0).position);
					float rayDistance;
					if (objPlane.Raycast (mRay, out rayDistance))
						gObj.transform.position = mRay.GetPoint (rayDistance) + m0 - dragObject.transform.right*0.0005f;
				} else if (Input.GetTouch (0).phase == TouchPhase.Ended && gObj) {
					gObj = null;
				}
			}
		}
	}


	public IEnumerator postInjectionMsg(String msg) {
		triggerInfoBubble(msg, 3.0f); 
		if (!hasBreathed) {
			yield return new WaitForSeconds(3.0f); 
			infoBubble.SetActive (true);
			infoBubble.GetComponentInChildren<Text> ().text = "When I get anxious, it helps to do some deep breathing";
			StartCoroutine(InfoBubbleClose (3.0f));
		} else {
			StartCoroutine(InfoBubbleClose (3.0f));
		}
	}


	public void StartEatingSequence() {
		
		// check for planes
		var screenPosition = Camera.main.ScreenToViewportPoint (new Vector2 (Screen.width / 4f, Screen.height / 4f));
		List<ARHitTestResult> hitResults = getHitTest(screenPosition);

		// if plane exists, place the dog
		if (hitResults.Count == 0)
			return;

		ARHitTestResult result = hitResults[0];
		Debug.Log ("placing bowl");

		// set the dog on the platform
		Vector3 planePos = UnityARMatrixOps.GetPosition (result.worldTransform);
		dogFood.transform.position = new Vector3 (planePos.x, planePos.y, planePos.z);
		dogFood.transform.rotation = Quaternion.Euler (Vector3.zero);
		foodPos = dogFood.transform.position;
		dogFood.SetActive (true);
		rotatingTargetPos = dogFood.transform.position;
		goingToFood = true;
		rotating = true;
		Walk ();
	}

	public void GoToFoodAndEat() {
		
		//check distance
		float distance = Math.Abs(Vector3.Distance(corgi.transform.position, foodPos));
		Debug.Log ("DISTANCE: " + distance);

		// get size of corgi
		float xscale = corgi.transform.localScale.x;

		// got to food so stop + eat
		if (distance < .24) {
			Debug.Log ("reached food going to start eating");
			numEatingEvents += 1;
			fraction = 0;
			goingToFood = false;
			isLastEatingEvent = numEatingEvents == 2;
			StartCoroutine(StartEat());
		} else {
			// move dog forward
			fraction += Time.deltaTime * .015f;
			Vector3 currPos  = Vector3.Lerp(corgi.transform.position, foodPos, fraction);
			corgi.transform.position = currPos;
		}
	}

	/*
	public void EatWrapper() {
		StartCoroutine(StartEat());
	}*/
		
	public IEnumerator StartEat() {
		Debug.Log ("EATING");

		// first eat 
		Eat ();

		yield return new WaitForSeconds(5f); // waits 5 seconds

		// now face the camera and walk away from food
		StopEat ();
		LookAt();
		Sit ();

		// remove bowl
		dogFood.SetActive (false);

		// check for end conditions
		// if interaction complete then start exit sequence
		// if max food events reached disable eat button
		if (!isInteractionComplete () &&  isLastEatingEvent) {
			triggerInfoBubble ("I'm totally stuffed now!\n Yum!", 3.0f);
			eatButton.GetComponent<Button> ().interactable = false;
			//fetchButton.GetComponent<Button> ().Select ();
		}

	}

	private bool isInteractionComplete() {
		if (numEatingEvents > 0 && numFetches > 2 && numMeditationEvents > 0) {
			// needs to be some kind of outro activity
			eatButton.GetComponent<Button> ().interactable = false;
			breatheButton.GetComponent<Button> ().interactable = false;
			fetchButton.GetComponent<Button> ().interactable = false;
			StartGoingHome ();
			return true;
		} else {
			return false;
		}
	}

	//Breathing action
	public void AuraWarper(){
		StartCoroutine(Aura());
	}
	public IEnumerator Aura() {
		yield return new WaitForSeconds(3f); // waits 3 seconds
		auraGrowing = false;
	}
		
	public IEnumerator TimedBark(float time) {
		Bark ();
		yield return new WaitForSeconds(time); // waits 5 seconds
		Sit ();
	}

	public void Fetch() {

		// move the dog forward
		fraction += Time.deltaTime * speed;

		// originally start from the last point that the dog was on the plane
		Vector3 fetchingPos = Vector3.Lerp (startFetchingPos, endFetchingPos, fraction);

		Debug.Log ("fetchingPos: " + startFetchingPos);
		Debug.Log ("goalPos: " + endFetchingPos);

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
				startFetchingPos = endFetchingPos;
				Sit ();
				//Idle();

				// destroy current fetch ball
				// TODO: do something more clever here
				ball.transform.parent = oldParent;
				ball.SetActive(false);

				// restore corgi settings
				rotating = false;
				//corgiCollider = corgi.GetComponent<Collider>();
				//corgiCollider.enabled = true;

				// bring back prop
				//propFrisbee.SetActive(true);

				// first check if thi means we are done
				if (isInteractionComplete ()) {
					return;
				}

				// is this the third fetch?
				if (numFetches % 2 == 0 && numMeditationEvents == 0) {
					Debug.Log ("prompt HERE!!!!!!");
					promptMeditation ();
				} else if (numFetches % 3 == 0 && numEatingEvents == 0) {
					promptFeeding ();
				} 

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
				ball = GameObject.FindWithTag ("propFrisbee");
				oldParent = ball.transform.parent;
				ball.transform.parent = corgi.transform;
				ball.transform.position = corgi.transform.position + Camera.main.transform.forward * 0.25f;

			}
		} else {
			corgi.transform.position = fetchingPos;
		}
	}
		
	public void promptMeditation() {
		Debug.Log ("prompt MEDIATAIPODJSF");
		Bark ();
		breatheButton.GetComponent<Button>().Select();
		triggerInfoBubble ("When I get anxious, it helps to\n do some deep breathing", 5.0f);
	}

	public void promptFeeding() {
		eatButton.GetComponent<Button>().Select();
		triggerInfoBubble ("All that fetching\nmade me hungry!", 5.0f);
	}

	/*
	//Random Static Action :
	public void doRandomStaticAction(){
		shouldMove = false;
		rotating = false; 
		//Random static actiona and random time
		if (randomStaticAction == "Sit"){
			Sit ();
			LookAt ();
		}else if (randomStaticAction == "Dig"){
			Dig ();
		}else if (randomStaticAction == "SitScratch"){
			SitScratch ();
		}else if (randomStaticAction == "Lay"){
			Lay ();
		}
		warpRandomWalkAgain (randomStaticTime);
	}


	public void warpRandomWalkAgain(float time) {
		StartCoroutine(RandomWalkAgain(time));
	}

	public IEnumerator RandomWalkAgain(float time) {
		yield return new WaitForSeconds(time);
		numberOfRandomTargetReached=0;
		//Here we can add a random number of numberOfRandomTargetReached
		//HERE
		isRandomlyWalking = true;
		rotating = true; 
	}

	public string getRandomStaticAction(float randomNumber){
		if (randomNumber <= .25f) {
			return "Sit";
		} else if (randomNumber <= .5f) {
			return "Dig";
		} else if (randomNumber <= .75f) {
			return "SitScratch";
		} else {
			return "Lay";
		}
	}
	public float getRandomStaticTime(string randomAction){
		if (randomAction == "Sit") {
			return 14f;
		} else if (randomAction == "Dig") {
			return 10f;
		} else if (randomAction == "SitScratch") {
			return 6f;
		} else if (randomAction == "Lay") {
			return 14f;
		} else {
			return 7f;
		}
	}



	// Random Walking
	public void activateRandomWalking(){
		//Walk ();
		randomBehavior = true;
		isRandomlyWalking = true;
		numberOfRandomTargetReached = 0;
		shouldMove = true;
		rotating = true;
		//fraction = 0;
		rotatingTargetPos = newRandomDirection ();
		Debug.Log ("rotatingTargetPos: "+rotatingTargetPos);
		Debug.Log ("activateRandomWalking called");
		Debug.Log ("Parameters Value : isBreathing: " + isBreathing + ", isRandomly walking: "+ isRandomlyWalking+ ", shouldMove: "+ shouldMove);
	}


	//// Returns a return value among {-1,0,1}
	//// randomNumber is between 0.0 and 1.0
	public float returnRandomInteger(float randomNumber){
		if (randomNumber <= .2f) {
			return -2;
		} else if (randomNumber <= .4f) {
			return -1;
		} else if (randomNumber <= .6f) {
			return 0;
		} else if (randomNumber <= .8f) {
			return 1;
		} else {
			return 2;
		}
	}
	public Vector3 newRandomDirection(){
		randX = returnRandomInteger (UnityEngine.Random.value);
		randZ = returnRandomInteger (UnityEngine.Random.value);
		Debug.Log("New direction: randX: "+ randX + ", randZ: "+randZ);
		Vector3 randVector = new Vector3 (randX, 0, randZ);
		if (randVector.magnitude > 0) {
			randVector = randVector / randVector.magnitude;
		}
		randVector = corgi.transform.position + randVector*corgi.transform.localScale.x * .025f ;//Random position next to the dog
		return randVector;
	}

	public void RandomWalk(){
		//Debug.Log ("Random Walk called");
		//Debug.Log ("Parameters Value : isBreathing: " + isBreathing + ", isRandomly walking: "+ isRandomlyWalking+ ", shouldMove: "+ shouldMove);
		//Debug.Log ("rotatingTargetPos: "+rotatingTargetPos);

		if(!isRandomlyWalking) return;
		Walk ();
		float distance = Math.Abs(Vector3.Distance(corgi.transform.position, rotatingTargetPos));
		if (distance < 0.06) {
			Debug.Log ("Arrived at Random Destination" + distance);
			numberOfRandomTargetReached += 1;
			if (numberOfRandomTargetReached == 2) {
				Debug.Log ("Stop Random Walking");
				randomStaticAction = getRandomStaticAction (UnityEngine.Random.value);
				randomStaticTime = getRandomStaticTime (randomStaticAction);
				isRandomlyWalking = false;
			} else {
				rotatingTargetPos = newRandomDirection ();
			}
		}
	}
	*/
	
	public void triggerInfoBubble(string infoMsg, float time) {
		infoBubble.SetActive (true);
		infoBubble.GetComponentInChildren<Text> ().text = infoMsg;
		StartCoroutine(InfoBubbleClose(time));
	}

	public IEnumerator InfoBubbleClose(float time) {
		yield return new WaitForSeconds(time);  
		infoBubble.SetActive (false);
	}

	public void StartBreathingSequence() {
		LookAt ();
		Sit ();
		if (!injectionDone) {
			StartCoroutine (startBreathing ("Sometimes I get anxious when\n I know I have to get an\n injection. It helps to do\n deep breathing"));
		} else {
			StartCoroutine (startBreathing ("When I get anxious, \n it helps to do some deep breathing"));
		}
	}

	public IEnumerator startBreathing(String msg) {
		triggerInfoBubble(msg, 2.0f);
		yield return new WaitForSeconds(3.0f); 
		if (numMeditationEvents > 0) {
			Breathe ();
		} else {
			infoBubble.SetActive (true);
			infoBubble.GetComponentInChildren<Text> ().text = "First, we start by getting as\n comfy as possible, and\n relaxing our bodies";
			breatheInstructionButton1.SetActive (true);
		}
	}

	public void breathingStepOneClose() {
		infoBubble.GetComponentInChildren<Text> ().text = "Next, we’ll breathe in through\n our nose for 4 seconds, then\n breathe out from our mouths\n for 4 seconds. You can\n watch me first!";
		breatheInstructionButton1.SetActive(false);
		breatheInstructionButton2.SetActive(true);
	}

	public void breathingStepTwoClose() {
		aura.transform.position = new Vector3 (corgi.transform.position.x, aura.transform.position.y, corgi.transform.position.z);
		aura.SetActive (true);
		isBreathingTest = true;
		breatheInstructionButton2.SetActive(false);
		BreatheTest ();
	}

	public void BreatheTest(){

		// Transformation of the sphere
		if (auraGrowing && aura.transform.localScale.x < 14) {
			Debug.Log ("aura growing");
			infoBubble.GetComponentInChildren<Text> ().text = "In through the nose for 4...";
			aura.transform.localScale += new Vector3 (0.04F, 0.04F, 0.04F);
		} else if (auraGrowing && aura.transform.localScale.x >= 14) {
			infoBubble.GetComponentInChildren<Text> ().text = "Hold your breath";
			AuraWarper ();
		} else if (!auraGrowing && aura.transform.localScale.x > 5) {
			infoBubble.GetComponentInChildren<Text> ().text = "Out through the mouth for 4...";
			aura.transform.localScale -= new Vector3 (0.04F, 0.04F, 0.04F);
		} else if (aura.transform.localScale.x <= 5) {
			StartCoroutine (waitAndBreathe ());
		}
	}

	public IEnumerator waitAndBreathe() {
		infoBubble.GetComponentInChildren<Text> ().text = "Now let's do it together!";
		yield return new WaitForSeconds(2.5f);  
		Breathe ();
		isBreathing = true;
		isBreathingTest = false;
	}

	public void Breathe(){

		// Transformation of the sphere
		if (auraGrowing && aura.transform.localScale.x < 14) {
			Debug.Log ("aura growing");
			infoBubble.GetComponentInChildren<Text> ().text = "In through the nose for 4...";
			aura.transform.localScale += new Vector3 (0.04F, 0.04F, 0.04F);
		} else if (auraGrowing && aura.transform.localScale.x >= 14) {
			infoBubble.GetComponentInChildren<Text> ().text = "Hold your breath";
			AuraWarper ();
		} else if (!auraGrowing && aura.transform.localScale.x > 5) {
			infoBubble.GetComponentInChildren<Text> ().text = "Out through the mouth for 4...";
			aura.transform.localScale -= new Vector3 (0.04F, 0.04F, 0.04F);
		} else if (aura.transform.localScale.x <= 5) {
			auraGrowing = true;
			nbBreathingCycles += 1;
		}

		// End of the 3 cycles of breathing => re-initialize the parameters
		if (nbBreathingCycles >= 4) {
			triggerInfoBubble ("Great job! Thanks\n for doing that with me. ", 14.0f);
			numMeditationEvents += 1;
			isBreathing = false;
			nbBreathingCycles = 0;
			auraGrowing = false;
			aura.SetActive (false);
			hasBreathed = true;
			isInteractionComplete ();
		}
	}

		
	private List<ARHitTestResult> getHitTest(Vector2 screenPosition) {

		// Project from the middle of the screen to look for a hit point on the detected surfaces.
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

	public void PlaceDog() {

		// check for planes
		var screenPosition = Camera.main.ScreenToViewportPoint (new Vector2 (Screen.width / 2f, Screen.height / 2f));
		List<ARHitTestResult> hitResults = getHitTest(screenPosition);

		// if plane exists, place the dog
		if (hitResults.Count == 0)
			return;

		ARHitTestResult result = hitResults[0];

		// set the dog on the platform
		corgi.transform.rotation = Quaternion.Euler (Vector3.zero);
		Vector3 pos = UnityARMatrixOps.GetPosition (result.worldTransform);
		corgi.transform.position = new Vector3 (pos.x, pos.y + .075f, pos.z);

		LookAt ();
		corgi.SetActive (true);
		dogInScene = true;
	}

	public void LookAt() {
		corgi.transform.LookAt (Camera.main.transform.position);
		corgi.transform.eulerAngles = new Vector3(0, corgi.transform.eulerAngles.y, 0);
	}

	public void setLevelOne() {
		Debug.Log ("level 1");
		introPanel.SetActive (true);
		levelPanel.SetActive (false);
		level = 1;
		setDragObject ();
	}

	public void setLevelTwo() {
		Debug.Log ("level 2");
		introPanel.SetActive (true);
		levelPanel.SetActive (false);
		level = 2;
		setDragObject ();
	}

	public void setLevelThree() {
		Debug.Log ("level 3");
		introPanel.SetActive (true);
		levelPanel.SetActive (false);
		level = 3;
		setDragObject ();
	}

	public void goBackToMenu(){
		SceneController.CrossSceneInformation = "menu";
		Application.LoadLevel ("menu");
	}

	public void goBackToEmojis(){
		SceneController.CrossSceneInformation = "emojis";
		Application.LoadLevel ("menu");
	}

	public void hideIntroPanel(){
		introPanel.SetActive (false);
		isReadyForQRDetection = true;
	}

	public void hideExitPanel(){
		exitPanel.SetActive (false);
		goBackToEmojis ();
	}

	public void hideDogNamePanel(){
		dogNamePanel.SetActive (false);
		InitialSequenceWrapper ();
	}
		
	public void goBackToCamera(){
		Application.LoadLevel ("camera");
	}
}