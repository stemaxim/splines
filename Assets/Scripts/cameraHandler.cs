using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class cameraHandler : MonoBehaviour {
	static cameraHandler _instance;
	public static cameraHandler Instance{
		get {return _instance;}
	}
	void Awake(){
		_instance = this;
		cam = GetComponent<Camera> ();
	}
	Camera cam;

	[Range(0f,200f)]
	public float topBorder = 200f;
	[Range(0f,200f)]
	public float bottomBorder = 50f;
	[Range(0f,200f)]
	public float leftBorder = 50f;
	[Range(0f,200f)]
	public float rightBorder = 50f;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		// Move camera 1 touch
		if (Input.touchCount == 1) {
			if (Input.touches [0].phase == TouchPhase.Moved) {
				float k = 2 * transform.position.z * Mathf.Tan (cam.fieldOfView * Mathf.PI / 360f) / (float)Screen.height;
				Vector3 delta = Input.touches[0].deltaPosition * k;
				transform.position += CheckMove(delta);
			}
		}

		// Zoom. Move camera forward/backvard
		if (Input.touchCount > 1) {
			float k = 2 * transform.position.z * Mathf.Tan (cam.fieldOfView * Mathf.PI / 360f) / (float)Screen.height;
			Vector2 prevCenter = Vector3.zero;
			Vector2 currCenter = Vector3.zero;
			foreach (Touch touch in Input.touches) {
				prevCenter += touch.position - touch.deltaPosition;
				currCenter += touch.position;
			}
			prevCenter = prevCenter / Input.touchCount;
			currCenter = currCenter / Input.touchCount;

			float prevDistance = 0;
			float currDistance = 0;
			foreach (Touch touch in Input.touches) {
				prevDistance += (prevCenter - touch.position - touch.deltaPosition).magnitude;
				currDistance += (currCenter - touch.position).magnitude;
			}
			prevDistance = prevDistance / Input.touchCount;
			currDistance = currDistance / Input.touchCount;

			Vector3 delta = ((Vector3)currCenter - (Vector3)prevCenter - Vector3.forward * (prevDistance - currDistance) / Mathf.Tan(cam.fieldOfView * Mathf.PI / 360f)) * k;

			transform.position += CheckMove(delta);
		}


		if (Input.GetKey (KeyCode.Escape)) {
			Application.Quit ();
		}
	}

	Vector3 CheckMove(Vector3 delta){

		int GMInstancemaxX = 620;
		int GMInstanceminX = 0;
		int GMInstancemaxY = 1000;
		int GMInstanceminY = 0;

		if (delta.x > 0) {
			if (cam.WorldToScreenPoint (Vector3.right * GMInstancemaxX - delta).x < Screen.width - rightBorder) {
				delta.x = 0;
				//delta.z = 0;
			}
		} else {
			if (cam.WorldToScreenPoint (Vector3.right * GMInstanceminX - delta).x > leftBorder) {
				delta.x = 0;
				//delta.z = 0;
			}
		}

		if (delta.y > 0) {
			if (cam.WorldToScreenPoint (Vector3.up * GMInstancemaxY - delta).y < Screen.height - topBorder) {
				delta.y = 0;
				//delta.z = 0;
			}
		} else {
			if (cam.WorldToScreenPoint (Vector3.up * GMInstanceminY - delta).y > bottomBorder) {
				delta.y = 0;
				//delta.z = 0;
			}
		}/**/

		if ((transform.position + delta).z > -1f) {
			delta.z = 0;
		}

		return delta;
	}
}
