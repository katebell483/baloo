using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;
using System;

public class DogControl : MonoBehaviour {

	// basic dog params
	public GameObject corgi;
	private Animation animation;
	private bool shouldMove = false;
	public bool dogInScene = false;
	Collider corgiCollider;

	// other game objects in the scene
	public GameObject mat;
	public GameObject ball;
	public GameObject dogFood;

	// fetching params
	private bool initialFetchSequence = false;
	public bool fetching = false;
	public bool goingToFood = false;
	private bool returnTrip = false;
	private Vector3 endFetchingPos;
	public Vector3 foodPos;
	private Vector3 startOffPlaneFetchingPos;
	private Vector3 fetchOrigin;
	private float speed = .3f;
	private float fraction = 0; 

	// rotating params
	private bool rotating = false;
	private Vector3 rotatingTargetPos;

	// eating params
	public GameObject eatButton;

	// sitting params
	private bool isSitting = false;

	// Use this for initialization
	void Start () {
		animation = GetComponent<Animation> ();
		mat = GameObject.FindWithTag ("Mat");
		corgi = GameObject.FindWithTag("Corgi");
		ball = GameObject.FindWithTag ("Ball");
		Physics.IgnoreCollision(corgi.GetComponent<Collider>(), ball.GetComponent<Collider>());
		Physics.IgnoreCollision(corgi.GetComponent<Collider>(), mat.GetComponent<Collider>());
		corgiCollider = corgi.GetComponent<Collider>();
		dogFood = GameObject.FindWithTag ("dogFood");
	}

	// Update is called once per frame
	void Update () {

		// is the dog rotating?
		if (rotating) {
			rotateDog (rotatingTargetPos);
		}

		if (shouldMove && !fetching) {
			transform.Translate (Vector3.forward * Time.deltaTime * (transform.localScale.x * .25f));
			startOffPlaneFetchingPos = transform.position;
		} 

		if (SwipeManager.Instance.IsSwiping(SwipeDirection.Down)){
			if (!isSitting) {
				Sit ();
			} else {
				Lay ();
			}
		}
			

		if (initialFetchSequence) {
			rotating = true;
			rotatingTargetPos = ball.transform.position;
		}

		if (fetching) {

			// move the dog forward
			fraction += Time.deltaTime * speed;

			// originally start from the last point that the dog was on the plane
			Vector3 fetchingPos = Vector3.Lerp (startOffPlaneFetchingPos, endFetchingPos, fraction);

			Debug.Log ("_________________________________________________");
			Debug.Log ("fraction " + fraction);
			Debug.Log ("fetchingPos " + fetchingPos);
			Debug.Log ("endfetchingPos " + endFetchingPos);
			Debug.Log ("_________________________________________________");

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
					ball.transform.parent = null;
					rotating = false;
					corgiCollider.enabled = true;

				} else {
					Debug.Log ("turning around");
					returnTrip = true;
					fraction = 0;
					endFetchingPos = fetchOrigin;
					startOffPlaneFetchingPos = fetchingPos;

					// rotate
					rotating = true;
					rotatingTargetPos = fetchOrigin;

					// grarb the ball by parenting it to the dog
					Debug.Log ("grabbing ball");
					ball.transform.parent = transform;
					//ball.transform.Translate(0, .1f, -.1f);
				}
			} else {
				transform.position = fetchingPos;
			}
		}


		if (goingToFood) {

			// move dog forward
			fraction += Time.deltaTime * .025f;
			Vector3 fetchingPos  = Vector3.Lerp(transform.position, foodPos, fraction);

			Debug.Log ("HELL");
			Debug.Log (transform.position);
			Debug.Log ("BYE");
			Debug.Log (foodPos);

			//check distance
			float distance = Math.Abs(Vector3.Distance(fetchingPos, foodPos));
			Debug.Log ("DISTANCE: " + distance);

			// got to food so stop + eat
			if (distance < .06) {
				Debug.Log ("GOT TO FOOD!");
				rotating = false;
				fraction = 0;
				goingToFood = false;
				EatWrapper ();
			} else {
				transform.position = fetchingPos;
			}

		}
	}
		
	public void rotateDog(Vector3 targetPos) {
		Vector3 targetPoint = new Vector3(targetPos.x, corgi.transform.position.y, targetPos.z) - corgi.transform.position;
		Quaternion targetRotation = Quaternion.LookRotation (targetPoint, Vector3.up);
		corgi.transform.rotation = Quaternion.Slerp(corgi.transform.rotation, targetRotation, Time.deltaTime * 3.0f);
	}

	void OnTriggerExit(Collider other) {
		Debug.Log (other.tag);

		if (initialFetchSequence) {
			
			Debug.Log ("HIT WALL STARTING FETCH");

			// disable colliders
			corgiCollider.enabled = false;

			endFetchingPos = ball.transform.position;
			Debug.Log ("end fetching pos " + endFetchingPos);

			Jump ();
			Gallop ();

			fetching = true;
			initialFetchSequence = false;

		} else {
			transform.LookAt (mat.transform.position);
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
		transform.rotation = Quaternion.Euler (Vector3.zero);
		transform.position = UnityARMatrixOps.GetPosition (result.worldTransform);

		LookAt ();

		dogInScene = true;
	}

	public void fetchBall(Vector3 ballPos) {
		Debug.Log ("sending dog to fetch");

		// mark position
		Debug.Log ("start fetching pos " + fetchOrigin);
		fetchOrigin = transform.position;

		// walk to the edge
		Walk ();

		// then turn and walk towards edge of platform
		initialFetchSequence = true;

		return;
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
		isSitting = true;
		shouldMove = false;
		animation.CrossFade ("CorgiSitIdle");
	}

	public void GoToBowl() {
		Debug.Log ("GOING TO BOWL");
		goingToFood = true;
		rotatingTargetPos = dogFood.transform.position;
		rotating = true;
		Walk ();
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

	public void Walk() {
		isSitting = false;
		shouldMove = true;
		animation.CrossFade ("CorgiTrot");
	}

	public void Gallop() {
		isSitting = false;
		shouldMove = true;
		animation.CrossFade ("CorgiGallop");
	}

	public void Jump() {
		shouldMove = false;
		animation.CrossFade ("CorgiJump");
	}

	public void InitialSequenceWrapper() {
		StartCoroutine(InitialSequence());
	}

	public IEnumerator InitialSequence() {
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