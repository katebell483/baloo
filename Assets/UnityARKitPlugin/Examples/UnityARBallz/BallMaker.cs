using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;

public class BallMaker : MonoBehaviour {

	public GameObject ballPrefab;
	private GameObject currBall;
	public float createHeight;
	private MaterialPropertyBlock props;
	private float minX, maxX, minY, maxY;

	// Use this for initialization
	void Start () {
		props = new MaterialPropertyBlock ();
		float camDistance = Vector3.Distance(transform.position, Camera.main.transform.position);
		Vector2 bottomCorner = Camera.main.ViewportToWorldPoint(new Vector3(0,0, camDistance));
		Vector2 topCorner = Camera.main.ViewportToWorldPoint(new Vector3(1,1, camDistance));

		minX = bottomCorner.x;
		maxX = topCorner.x;
		minY = bottomCorner.y;
		maxY = topCorner.y;

	}

	public void CreateBall()
	{
		Debug.Log ("creating Ball!");

		// destroy any old balls
		Destroy(currBall);

		Vector3 position = Camera.main.transform.position + Camera.main.transform.forward * 1.0f;

		GameObject ballGO = Instantiate (ballPrefab, position, Quaternion.identity);
		ballGO.gameObject.tag = "Ball";
		currBall = ballGO;

		float r = 1.0f;
		float g = 0.0f;
		float b = 0.0f;

		props.SetColor("_InstanceColor", new Color(r, g, b));

		MeshRenderer renderer = ballGO.GetComponent<MeshRenderer>();
		renderer.SetPropertyBlock(props);

	}

	// Update is called once per frame
	void Update () {
		if (Input.touchCount > 0 )
		{
			var touch = Input.GetTouch(0);
			if (touch.phase == TouchPhase.Began)
			{
				var screenPosition = Camera.main.ScreenToViewportPoint(touch.position);
				ARPoint point = new ARPoint {
					x = screenPosition.x,
					y = screenPosition.y
				};
						
				List<ARHitTestResult> hitResults = UnityARSessionNativeInterface.GetARSessionNativeInterface ().HitTest (point, 
					ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent);
				if (hitResults.Count > 0) {
					foreach (var hitResult in hitResults) {
						Vector3 position = UnityARMatrixOps.GetPosition (hitResult.worldTransform);
						//CreateBall (new Vector3 (position.x, position.y + createHeight, position.z));
						break;
					}
				}

			}
		}

	}

}
