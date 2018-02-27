using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
	public class touchTxt : MonoBehaviour {
	Text txt;
	// Use this for initialization
	void Start () {
		txt = GetComponent<Text> ();
	}

	// Update is called once per frame
	void Update () {
//		if (Input.touchCount > 0) {
//			txt.text = Input.touches [0].position.ToString ();
//		} else {
//			txt.text = "0 touches";
//		}
	}
}

