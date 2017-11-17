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

	private bool inAir;

	public Vector3 origBallPos;
	private Vector3 startBallPos;
	private Vector3 endBallPos;
	private Vector3 nonTouchBallStart;

	private float dist;
	private bool dragging = false;
	private Vector3 offset;
	private Transform toDrag;

	// for swiping code
	bool isSwiping;
	float maxTime = .5f;
	float minSwipeDist = .07f;
	float startTime;
	Vector3 startSwipePos;

	// Use this for initialization
	void Start () {
		props = new MaterialPropertyBlock ();
		corgi = GameObject.FindWithTag("Corgi");
		currBall = GameObject.FindWithTag("Ball");
	}


	// Update is called once per frame
	void Update () {
		Vector3 v;

		//Debug.Log ("CUR BALL POS: " + currBall.transform.position);

		if (corgi.GetComponent<DogControl> ().fetching) {
			return;
		}
			
		if (Input.touchCount > 0 && currBall != null) {

			Touch touch = Input.touches [0];

			Vector3 pos = touch.position;

			if (touch.phase == TouchPhase.Began) {
				Debug.Log ("touch started");

				nonTouchBallStart = currBall.transform.position;

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
				Vector3 dragPos = v + offset;
				Debug.Log ("Ball Drag, Pos" + dragPos);

				if (dragPos.y < origBallPos.y) {
					dragPos.y = origBallPos.y;
				}

				if(dragPos.z > origBallPos.z) {
					dragPos.z = origBallPos.z;
				}

				toDrag.position = dragPos;
			}

			// touch released. check if the movement was a swipe to indicate fetch
			if (dragging && (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)) {
				
				Debug.Log ("dragging complete");
				dragging = false;

				// compare the duration and movement of drag
				Vector3 endPos = touch.position;
				endPos.z = currBall.transform.position.z - Camera.main.transform.position.z;
				endPos = Camera.main.ScreenToWorldPoint(endPos);

				float endTime = Time.time;

				float swipeDistance = Vector3.Distance(endPos, startBallPos);
				float swipeTime = endTime - startTime;

				Debug.Log ("Ball Drag, Swipe Time: " + swipeTime);
				Debug.Log ("Ball Drag, Swipe Dist: " + swipeDistance);

				// if time is sig short and movement sig large shoot the ball
				if (swipeTime < maxTime && swipeDistance > minSwipeDist) {
					
					isSwiping = true;
					Debug.Log ("Ball Drag, Launching ball");

					// get swipe direction
					var dir = endPos - startBallPos;
					dir.Normalize ();

					// don't shoot down
					if (dir.y < 0f)
						return;

					// clamp up dir at .7
					if (dir.y > .6f) {
						dir.y = .6f;
					}
						
					Debug.Log ("**********************************************");
					Debug.Log ("VVVdir: " + dir);
				
					// now send ball flying in that direction
					Rigidbody rb = currBall.GetComponent<Rigidbody> ();
					rb.transform.LookAt (dir);
					rb.useGravity = true;
					rb.isKinematic = false;
					rb.AddForce(dir * Force);

					// keep track of flying ball
					inAir = true;
				}
			}

		}

		if (inAir) {
			
			Debug.Log ("BALL MOVING Y " + currBall.transform.position.y);
			Debug.Log ("BALL MOVING ORIG Y " + startBallPos.y);

			if (currBall.transform.position.y < startBallPos.y - .1f) {
				
				Debug.Log ("throw over!");

				// remove velocity + gravity from ball so it hovers in the air
				Rigidbody rb = currBall.GetComponent<Rigidbody> ();
				rb.velocity = Vector3.zero;
				rb.angularVelocity = Vector3.zero; 
				rb.useGravity = false;

				// track the landing spot so the dog can find it
				endBallPos = currBall.transform.position;

				// work of the ball is done
				inAir = false;

				// send the dog to fetch it
				if(corgi.GetComponent<DogControl> ().dogInScene) {
					corgi.GetComponent<DogControl> ().fetchBall (endBallPos);
				}

				//StartCoroutine(sendDogForBall(endBallPos));
			}
		}
	}

	public void setOrigPos(Vector3 pos) {
		origBallPos = pos;
	}

	public void CreateBall() {

		/*
		if (corgi.GetComponent<DogControl> ().fetching) {
			return;
		}*/

		Debug.Log ("putting ball back to: " + origBallPos);

		currBall.transform.position = origBallPos;


		/*
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
		*/

	}

	public void throwBall() {
		Rigidbody rb = currBall.GetComponent<Rigidbody> ();
		rb.useGravity = true;
		rb.isKinematic = false;
		var fwd = Camera.main.transform.forward;
		currBall.transform.LookAt(fwd);
		Vector3 dir = new Vector3 (transform.forward.x, transform.forward.y +.6f, transform.forward.z);
		rb.AddForce (dir * Force);
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
