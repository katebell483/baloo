using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;

public class BallMaker : MonoBehaviour {

	public GameObject ballPrefab;
	public float Force;
	public GameObject currBall;
	public float createHeight;
	private MaterialPropertyBlock props;

	private GameObject corgi;

	Vector3 touchPosWorld;

	public bool hasbeenthrown;
	private bool inAir;

	private Vector3 startBallPos;
	private Vector3 endBallPos;

	private float dist;
	private bool dragging = false;
	private Vector3 offset;
	private Transform toDrag;

	// for swiping code
	bool isSwiping;
	float maxTime = .5f;
	float minSwipeDist = 100.0f;
	float startTime;
	Vector3 startSwipePos;

	// Use this for initialization
	void Start () {
		props = new MaterialPropertyBlock ();
		corgi = GameObject.FindWithTag("Corgi");
		hasbeenthrown = false;
	}


	// Update is called once per frame
	void Update () {
		Vector3 v;


		if (corgi.GetComponent<DogControl> ().fetching) {
			return;
		}
			
		if (Input.touchCount > 0 && currBall != null) {

			Touch touch = Input.touches [0];

			Vector3 pos = touch.position;

			if (touch.phase == TouchPhase.Began) {
				Debug.Log ("touch started");

				startTime = Time.time;

				startBallPos = touch.position;
				startBallPos.z = currBall.transform.position.z - Camera.main.transform.position.z;
				startBallPos = Camera.main.ScreenToWorldPoint(startBallPos);

				RaycastHit hit;
				Ray ray = Camera.main.ScreenPointToRay (pos); 
				if (Physics.Raycast (ray, out hit) && hit.transform.gameObject.tag == "Ball") {

					Debug.Log ("ball hit");
					toDrag = hit.transform;
					dist = hit.transform.position.z - Camera.main.transform.position.z;
					v = new Vector3 (pos.x, pos.y, dist);
					v = Camera.main.ScreenToWorldPoint (v);
					offset = toDrag.position - v;
					dragging = true;
				}
			}

			if (dragging && touch.phase == TouchPhase.Moved) {
				Debug.Log ("dragging ball");
				v = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, dist);
				v = Camera.main.ScreenToWorldPoint (v);
				toDrag.position = v + offset;
			}

			if (dragging && (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)) {
				Debug.Log ("dragging complete");
				dragging = false;

				float endTime = Time.time;
				Vector3 endPos = touch.position;

				float swipeDistance = Vector3.Distance(endPos, startBallPos);
				float swipeTime = endTime - startTime;

				// TODO: make sure its in the forward direction
				Debug.Log ("SwipeTime: " + swipeTime);
				Debug.Log ("SwipeDist: " + swipeDistance);
				if (swipeTime < maxTime && swipeDistance > minSwipeDist) {
					isSwiping = true;
					Debug.Log ("IS SWIPPING!");

					Rigidbody rb = currBall.GetComponent<Rigidbody> ();
					rb.useGravity = true;
					rb.isKinematic = false;

					endPos.z = currBall.transform.position.z - Camera.main.transform.position.z;
					endPos = Camera.main.ScreenToWorldPoint(endPos);

					var dir = endPos - startBallPos;

					dir.Normalize ();

					// don't shoot down
					if (dir.y < 0f)
						return;

					// clamp up dir at .7
					if (dir.y > .6f) {
						dir.y = .6f;
					}

					/*
					// clamp dir side to side at range -.5 to .5
					if (dir.x > .5f) {
						dir.x = .5f;
					}

					if (dir.x < -.5f) {
						dir.x = -.5f;
					}*/

					Debug.Log ("**********************************************");
					Debug.Log ("VVVdir: " + dir);
				
					rb.transform.LookAt (dir);
					rb.AddForce(dir * Force);

					hasbeenthrown = true;
					inAir = true;
				}
			}



		}

		if (inAir) {

			if (currBall.transform.position.y < startBallPos.y) {
				Rigidbody rb = currBall.GetComponent<Rigidbody> ();
				rb.velocity = Vector3.zero;
				rb.angularVelocity = Vector3.zero; 
				endBallPos = currBall.transform.position;
				rb.useGravity = false;
				inAir = false;

				if(corgi.GetComponent<DogControl> ().dogInScene) {
					corgi.GetComponent<DogControl> ().fetchBall (endBallPos);
				}

				//StartCoroutine(sendDogForBall(endBallPos));
			}
		}
	}

	public void CreateBall() {

		if (corgi.GetComponent<DogControl> ().fetching) {
			return;
		}

		Debug.Log ("creating Ball!");

		// destroy any old balls
		Destroy(currBall);

		Vector3 position = Camera.main.transform.position + Camera.main.transform.forward * 1.0f;

		GameObject ballGO = Instantiate (ballPrefab, position, Quaternion.identity);
		Rigidbody rb = ballGO.GetComponent<Rigidbody> ();
		rb.useGravity = false;
		currBall = ballGO;
		ballGO.tag = "Ball";

		startBallPos = currBall.transform.position;

		float r = 1.0f;
		float g = 0.0f;
		float b = 0.0f;

		props.SetColor("_InstanceColor", new Color(r, g, b));

		MeshRenderer renderer = ballGO.GetComponent<MeshRenderer>();
		renderer.SetPropertyBlock(props);

	}

	public void throwBall() {
		Rigidbody rb = currBall.GetComponent<Rigidbody> ();
		rb.useGravity = true;
		rb.isKinematic = false;
		var fwd = Camera.main.transform.forward;
		currBall.transform.LookAt(fwd);
		Vector3 dir = new Vector3 (transform.forward.x, transform.forward.y +.6f, transform.forward.z);
		rb.AddForce (dir * Force);
		hasbeenthrown = true;
		inAir = true;
	}


	/* TODO: add delay by uncommenting this funciton + call
	IEnumerator sendDogForBall(Vector3 endPos) {
		// wait for one second and then send the dog to get it
		yield return new WaitForSeconds(1);
		DogControl dc = new DogControl ();
		dc.fetchBall (endBallPos);
	}
	*/
}
