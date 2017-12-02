using System.Collections;
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
	private Transform initialCorgiTransform;

	// UI elements
	public GameObject fetchButton;
	public GameObject eatButton;
	public GameObject breatheButton;
	public GameObject infoBubble;

	// panels
	public GameObject introPanel;
	public GameObject dogNamePanel;
	public GameObject exitPanel;
	public bool isReadyForQRDetection;

	// Breathing objects
	public GameObject aura;

	// other game objects in the scene
	public GameObject mat;
	public GameObject ball;
	public GameObject dogFood;
	public GameObject propFrisbee;
	public GameObject dogHouse;

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

	// eating params
	public bool goingToFood = false;
	public Vector3 foodPos;
	private int numEatingEvents = 0;
	private bool isLastEatingEvent = false;

	// Random walking parameters
	public bool isRandomlyWalking = false;
	float randX, randZ;

	// going home params
	private bool isGoingHome = false;

	// petting params
	private bool corgiTouched = false;
	private IEnumerator sittingCoroutine;
	private bool waitingToSit = false;
	private IEnumerator layingCoroutine;

	//Daniel: breathing params
	private bool isBreathing = false;
	private bool auraGrowing = true; //true if growing, false if reducing
	private float nbBreathingCycles = 0;
	private int numMeditationEvents = 0;

	// rotating params
	public bool rotating = false;
	private Vector3 rotatingTargetPos;
	private int randomWalkTime = 0;

	// Use this for initialization
	void Start () {
		
		animation = corgi.GetComponent<Animation> ();

		/* panels */
		//introPanel = GameObject.FindWithTag ("introPanel");
		introPanel.SetActive(true);

		//dogNamePanel = GameObject.FindWithTag ("dogNamePanel");
		dogNamePanel.SetActive(false);

		//exitPanel = GameObject.FindWithTag ("exitPanel");
		exitPanel.SetActive(false);
	
		/*

		mat = GameObject.FindWithTag ("Mat");
		corgi = GameObject.FindWithTag("Corgi");
		dogFood = GameObject.FindWithTag ("dogFood");
		infoBubble = GameObject.FindWithTag ("infoBubble");

		triggerInfoBubble ("Welcome to Baloo. Show me the QR Code!", 4.0f);

		breatheButton = GameObject.FindWithTag ("BreatheButton");
		fetchButton = GameObject.FindWithTag ("FetchButton");
		eatButton = GameObject.FindWithTag ("EatButton");

		aura = GameObject.FindWithTag ("Aura");
		*/
	}

	// Update is called once per frame
	void Update () {

		// turn off buttons until dog in scene
		if (dogInScene) {
			fetchButton.SetActive (true);
			eatButton.SetActive (true);
			breatheButton.SetActive (true);
		} else {
			fetchButton.SetActive (false);
			eatButton.SetActive (false);
			breatheButton.SetActive (false);
		}

		// even rotating but no more distance walk in place
		/*
		if (rotating && !shouldMove) {
			WalkInPlace ();
		}
		*/
	
		//Daniel: Breathing phase:
		if (isBreathing) {
			Breathe ();
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
			
		if (shouldMove && !fetching) {

			if (isRandomlyWalking) {
				RandomWalk ();
			}
				
			else{
				corgi.transform.Translate (Vector3.forward * Time.deltaTime * (corgi.transform.localScale.x * .015f));
				// we want to keep track of the corgis position before it leaves the plane
				startFetchingPos = corgi.transform.position;
			}
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


	public void InitialSequenceWrapper() {
		initialCorgiTransform = corgi.transform;
		StartCoroutine(InitialSequence());
	}

	public IEnumerator InitialSequence() {
		Debug.Log ("Dog starting initial sequence");
		yield return new WaitForSeconds(1.0f); 
		Walk ();
		yield return new WaitForSeconds(2.0f); 
		//isRandomlyWalking = true;
		//yield return new WaitForSeconds (3.0f);
		print ("random walking over");
		//randomWalkTime = 0;
		//isRandomlyWalking = false;
		//rotating = false;
		LookAt();
		StartCoroutine(TimedBark(1.0f));
	}
		
	public void rotateDog(Vector3 targetPos) {
		Debug.Log ("ROTATING");
		Vector3 targetPoint = new Vector3(targetPos.x, corgi.transform.position.y, targetPos.z) - corgi.transform.position;
		Quaternion targetRotation = Quaternion.LookRotation (targetPoint, Vector3.up);
		corgi.transform.rotation = Quaternion.Slerp(corgi.transform.rotation, targetRotation, Time.deltaTime * 4.0f);
		if(targetRotation == corgi.transform.rotation) {
			rotating = false;
		}
	}

	void OnTriggerExit(Collider other) {
		Debug.Log (other.tag);

		if (initialFetchSequence) {

			// disable colliders
			corgiCollider = corgi.GetComponent<Collider> ();
			corgiCollider.enabled = false;

			Gallop ();

			fetching = true;
			initialFetchSequence = false;

		} else if (isRandomlyWalking) {
			rotatingTargetPos = mat.transform.position;
			rotating = true;
		} else {
			Debug.Log ("HIT WALL GENERAL CASE: " + other.tag);
			corgi.transform.LookAt (mat.transform.position);
			Sit ();
		}
	}

	public void StartFetchingSequence(Vector3 ballPos) {
		Debug.Log ("sending dog to fetch");

		numFetches += 1;

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

	public void WalkInPlace() {
		shouldMove = false;
		animation.CrossFade ("CorgiTrot");
	}

	public void Dig() {
		shouldMove = false;
		animation.CrossFade ("CorgiIdleDig");
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
		animation.CrossFade ("CorgiLayIdleLong");
	}

	public void LayDown() {
		shouldMove = false;
		animation.CrossFade ("CorgiSitToLay");
	}

	public void IdleToLay() {
		shouldMove = false;
		animation.CrossFade ("CorgiIdleToLay");
	}

	public void LayToSit() {
		shouldMove = false;
		animation.CrossFade ("CorgiLaytoSit");
	}

	public void Sit() {
		Debug.Log ("Sitting");
		shouldMove = false;
		animation.CrossFade ("CorgiSitIdle");
	}

	public void SitScratch() {
		shouldMove = false;
		animation.CrossFade ("CorgiSitScratch");
	}

	public void SitLong() {
		shouldMove = false;
		animation.CrossFade ("CorgiSitIdleLong");
	}

	public IEnumerator WalkAndDigAndSit(bool shouldScratch) {
		Debug.Log ("digging");
		Walk ();
		yield return new WaitForSeconds (1f);
		Dig ();
		yield return new WaitForSeconds (2.5f);
		Debug.Log ("digging stopped ");
		if (shouldScratch) {
			SitScratch ();
		} else {
			Sit ();
		}
	}

	public IEnumerator WalkAndSit() {
		Walk ();
		yield return new WaitForSeconds (1f);
		SitLong ();
	}

	public IEnumerator WaitAndSit(float waitTime, bool isLaying) {
		Debug.Log ("waiting to sit");
		waitingToSit = true;
		yield return new WaitForSeconds (waitTime);
		waitingToSit = false;
		if(isLaying) {
			LayToSit();
		} else {
			Sit ();
		}
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
		LayDown();
		yield return new WaitForSeconds (1.5f);
		Debug.Log ("exit sequence complete");
		exitPanel.SetActive (true);
	}

	public IEnumerator WaitAndLay(float waitTime) {
		Debug.Log ("waiting to lay");
		yield return new WaitForSeconds(waitTime); 
		LayDown ();
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
	}

	public void StartEatingSequence() {
		Debug.Log ("GOING TO BOWL");
		goingToFood = true;
		rotatingTargetPos = dogFood.transform.position;
		Debug.Log ("BOWL POS: " + rotatingTargetPos);
		rotating = true;
		Walk ();
	}
		
	public void GoToFoodAndEat() {
		

		//check distance
		float distance = Math.Abs(Vector3.Distance(corgi.transform.position, foodPos));
		Debug.Log ("DISTANCE: " + distance);

		// got to food so stop + eat
		if (distance < .1 && rotating) {
			Debug.Log ("GOT TO FOOD!");
			WalkInPlace ();
		} else if (distance < .1 && !rotating) {
			numEatingEvents += 1;
			fraction = 0;
			goingToFood = false;
			isLastEatingEvent = numEatingEvents == 2;
			EatWrapper ();
		} else {
			// move dog forward
			fraction += Time.deltaTime * .015f;
			Vector3 currPos  = Vector3.Lerp(corgi.transform.position, foodPos, fraction);
			corgi.transform.position = currPos;
		}
	}

	public void EatWrapper() {
		StartCoroutine(Eat());
	}
		
	public IEnumerator Eat() {
		Debug.Log ("EATING");

		// first eat 
		shouldMove = false;
		animation.CrossFade ("CorgiEat");
		yield return new WaitForSeconds(5f); // waits 5 seconds

		// now face the camera and walk away from food
		LookAt();
		Walk ();
		yield return new WaitForSeconds (1f);

		// stop waking & sit or dig
		shouldMove = false;
		bool dig = UnityEngine.Random.value < .33; // dig every 3ish times
		if (dig) {
			animation.CrossFade ("CorgiIdleDig");
		} else {
			SitLong ();
		}
			
		// check for end conditions
		// if interaction complete then start exit sequence
		// if max food events reached disable eat button
		if (!isInteractionComplete () &&  isLastEatingEvent) {
			triggerInfoBubble ("I'm full! Let's play!", 3.0f);
			eatButton.GetComponent<Button> ().interactable = false;
			fetchButton.GetComponent<Button> ().Select ();
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

	public void BarkLong() {
		Debug.Log ("Barking dog !");
		shouldMove = false;
		animation.CrossFade ("CorgiIdleBarkingLong");
	}

	public IEnumerator TimedBark(float time) {
		Bark ();
		yield return new WaitForSeconds(time); // waits 5 seconds
		animation.Stop("CorgiIdleBarking");
		SitLong ();
	}

	public void Bark(){
		shouldMove = false;
		animation.CrossFade ("CorgiIdleBarking");
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

				// bring back prop
				propFrisbee.SetActive(true);

				// first check if thi means we are done
				if (isInteractionComplete ()) {
					return;
				}

				// is this the third fetch?
				if (numFetches % 3 == 0 && numMeditationEvents == 0) {
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
				ball = GameObject.FindWithTag ("Ball");
				ball.transform.parent = corgi.transform;
			}
		} else {
			corgi.transform.position = fetchingPos;
		}
	}

	public void promptMeditation() {
		BarkLong ();
		breatheButton.GetComponent<Button>().Select();
		triggerInfoBubble ("Looks like all that fetching got Baloo all excited! Why don't we meditate together to relax him?", 5.0f);
	}

	public void promptFeeding() {
		eatButton.GetComponent<Button>().Select();
		triggerInfoBubble ("All that fetching made me hungry!", 5.0f);
	}

	// Random Walking
	//// Returns a return value among {-1,0,1}
	//// randomNumber is between 0.0 and 1.0
	public float returnRandomInteger(float randomNumber){
		if (randomNumber <= .25f) {
			return -2;
		} else if (randomNumber <= .5f) {
			return -1;
		} else if (randomNumber <= .75f) {
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
		randVector = corgi.transform.position + randVector*corgi.transform.localScale.x;//Random position next to the dog
		return randVector;
	}

	public void RandomWalk(){
		Debug.Log ("Random Walk called");
		Debug.Log ("Parameters Value : isBreathing: " + isBreathing + ", isRandomly walking: "+ isRandomlyWalking+ ", shouldMove: "+ shouldMove);
		Debug.Log ("rotatingTargetPos: "+rotatingTargetPos);

		if(!isRandomlyWalking) return;
			
		Walk ();

		randomWalkTime += 1;

		fraction += Time.deltaTime * .025f;

		Vector3 curPos = Vector3.Lerp (transform.position, rotatingTargetPos, fraction);
		float distanceToRandPosition = Math.Abs (Vector3.Distance (transform.position, rotatingTargetPos));

		// walk towards random distance for 5 frames
		if (randomWalkTime % 30 == 0) {
			Debug.Log ("Baloo arrived to the random destination, ditance: " + distanceToRandPosition);
			fraction = 0;
			rotating = true;
			if (randomWalkTime > 90) {
				rotatingTargetPos = mat.transform.position;
			} else {
				rotatingTargetPos = newRandomDirection ();
			}
			RandomWalk ();
		} else {
			corgi.transform.position = curPos;
		}
	}

	public void triggerInfoBubble(string infoMsg, float time) {
		infoBubble.SetActive (true);
		infoBubble.GetComponentInChildren<Text> ().text = infoMsg;
		StartCoroutine(InfoBubbleClose(time));
	}

	public IEnumerator InfoBubbleClose(float time) {
		yield return new WaitForSeconds(time);  
		infoBubble.SetActive (false);
	}

	public void Breathe(){

		LookAt ();
		aura.transform.position = new Vector3 (corgi.transform.position.x, aura.transform.position.y, corgi.transform.position.z);
		aura.SetActive (true);
		isBreathing = true;

		infoBubble.SetActive (true);

		// Transformation of the sphere
		if (auraGrowing && aura.transform.localScale.x < 9) {
			infoBubble.GetComponentInChildren<Text> ().text = "Breathe in to calm Baloo";
			aura.transform.localScale += new Vector3 (0.02F, 0.02F, 0.02F);
		} else if (auraGrowing && aura.transform.localScale.x >= 9) {
			infoBubble.GetComponentInChildren<Text> ().text = "Hold your breath";
			AuraWarper ();
		} else if (!auraGrowing && aura.transform.localScale.x > 4) {
			infoBubble.GetComponentInChildren<Text> ().text = "Breathe out slowly";
			aura.transform.localScale -= new Vector3 (0.02F, 0.02F, 0.02F);
		} else if (aura.transform.localScale.x <= 4) {
			auraGrowing = true;
			nbBreathingCycles += 1;
		}

		// End of the 3 cycles of breathing => re-initialize the parameters
		if (nbBreathingCycles >= 3) {
			triggerInfoBubble ("Baloo feels great after his meditation! How about you?", 4.0f);
			numMeditationEvents += 1;
			isBreathing = false;
			nbBreathingCycles = 0;
			auraGrowing = false;
			aura.SetActive (false);
			Sit();
			isInteractionComplete ();
		}

		// After 1 cycle Baloo is Barking less
		else if (nbBreathingCycles >= 1) {
			Bark ();
		}

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

	public void LookAt() {
		corgi.transform.LookAt (Camera.main.transform.position);
		corgi.transform.eulerAngles = new Vector3(0, corgi.transform.eulerAngles.y, 0);
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