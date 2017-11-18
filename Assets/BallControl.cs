using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;

public class BallControl : MonoBehaviour {

	public GameObject ballPrefab;
	public float Force;
	public GameObject currBall;
	public float createHeight;
	private MaterialPropertyBlock props;

	private GameObject corgi;

	Vector3 touchPosWorld;

	private bool inAir;

	private Vector3 startBallPos;
	private Vector3 endBallPos;
	Collider ballCollider;

	// for swiping code
	Vector2 touchStart;
	Vector2 touchEnd;
	float flickTime = 5;
	float flickLength = 0;
	float ballVelocity;
	float ballSpeed = 0;
	Vector3 worldAngle;
	bool GetVelocity; 
	float comfortZone = .2f;
	public bool fetchEnabled = false;

	// Use this for initialization
	void Start () {
		props = new MaterialPropertyBlock ();
		corgi = GameObject.FindWithTag("Corgi");
		currBall = GameObject.FindWithTag("Ball");
	}


	// Update is called once per frame
	void Update () {
	//	Vector3 v;

		if (corgi.GetComponent<DogControl> ().fetching) {
			return;
		}

		if (Input.touchCount > 0) {
			
			var touch = Input.touches[0];

			if (touch.phase == TouchPhase.Began) {
				flickTime = 5;
				timeIncrease ();
				GetVelocity = true;
				touchStart = touch.position;

				RaycastHit hit;
				Ray ray = Camera.main.ScreenPointToRay (touchStart); 
				if (Physics.Raycast (ray, out hit) && hit.transform.gameObject.tag == "Ball") {
					Debug.Log ("HIT BALL");
					fetchEnabled = true;
				}
			}

			if (touch.phase == TouchPhase.Ended && fetchEnabled) {
				var swipeDist = (touch.position - touchStart).magnitude;
				Debug.Log ("BALL SWIPE DIST: " + swipeDist);
				if (swipeDist > comfortZone) {
					fetchEnabled = false;
					GetVelocity = false;
					touchEnd = touch.position;
					GetSpeed ();
					GetAngle ();
					Rigidbody rb = currBall.GetComponent<Rigidbody> ();
					rb.useGravity = true;
					ballCollider.enabled = false;
					inAir = true;
					rb.AddForce (new Vector3 ((worldAngle.x * ballSpeed), (worldAngle.y * ballSpeed), (worldAngle.z * ballSpeed)));
				}
			}

			if (GetVelocity) {
				flickTime++;
			}

		}

		if (inAir) {

			Debug.Log ("BALL MOVING Y " + currBall.transform.position.y);
			Debug.Log ("BALL MOVING ORIG Y " + startBallPos.y);

			GameObject matPlane = GameObject.FindWithTag ("MatPlane");
			float matLevel = matPlane.transform.position.y;
			
			if (currBall.transform.position.y < matLevel - .1f) {

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
					corgi.GetComponent<DogControl> ().StartFetchingSequence (endBallPos);
				}
			}
		}
	}



	public void timeIncrease() {
		if (GetVelocity) {
			flickTime++;
		}
	}

	public void GetSpeed() {
		flickLength = 90;
		if (flickTime > 0) {
			ballVelocity = flickLength / (flickLength - flickTime);
		}

		ballSpeed = ballVelocity * 3;
		ballSpeed = ballSpeed - (ballSpeed * 1.65f);

		if (ballSpeed <= -33){
			ballSpeed = -33;
		}

		Debug.Log("flick was" + flickTime);
		flickTime = 5;
	}

	public void GetAngle () {
		worldAngle = Camera.main.ScreenToWorldPoint(new Vector3 (touchEnd.x, touchEnd.y + 1000, ((Camera.main.nearClipPlane - 100)*1.8f)));
	}
		
	public void CreateBall() {

		Debug.Log ("creating ball");

		// destroy any old balls
		Destroy(currBall);

		Vector3 position = Camera.main.transform.position + Camera.main.transform.forward * .2f;
		position.y = position.y - .1f;

		GameObject ballGO = Instantiate (ballPrefab, position, Quaternion.identity);

		Rigidbody rb = ballGO.GetComponent<Rigidbody> ();
		rb.useGravity = false;
		currBall = ballGO;
		ballGO.tag = "Ball";
		ballCollider = currBall.GetComponent<Collider>();


		startBallPos = currBall.transform.position;

		float r = 1.0f;
		float g = 0.0f;
		float b = 0.0f;

		props.SetColor("_InstanceColor", new Color(r, g, b));

		MeshRenderer renderer = ballGO.GetComponent<MeshRenderer>();
		renderer.SetPropertyBlock(props);

	}
}
