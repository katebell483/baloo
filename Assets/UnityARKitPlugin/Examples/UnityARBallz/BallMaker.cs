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


	public bool hasbeenthrown;
	private bool inAir;

	private Vector3 startBallPos;
	private Vector3 endBallPos;

	// Use this for initialization
	void Start () {
		props = new MaterialPropertyBlock ();
		corgi = GameObject.FindWithTag("Corgi");
		hasbeenthrown = false;
	}

	public void CreateBall() {
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

	// Update is called once per frame
	void Update () {
		// if its not thrown keep ball in front of camera
		if(!hasbeenthrown && currBall != null){
			Vector3 position = Camera.main.transform.position + Camera.main.transform.forward * 1.0f;
			currBall.transform.position = position;
		}

		if (inAir) {
			
			if(currBall.transform.position.y < startBallPos.y) {
				Rigidbody rb = currBall.GetComponent<Rigidbody> ();
				rb.velocity = Vector3.zero;
				rb.angularVelocity = Vector3.zero; 
				endBallPos = currBall.transform.position;
				rb.useGravity = false;
				inAir = false;

				corgi.GetComponent<DogControl>().fetchBall(endBallPos);

				//StartCoroutine(sendDogForBall(endBallPos));
			}
		}

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
